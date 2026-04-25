namespace Lamour.Domain.Entities;

public enum EmployeeRole { Admin, Cashier, Warehouse }

public class Employee
{
    public int          Id           { get; set; }
    public string       Name         { get; set; } = string.Empty;
    public string       Phone        { get; set; } = string.Empty;
    public EmployeeRole Role         { get; set; } = EmployeeRole.Cashier;
    public string       PasswordHash { get; set; } = string.Empty;
    public bool         IsActive     { get; set; } = true;
}
