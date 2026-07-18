---
name: ct-chain-of-thought
description: Systematic step-by-step technical design analysis for complex BE features in BE Window Lamour. Use when designing a new endpoint or solving a complex problem that requires thorough reasoning across requirements, Clean Architecture layers, data flow, business rules, edge cases, testing, and implementation roadmap.
model: sonnet
effort: high
---

# BE Chain of Thought - Technical Design Analysis

> **Anti-Hallucination:** Verify every class, interface, route, and EF entity against the codebase before generating code. See [ct-anti-hallucination](../ct-anti-hallucination/SKILL.md).

## Overview

This skill provides a systematic Chain of Thought analysis framework for complex ASP.NET Core backend development problems in BE Window Lamour. It breaks down problems into logical steps covering requirement analysis, architecture design, data flow, business rules, edge cases, testing strategy, and implementation roadmap.

## When to Use This Skill

**Use this skill when:**
- Designing a new complex feature end-to-end (e.g. invoice confirmation with stock guard)
- Analyzing technical trade-offs before implementation
- Planning architecture for a multi-layer change
- Identifying risks, edge cases, and test coverage before coding
- Reviewing a design before starting a sprint

## Input Format

```
FEATURE_TO_ANALYZE: [Feature or technical problem to analyze]
CONTEXT: [Context and feature domain in BE Window Lamour]
COMPLEXITY_LEVEL: [Simple / Medium / Complex]
FOCUS_AREAS: [Specific aspects to focus on, optional]
```

## Analysis Structure

When the user provides input, perform a **step-by-step Chain of Thought analysis** across these 6 phases:

---

### 1. Requirement Analysis

- List all functional and non-functional assumptions about the feature
- Identify key API flows and expected request/response contracts
- Define constraints: auth, validation, pagination, concurrency, stock guard
- Identify WPF client expectations (snake_case fields, status codes, error shapes)

### 2. Architecture Design (Clean Architecture)

- Break down feature into layers: Controller → UseCase → IRepository → EF Core → PostgreSQL
- Explain responsibility of each layer and communication patterns
- Identify DI registration points (`XxxServiceCollectionExtensions.cs`)
- Note new EF entities, migrations, or configurations needed

### 3. Data Flow & Logic (Step-by-Step)

- Trace full lifecycle: HTTP request → Controller → UseCase → Repository → DB → DTO → HTTP response
- Include loading, success, and error state handling
- Detail data transformation between layers (entity → DTO mapping)
- Note where domain exceptions are thrown and how GlobalExceptionHandler maps them

### 4. Business Rules & Edge Cases

- List 4–6 possible edge cases or error scenarios
- Propose handling strategies for each (DomainException, NotFoundException, etc.)
- Cover concurrency concerns (e.g. stock consumed by two simultaneous requests)
- Consider invoice state machine transitions (Draft → Confirmed → Cancelled)

### 5. Testing & Validation Plan

- Suggest 3–5 key unit tests using xUnit + Moq
- Identify integration test scenarios requiring `WebApplicationFactory<Program>` + SQLite in-memory
- Describe mock strategies for IRepository dependencies
- Note which business rules must have explicit test coverage

### 6. Implementation Roadmap

- Summarize the step-by-step implementation plan across all 4 layers
- Highlight risks, technical debt, and migration concerns
- Identify potential performance bottlenecks (N+1 queries, missing indexes)
- Estimate relative complexity per layer

---

## Code Standards to Follow

- **Clean Architecture** layers (Api → Application → Domain → Infrastructure)
- **AsNoTracking()** on all read-only EF queries
- **CancellationToken ct = default** on every async public method
- **[JsonPropertyName("snake_case")]** on every DTO field
- **GlobalExceptionHandler** — no try/catch in Controllers
- **ILogger<T>** — never `Console.WriteLine`

## Output Style

Think aloud and explain reasoning before the final summary. The output should read like a **senior backend engineer walking through a design document** before coding — not just a list of bullet points.

**Important:** Do not jump to code immediately. Analyze first, then provide implementation details only after the full analysis is complete.

---

## Example Analysis

### Sample Input

```
FEATURE_TO_ANALYZE: Confirm an export invoice with stock validation
CONTEXT: ExportInvoices feature — decrement stock on confirmation, reject if insufficient
COMPLEXITY_LEVEL: Complex
FOCUS_AREAS: Business rules, concurrency, stock guard
```

---

### 1. Requirement Analysis

**Functional assumptions:**
- `POST /api/v1/export-invoices/{id}/confirm` — confirms a Draft invoice
- Before confirming: validate each line item has sufficient stock
- On success: decrement stock for each product, mark invoice as Confirmed
- Invoice number auto-generated server-side: `XK-{yyyyMMdd}-{seq:D3}`

**Non-functional assumptions:**
- Concurrent confirmation of two invoices using the same product stock must not allow negative stock
- Response: `200 OK` with confirmed `ExportInvoiceResponseDto`
- Error cases: invoice not found (404), not in Draft status (400), insufficient stock (400)

---

### 2. Architecture Design (Clean Architecture)

```
ExportInvoicesController         POST /{id}/confirm → IConfirmExportInvoiceUseCase
         ↓ calls
ConfirmExportInvoiceUseCase      Validates status, checks stock, decrements, saves
         ↓ uses
IExportInvoiceRepository         GetWithLinesAsync, UpdateAsync
IProductRepository               GetByIdAsync, DecrementStockAsync
         ↓ implements
ExportInvoiceRepository          EF Core + AppDbContext
ProductRepository                EF Core + AppDbContext
```

DI: `ExportInvoiceServiceCollectionExtensions.cs` registers UseCase + both repositories.

---

### 3. Data Flow & Logic

1. `POST /{id}/confirm` → Controller calls `_confirmUseCase.ExecuteAsync(id, ct)`
2. UseCase fetches invoice with lines: `_invoiceRepo.GetWithLinesAsync(id, ct)`
3. If null → throw `NotFoundException("ExportInvoice", id)` → 404
4. If `invoice.Status != Draft` → throw `DomainException("Invoice is not in Draft status.")` → 400
5. For each line: fetch product → validate `product.StockQuantity >= line.Quantity`
   - If any fails → throw `InsufficientStockException(productName, available, requested)` → 400
6. Begin DB transaction:
   - Decrement each product's stock: `product.StockQuantity -= line.Quantity`
   - Set `invoice.Status = Confirmed`, `invoice.ConfirmedAt = DateTime.UtcNow`
   - Auto-generate invoice number if not set: `XK-{yyyyMMdd}-{seq:D3}`
7. Save changes → commit transaction
8. Map to `ExportInvoiceResponseDto` → return 200 OK

---

### 4. Business Rules & Edge Cases

| Edge Case | Handling Strategy |
|---|---|
| Invoice not found | `NotFoundException` → 404 |
| Invoice already Confirmed | `DomainException("Invoice is not in Draft status.")` → 400 |
| Invoice Cancelled | Same as above — only Draft can be confirmed |
| One product has insufficient stock | `InsufficientStockException(name, available, requested)` → 400 |
| Concurrent requests for same invoice | DB transaction + row lock; second request gets conflict error |
| Product deleted between fetch and confirm | `NotFoundException` on product fetch → 404 |

---

### 5. Testing & Validation Plan

```csharp
// xUnit + Moq

[Fact]
public async Task ExecuteAsync_InvoiceNotFound_ThrowsNotFoundException()

[Fact]
public async Task ExecuteAsync_InvoiceNotDraft_ThrowsDomainException()

[Fact]
public async Task ExecuteAsync_InsufficientStock_ThrowsInsufficientStockException()

[Fact]
public async Task ExecuteAsync_ValidInvoice_DecrementsStockAndReturnsConfirmedDto()

// Integration test (WebApplicationFactory + SQLite):
[Fact]
public async Task POST_confirm_ValidDraftInvoice_Returns200WithConfirmedStatus()
```

---

### 6. Implementation Roadmap

| Step | Task | Risk |
|---|---|---|
| 1 | Add `Status` enum + `ConfirmedAt` to `ExportInvoice` entity | Low |
| 2 | EF migration: add `Status` column + index | Medium |
| 3 | Implement `IExportInvoiceRepository.GetWithLinesAsync` | Low |
| 4 | Implement `ConfirmExportInvoiceUseCase` with transaction | High |
| 5 | Add `POST /{id}/confirm` action to Controller | Low |
| 6 | Register in `ExportInvoiceServiceCollectionExtensions.cs` | Low |
| 7 | Write unit tests (4 scenarios) | Medium |
| 8 | Write integration test | Medium |

**Risk note:** Concurrency on stock decrement requires a DB-level lock or optimistic concurrency (`RowVersion`) to prevent negative stock under simultaneous requests.
