using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Departments.Dtos;

public class DepartmentResponseDto
{
    [JsonPropertyName("id")]   public int    Id   { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
