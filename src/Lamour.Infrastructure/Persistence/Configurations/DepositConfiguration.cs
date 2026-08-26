using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class DepositConfiguration : IEntityTypeConfiguration<Deposit>
{
    public void Configure(EntityTypeBuilder<Deposit> builder)
    {
        builder.ToTable("deposits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DocumentNumber).HasColumnName("document_number").HasMaxLength(50).IsRequired();
        builder.Property(x => x.AccountingDate).HasColumnName("accounting_date").IsRequired();
        builder.Property(x => x.DocumentDate).HasColumnName("document_date").IsRequired();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(200);
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.RemainingBalance).HasColumnName("remaining_balance").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired().HasDefaultValue(DepositStatus.Active);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.SourceSalesOrderId).HasColumnName("source_sales_order_id");
        builder.HasOne(x => x.SourceSalesOrder)
               .WithMany()
               .HasForeignKey(x => x.SourceSalesOrderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AccountingDate);
        builder.HasIndex(x => x.DocumentNumber).IsUnique();
        builder.HasIndex(x => x.SourceSalesOrderId);

        // Concurrency token dùng cột hệ thống `xmin` của PostgreSQL — chặn lost-update khi
        // 2 request trừ cọc (CreateDepositDeductionUseCase) chạy đồng thời trên cùng 1 Deposit.
        // Không phải cột thật do BE tạo ra — Postgres tự sinh/cập nhật `xmin` trên mỗi row,
        // EF chỉ đọc lại và thêm vào WHERE của UPDATE để phát hiện ghi đè.
        builder.Property<uint>("xmin")
               .HasColumnName("xmin")
               .HasColumnType("xid")
               .ValueGeneratedOnAddOrUpdate()
               .IsConcurrencyToken();
    }
}
