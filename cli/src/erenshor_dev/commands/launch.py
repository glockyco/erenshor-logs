"""Launch Erenshor."""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path
from typing import TYPE_CHECKING

import click

from erenshor_dev.config import load_config

if TYPE_CHECKING:
    from erenshor_dev.config import Config

MAIN_STEAM_APP_ID = "2382520"
PLAYTEST_STEAM_APP_ID = "3090030"


@click.command()
def launch() -> None:
    """Launch Erenshor.

    On macOS, launches through Steam via CrossOver using the configured bottle.
    On Windows, launches the game executable directly.
    """
    config = load_config()

    if sys.platform == "darwin":
        _launch_crossover(config)
    elif sys.platform == "win32":
        _launch_windows(config)
    else:
        click.secho(f"Unsupported platform: {sys.platform}", fg="red")
        raise SystemExit(1)


def detect_steam_app_id(erenshor_path: Path) -> str:
    """Return the Steam app id for the configured Erenshor install."""
    install_name = erenshor_path.name.casefold()

    if install_name == "erenshor":
        return MAIN_STEAM_APP_ID
    if install_name == "erenshor playtest":
        return PLAYTEST_STEAM_APP_ID

    raise ValueError(
        "Unable to infer Steam app id from ERENSHOR_PATH. Expected the "
        "install directory to be named 'Erenshor' or 'Erenshor Playtest'."
    )


def find_steam_executable(erenshor_path: Path) -> Path:
    """Return steam.exe beside the steamapps directory for a Steam install."""
    steamapps_dir = erenshor_path.parent.parent
    steam_dir = steamapps_dir.parent
    return steam_dir / "steam.exe"


def build_crossover_steam_launch_command(config: Config) -> list[str]:
    """Build the CrossOver command that launches Erenshor through Steam."""
    if not config.crossover_bottle:
        raise ValueError("CROSSOVER_BOTTLE not set")

    return [
        str(config.wine_binary),
        "--bottle",
        config.crossover_bottle,
        str(find_steam_executable(config.erenshor_path)),
        "-applaunch",
        detect_steam_app_id(config.erenshor_path),
    ]


def _launch_crossover(config: Config) -> None:
    """Launch through Steam via CrossOver on macOS."""
    if not config.crossover_bottle:
        click.secho("Error: CROSSOVER_BOTTLE not set in cli/.env", fg="red")
        click.echo()
        click.echo("Set CROSSOVER_BOTTLE to your CrossOver bottle name.")
        click.echo("Example: CROSSOVER_BOTTLE=Steam")
        raise SystemExit(1)

    try:
        command = build_crossover_steam_launch_command(config)
    except ValueError as error:
        click.secho(f"Error: {error}", fg="red")
        raise SystemExit(1) from error

    steam_executable = Path(command[3])
    if not steam_executable.exists():
        click.secho(f"Error: steam.exe not found: {steam_executable}", fg="red")
        raise SystemExit(1)

    app_id = command[-1]

    click.echo("Launching Erenshor through Steam via CrossOver...")
    click.echo(f"  Bottle: {config.crossover_bottle}")
    click.echo(f"  Steam: {steam_executable}")
    click.echo(f"  App ID: {app_id}")

    result = subprocess.run(command, cwd=steam_executable.parent)

    if result.returncode != 0:
        click.secho(f"CrossOver exited with code {result.returncode}", fg="yellow")


def _launch_windows(config: Config) -> None:
    """Launch directly on Windows."""
    exe_path = config.erenshor_path / "Erenshor.exe"

    click.echo(f"Launching {exe_path}...")

    subprocess.Popen(
        [str(exe_path)],
        cwd=config.erenshor_path,
        creationflags=subprocess.DETACHED_PROCESS,  # type: ignore[attr-defined]
    )

    click.secho("Game launched!", fg="green")
