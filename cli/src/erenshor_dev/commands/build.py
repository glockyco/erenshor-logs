"""Build the mod."""

import subprocess

import click

from erenshor_dev.config import get_project_root


@click.command()
@click.option("--release", "-r", is_flag=True, help="Build in Release configuration")
def build(release: bool) -> None:
    """Build the mod using dotnet build.

    Compiles the BepInEx plugin. The output DLL will be in
    mod/bin/Debug/ or mod/bin/Release/ depending on configuration.
    """
    mod_path = get_project_root() / "mod"
    configuration = "Release" if release else "Debug"

    click.echo(f"Building mod ({configuration})...")

    result = subprocess.run(
        ["dotnet", "build", "-c", configuration],
        cwd=mod_path,
        capture_output=False,
    )

    if result.returncode != 0:
        raise SystemExit(result.returncode)

    click.echo()
    click.secho("Build successful!", fg="green")
