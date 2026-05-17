import unittest
from pathlib import Path

from erenshor_dev.commands import launch
from erenshor_dev.config import Config


class SteamLaunchTests(unittest.TestCase):
    def test_detects_main_app_id_from_install_path(self) -> None:
        app_id = launch.detect_steam_app_id(
            Path("/bottle/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor")
        )

        self.assertEqual(app_id, "2382520")

    def test_detects_playtest_app_id_from_install_path(self) -> None:
        app_id = launch.detect_steam_app_id(
            Path(
                "/bottle/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor Playtest"
            )
        )

        self.assertEqual(app_id, "3090030")

    def test_builds_crossover_steam_app_launch_command(self) -> None:
        config = Config(
            erenshor_path=Path(
                "/Users/me/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Erenshor Playtest"
            ),
            crossover_bottle="Steam",
            crossover_path=Path("/Applications/CrossOver.app"),
        )

        command = launch.build_crossover_steam_launch_command(config)

        self.assertEqual(
            command,
            [
                "/Applications/CrossOver.app/Contents/SharedSupport/CrossOver/bin/wine",
                "--bottle",
                "Steam",
                "/Users/me/Bottles/Steam/drive_c/Program Files (x86)/Steam/steam.exe",
                "-applaunch",
                "3090030",
            ],
        )

    def test_rejects_unknown_steam_install_names(self) -> None:
        with self.assertRaises(ValueError):
            launch.detect_steam_app_id(
                Path(
                    "/bottle/drive_c/Program Files (x86)/Steam/steamapps/common/Other Game"
                )
            )


if __name__ == "__main__":
    unittest.main()
