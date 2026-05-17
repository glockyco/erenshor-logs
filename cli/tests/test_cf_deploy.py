import unittest

from erenshor_dev.commands import cf_deploy


class CloudflareDeployCommandTests(unittest.TestCase):
    def test_builds_production_wrangler_command(self) -> None:
        command = cf_deploy.build_wrangler_deploy_command(
            preview=False,
            preview_alias=None,
            message=None,
        )

        self.assertEqual(command, ["pnpm", "cf-deploy"])

    def test_builds_preview_wrangler_command(self) -> None:
        command = cf_deploy.build_wrangler_deploy_command(
            preview=True,
            preview_alias="preview-fa1dfed",
            message="Preview fa1dfed",
        )

        self.assertEqual(
            command,
            [
                "pnpm",
                "exec",
                "wrangler",
                "versions",
                "upload",
                "--preview-alias",
                "preview-fa1dfed",
                "--message",
                "Preview fa1dfed",
            ],
        )

    def test_preview_requires_alias(self) -> None:
        with self.assertRaises(ValueError):
            cf_deploy.build_wrangler_deploy_command(
                preview=True,
                preview_alias=None,
                message="Preview fa1dfed",
            )

    def test_builds_preview_alias_from_commit(self) -> None:
        self.assertEqual(
            cf_deploy.build_preview_alias("fa1dfed123456789"),
            "preview-fa1dfed",
        )


if __name__ == "__main__":
    unittest.main()
