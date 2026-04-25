using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(s => s.Group).HasColumnName("group").HasMaxLength(100);
        builder.Property(s => s.TaxCode).HasColumnName("tax_code").HasMaxLength(20);
        builder.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(s => s.IsStopTracking).HasColumnName("is_stop_tracking").HasDefaultValue(false);

        builder.HasIndex(s => s.Code).IsUnique();
    }
}
