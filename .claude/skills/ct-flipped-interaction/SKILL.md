---
name: ct-flipped-interaction
description: Ask clarifying questions before implementing any BE feature in BE Window Lamour. Use when the user provides a vague or incomplete feature request and you need to gather full requirements — scope, API contract, EF entities, business rules, DI wiring, and test strategy — before writing any code.
model: sonnet
effort: medium
---

# BE Flipped Interaction - Ask Before Implementing

> **Anti-Hallucination:** Verify every class, interface, route, and EF entity against the codebase before generating code. See [ct-anti-hallucination](../ct-anti-hallucination/SKILL.md).

## Overview

This skill implements the **Flipped Interaction Pattern** for ASP.NET Core backend development in BE Window Lamour. Instead of immediately proposing solutions, the AI asks systematic clarifying questions first to fully understand the requirements before writing any code.

## When to Use This Skill

**Use this skill when:**
- The feature request is vague or underspecified
- The API contract or WPF client DTO shape is unclear
- EF entity changes or new tables are involved
- Business rules and validation logic need confirmation
- DI wiring and layer responsibilities are ambiguous
- You want to avoid rework from incorrect assumptions

## Input Format

```
FEATURE_REQUEST: [Feature description]
CONTEXT: [Context and reason for this feature]
PRIORITY: [High / Medium / Low]
```

## Priority Field Behavior

The **PRIORITY** field shapes how the AI asks questions and proposes solutions:

- **High**: Focus on fastest, lowest-risk solutions. Target minimum viable endpoint. Reuse existing entities and patterns. Suggest incremental implementation.
- **Medium**: Balance speed vs. quality. Cover complete business logic and edge cases. May require new entity or UseCase creation.
- **Low**: Explore optimal, future-proof solutions. Include scalability, pagination, and validation details. May propose architecture improvements.

## Flipped Interaction Rules

**CRITICAL: Follow these rules strictly**

1. **Ask clarifying questions FIRST** — do not propose any implementation
2. **DO NOT assume** requirements not explicitly stated
3. **DO NOT provide code** until all requirements are crystal clear
4. **DO NOT start implementation** until confirmed understanding is 100%
5. **Always verify** that referenced entities, DTOs, and routes already exist

## Information Categories to Gather

### 1. Feature Scope & Requirements

- What endpoint(s) are needed and what do they do?
- What are the acceptance criteria and success/failure scenarios?
- Are there business rules (uniqueness, stock guard, immutability)?

### 2. API Contract

- HTTP method + route (e.g. `POST /api/v1/suppliers`)
- Request body shape: field names (snake_case), types, nullable
- Response shape: fields, types, status codes (200/201/204/400/404)
- Auth requirement: `[Authorize]` Bearer or anonymous?

### 3. Data Model

- Does this involve an existing EF entity or a new one?
- What columns/properties are needed?
- Any unique constraints, foreign keys, or soft-delete requirements?

### 4. Business Rules & Validation

- What makes a request invalid (400 DomainException)?
- What triggers a not-found error (404 NotFoundException)?
- Are there state machine constraints (e.g. Confirmed invoice is immutable)?

### 5. Testing Requirements

- Should UseCase tests mock the repository with Moq?
- Are integration tests (SQLite in-memory via WebApplicationFactory) needed?
- Which edge cases must be explicitly tested?

## Questioning Strategy

- Ask **one essential question at a time** to avoid overwhelming the user
- After each answer, determine if more clarification is needed or if implementation can begin
- Adjust question depth based on PRIORITY: High → fewer, focused questions; Low → thorough exploration
- Confirm your full understanding before writing any code

## Architecture Requirements for Implementation

Once requirements are confirmed, all implementations must follow:
- **Clean Architecture** — Api → Application → Domain → Infrastructure
- **DTOs only** at API boundaries — never expose EF entities
- **CancellationToken ct = default** on every async public method
- **AsNoTracking()** on all read-only EF queries
- **GlobalExceptionHandler** handles domain exceptions — no try/catch in Controllers
- **[JsonPropertyName("snake_case")]** on every DTO field
- **ILogger<T>** injected via constructor — never `Console.WriteLine`

**Important:** Do not provide any code or architecture proposal until the user confirms all requirements are understood.

---

## Example Interaction

### Sample Input

```
FEATURE_REQUEST: Add a duplicate supplier endpoint
CONTEXT: Users want to clone an existing supplier's data as a starting point
PRIORITY: Low
```

---

### AI Behavior (Low Priority → thorough exploration)

**Question 1 — Contract:**

> Before I propose anything, I want to confirm the API contract.
>
> Should this be:
> - `POST /api/v1/suppliers/{id}/duplicate` — duplicates supplier with given id, or
> - `POST /api/v1/suppliers/duplicate` with a body `{ source_id: int }`?
>
> This determines the route template and whether the WPF client passes the ID in the path or body.

---

**User answers:** "POST /api/v1/suppliers/{id}/duplicate in the path."

---

**Question 2 — Duplicate Behavior:**

> When cloning a supplier:
> - Should the new `code` be auto-generated (e.g. `ORIGINAL-COPY`) or left blank for the user to fill in?
> - Should the response return the newly created supplier immediately (201), or just a success indicator?
> - Should it fail if the source supplier doesn't exist (404) or if the generated code already exists (400)?

---

**User answers:** "Code = original + '-COPY', return 201 with new supplier, 404 if source missing, 400 if code conflict."

---

**Confirmed Understanding:**

> Let me confirm what I'll implement:
>
> - `POST /api/v1/suppliers/{id}/duplicate` — requires Bearer auth
> - Fetches source supplier by id → throws `NotFoundException` if not found (→ 404)
> - Creates new supplier with `Code = "{original}-COPY"`, same name/phone/address/tax_code
> - Validates new code is unique → throws `DomainException` if conflict (→ 400)
> - Returns `201 Created` with `SupplierResponseDto` + `Location: /api/v1/suppliers/{newId}`
> - Layers: `DuplicateSupplierUseCase` → `ISupplierRepository` → `SuppliersController`
> - Unit tests: duplicate success, source not found (404), code conflict (400)
>
> Does this match your expectations? Shall I start implementation?

---

### Priority Impact Summary

| Priority | Questions Asked | Scope |
|---|---|---|
| **High** | 1–2 focused on route + return shape | Fastest path using existing patterns |
| **Medium** | 3–4 covering business rules + edge cases | Balanced: new UseCase, full validation |
| **Low** | 4–5 thorough: all edge cases + tests + DI | Optimal architecture, full test suite |
