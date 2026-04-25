namespace Lamour.Domain.Entities;

public class Supplier
{
    public int    Id             { get; set; }
    public string Code           { get; set; } = string.Empty;
    public string Name           { get; set; } = string.Empty;
    public string Address        { get; set; } = string.Empty;
    public string Group          { get; set; } = string.Empty;
    public string TaxCode        { get; set; } = string.Empty;
    public string Phone          { get; set; } = string.Empty;
    public bool   IsStopTracking { get; set; }
}
