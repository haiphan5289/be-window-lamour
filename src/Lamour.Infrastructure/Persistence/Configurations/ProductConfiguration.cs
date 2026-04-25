using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(p => p.Code).HasColumnName("code").HasMaxLength(50);
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Unit).HasColumnName("unit").HasMaxLength(50);
        builder.Property(p => p.CostPrice).HasColumnName("cost_price").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.SellingPrice).HasColumnName("selling_price").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(p => p.Code).IsUnique().HasFilter("code <> ''");
    }
}
