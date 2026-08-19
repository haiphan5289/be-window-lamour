namespace Lamour.Domain.Entities;

public enum EmployeeRole     { Admin, Cashier, Warehouse }
public enum EmployeeJobTitle { Admin, TruongPhong, NhanVienBanHang, NhanVienKho, ThuNgan, Khac }

// Đơn vị (2026-08-19): chuyển từ enum cứng (PGD/PKD/Spa/GD/Kho) sang string tự do — enum member
// không thể chứa dấu tiếng Việt/khoảng trắng ("Kho và Quỹ", "Phòng Kinh Doanh"...), nên validate
// bằng allowed-list thay vì Enum.TryParse. Xem CreateEmployeeUseCase/UpdateEmployeeUseCase.
public static class EmployeeUnits
{
    public static readonly string[] AllowedValues =
    {
        "Kho và Quỹ",
        "Marketting",
        "Phòng Đào Tạo",
        "Phòng Giám Đốc",
        "Phòng Kinh Doanh",
        "Phòng Nhân Sự",
        "Tiệm spa",
    };
}

// Giới tính (2026-08-19) — cùng lý do trên, string tự do thay vì enum.
public static class EmployeeGenders
{
    public static readonly string[] AllowedValues = { "Nam", "Nữ" };
}

public class Employee
{
    public int              Id                { get; set; }
    public string           Code              { get; set; } = string.Empty;  // NV00001
    public string           Name              { get; set; } = string.Empty;
    public string           Gender            { get; set; } = "Nam";
    public string           Phone             { get; set; } = string.Empty;
    public EmployeeRole     Role              { get; set; } = EmployeeRole.Cashier;
    public string           Unit              { get; set; } = "Tiệm spa";
    public EmployeeJobTitle JobTitle          { get; set; } = EmployeeJobTitle.Khac;
    public string?          BankAccountNumber { get; set; }
    public string?          BankName          { get; set; }
    public string           PasswordHash      { get; set; } = string.Empty;
    public bool             IsActive          { get; set; } = true;
}
