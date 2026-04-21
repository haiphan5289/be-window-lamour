---
name: ct-theme
description: BE response formatting, error handling conventions, and GlobalExceptionHandler setup for BE Window Lamour. BE equivalent of iOS CT Theme — defines the consistent "look and feel" of API responses across all endpoints. Use when setting up or reviewing error responses, middleware, and response envelope conventions.
---

# BE Response Formatting — GlobalExceptionHandler & Middleware

> The BE equivalent of a UI theme: **consistent error responses, middleware setup, and response formatting** across all endpoints.

---

## GlobalExceptionHandler

Maps domain exceptions to correct HTTP status codes. Registered once in `Program.cs`.

```csharp
// Lamour.Api/Middleware/GlobalExceptionHandler.cs
using Lamour.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException e         => (StatusCodes.Status404NotFound,       "Not Found"),
            DomainException e           => (StatusCodes.Status400BadRequest,      "Business Rule Violation"),
            InsufficientStockException e => (StatusCodes.Status400BadRequest,      "Insufficient Stock"),
            ValidationException e       => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
            _                           => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode == 500)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// In pipeline:
app.UseExceptionHandler();
```

---

## Standard Error Response Shape (ProblemDetails)

```json
{
  "status": 400,
  "title": "Business Rule Violation",
  "detail": "Code 'S001' already exists."
}

{
  "status": 404,
  "title": "Not Found",
  "detail": "Supplier with id 99 was not found."
}

{
  "status": 500,
  "title": "Internal Server Error",
  "detail": "An unexpected error occurred."
}
```

---

## Domain Exceptions

```csharp
// Lamour.Domain/Exceptions/NotFoundException.cs
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id {id} was not found.") { }
}

// Lamour.Domain/Exceptions/DomainException.cs
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Lamour.Domain/Exceptions/InsufficientStockException.cs
public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, int available, int requested)
        : base($"Insufficient stock for '{productName}': available {available}, requested {requested}.") { }
}
```

---

## Request Logging Middleware (optional)

```csharp
// Lamour.Api/Middleware/RequestLoggingMiddleware.cs
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("{Method} {Path}", context.Request.Method, context.Request.Path);
        await _next(context);
        _logger.LogInformation("{Method} {Path} → {StatusCode}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
}
```

---

## Full Middleware Pipeline (Program.cs)

```csharp
app.UseExceptionHandler();          // GlobalExceptionHandler — always first
app.UseHttpsRedirection();
app.UseAuthentication();            // JWT — before Authorization
app.UseAuthorization();
// app.UseMiddleware<RequestLoggingMiddleware>(); // optional
app.MapControllers();
```
