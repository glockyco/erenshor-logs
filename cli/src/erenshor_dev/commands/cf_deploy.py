"""Deploy the mod and web app to Cloudflare Pages."""

from __future__ import annotations

import shutil
import subprocess
from pathlib import Path

import click

from erenshor_dev.config import get_project_root

MOD_DLL = "ErenshorLogs.dll"


def build_preview_alias(commit: str) -> str:
    """Build the preview alias for a git commit."""
    return f"preview-{commit[:7]}"


def build_wrangler_deploy_command(
    *,
    preview: bool,
    preview_alias: str | None,
    message: str | None,
) -> list[str]:
    """Build the Wrangler command for production or preview deploys."""
    if not preview:
        return ["pnpm", "cf-deploy"]

    if not preview_alias:
        raise ValueError("preview_alias is required for preview deploys")

    command = [
        "pnpm",
        "exec",
        "wrangler",
        "versions",
        "upload",
        "--preview-alias",
        preview_alias,
    ]

    if message:
        command.extend(["--message", message])

    return command


def get_git_commit(project_root: Path) -> str:
    """Return the current short git commit hash."""
    result = subprocess.run(
        ["git", "rev-parse", "--short", "HEAD"],
        cwd=project_root,
        check=True,
        capture_output=True,
        text=True,
    )
    return result.stdout.strip()


@click.command(name="cf-deploy")
@click.option("--no-build-mod", is_flag=True, help="Skip building the mod")
@click.option("--no-build-web", is_flag=True, help="Skip building the web app")
@click.option(
    "--debug", is_flag=True, help="Use Debug build of mod (default is Release)"
)
@click.option("--preview", is_flag=True, help="Upload a Worker preview version")
@click.option(
    "--preview-alias",
    type=str,
    default=None,
    help="Preview alias to use with --preview. Defaults to preview-<commit>.",
)
def cf_deploy(
    no_build_mod: bool,
    no_build_web: bool,
    debug: bool,
    preview: bool,
    preview_alias: str | None,
) -> None:
    """Build and deploy the mod and web app to Cloudflare Pages.

    This command:
    1. Builds the mod in Release configuration (unless --debug or --no-build-mod)
    2. Copies the mod DLL to web/static/mods/
    3. Builds the web app (unless --no-build-web)
    4. Deploys to Cloudflare Workers using wrangler

    The deployed site will serve:
    - Web app at: https://erenshor-logs.wowmuch1.workers.dev/
    - Mod DLL at: https://erenshor-logs.wowmuch1.workers.dev/mods/ErenshorLogs.dll

    Prerequisites:
    - wrangler installed: cd web && pnpm install
    - wrangler authenticated: wrangler login
    """
    project_root = get_project_root()
    mod_path = project_root / "mod"
    web_path = project_root / "web"
    configuration = "Debug" if debug else "Release"

    # Step 1: Build the mod (unless skipped)
    if not no_build_mod:
        click.echo(f"Building mod ({configuration})...")
        result = subprocess.run(
            ["dotnet", "build", "-c", configuration],
            cwd=mod_path,
            capture_output=False,
        )
        if result.returncode != 0:
            click.secho("Error: Mod build failed", fg="red")
            raise SystemExit(result.returncode)
        click.echo()

    # Step 2: Find the built DLL
    dll_path = mod_path / "bin" / configuration / "netstandard2.1" / MOD_DLL
    if not dll_path.exists():
        click.secho(f"Error: Built DLL not found: {dll_path}", fg="red")
        click.echo("Make sure the mod build succeeded.")
        click.echo("Run without --no-build-mod or build manually first.")
        raise SystemExit(1)

    # Step 3: Copy DLL to web/static/mods/
    static_mods_path = web_path / "static" / "mods"
    static_mods_path.mkdir(parents=True, exist_ok=True)

    dst = static_mods_path / MOD_DLL
    shutil.copy2(dll_path, dst)
    click.echo(f"Copied {dll_path.name} -> {dst.relative_to(project_root)}")
    click.echo()

    # Step 4: Build the web app (unless skipped)
    if not no_build_web:
        click.echo("Building web app...")
        # Check if node_modules exists
        if not (web_path / "node_modules").exists():
            click.echo("Installing web dependencies...")
            result = subprocess.run(
                ["pnpm", "install"],
                cwd=web_path,
                capture_output=False,
            )
            if result.returncode != 0:
                click.secho("Error: pnpm install failed", fg="red")
                raise SystemExit(result.returncode)

        result = subprocess.run(
            ["pnpm", "build"],
            cwd=web_path,
            capture_output=False,
        )
        if result.returncode != 0:
            click.secho("Error: Web app build failed", fg="red")
            raise SystemExit(result.returncode)
        click.echo()

    # Step 5: Deploy to Cloudflare
    deploy_target = "Cloudflare preview" if preview else "Cloudflare"
    click.echo(f"Deploying to {deploy_target}...")
    if preview and preview_alias is None:
        commit = get_git_commit(project_root)
        preview_alias = build_preview_alias(commit)

    message = (
        f"Preview {preview_alias.removeprefix('preview-')}" if preview_alias else None
    )
    try:
        command = build_wrangler_deploy_command(
            preview=preview,
            preview_alias=preview_alias,
            message=message,
        )
    except ValueError as error:
        click.secho(f"Error: {error}", fg="red")
        raise SystemExit(1) from error

    result = subprocess.run(
        command,
        cwd=web_path,
        capture_output=False,
    )
    if result.returncode != 0:
        click.secho("Error: Cloudflare deployment failed", fg="red")
        click.echo()
        click.echo("Make sure you're authenticated with wrangler:")
        click.echo("  cd web && pnpm wrangler login")
        raise SystemExit(result.returncode)

    click.echo()
    click.secho("Deployment successful!", fg="green")
    click.echo()
    click.echo("URLs:")
    if preview and preview_alias:
        click.echo(
            f"  Preview: https://{preview_alias}-erenshor-logs.wowmuch1.workers.dev/"
        )
    else:
        click.echo("  Web app: https://erenshor-logs.wowmuch1.workers.dev/")
        click.echo(
            "  Mod DLL: "
            "https://erenshor-logs.wowmuch1.workers.dev/mods/ErenshorLogs.dll"
        )
