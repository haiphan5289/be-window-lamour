using System.Text.Json.Serialization;

namespace Lamour.Application.Features.ExpenseCategories.Dtos;

public class UpdateExpenseCategoryRequestDto
{
    [JsonPropertyName("code")]          public string  Code         { get; set; } = string.Empty;
    [JsonPropertyName("name")]          public string  Name         { get; set; } = string.Empty;
    [JsonPropertyName("department_id")] public int?    DepartmentId { get; set; }
    [JsonPropertyName("description")]   public string? Description  { get; set; }
}
