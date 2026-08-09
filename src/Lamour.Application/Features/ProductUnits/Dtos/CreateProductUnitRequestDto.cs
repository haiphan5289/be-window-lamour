using System.Text.Json.Serialization;

namespace Lamour.Application.Features.ProductUnits.Dtos;

public class CreateProductUnitRequestDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
