# Đặt Cọc (Deposit) — Feature Document (BE)

> **Jira:** — | **Branch:** `dev` | **Generated:** 2026-08-09

---

## PRD Summary

> API quản lý tiền cọc khách hàng (Deposit) và trừ cọc (DepositDeduction) cho hệ thống Lamour Spa & Cosmetics.

- **Goal:** Cho phép ghi nhận khoản tiền khách đặt cọc trước, sau đó trừ dần vào các Chứng từ bán hàng (Sales Order) cho tới khi hết số dư, kèm báo cáo theo dõi các lần trừ cọc.
- **User story:** As a Lamour cashier, I want to record a customer deposit and later deduct part of it against a specific sales order, so that the remaining balance is tracked automatically and I have a report of every deduction made.
- **Acceptance criteria:**
  - [x] `GET /api/v1/deposits` trả danh sách tất cả phiếu cọc kèm các lần trừ cọc (deductions)
  - [x] `GET /api/v1/deposits/{id}` trả chi tiết một phiếu cọc
  - [x] `GET /api/v1/deposits/next-code` trả số chứng từ tiếp theo dạng `DC{5 digits}`
  - [x] `GET /api/v1/deposits/by-customer/{customerId}` trả các cọc còn số dư của 1 khách hàng (dùng cho dropdown chọn cọc khi trừ)
  - [x] `POST /api/v1/deposits` tạo mới, `RemainingBalance` khởi tạo = `Amount`, `Status = Active`
  - [x] `PUT /api/v1/deposits/{id}` cập nhật — chỉ cho phép khi chưa bị trừ lần nào
  - [x] `DELETE /api/v1/deposits/{id}` xóa — chỉ cho phép khi chưa bị trừ lần nào
  - [x] `GET /api/v1/deposit-deductions` — **báo cáo đơn trừ cọc**, filter theo `customer_id`/`employee_id`/`sales_order_id`/`from_date`/`to_date`
  - [x] `GET /api/v1/deposit-deductions/{id}` trả chi tiết 1 lần trừ cọc
  - [x] `POST /api/v1/deposit-deductions` tạo lần trừ cọc mới, gắn với 1 Sales Order cụ thể, trừ số dư cọc
  - [x] `DELETE /api/v1/deposit-deductions/{id}` xóa lần trừ cọc, hoàn lại số dư cho cọc

---

## Business Rules

| Rule | Description |
|------|-------------|
| Số chứng từ Deposit | Prefix `DC`, format `DC{5 digits}` — sinh qua `GET /deposits/next-code`, gửi lên khi tạo (giống pattern SalesOrder) |
| Số chứng từ DepositDeduction | Prefix `TC`, format `TC{5 digits}` — **tự sinh ở BE** bên trong `CreateDepositDeductionUseCase` (không cần client generate) |
| Khởi tạo cọc | `Amount > 0` bắt buộc — `DomainException` nếu vi phạm. `RemainingBalance` = `Amount`, `Status = Active` |
| Trừ cọc bắt buộc gắn Sales Order | `DepositDeduction.SalesOrderId` bắt buộc — validate Sales Order tồn tại (`NotFoundException` → 404 nếu không) |
| Guard số dư | `Amount trừ > 0` và `Amount trừ <= Deposit.RemainingBalance` — nếu vượt: `DomainException("Số tiền trừ cọc vượt quá số dư còn lại.")` → 400 |
| Trừ cọc thành công | `Deposit.RemainingBalance -= Amount`; nếu về `0` → `Status = Depleted`, ngược lại giữ/trả về `Active` |
| Xóa lần trừ cọc | Hoàn `Deposit.RemainingBalance += Amount`, `Status = Active` (đã hoàn nên chắc chắn > 0), rồi xóa dòng `DepositDeduction` |
| Sửa/Xóa Deposit header | Chỉ cho phép khi **chưa bị trừ lần nào** (`RemainingBalance == Amount`) — ngược lại `DomainException("Cọc đã bị trừ, không thể sửa/xóa.")` |
| Không đụng Quỹ Tiền Mặt | Toàn bộ feature này **không** tạo/sửa/xóa bất kỳ dòng `CashTransaction` nào — tiền đã được ghi nhận vào quỹ tại thời điểm tạo cọc (ngoài phạm vi feature này), trừ cọc chỉ là điều chỉnh số dư nội bộ |
| Không đụng tồn kho | Deposit/DepositDeduction hoàn toàn không liên quan `Product`/`StockQuantity` |
| DB Transaction | `CreateDepositDeductionUseCase` và `DeleteDepositDeductionUseCase` dùng `IUnitOfWork.BeginAsync` → `CommitAsync`/`RollbackAsync` (2 thao tác ghi: deduction row + deposit balance update phải atomic) |
| DateTime UTC | Lưu `DateTime.UtcNow` / `DateTime.SpecifyKind(..., Utc)`, WPF convert sang local time khi hiển thị |

---

## Architecture Overview

### Key Components

| Layer | File | Role |
|-------|------|------|
| Controller | `Lamour.Api/Controllers/DepositsController.cs` | HTTP entry point — 7 actions |
| Controller | `Lamour.Api/Controllers/DepositDeductionsController.cs` | HTTP entry point — 4 actions (báo cáo + CRUD trừ cọc) |
| UseCase | `UseCases/GetDepositsUseCase.cs` | Fetch tất cả cọc; chứa `internal static MapToDto()` dùng chung |
| UseCase | `UseCases/GetDepositByIdUseCase.cs` | Fetch 1 cọc theo id |
| UseCase | `UseCases/GetNextDepositCodeUseCase.cs` | Trả số chứng từ tiếp theo `DC{5}` |
| UseCase | `UseCases/GetDepositsByCustomerUseCase.cs` | Cọc còn số dư của 1 khách hàng (cho WPF dropdown) |
| UseCase | `UseCases/CreateDepositUseCase.cs` | Validate `Amount > 0` → tạo cọc, `RemainingBalance = Amount` |
| UseCase | `UseCases/UpdateDepositUseCase.cs` | Guard chưa bị trừ → cập nhật header |
| UseCase | `UseCases/DeleteDepositUseCase.cs` | Guard chưa bị trừ → xóa |
| UseCase | `UseCases/GetDepositDeductionsUseCase.cs` | Báo cáo — nhận filter → `IDepositDeductionRepository.GetAllAsync` → map; chứa `internal static MapToDto()` dùng chung |
| UseCase | `UseCases/GetDepositDeductionByIdUseCase.cs` | Fetch 1 lần trừ cọc |
| UseCase | `UseCases/CreateDepositDeductionUseCase.cs` | Validate Deposit + SalesOrder tồn tại → guard số dư → `IUnitOfWork` transaction → tạo deduction + trừ balance |
| UseCase | `UseCases/DeleteDepositDeductionUseCase.cs` | `IUnitOfWork` transaction → hoàn balance → xóa deduction |
| Repository | `Repositories/IDepositRepository.cs` / `IDepositDeductionRepository.cs` | Data access contract |
| Repository | `Lamour.Infrastructure/Repositories/DepositRepository.cs` / `DepositDeductionRepository.cs` | EF Core implementation |
| Entity | `Lamour.Domain/Entities/Deposit.cs` | Domain model + `DepositStatus` enum |
| Entity | `Lamour.Domain/Entities/DepositDeduction.cs` | Domain model, FK → `Deposit` + `SalesOrder` |
| Config | `Lamour.Infrastructure/Persistence/Configurations/DepositConfiguration.cs` / `DepositDeductionConfiguration.cs` | EF table mapping |

### Data Flow (Create Deduction)

```
HTTP POST /api/v1/deposit-deductions
  → DepositDeductionsController.Create
  → ICreateDepositDeductionUseCase.ExecuteAsync()
  → IDepositRepository.GetByIdTrackedAsync(deposit_id)     ← 404 nếu không có
  → ISalesOrderRepository.GetByIdAsync(sales_order_id)     ← 404 nếu không có
  → guard: amount > 0 && amount <= RemainingBalance         ← 400 nếu vi phạm
  → IUnitOfWork.BeginAsync()
  → IDepositDeductionRepository.AddAsync(deduction)          ← insert deduction row
  → deposit.RemainingBalance -= amount; Status update
  → IDepositRepository.UpdateAsync(deposit)                  ← update deposit row
  → IUnitOfWork.CommitAsync()
  ← DepositDeductionResponseDto
```

```mermaid
graph TD
    A[DepositsController] --> B[GetDepositsUseCase]
    A --> BB[GetDepositByIdUseCase]
    A --> C[CreateDepositUseCase]
    A --> D[UpdateDepositUseCase]
    A --> E[DeleteDepositUseCase]
    A --> N[GetNextDepositCodeUseCase]
    A --> BC[GetDepositsByCustomerUseCase]
    B --> H[IDepositRepository]
    BB --> H
    C --> H
    D --> H
    E --> H
    N --> H
    BC --> H
    H --> I[AppDbContext / PostgreSQL]

    A2[DepositDeductionsController] --> R[GetDepositDeductionsUseCase]
    A2 --> RB[GetDepositDeductionByIdUseCase]
    A2 --> CR[CreateDepositDeductionUseCase]
    A2 --> DR[DeleteDepositDeductionUseCase]
    R --> H2[IDepositDeductionRepository]
    RB --> H2
    CR --> H2
    CR --> H
    CR --> SO[ISalesOrderRepository]
    CR --> U[IUnitOfWork]
    DR --> H2
    DR --> H
    DR --> U
    H2 --> I
```

---

## API Contracts

| Method | Endpoint | Input | Output |
|--------|----------|-------|--------|
| `GET` | `/api/v1/deposits` | — | `DepositResponseDto[]` |
| `GET` | `/api/v1/deposits/{id}` | — | `DepositResponseDto` (200) / 404 |
| `GET` | `/api/v1/deposits/next-code` | — | `{ "code": "DC00001" }` (200) |
| `GET` | `/api/v1/deposits/by-customer/{customerId}` | — | `DepositResponseDto[]` (chỉ cọc `RemainingBalance > 0`) |
| `POST` | `/api/v1/deposits` | `CreateDepositRequestDto` | `DepositResponseDto` (201) |
| `PUT` | `/api/v1/deposits/{id}` | `UpdateDepositRequestDto` | `DepositResponseDto` (200) / 400 nếu đã bị trừ / 404 |
| `DELETE` | `/api/v1/deposits/{id}` | — | 204 / 400 nếu đã bị trừ / 404 |
| `GET` | `/api/v1/deposit-deductions?customer_id=&employee_id=&sales_order_id=&from_date=&to_date=` | — (query only) | `DepositDeductionResponseDto[]` (200) — **báo cáo đơn trừ cọc** |
| `GET` | `/api/v1/deposit-deductions/{id}` | — | `DepositDeductionResponseDto` (200) / 404 |
| `POST` | `/api/v1/deposit-deductions` | `CreateDepositDeductionRequestDto` | `DepositDeductionResponseDto` (201) / 400 (vượt số dư) / 404 |
| `DELETE` | `/api/v1/deposit-deductions/{id}` | — | 204 / 404 |

### Request — Create Deposit

```json
{
  "document_number": "DC00001",
  "accounting_date": "2026-08-09T00:00:00",
  "document_date": "2026-08-09T00:00:00",
  "customer_id": 1,
  "employee_id": 2,
  "description": "Khách cọc mua mỹ phẩm",
  "reference": null,
  "amount": 30000000
}
```

### Response — Deposit (201/200)

```json
{
  "id": 1,
  "document_number": "DC00001",
  "accounting_date": "2026-08-09T00:00:00Z",
  "document_date": "2026-08-09T00:00:00Z",
  "customer_id": 1,
  "customer_name": "Nguyễn Văn A",
  "employee_id": 2,
  "employee_name": "Trần Thị B",
  "description": "Khách cọc mua mỹ phẩm",
  "reference": null,
  "amount": 30000000,
  "remaining_balance": 30000000,
  "status": 0,
  "created_at": "2026-08-09T08:00:00Z",
  "deductions": []
}
```

### Request — Create DepositDeduction

```json
{
  "deposit_id": 1,
  "sales_order_id": 5,
  "amount": 20000000,
  "accounting_date": "2026-08-09T00:00:00",
  "document_date": "2026-08-09T00:00:00",
  "description": "Trừ cọc thanh toán đơn BC00005"
}
```

### Response — DepositDeduction (201/200)

```json
{
  "id": 1,
  "document_number": "TC00001",
  "deposit_id": 1,
  "deposit_document_number": "DC00001",
  "sales_order_id": 5,
  "sales_order_document_number": "BC00005",
  "customer_id": 1,
  "customer_name": "Nguyễn Văn A",
  "employee_id": 2,
  "employee_name": "Trần Thị B",
  "amount": 20000000,
  "accounting_date": "2026-08-09T00:00:00Z",
  "document_date": "2026-08-09T00:00:00Z",
  "description": "Trừ cọc thanh toán đơn BC00005",
  "created_at": "2026-08-09T08:00:00Z"
}
```

Sau lần trừ cọc trên, `GET /api/v1/deposits/1` sẽ trả `remaining_balance: 10000000`, `status: 0` (Active).

## Enums

### DepositStatus
```
Active   = 0  — Còn số dư
Depleted = 1  — Đã trừ hết (RemainingBalance == 0)
```

---

## EF Migration

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef migrations add AddDeposits \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

Tables created: `deposits`, `deposit_deductions` (FK `deposit_id` và `sales_order_id` đều `ON DELETE RESTRICT` — xóa `Deposit`/`SalesOrder` khi còn deduction tham chiếu sẽ lỗi ở DB level, ngoài guard ở UseCase layer).

---

## DI Registration (`Program.cs`)

```csharp
// ── Deposits DI ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDepositRepository, DepositRepository>();
builder.Services.AddScoped<IDepositDeductionRepository, DepositDeductionRepository>();
builder.Services.AddScoped<IGetDepositsUseCase, GetDepositsUseCase>();
builder.Services.AddScoped<IGetDepositByIdUseCase, GetDepositByIdUseCase>();
builder.Services.AddScoped<IGetNextDepositCodeUseCase, GetNextDepositCodeUseCase>();
builder.Services.AddScoped<IGetDepositsByCustomerUseCase, GetDepositsByCustomerUseCase>();
builder.Services.AddScoped<ICreateDepositUseCase, CreateDepositUseCase>();
builder.Services.AddScoped<IUpdateDepositUseCase, UpdateDepositUseCase>();
builder.Services.AddScoped<IDeleteDepositUseCase, DeleteDepositUseCase>();
builder.Services.AddScoped<IGetDepositDeductionsUseCase, GetDepositDeductionsUseCase>();
builder.Services.AddScoped<IGetDepositDeductionByIdUseCase, GetDepositDeductionByIdUseCase>();
builder.Services.AddScoped<ICreateDepositDeductionUseCase, CreateDepositDeductionUseCase>();
builder.Services.AddScoped<IDeleteDepositDeductionUseCase, DeleteDepositDeductionUseCase>();
```

`IUnitOfWork` đã có sẵn DI registration dùng chung với Sales/SalesReturn.

---

## Notes

- `[Authorize]` bật trên cả 2 controller — WPF cần gửi Bearer JWT
- `DepositDeduction.DocumentNumber` (`TC{5}`) tự sinh ở BE trong `CreateDepositDeductionUseCase` — khác với `Deposit.DocumentNumber` (`DC{5}`) sinh qua endpoint `next-code` rồi client gửi lên khi tạo (giống pattern SalesOrder `BC{5}`)
- `GET /api/v1/deposit-deductions` là báo cáo cấp DÒNG (mỗi dòng = 1 lần trừ cọc), filter kết hợp AND, tất cả optional
- `GET /api/v1/deposits/by-customer/{customerId}` chỉ trả cọc có `RemainingBalance > 0` — dùng để WPF hiển thị dropdown "chọn cọc để trừ" trong popup Sales Order
- **WPF integration (2026-08-09, không đổi BE):** `POST /api/v1/deposit-deductions` được gọi từ `SalesOrderWindow` (Chứng từ bán hàng) — mỗi cọc còn số dư hiện như 1 lựa chọn "sản phẩm ảo" trong dropdown Mã hàng/Tên hàng (`DepositProductPickerItem`), chọn thủ công qua "+ Thêm dòng" giống chọn 1 sản phẩm thật; gọi API này SAU KHI Sales Order đã lưu thành công (`SalesOrderId` truyền vào là id đơn vừa tạo). Xem chi tiết phía client tại `desktop-lamour/.../Sales/docs/sales.md` (mục "Trừ cọc", updated 2026-08-09).

---

*Generated 2026-08-09*
*Updated 2026-08-09: thêm ghi chú tích hợp phía WPF client (Sales Order) — không có thay đổi contract/code nào ở BE.*
