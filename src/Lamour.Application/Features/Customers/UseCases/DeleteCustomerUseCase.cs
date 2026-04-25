using Lamour.Application.Features.Customers.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Customers.UseCases;

public class DeleteCustomerUseCase : IDeleteCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly ILogger<DeleteCustomerUseCase> _logger;

    public DeleteCustomerUseCase(ICustomerRepository repo, ILogger<DeleteCustomerUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var customer = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Customer {id} not found.");

        await _repo.DeleteAsync(customer, ct);
        _logger.LogInformation("Deleted customer {Id}", id);
    }
}
