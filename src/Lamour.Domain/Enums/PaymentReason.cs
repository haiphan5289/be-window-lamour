namespace Lamour.Domain.Enums;

/// <summary>
/// Payment/Receipt reasons for cash transactions
/// Thu* = Receipt (money in), Chi* = Payment (money out)
/// </summary>
public enum PaymentReason
{
    // Receipt reasons (Phiếu Thu)
    ThuKhac,      // Other receipt
    ThuTienHang,  // Sales receipt
    ThuCongNo,    // Debt collection
    
    // Payment reasons (Phiếu Chi)
    ChiKhac,      // Other payment
    ChiMuaHang,   // Purchase payment
    ChiTraNo,     // Debt payment
    ChiLuong      // Salary payment
}
