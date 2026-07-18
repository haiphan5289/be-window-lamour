---
name: ct-cell
description: Generate BE DTO (Data Transfer Object) models — Request DTOs, Response DTOs, and line-item DTOs for invoices. The BE equivalent of iOS Cell templates. Use when defining the data contract for a new API endpoint or invoice line item.
argument-hint: "dtoName:[Name] type:[Request|Response|LineItem|Pagination] feature:[Feature]"
---

# BE DTO Generator — Request / Response / Line Item Models

> Maps the iOS concept of "Cell" (data display unit) to the BE equivalent: **DTOs** that define the data contract between API and client.

Generates strongly-typed DTOs with `[JsonPropertyName("snake_case")]` for the BE Window Lamour API.

---

## DTO Types

| Type | Purpose |
|---|---|
| `Request` | Input from client — Create/Update operations |
| `Response` | Output to client — what the API returns |
| `LineItem` | Sub-items in invoices (invoice lines) |
| `Pagination` | Paginated list wrapper |

---

## Response DTO Template

```csharp
// Lamour.Application/Features/[Feature]/Dtos/[Name]ResponseDto.cs
using System.Text.Json.Serialization;

namespace Lamour.Application.Features.[Feature].Dtos;

public class [Name]ResponseDto
{
    [JsonPropertyName("id")]               public int Id { get; set; }
    [JsonPropertyName("code")]             public string Code { get; set; } = "";
    [JsonPropertyName("name")]             public string Name { get; set; } = "";
    [JsonPropertyName("is_active")]        public bool IsActive { get; set; }
    [JsonPropertyName("created_at")]       public DateTime CreatedAt { get; set; }
}
```

---

## Request DTOs Template

```csharp
// Create DTO — all required fields
public class Create[Name]RequestDto
{
    [JsonPropertyName("code")]    public string Code { get; set; } = "";
    [JsonPropertyName("name")]    public string Name { get; set; } = "";
    [JsonPropertyName("phone")]   public string Phone { get; set; } = "";
}

// Update DTO — same fields, all optional or same as create
public class Update[Name]RequestDto
{
    [JsonPropertyName("code")]    public string Code { get; set; } = "";
    [JsonPropertyName("name")]    public string Name { get; set; } = "";
    [JsonPropertyName("phone")]   public string Phone { get; set; } = "";
}
```

---

## Invoice Line Item DTO

```csharp
// For import/export invoice lines
public class [Invoice]LineDto
{
    [JsonPropertyName("product_id")]   public int ProductId { get; set; }
    [JsonPropertyName("product_name")] public string ProductName { get; set; } = "";
    [JsonPropertyName("quantity")]     public int Quantity { get; set; }
    [JsonPropertyName("unit_cost")]    public decimal UnitCost { get; set; }
    [JsonPropertyName("subtotal")]     public decimal Subtotal { get; set; }
}

public class Create[Invoice]LineRequestDto
{
    [JsonPropertyName("product_id")] public int ProductId { get; set; }
    [JsonPropertyName("quantity")]   public int Quantity { get; set; }
    [JsonPropertyName("unit_cost")]  public decimal UnitCost { get; set; }
}
```

---

## Invoice with Lines DTO

```csharp
public class ExportInvoiceResponseDto
{
    [JsonPropertyName("id")]              public int Id { get; set; }
    [JsonPropertyName("invoice_number")]  public string InvoiceNumber { get; set; } = "";
    [JsonPropertyName("customer_name")]   public string? CustomerName { get; set; }
    [JsonPropertyName("customer_phone")]  public string? CustomerPhone { get; set; }
    [JsonPropertyName("invoice_date")]    public DateTime InvoiceDate { get; set; }
    [JsonPropertyName("status")]          public string Status { get; set; } = "";
    [JsonPropertyName("discount_amount")] public decimal DiscountAmount { get; set; }
    [JsonPropertyName("tax_rate")]        public decimal TaxRate { get; set; }
    [JsonPropertyName("sub_total")]       public decimal SubTotal { get; set; }
    [JsonPropertyName("tax_amount")]      public decimal TaxAmount { get; set; }
    [JsonPropertyName("total_amount")]    public decimal TotalAmount { get; set; }
    [JsonPropertyName("lines")]           public List<ExportInvoiceLineDto> Lines { get; set; } = [];
}
```

---

## Pagination DTO

```csharp
public class PagedResponseDto<T>
{
    [JsonPropertyName("items")]       public IEnumerable<T> Items { get; set; } = [];
    [JsonPropertyName("total")]       public int Total { get; set; }
    [JsonPropertyName("page")]        public int Page { get; set; }
    [JsonPropertyName("page_size")]   public int PageSize { get; set; }
    [JsonPropertyName("total_pages")] public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}

public class PagedRequestDto
{
    [JsonPropertyName("page")]      public int Page { get; set; } = 1;
    [JsonPropertyName("page_size")] public int PageSize { get; set; } = 20;
    [JsonPropertyName("search")]    public string? Search { get; set; }
}
```

---

## DTO Rules

1. All properties use `[JsonPropertyName("snake_case")]` — WPF client expects snake_case
2. String properties default to `""` not `null` (unless explicitly optional)
3. Nullable types: use `string?` for optional fields
4. Computed fields (Subtotal, TotalAmount) are calculated in the DTO or domain entity — not stored
5. Never include internal fields (e.g., EF navigation properties) in response DTOs
