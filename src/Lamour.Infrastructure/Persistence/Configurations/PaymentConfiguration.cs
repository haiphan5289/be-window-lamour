using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PayeeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.PaymentReason).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Attachment).HasMaxLength(500);
        builder.Property(x => x.Reference).HasMaxLength(200);
        builder.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AccountingDate).IsRequired();
        builder.Property(x => x.DocumentDate).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Supplier)
               .WithMany()
               .HasForeignKey(x => x.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentEmployee)
               .WithMany()
               .HasForeignKey(x => x.PaymentEmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Entries)
               .WithOne(x => x.Payment)
               .HasForeignKey(x => x.PaymentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PaymentEntryConfiguration : IEntityTypeConfiguration<PaymentEntry>
{
    public void Configure(EntityTypeBuilder<PaymentEntry> builder)
    {
        builder.ToTable("payment_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DebitAccount).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreditAccount).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.SubjectCode).HasMaxLength(50);
        builder.Property(x => x.SubjectName).HasMaxLength(200);
        builder.Property(x => x.BankAccount).HasMaxLength(100);
    }
}
