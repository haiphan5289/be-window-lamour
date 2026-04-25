using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(c => c.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(c => c.Province).HasColumnName("province").HasMaxLength(100);
        builder.Property(c => c.CustomerGroup).HasColumnName("customer_group").HasMaxLength(100);
        builder.Property(c => c.TaxCode).HasColumnName("tax_code").HasMaxLength(20);
        builder.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(20);

        builder.HasIndex(c => c.Code).IsUnique();
    }
}
