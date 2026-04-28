# Bug Tracking — Lamour System

> Tracks bugs found and fixed across `be-window-lamour` (BE) and `desktop-lamour` (WPF).
> Format: `[Date] | Severity | Status | Module`

---

## BUG-001 — Tổng hợp tồn kho không hiển thị sản phẩm inactive

| Field | Value |
|-------|-------|
| **Date** | 2026-04-28 |
| **Severity** | Low |
| **Status** | ✅ Fixed |
| **Project** | BE (`be-window-lamour`) |
| **Module** | Warehouse / Tổng hợp tồn kho |

### Symptom
Màn hình Tổng hợp tồn kho (`TongHopTonKhoView`) không hiển thị sản phẩm có `is_active = false`.

### Root Cause
`IInventoryRepository.GetAllActiveAsync()` filter `WHERE p.is_active = true` → loại bỏ sản phẩm inactive.
`GetInventorySummaryUseCase` gọi `GetAllActiveAsync` thay vì `GetAllAsync`.

### Fix
**File:** `src/Lamour.Application/Features/Warehouse/Repositories/IInventoryRepository.cs`
- Thêm method `GetAllAsync()` (không filter `is_active`).

**File:** `src/Lamour.Infrastructure/Repositories/InventoryRepository.cs`
- Implement `GetAllAsync()`: query không có `WHERE p.is_active`.

**File:** `src/Lamour.Application/Features/Warehouse/UseCases/GetInventorySummaryUseCase.cs`
- Đổi `GetAllActiveAsync(ct)` → `GetAllAsync(ct)`.

---

## BUG-002 — Nút "Quay lại" từ WarehouseView không điều hướng về Home

| Field | Value |
|-------|-------|
| **Date** | 2026-04-28 |
| **Severity** | Medium |
| **Status** | ✅ Fixed |
| **Project** | App (`desktop-lamour`) |
| **Module** | Navigation / WarehouseView |

### Symptom
Sau khi login và vào **Home → Kho**, nhấn `← Quay lại` trên `WarehouseView` không trở về Home.
Hành vi sai: GoBack pop "LoginView" từ backStack → redirect về LoginView.

### Root Cause
`LoginViewModel.LoginAsync` dùng `NavigateTo(NavigationRoutes.Main)`:
- Push "LoginView" vào `_backStack`.
- Sau đó từ Home → Warehouse: push "MainView" vào stack.
- GoBack từ Warehouse → pop "MainView" → đúng.
- GoBack lần 2 từ Home → pop "LoginView" → sai: quay về Login thay vì dừng.

Vấn đề cốt lõi: Login vẫn tồn tại trong back stack sau khi đã login xong.

### Fix
**File:** `src/DesktopLamour/Core/Navigation/INavigationService.cs`
- Thêm method `NavigateToHome()`.

**File:** `src/DesktopLamour/Core/Navigation/NavigationService.cs`
- Implement `NavigateToHome()`: clear toàn bộ `_backStack`, set `_currentView = "HomeView"`, navigate tới `HomeView`.

**File:** `src/DesktopLamour/Features/Authentication/ViewModels/LoginViewModel.cs`
- Đổi `NavigateTo(NavigationRoutes.Main)` → `NavigateToHome()` sau khi login thành công.
- LoginView không còn tồn tại trong backStack.

---

## BUG-003 — Thiếu nút 🏠 Trang chủ trên WarehouseView và TongHopTonKhoView

| Field | Value |
|-------|-------|
| **Date** | 2026-04-28 |
| **Severity** | Low |
| **Status** | ✅ Fixed |
| **Project** | App (`desktop-lamour`) |
| **Module** | Warehouse / Navigation |

### Symptom
Không có nút để về thẳng Trang chủ từ `WarehouseView` và `TongHopTonKhoView`. Người dùng phải nhấn "Quay lại" nhiều lần.

### Root Cause
Tính năng chưa được implement.

### Fix
**File:** `src/DesktopLamour/Features/HomePage/Warehouse/ViewModels/WarehouseViewModel.cs`
- Thêm `[RelayCommand] NavigateToHome()` → gọi `_navigationService.NavigateToHome()`.

**File:** `src/DesktopLamour/Features/HomePage/Warehouse/ViewModels/TongHopTonKhoViewModel.cs`
- Thêm `[RelayCommand] NavigateToHome()` → gọi `_navigationService.NavigateToHome()`.

**File:** `src/DesktopLamour/Features/HomePage/Warehouse/Views/WarehouseView.xaml`
**File:** `src/DesktopLamour/Features/HomePage/Warehouse/Views/TongHopTonKhoView.xaml`
- Thêm button `🏠 Trang chủ` cạnh `← Quay lại` trong page header.

---

## BUG-004 — Phiếu Nhập Kho lỗi 400 khi chọn sản phẩm inactive

| Field | Value |
|-------|-------|
| **Date** | 2026-04-28 |
| **Severity** | High |
| **Status** | ✅ Fixed |
| **Project** | App (`desktop-lamour`) |
| **Module** | Warehouse / Phiếu Nhập Kho |

### Symptom
Nhấn **Lưu nhập** trên form `Phiếu Nhập Kho` → lỗi:
> `Response status code does not indicate success: 400 (Bad Request).`

### Root Cause
`WarehouseReceiptFormViewModel.LoadAsync` nạp toàn bộ sản phẩm từ `GET /api/v1/products` (không filter `is_active`).
Người dùng chọn sản phẩm inactive (vd: "banh xe — 2222").
BE `CreateWarehouseReceiptUseCase` (line 62–63) validate:
```csharp
if (!product.IsActive)
    throw new DomainException("Hàng hóa đã ngưng kinh doanh và không thể nhập kho.");
```
→ throw `DomainException` → `GlobalExceptionHandler` trả về 400.

### Fix
**File:** `src/DesktopLamour/Features/HomePage/Warehouse/ViewModels/WarehouseReceiptFormViewModel.cs` (line 78)

```csharp
// Before
Products = products.Select(p => (ISearchableItem)new WarehouseProductItem(p)).ToList().AsReadOnly();

// After
Products = products.Where(p => p.IsActive).Select(p => (ISearchableItem)new WarehouseProductItem(p)).ToList().AsReadOnly();
```

Dropdown Phiếu Nhập Kho chỉ hiển thị sản phẩm đang kinh doanh (`is_active = true`).

---

## Backlog — Known Issues (Chưa Fix)

| ID | Module | Issue | Priority |
|----|--------|-------|----------|
| BUG-005 | TongHopTonKho | `FromDate > ToDate` không validate phía WPF — BE trả rỗng nhưng không có warning | Low |
| BUG-006 | Navigation | `ResolveView` trả `null` nếu route không có trong switch → blank screen, không có error message | Low |
| BUG-007 | Navigation | GoBack từ Home pop về LoginView (nếu user navigate nhiều lần) — đã giảm thiểu bằng `NavigateToHome()` | Low |

---

*Maintained by: haiphan | Last updated: 2026-04-28*
