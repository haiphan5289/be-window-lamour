using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Auth.Dtos;

public class LoginRequestDto
{
    [JsonPropertyName("phone")]    public string Phone    { get; set; } = string.Empty;
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
}
