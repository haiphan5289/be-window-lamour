using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Employees.Dtos;

public class UpdateEmployeeRequestDto
{
    [JsonPropertyName("name")]      public string  Name     { get; set; } = string.Empty;
    [JsonPropertyName("phone")]     public string  Phone    { get; set; } = string.Empty;
    [JsonPropertyName("role")]      public string  Role     { get; set; } = "Cashier";
    [JsonPropertyName("password")]  public string? Password { get; set; }
    [JsonPropertyName("is_active")] public bool    IsActive { get; set; } = true;
}
