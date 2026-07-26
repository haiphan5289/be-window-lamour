using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Backups.Dtos;

public class BackupResponseDto
{
    [JsonPropertyName("file_name")]  public string   FileName  { get; set; } = string.Empty;
    [JsonPropertyName("size_bytes")] public long     SizeBytes { get; set; }
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
}
