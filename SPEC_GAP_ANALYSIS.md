# So sánh Spec (PDF) vs BE + WPF — Gap Analysis

> **Nguồn spec:** `/Users/haiphan/Desktop/App_Window/App Quản lý kho - La'mour.pdf` (10 trang, 15 nhóm chức năng)
> **BE:** `be-window-lamour` (Clean Architecture .NET)
> **WPF:** `desktop-lamour`
> **Ngày review:** 2026-08-04

Đối chiếu 15 nhóm chức năng trong PDF với những gì đã implement ở BE và WPF. Mục đích: xác định phần còn thiếu (cần làm) và phần đã làm dư (không nằm trong spec) để lên roadmap tiếp theo.

---

## 1. Chưa có ở cả BE và WPF (gap ưu tiên cao)

| # PDF | Chức năng | Tình trạng |
|---|---|---|
| 4 | **"Hệ thống tài khoản"** (danh mục TK cố định, import Excel) | Không tồn tại. TK nợ/TK có hiện là enum cứng 4 giá trị (`Cash111/Bank112/Receivable131/Payroll334` — `Lamour.Domain/Enums/AccountCode.cs`), không phải bảng dữ liệu quản lý/import được |
| 7 | **Phiếu xuất kho** (module riêng: người nhận, địa chỉ, lý do xuất) | BE: không có entity/controller `ExportInvoice` (`export_qty`/`export_value` hardcode = 0 trong `InventorySummaryItemDto`). WPF: tile "Phiếu Xuất Kho" trong `WarehouseView.xaml` chỉ là placeholder disable, không có ViewModel/UseCase |
| 15 | **Màn hình cấu hình quyền/tính năng theo từng nhân viên** | Không có ở đâu — chỉ có enum `EmployeeRole` (Admin/Cashier/Warehouse) đơn giản, không có feature-flag per-employee |
| 5 | **Ngày hết hạn sản phẩm** → phân loại theo hạn dùng | Không có field expiry trên `Product` ở cả BE lẫn WPF → không group được theo ngày sắp hết hạn (PDF yêu cầu ở cả phiếu nhập/xuất/kho) |
| 5 | **Chọn kho / chuyển đổi giữa nhiều kho** | BE: `Warehouse` entity tồn tại nhưng inventory API không filter theo kho (`IInventoryRepository` không có param warehouse). WPF: cột "Kho" hardcode text `"Kho chính"` (`WarehouseReceiptFormWindow.xaml:218`), không có control chọn kho |
| 11 | **Chiết khấu 2 chiều (số tiền ↔ %)** trên chứng từ bán hàng | Chỉ có `DiscountRate` (%) trên `SalesOrderLine.cs:61`, không có field số tiền chiết khấu + auto tính ngược lại |
| 13 | **TK nợ/TK có filter theo ký tự gõ (autocomplete)** ở Phiếu thu/chi | `ReceiptViewModel.cs:71-74` chỉ là combo tĩnh 4 giá trị, không filter-as-you-type vì chưa có COA thật để search |

## 2. Có một phần — thiếu field/behavior cụ thể

| # PDF | Chức năng | BE | WPF |
|---|---|---|---|
| 1 | Chứng từ bán hàng | Thiếu endpoint in/PDF; không tự động điền địa chỉ khi chọn KH (chỉ dùng lúc in) | Không hiển thị/autofill địa chỉ trên form; không có nút "in biên lai" thủ công (chỉ auto-print sau save trong `SaveAsync`); cột Số lượng không nằm ngay sau cột Khuyến mãi (3 cột TK/ĐVT chen giữa); `DatePicker` trên form không set `dd/MM/yyyy` |
| 1 | Nút "+" tạo KH mới khi không tìm thấy MSKH/SĐT | N/A | Nút "+" (`AddCustomerCommand`) luôn hiện, không ẩn/hiện có điều kiện theo kết quả tìm kiếm |
| 3 | Vật tư hàng hoá — field TK mặc định + toggle default/list, giá bán đối diện | `Product` entity **không có field TK** nào | Form sản phẩm cũng không có field TK, không có section "Thông tin chung" tách biệt (`ProductFormWindow.xaml`) |
| 5 | Tồn kho đầu kỳ tô đậm | Tính on-the-fly xấp xỉ sai (`ClosingQty - ImportQty`, không trừ export — xem `warehouse.md` Known Limitations #1) | Hiển thị như 1 cột thường (`TongHopTonKhoView.xaml`), không phải hàng tổng riêng in đậm |
| 8 | Danh sách khách hàng | Thiếu field "sđt chi nhánh"; msnv chỉ lưu FK, trả về **tên** NV chứ không phải mã | Thiếu cột "sđt chi nhánh"; cột "msnv" hiển thị tên NV (`SaleCareEmployeeName`) chứ không phải mã |
| 10 | Quỹ tiền mặt — sửa/xoá, mỗi item = 1 dòng riêng | Không có CRUD trực tiếp cho `CashTransaction` (chỉ đọc); 1 phiếu thu/chi nhiều item → gộp thành **1 dòng** (ngược yêu cầu PDF) | Grid `IsReadOnly=True` (`AccountingView.xaml`), không sửa/xoá trực tiếp trên lưới |
| 12 | Phiếu thu/chi — in, sửa | Không có endpoint in | Không có action "in" cho Receipt/Payment (khác Sales Order tự in) |
| 14 | Đăng nhập — phân quyền theo vai trò (VD chỉ Giám đốc in được DS khách hàng) | Chỉ `[Authorize(Roles="Admin")]` cho Backups, không có role "Giám đốc" | Chỉ 1 gate `IsAdmin` ẩn/hiện mục Backup; chưa có tính năng in DS khách hàng nào để giới hạn |
| 14 | Khoá app khi nhân viên nghỉ | Có: `IsActive=false` chặn login (`login-view.md:52`) | Có field `IsActive` nhưng không force-logout/block-relogin ngay lập tức phía client |

## 3. Đã khớp đầy đủ với PDF

- **#2** Danh sách chứng từ bán hàng: read-only, sort mới nhất → cũ nhất (`SalesOrderRepository.GetAllAsync`, `SalesOrderListViewModel.cs:182`)
- **#1** Tab "Thông tin bổ sung" (Ghi chú/PT giao hàng/PT thanh toán), cột khuyến mãi force 0đ (`SalesOrderWindow.xaml:383-644`)
- **#6** Phiếu nhập kho: mã/tên SP, số lượng, tự cập nhật tồn kho sau xác nhận
- **#8/#9** Auto tăng MSKH (`KH{5digits}`), MSKH khoá không sửa được sau khi tạo, hiển thị số lượng KH (`TotalCustomersText`)
- **#10/#11** Danh sách & thêm/sửa nhân viên: đủ field mã/tên/chức danh/vai trò
- **#12** Phiếu thu/chi: đã bỏ đúng 2 field "hạn thanh toán" và "số tiền thu" theo yêu cầu
- **#14** Đăng nhập bằng SĐT+mật khẩu, JWT

---

## 4. Feature làm dư (không nằm trong PDF spec)

### 4.1 Module hoàn toàn dư

| Module | BE | WPF | Ghi chú |
|---|---|---|---|
| **SalesReturn** — Chứng từ hàng bán bị trả lại | Full CRUD, tự hoàn tồn kho, mã `BTL{5digits}`, 2 loại trả (giảm trừ công nợ / trả tiền mặt) | Có UI riêng | PDF không hề nhắc đến "hàng trả lại" ở bất kỳ mục nào trong 15 mục |
| **Suppliers** — Nhà cung cấp | Full CRUD + duplicate | Có UI riêng | Ngay cả mục "Phiếu nhập kho" trong PDF cũng không yêu cầu field nhà cung cấp |
| **Categories** — Danh mục sản phẩm | CRUD | Có UI riêng | PDF không có mục danh mục sản phẩm (khác với "Hệ thống tài khoản" — cái đó PDF yêu cầu nhưng **chưa làm**, xem mục 1) |
| **Backups** — Sao lưu/phục hồi DB | Create/Delete/Restore, giới hạn `Roles="Admin"` | Section "Hệ thống" riêng cho Admin | Hoàn toàn không có trong PDF |
| **Realtime Sync** (SignalR) | DataSyncHub | `RealtimeSyncService` đẩy live update cho Customer/Employee/Product/Supplier/Category cache | Cải tiến kỹ thuật tự thêm, PDF không yêu cầu |

### 4.2 Field/tính năng dư trong module đã có trong PDF

| Ở đâu | Field/tính năng dư | So với PDF |
|---|---|---|
| `Product` (BE) | `VatRate`, `TaxReductionType`, `ImportTaxRate`, `ExportTaxRate`, `ExciseTaxGroup` | Mục "Vật tư hàng hoá" trong PDF không nhắc thuế VAT |
| `Customer` (BE+WPF) | `CustomerGroup` (nhóm KH), `TaxCode` (mã số thuế) | PDF chỉ liệt kê: MSKH, tên, địa chỉ, tỉnh, sđt, msnv, sđt chi nhánh |
| `Employee` (BE+WPF) | `BankAccountNumber`, `BankName`, `Unit`, `JobTitle` (enum 5 giá trị thay vì free-text) | PDF chỉ yêu cầu: MSNV, Tên, Chức danh, Vai trò |
| `SalesOrder` (BE) | `Reference`, payment terms/due-date | Không có trong PDF |
| `WarehouseReceipt` (BE) | Cơ chế **Draft → Confirmed** (workflow trạng thái) | PDF mô tả phiếu nhập kho đơn giản: nhập SP+SL rồi tự cập nhật kho ngay, không có khái niệm nháp/xác nhận |
| Customers (BE) | **Import Excel cho Customers** (`POST /customers/import-excel`) | PDF yêu cầu import Excel cho **"Hệ thống tài khoản"**, không phải cho khách hàng — effort có vẻ bị lệch chỗ (COA thiếu, Customer thì dư) |

**Điểm đáng lưu ý nhất:** PDF ghi rõ "Hệ thống tài khoản: Dữ liệu cố định, import excel" nhưng COA chưa được xây; trong khi tính năng import Excel lại được làm cho Customers (không được yêu cầu). Đáng cân nhắc khi lên roadmap tiếp theo.

---

## 5. Đề xuất ưu tiên (chưa quyết định, chỉ gợi ý)

1. **Hệ thống tài khoản (COA)** — nhiều mục khác phụ thuộc vào nó (TK nợ/có filter, field TK trên Product)
2. **Phiếu xuất kho** — đối xứng với phiếu nhập, đang là module lõi còn thiếu
3. **Ngày hết hạn sản phẩm + đa kho** — ảnh hưởng tới cả nhập/xuất/tồn kho
4. **Phân quyền tính năng theo nhân viên** — phụ thuộc vào việc có role "Giám đốc" trước
