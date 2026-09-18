namespace Lamour.Domain.Entities;

public class Customer
{
    public int    Id                 { get; set; }
    public string Code               { get; set; } = string.Empty;
    public string Name                { get; set; } = string.Empty;
    public string Address             { get; set; } = string.Empty;
    public string Province            { get; set; } = string.Empty;
    // 2026-09-18: Quận/Huyện, Xã/Phường — tách riêng khỏi Address (chuỗi tự do) để khớp báo cáo
    // "Tổng hợp bán hàng theo Khách hàng" (MISA có 3 cột địa chỉ riêng: Tỉnh/Thành phố, Quận/Huyện,
    // Xã/Phường). Cả 2 optional — dữ liệu khách hàng cũ không có giá trị này.
    public string District            { get; set; } = string.Empty;
    public string Ward                { get; set; } = string.Empty;
    public string CustomerGroup       { get; set; } = string.Empty;
    public string TaxCode             { get; set; } = string.Empty;
    public string Phone               { get; set; } = string.Empty;
    public int?   SaleCareEmployeeId  { get; set; }
    public Employee? SaleCareEmployee { get; set; }
}
