using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Departments.Dtos;

public class UpdateDepartmentRequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
