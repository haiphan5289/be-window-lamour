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

    // 2026-09-28: riêng cho Phiếu thu tiền mặt khách hàng HÀNG LOẠT (CreateBulkCustomerReceiptUseCase)
    // — không cho user chọn tay ở popup Phiếu thu thường (ReceiptViewModel.PaymentReasons không có
    // giá trị này), chỉ BE tự gán khi tạo phiếu hàng loạt. Theo yêu cầu: cột "Lý do thu/chi" trên màn
    // Quỹ phải hiện "Phiếu thu tiền mặt khách hàng hàng loạt", không phải "Thu công nợ" — dù entries
    // vẫn hạch toán TK 131 (Receivable) giống thu công nợ thật.
    ThuKhachHangHangLoat,

    // Payment reasons (Phiếu Chi)
    ChiKhac,      // Other payment
    ChiMuaHang,   // Purchase payment
    ChiTraNo,     // Debt payment
    ChiLuong,     // Salary payment

    // 2026-09-29: 3 lý do còn lại trong ô "Lý do chi" của MISA (Tạm ứng cho nhân viên · Gửi tiền vào
    // ngân hàng · Chi khác · Thuế TNDN tạm tính). Thêm CUỐI enum; DB lưu dạng chuỗi
    // (HasConversion<string>) nên không cần migration. ChiMuaHang/ChiTraNo/ChiLuong giữ lại để phiếu cũ
    // vẫn đọc được, nhưng WPF không cho chọn mới nữa.
    TamUngNhanVien,
    GuiTienNganHang,
    ThueTNDNTamTinh,

    // 2026-10-01: ô "Lý do nộp" của Phiếu thu theo MISA (Rút tiền gửi về nộp quỹ · Thu hoàn thuế GTGT ·
    // Thu hoàn ứng · Thu khác). ThuTienHang/ThuCongNo giữ lại để phiếu cũ vẫn đọc được, WPF không cho
    // chọn mới nữa.
    RutTienGuiVeNopQuy,
    ThuHoanThueGTGT,
    ThuHoanUng
}
