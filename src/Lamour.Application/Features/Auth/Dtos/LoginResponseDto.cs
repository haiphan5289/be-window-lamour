using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Auth.Dtos;

public class LoginResponseDto
{
    [JsonPropertyName("user_id")]      public int    UserId      { get; set; }
    [JsonPropertyName("phone")]        public string Phone       { get; set; } = string.Empty;
    [JsonPropertyName("name")]         public string Name        { get; set; } = string.Empty;
    [JsonPropertyName("role")]         public string Role        { get; set; } = string.Empty;
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
}
