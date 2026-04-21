---
name: ct-figma-implement-design
description: Implement a BE API from an API contract spec, Swagger doc, or client-side design document. Maps the iOS "Figma to code" workflow to the BE equivalent — translating a client API contract (WPF DTOs, Swagger spec, or endpoint description) into correct ASP.NET Core implementation. Use when a client contract already exists and the BE must match it exactly.
argument-hint: "contractSource:[WPF DTOs | Swagger spec | endpoint description] feature:[Feature]"
---

# BE Contract Implementation — API-First from Spec

> Maps an **existing client contract** (WPF DTO file, Swagger spec, or endpoint description) to a correct ASP.NET Core implementation. Ensures 1:1 fidelity between spec and implementation.

---

## Input Formats

### Option A — WPF Client DTO (most common for this project)

Paste the WPF client's existing DTO/Service file:

```csharp
// From desktop-lamour SupplierService.cs
// Endpoint: GET /api/v1/suppliers
// Response: IEnumerable<SupplierResponseDto>
public class SupplierResponseDto
{
    [JsonPropertyName("id")]               public int Id { get; set; }
    [JsonPropertyName("code")]             public string Code { get; set; } = "";
    [JsonPropertyName("is_stop_tracking")] public bool IsStopTracking { get; set; }
}
```

### Option B — Swagger/OpenAPI spec

```yaml
/api/v1/suppliers:
  get:
    summary: List all suppliers
    responses:
      200:
        content:
          application/json:
            schema:
              type: array
              items:
                $ref: '#/components/schemas/SupplierResponse'
```

### Option C — Endpoint description

```
GET /api/v1/suppliers
Auth: Bearer
Response: array of {id: int, code: string, name: string, is_stop_tracking: bool}
```

---

## Implementation Steps

### Step 1 — Extract Contract

From the input, identify:
- HTTP method + route
- Auth requirement
- Request field names + types (exact snake_case spelling)
- Response field names + types (exact snake_case spelling)
- Status codes

### Step 2 — Match Existing DTOs

Check `Lamour.Contracts/` and `Lamour.Application/Features/[Feature]/Dtos/` — reuse if exists, create if not.

```csharp
// MUST match exactly what the WPF client expects
public class SupplierResponseDto
{
    [JsonPropertyName("id")]               public int Id { get; set; }
    [JsonPropertyName("code")]             public string Code { get; set; } = "";
    [JsonPropertyName("name")]             public string Name { get; set; } = "";
    [JsonPropertyName("is_stop_tracking")] public bool IsStopTracking { get; set; }
}
```

### Step 3 — Implement Controller Action

```csharp
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<SupplierResponseDto>), 200)]
public async Task<IActionResult> GetAll(CancellationToken ct)
    => Ok(await _getAll.ExecuteAsync(ct));
```

### Step 4 — Verify Field Parity

Cross-check: every field the WPF client reads must be present in the response DTO with matching `[JsonPropertyName]`.

| WPF Client reads | BE Response DTO |
|---|---|
| `dto.IsStopTracking` | `[JsonPropertyName("is_stop_tracking")]` ✅ |
| `dto.TaxCode` | `[JsonPropertyName("tax_code")]` ✅ |

---

## Contract Parity Checklist

- [ ] All JSON field names match exactly (case-sensitive snake_case)
- [ ] All field types match (int/string/bool/decimal)
- [ ] Nullable fields match (string? vs string)
- [ ] HTTP method and route match exactly
- [ ] Status codes match client expectations (200/201/204/400/404)
- [ ] Auth requirement matches ([Authorize] vs anonymous)
- [ ] Array vs single object response matches
