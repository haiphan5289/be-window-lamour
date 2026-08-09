using System.Text.Json.Serialization;

namespace Lamour.Application.Features.AccountSettings.Dtos;

public class AccountSettingResponseDto
{
    [JsonPropertyName("id")]          public int    Id          { get; set; }
    [JsonPropertyName("code")]        public string Code        { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
}
