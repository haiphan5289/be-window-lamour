using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Categories.Dtos;

public class UpdateCategoryRequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
