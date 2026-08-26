using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.ToTable("sales_returns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DocumentNumber).HasColumnName("document_number").HasMaxLength(50).IsRequired();
        builder.Property(x => x.AccountingDate).HasColumnName("accounting_date").IsRequired();
        builder.Property(x => x.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(200);
        builder.Property(x => x.ReturnType).HasColumnName("return_type").IsRequired().HasDefaultValue(SalesReturnType.ReduceDebt);
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.TotalDiscount).HasColumnName("total_discount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.TotalPayment).HasColumnName("total_payment").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Lines)
               .WithOne(x => x.SalesReturn)
               .HasForeignKey(x => x.SalesReturnId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AccountingDate);
        builder.HasIndex(x => x.DocumentNumber).IsUnique();
    }
}

public class SalesReturnLineConfiguration : IEntityTypeConfiguration<SalesReturnLine>
{
    public void Configure(EntityTypeBuilder<SalesReturnLine> builder)
    {
        builder.ToTable("sales_return_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SalesReturnId).HasColumnName("sales_return_id");
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
        builder.Property(x => x.ProductCode).HasColumnName("product_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ReturnAccount).HasColumnName("return_account").HasMaxLength(20).IsRequired();
        builder.Property(x => x.DebtAccount).HasColumnName("debt_account").HasMaxLength(20).IsRequired();
        builder.Property(x => x.DiscountAccount).HasColumnName("discount_account").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.DiscountRate).HasColumnName("discount_rate").HasPrecision(5, 2).IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.SalesOrderNumber).HasColumnName("sales_order_number").HasMaxLength(50);

        builder.Property(x => x.TaxRate).HasColumnName("tax_rate").HasPrecision(5, 2).IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2).IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.TaxAccount).HasColumnName("tax_account").HasMaxLength(20).IsRequired().HasDefaultValue("33311");

        builder.Property(x => x.CostAccount).HasColumnName("cost_account").HasMaxLength(20).IsRequired().HasDefaultValue("1561");
        builder.Property(x => x.CogsAccount).HasColumnName("cogs_account").HasMaxLength(20).IsRequired().HasDefaultValue("632");
        builder.Property(x => x.CostPrice).HasColumnName("cost_price").HasPrecision(18, 2).IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.CostAmount).HasColumnName("cost_amount").HasPrecision(18, 2).IsRequired().HasDefaultValue(0m);

        builder.Property(x => x.DepartmentId).HasColumnName("department_id");

        builder.HasOne(x => x.Product)
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
               .WithMany()
               .HasForeignKey(x => x.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
               .WithMany()
               .HasForeignKey(x => x.DepartmentId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
