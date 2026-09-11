# Guardrails — ct-audience-persona-pattern

> These rules were each learned from a real back-and-forth with the user this session (SalesReturn workflow review for kế toán). Read them before reading any feature doc or writing an explanation.

---

## Core Rules

### 1. BE-only is incomplete for any workflow/feature explanation

A `SalesReturn` explanation built only from `src/Lamour.Application/Features/SalesReturn/docs/sales-return.md` and the BE UseCases looked complete but missed real user-facing behavior that only exists client-side: a WPF-only `IsReadOnly` flag with no BE equivalent, a "Bỏ ghi" button that only reverts status (doesn't unlock the form), a separate "Sửa" button that's the actual unlock action, and a "In" button that prints a different document (Phiếu Nhập Kho) than what a naive BE-only read would suggest ("Phiếu trả lại hàng bán", which was actually deleted).

**Always read both `src/Lamour.Application/Features/[Feature]/docs/` (BE) and `desktop-lamour/src/DesktopLamour/Features/HomePage/[Feature]/docs/` (WPF)**, and both the BE UseCase code and the WPF ViewModel/`.xaml` code, before writing any workflow explanation — regardless of which side `CONTEXT` happens to name.

### 2. Feature docs accumulate reversed decisions — the latest dated entry wins

Both `sales-return.md` docs (BE and WPF) contain multiple "Update — YYYY-MM-DD" sections where a later entry explicitly reverses an earlier one (e.g. "bỏ hẳn vòng đời Nháp → Ghi sổ" on 2026-09-07, then "tái kích hoạt Bỏ ghi (đảo ngược 1 phần quyết định 2026-09-07)" on 2026-09-09, then "đổi ý lần 2" later the same day). A PRD Summary or business-rules table near the top of the doc is often **stale** relative to the changelog below it.

Read chronologically. When a table/summary conflicts with a later dated update, the dated update wins — and when in doubt, the live code wins over both (Rule 3).

### 3. Docs are a starting point, never the source of truth — verify against code every time

Even after resolving Rule 2's chronology, the doc can still be wrong: e.g. the BE doc's business-rules table said "Sửa/Xóa chỉ cho phép khi Draft," but the actual `UpdateSalesReturnUseCase`/`DeleteSalesReturnUseCase` code has no such guard — they work regardless of status.

Before describing any guard condition, status transition, or button behavior to the audience, **open the actual UseCase / ViewModel / `.xaml` file and confirm it** — don't paraphrase the doc's claim as fact.

### 4. Never begin explaining before audience + goal + context are confirmed

Applies even when the user's initial message already supplies all three via `TOPIC`/`AUDIENCE`/`GOAL`/`CONTEXT` — still restate the confirmed understanding back in one short block (see PROMPT.md Step 1) before writing the explanation, so a wrong inference gets caught before the full write-up, not after.

### 5. A PM/QA/business deliverable ships as an Artifact, not just chat text

The first pass of the SalesReturn review answer was left inline in chat and only offered to publish — the user had to ask for the artifact explicitly on the next turn. That's backwards: once the confirmed audience is PM/QA/business, publishing via `ct-review-artifact` is part of finishing the task, not an optional extra to offer. Do it, then report the link.

### 6. Real button labels only — never invent or paraphrase UI text

When describing WPF screens to a business audience, use the exact string from the `.xaml` (`Content="➕ Thêm"`, a `DocumentToolbar`'s built-in default label, etc.) — grep the `.xaml` for the actual `Content=`/label attribute rather than assuming a button is called what its command name suggests (e.g. `UnconfirmCommand` renders as the button labeled "Bỏ ghi", not "Unconfirm" or "Hủy xác nhận").

---

## Common Pitfalls

### "The doc already answers this" without opening the code

A feature doc that reads as complete and internally consistent can still describe a decision that was later partially reversed in code without the doc being fully updated to match (see Rule 2/3). Treat every doc claim as a hypothesis to confirm against the UseCase/ViewModel, not a fact to relay.

### Explaining a WPF screen from the BE contract alone

The BE response DTO (`status: "Draft" | "Confirmed"`) cannot tell you that the WPF layers a *third*, client-only UI state (`IsReadOnly`) on top of those two BE values to distinguish "just unconfirmed, still locked" from "unconfirmed and now being edited." Anything client-only like this only shows up by reading the ViewModel directly.

### Asking three questions when the user already answered them

If `TOPIC`/`AUDIENCE`/`GOAL`/`CONTEXT` all arrive in the initial message (e.g. via the skill's structured input format), don't re-ask them one by one — go straight to the "Confirmed Understanding" restatement in PROMPT.md Step 1, then proceed. The one-at-a-time question flow is for filling gaps, not re-litigating what's already given.
