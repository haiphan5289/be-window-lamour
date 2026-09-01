using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PayerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.PaymentReason).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Attachment).HasMaxLength(500);
        builder.Property(x => x.Reference).HasMaxLength(200);
        builder.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AccountingDate).IsRequired();
        builder.Property(x => x.DocumentDate).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Column-level default is Confirmed (NOT Draft) — this ONLY affects the EF migration's
        // backfill of ALREADY-EXISTING rows. Those rows were created under the OLD behavior,
        // where CashTransaction was posted immediately at Create time, so they must be marked
        // Confirmed (never re-processed by a future Confirm action, which would double-post the
        // cash-ledger entry). All NEW rows going forward have Status explicitly left at its C#
        // property default (`= ReceiptStatus.Draft`) by CreateReceiptUseCase, so this column
        // default never actually applies to new inserts — it only backfills historical data
        // during migration. Do NOT "fix" this to Draft — that would silently mark pre-existing
        // (already CashTransaction-posted) rows as Draft, allowing them to be Confirmed again and
        // double-posting their cash-ledger effect.
        //
        // HasSentinel(-1) is REQUIRED here: ReceiptStatus.Draft == 0 (the CLR default for the
        // enum). Without an explicit sentinel, EF Core treats any property whose value equals the
        // CLR default as "unset" and substitutes the column default (Confirmed) on INSERT instead
        // of sending the actual value — meaning every brand-new Draft row would silently be
        // persisted as Confirmed. Setting the sentinel to an out-of-range value (-1, never a real
        // Status) makes EF treat Draft (0) as a real, explicitly-assigned value that must be sent.
        builder.Property(x => x.Status).HasConversion<int>().IsRequired()
               .HasDefaultValue(ReceiptStatus.Confirmed)
               .HasSentinel((ReceiptStatus)(-1));
        builder.Property(x => x.ConfirmedAt);

        builder.HasOne(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CollectorEmployee)
               .WithMany()
               .HasForeignKey(x => x.CollectorEmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Entries)
               .WithOne(x => x.Receipt)
               .HasForeignKey(x => x.ReceiptId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReceiptEntryConfiguration : IEntityTypeConfiguration<ReceiptEntry>
{
    public void Configure(EntityTypeBuilder<ReceiptEntry> builder)
    {
        builder.ToTable("receipt_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DebitAccount).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreditAccount).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.SubjectCode).HasMaxLength(50);
        builder.Property(x => x.SubjectName).HasMaxLength(200);
        builder.Property(x => x.BankAccount).HasMaxLength(100);

        builder.HasOne(x => x.SalesOrder)
               .WithMany()
               .HasForeignKey(x => x.SalesOrderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
