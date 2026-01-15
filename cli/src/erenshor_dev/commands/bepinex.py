"""Download and install BepInEx."""

import io
import zipfile
from urllib.request import urlopen

import click

from erenshor_dev.config import load_config

BEPINEX_VERSION = "5.4.23.2"
BEPINEX_URL = (
    f"https://github.com/BepInEx/BepInEx/releases/download/"
    f"v{BEPINEX_VERSION}/BepInEx_win_x64_{BEPINEX_VERSION}.zip"
)


@click.command()
@click.option(
    "--force", "-f", is_flag=True, help="Overwrite existing BepInEx installation"
)
def bepinex(force: bool) -> None:
    """Download and install BepInEx to the game folder.

    Downloads BepInEx from GitHub and extracts it to your Erenshor
    installation. This is required for the mod to load.
    """
    config = load_config()
    game_path = config.erenshor_path
    bepinex_path = config.bepinex_path

    if bepinex_path.exists() and not force:
        click.secho("BepInEx is already installed.", fg="yellow")
        click.echo(f"  Location: {bepinex_path}")
        click.echo()
        click.echo("Use --force to reinstall.")
        return

    click.echo(f"Downloading BepInEx {BEPINEX_VERSION}...")
    click.echo(f"  From: {BEPINEX_URL}")

    try:
        with urlopen(BEPINEX_URL, timeout=30) as response:
            data = response.read()
    except Exception as e:
        click.secho(f"Error downloading BepInEx: {e}", fg="red")
        raise SystemExit(1) from None

    click.echo(f"Extracting to {game_path}...")

    try:
        with zipfile.ZipFile(io.BytesIO(data)) as zf:
            zf.extractall(game_path)
    except Exception as e:
        click.secho(f"Error extracting BepInEx: {e}", fg="red")
        raise SystemExit(1) from None

    # Create plugins directory if it doesn't exist
    config.plugins_path.mkdir(parents=True, exist_ok=True)

    click.echo()
    click.secho("BepInEx installed successfully!", fg="green")
    click.echo()
    click.echo("Next steps:")
    click.echo("  1. Run 'erenshor build' to build the mod")
    click.echo("  2. Run 'erenshor deploy' to deploy the mod")
    click.echo("  3. Launch the game to test")
