import unittest
from pathlib import Path

from erenshor_dev.commands import setup


class SetupReferenceTests(unittest.TestCase):
    def test_required_dlls_include_input_legacy_module(self) -> None:
        self.assertIn("Assembly-CSharp.dll", setup.REQUIRED_DLLS)
        self.assertIn("UnityEngine.CoreModule.dll", setup.REQUIRED_DLLS)
        self.assertIn("UnityEngine.InputLegacyModule.dll", setup.REQUIRED_DLLS)

    def test_playtest_variant_uses_configured_game_install(self) -> None:
        source = setup.resolve_managed_source(
            erenshor_path=Path("/Games/Erenshor Playtest"),
            variant="playtest",
        )

        self.assertEqual(source, Path("/Games/Erenshor Playtest/Erenshor_Data/Managed"))

    def test_main_variant_is_rejected(self) -> None:
        with self.assertRaises(ValueError):
            setup.resolve_managed_source(
                erenshor_path=Path("/Games/Erenshor"),
                variant="main",
            )


if __name__ == "__main__":
    unittest.main()
