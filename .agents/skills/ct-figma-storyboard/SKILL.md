---
name: ct-figma-storyboard
description: Generate Swagger/OpenAPI annotations and HTTP integration tests for a BE feature. BE equivalent of iOS Figma-to-Storyboard. Maps an API design into Swagger docs (ProducesResponseType, XML comments) and HttpClient integration test scenarios. Use when documenting an endpoint or writing HTTP-level tests.
argument-hint: "feature:[Feature] operations:[list of endpoint paths]"
---

# BE Swagger Annotations + Integration Tests

> Maps the iOS "Storyboard from Figma" workflow to the BE equivalent: generating **Swagger annotations** (the visual spec) and **HTTP integration tests** (the executable validation).

---

## Part 1 — Swagger/OpenAPI Annotations

Add `[ProducesResponseType]` and XML summary comments to Controllers:

```csharp
/// <summary>Returns all suppliers.</summary>
/// <response code="200">List of suppliers returned successfully.</response>
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<SupplierResponseDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetAll(CancellationToken ct)
    => Ok(await _getAll.ExecuteAsync(ct));

/// <summary>Creates a new supplier.</summary>
/// <param name="dto">Supplier creation data.</param>
/// <response code="201">Supplier created successfully.</response>
/// <response code="400">Validation failed (duplicate code, missing name).</response>
[HttpPost]
[ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] CreateSupplierRequestDto dto, CancellationToken ct)
{
    var result = await _create.ExecuteAsync(dto, ct);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}

/// <summary>Confirms an export invoice. Validates stock availability before confirming.</summary>
/// <response code="200">Invoice confirmed, stock decremented.</response>
/// <response code="400">Insufficient stock or invoice not in Draft status.</response>
/// <response code="404">Invoice not found.</response>
[HttpPost("{id:int}/confirm")]
[ProducesResponseType(typeof(ExportInvoiceResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    => Ok(await _confirm.ExecuteAsync(id, ct));
```

---

## Enable XML Comments in Program.cs

```csharp
builder.Services.AddSwaggerGen(opt =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    opt.IncludeXmlComments(xmlPath);

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});
```

Enable in `.csproj`:
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
```

---

## Part 2 — HTTP Integration Tests

```csharp
// tests/Lamour.Api.IntegrationTests/SuppliersIntegrationTests.cs
[Trait("Category", "Integration")]
public class SuppliersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SuppliersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real DB with SQLite in-memory
                var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("DataSource=:memory:"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GET_suppliers_ReturnsOk()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GetTestToken());

        // Act
        var response = await _client.GetAsync("/api/v1/suppliers");

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<SupplierResponseDto>>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task POST_suppliers_ValidBody_Returns201()
    {
        // Arrange
        var dto = new CreateSupplierRequestDto { Code = "TEST", Name = "Test Supplier" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/suppliers", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SupplierResponseDto>();
        Assert.Equal("TEST", created?.Code);
    }

    [Fact]
    public async Task GET_suppliers_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/suppliers");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string GetTestToken() => "test-jwt-token"; // Use real JWT generation for full tests
}
```
