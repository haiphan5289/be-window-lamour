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
