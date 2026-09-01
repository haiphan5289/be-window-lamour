# Output Schema — ct-print-invoice-layout

## Print Window File Shape

Every print window is a pair of files, following `SalesOrderPrintWindow`/`SalesReturnPrintWindow`/`WarehouseReceiptPrintWindow`:

| File | Contents |
|---|---|
| `XxxPrintWindow.xaml` | Just a `DocumentViewer` + a small toolbar (Print / Close buttons). No layout logic here. |
| `XxxPrintWindow.xaml.cs` | `Initialize(dto, ...)` sets `DocumentViewer.Document = BuildDocument(...)`. All real layout lives in the static `BuildDocument(...)` method, returning a `FlowDocument`. |

Do not put layout logic in XAML — copy the existing `.xaml` almost verbatim, only changing the window title/class name.

---

## `FlowDocument` Structure (must contain, in this order)

```
FlowDocument (A5 page size, White background, PagePadding ~16)
└── outer frame Table (1 column, 1 cell, bordered ~1.2, padding ~14) — the visible receipt border
    ├── header paragraph
    │     Floater(logo image, ~120x40, left-aligned) + Bold company name (FontSize 13)
    │     + address line + tax-code line + tel/website line [+ bank account line]
    ├── title + Ngày/Số/Nợ/Có block — ONE shared 3-column Table (see GUARDRAILS Rule 2):
    │     col0 = spacer | col1 = title/Ngày.../Số: (centered) | col2 = Nợ:/Có: (right-aligned, padded)
    │     row0: title only in col1, empty cells in col0/col2
    │     row1: "Ngày {d} tháng {M} năm {y}" (italic, centered) in col1 | "Nợ: {debitAccount}" in col2
    │     row2: "Số: {DocumentNumber}" (bold, red) in col1 | "Có: {creditAccount}" in col2
    ├── general-info paragraphs, each "- Label: value", full content width, left-aligned:
    │     "- Họ và tên người giao/nhận: ..."
    │     "- Địa chỉ: ..."
    │     "- Diễn giải: ..."
    │     "- Theo {reference} ngày {d} tháng {M} năm {y} của {partner}"
    ├── warehouse/địa điểm row (2-column table, full width): "- Nhập tại kho: {warehouse}" | "Địa điểm: "
    ├── line-items Table — column widths sum to the page's content width budget:
    │     header row 1: STT | Mã hàng | Tên hàng | Mã quy cách | ĐVT | Số lượng | Đơn giá | Thành tiền
    │     header row 2 (MANDATORY, see GUARDRAILS Rule 5): A | B | C | D | E | 1 | 2 | 3
    │     data rows: one per line, product name left-aligned, everything else centered
    │     "Cộng" row: label spans all columns but the last, total right-aligned in the last column
    ├── "- Tổng số tiền (Viết bằng chữ): {amountInWords, blank if 0 — see GUARDRAILS Rule 4}"
    ├── "- Số chứng từ gốc kèm theo: "
    ├── "Ngày ..... tháng ..... năm ........." (italic, right-aligned)
    ├── signature Table — 4 columns, one TableRow, MANDATORY 4 roles (see GUARDRAILS Rule 6):
    │     bold role label + LineBreak + italic "(Ký, họ tên)" (FontSize 10), TextAlignment Center
    └── spacer BlockUIContainer, MinHeight = A5PageHeight - EstimateContentHeight(lineCount)
          — pushes the footer toward a consistent vertical position regardless of line-item count
```

---

## Required Constants (copy from the nearest existing print window)

```csharp
private const double MmToDip = 96.0 / 25.4;
private static readonly double A5PageWidth  = 148 * MmToDip;
private static readonly double A5PageHeight = 210 * MmToDip;

// Column width budget for the line-items table — verify the sum against the actual content
// width (A5PageWidth minus PagePadding, frame border, and frame cell padding) before reusing
// for a document type with a different column set.
private static readonly int[] ColumnWidths = { /* ... */ };

private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));
```

`EstimateContentHeight(int lineCount)` must be updated whenever blocks are added/removed from `BuildDocument`, so the spacer keeps the footer roughly where it was before.

---

## Completion Block

Print this after the print window is implemented/fixed and the build passes:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-print-invoice-layout COMPLETE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

File:        <TARGET_FILE>
Build:       dotnet build — 0 errors
Artifacts:   bin/obj restored + cleaned (git status verified)
Verified on UTM: <yes — describe what was confirmed | not yet — ask user to sync + screenshot>

⚠️  A clean build does NOT catch FlowDocument runtime bugs (Table/TableCell ownership,
    layout misalignment) — real verification only happens on the UTM Windows VM.
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

Never claim a print-layout task is "done" on the strength of the build alone — always caveat that UTM verification (or a user-provided screenshot) is the real check.
