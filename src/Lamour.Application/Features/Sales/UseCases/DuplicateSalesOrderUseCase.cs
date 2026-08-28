using Lamour.Application.Features.Sales.Dtos;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class DuplicateSalesOrderUseCase : IDuplicateSalesOrderUseCase
{
    private readonly IGetSalesOrderByIdUseCase     _getById;
    private readonly IGetNextSalesOrderCodeUseCase _getNextCode;
    private readonly ICreateSalesOrderUseCase      _createOrder;
    private readonly ILogger<DuplicateSalesOrderUseCase> _logger;

    public DuplicateSalesOrderUseCase(
        IGetSalesOrderByIdUseCase     getById,
        IGetNextSalesOrderCodeUseCase getNextCode,
        ICreateSalesOrderUseCase      createOrder,
        ILogger<DuplicateSalesOrderUseCase> logger)
    {
        _getById     = getById;
        _getNextCode = getNextCode;
        _createOrder = createOrder;
        _logger      = logger;
    }

    // Tái dùng nguyên ICreateSalesOrderUseCase (validate tồn kho + trừ kho + đồng bộ Đặt cọc) thay vì
    // tự AddAsync entity — bản sao đại diện 1 giao dịch bán hàng THẬT mới (giống hệt user tự gõ lại
    // đơn), nên tồn kho PHẢI trừ lại lần nữa, không phải chỉ copy dữ liệu. Số chứng từ sinh mới qua
    // GetNextSalesOrderCodeUseCase (không nối "-COPY" như DuplicatePaymentUseCase — Payment không có
    // side-effect tồn kho nên "-COPY" chấp nhận được, còn đơn bán hàng cần 1 số chứng từ thật hợp lệ).
    public async Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _getById.ExecuteAsync(id, ct)
            ?? throw new NotFoundException($"SalesOrder with id {id} not found.");

        var documentNumber = await _getNextCode.ExecuteAsync(SalesOrderCodeSource.Direct, ct);
        var today = DateTime.UtcNow.Date;

        var request = new CreateSalesOrderRequestDto
        {
            DocumentNumber          = documentNumber,
            AccountingDate          = today,
            DocumentDate            = today,
            CustomerId              = source.CustomerId,
            CustomerNameOverride    = source.CustomerNameOverride,
            CustomerAddressOverride = source.CustomerAddressOverride,
            EmployeeId              = source.EmployeeId,
            Description             = source.Description,
            Reference               = source.Reference,
            PaymentTerms            = source.PaymentTerms,
            PaymentDueDays          = source.PaymentDueDays,
            PaymentDueDate          = source.PaymentDueDate,
            Notes                   = source.Notes,
            DeliveryMethod          = source.DeliveryMethod,
            PaymentMethod           = source.PaymentMethod,
            Lines                   = source.Lines.Select(l => new SalesOrderLineDto
            {
                ProductId         = l.ProductId,
                WarehouseId       = l.WarehouseId,
                IsPromotion       = l.IsPromotion,
                Unit              = l.Unit,
                Quantity          = l.Quantity,
                UnitPrice         = l.UnitPrice,
                DiscountRate      = l.DiscountRate,
                Amount            = l.Amount,
                IsAmountManual    = l.IsAmountManual,
                ReceivableAccount = l.ReceivableAccount,
                RevenueAccount    = l.RevenueAccount,
            }).ToList(),
        };

        var duplicated = await _createOrder.ExecuteAsync(request, ct);

        _logger.LogInformation("Duplicated SalesOrder {SourceId} → {NewId} ({DocumentNumber})",
            id, duplicated.Id, duplicated.DocumentNumber);

        return duplicated;
    }
}
