"""Deploy the mod to the game's plugins folder."""

import shutil
import subprocess

import click

from erenshor_dev.config import get_project_root, load_config

MOD_DLL = "ErenshorLogs.dll"


@click.command()
@click.option("--no-build", is_flag=True, help="Skip building before deploy")
@click.option("--release", "-r", is_flag=True, help="Deploy Release build")
def deploy(no_build: bool, release: bool) -> None:
    """Build and deploy the mod to BepInEx plugins folder.

    Builds the mod (unless --no-build is specified) and copies the
    output DLL to the BepInEx plugins folder in your game installation.
    """
    config = load_config()
    mod_path = get_project_root() / "mod"
    configuration = "Release" if release else "Debug"

    # Build first (unless skipped)
    if not no_build:
        click.echo(f"Building mod ({configuration})...")
        result = subprocess.run(
            ["dotnet", "build", "-c", configuration],
            cwd=mod_path,
            capture_output=False,
        )
        if result.returncode != 0:
            raise SystemExit(result.returncode)
        click.echo()

    # Find the built DLL
    dll_path = mod_path / "bin" / configuration / "netstandard2.1" / MOD_DLL
    if not dll_path.exists():
        click.secho(f"Error: Built DLL not found: {dll_path}", fg="red")
        click.echo("Make sure the build succeeded.")
        raise SystemExit(1)

    # Check BepInEx is installed
    plugins_path = config.plugins_path
    if not plugins_path.exists():
        click.secho("Error: BepInEx plugins folder not found.", fg="red")
        click.echo(f"  Expected: {plugins_path}")
        click.echo()
        click.echo("Run 'erenshor bepinex' to install BepInEx first.")
        raise SystemExit(1)

    # Copy the DLL
    dst = plugins_path / MOD_DLL
    shutil.copy2(dll_path, dst)

    click.secho("Deployed successfully!", fg="green")
    click.echo(f"  {dll_path.name} -> {dst}")
