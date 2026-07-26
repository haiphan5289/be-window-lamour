using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Backups.Dtos;

public class RestoreBackupRequestDto
{
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
}
