using Lamour.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(10).IsRequired().HasDefaultValue("");
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(10).IsRequired().HasDefaultValue("Nam");
        builder.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired().HasDefaultValue("");
        builder.Property(e => e.Role).HasColumnName("role")
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(30).IsRequired();
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(e => e.JobTitle).HasColumnName("job_title")
            .HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.BankAccountNumber).HasColumnName("bank_account_number").HasMaxLength(30);
        builder.Property(e => e.BankName).HasColumnName("bank_name").HasMaxLength(100);
        builder.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
    }
}
