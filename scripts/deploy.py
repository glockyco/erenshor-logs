#!/usr/bin/env python3
"""Build the mod and deploy to BepInEx plugins folder."""

import os
import shutil
import subprocess
import sys
from pathlib import Path

from dotenv import load_dotenv


def get_project_root() -> Path:
    """Get the project root directory (parent of scripts/)."""
    return Path(__file__).parent.parent


def get_env_path(var_name: str, required: bool = True) -> Path | None:
    """Get a path from environment variable."""
    value = os.getenv(var_name)
    if not value:
        if required:
            print(f"Error: {var_name} not set in .env file")
            print()
            print("To fix this:")
            print("  1. Copy scripts/.env.example to scripts/.env")
            print("  2. Edit scripts/.env and set your paths")
        return None
    return Path(value)


def get_plugins_path() -> Path | None:
    """Get the BepInEx plugins path from environment."""
    # First check explicit plugins path
    plugins_path = get_env_path("BEPINEX_PLUGINS_PATH", required=False)
    if plugins_path:
        return plugins_path

    # Derive from game path
    game_path = get_env_path("ERENSHOR_PATH", required=True)
    if not game_path:
        return None

    return game_path / "BepInEx" / "plugins"


def build_mod() -> bool:
    """Build the mod using dotnet."""
    project_root = get_project_root()
    mod_dir = project_root / "mod"

    print("Building mod...")
    result = subprocess.run(
        ["dotnet", "build", "-c", "Release"],
        cwd=mod_dir,
        capture_output=False,
    )

    if result.returncode != 0:
        print("Build failed")
        return False

    print("Build succeeded")
    return True


def deploy_mod(plugins_path: Path) -> bool:
    """Copy built mod DLL to plugins folder."""
    project_root = get_project_root()
    dll_path = (
        project_root / "mod" / "bin" / "Release" / "netstandard2.1" / "ErenshorLogs.dll"
    )

    if not dll_path.exists():
        print(f"Error: Built DLL not found at {dll_path}")
        print("Make sure the build succeeded.")
        return False

    if not plugins_path.exists():
        print(f"Error: Plugins folder does not exist: {plugins_path}")
        print()
        print("Make sure:")
        print("  1. BepInEx is installed in your game folder")
        print("  2. ERENSHOR_PATH in scripts/.env is correct")
        return False

    dst = plugins_path / "ErenshorLogs.dll"
    shutil.copy2(dll_path, dst)
    print(f"Deployed: {dst}")
    return True


def main() -> int:
    """Main entry point."""
    # Load .env from scripts directory
    env_file = Path(__file__).parent / ".env"
    if not env_file.exists():
        print("Error: scripts/.env file not found")
        print()
        print("To get started:")
        print("  cp scripts/.env.example scripts/.env")
        print("Then edit scripts/.env with your paths.")
        return 1

    load_dotenv(env_file)

    plugins_path = get_plugins_path()
    if not plugins_path:
        return 1

    print(f"Target: {plugins_path}")
    print()

    if not build_mod():
        return 1

    print()

    if not deploy_mod(plugins_path):
        return 1

    print()
    print("Deploy complete!")
    return 0


if __name__ == "__main__":
    sys.exit(main())
