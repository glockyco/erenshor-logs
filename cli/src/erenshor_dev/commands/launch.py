"""Launch Erenshor."""

from __future__ import annotations

import subprocess
import sys
from typing import TYPE_CHECKING

import click

from erenshor_dev.config import load_config

if TYPE_CHECKING:
    from erenshor_dev.config import Config


@click.command()
def launch() -> None:
    """Launch Erenshor.

    On macOS, launches via CrossOver using the configured bottle.
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


def _launch_crossover(config: Config) -> None:
    """Launch via CrossOver on macOS."""
    if not config.crossover_bottle:
        click.secho("Error: CROSSOVER_BOTTLE not set in cli/.env", fg="red")
        click.echo()
        click.echo("Set CROSSOVER_BOTTLE to your CrossOver bottle name.")
        click.echo("Example: CROSSOVER_BOTTLE=Steam")
        raise SystemExit(1)

    exe_path = config.erenshor_path / "Erenshor.exe"

    click.echo("Launching Erenshor via CrossOver...")
    click.echo(f"  Bottle: {config.crossover_bottle}")
    click.echo(f"  Executable: {exe_path}")

    # Use CrossOver's wine binary directly
    result = subprocess.run(
        [
            "/Applications/CrossOver.app/Contents/SharedSupport/CrossOver/bin/wine",
            "--bottle",
            config.crossover_bottle,
            str(exe_path),
        ],
        cwd=config.erenshor_path,
    )

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
