---
name: ct-unittest
description: Generate xUnit + Moq unit test structure for BE ASP.NET Core — UseCase tests with mocked repositories, Controller tests with mocked UseCases, and repository integration tests with in-memory SQLite. Use when adding tests for any BE layer.
argument-hint: "className:[Name] layer:[UseCase|Controller|Repository] feature:[Feature]"
---

# BE Unit Test Generator — xUnit + Moq

> **Anti-Hallucination:** Verify interface names, method signatures, and DTO types before generating tests.

Generates unit tests for the **BE Window Lamour** project using xUnit + Moq.

---

## Test Conventions

| Convention | Value |
|---|---|
| Framework | xUnit 2.x |
| Mocking | Moq 4.x |
| Naming | `[MethodName]_[Scenario]_[ExpectedBehavior]` |
| Category | `[Trait("Category", "Unit")]` |
| Arrange/Act/Assert | Always comment AAA sections |

---

## UseCase Test Template

```csharp
// tests/Lamour.Application.Tests/Features/[Feature]/[Name]UseCaseTests.cs
using Lamour.Application.Features.[Feature].Dtos;
using Lamour.Application.Features.[Feature].UseCases;
using Lamour.Domain.Exceptions;
using Lamour.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.[Feature];

[Trait("Category", "Unit")]
public class Create[Name]UseCaseTests
{
    private readonly Mock<I[Name]Repository> _repositoryMock;
    private readonly ICreate[Name]UseCase _sut;

    public Create[Name]UseCaseTests()
    {
        _repositoryMock = new Mock<I[Name]Repository>();
        _sut = new Create[Name]UseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReturnsCreatedDto()
    {
        // Arrange
        var dto = new Create[Name]RequestDto { Code = "TEST", Name = "Test Item" };
        var expected = new [Name]ResponseDto { Id = 1, Code = "TEST", Name = "Test Item" };

        _repositoryMock.Setup(r => r.CodeExistsAsync("TEST", null, default))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.[Name]>(), default))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.ExecuteAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Code, result.Code);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Domain.Entities.[Name]>(), default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateCode_ThrowsDomainException()
    {
        // Arrange
        var dto = new Create[Name]RequestDto { Code = "DUPLICATE", Name = "Test" };

        _repositoryMock.Setup(r => r.CodeExistsAsync("DUPLICATE", null, default))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _sut.ExecuteAsync(dto));
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Domain.Entities.[Name]>(), default), Times.Never);
    }
}
```

---

## Invoice Business Rule Test Template

```csharp
[Trait("Category", "Unit")]
public class ConfirmExportInvoiceUseCaseTests
{
    private readonly Mock<IExportInvoiceRepository> _invoiceRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IConfirmExportInvoiceUseCase _sut;

    public ConfirmExportInvoiceUseCaseTests()
    {
        _invoiceRepoMock = new Mock<IExportInvoiceRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _sut = new ConfirmExportInvoiceUseCase(_invoiceRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SufficientStock_ConfirmsInvoice()
    {
        // Arrange
        var invoiceId = 1;
        var invoice = new ExportInvoiceResponseDto { Id = invoiceId, Status = InvoiceStatus.Draft };
        // ... setup mocks

        // Act
        await _sut.ExecuteAsync(invoiceId);

        // Assert
        _invoiceRepoMock.Verify(r => r.ConfirmAsync(invoiceId, default), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_InsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange — product stock < line quantity
        // ...

        // Act & Assert
        await Assert.ThrowsAsync<InsufficientStockException>(() => _sut.ExecuteAsync(1));
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyConfirmed_ThrowsDomainException()
    {
        // Arrange — invoice.Status == Confirmed
        // ...

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _sut.ExecuteAsync(1));
    }
}
```

---

## Controller Test Template

```csharp
// tests/Lamour.Api.Tests/Controllers/[Name]ControllerTests.cs
using Lamour.Api.Controllers;
using Lamour.Application.Features.[Feature].Dtos;
using Lamour.Application.Features.[Feature].UseCases;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Lamour.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public class [Name]ControllerTests
{
    private readonly Mock<IGetAll[Name]UseCase> _getAllMock;
    private readonly Mock<ICreate[Name]UseCase> _createMock;
    private readonly [Name]Controller _sut;

    public [Name]ControllerTests()
    {
        _getAllMock = new Mock<IGetAll[Name]UseCase>();
        _createMock = new Mock<ICreate[Name]UseCase>();
        _sut = new [Name]Controller(_getAllMock.Object, _createMock.Object, /* other mocks */);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        // Arrange
        var items = new List<[Name]ResponseDto>
        {
            new() { Id = 1, Name = "Item 1" },
            new() { Id = 2, Name = "Item 2" }
        };
        _getAllMock.Setup(uc => uc.ExecuteAsync(default)).ReturnsAsync(items);

        // Act
        var result = await _sut.GetAll(default);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<[Name]ResponseDto>>(ok.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreated()
    {
        // Arrange
        var dto = new Create[Name]RequestDto { Name = "New Item" };
        var created = new [Name]ResponseDto { Id = 1, Name = "New Item" };
        _createMock.Setup(uc => uc.ExecuteAsync(dto, default)).ReturnsAsync(created);

        // Act
        var result = await _sut.Create(dto, default);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }
}
```

---

## Integration Test Template (SQLite In-Memory)

```csharp
// tests/Lamour.Api.IntegrationTests/[Name]IntegrationTests.cs
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Trait("Category", "Integration")]
public class [Name]RepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly [Name]Repository _sut;

    public [Name]RepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new [Name]Repository(_db);
    }

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsDto()
    {
        // Arrange
        var entity = new Domain.Entities.[Name] { Code = "T01", Name = "Test" };

        // Act
        var result = await _sut.CreateAsync(entity);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("T01", result.Code);
    }

    public void Dispose() => _db.Dispose();
}
```

---

## Test Naming Convention

```
[MethodName]_[Scenario]_[ExpectedBehavior]

Examples:
  ExecuteAsync_ValidInput_ReturnsCreatedDto
  ExecuteAsync_DuplicateCode_ThrowsDomainException
  ExecuteAsync_InsufficientStock_ThrowsInsufficientStockException
  GetAll_EmptyDatabase_ReturnsEmptyList
  Delete_NonExistentId_ThrowsNotFoundException
```
