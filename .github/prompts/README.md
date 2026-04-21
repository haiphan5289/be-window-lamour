# AI Prompts — BE Window Lamour

Prompt files for GitHub Copilot and Claude Code to assist with the **BE Window Lamour** ASP.NET Core backend.

## Available Prompts

| File | Purpose |
|------|---------|
| `AI_generate_endpoint.md` | Generate a full API endpoint across all 4 Clean Architecture layers (Entity → Repository → UseCase → Controller + DI + Migration) |

## How to Use with GitHub Copilot

Open `AI_generate_endpoint.md` and ask Copilot:
> "Follow this guide to generate a new [Feature] endpoint. Feature: [description], Route: [HTTP_METHOD /api/v1/route], Fields: [list fields]"

## How to Use with Claude Code

Reference the prompt file in your instruction:
> "Following `.github/prompts/AI_generate_endpoint.md`, generate the Employees CRUD endpoint."

## Generation Order (for a new feature)

1. Domain entity (`Lamour.Domain/Entities/`)
2. EF Core configuration (`Lamour.Infrastructure/Persistence/Configurations/`)
3. Repository interface + implementation (`Lamour.Infrastructure/Repositories/`)
4. UseCase interface + implementation + DTOs (`Lamour.Application/Features/[Feature]/`)
5. Controller (`Lamour.Api/Controllers/`)
6. DI extension + wire in `Program.cs`
7. EF Core migration

## Key Rules (always enforced)

- All DTOs use `[JsonPropertyName("snake_case")]`
- All async methods accept `CancellationToken ct = default`
- Never use `.Result` or `.Wait()`
- Business logic goes in UseCase, not Controller
- Stock guard required before export invoice confirmation
- `[Authorize]` required on all non-auth endpoints
