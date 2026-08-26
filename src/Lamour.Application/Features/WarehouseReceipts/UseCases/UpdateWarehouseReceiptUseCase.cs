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

public class UpdateWarehouseReceiptUseCase : IUpdateWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptRepository _receiptRepo;
    private readonly ICustomerRepository         _customerRepo;
    private readonly ISupplierRepository         _supplierRepo;
    private readonly IEmployeeRepository         _employeeRepo;
    private readonly IProductRepository          _productRepo;
    private readonly ILogger<UpdateWarehouseReceiptUseCase> _logger;

    public UpdateWarehouseReceiptUseCase(
        IWarehouseReceiptRepository receiptRepo,
        ICustomerRepository customerRepo,
        ISupplierRepository supplierRepo,
        IEmployeeRepository employeeRepo,
        IProductRepository productRepo,
        ILogger<UpdateWarehouseReceiptUseCase> logger)
    {
        _receiptRepo  = receiptRepo;
        _customerRepo = customerRepo;
        _supplierRepo = supplierRepo;
        _employeeRepo = employeeRepo;
        _productRepo  = productRepo;
        _logger       = logger;
    }

    public async Task<WarehouseReceiptResponseDto> ExecuteAsync(
        int id, UpdateWarehouseReceiptRequestDto request, CancellationToken ct = default)
    {
        var receipt = await _receiptRepo.GetByIdAsync(id, ct)
            ?? throw new DomainException($"WarehouseReceipt with id {id} not found.");

        if (receipt.Status != WarehouseReceiptStatus.Draft)
            throw new DomainException("Only Draft receipts can be updated. Bỏ ghi phiếu trước khi sửa.");

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

        receipt.ReceiptType    = (WarehouseReceiptType)request.ReceiptType;
        receipt.CustomerId     = request.CustomerId;
        receipt.SupplierId     = request.SupplierId;
        receipt.EmployeeId     = request.EmployeeId;
        receipt.AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
        receipt.DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc);
        receipt.Description    = request.Description;
        receipt.DeliveryPerson = request.DeliveryPerson;
        receipt.Reference      = request.Reference;
        receipt.TotalAmount    = request.Lines.Sum(l => l.Amount);

        // Receipt còn Draft chưa từng tác động tồn kho (chỉ Confirm mới cộng kho), nên replace
        // toàn bộ dòng hàng không cần hoàn tác/tính lại tồn kho gì. FK WarehouseReceiptLine →
        // WarehouseReceipt là required + OnDelete(Cascade) (xem WarehouseReceiptLineConfiguration)
        // nên .Lines.Clear() sẽ khiến EF xóa các dòng cũ mồ côi khi SaveChanges.
        receipt.Lines.Clear();
        foreach (var l in request.Lines)
        {
            receipt.Lines.Add(new WarehouseReceiptLine
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
            });
        }

        await _receiptRepo.SaveChangesAsync(ct);

        // Reload để nạp Product/Warehouse navigation của các dòng mới (cần cho MapToDto hiển thị
        // product_code/product_name/warehouse_name) — .Lines.Add() ở trên không tự Include được.
        var reloaded = await _receiptRepo.GetByIdAsync(id, ct)
            ?? throw new DomainException($"WarehouseReceipt with id {id} not found after update.");

        _logger.LogInformation("Updated WarehouseReceipt {ReceiptNumber}", reloaded.ReceiptNumber);

        return CreateWarehouseReceiptUseCase.MapToDto(reloaded);
    }
}
