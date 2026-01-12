# Contributing to Erenshor Logs

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Set up development environment (see README.md)
4. Create a branch for your changes

## Development Setup

### Mod Development

The mod requires:
- .NET Framework 4.7.2 SDK
- Erenshor installed with BepInEx
- References to game assemblies (see mod/README.md)

### Web Development

The web app requires:
- Node.js 18+
- pnpm

```bash
cd web
pnpm install
pnpm dev
```

## Code Style

### C# (Mod)

- Use C# 9.0 features where appropriate
- Follow standard .NET naming conventions
- Add XML documentation for public APIs
- Keep methods focused and small

### TypeScript (Web)

- Use TypeScript strict mode
- Prefer functional patterns
- Use Svelte stores for state management
- Follow existing code formatting (Prettier)

## Commit Messages

We use conventional commits:

```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(mod): add DoT damage attribution
fix(web): correct DPS calculation for partial seconds
docs: update installation instructions
```

## Pull Requests

1. Create a feature branch from `main`
2. Make your changes with clear commits
3. Test your changes thoroughly
4. Update documentation if needed
5. Submit a PR with a clear description

### PR Checklist

- [ ] Code compiles without warnings
- [ ] Changes are tested
- [ ] Documentation is updated
- [ ] Commit messages follow conventions

## Reporting Issues

### Bug Reports

Please include:
- Erenshor version
- Mod version
- Steps to reproduce
- Expected vs actual behavior
- Combat log excerpt if relevant

### Feature Requests

Please include:
- Clear description of the feature
- Use case / why it's needed
- Any implementation ideas

### Attribution Issues

If ability attribution is incorrect:
- Export a combat log showing the issue
- Describe what ability was used
- Note what it was attributed as vs what it should be

## Questions?

Open a discussion on GitHub or reach out on the Erenshor Discord.
