"""Fetch and decompile game source for reference."""

from __future__ import annotations

import os
import shutil
import subprocess
import sys
from collections.abc import Mapping
from dataclasses import dataclass
from pathlib import Path
from typing import Protocol

import click
from dotenv import load_dotenv

from erenshor_dev.config import get_project_root, load_config

GAME_DLL = "Assembly-CSharp.dll"


@dataclass(frozen=True)
class GameVariant:
    """Steam metadata for a decompilable game variant."""

    app_id: str
    install_dir_name: str


class DecompileConfig(Protocol):
    """Shared config shape for installed and reference decompilation."""

    @property
    def dotnet8_root(self) -> Path | None: ...

    @property
    def steam_username(self) -> str: ...

    @property
    def steam_platform(self) -> str: ...


@dataclass(frozen=True)
class ReferenceConfig:
    """Configuration needed to fetch and decompile reference game builds."""

    dotnet8_root: Path | None
    steam_username: str
    steam_platform: str


def create_reference_config(environ: Mapping[str, str]) -> ReferenceConfig:
    """Create reference-build config without requiring a local game install."""
    dotnet8_root = environ.get("DOTNET8_ROOT")
    return ReferenceConfig(
        dotnet8_root=Path(dotnet8_root) if dotnet8_root else None,
        steam_username=environ.get("STEAM_USERNAME", "anonymous"),
        steam_platform=environ.get("STEAMCMD_PLATFORM", "windows"),
    )


GAME_VARIANTS: dict[str, GameVariant] = {
    "main": GameVariant(app_id="2382520", install_dir_name="main"),
    "playtest": GameVariant(app_id="3090030", install_dir_name="playtest"),
}


def build_steamcmd_command(
    *,
    app_id: str,
    install_dir: Path,
    username: str,
    platform: str,
    validate: bool,
) -> list[str]:
    """Build the SteamCMD command for downloading one Erenshor variant."""
    command = [
        "steamcmd",
        "+@sSteamCmdForcePlatformType",
        platform,
        "+force_install_dir",
        str(install_dir.absolute()),
        "+login",
        username,
        "+app_update",
        app_id,
    ]
    if validate:
        command.append("validate")
    command.append("+quit")
    return command


@click.command()
@click.option(
    "--variant",
    "variants",
    multiple=True,
    type=click.Choice(["installed", "main", "playtest"]),
    help=(
        "Source to decompile. Use multiple times, e.g. "
        "--variant main --variant playtest. Defaults to installed."
    ),
)
@click.option(
    "--download/--no-download",
    default=False,
    help="Download/update Steam game files before decompiling main/playtest variants.",
)
@click.option(
    "--validate",
    is_flag=True,
    help="Ask SteamCMD to validate downloaded files. Slower, but checks integrity.",
)
def decompile(variants: tuple[str, ...], download: bool, validate: bool) -> None:
    """Decompile game source to reference/game-source/.

    Without options, decompiles mod/lib/Assembly-CSharp.dll to the historical
    reference/game-source/ directory. For update debugging, run:

      erenshor decompile --variant main --variant playtest --download

    That downloads current Steam builds into reference/game-downloads/ and
    decompiles scripts into reference/game-source/main and
    reference/game-source/playtest.
    """
    project_root = get_project_root()
    selected_variants = variants or ("installed",)
    config: DecompileConfig
    if "installed" in selected_variants:
        config = load_config()
    else:
        load_dotenv(project_root / "cli" / ".env")
        config = create_reference_config(os.environ)

    if validate and not download:
        click.secho("Error: --validate requires --download.", fg="red")
        raise SystemExit(1)

    _ensure_ilspycmd_ready(config.dotnet8_root)
    env = _build_ilspy_environment(config.dotnet8_root)

    for variant in selected_variants:
        if variant == "installed":
            source_dll = project_root / "mod" / "lib" / GAME_DLL
            output_dir = project_root / "reference" / "game-source"
            missing_hint = "Run 'erenshor setup' first to copy game DLLs."
        else:
            variant_config = GAME_VARIANTS[variant]
            install_dir = (
                project_root
                / "reference"
                / "game-downloads"
                / variant_config.install_dir_name
            )
            if download:
                _download_variant(
                    app_id=variant_config.app_id,
                    install_dir=install_dir,
                    username=config.steam_username,
                    platform=config.steam_platform,
                    validate=validate,
                )
            source_dll = install_dir / "Erenshor_Data" / "Managed" / GAME_DLL
            output_dir = project_root / "reference" / "game-source" / variant
            missing_hint = (
                "Run 'erenshor decompile --variant "
                f"{variant} --download' to fetch this Steam build first."
            )

        preserve_dirs = {"main", "playtest"} if variant == "installed" else None
        _decompile_source(source_dll, output_dir, env, missing_hint, preserve_dirs)


def _download_variant(
    *,
    app_id: str,
    install_dir: Path,
    username: str,
    platform: str,
    validate: bool,
) -> None:
    """Download one Steam variant with SteamCMD."""
    if shutil.which("steamcmd") is None:
        click.secho("Error: steamcmd not found.", fg="red")
        click.echo("Install it with: brew install steamcmd")
        raise SystemExit(1)

    install_dir.mkdir(parents=True, exist_ok=True)
    command = build_steamcmd_command(
        app_id=app_id,
        install_dir=install_dir,
        username=username,
        platform=platform,
        validate=validate,
    )

    click.echo(f"Downloading Steam app {app_id}...")
    result = subprocess.run(command, check=False)
    if result.returncode != 0:
        click.secho(
            f"Error: SteamCMD failed with exit code {result.returncode}.", fg="red"
        )
        raise SystemExit(1)


def _decompile_source(
    source_dll: Path,
    output_dir: Path,
    env: dict[str, str] | None,
    missing_hint: str,
    preserve_dirs: set[str] | None = None,
) -> None:
    """Decompile one Assembly-CSharp.dll into C# source files."""
    if not source_dll.exists():
        click.secho(f"Error: {GAME_DLL} not found: {source_dll}", fg="red")
        click.echo(missing_hint)
        raise SystemExit(1)

    output_dir.mkdir(parents=True, exist_ok=True)
    _clean_output_directory(output_dir, preserve_dirs=preserve_dirs)

    click.echo(f"Decompiling {GAME_DLL}...")
    click.echo(f"  Source: {source_dll}")
    click.echo(f"  Output: {output_dir}")

    result = subprocess.run(
        [
            _get_ilspycmd_path(),
            "-p",
            "-o",
            str(output_dir),
            str(source_dll),
        ],
        env=env,
        capture_output=True,
        text=True,
        check=False,
    )

    if result.returncode != 0:
        click.secho("Error: Decompilation failed.", fg="red")
        if result.stderr:
            click.echo(result.stderr)
        raise SystemExit(1)

    cs_files = list(output_dir.rglob("*.cs"))
    click.echo()
    click.secho(f"Decompiled {len(cs_files)} files.", fg="green")
    click.echo()
    click.echo("Key files for combat logging:")
    for name in ["Character.cs", "Stats.cs", "UseSkill.cs", "CastSpell.cs", "NPC.cs"]:
        if (output_dir / name).exists():
            click.echo(f"  {name}")


def _ensure_ilspycmd_ready(dotnet8_root: Path | None) -> None:
    """Validate ilspycmd and its runtime before decompiling."""
    if not _check_ilspycmd_installed():
        click.secho("Error: ilspycmd not found.", fg="red")
        click.echo()
        click.echo("Install it with:")
        click.echo("  dotnet tool install -g ilspycmd")
        raise SystemExit(1)

    if sys.platform != "win32":
        if not dotnet8_root:
            click.secho("Error: DOTNET8_ROOT not set in cli/.env", fg="red")
            click.echo()
            click.echo("ilspycmd requires .NET 8 runtime. Set DOTNET8_ROOT to your")
            click.echo(".NET 8 installation:")
            click.echo()
            click.echo(
                "  Homebrew (macOS): DOTNET8_ROOT=/opt/homebrew/opt/dotnet@8/libexec"
            )
            click.echo()
            click.echo("Install .NET 8 via Homebrew if needed:")
            click.echo("  brew install dotnet@8")
            raise SystemExit(1)

        if not dotnet8_root.exists():
            click.secho(f"Error: DOTNET8_ROOT does not exist: {dotnet8_root}", fg="red")
            raise SystemExit(1)


def _build_ilspy_environment(dotnet8_root: Path | None) -> dict[str, str] | None:
    """Build the environment needed by ilspycmd."""
    if sys.platform == "win32":
        return None

    if dotnet8_root is None:
        return None

    env = os.environ.copy()
    env["DOTNET_ROOT"] = str(dotnet8_root)
    return env


def _check_ilspycmd_installed() -> bool:
    """Check if ilspycmd is installed as a dotnet global tool."""
    result = subprocess.run(
        ["dotnet", "tool", "list", "-g"],
        capture_output=True,
        text=True,
        check=False,
    )
    return "ilspycmd" in result.stdout


def _get_ilspycmd_path() -> str:
    """Get the path to ilspycmd executable."""
    if sys.platform == "win32":
        return "ilspycmd"

    return str(Path.home() / ".dotnet" / "tools" / "ilspycmd")


def _clean_output_directory(
    output_dir: Path, preserve_dirs: set[str] | None = None
) -> None:
    """Remove existing decompiled files, preserving README.md and selected dirs."""
    for f in output_dir.glob("*.cs"):
        f.unlink()
    for f in output_dir.glob("*.csproj"):
        f.unlink()

    preserved = preserve_dirs or set()
    for item in output_dir.iterdir():
        if item.is_dir() and item.name not in preserved:
            shutil.rmtree(item)
