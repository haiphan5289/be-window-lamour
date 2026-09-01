# Prompt — ct-print-invoice-layout

> See [GUARDRAILS.md](GUARDRAILS.md) before writing any `FlowDocument`/`Table` code.
> Input parameters are defined in [INPUT_SCHEMA.md](INPUT_SCHEMA.md).
> Output shape is defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

---

## Pre-flight — Anti-Hallucination Verification (MANDATORY, runs BEFORE Step 0)

Load and apply all rules from:
@.claude/skills/ct-anti-hallucination/SKILL.md

**Fallback** — if the @-reference does not resolve:
```
Read /Users/hai.phan/Desktop/haiphan/be-window-lamour/.claude/skills/ct-anti-hallucination/SKILL.md
```

Every DTO property, class name, and file path used below must be verified against `desktop-lamour` before it appears in generated code — never invent a property name because it "sounds right" for an invoice.

---

## Step 0 — Read the existing pattern first

Never design a print window from scratch — three already exist and share one shape.

1. Resolve `TARGET_FILE` and the nearest reference file per [INPUT_SCHEMA.md](INPUT_SCHEMA.md)'s auto-detection table.
2. `Read` the nearest reference file in full (`SalesOrderPrintWindow.xaml.cs`, `SalesReturnPrintWindow.xaml.cs`, or `WarehouseReceiptPrintWindow.xaml.cs`).
3. If `TARGET_FILE` already exists (a fix/match task, not a new window), `Read` it in full too — don't edit blind.
4. Note the DTO type passed to `Initialize(...)` and confirm every field referenced in the print layout actually exists on it (`Grep` the Dto file) before using it.

---

## Step 1 — Determine the task shape

| Situation | Path |
|---|---|
| `TARGET_FILE` doesn't exist yet | New print window — go to Step 3 (skip MISA comparison unless `REFERENCE: has-misa-screenshot` was given) |
| `TARGET_FILE` exists, `REFERENCE` unset | Ask: "Bạn có ảnh mẫu MISA để so khớp không, hay chỉ cần layout chuẩn theo mẫu 01-VT?" |
| `TARGET_FILE` exists, `REFERENCE: has-misa-screenshot` | Go to Step 2 |
| `TARGET_FILE` exists, `REFERENCE: layout-only` | Go to Step 3 directly |

---

## Step 2 — MISA-comparison workflow (only when a reference screenshot is involved)

1. **Confirm which image is which before touching any code.** If the user attaches one screenshot and says "chỉnh giống layout misa", don't assume it's the MISA target — it may be the app's own current output being flagged as wrong. If genuinely ambiguous (e.g. it could be either, or two screenshots look identical), ask directly rather than guessing — a wrong guess burns a full round-trip.
2. If no MISA reference image is in the current context yet, ask the user to attach/re-attach it. Do not proceed from memory of "how mẫu 01-VT usually looks" — this business layers its own header/signature customizations on top of the standard template, so only the actual reference is authoritative.
3. Read the reference image and the current app output side by side (both via the `Read` tool if given as file paths, or from what's inline in the conversation). Identify concrete, specific differences (position, alignment, spacing, wording) — don't restate the whole layout back at the user.
4. If nothing concrete differs after comparison, say so plainly and stop — don't invent a change to justify effort.

---

## Step 3 — Implement

Follow the structure in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) exactly, applying every rule in [GUARDRAILS.md](GUARDRAILS.md) as you go — in particular:

- Construct every `TableRow`'s cells fresh, never reused across rows (Rule 1).
- Title + Ngày/Số/Nợ/Có in one shared 3-column table (Rule 2).
- Right-padding reserved on right-flush cells (Rule 3).
- Blank instead of default-placeholder for zero/empty derived fields (Rule 4).
- A/B/C/.../1/2/3 header row present (Rule 5).
- 4-role signature block (Rule 6).

When only a narrow area needs fixing (a MISA-comparison round), edit only that section — don't rewrite the whole `BuildDocument` method.

Update `EstimateContentHeight(...)` if blocks were added or removed.

---

## Step 4 — Build and clean

```bash
dotnet build src/DesktopLamour/DesktopLamour.csproj -c Debug
```
Confirm `0 Warning(s) / 0 Error(s)`. Then, since `desktop-lamour` tracks `bin`/`obj` directly in git:
```bash
git checkout -- src/DesktopLamour/bin src/DesktopLamour/obj
git clean -fdq src/DesktopLamour/bin src/DesktopLamour/obj
git status --porcelain
```
Confirm only the intended source files remain modified. Skip this step only if the user has explicitly said to skip building.

---

## Step 5 — Report honestly, don't over-claim

A clean build does **not** prove the print layout is correct — `Table`/`TableCell` ownership bugs and alignment issues are runtime FlowDocument behavior, only observable when the document actually renders (on the UTM Windows VM, or from a screenshot the user provides). State this explicitly rather than declaring the task "done."

Print the completion block from [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

Never commit or push without the user's explicit go-ahead.

---

## Step 6 — Iterate on feedback

Expect the user to reply with a cropped screenshot of just the one area still off. Go back to Step 2/3 for that specific area only — don't re-derive or re-explain the whole document each round.
