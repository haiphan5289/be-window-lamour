# Account Settings (Tài khoản kế toán) — Feature Document (BE + WPF)

> **Jira:** — | **Branch:** `dev` | **Generated:** 2026-08-09 | **Updated:** 2026-08-10 (thêm 111/112/131/334 cho Phiếu chi — xem changelog cuối file)

---

## PRD Summary

> Màn "Kho" cần danh mục cài đặt "TK Cài đặt" — danh sách tài khoản kế toán (Thông tư 200 style) dùng làm dropdown cho các trường TK Kho/TK doanh thu/TK chiết khấu/TK giảm giá/TK trả lại/TK chi phí trong nghiệp vụ. User cung cấp 6 ảnh chụp dropdown tài khoản tham khảo (Số tài khoản + Tên tài khoản) làm nguồn seed data.

- **Goal:** CRUD API cho Tài khoản kế toán (`Code` + `Description`), seed sẵn 36 tài khoản chuẩn Thông tư 200 lấy từ dữ liệu trong ảnh, màn quản lý riêng truy cập từ tile "📒 Tài khoản kế toán" trong hub Kho.
- **Scope quyết định (user xác nhận qua `/ct-be-to-desktop`):**
  - Full CRUD (không phải list tĩnh)
  - **Chưa** wire vào `ProductFormWindow` (Product chưa có field TK nào — 6 field TK Kho/doanh thu/chiết khấu/giảm giá/trả lại/chi phí trong ảnh chỉ là tham khảo data, không tạo mới trên Product ở phase này)
  - Seed sẵn dữ liệu mẫu qua EF migration `HasData`
- **Acceptance criteria:**
  - [x] `GET /api/v1/account-settings` trả toàn bộ danh sách, sort theo `Code`
  - [x] `POST /api/v1/account-settings` tạo mới, validate `code` + `description` required, `code` unique case-insensitive
  - [x] `PUT /api/v1/account-settings/{id}` cập nhật, validate unique (exclude self)
  - [x] `DELETE /api/v1/account-settings/{id}` xóa — không có ràng buộc FK (chưa dùng ở đâu khác)
  - [x] Seed 36 tài khoản (151, 152, 1531–1534, 1551, 1557, 1561, 1562, 1567, 157, 158, 3339, 5111–5118, 711, 154, 2411–2413, 242, 6111, 6112, 632, 6232, 6412, 6413, 6417, 6422, 6423, 811) kèm tên chuẩn Thông tư 200
  - [x] WPF: tile "📒 Tài khoản kế toán" trong `WarehouseView` → `AccountSettingListView` (List + Thêm + Sửa + Xóa)
  - [x] Dùng chung hạ tầng cache (load 1 lần sau login) + SignalR realtime đã có sẵn

---

## Business Rules

| Rule | Description |
|------|-------------|
| Code + Description required | `code`/`description` không được trống — `DomainException` |
| Code unique | `code` unique case-insensitive — `DomainException` nếu trùng (Create: check toàn bộ; Update: exclude chính nó) |
| Không có ràng buộc IsInUse | ⚠️ **Cập nhật 2026-08-10**: `AccountSetting` **giờ được `PaymentEntry` tham chiếu** (`DebitAccountSettingId`/`CreditAccountSettingId`, FK `Restrict`) — DB sẽ chặn xoá nếu đang được Payment dùng, nhưng `DeleteAccountSettingUseCase` **chưa có guard `IsInUseAsync` ở tầng UseCase** (sẽ ra lỗi FK constraint thô từ Postgres thay vì message rõ ràng). Xem "Known gaps" cuối file. |
| WPF cache-first | `IAccountSettingService.GetAllAsync` cache-first — load 1 lần sau login (`PostLoginSyncService`), tự cập nhật qua `DataSyncHub` |

---

## Architecture Overview

### Key Components (BE)

| Layer | File | Role |
|-------|------|------|
| Controller | `Lamour.Api/Controllers/AccountSettingsController.cs` | 4 HTTP actions (GetAll/Create/Update/Delete), `[Authorize]` |
| UseCase | `UseCases/GetAccountSettingsUseCase.cs` | Fetch all + map to DTO; `MapToDto` static dùng chung |
| UseCase | `UseCases/CreateAccountSettingUseCase.cs` | Validate required + `CodeExistsAsync` → persist → broadcast `AccountSettingCreated` |
| UseCase | `UseCases/UpdateAccountSettingUseCase.cs` | Find → validate unique (exclude self) → update → broadcast `AccountSettingUpdated` |
| UseCase | `UseCases/DeleteAccountSettingUseCase.cs` | Find → delete → broadcast `AccountSettingDeleted` |
| Repository | `Repositories/IAccountSettingRepository.cs` | `GetAllAsync`, `GetByIdAsync`, `CodeExistsAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` |
| Repository | `Lamour.Infrastructure/Repositories/AccountSettingRepository.cs` | EF Core implementation, sort theo `Code` |
| Entity | `Lamour.Domain/Entities/AccountSetting.cs` | `Id`, `Code`, `Description` |
| Config | `Lamour.Infrastructure/Persistence/Configurations/AccountSettingConfiguration.cs` | Table `account_settings`, `Code` unique index, `HasData` seed 36 rows |
| Realtime | `Lamour.Api/Realtime/SignalRNotificationBroadcaster.cs` | `AccountSettingCreatedAsync`/`AccountSettingUpdatedAsync`/`AccountSettingDeletedAsync` |

### Data Flow

```
HTTP Request
  → AccountSettingsController
  → IXxxAccountSettingUseCase.ExecuteAsync()
  → IAccountSettingRepository
  → AppDbContext (EF Core + PostgreSQL table: account_settings)
  ← AccountSetting entity → AccountSettingResponseDto
  ← INotificationBroadcaster.AccountSettingXxxAsync() → SignalR DataSyncHub
  ← IActionResult
```

---

## Key Files & Symbols

### Domain
- [`Lamour.Domain/Entities/AccountSetting.cs`](../../../../Lamour.Domain/Entities/AccountSetting.cs) — `Id`, `Code`, `Description`

### Application — Repositories
- [`Repositories/IAccountSettingRepository.cs`](../Repositories/IAccountSettingRepository.cs) — `GetAllAsync`, `GetByIdAsync`, `CodeExistsAsync(code, excludeId?)`, `AddAsync`, `UpdateAsync`, `DeleteAsync`

### Application — DTOs
- [`Dtos/AccountSettingResponseDto.cs`](../Dtos/AccountSettingResponseDto.cs) — `id`, `code`, `description`
- [`Dtos/CreateAccountSettingRequestDto.cs`](../Dtos/CreateAccountSettingRequestDto.cs) — `code`, `description`
- [`Dtos/UpdateAccountSettingRequestDto.cs`](../Dtos/UpdateAccountSettingRequestDto.cs) — `code`, `description`

### Application — UseCases
- [`UseCases/GetAccountSettingsUseCase.cs`](../UseCases/GetAccountSettingsUseCase.cs) — `ExecuteAsync()` → `IEnumerable<AccountSettingResponseDto>`; `internal static MapToDto()` dùng chung
- [`UseCases/CreateAccountSettingUseCase.cs`](../UseCases/CreateAccountSettingUseCase.cs) — Validate required + `CodeExistsAsync` → `AddAsync` → broadcast
- [`UseCases/UpdateAccountSettingUseCase.cs`](../UseCases/UpdateAccountSettingUseCase.cs) — `GetByIdAsync` → validate → `UpdateAsync` → broadcast
- [`UseCases/DeleteAccountSettingUseCase.cs`](../UseCases/DeleteAccountSettingUseCase.cs) — `GetByIdAsync` → `DeleteAsync` → broadcast

### Infrastructure
- [`Lamour.Infrastructure/Repositories/AccountSettingRepository.cs`](../../../../Lamour.Infrastructure/Repositories/AccountSettingRepository.cs) — EF Core impl, `AsNoTracking()` trên mọi read
- [`Lamour.Infrastructure/Persistence/Configurations/AccountSettingConfiguration.cs`](../../../../Lamour.Infrastructure/Persistence/Configurations/AccountSettingConfiguration.cs) — table `account_settings`, unique index trên `code`, `HasData` seed 36 tài khoản

---

## API Contracts

| Method | Endpoint | Input | Output |
|--------|----------|-------|--------|
| `GET` | `/api/v1/account-settings` | — | `AccountSettingResponseDto[]` |
| `POST` | `/api/v1/account-settings` | `CreateAccountSettingRequestDto` | `AccountSettingResponseDto` (201) |
| `PUT` | `/api/v1/account-settings/{id}` | `UpdateAccountSettingRequestDto` | `AccountSettingResponseDto` (200) |
| `DELETE` | `/api/v1/account-settings/{id}` | — | 204 No Content |

### Request — Create / Update
```json
{ "code": "632", "description": "Giá vốn hàng bán" }
```

### Response
```json
{ "id": 29, "code": "632", "description": "Giá vốn hàng bán" }
```

---

## Edge Cases & Error Handling

| Scenario | Expected Behavior | Handled? |
|----------|------------------|----------|
| `code`/`description` trống | `DomainException` → 400 | ✅ |
| `code` đã tồn tại (Create) | `DomainException` → 400 | ✅ |
| `code` trùng khi Update (exclude self) | `DomainException` → 400 | ✅ |
| `id` không tồn tại (Update/Delete) | `NotFoundException` → 404 | ✅ |

---

## Test Coverage Notes

| Component | Test File | Coverage |
|-----------|-----------|----------|
| `GetAccountSettingsUseCase` / `CreateAccountSettingUseCase` / `UpdateAccountSettingUseCase` / `DeleteAccountSettingUseCase` | — | ❌ Missing |

---

## Seed Data (43 tài khoản: 39 Thông tư 200 + 3 tài khoản 521x + 4 tài khoản cash-flow 2026-08-10)

> Id 40-43 (`111`/`112`/`131`/`334`) seed riêng cho Phiếu Chi — TK Nợ/TK Có của Payment đổi từ enum `AccountCode` cứng sang FK thật tới bảng này (2026-08-10). Xem [`phieu-chi.md`](../../Accounting/docs/phieu-chi.md).

| Id | Code | Description |
|---|---|---|
| 40 | 111 | Tiền mặt |
| 41 | 112 | Tiền gửi ngân hàng |
| 42 | 131 | Phải thu của khách hàng |
| 43 | 334 | Phải trả người lao động |

### Seed gốc (39 tài khoản, Thông tư 200 + 3 tài khoản 521x bổ sung)

| Code | Description | Code | Description |
|---|---|---|---|
| 151 | Hàng mua đang đi đường | 154 | Chi phí sản xuất, kinh doanh dở dang |
| 152 | Nguyên liệu, vật liệu | 2411 | Mua sắm TSCĐ |
| 1531 | Công cụ, dụng cụ | 2412 | Xây dựng cơ bản |
| 1532 | Bao bì luân chuyển | 2413 | Sửa chữa lớn TSCĐ |
| 1533 | Đồ dùng cho thuê | 242 | Chi phí trả trước |
| 1534 | Thiết bị, phụ tùng thay thế | 6111 | Mua nguyên liệu, vật liệu |
| 1551 | Thành phẩm nhập kho | 6112 | Mua hàng hóa |
| 1557 | Thành phẩm bất động sản | 632 | Giá vốn hàng bán |
| 1561 | Giá mua hàng hóa | 6232 | Chi phí vật liệu |
| 1562 | Chi phí thu mua hàng hóa | 6412 | Chi phí vật liệu, bao bì |
| 1567 | Hàng hóa bất động sản | 6413 | Chi phí dụng cụ, đồ dùng |
| 157 | Hàng gửi đi bán | 6417 | Chi phí dịch vụ mua ngoài |
| 158 | Hàng hóa kho bảo thuế | 6422 | Chi phí vật liệu quản lý |
| 3339 | Phí, lệ phí và các khoản phải nộp khác | 6423 | Chi phí đồ dùng văn phòng |
| 5111 | Doanh thu bán hàng hóa | 811 | Chi phí khác |
| 5112 | Doanh thu bán các thành phẩm | | |
| 5113 | Doanh thu cung cấp dịch vụ | | |
| 5114 | Doanh thu trợ cấp, trợ giá | | |
| 5117 | Doanh thu kinh doanh bất động sản đầu tư | | |
| 5118 | Doanh thu khác | 5211 | Chiết khấu thương mại |
| 711 | Thu nhập khác | 5212 | Hàng bán bị trả lại |
| | | 5213 | Giảm giá hàng bán |

---

## EF Migration

Migration `AddProductUnitsAndAccountSettings` (`20260809102942_...`) — tạo cả 2 bảng `product_units` + `account_settings` trong cùng 1 migration. Seed qua `HasData` trong `AccountSettingConfiguration`, verify bằng `dotnet ef database update` (local DB, username `hai.phan`). Chi tiết migration command xem [`product-units.md`](../../ProductUnits/docs/product-units.md).

Migration `AddDiscountReturnAccountSettings` (`20260809113714_...`, 2026-08-09) — thêm 3 tài khoản `5211`/`5212`/`5213` (id 37-39), phát sinh khi wire default TK kế toán cho popup "Thêm vật tư hàng hoá" ([`product-list.md`](../../../../../../desktop-lamour/src/DesktopLamour/Features/HomePage/ProductList/docs/product-list.md) phía WPF) — dropdown mẫu MISA dùng các mã 521x này cho TK chiết khấu/giảm giá/trả lại nhưng seed ban đầu chỉ có dải doanh thu `511x`.

---

## WPF Client (`desktop-lamour`)

> Không có doc riêng phía WPF cho Account Settings — module nhỏ, ghi chú gộp ở đây.

### Module mới: `Features/HomePage/AccountSettings/`

Cấu trúc giống pattern Supplier (Code + tên) hơn Category (2 field thay vì 1):

| File | Role |
|---|---|
| `Domain/Models/AccountSetting.cs` | Implements `ISearchableItem` — `Code` = số tài khoản thật, `Name => Description` (để thỏa interface), `DisplayText => "{Code} — {Description}"` (sẵn sàng dùng làm dropdown item nếu wire vào nơi khác sau này) |
| `Data/Services/IAccountSettingService.cs` / `AccountSettingService.cs` | HttpClient cache-first, `EnsureSuccessOrThrowAsync` — 4 method GetAll/Create/Update/Delete |
| `Data/Cache/IAccountSettingCacheStore.cs` / `AccountSettingCacheStore.cs` | `EntityCacheStore<AccountSettingResponseDto>` |
| `Data/Repositories/IAccountSettingRepository.cs` / `AccountSettingRepository.cs` | Map DTO ↔ `AccountSetting` model |
| `Domain/UseCases/*` (4 pairs) | Validate client-side (`ValidationException`, unique-code exclude self khi Update) trước khi gọi API |
| `ViewModels/AccountSettingFormViewModel.cs` + `Views/AccountSettingFormWindow.xaml` | Popup 2 field `Code` + `Description` — Add/Edit chung |
| `ViewModels/AccountSettingListViewModel.cs` + `Views/AccountSettingListView.xaml` | List (2 cột: Số tài khoản, Tên tài khoản) + Thêm + Sửa + Xóa |

### Truy cập từ hub Kho

- `WarehouseView.xaml`: tile "📒 Tài khoản kế toán" trong section "Cài đặt" mới, cạnh "📏 Đơn vị tính"
- `WarehouseViewModel.cs`: `NavigateToAccountSettingsCommand` → `NavigationService.NavigateTo(NavigationRoutes.AccountSettings.List)`
- `NavigationRoutes.AccountSettings.List = "AccountSettingListView"` + case tương ứng trong `NavigationService.ResolveView`

### Realtime — dùng chung hạ tầng đã có

`RealtimeSyncService`/`RealtimeServiceCollectionExtensions`/`PostLoginSyncService` đã thêm `IAccountSettingCacheStore` + lắng nghe `AccountSettingCreated`/`AccountSettingUpdated`/`AccountSettingDeleted` từ `DataSyncHub`.

### Known gaps (chưa làm, ngoài phạm vi yêu cầu ban đầu)

- Chưa wire vào `ProductFormWindow` — Product chưa có field TK Kho/TK doanh thu/TK chiết khấu/TK giảm giá/TK trả lại/TK chi phí nào cả. Nếu cần sau này, model `AccountSetting.DisplayText` đã sẵn định dạng phù hợp cho dropdown (`"{Code} — {Description}"`).
- Chưa có unit test nào (BE lẫn WPF).
- **Mới 2026-08-10**: `PaymentEntry` (Phiếu chi) giờ tham chiếu `AccountSetting` qua FK `Restrict` — `DeleteAccountSettingUseCase` chưa có guard `IsInUseAsync`, xoá 1 tài khoản đang được Payment dùng sẽ ra lỗi FK constraint thô từ Postgres thay vì message rõ ràng như "Đơn vị tính đang được sản phẩm sử dụng, không thể xoá" (pattern đã có ở `ProductUnit`/`DeleteProductUnitUseCase`). Nên thêm `IsInUseAsync(int accountSettingId)` kiểm tra `PaymentEntry.DebitAccountSettingId`/`CreditAccountSettingId` trước khi cho phép xoá.

## Changelog

- **2026-08-10** — Thêm 4 tài khoản `111`/`112`/`131`/`334` (Id 40-43) phục vụ Phiếu chi chuyển TK Nợ/TK Có từ enum `AccountCode` cứng sang FK thật (xem [`phieu-chi.md`](../../Accounting/docs/phieu-chi.md)). Migration `ConvertPaymentAccountsToAccountSettingFk`.
- **2026-08-09** — Thêm 3 tài khoản `5211`/`5212`/`5213` (Id 37-39) cho popup "Thêm vật tư hàng hoá".

---

*Generated by `/ct-be-to-desktop` on 2026-08-09*
