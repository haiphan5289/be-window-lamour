using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class CashTransactionConfiguration : IEntityTypeConfiguration<CashTransaction>
{
    public void Configure(EntityTypeBuilder<CashTransaction> builder)
    {
        builder.ToTable("cash_transactions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(c => c.AccountingDate).HasColumnName("accounting_date").IsRequired();
        builder.Property(c => c.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(c => c.ReceiptNumber).HasColumnName("receipt_number").HasMaxLength(20);
        builder.Property(c => c.PaymentNumber).HasColumnName("payment_number").HasMaxLength(20);
        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(c => c.Account).HasColumnName("account").HasMaxLength(10).IsRequired();
        builder.Property(c => c.CounterAccount).HasColumnName("counter_account").HasMaxLength(10).IsRequired();
        builder.Property(c => c.DebitAmount).HasColumnName("debit_amount").HasPrecision(18, 2);
        builder.Property(c => c.CreditAmount).HasColumnName("credit_amount").HasPrecision(18, 2);
        builder.Property(c => c.PersonName).HasColumnName("person_name").HasMaxLength(200);
        builder.Property(c => c.PaymentReason).HasColumnName("payment_reason").HasMaxLength(30);
        builder.Property(c => c.DocumentType).HasColumnName("document_type").HasMaxLength(100).IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(c => c.AccountingDate);

        // ── Seed Data ─────────────────────────────────────────────────────────
        var date = new DateTime(2023, 11, 2, 0, 0, 0, DateTimeKind.Utc);

        // Phiếu Thu PT00678 — 5 records (CounterAccount="131", PersonName="Thanh Đức")
        builder.HasData(
            new CashTransaction
            {
                Id = 1, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = "PT00678", PaymentNumber = null,
                Description = "Thu tiền khách hàng", Account = "111",
                CounterAccount = "131", DebitAmount = 520000m, CreditAmount = 0m,
                PersonName = "Thanh Đức", CreatedAt = date, PaymentReason = "ThuTienHang", DocumentType = "Phiếu thu tiền mặt khách hàng"
            },
            new CashTransaction
            {
                Id = 2, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = "PT00678", PaymentNumber = null,
                Description = "Thu tiền khách hàng", Account = "111",
                CounterAccount = "131", DebitAmount = 5850000m, CreditAmount = 0m,
                PersonName = "Thanh Đức", CreatedAt = date, PaymentReason = "ThuTienHang", DocumentType = "Phiếu thu tiền mặt khách hàng"
            },
            new CashTransaction
            {
                Id = 3, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = "PT00678", PaymentNumber = null,
                Description = "Thu tiền khách hàng", Account = "111",
                CounterAccount = "131", DebitAmount = 5025000m, CreditAmount = 0m,
                PersonName = "Thanh Đức", CreatedAt = date, PaymentReason = "ThuTienHang", DocumentType = "Phiếu thu tiền mặt khách hàng"
            },
            new CashTransaction
            {
                Id = 4, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = "PT00678", PaymentNumber = null,
                Description = "Thu tiền khách hàng", Account = "111",
                CounterAccount = "131", DebitAmount = 5640000m, CreditAmount = 0m,
                PersonName = "Thanh Đức", CreatedAt = date, PaymentReason = "ThuTienHang", DocumentType = "Phiếu thu tiền mặt khách hàng"
            },
            new CashTransaction
            {
                Id = 5, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = "PT00678", PaymentNumber = null,
                Description = "Thu tiền khách hàng", Account = "111",
                CounterAccount = "131", DebitAmount = 5200000m, CreditAmount = 0m,
                PersonName = "Thanh Đức", CreatedAt = date, PaymentReason = "ThuTienHang", DocumentType = "Phiếu thu tiền mặt khách hàng"
            },

            // Phiếu Chi PC02215 — 5 records (CounterAccount="6418", PersonName="LÊ HOÀNG THANH ĐỨC")
            new CashTransaction
            {
                Id = 6, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02215",
                Description = "Diễm", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 615000m,
                PersonName = "LÊ HOÀNG THANH ĐỨC", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            },
            new CashTransaction
            {
                Id = 7, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02215",
                Description = "Thảo Uyên", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 1055000m,
                PersonName = "LÊ HOÀNG THANH ĐỨC", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            },
            new CashTransaction
            {
                Id = 8, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02215",
                Description = "Phúc Nhi", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 1228000m,
                PersonName = "LÊ HOÀNG THANH ĐỨC", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            },
            new CashTransaction
            {
                Id = 9, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02215",
                Description = "Hân", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 174000m,
                PersonName = "LÊ HOÀNG THANH ĐỨC", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            },
            new CashTransaction
            {
                Id = 10, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02215",
                Description = "Hương Ly", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 105000m,
                PersonName = "LÊ HOÀNG THANH ĐỨC", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            },

            // More Phiếu Chi
            new CashTransaction
            {
                Id = 11, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02216",
                Description = "Mua like fanpage tháng 10/2023", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 450000m,
                PersonName = "NGUYỄN HÀ THANH HÀ", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            },
            new CashTransaction
            {
                Id = 12, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02217",
                Description = "Phí lưu kho t10/2023", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 1715000m,
                PersonName = "LÊ HOÀNG THANH ĐỨC", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            },
            new CashTransaction
            {
                Id = 13, AccountingDate = date, DocumentDate = date,
                ReceiptNumber = null, PaymentNumber = "PC02218",
                Description = "Thuê VP t11/2023", Account = "111",
                CounterAccount = "6418", DebitAmount = 0m, CreditAmount = 40000000m,
                PersonName = "LÊ HOÀNG THANH ĐỨC", CreatedAt = date, PaymentReason = "ChiKhac", DocumentType = "Phiếu chi"
            }
        );
    }
}
