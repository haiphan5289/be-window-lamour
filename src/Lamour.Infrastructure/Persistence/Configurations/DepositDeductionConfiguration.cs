using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class DepositDeductionConfiguration : IEntityTypeConfiguration<DepositDeduction>
{
    public void Configure(EntityTypeBuilder<DepositDeduction> builder)
    {
        builder.ToTable("deposit_deductions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DocumentNumber).HasColumnName("document_number").HasMaxLength(50).IsRequired();
        builder.Property(x => x.DepositId).HasColumnName("deposit_id");
        builder.Property(x => x.SalesOrderId).HasColumnName("sales_order_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.AccountingDate).HasColumnName("accounting_date").IsRequired();
        builder.Property(x => x.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.Deposit)
               .WithMany(x => x.Deductions)
               .HasForeignKey(x => x.DepositId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalesOrder)
               .WithMany()
               .HasForeignKey(x => x.SalesOrderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AccountingDate);
        builder.HasIndex(x => x.DocumentNumber).IsUnique();
    }
}
