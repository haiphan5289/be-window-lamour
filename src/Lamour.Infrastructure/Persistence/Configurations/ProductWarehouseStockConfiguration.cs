using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class ProductWarehouseStockConfiguration : IEntityTypeConfiguration<ProductWarehouseStock>
{
    public void Configure(EntityTypeBuilder<ProductWarehouseStock> builder)
    {
        builder.ToTable("product_warehouse_stocks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(x => x.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();

        builder.HasOne(x => x.Product)
               .WithMany()
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
               .WithMany()
               .HasForeignKey(x => x.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
