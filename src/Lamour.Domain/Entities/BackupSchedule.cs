namespace Lamour.Domain.Entities;

public class BackupSchedule
{
    public int       Id            { get; set; }
    public bool      IsEnabled     { get; set; }
    public TimeOnly  TimeOfDay     { get; set; } = new(2, 0);
    public int       IntervalDays  { get; set; } = 1;
    public int       RetentionDays { get; set; } = 30;
    public string    Directory     { get; set; } = string.Empty;
    public DateTime? LastRunAt     { get; set; }
}
