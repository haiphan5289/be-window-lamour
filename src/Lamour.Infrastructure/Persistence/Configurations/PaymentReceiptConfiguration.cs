using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
{
    public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
    {
        builder.ToTable("payment_receipts");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(r => r.ReceiptNumber).HasColumnName("receipt_number").HasMaxLength(20).IsRequired();
        builder.Property(r => r.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(r => r.EmployeeId).HasColumnName("employee_id");
        builder.Property(r => r.CollectionDate).HasColumnName("collection_date").IsRequired();
        builder.Property(r => r.Description).HasColumnName("description").HasMaxLength(300);
        builder.Property(r => r.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.PaymentMethod).HasColumnName("payment_method")
               .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();
        builder.Property(r => r.ExchangeRate).HasColumnName("exchange_rate").HasPrecision(18, 6).IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(r => r.Customer)
               .WithMany()
               .HasForeignKey(r => r.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Employee)
               .WithMany()
               .HasForeignKey(r => r.EmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.CollectionDate);
    }
}

public class PaymentReceiptLineConfiguration : IEntityTypeConfiguration<PaymentReceiptLine>
{
    public void Configure(EntityTypeBuilder<PaymentReceiptLine> builder)
    {
        builder.ToTable("payment_receipt_lines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(l => l.PaymentReceiptId).HasColumnName("payment_receipt_id").IsRequired();
        builder.Property(l => l.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(l => l.DocumentNumber).HasColumnName("document_number").HasMaxLength(50).IsRequired();
        builder.Property(l => l.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(50).IsRequired();
        builder.Property(l => l.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(l => l.DueDate).HasColumnName("due_date");
        builder.Property(l => l.AmountDue).HasColumnName("amount_due").HasPrecision(18, 2).IsRequired();
        builder.Property(l => l.AmountPaid).HasColumnName("amount_paid").HasPrecision(18, 2).IsRequired();

        builder.HasOne(l => l.PaymentReceipt)
               .WithMany(r => r.Lines)
               .HasForeignKey(l => l.PaymentReceiptId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
