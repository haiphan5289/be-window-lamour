using System.Text.Json.Serialization;

namespace Lamour.Application.Features.AccountSettings.Dtos;

public class UpdateAccountSettingRequestDto
{
    [JsonPropertyName("code")]        public string Code        { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
}
