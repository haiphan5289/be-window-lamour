using System.Text.Json.Serialization;

namespace Lamour.Application.Features.ProductUnits.Dtos;

public class ProductUnitResponseDto
{
    [JsonPropertyName("id")]   public int    Id   { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
