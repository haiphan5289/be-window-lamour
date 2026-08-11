using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(d => d.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

        builder.HasIndex(d => d.Name).IsUnique();

        builder.HasData(
            new Department { Id = 1, Name = "PHÒNG SALES" },
            new Department { Id = 2, Name = "PHÒNG MARKETING" },
            new Department { Id = 3, Name = "PHÒNG KHO VẬN" },
            new Department { Id = 4, Name = "PHÒNG TÀI CHÍNH - KẾ TOÁN" },
            new Department { Id = 5, Name = "PHÒNG NHÂN SỰ" },
            new Department { Id = 6, Name = "PHÒNG ĐÀO TẠO" },
            new Department { Id = 7, Name = "PHÒNG SPA" },
            new Department { Id = 8, Name = "KHÁC" }
        );
    }
}
