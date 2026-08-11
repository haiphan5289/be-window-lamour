namespace Lamour.Domain.Entities;

public class ExpenseCategory
{
    public int     Id           { get; set; }
    public string  Code         { get; set; } = string.Empty;
    public string  Name         { get; set; } = string.Empty;
    public int?    DepartmentId { get; set; }
    public string? Description  { get; set; }

    public Department? Department { get; set; }
}
