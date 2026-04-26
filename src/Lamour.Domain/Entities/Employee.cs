namespace Lamour.Domain.Entities;

public enum EmployeeRole     { Admin, Cashier, Warehouse }
public enum EmployeeUnit     { PGD, PKD, Spa, GD, Kho }
public enum EmployeeJobTitle { Admin, TruongPhong, NhanVienBanHang, NhanVienKho, ThuNgan, Khac }

public class Employee
{
    public int              Id                { get; set; }
    public string           Code              { get; set; } = string.Empty;  // NV00001
    public string           Name              { get; set; } = string.Empty;
    public string           Phone             { get; set; } = string.Empty;
    public EmployeeRole     Role              { get; set; } = EmployeeRole.Cashier;
    public EmployeeUnit     Unit              { get; set; } = EmployeeUnit.Spa;
    public EmployeeJobTitle JobTitle          { get; set; } = EmployeeJobTitle.Khac;
    public string?          BankAccountNumber { get; set; }
    public string?          BankName          { get; set; }
    public string           PasswordHash      { get; set; } = string.Empty;
    public bool             IsActive          { get; set; } = true;
}
