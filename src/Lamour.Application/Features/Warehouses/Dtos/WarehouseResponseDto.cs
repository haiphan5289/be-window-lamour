using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Warehouses.Dtos;

public class WarehouseResponseDto
{
    [JsonPropertyName("id")]        public int    Id       { get; set; }
    [JsonPropertyName("code")]      public string Code     { get; set; } = string.Empty;
    [JsonPropertyName("name")]      public string Name     { get; set; } = string.Empty;
    [JsonPropertyName("is_active")] public bool   IsActive { get; set; }
}
