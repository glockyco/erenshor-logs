#!/usr/bin/env python3
"""Copy game DLLs from Erenshor installation for mod compilation."""

import shutil
import sys
from pathlib import Path

# DLLs required for mod compilation
REQUIRED_DLLS = [
    "Assembly-CSharp.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.IMGUIModule.dll",
]

# Relative path from game root to managed DLLs
MANAGED_PATH = Path("Erenshor_Data") / "Managed"


def get_project_root() -> Path:
    """Get the project root directory (parent of scripts/)."""
    return Path(__file__).parent.parent


def find_managed_folder(game_path: Path) -> Path | None:
    """Find the Managed folder containing game DLLs."""
    managed = game_path / MANAGED_PATH
    if managed.is_dir():
        return managed

    # Maybe user passed the Managed folder directly
    if game_path.name == "Managed" and game_path.is_dir():
        return game_path

    # Maybe user passed Erenshor_Data
    if game_path.name == "Erenshor_Data":
        managed = game_path / "Managed"
        if managed.is_dir():
            return managed

    return None


def copy_dlls(game_path: Path) -> bool:
    """Copy required DLLs from game installation to mod/lib/."""
    managed = find_managed_folder(game_path)
    if managed is None:
        print(f"Error: Could not find Managed folder in {game_path}")
        print(f"Expected: {game_path / MANAGED_PATH}")
        print()
        print("Make sure the path points to your Erenshor installation root,")
        print("e.g., 'C:\\Program Files (x86)\\Steam\\steamapps\\common\\Erenshor'")
        return False

    project_root = get_project_root()
    lib_dir = project_root / "mod" / "lib"
    lib_dir.mkdir(parents=True, exist_ok=True)

    print(f"Source: {managed}")
    print(f"Target: {lib_dir}")
    print()

    missing = []
    copied = []

    for dll in REQUIRED_DLLS:
        src = managed / dll
        dst = lib_dir / dll

        if not src.exists():
            missing.append(dll)
            continue

        shutil.copy2(src, dst)
        copied.append(dll)
        print(f"  Copied: {dll}")

    print()

    if missing:
        print(f"Warning: {len(missing)} DLL(s) not found:")
        for dll in missing:
            print(f"  - {dll}")
        print()
        print("The mod may not compile without these files.")
        return False

    print(f"Successfully copied {len(copied)} DLL(s)")
    return True


def main() -> int:
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: uv run scripts/setup.py <erenshor-path>")
        print()
        print("Arguments:")
        print("  erenshor-path    Path to Erenshor game installation")
        print()
        print("Examples:")
        print(
            '  Windows: uv run scripts/setup.py "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Erenshor"'
        )
        print('  macOS:   uv run scripts/setup.py "/Volumes/UTM/Erenshor"')
        return 1

    game_path = Path(sys.argv[1]).resolve()

    if not game_path.exists():
        print(f"Error: Path does not exist: {game_path}")
        return 1

    success = copy_dlls(game_path)
    return 0 if success else 1


if __name__ == "__main__":
    sys.exit(main())
