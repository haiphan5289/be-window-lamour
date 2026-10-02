using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Accounting.UseCases;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lamour.Application.Tests.Features.Accounting.UseCases;

// 2026-10-01: Phiếu thu theo luồng MISA — đối tượng Khách hàng / Nhân viên, lý do nộp chi tiết, TK Nợ/Có
// lấy từ danh mục tài khoản.
public class CreateReceiptUseCaseTests
{
    private readonly Mock<IReceiptRepository>        _repo      = new();
    private readonly Mock<IAccountSettingRepository> _accounts  = new();
    private readonly Mock<ICustomerRepository>       _customers = new();
    private readonly Mock<IEmployeeRepository>       _employees = new();
    private Receipt? _saved;

    private CreateReceiptUseCase CreateSut()
    {
        var all = new List<AccountSetting>
        {
            new() { Id = 1, Code = "1111", Description = "Tiền Việt Nam" },
            new() { Id = 2, Code = "1388", Description = "Phải thu khác" },
            new() { Id = 3, Code = "131",  Description = "Phải thu của khách hàng" },
        };
        _accounts.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(all);
        _accounts.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((int id, CancellationToken _) => all.FirstOrDefault(a => a.Id == id));
        _employees.Setup(r => r.GetByIdAsync(16, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Employee { Id = 16, Name = "TRẦN THỊ NHI TRÚC" });
        _customers.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Customer { Id = 5, Name = "Chị Lan" });
        _repo.Setup(r => r.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Receipt r, CancellationToken _) => { _saved = r; return r; });

        return new CreateReceiptUseCase(_repo.Object, _accounts.Object, _customers.Object, _employees.Object,
            Mock.Of<ILogger<CreateReceiptUseCase>>());
    }

    private static CreateReceiptRequestDto Request(string? partnerType, int? partnerId, string reason = "ThuKhac") => new()
    {
        PartnerType = partnerType, PartnerId = partnerId, PayerName = "x", PaymentReason = reason,
        ReasonDetail = "  nhập quỹ  ", DocumentNumber = "PT00106",
        Entries = { new ReceiptEntryDto { Description = "nhập quỹ", DebitAccountId = 1, CreditAccountId = 2, Amount = 50_000_000m } },
    };

    [Fact]
    public async Task EmployeePartner_IsSaved_WithoutCustomerId_AndNotBulk()
    {
        var result = await CreateSut().ExecuteAsync(Request("Employee", 16));

        Assert.Equal(PaymentPartnerType.Employee, _saved!.PartnerType);
        Assert.Equal(16, _saved.PartnerId);
        Assert.Equal("TRẦN THỊ NHI TRÚC", _saved.PartnerName);
        Assert.Null(_saved.CustomerId);
        Assert.False(_saved.IsBulk);
        Assert.Equal("nhập quỹ", _saved.ReasonDetail);
        Assert.Equal(2, _saved.Entries.Single().CreditAccountSettingId);
        Assert.Equal("Draft", result.Status);
    }

    [Fact]
    public async Task CustomerPartner_AlsoFillsCustomerId()
    {
        await CreateSut().ExecuteAsync(Request("Customer", 5, "ThuHoanUng"));

        Assert.Equal(5, _saved!.CustomerId);
        Assert.Equal(PaymentReason.ThuHoanUng, _saved.PaymentReason);
    }

    [Theory]
    [InlineData(null, null)]          // thiếu đối tượng (chỉ phiếu hàng loạt mới được bỏ trống)
    [InlineData("Supplier", 1)]       // phiếu thu không nhận Nhà cung cấp
    [InlineData("Employee", 999)]     // nhân viên không tồn tại
    public async Task InvalidPartner_IsRejected(string? partnerType, int? partnerId)
    {
        await Assert.ThrowsAsync<DomainException>(() => CreateSut().ExecuteAsync(Request(partnerType, partnerId)));
        _repo.Verify(r => r.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MissingCreditAccount_IsRejected()
    {
        var request = Request("Employee", 16);
        request.Entries[0].CreditAccountId = 0;   // TK Có để trống trên form

        var ex = await Assert.ThrowsAsync<DomainException>(() => CreateSut().ExecuteAsync(request));
        Assert.Contains("tài khoản Có", ex.Message);
    }

    [Fact]
    public async Task BulkReceipt_NoPartner_LegacyAccountNames_AreResolvedFromCatalog()
    {
        var request = Request(null, null, "ThuKhachHangHangLoat");
        request.Entries[0] = new ReceiptEntryDto { Description = "Thu tiền khách hàng", DebitAccount = "Cash111", CreditAccount = "Receivable131", Amount = 100m };

        await CreateSut().ExecuteAsync(request);

        Assert.True(_saved!.IsBulk);
        Assert.Equal(1, _saved.Entries.Single().DebitAccountSettingId);
        Assert.Equal(3, _saved.Entries.Single().CreditAccountSettingId);
    }
}
