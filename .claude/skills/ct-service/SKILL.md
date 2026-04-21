---
name: ct-service
description: Generate a typed HttpClient service (interface + implementation) for calling external APIs from the BE. Use when the backend itself needs to call a third-party or internal microservice HTTP endpoint. Follows typed HttpClient pattern via AddHttpClient<TInterface, TImpl>.
argument-hint: "serviceName:[Name] baseUrl:[https://api.example.com] endpoints:[list]"
---

# BE External HttpClient Service Generator

> **Anti-Hallucination:** Verify endpoint paths and DTO shapes before generating. This skill is for OUTBOUND HTTP calls FROM the backend, not for incoming API endpoints.

Generates a **typed HttpClient service** for calling external APIs from `Lamour.Infrastructure`.

---

## When to Use

- The BE needs to call a **third-party API** (payment gateway, SMS provider, etc.)
- The BE needs to call another **internal microservice**
- The BE needs to call an **external supplier data API**

This is **different from** `ct-scaffold` — that generates inbound API handlers. This generates outbound HTTP callers.

---

## File Location

```
Lamour.Infrastructure/
└── ExternalServices/
    ├── I[Name]Service.cs
    └── [Name]Service.cs
    └── Dtos/
        ├── [Operation]RequestDto.cs
        └── [Operation]ResponseDto.cs
```

---

## Template

```csharp
// Lamour.Infrastructure/ExternalServices/I[Name]Service.cs
namespace Lamour.Infrastructure.ExternalServices;

public interface I[Name]Service
{
    Task<[Response]Dto?> [Operation]Async([Request]Dto request, CancellationToken ct = default);
}
```

```csharp
// Lamour.Infrastructure/ExternalServices/[Name]Service.cs
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Lamour.Infrastructure.ExternalServices;

public sealed class [Name]Service : I[Name]Service
{
    private readonly HttpClient _http;
    private readonly ILogger<[Name]Service> _logger;

    public [Name]Service(HttpClient http, ILogger<[Name]Service> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<[Response]Dto?> [Operation]Async(
        [Request]Dto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/endpoint", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<[Response]Dto>(cancellationToken: ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call [Name] API: {Message}", ex.Message);
            throw;
        }
    }
}
```

---

## DTOs

```csharp
// Lamour.Infrastructure/ExternalServices/Dtos/[Operation]RequestDto.cs
using System.Text.Json.Serialization;

namespace Lamour.Infrastructure.ExternalServices.Dtos;

public class [Operation]RequestDto
{
    [JsonPropertyName("field_name")] public string FieldName { get; set; } = "";
}

public class [Operation]ResponseDto
{
    [JsonPropertyName("success")]   public bool Success { get; set; }
    [JsonPropertyName("data")]      public string? Data { get; set; }
    [JsonPropertyName("message")]   public string? Message { get; set; }
}
```

---

## DI Registration

```csharp
// In Program.cs or infrastructure ServiceCollectionExtensions
services.AddHttpClient<I[Name]Service, [Name]Service>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:[Name]:BaseUrl"]
        ?? throw new InvalidOperationException("Missing [Name] base URL"));
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    // Add auth header if needed:
    // client.DefaultRequestHeaders.Add("X-Api-Key", config["ExternalServices:[Name]:ApiKey"]);
});
```

---

## appsettings.json Configuration

```json
{
  "ExternalServices": {
    "[Name]": {
      "BaseUrl": "https://api.example.com",
      "ApiKey": "your-api-key-here"
    }
  }
}
```

---

## Error Handling Patterns

```csharp
// Pattern 1 — throw on non-success (simple)
response.EnsureSuccessStatusCode();

// Pattern 2 — check and return null (graceful degradation)
if (!response.IsSuccessStatusCode)
{
    _logger.LogWarning("External API returned {StatusCode}", response.StatusCode);
    return null;
}

// Pattern 3 — wrap in domain exception
if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    throw new DomainException("External API authentication failed.");
```
