namespace Lamour.Domain.Entities;

// 2026-09-18: thêm "Staff" — role mặc định (ẩn hẳn ô chọn trên popup Thêm/Sửa nhân viên theo yêu
// cầu) cho nhân viên tạo mới qua desktop-lamour. Admin/Cashier/Warehouse vẫn tồn tại cho tài khoản
// đã có từ trước hoặc gán tay ở nơi khác (không còn đường nào tạo mới qua UI popup này nữa).
public enum EmployeeRole     { Admin, Cashier, Warehouse, Staff }

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
    public EmployeeRole     Role              { get; set; } = EmployeeRole.Staff;
    public string           Unit              { get; set; } = "Tiệm spa";
    // Chức danh (2026-09-18): chuyển từ enum cứng sang string tự nhập theo yêu cầu — cùng lý do
    // đã áp dụng cho Unit/Gender (2026-08-19): không muốn giới hạn giá trị người dùng có thể gõ.
    // Khác Unit/Gender, JobTitle KHÔNG dùng AllowedValues soft-list — chấp nhận bất kỳ chuỗi nào
    // (chỉ validate không rỗng, xem CreateEmployeeUseCase/UpdateEmployeeUseCase).
    public string           JobTitle          { get; set; } = "Khac";
    public string?          BankAccountNumber { get; set; }
    public string?          BankName          { get; set; }
    public string           PasswordHash      { get; set; } = string.Empty;
    public bool             IsActive          { get; set; } = true;
}
