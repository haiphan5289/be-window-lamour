---
name: ct-generate-usecase
description: Auto-generate and implement a UseCase across all BE Clean Architecture layers — Repository interface → UseCase → Controller action. Use when adding a new business operation to an existing feature. Asks for input before generating.
argument-hint: "useCaseName:[Name] feature:[Feature] httpMethod:[GET|POST|PUT|DELETE] endpoint:[/path] input:[InputType] output:[OutputType]"
---

# BE UseCase Generator — End-to-End

> **Anti-Hallucination:** Verify existing Repository interface, Controller, and DI extension before adding to them.

Auto-generates a **single business operation** (UseCase) and wires it through all layers.

---

## Inputs Required

Before generating, confirm:

| Input | Example |
|---|---|
| `useCaseName` | `ConfirmExportInvoice` |
| `feature` | `ExportInvoices` |
| `httpMethod` | `POST` |
| `endpoint` | `/api/v1/export-invoices/{id}/confirm` |
| `inputType` | `int id` (route param) or `ConfirmRequestDto` |
| `outputType` | `ExportInvoiceResponseDto` or `Unit` |
| Business rules | e.g. "stock guard, immutability check" |

---

## Generation Steps

### Step 1 — Add Method to Repository Interface

```csharp
// Add to I[Feature]Repository interface in Lamour.Infrastructure/Repositories/
Task<[Feature]ResponseDto> [UseCaseName]Async(int id, CancellationToken ct = default);
```

### Step 2 — Implement in Repository

```csharp
// Add to [Feature]Repository.cs
public async Task<[Feature]ResponseDto> [UseCaseName]Async(int id, CancellationToken ct = default)
{
    var entity = await _db.[Feature]s
        .Include(x => x.Lines)
        .FirstOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException(nameof([Feature]), id);

    // TODO: apply DB-level operation
    await _db.SaveChangesAsync(ct);
    return Map(entity);
}
```

### Step 3 — UseCase Interface + Implementation

```csharp
// Lamour.Application/Features/[Feature]/UseCases/[UseCaseName]UseCase.cs
public interface I[UseCaseName]UseCase
{
    Task<[OutputType]> ExecuteAsync([InputType], CancellationToken ct = default);
}

public sealed class [UseCaseName]UseCase : I[UseCaseName]UseCase
{
    private readonly I[Feature]Repository _repository;
    // Inject additional repos if needed (e.g., IProductRepository for stock guard)

    public [UseCaseName]UseCase(I[Feature]Repository repository)
        => _repository = repository;

    public async Task<[OutputType]> ExecuteAsync([InputType], CancellationToken ct = default)
    {
        // Business rule validation
        // e.g., stock guard:
        //   if (product.StockQuantity < line.Quantity)
        //       throw new InsufficientStockException(...);
        // e.g., immutability:
        //   if (invoice.Status != InvoiceStatus.Draft)
        //       throw new DomainException("Only draft invoices can be confirmed.");

        return await _repository.[UseCaseName]Async(id, ct);
    }
}
```

### Step 4 — Controller Action

```csharp
// Add to [Feature]Controller.cs
private readonly I[UseCaseName]UseCase _[useCaseName];

// In constructor, add:
I[UseCaseName]UseCase [useCaseName]

// Add action:
[HttpPost("{id:int}/[route-segment]")]
public async Task<IActionResult> [UseCaseName](int id, CancellationToken ct)
    => Ok(await _[useCaseName].ExecuteAsync(id, ct));
```

### Step 5 — Register in DI

```csharp
// Add to [Feature]ServiceCollectionExtensions.cs
services.AddScoped<I[UseCaseName]UseCase, [UseCaseName]UseCase>();
```

---

## Common Business Rule Patterns

### Stock Guard (Export Invoice Confirm)
```csharp
foreach (var line in invoice.Lines)
{
    var product = await _productRepo.GetByIdAsync(line.ProductId, ct)
        ?? throw new NotFoundException(nameof(Product), line.ProductId);
    if (product.StockQuantity < line.Quantity)
        throw new InsufficientStockException(product.Name, product.StockQuantity, line.Quantity);
}
// Then decrement stock
foreach (var line in invoice.Lines)
    await _productRepo.DecrementStockAsync(line.ProductId, line.Quantity, ct);
```

### Invoice Immutability Check
```csharp
if (invoice.Status != InvoiceStatus.Draft)
    throw new DomainException("Only draft invoices can be modified.");
```

### Unique Code Validation
```csharp
if (await _repository.CodeExistsAsync(dto.Code, excludeId: id, ct: ct))
    throw new DomainException($"Code '{dto.Code}' is already in use.");
```

### Duplicate (Clone) Pattern
```csharp
var source = await _repository.GetEntityByIdAsync(id, ct)
    ?? throw new NotFoundException(nameof([Feature]), id);
var clone = new [Feature]
{
    Code = $"{source.Code}_COPY",
    Name = source.Name,
    // copy all other fields, but NOT Id
};
return await _repository.CreateAsync(clone, ct);
```

---

## Checklist

- [ ] Repository interface method added
- [ ] Repository implementation complete
- [ ] UseCase interface + class created
- [ ] Business rules validated in UseCase (not Controller)
- [ ] Controller action added with correct route + HTTP method
- [ ] UseCase registered in DI extension
- [ ] `CancellationToken` propagated through all layers
- [ ] Unit test for UseCase written
