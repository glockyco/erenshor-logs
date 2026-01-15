"""Deploy the mod to the game's plugins folder."""

from __future__ import annotations

import re
import shutil
import subprocess
from pathlib import Path

import click

from erenshor_dev.config import get_project_root, load_config

MOD_DLL = "ErenshorLogs.dll"

# BepInEx config settings required for Erenshor mods
REQUIRED_CONFIG = {
    "Chainloader": {
        "HideManagerGameObject": "true",
    },
    "Logging.Console": {
        "Enabled": "true",
    },
}


@click.command()
@click.option("--no-build", is_flag=True, help="Skip building before deploy")
@click.option("--release", "-r", is_flag=True, help="Deploy Release build")
def deploy(no_build: bool, release: bool) -> None:
    """Build and deploy the mod to BepInEx plugins folder.

    Builds the mod (unless --no-build is specified) and copies the
    output DLL to the BepInEx plugins folder in your game installation.

    Also validates BepInEx configuration and fixes settings if needed.
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

    # Check and fix BepInEx config
    config_path = config.bepinex_path / "config" / "BepInEx.cfg"
    _check_bepinex_config(config_path)


def _check_bepinex_config(config_path: Path) -> None:
    """Check BepInEx config and fix settings if needed."""
    if not config_path.exists():
        click.echo()
        click.secho("Note: BepInEx config not found.", fg="yellow")
        click.echo("  Run the game once to generate it, then deploy again.")
        return

    content = config_path.read_text()
    changes: list[str] = []

    for section, settings in REQUIRED_CONFIG.items():
        for key, expected in settings.items():
            current = _get_config_value(content, section, key)
            if current is None:
                # Key not found - will be added at end of section
                content = _add_config_value(content, section, key, expected)
                changes.append(f"{key} = {expected} (added)")
            elif current.lower() != expected.lower():
                content = _set_config_value(content, section, key, expected)
                changes.append(f"{key} = {expected} (was {current})")

    if changes:
        config_path.write_text(content)
        click.echo()
        click.echo("Updated BepInEx.cfg:")
        for change in changes:
            click.echo(f"  {change}")


def _get_config_value(content: str, section: str, key: str) -> str | None:
    """Get a config value from an INI-style config file."""
    # Find the section
    section_pattern = rf"^\[{re.escape(section)}\]"
    section_match = re.search(section_pattern, content, re.MULTILINE)
    if not section_match:
        return None

    # Find the next section (or end of file)
    next_section = re.search(r"^\[", content[section_match.end() :], re.MULTILINE)
    section_end = (
        section_match.end() + next_section.start() if next_section else len(content)
    )
    section_content = content[section_match.end() : section_end]

    # Find the key
    key_pattern = rf"^{re.escape(key)}\s*=\s*(.+)$"
    key_match = re.search(key_pattern, section_content, re.MULTILINE)
    if not key_match:
        return None

    return key_match.group(1).strip()


def _set_config_value(content: str, section: str, key: str, value: str) -> str:
    """Set a config value in an INI-style config file."""
    # Find the section
    section_pattern = rf"^\[{re.escape(section)}\]"
    section_match = re.search(section_pattern, content, re.MULTILINE)
    if not section_match:
        return content

    # Find the next section (or end of file)
    next_section = re.search(r"^\[", content[section_match.end() :], re.MULTILINE)
    section_end = (
        section_match.end() + next_section.start() if next_section else len(content)
    )

    # Replace the key value within the section
    before = content[: section_match.end()]
    section_content = content[section_match.end() : section_end]
    after = content[section_end:]

    key_pattern = rf"^({re.escape(key)}\s*=\s*)(.+)$"
    section_content = re.sub(
        key_pattern, rf"\g<1>{value}", section_content, count=1, flags=re.MULTILINE
    )

    return before + section_content + after


def _add_config_value(content: str, section: str, key: str, value: str) -> str:
    """Add a config value to an INI-style config file."""
    # Find the section
    section_pattern = rf"^\[{re.escape(section)}\]"
    section_match = re.search(section_pattern, content, re.MULTILINE)
    if not section_match:
        # Section doesn't exist, add it at the end
        return content.rstrip() + f"\n\n[{section}]\n{key} = {value}\n"

    # Find the next section (or end of file)
    next_section = re.search(r"^\[", content[section_match.end() :], re.MULTILINE)
    if next_section:
        insert_pos = section_match.end() + next_section.start()
        return content[:insert_pos] + f"{key} = {value}\n\n" + content[insert_pos:]
    else:
        return content.rstrip() + f"\n{key} = {value}\n"
