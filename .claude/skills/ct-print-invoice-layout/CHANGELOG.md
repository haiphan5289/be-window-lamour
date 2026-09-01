# Changelog — ct-print-invoice-layout

## v1.0.0 — 2026-08-31

### Added
- Initial release, extracted from the real fix session on `WarehouseReceiptPrintWindow` (Phiếu Nhập Kho print layout matched to MISA reference)
- File structure split per `ct-ai-document` convention: thin `SKILL.md` router + `spec/INPUT_SCHEMA.md` + `spec/PROMPT.md` + `spec/OUTPUT_SCHEMA.md` + `spec/GUARDRAILS.md`
- Documented the `FlowDocument`/`Table` shared shape across `SalesOrderPrintWindow`, `SalesReturnPrintWindow`, `WarehouseReceiptPrintWindow`
- Guardrail: never reuse a `TableCell` across `TableRow`s (root cause of the "Item belongs to another collection" runtime crash)
- Guardrail: title + date/number + Nợ/Có must share one table's columns to guarantee vertical alignment, instead of mixing a full-width centered `Paragraph` with a separately-centered `Table` column
- Guardrail: reserve right-padding on right-flush cells to avoid clipping against the outer frame border
- Guardrail: blank instead of auto-filled placeholder for zero/empty derived fields (e.g. "Tổng số tiền" left blank when `TotalAmount == 0`)
- Guardrail: mandatory A/B/C/.../1/2/3 mẫu 01-VT symbol row, mandatory 4-role signature block
- MISA-comparison workflow: confirm which screenshot is the target vs. current output before editing, ask for the reference image rather than guessing from memory of the standard template
- Explicit reminder that a clean `dotnet build` never catches FlowDocument runtime layout bugs — only UTM verification does
