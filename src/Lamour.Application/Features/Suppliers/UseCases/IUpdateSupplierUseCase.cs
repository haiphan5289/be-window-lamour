using Lamour.Application.Features.Suppliers.Dtos;

namespace Lamour.Application.Features.Suppliers.UseCases;

public interface IUpdateSupplierUseCase
{
    Task<SupplierResponseDto> ExecuteAsync(int id, UpdateSupplierRequestDto request, CancellationToken ct = default);
}
