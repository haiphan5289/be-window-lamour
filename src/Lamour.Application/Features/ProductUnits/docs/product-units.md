# Product Units (Đơn vị tính) — Feature Document (BE + WPF)

> **Jira:** — | **Branch:** `dev` | **Generated:** 2026-08-09

---

## PRD Summary

> Màn "Kho" cần danh mục cài đặt "Đơn vị tính" (Chai, Hộp, Tuýp, Cái, Cuốn...) — tách riêng thành master-data entity, tương tự Category. Yêu cầu gốc: "Kho add more feature config/settings — DVT: Chai,Hộp,tuýp,cái,cuốn...".

- **Goal:** CRUD API cho Đơn vị tính, seed sẵn 10 đơn vị phổ biến, màn quản lý riêng truy cập từ tile "📏 Đơn vị tính" trong hub Kho.
- **Scope quyết định (user xác nhận qua `/ct-be-to-desktop`):**
  - Full CRUD (không phải list tĩnh)
  - **Chưa** wire vào `ProductFormWindow` (field `Unit` của Product vẫn là free-text) — để bước sau nếu cần
  - Seed sẵn dữ liệu mẫu qua EF migration `HasData`
- **Acceptance criteria:**
  - [x] `GET /api/v1/product-units` trả toàn bộ danh sách
  - [x] `POST /api/v1/product-units` tạo mới, validate `name` required + unique case-insensitive
  - [x] `PUT /api/v1/product-units/{id}` cập nhật, validate unique (exclude self)
  - [x] `DELETE /api/v1/product-units/{id}` xóa — không có ràng buộc FK (chưa dùng ở đâu khác)
  - [x] Seed 10 đơn vị: Cái, Hộp, Chai, Tuýp, Cuốn, Bộ, Set, Thùng, Gói, Lọ
  - [x] WPF: tile "📏 Đơn vị tính" trong `WarehouseView` → `ProductUnitListView` (List + Thêm + Sửa + Xóa)
  - [x] Dùng chung hạ tầng cache (load 1 lần sau login) + SignalR realtime đã có sẵn cho Customer/Employee/Product/Supplier/Category

---

## Business Rules

| Rule | Description |
|------|-------------|
| Name required | `name` không được trống — `DomainException` |
| Name unique | `name` unique case-insensitive — `DomainException` nếu trùng (Create: check toàn bộ; Update: exclude chính nó) |
| Không có ràng buộc IsInUse | Khác Category — `ProductUnit` chưa được `Product` tham chiếu (không wire vào Product form ở phase này), nên `DeleteProductUnitUseCase` xóa thẳng, không cần guard |
| WPF cache-first | `IProductUnitService.GetAllAsync` cache-first — load 1 lần sau login (`PostLoginSyncService`), tự cập nhật qua `DataSyncHub` khi có unit khác được tạo/sửa/xóa ở client khác |

---

## Architecture Overview

### Key Components (BE)

| Layer | File | Role |
|-------|------|------|
| Controller | `Lamour.Api/Controllers/ProductUnitsController.cs` | 4 HTTP actions (GetAll/Create/Update/Delete), `[Authorize]` |
| UseCase | `UseCases/GetProductUnitsUseCase.cs` | Fetch all + map to DTO; `MapToDto` static dùng chung |
| UseCase | `UseCases/CreateProductUnitUseCase.cs` | Validate required + unique → persist → broadcast `ProductUnitCreated` |
| UseCase | `UseCases/UpdateProductUnitUseCase.cs` | Find → validate unique (exclude self) → update → broadcast `ProductUnitUpdated` |
| UseCase | `UseCases/DeleteProductUnitUseCase.cs` | Find → delete → broadcast `ProductUnitDeleted` |
| Repository | `Repositories/IProductUnitRepository.cs` | `GetAllAsync`, `GetByIdAsync`, `NameExistsAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` |
| Repository | `Lamour.Infrastructure/Repositories/ProductUnitRepository.cs` | EF Core implementation |
| Entity | `Lamour.Domain/Entities/ProductUnit.cs` | `Id`, `Name` |
| Config | `Lamour.Infrastructure/Persistence/Configurations/ProductUnitConfiguration.cs` | Table `product_units`, `Name` unique index, `HasData` seed 10 rows |
| Realtime | `Lamour.Api/Realtime/SignalRNotificationBroadcaster.cs` | `ProductUnitCreatedAsync`/`ProductUnitUpdatedAsync`/`ProductUnitDeletedAsync` — cùng `DataSyncHub` với các entity khác |

### Data Flow

```
HTTP Request
  → ProductUnitsController
  → IXxxProductUnitUseCase.ExecuteAsync()
  → IProductUnitRepository
  → AppDbContext (EF Core + PostgreSQL table: product_units)
  ← ProductUnit entity → ProductUnitResponseDto
  ← INotificationBroadcaster.ProductUnitXxxAsync() → SignalR DataSyncHub → mọi WPF client đang kết nối
  ← IActionResult
```

---

## Key Files & Symbols

### Domain
- [`Lamour.Domain/Entities/ProductUnit.cs`](../../../../Lamour.Domain/Entities/ProductUnit.cs) — `Id`, `Name`

### Application — Repositories
- [`Repositories/IProductUnitRepository.cs`](../Repositories/IProductUnitRepository.cs) — `GetAllAsync`, `GetByIdAsync`, `NameExistsAsync(name, excludeId?)`, `AddAsync`, `UpdateAsync`, `DeleteAsync`

### Application — DTOs
- [`Dtos/ProductUnitResponseDto.cs`](../Dtos/ProductUnitResponseDto.cs) — `id`, `name`
- [`Dtos/CreateProductUnitRequestDto.cs`](../Dtos/CreateProductUnitRequestDto.cs) — `name`
- [`Dtos/UpdateProductUnitRequestDto.cs`](../Dtos/UpdateProductUnitRequestDto.cs) — `name`

### Application — UseCases
- [`UseCases/GetProductUnitsUseCase.cs`](../UseCases/GetProductUnitsUseCase.cs) — `ExecuteAsync()` → `IEnumerable<ProductUnitResponseDto>`; `internal static MapToDto()` dùng chung
- [`UseCases/CreateProductUnitUseCase.cs`](../UseCases/CreateProductUnitUseCase.cs) — Validate required + `NameExistsAsync` → `AddAsync` → broadcast
- [`UseCases/UpdateProductUnitUseCase.cs`](../UseCases/UpdateProductUnitUseCase.cs) — `GetByIdAsync` → validate → `UpdateAsync` → broadcast
- [`UseCases/DeleteProductUnitUseCase.cs`](../UseCases/DeleteProductUnitUseCase.cs) — `GetByIdAsync` → `DeleteAsync` → broadcast

### Infrastructure
- [`Lamour.Infrastructure/Repositories/ProductUnitRepository.cs`](../../../../Lamour.Infrastructure/Repositories/ProductUnitRepository.cs) — EF Core impl, `AsNoTracking()` trên mọi read
- [`Lamour.Infrastructure/Persistence/Configurations/ProductUnitConfiguration.cs`](../../../../Lamour.Infrastructure/Persistence/Configurations/ProductUnitConfiguration.cs) — table `product_units`, unique index trên `name`, `HasData` seed

---

## API Contracts

| Method | Endpoint | Input | Output |
|--------|----------|-------|--------|
| `GET` | `/api/v1/product-units` | — | `ProductUnitResponseDto[]` |
| `POST` | `/api/v1/product-units` | `CreateProductUnitRequestDto` | `ProductUnitResponseDto` (201) |
| `PUT` | `/api/v1/product-units/{id}` | `UpdateProductUnitRequestDto` | `ProductUnitResponseDto` (200) |
| `DELETE` | `/api/v1/product-units/{id}` | — | 204 No Content |

### Request — Create / Update
```json
{ "name": "Chai" }
```

### Response
```json
{ "id": 3, "name": "Chai" }
```

---

## Edge Cases & Error Handling

| Scenario | Expected Behavior | Handled? |
|----------|------------------|----------|
| `name` trống | `DomainException` → 400 | ✅ |
| `name` đã tồn tại (Create) | `DomainException` → 400 | ✅ |
| `name` trùng khi Update (exclude self) | `DomainException` → 400 | ✅ |
| `id` không tồn tại (Update/Delete) | `NotFoundException` → 404 | ✅ |

---

## Test Coverage Notes

| Component | Test File | Coverage |
|-----------|-----------|----------|
| `GetProductUnitsUseCase` / `CreateProductUnitUseCase` / `UpdateProductUnitUseCase` / `DeleteProductUnitUseCase` | — | ❌ Missing |

---

## EF Migration

Migration `AddProductUnitsAndAccountSettings` (`20260809102942_...`) — tạo cả 2 bảng `product_units` + `account_settings` trong cùng 1 migration (cùng đợt yêu cầu, cùng phase Kho settings). Seed data qua `HasData` trong `ProductUnitConfiguration`/`AccountSettingConfiguration` — 10 đơn vị tính + 36 tài khoản, đã verify bằng `dotnet ef database update` (local DB, username `hai.phan`).

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef migrations add AddProductUnitsAndAccountSettings \
  --project src/Lamour.Infrastructure --startup-project src/Lamour.Api
dotnet ef database update \
  --project src/Lamour.Infrastructure --startup-project src/Lamour.Api
```

---

## WPF Client (`desktop-lamour`)

> Không có doc riêng phía WPF cho Product Units — module nhỏ, ghi chú gộp ở đây.

### Module mới: `Features/HomePage/ProductUnits/`

Cấu trúc giống hệt pattern Category (Data/Services, Data/Cache, Data/Repositories, Domain/Models, Domain/UseCases):

| File | Role |
|---|---|
| `Domain/Models/ProductUnit.cs` | Implements `ISearchableItem` — `Code` luôn rỗng, `DisplayText => Name` |
| `Data/Services/IProductUnitService.cs` / `ProductUnitService.cs` | HttpClient cache-first, `EnsureSuccessOrThrowAsync` — 4 method GetAll/Create/Update/Delete |
| `Data/Cache/IProductUnitCacheStore.cs` / `ProductUnitCacheStore.cs` | `EntityCacheStore<ProductUnitResponseDto>` |
| `Data/Repositories/IProductUnitRepository.cs` / `ProductUnitRepository.cs` | Map DTO ↔ `ProductUnit` model |
| `Domain/UseCases/*` (4 pairs) | Validate client-side (`ValidationException`, unique-name exclude self khi Update) trước khi gọi API |
| `ViewModels/ProductUnitFormViewModel.cs` + `Views/ProductUnitFormWindow.xaml` | Popup 1 field `Name` — Add/Edit chung, `Initialize(ProductUnit? unit = null)` |
| `ViewModels/ProductUnitListViewModel.cs` + `Views/ProductUnitListView.xaml` | List + Thêm + Sửa + Xóa — skeleton `CategoryListView` |

### Truy cập từ hub Kho

- `WarehouseView.xaml`: thêm section "Cài đặt" với 2 tile mới — "📏 Đơn vị tính" và "📒 Tài khoản kế toán" — cạnh các tile nghiệp vụ kho hiện có
- `WarehouseViewModel.cs`: `NavigateToProductUnitsCommand` → `NavigationService.NavigateTo(NavigationRoutes.ProductUnits.List)`
- `NavigationRoutes.ProductUnits.List = "ProductUnitListView"` + case tương ứng trong `NavigationService.ResolveView`

### Realtime — dùng chung hạ tầng đã có

`RealtimeSyncService`/`RealtimeServiceCollectionExtensions`/`PostLoginSyncService` (tại `Features/Realtime/`) đã thêm `IProductUnitCacheStore` + lắng nghe `ProductUnitCreated`/`ProductUnitUpdated`/`ProductUnitDeleted` từ `DataSyncHub`.

### Known gaps (chưa làm, ngoài phạm vi yêu cầu ban đầu)

- Chưa wire vào `ProductFormWindow` (`Product.Unit` vẫn free-text) — chờ yêu cầu tiếp theo nếu cần đổi sang dropdown.
- Chưa có unit test nào (BE lẫn WPF).

---

*Generated by `/ct-be-to-desktop` on 2026-08-09*
