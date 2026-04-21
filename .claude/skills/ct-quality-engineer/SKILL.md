---
name: ct-quality-engineer
description: Multi-dimension QE validation for BE ASP.NET Core features — validates implementation against business requirements, Clean Architecture rules, security, async patterns, EF Core correctness, and test coverage. Provide a feature description or PR diff to get a structured validation report.
---

# BE Quality Engineer — Feature Validation

> Validates a BE feature implementation against business requirements AND technical standards.

---

## Input Format

```
FEATURE: [Feature name]
REQUIREMENT: [Business description / user story]
FILES: [List of changed files or paste diff]
```

---

## Validation Dimensions

### 1. Business Requirement Compliance

- [ ] All specified operations are implemented (CRUD, custom actions)
- [ ] All business rules are enforced in UseCase (not Controller)
- [ ] Stock guard present for export invoice confirmation
- [ ] Invoice immutability enforced (status check before mutation)
- [ ] Unique constraints validated before create/update
- [ ] Role-based access enforced (if required)
- [ ] Response includes all fields the client expects

### 2. Clean Architecture Compliance

- [ ] Controller only dispatches to UseCase — no business logic
- [ ] UseCase only calls Repository — no direct DB access
- [ ] Repository maps Entity → DTO at boundary — no entities in Application layer
- [ ] Domain has zero external dependencies
- [ ] DI extension exists and is called from `Program.cs`

### 3. Async / Threading

- [ ] No `.Result` or `.Wait()` anywhere in the feature
- [ ] `CancellationToken ct = default` on all async public methods
- [ ] No fire-and-forget `Task` (unobserved exceptions)
- [ ] No concurrent EF Core context access

### 4. EF Core Correctness

- [ ] `AsNoTracking()` on all read queries
- [ ] `Include()` present for all navigation properties accessed
- [ ] `SaveChangesAsync()` called after mutations
- [ ] `ExecuteDeleteAsync()` / `ExecuteUpdateAsync()` used for bulk operations
- [ ] Entity configuration exists in `Configurations/`
- [ ] `DbSet<T>` added to AppDbContext

### 5. API Contract

- [ ] All DTO properties have `[JsonPropertyName("snake_case")]`
- [ ] `[Authorize]` on controller (or at action level)
- [ ] `[ProducesResponseType]` annotations present
- [ ] Route follows `/api/v1/[feature]` convention
- [ ] 201 Created returned for POST, 204 NoContent for DELETE

### 6. Security

- [ ] No sensitive data (passwords, tokens) in response DTOs
- [ ] No SQL injection risk (parameterized queries via EF Core)
- [ ] JWT validation not bypassed
- [ ] Input validated — null/empty checks in UseCase

### 7. Test Coverage

- [ ] Unit test for happy path (valid input → correct DTO)
- [ ] Unit test for each business rule violation
- [ ] Unit test for not-found scenario
- [ ] Mock setup matches actual interface signatures
- [ ] `[Trait("Category", "Unit")]` present

---

## Report Format

```markdown
## QE Report: [Feature Name]

### ✅ Passed
- [List passing checks]

### ❌ Failed
- [Check]: [What's missing or wrong]
- [Check]: [What's missing or wrong]

### ⚠️ Warnings
- [Non-blocking issues]

### Verdict
PASS / FAIL — [1-sentence summary]
```
