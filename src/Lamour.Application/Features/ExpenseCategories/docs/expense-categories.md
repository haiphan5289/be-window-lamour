# Expense Categories (Khoản mục chi phí) — Feature Document (BE + WPF)

> **Branch:** `dev` | **Generated:** 2026-08-10

---

## PRD Summary

Master data "Khoản mục chi phí" — nguồn ban đầu là 2 ảnh mẫu MISA: (1) grid dropdown "Khoản mục CP" với 8 dòng Mã+Tên (01-08, PHÒNG SALES...KHÁC), (2) popup "Thêm Khoản mục chi phí" với 4 field: Mã (*), Tên (*), Thuộc (dropdown), Diễn giải.

- **Goal:** CRUD API cho Khoản mục chi phí (Code + Name + Department FK optional + Description), màn quản lý riêng trong hub Kho, sau đó (2026-08-10) wire tiếp vào cột "Khoản mục CP" trong lưới hạch toán Phiếu chi — xem [`phieu-chi.md`](../../Accounting/docs/phieu-chi.md).
- **Field "Thuộc"** → FK tới [`Department`](../../Departments/docs/departments.md) (user chọn "master data riêng" khi được hỏi, không phải enum cứng).
- **Không seed data mẫu** — khác Department (có seed 8 phòng ban), ExpenseCategory để trống, user tự tạo qua UI.

---

## Business Rules

| Rule | Description |
|------|-------------|
| Code + Name required | `DomainException` nếu trống |
| Code unique | Case-insensitive — Create: check toàn bộ; Update: exclude chính nó |
| DepartmentId optional | Nullable — nếu có giá trị, validate `Department` tồn tại (`DomainException` "Phòng ban không tồn tại" nếu không) |
| Description optional | Free-text, không validate |
| Không có guard IsInUse riêng | Xoá `ExpenseCategory` không kiểm tra `PaymentEntry.ExpenseCategoryId` ở tầng UseCase — dựa vào FK `OnDelete: SetNull` (Payment không bị chặn, chỉ mất liên kết) |

---

## Architecture Overview

### Key Components (BE)

| Layer | File | Role |
|-------|------|------|
| Entity | `Lamour.Domain/Entities/ExpenseCategory.cs` | `Id`, `Code`, `Name`, `DepartmentId` (nullable), `Department` (nav), `Description` (nullable) |
| Config | `Lamour.Infrastructure/Persistence/Configurations/ExpenseCategoryConfiguration.cs` | Table `expense_categories`, `Code` unique index, FK `department_id → departments.id` (`OnDelete: SetNull`) |
| Repository | `Repositories/IExpenseCategoryRepository.cs` / `Lamour.Infrastructure/Repositories/ExpenseCategoryRepository.cs` | `GetAllAsync` (`.Include(Department)`), `GetByIdAsync`, `CodeExistsAsync`, CRUD |
| UseCase | `UseCases/{Get,Create,Update,Delete}ExpenseCategoryUseCase.cs` | Create/Update inject thêm `IDepartmentRepository` để validate `DepartmentId` |
| Controller | `Lamour.Api/Controllers/ExpenseCategoriesController.cs` | 4 HTTP actions, `[Authorize]` |

### Dùng ở nơi khác (mở rộng 2026-08-10)

`PaymentEntry.ExpenseCategoryId` (nullable FK, `OnDelete: SetNull`) — cột "Khoản mục CP" trong lưới hạch toán Phiếu chi. Xem chi tiết migration/DTO ở [`phieu-chi.md`](../../Accounting/docs/phieu-chi.md).

---

## API Contracts

Base route: `api/v1/expense-categories`

| Method | Endpoint | Input | Output |
|--------|----------|-------|--------|
| `GET` | `/` | — | `ExpenseCategoryResponseDto[]` |
| `POST` | `/` | `CreateExpenseCategoryRequestDto` | `ExpenseCategoryResponseDto` (201) |
| `PUT` | `/{id}` | `UpdateExpenseCategoryRequestDto` | `ExpenseCategoryResponseDto` (200) |
| `DELETE` | `/{id}` | — | 204 No Content |

### Request — Create/Update
```json
{ "code": "05", "name": "PHÒNG NHÂN SỰ", "department_id": 5, "description": null }
```

### Response
```json
{
  "id": 3,
  "code": "05",
  "name": "PHÒNG NHÂN SỰ",
  "department_id": 5,
  "department_name": "PHÒNG NHÂN SỰ",
  "description": null
}
```

---

## EF Migration

`20260810082526_AddDepartmentsAndExpenseCategories` — cùng migration tạo `departments` (xem [`departments.md`](../../Departments/docs/departments.md)). Không seed data cho `expense_categories`.

Migration sau (`20260810094907_AddPaymentStatusAndExpenseCategoryLink`) thêm `payment_entries.expense_category_id` (FK `SetNull` tới bảng này) khi wire vào Phiếu chi.

---

## WPF Client (`desktop-lamour`)

### Module: `Features/HomePage/Warehouses/` (nhúng chung với feature Kho)

| File | Role |
|---|---|
| `Domain/Models/ExpenseCategory.cs` | **Không** implement `ISearchableItem` (khác hầu hết model khác) — chỉ `Id`, `Code`, `Name`, `DepartmentId`, `DepartmentName`, `Description`, `DisplayText => "{Code} — {Name}"` |
| `Data/Services/IExpenseCategoryService.cs` / `ExpenseCategoryService.cs` | HttpClient, gọi API trực tiếp (không cache-first) |
| `Domain/UseCases/*` (4 pairs) | Validate client-side |
| `ViewModels/ExpenseCategoryFormViewModel.cs` + `Views/ExpenseCategoryFormWindow.xaml` | Popup 4 field: Mã(*)/Tên(*)/Thuộc (`AppSearchableComboBox` load `Department`, có nút "+" mở `DepartmentFormWindow` inline)/Diễn giải |
| `ViewModels/ExpenseCategoryListViewModel.cs` + `Views/ExpenseCategoryListView.xaml` | List (4 cột: Mã khoản mục CP/Tên khoản mục CP/Thuộc/Diễn giải) + Thêm/Sửa/Xóa |

### Truy cập từ hub Kho

- `WarehouseView.xaml`: tile "💰 Khoản mục chi phí" trong section "Cài đặt"
- `WarehouseViewModel.cs`: `NavigateToExpenseCategoriesCommand` → `NavigationRoutes.ExpenseCategories.List`

### Wire vào Phiếu chi (2026-08-10)

`PaymentViewModel` inject `IGetExpenseCategoriesUseCase` — cột "Khoản mục CP" trong grid hạch toán là `ComboBox` luôn hiện sẵn trong `CellTemplate` (không `CellEditingTemplate` — xem bug story trong [`phieu-chi.md`](../../Accounting/docs/phieu-chi.md) về vì sao). `PaymentEntryItem.SelectedExpenseCategory` (type `ExpenseCategory?`, KHÔNG phải `int?` — đã đổi từ Id sang bind cả object sau khi debug bug ComboBox).

### Known gaps

- Không có SignalR realtime.
- `ExpenseCategory` model KHÔNG implement `ISearchableItem` — nếu cần dùng lại ở 1 `AppSearchableComboBox` khác (control chuẩn của app cho dropdown tìm kiếm), phải thêm interface này trước.
- Chưa có unit test nào (BE lẫn WPF).
- Cột "Mục thu/chi" trong ảnh mẫu gốc — **chưa làm**, chưa có data model tương ứng (khác hẳn Khoản mục CP), user chọn bỏ qua khi được hỏi.

---

*Generated by `/ct-be-to-desktop` on 2026-08-10*
