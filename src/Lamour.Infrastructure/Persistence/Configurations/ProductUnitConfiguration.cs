using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
{
    public void Configure(EntityTypeBuilder<ProductUnit> builder)
    {
        builder.ToTable("product_units");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

        builder.HasIndex(u => u.Name).IsUnique();

        builder.HasData(
            new ProductUnit { Id = 1,  Name = "Cái" },
            new ProductUnit { Id = 2,  Name = "Hộp" },
            new ProductUnit { Id = 3,  Name = "Chai" },
            new ProductUnit { Id = 4,  Name = "Tuýp" },
            new ProductUnit { Id = 5,  Name = "Cuốn" },
            new ProductUnit { Id = 6,  Name = "Bộ" },
            new ProductUnit { Id = 7,  Name = "Set" },
            new ProductUnit { Id = 8,  Name = "Thùng" },
            new ProductUnit { Id = 9,  Name = "Gói" },
            new ProductUnit { Id = 10, Name = "Lọ" }
        );
    }
}
