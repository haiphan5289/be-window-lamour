# Warehouses (Kho) — Feature Document (BE + WPF)

> **Jira:** — | **Branch:** `dev` | **Generated:** 2026-08-09 | **Last updated:** 2026-09-11 (cùng
> ngày, mục mới nhất) — **⚠️ tìm ra nguyên nhân THẬT SỰ** của "Kho bị mất khi mở lại chứng từ": không
> phải (chỉ) do lọc IsActive như "Bug fix" bên dưới từng kết luận, mà do `GetSalesReturnsUseCase.
> MapToDto` (BE) **thiếu hẳn field `WarehouseId`** khi map entity → response DTO — trả `warehouse_id:
> 0` cho MỌI dòng, mọi chứng từ, bất kể Kho gì. Đây là nguyên nhân chính, phổ biến hơn nhiều; lỗi lọc
> IsActive vẫn có thật nhưng chỉ ảnh hưởng riêng trường hợp Kho đã ngưng hoạt động. Chi tiết đầy đủ +
> cách phát hiện (đối chiếu `psql` DB thật với response API thật) ở
> `SalesReturn/docs/sales-return.md` mục "Bug fix — 2026-09-11: warehouse_id luôn trả về 0...". Trước
> đó cùng ngày: backfill `default_warehouse_id = HH` cho 81 sản phẩm hiện có — xem "Update —
> 2026-09-11: backfill..." ngay dưới. Trước đó: fix lọc IsActive (mục "Bug fix — 2026-09-11: Kho bị
> mất khi mở lại chứng từ" — **kết luận ban đầu ở đây KHÔNG SAI nhưng KHÔNG ĐỦ**, vẫn giữ vì đúng 1
> phần). Trước đó: ngưng hoạt động `KHO01`/`KHO02`, chỉ còn `HH`/`TB`.

---

## Update — 2026-09-11: backfill Kho ngầm định = Hàng hoá cho toàn bộ sản phẩm hiện có

Theo yêu cầu ("chuyển tất cả sản phẩm sang kho Hàng hoá hết đi"), xác nhận phạm vi qua
`AskUserQuestion` sau khi kiểm tra DB thật (`psql`): 81/81 sản phẩm đang để trống
`default_warehouse_id` (không phải do lỗi — trường này vốn optional, chỉ mới thêm popup "Kho ngầm
định" từ 2026-08-09, sản phẩm cũ chưa từng được set). Đã xác nhận rõ **chỉ backfill trường "gợi ý
mặc định"**, KHÔNG đụng tới số tồn kho thật (`product_warehouse_stocks` — vốn đang chia ra 3 Kho:
`HH` 74 dòng/19081, `TB` 29 dòng/103, `KHO01` 2 dòng/1 — giữ nguyên, không gộp).

| Thành phần | Thay đổi |
|---|---|
| Migration mới `SetDefaultWarehouseForExistingProducts` | `UPDATE products SET default_warehouse_id = (SELECT id FROM warehouses WHERE code='HH') WHERE default_warehouse_id IS NULL` — chỉ update dòng đang NULL, an toàn rerun, không ghi đè lựa chọn thủ công nếu có. `Down()` revert ngược lại (set NULL cho các dòng đang = HH) |

Verify: `dotnet build` 0 lỗi, `dotnet ef database update` áp dụng thành công, `psql` xác nhận
81/81 sản phẩm đã có `default_warehouse_id = 4` (HH).

**Lưu ý cho sản phẩm MỚI tạo sau này**: không cần backfill gì thêm — `ProductFormViewModel` (WPF) đã
tự động mặc định "Kho ngầm định" = HH cho mọi sản phẩm mới (`DefaultWarehouseCode = "HH"`, có từ
trước, xem mục "Update — 2026-09-11: ngưng hoạt động..." bên dưới).

## Bug fix — 2026-09-11: Kho bị mất khi mở lại chứng từ (regression từ lần lọc IsActive cùng ngày)

**Triệu chứng** (báo qua 3 ảnh chụp màn hình thật): thêm dòng sản phẩm mới → cột "Kho" hiện đúng
"Hàng hoá" → bấm Cất → vẫn đúng. Nhưng mở lại 1 chứng từ **đã lưu từ trước** (tạo trước khi Kho
"Kho chính" bị ngưng hoạt động) → cột "Kho" hiện **trống trơn**, dù "Mã hàng"/"Tên hàng" vẫn đúng.

**Root cause**: lần sửa "ngưng hoạt động Kho thừa" (mục ngay dưới) đã lọc `Warehouses` (danh sách
dùng cho combobox chọn Kho) chỉ còn Kho `IsActive`. Nhưng ở `SalesReturnViewModel`/
`SalesOrderViewModel`/`ProductFormViewModel`, khi mở 1 chứng từ/sản phẩm **đã lưu**, code resolve
`SelectedWarehouse` bằng cách tìm trong CHÍNH danh sách đã lọc đó
(`Warehouses.FirstOrDefault(w => w.Id == line.WarehouseId)`). Nếu dòng cũ trỏ tới 1 Kho vừa bị
ngưng hoạt động (ví dụ `KHO01`/"Kho chính" — default cũ trước khi có field `HH`), `FirstOrDefault`
trả về `null` → `SelectedWarehouse = null` → cột Kho trống, và nếu user bấm Cất mà không tự chọn lại
Kho thì BE sẽ trả 400 "Vui lòng chọn Kho..." (vì `WarehouseId` gửi lên là 0).

**Fix**: cả 3 ViewModel giờ giữ **2 danh sách riêng** — `Warehouses` (đã lọc `IsActive`, dùng làm
`ItemsSource` cho combobox — chỉ cho chọn Kho đang hoạt động) và `_allWarehouses` (KHÔNG lọc, chỉ
dùng để RESOLVE giá trị Kho đã lưu của 1 dòng/sản phẩm cũ). Không cần đổi gì ở `AppSearchableComboBox`
— control này cho phép `SelectedItem` không nằm trong `ItemsSource` vẫn hiện đúng `DisplayText` (xem
`AppSearchableComboBox.xaml.cs:OnSelectedItemChanged`), nên chỉ cần lookup đúng nguồn dữ liệu.

| File | Thay đổi |
|---|---|
| `SalesReturnViewModel.cs` | Thêm field `_allWarehouses`; dòng resolve Kho cho line đã lưu đổi từ `Warehouses.FirstOrDefault(...)` sang `_allWarehouses.FirstOrDefault(...)` |
| `SalesOrderViewModel.cs` | Tương tự |
| `ProductFormViewModel.cs` | Tương tự, áp dụng cho `SelectedDefaultWarehouse` ("Kho ngầm định") ở cả 2 chỗ nạp (load ban đầu + reload sau khi bấm "+" thêm Kho mới) |

**Lưu ý — bug cùng loại (KHÔNG sửa trong lần này)**: `_allProducts` trong cả 2 ViewModel Sales/
SalesReturn cũng đã lọc `IsActive` từ trước (không phải do lần sửa hôm nay), và dùng chung pattern
resolve tương tự cho "Mã hàng"/"Tên hàng" của 1 dòng cũ (`_allProducts.FirstOrDefault(p => p.Id ==
l.ProductId)`). Nếu 1 sản phẩm bị ngưng kinh doanh sau khi đã có chứng từ tham chiếu, nhiều khả năng
"Mã hàng"/"Tên hàng" cũng sẽ trống theo đúng cơ chế lỗi y hệt — chưa xác nhận qua test thật, và chưa
sửa vì nằm ngoài phạm vi báo lỗi lần này (chỉ báo "Kho"). Cần theo dõi/hỏi lại nếu gặp.

Verify: `dotnet build -p:EnableWindowsTargeting=true` 0 lỗi. **Chưa test thật trên UTM** — đặc biệt
cần mở lại đúng chứng từ trong 3 ảnh chụp màn hình gốc để xác nhận cột Kho hiện lại đúng "Hàng hoá".

---

## Update — 2026-09-11: ngưng hoạt động Kho thừa + lọc IsActive ở mọi nơi chọn Kho

Theo yêu cầu ("Sản phẩm chọn mặc định là Kho Hàng Hoá, chỉ 2 Kho là Kho Trưng Bày & Hàng hoá"), xác
nhận phạm vi qua nhiều vòng `AskUserQuestion` sau khi kiểm tra DB thật (`psql`) phát hiện **4 Kho
đang active**, không phải 2 như PRD gốc dự định — `KHO01`/"Kho chính" (seed từ
`AddWarehouseReceipts`) và `KHO02`/"Kho chi nhánh Q.1" (không có trong bất kỳ migration nào, chèn
thủ công ngoài luồng) đã lẫn vào bên cạnh `HH`/`TB`.

| Thành phần | Thay đổi |
|---|---|
| Migration mới `DeactivateExtraWarehouses` | `UpdateData` set `is_active=false` cho `id=1` (`KHO01`) và `id=3` (`KHO02`) — `Down()` phục hồi `true`. Không xoá row — chứng từ cũ đã tham chiếu 2 kho này vẫn giữ FK hợp lệ. `UpdateData` cho `id=3` an toàn no-op trên DB nào chưa từng có dòng này (fresh install). |
| `SalesOrderViewModel.cs` (WPF) | `Warehouses` giờ lọc `.Where(w => w.IsActive)` trước khi hiện combobox chọn Kho ở dòng sản phẩm — trước đây không lọc, `KHO01`/`KHO02` vẫn hiện dù đã tắt |
| `SalesReturnViewModel.cs` (WPF) | Tương tự lọc `IsActive`; **thêm mới** hằng số `DefaultWarehouseCode = "HH"` — mặc định Kho khi chọn Mã hàng đổi từ "lấy kho đầu tiên trong danh sách" (trước đây tình cờ là `KHO01`, giờ là `HH`) sang ưu tiên tìm `Code == "HH"` (mirror `SalesOrderViewModel`) |
| `ProductFormViewModel.cs` (WPF, popup "Sửa Vật tư, hàng hoá, dịch vụ") | `Warehouses` (combobox "Kho ngầm định") lọc `IsActive` ở cả 2 chỗ nạp (load ban đầu + reload sau khi bấm "+" thêm Kho mới) — mặc định chọn "HH" cho sản phẩm mới **đã có sẵn từ trước** (`DefaultWarehouseCode = "HH"`, không đổi), chỉ thiếu lọc IsActive nên trước đây `KHO01`/`KHO02` vẫn lọt vào danh sách chọn |

**Không đổi tên Kho** — `HH`/"Hàng hoá" và `TB`/"Trưng bày" giữ nguyên tên, không thêm tiền tố "Kho"
(xác nhận qua `AskUserQuestion`, chỉ là cách gọi tắt trong yêu cầu, không cần đổi dữ liệu thật).

**`WarehouseReceiptFormViewModel` không cần sửa** — không có combobox chọn Kho ở dòng, chỉ tính ngầm
`WarehouseId = product.DefaultWarehouseId ?? 4` (id=4 = HH) khi submit, không hiện danh sách Kho nào
cho user chọn nên không bị ảnh hưởng bởi Kho ngưng hoạt động.

Verify: `dotnet build` (BE) + `dotnet ef database update` áp dụng thành công, `psql` xác nhận
`warehouses.is_active` đúng `f` cho id 1/3, `t` cho id 4/5. `dotnet build -p:EnableWindowsTargeting=true`
(WPF) 0 lỗi. **Chưa test thật trên UTM.**

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
