# Phiếu Thu — BE Documentation

> Feature: Thu tiền khách hàng (Payment Receipt)
> Module: Accounting
> Implemented: 2026-04-26

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/v1/accounting/payment-receipts` | Bearer | Tạo phiếu thu mới |
| `GET`  | `/api/v1/accounting/payment-receipts` | Bearer | Lấy danh sách phiếu thu |

## Request — POST

```json
{
  "customer_id": 1,
  "employee_id": 3,
  "collection_date": "2023-11-16T00:00:00",
  "total_amount": 1000000,
  "payment_method": "Cash",
  "currency": "VND",
  "exchange_rate": 1.00,
  "lines": [
    {
      "document_date": "2023-11-16T00:00:00",
      "document_number": "PT001",
      "invoice_number": "XK001",
      "description": "Thu tiền hóa đơn tháng 11",
      "due_date": "2023-12-16T00:00:00",
      "amount_due": 1000000,
      "amount_paid": 1000000
    }
  ]
}
```

**Validation:**
- `customer_id` phải tồn tại trong DB → `DomainException` nếu không tìm thấy → 400
- `total_amount` phải > 0 → `DomainException` → 400
- `payment_method` phải là `"Cash"` hoặc `"BankTransfer"` → `DomainException` → 400

## Response — 201 Created

```json
{
  "id": 1,
  "receipt_number": "PT-20231116-001",
  "customer_id": 1,
  "customer_name": "CHI NHI",
  "employee_id": 3,
  "employee_name": "Nguyễn Văn A",
  "collection_date": "2023-11-16T00:00:00",
  "total_amount": 1000000,
  "payment_method": "Cash",
  "currency": "VND",
  "exchange_rate": 1.00,
  "created_at": "2026-04-26T08:20:13Z"
}
```

## Receipt Number Format

```
PT-{yyyyMMdd}-{seq:D3}
```

- `seq` = số phiếu thu trong ngày, bắt đầu từ 001
- Ví dụ: `PT-20231116-001`, `PT-20231116-002`

## Side Effect — CashTransaction

Khi tạo thành công, tự động tạo 1 row `CashTransaction`:

| Field | Value |
|-------|-------|
| `AccountingDate` | `CollectionDate` |
| `DocumentDate` | `CollectionDate` |
| `ReceiptNumber` | PT number |
| `Account` | `"111"` (tiền mặt) |
| `CounterAccount` | `"131"` (phải thu khách hàng) |
| `DebitAmount` | `TotalAmount` |
| `CreditAmount` | `0` |
| `Description` | `"Thu tiền khách hàng {CustomerName}"` |
| `PersonName` | `Employee.Name` khi có employee; fallback `Customer.Name` |

## Domain Entities

### PaymentReceipt

```
src/Lamour.Domain/Entities/PaymentReceipt.cs
```

| Column | Type | Notes |
|--------|------|-------|
| `Id` | int | PK |
| `ReceiptNumber` | string(20) | PT-YYYYMMDD-NNN |
| `CustomerId` | int | FK → Customers |
| `EmployeeId` | int? | FK → Employees (nullable) |
| `CollectionDate` | datetime | Ngày thu tiền |
| `TotalAmount` | decimal | Số tiền |
| `PaymentMethod` | string(20) | Cash / BankTransfer |
| `Currency` | string(10) | VND |
| `ExchangeRate` | decimal | 1.00 for VND |
| `CreatedAt` | datetime | UTC |

### PaymentReceiptLine

```
src/Lamour.Domain/Entities/PaymentReceiptLine.cs
```

| Column | Type | Notes |
|--------|------|-------|
| `Id` | int | PK |
| `PaymentReceiptId` | int | FK → PaymentReceipts (cascade delete) |
| `DocumentDate` | datetime | Ngày chứng từ |
| `DocumentNumber` | string | Số chứng từ |
| `InvoiceNumber` | string | Số hóa đơn (string ref, no FK yet) |
| `Description` | string | Diễn giải |
| `DueDate` | datetime? | Hạn thanh toán |
| `AmountDue` | decimal | Số phải thu |
| `AmountPaid` | decimal | Số thanh toán |

## Clean Architecture Layers

```
PaymentReceiptsController  (AccountingController)
        ↓
ICreatePaymentReceiptUseCase / CreatePaymentReceiptUseCase
IGetPaymentReceiptsUseCase  / GetPaymentReceiptsUseCase
        ↓
IPaymentReceiptRepository   (+ ICashLedgerRepository.AddAsync)
ICustomerRepository         (validate customer exists)
        ↓
PaymentReceiptRepository    EF Core + AppDbContext
CashLedgerRepository        EF Core + AppDbContext
```

## Files

```
src/Lamour.Domain/Entities/
  PaymentReceipt.cs
  PaymentReceiptLine.cs

src/Lamour.Application/Features/Accounting/
  Dtos/CreatePaymentReceiptRequestDto.cs
  Dtos/CreatePaymentReceiptLineDto.cs
  Dtos/PaymentReceiptResponseDto.cs
  Dtos/PaymentReceiptLineDto.cs
  Repositories/IPaymentReceiptRepository.cs
  UseCases/ICreatePaymentReceiptUseCase.cs
  UseCases/CreatePaymentReceiptUseCase.cs
  UseCases/IGetPaymentReceiptsUseCase.cs
  UseCases/GetPaymentReceiptsUseCase.cs

src/Lamour.Infrastructure/
  Persistence/Configurations/PaymentReceiptConfiguration.cs
  Repositories/PaymentReceiptRepository.cs
  Migrations/20260426082013_AddPaymentReceipts.cs

src/Lamour.Api/
  Controllers/AccountingController.cs  (updated)
  Program.cs                            (updated — DI registered)
```

## Known Limitations / Future Work

- `invoice_number` trong `PaymentReceiptLine` là string reference — chưa có FK tới `ExportInvoice`
- Khi `ExportInvoice` được implement, cần: thêm FK + update `paid_amount` trên ExportInvoice khi tạo phiếu thu
- Chưa có "Lấy dữ liệu" (auto-load outstanding invoices for customer)
