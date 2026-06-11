using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DocumentNumber).HasColumnName("document_number").HasMaxLength(50).IsRequired();
        builder.Property(x => x.AccountingDate).HasColumnName("accounting_date").IsRequired();
        builder.Property(x => x.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(200);
        builder.Property(x => x.PaymentTerms).HasColumnName("payment_terms").HasMaxLength(200);
        builder.Property(x => x.PaymentDueDays).HasColumnName("payment_due_days");
        builder.Property(x => x.PaymentDueDate).HasColumnName("payment_due_date");
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(x => x.DeliveryMethod).HasColumnName("delivery_method").HasMaxLength(200);
        builder.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(200);
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired().HasDefaultValue(SalesOrderStatus.Normal);

        builder.HasOne(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Lines)
               .WithOne(x => x.SalesOrder)
               .HasForeignKey(x => x.SalesOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AccountingDate);
        builder.HasIndex(x => x.DocumentNumber).IsUnique();
    }
}

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("sales_order_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SalesOrderId).HasColumnName("sales_order_id");
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsPromotion).HasColumnName("is_promotion");
        builder.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.DiscountRate).HasColumnName("discount_rate").HasPrecision(5, 2).IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ReceivableAccount).HasColumnName("receivable_account").HasMaxLength(20).IsRequired();
        builder.Property(x => x.RevenueAccount).HasColumnName("revenue_account").HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.Product)
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
