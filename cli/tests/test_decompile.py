import tempfile
import unittest
from pathlib import Path

from erenshor_dev.commands import decompile


class ReferenceConfigTests(unittest.TestCase):
    def test_create_reference_config_does_not_require_local_game_install(self) -> None:
        config = decompile.create_reference_config(
            {
                "DOTNET8_ROOT": "/tmp/dotnet8",
                "STEAM_USERNAME": "player",
                "STEAMCMD_PLATFORM": "windows",
            }
        )

        self.assertEqual(config.dotnet8_root, Path("/tmp/dotnet8"))
        self.assertEqual(config.steam_username, "player")
        self.assertEqual(config.steam_platform, "windows")


class BuildSteamCmdCommandTests(unittest.TestCase):
    def test_exports_download_command_builder(self) -> None:
        self.assertTrue(hasattr(decompile, "build_steamcmd_command"))

    def _builder(self):
        builder = getattr(decompile, "build_steamcmd_command", None)
        self.assertIsNotNone(builder)
        return builder

    def test_builds_incremental_download_command_for_variant(self) -> None:
        builder = self._builder()
        if builder is None:
            return

        command = builder(
            app_id="2382520",
            install_dir=Path("/tmp/erenshor-main"),
            username="player",
            platform="windows",
            validate=False,
        )

        self.assertEqual(
            command,
            [
                "steamcmd",
                "+@sSteamCmdForcePlatformType",
                "windows",
                "+force_install_dir",
                "/tmp/erenshor-main",
                "+login",
                "player",
                "+app_update",
                "2382520",
                "+quit",
            ],
        )

    def test_builds_validation_download_command(self) -> None:
        builder = self._builder()
        if builder is None:
            return

        command = builder(
            app_id="3090030",
            install_dir=Path("/tmp/erenshor-playtest"),
            username="player",
            platform="windows",
            validate=True,
        )

        self.assertIn("validate", command)
        self.assertLess(command.index("3090030"), command.index("validate"))


class CleanOutputDirectoryTests(unittest.TestCase):
    def test_preserves_variant_directories_when_refreshing_installed_source(
        self,
    ) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            output_dir = Path(tmp)
            main_dir = output_dir / "main"
            playtest_dir = output_dir / "playtest"
            old_dir = output_dir / "old-namespace"
            main_dir.mkdir()
            playtest_dir.mkdir()
            old_dir.mkdir()
            (output_dir / "OldClass.cs").write_text("", encoding="utf-8")

            decompile._clean_output_directory(
                output_dir, preserve_dirs={"main", "playtest"}
            )

            self.assertTrue(main_dir.exists())
            self.assertTrue(playtest_dir.exists())
            self.assertFalse(old_dir.exists())
            self.assertFalse((output_dir / "OldClass.cs").exists())


if __name__ == "__main__":
    unittest.main()
