"""Configuration loading from .env file."""

import sys
from pathlib import Path

import click
from dotenv import load_dotenv


class Config:
    """Configuration loaded from .env file."""

    def __init__(
        self,
        erenshor_path: Path,
        crossover_bottle: str | None = None,
        crossover_path: Path | None = None,
    ):
        self.erenshor_path = erenshor_path
        self.crossover_bottle = crossover_bottle
        self.crossover_path = crossover_path or Path("/Applications/CrossOver.app")

    @property
    def managed_path(self) -> Path:
        """Path to the Managed folder containing game DLLs."""
        return self.erenshor_path / "Erenshor_Data" / "Managed"

    @property
    def bepinex_path(self) -> Path:
        """Path to the BepInEx folder."""
        return self.erenshor_path / "BepInEx"

    @property
    def plugins_path(self) -> Path:
        """Path to the BepInEx plugins folder."""
        return self.bepinex_path / "plugins"

    @property
    def wine_binary(self) -> Path:
        """Path to CrossOver's wine binary (macOS only)."""
        return (
            self.crossover_path
            / "Contents"
            / "SharedSupport"
            / "CrossOver"
            / "bin"
            / "wine"
        )


def get_cli_dir() -> Path:
    """Get the cli/ directory path."""
    return Path(__file__).parent.parent.parent


def get_project_root() -> Path:
    """Get the project root directory."""
    return get_cli_dir().parent


def load_config() -> Config:
    """Load configuration from .env file.

    Exits with an error message if configuration is missing or invalid.
    """
    cli_dir = get_cli_dir()
    env_file = cli_dir / ".env"
    env_example = cli_dir / ".env.example"

    if not env_file.exists():
        click.secho("Error: Configuration not found.", fg="red", err=True)
        click.echo(err=True)
        click.echo("Create cli/.env from the example:", err=True)
        click.echo(f"  cp {env_example} {env_file}", err=True)
        click.echo(err=True)
        click.echo(
            "Then edit cli/.env and set ERENSHOR_PATH to your game installation.",
            err=True,
        )
        sys.exit(1)

    load_dotenv(env_file)

    import os

    erenshor_path_str = os.getenv("ERENSHOR_PATH")
    if not erenshor_path_str:
        click.secho("Error: ERENSHOR_PATH not set in cli/.env", fg="red", err=True)
        click.echo(err=True)
        click.echo(
            "Edit cli/.env and set ERENSHOR_PATH to your game installation.", err=True
        )
        click.echo(err=True)
        click.echo("Example paths:", err=True)
        click.echo(
            r"  Windows: C:\Program Files (x86)\Steam\steamapps\common\Erenshor",
            err=True,
        )
        click.echo(
            "  macOS:   ~/Library/Application Support/CrossOver/Bottles/Steam/"
            "drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor",
            err=True,
        )
        sys.exit(1)

    erenshor_path = Path(erenshor_path_str).expanduser()
    if not erenshor_path.exists():
        click.secho(
            f"Error: ERENSHOR_PATH does not exist: {erenshor_path}", fg="red", err=True
        )
        sys.exit(1)

    if not (erenshor_path / "Erenshor.exe").exists():
        click.secho(
            f"Error: Erenshor.exe not found in {erenshor_path}", fg="red", err=True
        )
        click.echo(
            "Make sure ERENSHOR_PATH points to the game installation folder.", err=True
        )
        sys.exit(1)

    crossover_bottle = os.getenv("CROSSOVER_BOTTLE")
    crossover_path_str = os.getenv("CROSSOVER_PATH")
    crossover_path = Path(crossover_path_str) if crossover_path_str else None

    return Config(
        erenshor_path=erenshor_path,
        crossover_bottle=crossover_bottle,
        crossover_path=crossover_path,
    )
