# Output Schema — ct-git-diff

## Layered Output Structure

Always produce output in this exact order. Never skip a layer.

---

### Layer 1 — Diff Summary

```
📊 Diff Summary
─────────────────────────────────────────
Branch:  <current> ← <target>
Commits: N commits ahead
Files:   X changed (+Y insertions, -Z deletions)
```

Followed by a file table grouped by architecture layer:

| Layer | Files Changed |
|-------|--------------|
| Api (Controllers / Middleware) | N |
| Application (UseCases / DTOs) | N |
| Domain (Entities / Exceptions / Enums) | N |
| Infrastructure (Repositories / EF Config / Migrations) | N |
| Tests | N |
| Config / Other | N |

List each changed file under its layer heading with change type:
- `M` — modified
- `A` — added
- `D` — deleted
- `R` — renamed

---

### Layer 2 — Structured Analysis

For each architecture layer that has changes, provide findings. See [PROMPT.md](PROMPT.md) Step 5 for full per-layer rules.

---

### Layer 3 — Review Checklist

```
BE Window Lamour Review Checklist
─────────────────────────────────────────
Architecture
  [ ] Clean Architecture layers respected (no cross-layer shortcuts)
  [ ] UseCases contain all business logic — Controllers are thin
  [ ] Repositories abstract EF Core — no DbContext in UseCases

Async / EF Core
  [ ] No .Result or .Wait() — all awaited
  [ ] CancellationToken ct passed through all async calls
  [ ] AsNoTracking() on all read-only queries
  [ ] ExecuteDeleteAsync() for deletes (not Remove + SaveChanges)

DTO Discipline
  [ ] No EF entities returned from API — DTOs only
  [ ] All DTO fields have [JsonPropertyName("snake_case")]
  [ ] Nullable fields match WPF client contract

Business Rules
  [ ] Stock guard present before export invoice confirmation
  [ ] Invoice immutability enforced (Status == Draft check)
  [ ] NotFoundException thrown for missing entities (not null return)
  [ ] DomainException for business rule violations (not generic Exception)

Security / DI
  [ ] [Authorize] present on protected endpoints
  [ ] No hardcoded secrets or connection strings
  [ ] Constructor injection only — no new XxxService()
  [ ] ILogger<T> used — no Console.WriteLine or Debug.Print

Test Coverage
  [ ] New UseCases have xUnit + Moq unit tests
  [ ] Business rule edge cases (stock guard, immutability) tested
  [ ] Changed files have corresponding test changes
```

Mark each item: ✅ Pass | ❌ Fail (with `file:line`) | ➖ Not applicable

---

### Layer 4 — Narrative Summary

3–8 sentences covering:
1. What changed (high-level feature/fix description)
2. Key architectural decisions visible in the diff
3. Risk areas that could cause regressions
4. Missing pieces (no tests, no error handling, etc.)

Suitable for use as a **PR description draft**.

---

## Final Confirmation

Print this block after all layers complete:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-git-diff COMPLETE — <current branch>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Compared: <current> ← <target>
Files:    X changed | +Y -Z

💡 Suggested Next Steps:
  1. /review-code <highest-risk file>
  2. /ct-unittest <untested UseCase>
  3. /ct-bugfix-skill — if checklist flagged failures
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

Only suggest steps relevant to findings. Never auto-invoke them.
