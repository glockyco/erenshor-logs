"""Decompile game source for reference."""

from __future__ import annotations

import shutil
import subprocess
import sys
from typing import TYPE_CHECKING

import click

from erenshor_dev.config import get_project_root, load_config

if TYPE_CHECKING:
    from pathlib import Path

GAME_DLL = "Assembly-CSharp.dll"


@click.command()
def decompile() -> None:
    """Decompile game source to reference/game-source/.

    Uses ilspycmd to decompile Assembly-CSharp.dll into individual C# files
    for easier searching and hook development. Requires ilspycmd to be
    installed via 'dotnet tool install -g ilspycmd'.

    On macOS/Linux, requires DOTNET8_ROOT to be set in cli/.env since
    ilspycmd needs .NET 8 runtime.
    """
    config = load_config()
    project_root = get_project_root()

    # Check source DLL exists
    source_dll = project_root / "mod" / "lib" / GAME_DLL
    if not source_dll.exists():
        click.secho(f"Error: {GAME_DLL} not found in mod/lib/", fg="red")
        click.echo()
        click.echo("Run 'erenshor setup' first to copy game DLLs.")
        raise SystemExit(1)

    # Check ilspycmd is installed
    if not _check_ilspycmd_installed():
        click.secho("Error: ilspycmd not found.", fg="red")
        click.echo()
        click.echo("Install it with:")
        click.echo("  dotnet tool install -g ilspycmd")
        raise SystemExit(1)

    # On macOS/Linux, check DOTNET8_ROOT is configured
    env = None
    if sys.platform != "win32":
        if not config.dotnet8_root:
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

        if not config.dotnet8_root.exists():
            click.secho(
                f"Error: DOTNET8_ROOT does not exist: {config.dotnet8_root}",
                fg="red",
            )
            raise SystemExit(1)

        # Set up environment for ilspycmd
        import os

        env = os.environ.copy()
        env["DOTNET_ROOT"] = str(config.dotnet8_root)

    # Prepare output directory
    output_dir = project_root / "reference" / "game-source"
    output_dir.mkdir(parents=True, exist_ok=True)

    # Clean existing decompiled files (preserve README.md)
    _clean_output_directory(output_dir)

    click.echo(f"Decompiling {GAME_DLL}...")
    click.echo(f"  Source: {source_dll}")
    click.echo(f"  Output: {output_dir}")

    # Run ilspycmd in project mode
    result = subprocess.run(
        [
            _get_ilspycmd_path(),
            "-p",  # Project mode: one file per class
            "-o",
            str(output_dir),
            str(source_dll),
        ],
        env=env,
        capture_output=True,
        text=True,
    )

    if result.returncode != 0:
        click.secho("Error: Decompilation failed.", fg="red")
        if result.stderr:
            click.echo(result.stderr)
        raise SystemExit(1)

    # Count output files
    cs_files = list(output_dir.rglob("*.cs"))
    click.echo()
    click.secho(f"Decompiled {len(cs_files)} files.", fg="green")
    click.echo()
    click.echo("Key files for combat logging:")
    for name in ["Character.cs", "Stats.cs", "UseSkill.cs", "CastSpell.cs"]:
        if (output_dir / name).exists():
            click.echo(f"  {name}")


def _check_ilspycmd_installed() -> bool:
    """Check if ilspycmd is installed as a dotnet global tool."""
    result = subprocess.run(
        ["dotnet", "tool", "list", "-g"],
        capture_output=True,
        text=True,
    )
    return "ilspycmd" in result.stdout


def _get_ilspycmd_path() -> str:
    """Get the path to ilspycmd executable."""
    if sys.platform == "win32":
        return "ilspycmd"
    else:
        # On macOS/Linux, use the full path from .dotnet/tools
        import os

        home = os.path.expanduser("~")
        return f"{home}/.dotnet/tools/ilspycmd"


def _clean_output_directory(output_dir: Path) -> None:
    """Remove existing decompiled files, preserving README.md."""
    # Remove .cs and .csproj files in root
    for f in output_dir.glob("*.cs"):
        f.unlink()
    for f in output_dir.glob("*.csproj"):
        f.unlink()

    # Remove subdirectories (namespace folders)
    for item in output_dir.iterdir():
        if item.is_dir():
            shutil.rmtree(item)
