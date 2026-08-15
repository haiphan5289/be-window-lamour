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
        builder.Property(p => p.CategoryId).HasColumnName("category_id");
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

        // Header fields — "Sửa Vật tư, hàng hoá, dịch vụ" popup (2026-08-09)
        builder.Property(p => p.Nature).HasColumnName("nature").HasConversion<string>().HasMaxLength(20).HasDefaultValue(Domain.Enums.ProductNature.VatTuHangHoa);
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.ProductUnitId).HasColumnName("product_unit_id");
        builder.HasOne(p => p.ProductUnit).WithMany().HasForeignKey(p => p.ProductUnitId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(p => p.WarrantyPeriod).HasColumnName("warranty_period").HasMaxLength(100);
        builder.Property(p => p.MinStockQuantity).HasColumnName("min_stock_quantity").HasDefaultValue(0);
        builder.Property(p => p.Origin).HasColumnName("origin").HasMaxLength(200);
        builder.Property(p => p.PurchaseDescription).HasColumnName("purchase_description");
        builder.Property(p => p.SaleDescription).HasColumnName("sale_description");

        // Tab "Ngầm định"
        builder.Property(p => p.DefaultWarehouseId).HasColumnName("default_warehouse_id");
        builder.HasOne(p => p.DefaultWarehouse).WithMany().HasForeignKey(p => p.DefaultWarehouseId).OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.StockAccountId).HasColumnName("stock_account_id");
        builder.HasOne(p => p.StockAccount).WithMany().HasForeignKey(p => p.StockAccountId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(p => p.RevenueAccountId).HasColumnName("revenue_account_id");
        builder.HasOne(p => p.RevenueAccount).WithMany().HasForeignKey(p => p.RevenueAccountId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(p => p.DiscountAccountId).HasColumnName("discount_account_id");
        builder.HasOne(p => p.DiscountAccount).WithMany().HasForeignKey(p => p.DiscountAccountId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(p => p.PriceReductionAccountId).HasColumnName("price_reduction_account_id");
        builder.HasOne(p => p.PriceReductionAccount).WithMany().HasForeignKey(p => p.PriceReductionAccountId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(p => p.ReturnAccountId).HasColumnName("return_account_id");
        builder.HasOne(p => p.ReturnAccount).WithMany().HasForeignKey(p => p.ReturnAccountId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(p => p.CostAccountId).HasColumnName("cost_account_id");
        builder.HasOne(p => p.CostAccount).WithMany().HasForeignKey(p => p.CostAccountId).OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.TradeDiscountRate).HasColumnName("trade_discount_rate").HasPrecision(9, 2).HasDefaultValue(0);
        builder.Property(p => p.SpecialGoodsType).HasColumnName("special_goods_type").HasMaxLength(100);
        builder.Property(p => p.LatestPurchasePrice).HasColumnName("latest_purchase_price").HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(p => p.IsPromotionalGood).HasColumnName("is_promotional_good").HasDefaultValue(false);
        builder.Property(p => p.IsDepositProduct).HasColumnName("is_deposit_product").HasDefaultValue(false);
    }
}
