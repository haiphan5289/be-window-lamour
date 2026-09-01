# Guardrails — ct-print-invoice-layout

> These rules were each learned from a real runtime bug or a real back-and-forth with the user this session. Read them before writing any `FlowDocument`/`Table` code.

---

## Core Rules

### 1. Never reuse a `TableCell` instance across two `TableRow`s

A `TableCell` already added to one row's `Cells` is owned (logical-parent) by that row. Adding the same instance to another row's `Cells` throws:
```
ArgumentException: "Item belongs to another collection currently. Item must be removed first."
```
This only throws at **render/pagination time**, not compile time — a `dotnet build` with 0 errors does **not** catch it. It only surfaces once the document actually renders in `DocumentViewer` or prints, which in this project only happens on the UTM Windows VM.

- Always construct **every** row with its own **freshly-created** `TableCell`s.
- Even a visually-identical empty spacer cell needs its own `new TableCell(new Paragraph())` per row — never hoist one instance into a variable and reuse it across rows.
- If you see a helper like `CombineRow(TableCell left, TableCell right)` that takes cells from one row and re-adds them to another — that is the anti-pattern. Delete it and construct each row directly.

### 2. Centered lines that must align with an adjacent right-flush block go in the SAME table, SAME columns

Symptom: a title line (`Paragraph { TextAlignment = Center }`, full content width) and a "Ngày.../Số:" line (centered inside one column of a separate, narrower table) look almost — but not quite — aligned on the same vertical axis.

Root cause: the two use different centering math. A standalone paragraph centers on the *actual* rendered content width; a table column centers on *that column's* width, which only lines up with the paragraph's center if your hand-estimated "total content width" constant (after page padding + frame border + cell padding) is exactly right. It rarely is.

**Fix:** put every line that must share a vertical axis with a right-flush block into **one** table with **one** set of column definitions:
```
[ spacer column ] [ center-content column ] [ right-content column ]
```
Title, "Ngày...", and "Số:" all go in the *same* center column (one `TableRow` each, `ColumnSpan = 1`, never spanning into the spacer or right column). This guarantees they share the exact same center **by construction**, regardless of what the page's true content width turns out to be — no width arithmetic to get right.

### 3. Reserve right-padding on any right-flush cell

Don't size a right column so its right edge lands exactly on your estimated content-width boundary — text clips against the outer frame border (e.g. "Nợ: 1561" rendering as "Nợ: 156" with the last digit cut off). Either:
- Shrink the column ~10-15% under the naive full-width calculation, and/or
- Add `Padding = new Thickness(0, 0, 10, 0)` on that cell.

Do both when in doubt — there's no downside to a little extra breathing room from the border.

### 4. Leave a field blank instead of auto-filling a "zero"/default placeholder

E.g. "Tổng số tiền (Viết bằng chữ)" must stay **empty** when `TotalAmount == 0`, not print "Không đồng" — MISA leaves such fields blank for hand-fill on drafts/not-yet-final documents. Apply the same principle to any derived-text field: check for the "no real value yet" case (`== 0`, `null`, empty string) before formatting, and render `""` instead of a computed default.

### 5. The A/B/C/.../1/2/3 symbol row is mandatory, not optional

The second header row of the line-items table (`A B C D E 1 2 3` under `STT Mã hàng Tên hàng ...`) is part of the official mẫu 01-VT template (Thông tư 200/2014/TT-BTC) — always include it, it is not a data column invented for this app.

### 6. Signature block is always 4 roles with an italic note

Bold role label + `(Ký, họ tên)` in italic on the line below via `new LineBreak()`. Role names vary slightly by document type (Người lập phiếu / Người giao hàng or Người nhận hàng / Thủ kho / Kế toán trưởng or Nhân viên giao hàng) — copy the nearest existing print window's exact role set for the same document family, only renaming what the new type genuinely needs.

---

## Common Pitfalls

### "It builds with 0 errors, so it's done"

A clean `dotnet build` only catches compile errors. `Table`/`TableCell` ownership violations (Rule 1) and layout misalignment (Rule 2) are **runtime** FlowDocument issues that only show up when the document actually paginates — which requires opening the print window on the UTM Windows VM. Never tell the user a print-layout fix is verified based on the build alone.

### Confusing which screenshot is which during a MISA-comparison round

When the user attaches a screenshot and says "chỉnh giống layout misa", don't assume which image is the app's current output and which is the MISA target — ask, if there's any ambiguity (e.g. the two look identical, or the message doesn't make it obvious). Guessing wrong burns a full round-trip.

### `desktop-lamour` tracks `bin`/`obj` in git

Not gitignored. After every local build, restore + clean them before reviewing the diff:
```bash
git checkout -- src/DesktopLamour/bin src/DesktopLamour/obj
git clean -fdq src/DesktopLamour/bin src/DesktopLamour/obj
git status --porcelain   # confirm only intended source files remain modified
```
Never commit/push without the user's explicit go-ahead.

### Re-deriving the whole document on every round of feedback

The user will typically send a tightly cropped screenshot of just the one area that's still off (e.g. just the header block). Fix that one area — don't re-read/re-explain the entire print window's structure each time.
