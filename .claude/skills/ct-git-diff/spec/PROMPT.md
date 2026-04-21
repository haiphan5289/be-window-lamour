# Prompt — ct-git-diff

> See [GUARDRAILS.md](GUARDRAILS.md) before executing any step.
> Input parameters are defined in [INPUT_SCHEMA.md](INPUT_SCHEMA.md).
> Output format is defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

---

## Step 1 — Resolve Target Branch

1. Check if a branch name or SHA was passed as an argument.
2. If provided, use it directly. If it is a SHA, validate it exists:
   ```bash
   git cat-file -t <SHA>   # must return "commit"
   ```
3. If omitted, auto-detect the base branch:
   ```bash
   for branch in main dev master; do
     git ls-remote --heads origin $branch | grep -q $branch && echo $branch && break
   done
   ```
4. If none found, stop and ask the user to specify a target explicitly.
5. Confirm the resolved target to the user before proceeding.

---

## Step 2 — Run Git Commands

Run the appropriate git commands based on flags:

```bash
# Current branch name
git rev-parse --abbrev-ref HEAD

# Commit count ahead
git rev-list --count <TARGET>...HEAD

# Summary mode (default)
git diff --stat <TARGET>...HEAD [-- <path>]

# Full mode (--full flag)
git diff <TARGET>...HEAD [-- <path>]

# With --focus: filter by keyword
git diff --name-only <TARGET>...HEAD | grep -i "<keyword>"

# With --since: filter by date
git log <TARGET>...HEAD --since="<date>" --name-only --pretty=format:"" | sort -u

# With --limit: cap file count
git diff --name-only <TARGET>...HEAD | head -<n>
```

Apply `--path` as the trailing `-- <path>` argument to every git command.
Combine flags as needed (e.g. `--path` + `--focus` + `--limit` all at once).

---

## Step 3 — Large Diff Guard

Check if the number of changed files exceeds the `--limit` (default: 100).

If exceeded:
1. Show warning:
   ```
   ⚠️ Large diff: X files changed. Showing first <limit>. Use --path or --limit to narrow scope.
   ```
2. List top changed directories so the user can pick a `--path`:
   ```bash
   git diff --name-only <TARGET>...HEAD | awk -F'/' '{print $1"/"$2}' | sort | uniq -c | sort -rn | head -10
   ```
3. Truncate the file list to the limit and continue.

---

## Step 4 — Classify Files by Architecture Layer

For each changed file, map it to one of these layers based on path keywords:

| Layer | Path keywords |
|-------|--------------|
| Api | `Controllers`, `Middleware`, `Lamour.Api` |
| Application | `UseCases`, `Dtos`, `Features`, `Lamour.Application` |
| Domain | `Entities`, `Exceptions`, `Enums`, `Lamour.Domain` |
| Infrastructure | `Repositories`, `Configurations`, `Migrations`, `Lamour.Infrastructure` |
| Tests | `Tests`, `Spec`, `Mock`, `IntegrationTests` |
| Config / Other | anything else (Program.cs, appsettings, AGENTS.md, etc.) |

Build the Layer 1 summary table from this classification.

---

## Step 5 — Produce Layered Output

Generate all four layers in order. See [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) for the exact format.

### Layer 2 rules by architecture layer:

**Api Layer (Controllers)**
- Flag Controllers doing business logic directly (should delegate to UseCase)
- Flag missing `[Authorize]` on endpoints that require authentication
- Flag try/catch blocks in Controllers (GlobalExceptionHandler should handle errors)
- Flag Controllers injecting repositories directly (must go through UseCase)

**Application Layer (UseCases / DTOs)**
- Flag UseCases with direct EF Core / DbContext calls (must go through IRepository)
- Flag DTOs missing `[JsonPropertyName("snake_case")]` attributes
- Flag async methods missing `CancellationToken ct` parameter

**Domain Layer (Entities / Exceptions)**
- Flag entities with business logic outside the domain layer
- Flag use of `DomainException` or `NotFoundException` incorrectly (wrong type for the scenario)

**Infrastructure Layer (Repositories / EF Config)**
- Flag read-only queries missing `AsNoTracking()`
- Flag `.Result` or `.Wait()` calls (async deadlock risk)
- Flag EF entities returned directly from repository methods (must map to DTOs)
- Flag missing `cancellationToken` passed to EF Core methods

**Tests Layer**
- Note which changed non-test files have corresponding test changes
- Flag changed files with **no corresponding test change** as a coverage gap

### Layer 3 checklist:

Apply the BE review checklist from [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) against the diff. Mark each item ✅ / ❌ / ➖.

For ❌ failures: include `filename:line` reference.

### Layer 4 narrative:

Write 3–8 sentences. Cover: what changed, key decisions, risk areas, missing pieces.

---

## Step 6 — Print Final Confirmation

Print the completion block from [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

Only include next-step suggestions that are directly relevant to findings (e.g. only suggest `/ct-unittest` if test gaps were found).
