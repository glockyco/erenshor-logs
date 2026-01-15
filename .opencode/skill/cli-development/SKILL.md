---
name: cli-development
description: Add or modify CLI commands. Use when adding new commands, changing command behavior, or working on the cli/ package.
---

# CLI Development

The `cli/` directory contains development tools for the mod workflow. Commands
are implemented as Click functions and registered in `main.py`.

## Adding a New Command

1. Create a new file in `cli/src/erenshor_dev/commands/`:

```python
"""Brief description of what this command does."""

import click

from erenshor_dev.config import load_config, get_project_root


@click.command()
@click.option("--flag", "-f", is_flag=True, help="Description of flag")
def mycommand(flag: bool) -> None:
    """One-line description shown in --help.

    Extended description explaining what the command does and when to use it.
    """
    config = load_config()  # Only if you need game paths
    # Implementation here
```

2. Register it in `cli/src/erenshor_dev/main.py`:

```python
from erenshor_dev.commands.mycommand import mycommand

cli.add_command(mycommand)
```

## Using Configuration

Commands that need the game installation path should use `load_config()`:

```python
from erenshor_dev.config import load_config

config = load_config()
config.erenshor_path   # Path to game installation
config.managed_path    # Erenshor_Data/Managed/
config.bepinex_path    # BepInEx/
config.plugins_path    # BepInEx/plugins/
config.crossover_bottle  # CrossOver bottle name (macOS)
```

Commands that only need project paths can use helpers directly:

```python
from erenshor_dev.config import get_project_root, get_cli_dir

project_root = get_project_root()  # Repository root
cli_dir = get_cli_dir()            # cli/ directory
```

## Output Conventions

Use Click's output functions for consistent messaging:

```python
click.echo("Normal output")
click.secho("Success!", fg="green")
click.secho("Warning: something", fg="yellow")
click.secho("Error: something", fg="red", err=True)
```

For errors that should stop execution:

```python
click.secho("Error: message", fg="red", err=True)
raise SystemExit(1)
```

## Testing Commands

```bash
cd cli
uv sync                    # Install dependencies
uv run erenshor --help     # List commands
uv run erenshor CMD --help # Command help
```

## Type Checking and Linting

```bash
cd cli
uv run ruff check src      # Lint
uv run ruff format src     # Format
uv run mypy src            # Type check
```

Pre-commit hooks run these automatically on `cli/**/*.py` files.
