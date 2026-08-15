using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Suppliers.Repositories;
using Lamour.Application.Features.WarehouseReceipts.Dtos;
using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public class CreateWarehouseReceiptUseCase : ICreateWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptRepository _receiptRepo;
    private readonly ICustomerRepository         _customerRepo;
    private readonly ISupplierRepository         _supplierRepo;
    private readonly IEmployeeRepository         _employeeRepo;
    private readonly IProductRepository          _productRepo;
    private readonly ILogger<CreateWarehouseReceiptUseCase> _logger;

    public CreateWarehouseReceiptUseCase(
        IWarehouseReceiptRepository receiptRepo,
        ICustomerRepository customerRepo,
        ISupplierRepository supplierRepo,
        IEmployeeRepository employeeRepo,
        IProductRepository productRepo,
        ILogger<CreateWarehouseReceiptUseCase> logger)
    {
        _receiptRepo  = receiptRepo;
        _customerRepo = customerRepo;
        _supplierRepo = supplierRepo;
        _employeeRepo = employeeRepo;
        _productRepo  = productRepo;
        _logger       = logger;
    }

    public async Task<WarehouseReceiptResponseDto> ExecuteAsync(
        CreateWarehouseReceiptRequestDto request, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(WarehouseReceiptType), request.ReceiptType))
            throw new DomainException($"Invalid receipt_type: {request.ReceiptType}. Valid: 1=FinishedGoodsProduced, 2=ReturnedGoods, 3=Other, 4=ProcessingReceived.");

        if (request.Lines.Count == 0)
            throw new DomainException("At least one line item is required.");

        if (request.CustomerId.HasValue && request.SupplierId.HasValue)
            throw new DomainException("A receipt cannot reference both a customer and a supplier — choose one.");

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value, ct);
            if (customer is null)
                throw new DomainException($"Customer with id {request.CustomerId.Value} not found.");
        }

        if (request.SupplierId.HasValue)
        {
            var supplier = await _supplierRepo.GetByIdAsync(request.SupplierId.Value, ct);
            if (supplier is null)
                throw new DomainException($"Supplier with id {request.SupplierId.Value} not found.");
        }

        if (request.EmployeeId.HasValue)
        {
            var employee = await _employeeRepo.GetByIdAsync(request.EmployeeId.Value, ct);
            if (employee is null)
                throw new DomainException($"Employee with id {request.EmployeeId.Value} not found.");
        }

        foreach (var line in request.Lines)
        {
            var product = await _productRepo.GetByIdAsync(line.ProductId, ct);
            if (product is null)
                throw new DomainException($"Product with id {line.ProductId} not found.");
            if (!product.IsActive)
                throw new DomainException($"Hàng hóa '{product.Name}' đã ngưng kinh doanh và không thể nhập kho.");
        }

        var accountingDateUtc = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
        var documentDateUtc   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc);
        var totalAmount       = request.Lines.Sum(l => l.Amount);
        var receiptNumber     = await _receiptRepo.GetNextReceiptNumberAsync(ct);

        var receipt = new WarehouseReceipt
        {
            ReceiptNumber  = receiptNumber,
            ReceiptType    = (WarehouseReceiptType)request.ReceiptType,
            Status         = WarehouseReceiptStatus.Draft,
            CustomerId     = request.CustomerId,
            SupplierId     = request.SupplierId,
            EmployeeId     = request.EmployeeId,
            AccountingDate = accountingDateUtc,
            DocumentDate   = documentDateUtc,
            Description    = request.Description,
            DeliveryPerson = request.DeliveryPerson,
            Reference      = request.Reference,
            TotalAmount    = totalAmount,
            CreatedAt      = DateTime.UtcNow,
            Lines          = request.Lines.Select(l => new WarehouseReceiptLine
            {
                ProductId           = l.ProductId,
                WarehouseId         = l.WarehouseId,
                Quantity            = l.Quantity,
                UnitPrice           = l.UnitPrice,
                Amount              = l.Amount,
                DebitAccount        = l.DebitAccount,
                CreditAccount       = l.CreditAccount,
                CostItem            = l.CostItem,
                CostObject          = l.CostObject,
                Project             = l.Project,
                PurchaseOrderNumber = l.PurchaseOrderNumber,
                SalesContractNumber = l.SalesContractNumber,
                LoanContractNumber  = l.LoanContractNumber,
                StatisticsCode      = l.StatisticsCode,
            }).ToList()
        };

        var saved = await _receiptRepo.AddAsync(receipt, ct);

        _logger.LogInformation(
            "Created WarehouseReceipt {ReceiptNumber} type={ReceiptType}",
            receiptNumber, receipt.ReceiptType);

        return MapToDto(saved);
    }

    internal static WarehouseReceiptResponseDto MapToDto(WarehouseReceipt r) => new()
    {
        Id             = r.Id,
        ReceiptNumber  = r.ReceiptNumber,
        ReceiptType    = (int)r.ReceiptType,
        Status         = r.Status.ToString(),
        CustomerId     = r.CustomerId,
        CustomerName   = r.Customer?.Name,
        SupplierId     = r.SupplierId,
        SupplierName   = r.Supplier?.Name,
        EmployeeId     = r.EmployeeId,
        EmployeeName   = r.Employee?.Name,
        AccountingDate = r.AccountingDate,
        DocumentDate   = r.DocumentDate,
        Description    = r.Description,
        DeliveryPerson = r.DeliveryPerson,
        Reference      = r.Reference,
        TotalAmount    = r.TotalAmount,
        CreatedAt      = r.CreatedAt,
        ConfirmedAt    = r.ConfirmedAt,
        Lines          = r.Lines.Select(l => new WarehouseReceiptLineDto
        {
            Id            = l.Id,
            ProductId     = l.ProductId,
            ProductCode   = l.Product?.Code   ?? "",
            ProductName   = l.Product?.Name   ?? "",
            WarehouseId   = l.WarehouseId,
            WarehouseName = l.Warehouse?.Name ?? "",
            Quantity      = l.Quantity,
            UnitPrice     = l.UnitPrice,
            Amount        = l.Amount,
            DebitAccount  = l.DebitAccount,
            CreditAccount = l.CreditAccount,
            CostItem            = l.CostItem,
            CostObject          = l.CostObject,
            Project             = l.Project,
            PurchaseOrderNumber = l.PurchaseOrderNumber,
            SalesContractNumber = l.SalesContractNumber,
            LoanContractNumber  = l.LoanContractNumber,
            StatisticsCode      = l.StatisticsCode,
        }).ToList()
    };
}
