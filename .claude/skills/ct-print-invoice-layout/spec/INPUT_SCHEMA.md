# Input Schema — ct-print-invoice-layout

## Invocation Syntax

```
/ct-print-invoice-layout
DOCUMENT_TYPE: Phiếu Xuất Kho
```
New print window for a document type that has no `XxxPrintWindow` yet.

```
/ct-print-invoice-layout
TARGET_FILE: Features/HomePage/Warehouse/Views/WarehouseReceiptPrintWindow.xaml.cs
REFERENCE: has-misa-screenshot
```
Fix/match an existing print window's layout — explicit target + reference availability.

```
/ct-print-invoice-layout
```
Minimal — the skill asks which print window and whether a MISA reference exists (see Step 0/1 in [PROMPT.md](PROMPT.md)).

---

## Parameters

| Parameter | Required | Auto-detect | Description |
|-----------|----------|-------------|-------------|
| `DOCUMENT_TYPE` | No | Inferred from user's message | Vietnamese name of the voucher being printed (e.g. "Phiếu Nhập Kho", "Hoá đơn bán hàng") — used to pick the nearest existing print window to copy from |
| `TARGET_FILE` | No | Ask the user, or `Grep` for `PrintWindow.xaml.cs` under the feature folder implied by `DOCUMENT_TYPE` | Path (relative to `desktop-lamour/src/DesktopLamour/`) to the print window being created or fixed |
| `REFERENCE` | No | Ask the user (see Step 1 in PROMPT.md) | `has-misa-screenshot` — user has/will provide a MISA reference image to match pixel-for-pixel; `layout-only` — no MISA reference, just wire up a working print window following the existing pattern |

---

## Auto-Detection Logic

### Nearest existing print window — from `DOCUMENT_TYPE`

| `DOCUMENT_TYPE` keyword | Nearest reference file |
|---|---|
| bán hàng, hoá đơn bán | `Features/HomePage/Sales/Views/SalesOrderPrintWindow.xaml.cs` |
| trả lại, hàng bán bị trả | `Features/HomePage/SalesReturn/Views/SalesReturnPrintWindow.xaml.cs` |
| nhập kho, xuất kho, kho | `Features/HomePage/Warehouse/Views/WarehouseReceiptPrintWindow.xaml.cs` |
| phiếu thu, phiếu chi | Check `Features/HomePage/Accounting/Views/` first — if no `PrintWindow` exists yet, use `WarehouseReceiptPrintWindow.xaml.cs` as the nearest generic voucher-print shape |

If `DOCUMENT_TYPE` matches none of these, `Grep` the whole `desktop-lamour` repo for `*PrintWindow.xaml.cs` and ask the user which is closest.

### `TARGET_FILE` — from feature folder

```bash
find src/DesktopLamour/Features -iname "*PrintWindow.xaml.cs"
```
Match by folder name against the feature implied by `DOCUMENT_TYPE`.

---

## `REFERENCE` Behavior

| Value | Behavior |
|---|---|
| `has-misa-screenshot` | Follow the full MISA-comparison workflow in [PROMPT.md](PROMPT.md) Step 2 — confirm which image is the target vs. current output before editing anything |
| `layout-only` | Skip MISA comparison — just implement the standard shape from [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) using the nearest existing print window as the template |
| not provided | Ask once: "Bạn có ảnh mẫu MISA để so khớp không, hay chỉ cần layout chuẩn theo mẫu 01-VT?" |
