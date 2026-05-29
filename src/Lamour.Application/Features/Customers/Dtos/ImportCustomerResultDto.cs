using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Customers.Dtos;

public class ImportCustomerResultDto
{
    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("imported")]
    public int Imported { get; init; }

    [JsonPropertyName("skipped")]
    public int Skipped { get; init; }

    [JsonPropertyName("errors")]
    public IReadOnlyList<ImportRowErrorDto> Errors { get; init; } = [];
}

public class ImportRowErrorDto
{
    [JsonPropertyName("row")]
    public int Row { get; init; }

    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;
}
