# Changelog

## [Unreleased] — 2026-09-18

BE commit: `2aca0d5` "[CS] fix report" (so với `945a816`)
WPF commit: `56fd693` "[CS] fix report" (so với `63c97cb`)

### ✨ Features

**Nhân viên (Employee)**
- "Chức danh" (JobTitle) đổi từ ComboBox cố định (`Admin/TruongPhong/NhanVienBanHang/NhanVienKho/ThuNgan/Khac`) sang **ô nhập text tự do** — không còn giới hạn danh sách dựng sẵn.
  - `EmployeeFormWindow.xaml`, `EmployeeFormViewModel.cs`
- **Ẩn hẳn field "Chức vụ" (Role)** khỏi form Thêm/Sửa nhân viên — mọi nhân viên tạo mới giờ mặc định `Role = "Staff"` (trước đây là `"Cashier"`, chọn tay qua ComboBox `Admin/Cashier/Warehouse`). Ô "Số điện thoại" giãn full-width để lấp khoảng trống.
  - `EmployeeFormWindow.xaml`, `EmployeeFormViewModel.cs`, `Employee.cs`, `Create/UpdateEmployeeRequestDto.cs`
  - BE: default role đổi tương ứng ở `CreateEmployeeUseCase`, `UpdateEmployeeUseCase`, `ImportExcelEmployeesUseCase`

**Khách hàng (Customer) — địa chỉ chi tiết**
- Thêm 2 field mới **Quận/Huyện** (`District`) và **Xã/Phường** (`Ward`), tách riêng khỏi `Address`/`Province` để khớp báo cáo MISA (3 cột địa chỉ).
  - BE: entity `Customer.cs` + `CustomerConfiguration.cs` (`varchar(100)`) + migration `20260918095102_AddCustomerDistrictWard`
  - BE: DTO + UseCase (`Create/Update/GetCustomers/DuplicateCustomer`) map 2 field qua các chiều
  - WPF: `CustomerFormWindow.xaml` thêm 1 hàng nhập Quận/Huyện | Xã/Phường; `CustomerFormViewModel.cs`, `Customer.cs`, `Create/UpdateCustomerInput.cs`, `CustomerRepository.cs`

**Báo cáo bán hàng (SalesOrderReport) — rà soát lại toàn bộ theo MISA**
- Thêm report 1 chiều mới **"Đơn vị kinh doanh"** — group theo `Employee.Unit` (không cần entity mới, tái dùng field có sẵn).
- Thêm report 3 chiều mới **"Nhân viên, khách hàng và mặt hàng"** — gom nhóm theo Nhân viên (ngoài), hiện cột phẳng "Tên khách hàng" (giữa), Mặt hàng (trong). `ReportDisplayRow.cs` thêm `MiddleCode/MiddleName` + `SetMiddleId()`.
- Report "Khách hàng" thêm 3 cột **Tỉnh/Thành phố · Quận/Huyện · Xã/Phường**.
- Sửa lại rule gom nhóm (Expander collapsible): gom nhóm khi **Mặt hàng là dimension ngoài** (áp dụng cho cả "Mặt hàng & Khách hàng" VÀ "Mặt hàng & Nhân viên" — trước đó chỉ áp dụng 1 report, sai).
- Sửa lại rule hiện/ẩn cột Tiền vốn/Lãi gộp/Tỷ lệ lãi gộp: thay ruleset suy diễn (derive từ dimension) bằng **bảng cấu hình tường minh `ColumnConfigs`** — mỗi report type có 1 dòng cấu hình riêng, comment rõ "đã xác nhận qua ảnh MISA" hay "default — chưa có ảnh".
- Màn "Sổ chi tiết bán hàng" khi drill-down từ report **"Nhân viên"** đổi tiêu đề cửa sổ/in thành **"SỔ CHI TIẾT BÁN HÀNG THEO NHÂN VIÊN"** (các biến thể drill khác vẫn dùng tiêu đề chung "SỔ CHI TIẾT BÁN HÀNG").
  - Files: `SalesOrderReportTypes.cs`, `SalesOrderReportViewModel.cs`, `SalesOrderReportView.xaml(.cs)`, `SalesOrderReportDetailViewModel.cs`, `SalesOrderReportDetailView.xaml`, `SalesOrderDetailFilter.cs` (+`SourceReportType`)
  - BE: `SalesOrderSummaryLineDto.cs` + `GetSalesOrderSummaryReportUseCase.cs` thêm field `customer_province/district/ward`, `employee_unit`

> ⚠️ **Phạm vi bị thu hẹp so với dự kiến ban đầu**: biến thể "Sổ chi tiết bán hàng theo Nhân viên" chỉ đổi **tiêu đề**, KHÔNG đổi bộ cột/gom nhóm — ảnh mẫu MISA cho thấy biến thể này dùng bộ cột tổng hợp khác hẳn (Doanh số bán/Chiết khấu/SL trả lại) mà BE hiện chưa có nguồn dữ liệu tương ứng, và ảnh bị cắt nên chưa xác nhận đủ. Cần ảnh đầy đủ hơn hoặc thống nhất lại data source trước khi làm tiếp phần này.
> ⚠️ Cấu hình cột (`ColumnConfigs`) của 2 report `Khách hàng & Nhân viên` và `Khách hàng & Mặt hàng` còn là **default chưa xác nhận qua ảnh** — cần review khi có ảnh mẫu MISA cho 2 report này.

### 🐛 Bug fixes

**Chứng từ bán hàng (SalesOrderListView)**
- Fix màu chữ dòng trạng thái **"Treo" bị mất khi dòng đang SELECTED** (dòng chọn đè màu xanh mặc định của `DataGridCell`, ưu tiên cao hơn `RowStyle` từ ngoài) — thay hẳn `Template` của cell khi `Held + Selected` để giữ đúng màu brand + chữ đậm.
- Bỏ ô tìm kiếm **"🔍 Tìm kiếm chứng từ..."** và nút **"🔍 Lọc"** — các bộ lọc khác (Kỳ/Từ-Đến ngày/Trạng thái + lọc theo cột) đã tự áp dụng ngay (live filter), không cần 2 control này nữa (mirror `SalesReturnListView`).

**In hoá đơn (SalesOrderPrintWindow)**
- Điều chỉnh độ rộng cột bảng in: lấy thêm 8 từ cột "THUẾ SUẤT" (72→64) sang cột "TÊN SẢN PHẨM" (96→104) — thuế suất còn dư chỗ, tên sản phẩm dài bị cắt.

### 📦 Files changed

**be-window-lamour** (24 files) — xem `git show --stat 2aca0d5`
**desktop-lamour** (29 files) — xem `git show --stat 56fd693`

### ✅ Verification

- BE: `dotnet build` — 0 Warning / 0 Error. Migration `AddCustomerDistrictWard` đã `dotnet ef database update` thành công vào DB local.
- WPF: `dotnet build` — 0 Warning / 0 Error.
- ⚠️ Build sạch ≠ đã test UTM. Cần `.\sync.ps1` + chạy thử trên UTM, đặc biệt: report "Đơn vị kinh doanh" mới, report 3 chiều mới, cột địa chỉ report "Khách hàng", form Khách hàng có field mới, report "Mặt hàng & Nhân viên" (gom nhóm — chưa test render thật).
