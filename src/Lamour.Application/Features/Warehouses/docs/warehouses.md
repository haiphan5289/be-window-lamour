# Warehouses (Kho) — Feature Document (BE + WPF)

> **Jira:** — | **Branch:** `dev` | **Generated:** 2026-08-09

---

## PRD Summary

> `Warehouse` entity đã tồn tại từ trước (dùng bởi `WarehouseReceiptLine.WarehouseId`) nhưng **chưa có CRUD API nào** — chỉ có 1 `EF Configuration` + 1 row seed (`KHO01`/"Kho chính") viết tay trong `WarehouseReceiptConfiguration.cs`. Yêu cầu redesign popup "Sửa Vật tư, hàng hoá, dịch vụ" (xem [`products.md`](../../Products/docs/products.md)) cần field "Kho ngầm định" — cơ hội để hoàn thiện luôn CRUD danh sách Kho, theo yêu cầu user: "bên màn hình Kho build thêm chức năng list Kho, Kho ngầm định gồm: HH - Hàng hoá, TB - Trừng bày".

- **Goal:** CRUD API đầy đủ cho danh sách Kho (Code + Name + IsActive), màn quản lý riêng trong hub Kho, dùng làm nguồn "Kho ngầm định" cho Product.
- **Acceptance criteria:**
  - [x] `GET /api/v1/warehouses` trả toàn bộ danh sách, sort theo `Code`
  - [x] `POST /api/v1/warehouses` tạo mới, validate `code`/`name` required, `code` unique case-insensitive
  - [x] `PUT /api/v1/warehouses/{id}` cập nhật, validate unique (exclude self)
  - [x] `DELETE /api/v1/warehouses/{id}` xóa
  - [x] Seed thêm 2 kho: `HH` - "Hàng hoá", `TB` - "Trưng bày" (giữ nguyên `KHO01`/"Kho chính" đã seed từ trước, không xóa)
  - [x] WPF: tile "🏬 Danh sách Kho" trong `WarehouseView` (hub Kho) → màn List + Thêm/Sửa/Xóa
  - [x] Dùng chung hạ tầng cache + SignalR realtime

---

## ⚠️ Naming note (đọc trước khi sửa)

- **BE**: entity vẫn tên `Warehouse` (`Lamour.Domain.Entities.Warehouse`), namespace feature là `Lamour.Application.Features.Warehouses` (**plural**) — khác với `Lamour.Application.Features.Warehouse` (**singular**, feature cũ chứa `GetInventorySummaryUseCase`). Dùng type alias `using WarehouseEntity = Lamour.Domain.Entities.Warehouse;` trong vài file Application layer để tránh compiler resolve nhầm `Warehouse` thành namespace lồng nhau (`Lamour.Application.Features.Warehouse`) — lỗi `CS0118: 'Warehouse' is a namespace but is used like a type`.
- **WPF**: namespace feature mới là `DesktopLamour.Features.HomePage.Warehouses` (plural), nhưng model/service/repository đặt tên **`WarehouseSetting`** (không phải `Warehouse`) — vì `Features.HomePage.Warehouse` (singular, feature Phiếu nhập kho/Tổng hợp tồn kho cũ) đã có sẵn `IWarehouseRepository`/`WarehouseRepository`/`IWarehouseService`/`WarehouseService` với đúng tên đó. Đặt trùng tên sẽ vừa bị namespace-collision (như BE) vừa bị `CS0104: ambiguous reference` khi `HomeServiceCollectionExtensions.cs` `using` cả 2 namespace cùng lúc.

---

## Business Rules

| Rule | Description |
|------|-------------|
| Code + Name required | `code`/`name` không được trống — `DomainException` |
| Code unique | `code` unique case-insensitive — `DomainException` nếu trùng (Create: check toàn bộ; Update: exclude chính nó) |
| Không có guard IsInUse | `WarehouseReceiptLine.WarehouseId` dùng `OnDelete: Restrict` (chặn ở DB nếu đang có phiếu nhập tham chiếu) nhưng `Product.DefaultWarehouseId` dùng `OnDelete: SetNull` (Product không bị chặn) — `DeleteWarehouseUseCase` không tự thêm guard nào ở tầng UseCase, dựa hoàn toàn vào FK constraint của DB |
| WPF cache-first | `IWarehouseSettingService.GetAllAsync` cache-first — load 1 lần sau login (`PostLoginSyncService`), tự cập nhật qua `DataSyncHub` |

---

## Architecture Overview

### Key Components (BE)

| Layer | File | Role |
|-------|------|------|
| Controller | `Lamour.Api/Controllers/WarehousesController.cs` | 4 HTTP actions (GetAll/Create/Update/Delete), `[Authorize]` |
| UseCase | `UseCases/GetWarehousesUseCase.cs` | Fetch all + map to DTO |
| UseCase | `UseCases/CreateWarehouseUseCase.cs` | Validate required + unique → persist → broadcast `WarehouseCreated` |
| UseCase | `UseCases/UpdateWarehouseUseCase.cs` | Find → validate unique (exclude self) → update → broadcast `WarehouseUpdated` |
| UseCase | `UseCases/DeleteWarehouseUseCase.cs` | Find → delete → broadcast `WarehouseDeleted` |
| Repository | `Repositories/IWarehouseRepository.cs` | `GetAllAsync`, `GetByIdAsync`, `CodeExistsAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` |
| Repository | `Lamour.Infrastructure/Repositories/WarehouseRepository.cs` | EF Core implementation, sort theo `Code` |
| Entity | `Lamour.Domain/Entities/Warehouse.cs` | `Id`, `Code`, `Name`, `IsActive` — **đã tồn tại từ trước**, không đổi |
| Config | `Lamour.Infrastructure/Persistence/Configurations/WarehouseReceiptConfiguration.cs` | Chứa `WarehouseConfiguration` (table `warehouses`) **cùng file** với `WarehouseReceiptConfiguration`/`WarehouseReceiptLineConfiguration` — dễ bị miss khi grep theo tên file; `HasData` seed 3 rows: `KHO01`(cũ) + `HH`/`TB`(2026-08-09) |
| Realtime | `Lamour.Api/Realtime/SignalRNotificationBroadcaster.cs` | `WarehouseCreatedAsync`/`WarehouseUpdatedAsync`/`WarehouseDeletedAsync` |

### Data Flow

```
HTTP Request
  → WarehousesController
  → IXxxWarehouseUseCase.ExecuteAsync()
  → IWarehouseRepository
  → AppDbContext (EF Core + PostgreSQL table: warehouses)
  ← Warehouse entity → WarehouseResponseDto
  ← INotificationBroadcaster.WarehouseXxxAsync() → SignalR DataSyncHub
  ← IActionResult
```

---

## API Contracts

| Method | Endpoint | Input | Output |
|--------|----------|-------|--------|
| `GET` | `/api/v1/warehouses` | — | `WarehouseResponseDto[]` |
| `POST` | `/api/v1/warehouses` | `CreateWarehouseRequestDto` | `WarehouseResponseDto` (201) |
| `PUT` | `/api/v1/warehouses/{id}` | `UpdateWarehouseRequestDto` | `WarehouseResponseDto` (200) |
| `DELETE` | `/api/v1/warehouses/{id}` | — | 204 No Content |

### Request — Create / Update
```json
{ "code": "HH", "name": "Hàng hoá", "is_active": true }
```

### Response
```json
{ "id": 4, "code": "HH", "name": "Hàng hoá", "is_active": true }
```

---

## Seed Data

| Id | Code | Name | Ghi chú |
|---|---|---|---|
| 1 | KHO01 | Kho chính | Seed cũ, giữ nguyên |
| 3 | KHO02 | Kho chi nhánh Q.1 | Đã tồn tại sẵn trong DB local trước khi làm feature này (không rõ nguồn — không phải từ `HasData`); Id=2 bị bỏ qua khi chọn Id mới để tránh trùng |
| 4 | HH | Hàng hoá | Mới, 2026-08-09 |
| 5 | TB | Trưng bày | Mới, 2026-08-09 |

---

## EF Migration

Migration `ExtendProductForVTHHForm` (`20260809110425_...`) — cùng migration mở rộng `Product` (xem [`products.md`](../../Products/docs/products.md)), insert 2 row `warehouses` mới. Phải chạy `dotnet ef migrations remove` + `add` lại 1 lần vì lần đầu chọn Id=2/3 bị đụng data đã có sẵn trong DB local (`KHO02` đang giữ Id=3) — bài học: **luôn `SELECT * FROM warehouses` kiểm tra Id trống trước khi hardcode `HasData` Id cho bảng đã có data thủ công.**

---

## WPF Client (`desktop-lamour`)

### Module mới: `Features/HomePage/Warehouses/`

Naming: model = `WarehouseSetting` (xem lưu ý naming ở đầu file). Cấu trúc giống pattern Category/ProductUnit/AccountSetting:

| File | Role |
|---|---|
| `Domain/Models/WarehouseSetting.cs` | Implements `ISearchableItem` — `DisplayText => "{Code} — {Name}"` |
| `Data/Services/IWarehouseSettingService.cs` / `WarehouseSettingService.cs` | HttpClient cache-first |
| `Data/Cache/IWarehouseSettingCacheStore.cs` / `WarehouseSettingCacheStore.cs` | `EntityCacheStore<WarehouseSettingResponseDto>` |
| `Data/Repositories/IWarehouseSettingRepository.cs` / `WarehouseSettingRepository.cs` | Map DTO ↔ Model |
| `Domain/UseCases/*` (4 pairs) | Validate client-side trước khi gọi API |
| `ViewModels/WarehouseSettingFormViewModel.cs` + `Views/WarehouseSettingFormWindow.xaml` | Popup Code+Name+IsActive |
| `ViewModels/WarehouseSettingListViewModel.cs` + `Views/WarehouseSettingListView.xaml` | List (Mã kho/Tên kho/Hoạt động) + Thêm/Sửa/Xóa |

### Truy cập từ hub Kho

- `WarehouseView.xaml` (feature **singular** `Warehouse`, khác `Warehouses` mới): tile "🏬 Danh sách Kho" trong section "Cài đặt" (cạnh Đơn vị tính/Tài khoản kế toán)
- `WarehouseViewModel.cs`: `NavigateToWarehousesCommand` → `NavigationRoutes.Warehouses.List`
- `NavigationRoutes.cs`: thêm nested class `Warehouses` (plural, mới) cạnh `Warehouse` (singular, cũ) — 2 class tồn tại song song không xung đột (chỉ namespace/type collision xảy ra khi *cả 2 cùng định nghĩa 1 type/namespace tên giống nhau*, còn đây là 2 nested class tên khác nhau `Warehouse`/`Warehouses`)

### Wiring vào `ProductFormWindow`

- `ProductFormViewModel` inject thêm `IGetWarehouseSettingsUseCase` + `Func<WarehouseSettingFormWindow>` — field "Kho ngầm định" trong tab "Ngầm định", có nút "+" mở `WarehouseSettingFormWindow` giống pattern `AddCategoryCommand`
- Chi tiết đầy đủ xem [`products.md`](../../Products/docs/products.md)

### Realtime — dùng chung hạ tầng đã có

`RealtimeSyncService`/`RealtimeServiceCollectionExtensions`/`PostLoginSyncService` đã thêm `IWarehouseSettingCacheStore` + lắng nghe `WarehouseCreated`/`WarehouseUpdated`/`WarehouseDeleted` từ `DataSyncHub`.

### Known gaps

- Chưa có unit test nào (BE lẫn WPF).
- Chưa có `IsInUseAsync` guard khi xóa Warehouse đang được `Product.DefaultWarehouseId` tham chiếu (SetNull tự động, không cảnh báo user).

---

*Generated by `/ct-be-to-desktop` on 2026-08-09*
