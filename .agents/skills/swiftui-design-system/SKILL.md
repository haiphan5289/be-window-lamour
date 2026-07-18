---
name: swiftui-design-system
description: BE response contract conventions and JSON design system for BE Window Lamour. Defines the standard shape of API responses — snake_case field names, error response envelope, pagination format, status enums, and date/decimal conventions. Use when designing or reviewing the API response format.
---

# BE Response Contract Conventions — JSON Design System

> The BE equivalent of a UI design system: **standard shapes for every API response** to ensure consistent, predictable contracts for the WPF client.

---

## JSON Naming Convention

All fields use `snake_case`. Applied via `[JsonPropertyName]` on DTOs.

```csharp
// ✅ Correct
[JsonPropertyName("is_stop_tracking")] public bool IsStopTracking { get; set; }
[JsonPropertyName("tax_code")]         public string TaxCode { get; set; } = "";
[JsonPropertyName("invoice_number")]   public string InvoiceNumber { get; set; } = "";

// ❌ Wrong — PascalCase leaks to response
public bool IsStopTracking { get; set; }
```

---

## Standard Response Shapes

### Single Object
```json
{
  "id": 1,
  "code": "SUPPLIER_001",
  "name": "Nhà cung cấp ABC",
  "is_stop_tracking": false
}
```

### List
```json
[
  { "id": 1, "code": "S001", "name": "A" },
  { "id": 2, "code": "S002", "name": "B" }
]
```

### Created (201)
```json
{
  "id": 42,
  "code": "NEW_001",
  "name": "New Supplier"
}
```
Location header: `/api/v1/suppliers/42`

### Error (400/404/500)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "DomainException",
  "status": 400,
  "detail": "Code 'S001' already exists."
}
```

---

## Status Enum Serialization

Enums are serialized as strings (not integers):

```csharp
// In Program.cs:
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
```

```json
// ✅ "status": "Confirmed"  (not "status": 1)
{ "id": 1, "status": "Confirmed" }
```

---

## Date/Time Convention

All timestamps stored and returned as **UTC ISO 8601**:

```json
{ "created_at": "2026-04-21T08:30:00Z" }
```

```csharp
[JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
// Store: DateTime.UtcNow
// Display: convert to local time in WPF client
```

---

## Decimal Convention

Decimals serialized as numbers (not strings):

```json
{
  "unit_cost": 150000.00,
  "tax_rate": 0.10,
  "total_amount": 165000.00
}
```

```csharp
[JsonPropertyName("tax_rate")]     public decimal TaxRate { get; set; }
[JsonPropertyName("total_amount")] public decimal TotalAmount { get; set; }
```

---

## Pagination Shape

```json
{
  "items": [...],
  "total": 150,
  "page": 1,
  "page_size": 20,
  "total_pages": 8
}
```

---

## Empty vs Null Convention

| Scenario | Convention |
|---|---|
| Optional string | `null` (use `string?`) |
| Required string | `""` default (never null) |
| Empty list | `[]` (never null) |
| Optional object | `null` |
