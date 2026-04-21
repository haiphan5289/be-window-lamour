# Examples — ct-git-diff

---

## Example 1 — Summary mode, auto-detected base

### Input

```
/ct-git-diff
```

### What Claude confirms before running

```
Comparing: feature/add-suppliers → main
(your branch has 4 commits not yet in main)
```

> **Reading the arrow:** `your-branch → base` means "what did I add on top of base?"
> Base branch was auto-detected (main → dev → master, whichever exists remotely).

### Output

```
📊 Diff Summary
─────────────────────────────────────────
Branch:  feature/add-suppliers ← main
Commits: 4 commits ahead
Files:   11 changed (+623 insertions, -8 deletions)

Api (Controllers / Middleware)     │ 1 file  (1A)
Application (UseCases / DTOs)      │ 4 files (3A 1M)
Domain (Entities / Exceptions)     │ 2 files (2A)
Infrastructure (Repos / EF)        │ 3 files (3A)
Tests                              │ 0 files  ⚠️ no test changes
Config / Other                     │ 1 file  (1M — Program.cs)

Api
  A  src/Lamour.Api/Controllers/SuppliersController.cs

Application
  A  src/Lamour.Application/Features/Suppliers/UseCases/ICreateSupplierUseCase.cs
  A  src/Lamour.Application/Features/Suppliers/UseCases/CreateSupplierUseCase.cs
  A  src/Lamour.Application/Features/Suppliers/Dtos/CreateSupplierRequestDto.cs
  M  src/Lamour.Application/Features/Suppliers/Dtos/SupplierResponseDto.cs

Domain
  A  src/Lamour.Domain/Entities/Supplier.cs
  A  src/Lamour.Infrastructure/Persistence/Configurations/SupplierConfiguration.cs

Infrastructure
  A  src/Lamour.Infrastructure/Repositories/SupplierRepository.cs
  A  src/Lamour.Application/Features/Suppliers/SupplierServiceCollectionExtensions.cs
  A  src/Lamour.Infrastructure/Persistence/Migrations/20260421_AddSuppliers.cs

[Layer 2 — Structured Analysis]

Api Layer
  ✅ SuppliersController delegates all logic to CreateSupplierUseCase
  ✅ [Authorize] present on all actions
  ❌ SuppliersController.cs:38 — try/catch block present; GlobalExceptionHandler handles this

Application Layer
  ✅ CreateSupplierUseCase uses ISupplierRepository — no direct DbContext
  ❌ CreateSupplierRequestDto.cs:12 — field 'TaxCode' missing [JsonPropertyName("tax_code")]
  ✅ CancellationToken ct present on all async methods

Infrastructure Layer
  ❌ SupplierRepository.cs:25 — GetAllAsync missing AsNoTracking()
  ✅ No .Result or .Wait() calls
  ✅ Repository maps entity to SupplierResponseDto — no entity leakage

Tests
  ⚠️ 4 changed non-test files with no corresponding test changes

[Layer 3 — Review Checklist]

Architecture
  ✅ Clean Architecture layers respected
  ✅ UseCase contains all business logic
  ✅ Repository abstracts EF Core

Async / EF Core
  ✅ No .Result or .Wait()
  ✅ CancellationToken passed through
  ❌ AsNoTracking() missing in SupplierRepository.cs:25
  ✅ ExecuteDeleteAsync pattern — N/A (no deletes in this diff)

DTO Discipline
  ✅ No EF entities returned from API
  ❌ TaxCode field missing [JsonPropertyName] in CreateSupplierRequestDto.cs:12
  ✅ Nullable fields correct

Business Rules
  ✅ NotFoundException thrown for missing entities
  ✅ DomainException for duplicate code validation
  ➖ Stock guard — N/A (suppliers feature)
  ➖ Invoice immutability — N/A

Security / DI
  ✅ [Authorize] on all endpoints
  ✅ Constructor injection only
  ✅ ILogger<T> used
  ❌ SuppliersController.cs:38 — try/catch swallows exceptions (GlobalExceptionHandler not reached)

Test Coverage
  ❌ No test changes for any of the 4 new UseCase/Repository files

[Layer 4 — Narrative]

This branch adds the complete Suppliers feature across all 4 Clean Architecture layers —
Entity, EF config, migration, repository, UseCase, DTOs, and Controller. The architecture
is generally correct with proper layer separation and DI wiring. Three issues need attention:
`SupplierRepository.GetAllAsync` is missing `AsNoTracking()` (performance risk on large datasets),
`CreateSupplierRequestDto.TaxCode` is missing `[JsonPropertyName("tax_code")]` (will break the
WPF client contract), and `SuppliersController` has a try/catch block at line 38 that should be
removed — `GlobalExceptionHandler` already handles `DomainException` and `NotFoundException`.
No unit tests were added for the new UseCase or Repository, which is a gap for business-critical
supplier creation with duplicate-code validation.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-git-diff COMPLETE — feature/add-suppliers
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Compared: feature/add-suppliers ← main
Files:    11 changed | +623 -8

💡 Suggested Next Steps:
  1. /review-code src/Lamour.Api/Controllers/SuppliersController.cs
  2. /ct-unittest CreateSupplierUseCase
  3. /ct-bugfix-skill — fix AsNoTracking and JsonPropertyName issues
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Example 2 — Narrowed scope with flags

### Input

```
/ct-git-diff main --path src/Lamour.Application --focus UseCases --limit 20
```

### Resolution

```
Target: main (explicit)
Path filter: src/Lamour.Application
Focus filter: UseCases
Limit: 20 files
```

### Behaviour

- Runs: `git diff --name-only main...HEAD -- src/Lamour.Application | grep -i "UseCases" | head -20`
- Only files under `src/Lamour.Application` matching `UseCases` in their path are analyzed
- All four output layers scoped to those files only
- If no matching files found: `ℹ️ No files matching UseCases under src/Lamour.Application in the diff.`
