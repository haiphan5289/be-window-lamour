namespace Lamour.Domain.Entities;

public class Warehouse
{
    public int    Id       { get; set; }
    public string Code     { get; set; } = string.Empty;
    public string Name     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
}
