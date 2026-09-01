---
name: ct-print-invoice-layout
description: "Implement or fix a WPF \"In Hoá Đơn\" (invoice/receipt print) screen for desktop-lamour — FlowDocument-based A5 print windows like SalesOrderPrintWindow, SalesReturnPrintWindow, WarehouseReceiptPrintWindow. Use when creating a new print window, or matching an existing one's layout to a MISA reference screenshot (header/title/date-number/table/signature positioning)."
argument-hint: "[DOCUMENT_TYPE: <e.g. Phiếu Xuất Kho>] [TARGET_FILE: <path to XxxPrintWindow.xaml.cs>] [REFERENCE: has-misa-screenshot | layout-only]"
---

# ct-print-invoice-layout — In Hoá Đơn Print Layout Skill

Implement or fix a `FlowDocument`-based A5 print window in `desktop-lamour`, reusing the pattern already proven across `SalesOrderPrintWindow`, `SalesReturnPrintWindow`, and `WarehouseReceiptPrintWindow` — and avoiding the `Table`/`TableCell` runtime bugs and MISA-alignment pitfalls already hit once.

> **Anti-Hallucination:** Verify every class/property against the codebase before generating code. See [ct-anti-hallucination](../ct-anti-hallucination/SKILL.md).
> **Client-only:** all work happens in `desktop-lamour`, never in `be-window-lamour` — a print-layout task never needs a BE change.

---

## How to Use

**New print window for a document type that doesn't have one yet:**
```
/ct-print-invoice-layout
DOCUMENT_TYPE: Phiếu Xuất Kho
```

**Fix/match an existing print window to a MISA reference:**
```
/ct-print-invoice-layout
TARGET_FILE: Features/HomePage/Warehouse/Views/WarehouseReceiptPrintWindow.xaml.cs
REFERENCE: has-misa-screenshot
```

**Minimal — let the skill ask:**
```
/ct-print-invoice-layout
```

All parameters are optional — see [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) for auto-detection when omitted.

---

## File Structure

| File | Purpose |
|------|---------|
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Invocation syntax and parameter reference |
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | Required `FlowDocument` structure and shape |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | `Table`/`TableCell` bugs and MISA-comparison rules already solved once |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

---

## Execution

Load and execute: **[spec/PROMPT.md](spec/PROMPT.md)**

Fallback — if @-references do not resolve, use the Read tool:
```
Read /Users/hai.phan/Desktop/haiphan/be-window-lamour/.claude/skills/ct-print-invoice-layout/spec/PROMPT.md
```
