using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(w => w.Code).HasColumnName("code").IsRequired().HasMaxLength(20);
        builder.HasIndex(w => w.Code).IsUnique();
        builder.Property(w => w.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(w => w.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasData(
            new Warehouse { Id = 1, Code = "KHO01", Name = "Kho chính", IsActive = true },
            new Warehouse { Id = 4, Code = "HH",     Name = "Hàng hoá",  IsActive = true },
            new Warehouse { Id = 5, Code = "TB",     Name = "Trưng bày", IsActive = true }
        );
    }
}

public class WarehouseReceiptConfiguration : IEntityTypeConfiguration<WarehouseReceipt>
{
    public void Configure(EntityTypeBuilder<WarehouseReceipt> builder)
    {
        builder.ToTable("warehouse_receipts");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(r => r.ReceiptNumber).HasColumnName("receipt_number").HasMaxLength(25);
        builder.Property(r => r.ReceiptType).HasColumnName("receipt_type").HasConversion<int>().IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(r => r.CustomerId).HasColumnName("customer_id");
        builder.Property(r => r.SupplierId).HasColumnName("supplier_id");
        builder.Property(r => r.EmployeeId).HasColumnName("employee_id");
        builder.Property(r => r.AccountingDate).HasColumnName("accounting_date").IsRequired();
        builder.Property(r => r.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(r => r.Description).HasColumnName("description").HasMaxLength(300);
        builder.Property(r => r.DeliveryPerson).HasColumnName("delivery_person").HasMaxLength(200);
        builder.Property(r => r.Reference).HasColumnName("reference").HasMaxLength(100);
        builder.Property(r => r.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.ConfirmedAt).HasColumnName("confirmed_at");

        builder.HasOne(r => r.Customer)
               .WithMany()
               .HasForeignKey(r => r.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Supplier)
               .WithMany()
               .HasForeignKey(r => r.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Employee)
               .WithMany()
               .HasForeignKey(r => r.EmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.AccountingDate);
        builder.HasIndex(r => r.Status);
    }
}

public class WarehouseReceiptLineConfiguration : IEntityTypeConfiguration<WarehouseReceiptLine>
{
    public void Configure(EntityTypeBuilder<WarehouseReceiptLine> builder)
    {
        builder.ToTable("warehouse_receipt_lines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(l => l.WarehouseReceiptId).HasColumnName("warehouse_receipt_id").IsRequired();
        builder.Property(l => l.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(l => l.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(l => l.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(l => l.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
        builder.Property(l => l.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(l => l.DebitAccount).HasColumnName("debit_account").HasMaxLength(20).IsRequired();
        builder.Property(l => l.CreditAccount).HasColumnName("credit_account").HasMaxLength(20).IsRequired();

        builder.Property(l => l.CostItem).HasColumnName("cost_item").HasMaxLength(100);
        builder.Property(l => l.CostObject).HasColumnName("cost_object").HasMaxLength(100);
        builder.Property(l => l.Project).HasColumnName("project").HasMaxLength(100);
        builder.Property(l => l.PurchaseOrderNumber).HasColumnName("purchase_order_number").HasMaxLength(100);
        builder.Property(l => l.SalesContractNumber).HasColumnName("sales_contract_number").HasMaxLength(100);
        builder.Property(l => l.LoanContractNumber).HasColumnName("loan_contract_number").HasMaxLength(100);
        builder.Property(l => l.StatisticsCode).HasColumnName("statistics_code").HasMaxLength(100);

        builder.HasOne(l => l.WarehouseReceipt)
               .WithMany(r => r.Lines)
               .HasForeignKey(l => l.WarehouseReceiptId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Warehouse)
               .WithMany()
               .HasForeignKey(l => l.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
