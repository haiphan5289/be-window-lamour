---
name: ct-anti-hallucination
description: Anti-hallucination guardrails for all BE Window Lamour ASP.NET Core code generation. Enforces verify-before-use for every class, interface, method, route, and EF Core entity. Referenced by all other ct-* skills. Invoke directly when you suspect generated code references non-existent types, wrong namespaces, invented EF mappings, or stale file paths.
---

# Anti-Hallucination Rules for BE Window Lamour

> These rules apply to **every code generation task** in this project.  
> Before writing a single line of code, complete the verification checklist below.

---

## The Core Rule

**Never reference any class, interface, method, route, or EF entity you have not verified exists in the current codebase.**

A memory, a prior conversation, or a reference example is NOT proof that something exists now. Code is the only source of truth.

---

## Pre-Generation Verification Checklist

Complete every applicable item before generating code.

### 1. File Paths

- [ ] Use `Glob` to confirm every target file path exists before reading or referencing it
- [ ] If a path does not exist, ask the user — do NOT invent an alternative path
- [ ] Never assume a subfolder exists because a sibling folder exists

### 2. Class / Interface / Record Names

- [ ] Use `Grep` to find the exact declaration (`public class Foo`, `public interface IFoo`, `public record Foo`) before using it
- [ ] Check the namespace — the same name can exist in multiple projects with different behavior
- [ ] Never assume a class name based on a naming pattern (e.g. `SupplierRepository`) without verifying

### 3. Method / Property Signatures

- [ ] Read the actual file containing the class/interface before calling any method on it
- [ ] Verify parameter names, types, and return types exactly — do not guess from the method name
- [ ] Verify `CancellationToken ct = default` is present on async methods before passing it

### 4. EF Core Entities and DbSet

- [ ] Verify `DbSet<T>` exists in `AppDbContext` before referencing it
- [ ] Verify the entity's property names and types by reading `Lamour.Domain/Entities/`
- [ ] Verify EF configuration exists in `Lamour.Infrastructure/Persistence/Configurations/`
- [ ] **NEVER use**: `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` — always `await`
- [ ] **NEVER return** EF entities from the API — map to DTOs at repository boundary

### 5. Repository Interfaces

- [ ] Verify `IXxxRepository` exists in `Lamour.Application` before using it in a UseCase
- [ ] Verify the exact method signatures in the interface (e.g. `GetByIdAsync(int id, CancellationToken ct)`)
- [ ] Verify the repository is registered in DI (`XxxServiceCollectionExtensions.cs`)

### 6. UseCase Interfaces

- [ ] Verify `IXxxUseCase` exists in `Lamour.Application/Features/[Feature]/UseCases/`
- [ ] Verify the UseCase is registered in DI before injecting it into a Controller
- [ ] Verify the `ExecuteAsync` signature matches — input DTO + CancellationToken

### 7. DTO Field Names

- [ ] Read the DTO file to confirm every `[JsonPropertyName("snake_case")]` field before using it
- [ ] Verify nullable vs non-nullable matches the WPF client contract (`string?` vs `string`)
- [ ] Never invent a DTO field name — check what the WPF client sends

### 8. Route Paths

- [ ] Read the Controller `[Route]` attribute before adding a new action
- [ ] Verify the route template matches the WPF client's `HttpClient` calls
- [ ] Never invent a route — use the one defined in the API contract

### 9. NuGet Packages

- [ ] Verify a package is referenced in the `.csproj` before adding a `using` statement for it
- [ ] Never add `using Microsoft.AspNetCore.X` unless the package is referenced
- [ ] Do not copy `using` blocks blindly from reference examples — a different project may not have the same packages

### 10. Reference Files Are Patterns, Not Copy-Paste Sources

- [ ] Reference files show **structural patterns only**
- [ ] Every symbol copied from a reference must be individually verified in the current codebase
- [ ] Legacy patterns in reference files must be replaced with current equivalents

---

## Hallucination Red Flags — Stop and Verify

If you find yourself doing any of the following, **stop and verify** before continuing:

| Red flag | What to do instead |
|---|---|
| Writing `_context.Suppliers` without checking | Read `AppDbContext.cs` to verify `DbSet<Supplier>` |
| Using `ISupplierRepository.GetByCodeAsync` from memory | Read the interface file first |
| Writing `[Route("api/v1/suppliers")]` from memory | Read the existing Controller's Route attribute |
| Assuming `CreateSupplierRequestDto` has a `TaxCode` field | Read the DTO file |
| Injecting `IXxxUseCase` that may not be registered | Check `XxxServiceCollectionExtensions.cs` |
| Calling `.Where().FirstOrDefaultAsync()` without `AsNoTracking()` | Add `AsNoTracking()` for read-only queries |
| Assuming `NotFoundException` exists | Read `Lamour.Domain/Exceptions/` |
| Writing `var invoice = await _repo.GetByIdAsync(id)` without null check | Always throw `NotFoundException` on null |

---

## When Verification Fails

If a required class, method, or file cannot be found in the codebase:

1. **Do not invent a substitute** — report what is missing
2. **Ask the user** before proceeding: _"I could not find `IXxx` in the codebase. Can you point me to the correct name/path?"_
3. If the user confirms it does not exist yet: create it following existing patterns, and flag it clearly as a **new addition**

---

## Quick Verification Commands

```bash
# Verify a class/interface exists
Grep: pattern="public class SupplierRepository|public interface ISupplierRepository"

# Verify DbSet in AppDbContext
Grep: pattern="DbSet<" path="src/Lamour.Infrastructure/Persistence/AppDbContext.cs"

# Verify DTO field names
Read: src/Lamour.Application/Features/Suppliers/Dtos/SupplierResponseDto.cs

# Verify DI registration
Grep: pattern="AddScoped|AddTransient|AddSingleton" path="src/Lamour.Infrastructure"

# Verify a file path
Glob: pattern="**/SupplierRepository.cs"

# Verify UseCase interface
Read: src/Lamour.Application/Features/Suppliers/UseCases/ICreateSupplierUseCase.cs
```
