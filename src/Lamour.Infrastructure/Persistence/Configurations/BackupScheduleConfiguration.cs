using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class BackupScheduleConfiguration : IEntityTypeConfiguration<BackupSchedule>
{
    public void Configure(EntityTypeBuilder<BackupSchedule> builder)
    {
        builder.ToTable("backup_schedule");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(s => s.IsEnabled).HasColumnName("is_enabled").IsRequired();
        builder.Property(s => s.TimeOfDay).HasColumnName("time_of_day").IsRequired();
        builder.Property(s => s.IntervalDays).HasColumnName("interval_days").IsRequired();
        builder.Property(s => s.RetentionDays).HasColumnName("retention_days").IsRequired();
        builder.Property(s => s.Directory).HasColumnName("directory").HasMaxLength(500).IsRequired();
        builder.Property(s => s.LastRunAt).HasColumnName("last_run_at");

        builder.HasData(new BackupSchedule
        {
            Id            = 1,
            IsEnabled     = false,
            TimeOfDay     = new TimeOnly(2, 0),
            IntervalDays  = 1,
            RetentionDays = 30,
            Directory     = "/Users/haiphan/Desktop/haiphan/be-window-lamour/backups",
            LastRunAt     = null,
        });
    }
}
