"""Download and install BepInEx."""

from __future__ import annotations

import io
import subprocess
import sys
import zipfile
from typing import TYPE_CHECKING
from urllib.request import urlopen

import click

from erenshor_dev.config import load_config

if TYPE_CHECKING:
    from erenshor_dev.config import Config

BEPINEX_VERSION = "5.4.23.4"
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

    On macOS, also configures Wine to load the BepInEx proxy DLL.
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

    # On macOS, configure Wine DLL override for BepInEx proxy
    if sys.platform == "darwin":
        _configure_wine_dll_override(config)

    click.echo()
    click.secho("BepInEx installed successfully!", fg="green")
    click.echo()
    click.echo("Next steps:")
    click.echo("  1. Run the game once to generate BepInEx config")
    click.echo("  2. Run 'erenshor deploy' to build and deploy the mod")
    click.echo("  3. Launch the game to test")


def _configure_wine_dll_override(config: Config) -> None:
    """Configure Wine to use native winhttp.dll for BepInEx.

    BepInEx uses winhttp.dll as a proxy to inject into Unity games.
    Wine/CrossOver must be configured to load the game's winhttp.dll
    instead of the built-in Wine version.
    """

    if not config.crossover_bottle:
        click.secho(
            "Warning: CROSSOVER_BOTTLE not set, skipping Wine DLL override.",
            fg="yellow",
        )
        click.echo("  BepInEx may not load without this configuration.")
        click.echo("  Set CROSSOVER_BOTTLE in cli/.env and run 'erenshor bepinex -f'")
        return

    wine_binary = config.wine_binary
    if not wine_binary.exists():
        click.secho(f"Warning: CrossOver wine not found at {wine_binary}", fg="yellow")
        click.echo(
            "  Set CROSSOVER_PATH in cli/.env if CrossOver is installed elsewhere."
        )
        click.echo("  BepInEx may not load without the Wine DLL override.")
        return

    click.echo("Configuring Wine DLL override for BepInEx...")

    # Set per-application DLL override for Erenshor.exe
    # This tells Wine to use the native winhttp.dll from the game folder
    reg_key = r"HKEY_CURRENT_USER\Software\Wine\AppDefaults\Erenshor.exe\DllOverrides"

    result = subprocess.run(
        [
            str(wine_binary),
            "--bottle",
            config.crossover_bottle,
            "reg",
            "add",
            reg_key,
            "/v",
            "winhttp",
            "/t",
            "REG_SZ",
            "/d",
            "native,builtin",
            "/f",
        ],
        capture_output=True,
        text=True,
    )

    if result.returncode != 0:
        click.secho("Warning: Failed to set Wine DLL override.", fg="yellow")
        click.echo(f"  {result.stderr.strip()}")
        click.echo("  BepInEx may not load. You can set this manually in winecfg.")
    else:
        click.echo("  Wine DLL override configured.")
