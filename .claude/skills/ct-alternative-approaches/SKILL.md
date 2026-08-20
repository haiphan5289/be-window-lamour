---
name: ct-alternative-approaches
description: Generate 3–5 alternative solutions for BE Window Lamour ASP.NET Core problems with pros/cons analysis, C# code examples, comparison matrix, and decision framework. Use when you need to evaluate trade-offs between different architectural or implementation strategies before committing to one approach.
model: sonnet
effort: high
---

# BE Alternative Approaches - Multiple Solution Analysis

> **Anti-Hallucination:** Verify every class, interface, route, and EF entity against the codebase before generating code. See [ct-anti-hallucination](../ct-anti-hallucination/SKILL.md).

> **Lesson learned (verify existing infra before proposing solutions):** A prior run analyzed "tối ưu load data WPF gọi BE" and confidently claimed "no caching layer exists anywhere" after grepping only for `IMemoryCache`/`MemoryCache`/`ICacheService`/`singleton.*cache`. The real infra used a different vocabulary entirely — `IEntityCacheStore<T>`, `RealtimeSyncService`, `PostLoginSyncService`, `SignalRNotificationBroadcaster`, `DataSyncHub` — a full realtime cache-aside system (SignalR push + per-entity cache stores + post-login warmup) already covering Customers/Employees/Products/Suppliers/Categories/ProductUnits/AccountSettings/Warehouses on both BE and WPF. The narrow grep missed all of it, so every "alternative solution" proposed was solving an already-solved problem.
>
> **Before claiming "X doesn't exist" as a premise for any solution:**
> - Grep broadly, not just for the term you expect (`cache`) — also try `realtime`, `sync`, `hub`, `signalr`, `websocket`, `store`, `warmup`, `invalidat*`.
> - Check DI registration files (`*ServiceCollectionExtensions.cs`) for the feature area — they list every service actually wired up, which is faster and more reliable than guessing names.
> - Read the concrete method body of the layer you think is "just a raw HTTP call" (e.g. the `Data/Services/*.cs` implementation, not just its interface) — a cache-aside check can be hiding a few lines into the method.
> - If a problem "shouldn't exist yet" in a codebase this mature, that itself is a signal to look harder before writing 5 solutions for it.

## Overview

This skill generates **3–5 alternative solutions** for ASP.NET Core backend development problems in BE Window Lamour, with comprehensive pros/cons analysis, C# code examples, a comparison matrix, and a decision framework. It helps evaluate trade-offs before committing to an implementation strategy.

## When to Use This Skill

**Use this skill when:**
- Multiple viable approaches exist for the same problem
- Trade-offs between complexity, performance, and maintainability need evaluation
- The team needs to make an informed architectural decision
- Refactoring options need to be compared
- You want to avoid premature optimization or over-engineering

## Input Format

```
PROBLEM: [ASP.NET Core development problem or feature to solve]
CONTEXT: [Feature domain in BE Window Lamour]
COMPLEXITY_LEVEL: [Simple / Medium / Complex]
FOCUS_AREAS: [Aspects to focus on, optional]
SOLUTION_COUNT: [Number of alternatives: 3-5, optional]
```

## Analysis Structure

When the user provides input, generate multiple solutions following this structure:

---

### 1. Problem Analysis Framework

- Analyze the problem requirements and constraints
- Identify key technical challenges
- Consider performance, scale, and complexity factors
- Define success criteria for solutions
- Note WPF client contract requirements (snake_case, specific status codes)

### 2. Solution Generation (3–5 Alternatives)

- Generate multiple viable approaches using different methodologies
- Each solution must solve the **same problem** with a different strategy
- Organize by categories: Architecture-based, Technology-based, Implementation-based
- Ensure all solutions follow Clean Architecture patterns

---

## Required Solution Format

Each solution must include:

```markdown
## Solution [Number]: [Approach Name]

### Core Concept
Brief description of the fundamental approach and methodology.

### Implementation Strategy
Detailed explanation of how this solution works.

### Code Example
```csharp
// C# implementation example
// Namespace: Lamour.Application / Lamour.Infrastructure / etc.
```

### Advantages (Pros)
- ✅ Advantage 1: Explanation
- ✅ Advantage 2: Explanation

### Disadvantages (Cons)
- ❌ Disadvantage 1: Explanation
- ❌ Disadvantage 2: Explanation

### Best Use Cases
- Scenario 1: When to use this approach
- Scenario 2: Specific conditions that favor this solution

### Performance Impact
- DB query cost: [High/Medium/Low]
- Memory usage: [High/Medium/Low]
- Network I/O: [High/Medium/Low]

### Implementation Complexity
- Development time: [Short/Medium/Long]
- Learning curve: [Easy/Moderate/Steep]
- Testing complexity: [Simple/Moderate/Complex]
- Maintenance effort: [Low/Medium/High]
```

---

### 3. Evaluation & Comparison Matrix

After all solutions, provide a side-by-side comparison:

```markdown
| Criteria | Solution A | Solution B | Solution C |
|----------|------------|------------|------------|
| Development Time | ... | ... | ... |
| Complexity | ... | ... | ... |
| Performance | ... | ... | ... |
| Maintainability | ... | ... | ... |
| Scalability | ... | ... | ... |
| Team Learning Curve | ... | ... | ... |
| Recommended For | ... | ... | ... |
```

Score each criterion 1–5 for objective comparison.

### 4. Decision Framework

Provide a decision tree or framework to help choose between solutions:
- Consider: timeline, team experience, complexity requirements
- Offer specific recommendations for different scenarios
- Include risk assessment for each approach

### 5. Code Quality Standards for Every Solution

Every solution must address:
- Error handling with domain exceptions (`NotFoundException`, `DomainException`)
- `CancellationToken ct = default` on all async methods
- `AsNoTracking()` on read-only EF queries
- Unit test examples using xUnit + Moq
- Constructor injection only — no `new XxxService()`

---

## Architecture Requirements

All solutions must follow:
- **Clean Architecture** — Api → Application → Domain → Infrastructure
- **Interfaces** for every cross-layer dependency
- **DTOs only** at API boundary — never expose EF entities
- **[JsonPropertyName("snake_case")]** on all DTO fields
- **Constructor injection** via `Microsoft.Extensions.DependencyInjection`

## Customization Options

- **Solution Count**: 3–5 (default 3 for Simple, 4–5 for Complex)
- **Detail Level**: High-level concepts vs. full implementation
- **Focus Areas**: Performance, maintainability, testability, EF Core patterns
- **Team Context**: Adjust recommendations to team skill level

**Important:** Each solution must be a **viable alternative for the same problem** — not different problems. The goal is to explore different strategies to solve the exact same requirement.

---

## Example Problem Analysis

### Sample Input

```
PROBLEM: Implement pagination for a large supplier list (thousands of records)
CONTEXT: Suppliers feature — GET /api/v1/suppliers needs page/page_size support
COMPLEXITY_LEVEL: Medium
FOCUS_AREAS: Performance, WPF client contract
SOLUTION_COUNT: 3
```

### Context Analysis

- Performance: High (thousands of records, frequent polling from WPF)
- Scale: Medium (grows over time)
- Complexity: Moderate
- WPF client: sends `?page=1&page_size=20` query params

---

### Solution 1: EF Core Skip/Take (Offset Pagination)

**Core Concept**: Standard page-number pagination using EF Core `.Skip()` + `.Take()`. Simple, stateless, matches WPF client's `page` + `page_size` parameters exactly.

```csharp
public async Task<PagedResponseDto<SupplierResponseDto>> GetPagedAsync(
    int page, int pageSize, CancellationToken ct)
{
    var query = _context.Suppliers.AsNoTracking();
    var total = await query.CountAsync(ct);
    var items = await query
        .OrderBy(s => s.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(s => new SupplierResponseDto { /* map */ })
        .ToListAsync(ct);

    return new PagedResponseDto<SupplierResponseDto>
    {
        Items = items, Total = total, Page = page,
        PageSize = pageSize, TotalPages = (int)Math.Ceiling((double)total / pageSize)
    };
}
```

- ✅ Simple, matches WPF contract exactly, stateless
- ✅ Easy to understand and test
- ❌ COUNT query on every request (performance cost at large scale)
- ❌ Inconsistent results if records inserted/deleted between pages
- **Best for**: Up to ~100K records, simple admin UIs, stable datasets

**Performance**: DB Query Medium · Memory Low  
**Complexity**: Dev Short · Learning Easy · Testing Simple · Maintenance Low

---

### Solution 2: Cursor-Based Pagination (Keyset)

**Core Concept**: Instead of SKIP/TAKE, use a `last_id` cursor. The query uses `WHERE id > lastId LIMIT pageSize` — constant-time regardless of page number.

```csharp
public async Task<CursorPagedResponseDto<SupplierResponseDto>> GetAfterAsync(
    int? lastId, int pageSize, CancellationToken ct)
{
    var query = _context.Suppliers.AsNoTracking()
        .Where(s => lastId == null || s.Id > lastId)
        .OrderBy(s => s.Id)
        .Take(pageSize + 1); // +1 to detect hasMore

    var items = await query.Select(s => new SupplierResponseDto { /* map */ }).ToListAsync(ct);
    var hasMore = items.Count > pageSize;
    return new CursorPagedResponseDto<SupplierResponseDto>
    {
        Items = items.Take(pageSize).ToList(),
        NextCursor = hasMore ? items[pageSize - 1].Id : null
    };
}
```

- ✅ Constant-time query regardless of page depth
- ✅ No duplicate/missing records under concurrent inserts
- ❌ WPF client must change from page-number to cursor model
- ❌ Cannot jump to arbitrary page number
- **Best for**: Infinite scroll feeds, large datasets (1M+ records), real-time data

**Performance**: DB Query Low · Memory Low  
**Complexity**: Dev Medium · Learning Moderate · Testing Moderate · Maintenance Medium

---

### Solution 3: Cached Count + Lazy Total

**Core Concept**: Use offset pagination but cache the total count in `IMemoryCache` for 60 seconds — reduces COUNT query frequency at cost of slightly stale totals.

```csharp
public async Task<PagedResponseDto<SupplierResponseDto>> GetPagedAsync(
    int page, int pageSize, CancellationToken ct)
{
    var total = await _cache.GetOrCreateAsync("suppliers_count", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
        return await _context.Suppliers.CountAsync(ct);
    });

    var items = await _context.Suppliers.AsNoTracking()
        .OrderBy(s => s.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(s => new SupplierResponseDto { /* map */ })
        .ToListAsync(ct);

    return new PagedResponseDto<SupplierResponseDto> { Items = items, Total = total!, /* ... */ };
}
```

- ✅ Reduces DB load on high-frequency polling from WPF client
- ✅ Keeps WPF client contract identical to Solution 1
- ❌ Total count can be stale by up to 60s (acceptable for admin use)
- ❌ Adds `IMemoryCache` dependency and cache invalidation complexity
- **Best for**: High-frequency WPF polling, medium datasets (10K–500K)

**Performance**: DB Query Low (after cache warm) · Memory Low  
**Complexity**: Dev Medium · Learning Moderate · Testing Moderate · Maintenance Medium

---

### Comparison Matrix

| Criteria | Solution 1: Skip/Take | Solution 2: Cursor | Solution 3: Cached Count |
|---|---|---|---|
| Development Time | Short | Medium | Medium |
| Complexity | Low | Medium | Medium |
| DB Performance | Medium | High | High |
| WPF Client Change | None | Required | None |
| Scalability | Low | High | Medium |
| Team Learning Curve | Easy | Moderate | Moderate |
| **Recommended For** | < 100K records | > 500K / real-time | High-frequency polls |

### Decision Framework

```
If dataset < 100K AND WPF client page-number UX → Solution 1 (Skip/Take)
If dataset > 500K OR real-time data (inserts/deletes during session) → Solution 2 (Cursor)
If WPF polls frequently AND dataset is medium-large → Solution 3 (Cached Count)
```
