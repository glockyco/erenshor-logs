"""Copy game DLLs to mod/lib/ for compilation."""

import shutil
from pathlib import Path

import click

from erenshor_dev.config import get_project_root, load_config

# DLLs required for mod compilation
REQUIRED_DLLS = [
    "Assembly-CSharp.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.InputLegacyModule.dll",
]


def resolve_managed_source(erenshor_path: Path, variant: str) -> Path:
    """Resolve the managed assembly source for the supported game variant."""
    if variant != "playtest":
        raise ValueError("Erenshor Logs currently targets the playtest build")
    return erenshor_path / "Erenshor_Data" / "Managed"


@click.command()
@click.option(
    "--variant",
    type=click.Choice(["playtest"]),
    default="playtest",
    show_default=True,
    help="Game build to copy references from.",
)
def setup(variant: str) -> None:
    """Copy game DLLs to mod/lib/ for compilation.

    This copies the required game assemblies from your Erenshor Playtest
    installation to the mod/lib/ directory so the mod can be compiled.
    """
    config = load_config()
    managed_path = resolve_managed_source(config.erenshor_path, variant)
    lib_path = get_project_root() / "mod" / "lib"

    if not managed_path.exists():
        click.secho(f"Error: Managed folder not found: {managed_path}", fg="red")
        raise SystemExit(1)

    lib_path.mkdir(parents=True, exist_ok=True)

    click.echo(f"Copying DLLs from {managed_path}")
    click.echo(f"              to {lib_path}")
    click.echo()

    copied = 0
    for dll in REQUIRED_DLLS:
        src = managed_path / dll
        dst = lib_path / dll

        if not src.exists():
            click.secho(f"  Warning: {dll} not found, skipping", fg="yellow")
            continue

        shutil.copy2(src, dst)
        click.echo(f"  Copied {dll}")
        copied += 1

    click.echo()
    if copied == len(REQUIRED_DLLS):
        click.secho(f"Done! Copied {copied} DLLs.", fg="green")
    else:
        click.secho(f"Done. Copied {copied}/{len(REQUIRED_DLLS)} DLLs.", fg="yellow")
