---
name: cocoapods-to-spm
description: Add or upgrade a NuGet package in the BE Window Lamour project — adding new packages, upgrading versions, resolving conflicts, and registering packages in DI. Use when adding or changing a .NET dependency.
argument-hint: "packageName:[Name] version:[X.Y.Z] project:[Lamour.Api|Lamour.Infrastructure|Lamour.Application]"
---

# BE NuGet Package Management

> Adds, upgrades, or configures NuGet packages in the BE Window Lamour solution.

---

## Add a New Package

```bash
# Add to specific project
dotnet add src/Lamour.Infrastructure/Lamour.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Npgsql --version 8.0.0

dotnet add src/Lamour.Api/Lamour.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0

# Restore after adding
dotnet restore
```

---

## Core Package Reference

| Package | Project | Purpose |
|---------|---------|---------|
| `Microsoft.EntityFrameworkCore` | Infrastructure | EF Core ORM |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Infrastructure | PostgreSQL driver |
| `Microsoft.EntityFrameworkCore.Design` | Infrastructure | EF Core migrations |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Api | JWT auth middleware |
| `Swashbuckle.AspNetCore` | Api | Swagger/OpenAPI |
| `xunit` | Tests | Unit test framework |
| `Moq` | Tests | Mocking library |
| `Microsoft.EntityFrameworkCore.Sqlite` | Tests | In-memory SQLite for tests |

---

## Upgrade a Package

```bash
# Check outdated packages
dotnet list src/Lamour.Infrastructure package --outdated

# Upgrade specific package
dotnet add src/Lamour.Infrastructure package Microsoft.EntityFrameworkCore.Npgsql --version 8.0.4

# Verify no breaking changes
dotnet build
dotnet test
```

---

## Register Package in DI (Program.cs)

```csharp
// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

---

## Conflict Resolution

If two packages require different versions of a shared dependency:

```xml
<!-- In .csproj — pin the version explicitly -->
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
```

Run `dotnet restore` and verify `dotnet build` passes after pinning.
