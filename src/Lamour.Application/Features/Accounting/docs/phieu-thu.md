# Phiếu Thu — BE Documentation

> Feature: Phiếu Thu (Cash Receipt)
> Module: Accounting
> Rebuilt: 2026-04-29 (replaced old PaymentReceipt design)

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `GET`    | `/api/v1/accounting/receipts`       | Bearer | Lấy danh sách phiếu thu |
| `GET`    | `/api/v1/accounting/receipts/{id}`  | Bearer | Lấy phiếu thu theo ID |
| `POST`   | `/api/v1/accounting/receipts`       | Bearer | Tạo phiếu thu mới |
| `PUT`    | `/api/v1/accounting/receipts/{id}`  | Bearer | Cập nhật phiếu thu |
| `DELETE` | `/api/v1/accounting/receipts/{id}`  | Bearer | Xóa phiếu thu |

## Request — POST / PUT

```json
{
  "customer_id": 1,
  "payer_name": "Nguyễn Văn A",
  "address": null,
  "payment_reason": "ThuKhac",
  "collector_employee_id": null,
  "attachment": null,
  "reference": null,
  "accounting_date": "2026-04-29T00:00:00",
  "document_date": "2026-04-29T00:00:00",
  "document_number": "PT00067",
  "entries": [
    {
      "id": 0,
      "description": "Thu tiền khách hàng",
      "debit_account": "Cash111",
      "credit_account": "Receivable131",
      "amount": 1000000,
      "subject_code": null,
      "subject_name": null,
      "bank_account": null
    }
  ]
}
```

**Validation:**
- `customer_id` phải tồn tại → `DomainException` → 400
- `document_number` required
- `payment_reason` phải là enum hợp lệ: `ThuKhac`, `ThuTienHang`, `ThuCongNo`
- `debit_account` / `credit_account` phải là enum hợp lệ: `Cash111`, `Bank112`, `Receivable131`, `Payroll334`

## Response — 201 Created / 200 OK

```json
{
  "id": 1,
  "customer_id": 1,
  "customer_name": "Nguyễn Văn A",
  "payer_name": "Nguyễn Văn A",
  "address": null,
  "payment_reason": "ThuKhac",
  "collector_employee_id": null,
  "collector_employee_name": null,
  "attachment": null,
  "reference": null,
  "accounting_date": "2026-04-29T00:00:00Z",
  "document_date": "2026-04-29T00:00:00Z",
  "document_number": "PT00067",
  "created_at": "2026-04-29T08:00:00Z",
  "entries": [
    {
      "id": 1,
      "description": "Thu tiền khách hàng",
      "debit_account": "Cash111",
      "credit_account": "Receivable131",
      "amount": 1000000,
      "subject_code": null,
      "subject_name": null,
      "bank_account": null
    }
  ]
}
```

## Enums

### PaymentReason
```
ThuKhac      — Thu khác
ThuTienHang  — Thu tiền hàng
ThuCongNo    — Thu công nợ
```

### AccountCode (TK Nợ / TK Có)
```
Cash111        — 111 Tiền mặt
Bank112        — 112 Tiền gửi ngân hàng
Receivable131  — 131 Phải thu khách hàng
Payroll334     — 334 Phải trả người lao động
```

## Số chứng từ

`document_number` là free-text do người dùng nhập thủ công (ví dụ: `PT00067`).
Không có auto-generate hay sequence trên BE.

## Side Effect — CashTransaction (Quỹ Tiền Mặt)

Khi **tạo** hoặc **cập nhật** Receipt, tự động sync 1 row `CashTransaction`:

| Field           | Value                                                        |
|-----------------|--------------------------------------------------------------|
| `AccountingDate`| `receipt.AccountingDate`                                     |
| `DocumentDate`  | `receipt.DocumentDate`                                       |
| `ReceiptNumber` | `receipt.DocumentNumber`                                     |
| `Description`   | `receipt.PayerName` (chỉ tên người nộp, không có prefix)     |
| `Account`       | `"111"` (tiền mặt)                                           |
| `CounterAccount`| TK Có của entry đầu tiên → mapped "111"/"112"/"131"/"334"   |
| `DebitAmount`   | tổng `entries.Sum(e => e.Amount)`                            |
| `CreditAmount`  | `0`                                                          |
| `PersonName`    | `receipt.PayerName`                                          |

**Update flow**: xóa CT cũ theo `DocumentNumber` cũ → tạo CT mới với data mới.
**Delete flow**: xóa CT theo `DocumentNumber` → xóa Receipt.

## Domain Entities

### Receipt

```
src/Lamour.Domain/Entities/Receipt.cs
```

| Column               | Type          | Notes                        |
|----------------------|---------------|------------------------------|
| `Id`                 | int           | PK                           |
| `CustomerId`         | int           | FK → Customers (Restrict)    |
| `PayerName`          | string(200)   | Người nộp                    |
| `Address`            | string?(500)  | Địa chỉ                      |
| `PaymentReason`      | string(30)    | Enum stored as string        |
| `CollectorEmployeeId`| int?          | FK → Employees (SetNull)     |
| `Attachment`         | string?(500)  | Kèm theo                     |
| `Reference`          | string?(200)  | Tham chiếu                   |
| `AccountingDate`     | datetime      | Ngày hạch toán (UTC)         |
| `DocumentDate`       | datetime      | Ngày chứng từ (UTC)          |
| `DocumentNumber`     | string(50)    | Số chứng từ — user input     |
| `CreatedAt`          | datetime      | UTC                          |

### ReceiptEntry

```
src/Lamour.Domain/Entities/ReceiptEntry.cs
```

| Column          | Type         | Notes                             |
|-----------------|--------------|-----------------------------------|
| `Id`            | int          | PK                                |
| `ReceiptId`     | int          | FK → Receipts (Cascade delete)    |
| `Description`   | string(500)  | Diễn giải                         |
| `DebitAccount`  | string(20)   | TK Nợ — enum stored as string     |
| `CreditAccount` | string(20)   | TK Có — enum stored as string     |
| `Amount`        | decimal(18,2)| Số tiền                           |
| `SubjectCode`   | string?(50)  | Đối tượng                         |
| `SubjectName`   | string?(200) | Tên đối tượng                     |
| `BankAccount`   | string?(100) | TK ngân hàng                      |

## Clean Architecture Layers

```
ReceiptsController              GET/POST/PUT/DELETE /api/v1/accounting/receipts
        ↓
IGetReceiptsUseCase             / GetReceiptsUseCase
IGetReceiptByIdUseCase          / GetReceiptByIdUseCase
ICreateReceiptUseCase           / CreateReceiptUseCase  (+ CashTransaction side effect)
IUpdateReceiptUseCase           / UpdateReceiptUseCase
IDeleteReceiptUseCase           / DeleteReceiptUseCase  (+ CashTransaction cleanup)
        ↓
IReceiptRepository              GetAllAsync, GetByIdAsync, GetByIdTrackedAsync
                                AddAsync, UpdateAsync, DeleteAsync
ICashLedgerRepository           AddAsync, DeleteByReceiptNumberAsync
        ↓
ReceiptRepository               EF Core + AppDbContext
CashLedgerRepository            EF Core + AppDbContext
```

## Files

```
src/Lamour.Domain/
  Entities/Receipt.cs
  Entities/ReceiptEntry.cs
  Enums/PaymentReason.cs
  Enums/AccountCode.cs

src/Lamour.Application/Features/Accounting/
  Dtos/ReceiptEntryDto.cs
  Dtos/ReceiptResponseDto.cs
  Dtos/CreateReceiptRequestDto.cs
  Dtos/UpdateReceiptRequestDto.cs
  Repositories/IReceiptRepository.cs
  UseCases/IGetReceiptsUseCase.cs + GetReceiptsUseCase.cs
  UseCases/IGetReceiptByIdUseCase.cs + GetReceiptByIdUseCase.cs
  UseCases/ICreateReceiptUseCase.cs + CreateReceiptUseCase.cs
  UseCases/IUpdateReceiptUseCase.cs + UpdateReceiptUseCase.cs
  UseCases/IDeleteReceiptUseCase.cs + DeleteReceiptUseCase.cs

src/Lamour.Infrastructure/
  Persistence/Configurations/ReceiptConfiguration.cs  (Receipt + ReceiptEntry)
  Repositories/ReceiptRepository.cs
  Migrations/..._RebuildReceipts.cs

src/Lamour.Api/
  Controllers/ReceiptsController.cs  (new)
  Controllers/AccountingController.cs  (trimmed — only GetCashLedger remains)
  Program.cs  (DI updated)
```

## Removed (replaced by this rebuild)

- `PaymentReceipt` entity + `PaymentReceiptLine` entity
- All `PaymentReceipt*` DTOs, UseCases, Repository, Configuration
- DB tables: `payment_receipts`, `payment_receipt_lines` (dropped via `RebuildReceipts` migration)
- Endpoint: `POST /api/v1/accounting/payment-receipts`
- Endpoint: `GET /api/v1/accounting/payment-receipts`
