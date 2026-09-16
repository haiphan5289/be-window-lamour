using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public class DuplicateSalesReturnUseCase : IDuplicateSalesReturnUseCase
{
    private readonly IGetSalesReturnByIdUseCase     _getById;
    private readonly IGetNextSalesReturnCodeUseCase _getNextCode;
    private readonly ICreateSalesReturnUseCase      _createReturn;
    private readonly IUnconfirmSalesReturnUseCase   _unconfirmReturn;
    private readonly ILogger<DuplicateSalesReturnUseCase> _logger;

    public DuplicateSalesReturnUseCase(
        IGetSalesReturnByIdUseCase     getById,
        IGetNextSalesReturnCodeUseCase getNextCode,
        ICreateSalesReturnUseCase      createReturn,
        IUnconfirmSalesReturnUseCase   unconfirmReturn,
        ILogger<DuplicateSalesReturnUseCase> logger)
    {
        _getById         = getById;
        _getNextCode     = getNextCode;
        _createReturn    = createReturn;
        _unconfirmReturn = unconfirmReturn;
        _logger          = logger;
    }

    // Tái dùng ICreateSalesReturnUseCase để có đủ validate + tự tính lại thuế/giá vốn từ Product
    // giống hệt nhập tay — nhưng CreateSalesReturnUseCase luôn "Cất" = Ghi sổ ngay (cộng tồn kho
    // ngay khi tạo, xem comment 2026-09-11 trong chính use case đó). Bản sao lại cần LUÔN ở Treo,
    // không tự kèm phiếu nhập kho (theo yêu cầu). Vì vậy gọi thêm IUnconfirmSalesReturnUseCase ngay
    // sau đó để trả chứng từ mới về Held + hoàn tác đúng số tồn kho vừa cộng — net stock impact = 0,
    // tái dùng nguyên logic hoàn tác đã có (đã validate đủ tồn) thay vì tự viết lại.
    public async Task<SalesReturnResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _getById.ExecuteAsync(id, ct)
            ?? throw new NotFoundException($"SalesReturn with id {id} not found.");

        var documentNumber = await _getNextCode.ExecuteAsync(ct);
        var today = DateTime.UtcNow.Date;

        var request = new CreateSalesReturnRequestDto
        {
            DocumentNumber = documentNumber,
            AccountingDate = today,
            DocumentDate   = today,
            CustomerId     = source.CustomerId,
            EmployeeId     = source.EmployeeId,
            Description    = source.Description,
            Reference      = source.Reference,
            ReturnType     = source.ReturnType,
            Lines          = source.Lines.Select(l => new SalesReturnLineDto
            {
                ProductId        = l.ProductId,
                WarehouseId      = l.WarehouseId,
                ReturnAccount    = l.ReturnAccount,
                DebtAccount      = l.DebtAccount,
                DiscountAccount  = l.DiscountAccount,
                Unit             = l.Unit,
                Quantity         = l.Quantity,
                UnitPrice        = l.UnitPrice,
                DiscountRate     = l.DiscountRate,
                SalesOrderNumber = l.SalesOrderNumber,
                TaxAccount       = l.TaxAccount,
                CostAccount      = l.CostAccount,
                CogsAccount      = l.CogsAccount,
                DepartmentId     = l.DepartmentId,
            }).ToList(),
        };

        var created = await _createReturn.ExecuteAsync(request, ct);
        var held    = await _unconfirmReturn.ExecuteAsync(created.Id, ct);

        _logger.LogInformation(
            "Duplicated SalesReturn {SourceId} → {NewId} ({DocumentNumber}), reverted to Held",
            id, held.Id, held.DocumentNumber);

        return held;
    }
}
