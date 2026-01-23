"""CLI entry point."""

import click

from erenshor_dev.commands.bepinex import bepinex
from erenshor_dev.commands.build import build
from erenshor_dev.commands.cf_deploy import cf_deploy
from erenshor_dev.commands.decompile import decompile
from erenshor_dev.commands.deploy import deploy
from erenshor_dev.commands.launch import launch
from erenshor_dev.commands.setup import setup


@click.group()
@click.version_option()
def cli() -> None:
    """Erenshor Logs development CLI.

    Tools for building and deploying the combat logging mod.
    """


cli.add_command(setup)
cli.add_command(bepinex)
cli.add_command(build)
cli.add_command(deploy)
cli.add_command(cf_deploy)
cli.add_command(launch)
cli.add_command(decompile)
