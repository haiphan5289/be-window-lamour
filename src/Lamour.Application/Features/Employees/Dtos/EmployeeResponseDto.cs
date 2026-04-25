using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Employees.Dtos;

public class EmployeeResponseDto
{
    [JsonPropertyName("id")]        public int    Id       { get; set; }
    [JsonPropertyName("name")]      public string Name     { get; set; } = string.Empty;
    [JsonPropertyName("phone")]     public string Phone    { get; set; } = string.Empty;
    [JsonPropertyName("role")]      public string Role     { get; set; } = string.Empty;
    [JsonPropertyName("is_active")] public bool   IsActive { get; set; }
}
