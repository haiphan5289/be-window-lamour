using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Backups.Dtos;

public class UpdateBackupScheduleRequestDto
{
    [JsonPropertyName("is_enabled")]     public bool   IsEnabled     { get; set; }
    [JsonPropertyName("time_of_day")]    public string TimeOfDay     { get; set; } = "02:00";
    [JsonPropertyName("interval_days")]  public int    IntervalDays  { get; set; } = 1;
    [JsonPropertyName("retention_days")] public int    RetentionDays { get; set; }
    [JsonPropertyName("directory")]      public string Directory     { get; set; } = string.Empty;
}
