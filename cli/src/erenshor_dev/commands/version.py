"""Display the current project version."""

import subprocess
from datetime import datetime

import click

from erenshor_dev.config import get_project_root


@click.command()
def version() -> None:
    """Display the current git-based version.

    This shows the version that will be used for builds.
    Format: YYYY.MM.DD-COMMITHASH

    Examples:
      Clean:  2026.01.24-72bdbea
      Dirty:  2026.01.24-72bdbea-dirty-20260124-203045
      No git: 0.0.0-20260124-203045
    """
    project_root = get_project_root()

    try:
        # Get version from git
        result = subprocess.run(
            ["git", "log", "-1", "--format=%cd-%h", "--date=format:%Y.%m.%d"],
            capture_output=True,
            text=True,
            check=True,
            cwd=project_root,
        )
        git_version = result.stdout.strip()

        # Check for dirty working tree
        status_result = subprocess.run(
            ["git", "status", "--porcelain"],
            capture_output=True,
            text=True,
            check=True,
            cwd=project_root,
        )

        is_dirty = bool(status_result.stdout.strip())

        if is_dirty:
            timestamp = datetime.utcnow().strftime("%Y%m%d-%H%M%S")
            version_str = f"{git_version}-dirty-{timestamp}"
            click.secho(version_str, fg="yellow", bold=True)
            click.echo("⚠️  Working tree has uncommitted changes")
        else:
            click.secho(git_version, fg="green", bold=True)

    except (subprocess.CalledProcessError, FileNotFoundError):
        # Fallback version
        now = datetime.utcnow()
        date = now.strftime("%Y%m%d")
        time = now.strftime("%H%M%S")
        fallback = f"0.0.0-{date}-{time}"
        click.secho(fallback, fg="red", bold=True)
        click.echo("⚠️  Git not available, using fallback version")
