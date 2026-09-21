using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class AppStateConfiguration : IEntityTypeConfiguration<AppState>
{
    public void Configure(EntityTypeBuilder<AppState> builder)
    {
        builder.ToTable("app_states");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasColumnName("key").HasMaxLength(100);
        builder.Property(x => x.Value).HasColumnName("value").HasMaxLength(500).IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
