# Phase 0 — Environment discovery

Recorded 2026-11-25. Facts verified in-session; update if the environment changes.

## Workspace

| Item | Value |
|------|-------|
| Root | `/home/renato/Codee/neti-pruebas/backend` |
| Initial content | Only `.git` |
| Git | Initialized, unborn `HEAD`, no commits, no remotes |
| Default branch | Renamed `master` → `main` (approved) |
| Repository `AGENTS.md` | None (parent directory is not a repository) |

## Toolchain

| Tool | State |
|------|-------|
| .NET SDK | 10.0.404 (LTS) installed locally in `~/.dotnet` |
| ASP.NET Core runtime | 10.0.15 |
| `dotnet-ef` | Not installed yet (Phase 3) |
| Docker CLI | 27.1.1 |
| Docker Compose | v2.21.0 |
| Docker daemon | Down; no `/var/run/docker.sock` |
| PostgreSQL | Client 14.22; cluster 14 `main` installed but down |
| `sudo` | Requires password; agent cannot escalate |
| Init system | WSL2 without systemd (`systemctl` unavailable) |
| Codebase Memory | MCP available; this repository not indexed yet |

## Environment constraints

- The SDK is user-local, so commands need `DOTNET_ROOT="$HOME/.dotnet"` and `~/.dotnet` on `PATH`.
- Docker and PostgreSQL cannot be started by the agent without privileges; Docker Desktop/WSL integration will provide the database service for Phase 6.
- Because the project starts empty, the structural baseline is trivially green; real gates begin with Phase 2 (`dotnet build`).

## Baseline evidence

| Command | Result |
|---------|--------|
| `git status --short --branch` | `## main` clean after initial commit |
| `dotnet --version` | `10.0.404` |
| `dotnet --list-sdks` | `10.0.404 [/home/renato/.dotnet/sdk]` |
| `dotnet --list-runtimes` | `Microsoft.AspNetCore.App 10.0.15`, `Microsoft.NETCore.App 10.0.15` |

## Notes for future sessions

- Use `export DOTNET_ROOT="$HOME/.dotnet" PATH="$HOME/.dotnet:$PATH"` before .NET commands or add the SDK to the shell profile deliberately.
- Do not assume `docker` or `psql` services are running; check `docker info` / `pg_isready` first.
