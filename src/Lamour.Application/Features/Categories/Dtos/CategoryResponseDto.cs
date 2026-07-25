using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Categories.Dtos;

public class CategoryResponseDto
{
    [JsonPropertyName("id")]   public int    Id   { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
