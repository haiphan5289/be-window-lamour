using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
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
        builder.Property(p => p.CategoryId).HasColumnName("category_id").IsRequired();
        builder.HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(p => p.Unit).HasColumnName("unit").HasMaxLength(50);
        builder.Property(p => p.CostPrice).HasColumnName("cost_price").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.SellingPrice).HasColumnName("selling_price").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.Property(p => p.VatRate).HasColumnName("vat_rate").HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.TaxReductionType).HasColumnName("tax_reduction_type").HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ImportTaxRate).HasColumnName("import_tax_rate").HasPrecision(18, 2);
        builder.Property(p => p.ExportTaxRate).HasColumnName("export_tax_rate").HasPrecision(18, 2);
        builder.Property(p => p.ExciseTaxGroup).HasColumnName("excise_tax_group").HasMaxLength(100);

        builder.HasIndex(p => p.Code).IsUnique().HasFilter("code <> ''");
    }
}
