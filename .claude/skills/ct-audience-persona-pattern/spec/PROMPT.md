# Prompt — ct-audience-persona-pattern

> See [GUARDRAILS.md](GUARDRAILS.md) before reading any feature doc or writing the explanation.
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

Every class, route, EF entity, ViewModel property, and XAML binding used in the explanation must be verified against the codebase before it appears — never invent a button label, a status value, or a guard condition because it "sounds right."

---

## Step 0 — Resolve `TOPIC` and `CONTEXT` to a feature

Identify which feature(s) `TOPIC` touches. If `CONTEXT` already names a doc path or feature, use it; otherwise infer the nearest feature from `TOPIC` (e.g. "Sales Order", "SalesReturn", "Products", "Deposits").

---

## Step 1 — Ask for missing input, ONE question at a time

**🚨 Follow strictly — do not skip:**

1. If `AUDIENCE` is missing, ask for it first. Offer 2-4 concrete role options grounded in this project (see [INPUT_SCHEMA.md](INPUT_SCHEMA.md) Audience Role Reference) — never a generic "who is your audience?".
2. Once `AUDIENCE` is known, if `GOAL` is missing, ask what the reader needs to be able to do afterward (implement, review a PR, debug, sign off before vận hành, write test cases).
3. Once `GOAL` is known, if `CONTEXT` is missing or ambiguous, ask which feature/module/doc it relates to.
4. **Do not start explaining** until all three are confirmed. Restate the confirmed understanding back to the user in one short block before writing anything (see Example Interaction below).
5. **Do not use inappropriate technical depth** for the confirmed audience — no EF Core internals for a kế toán, no business-only framing for a dev implementing the UseCase.

### Example Interaction

```
Question 1 — Vai trò người nghe:
Trước khi giải thích, mình cần biết bạn là ai để chỉnh độ sâu cho phù hợp.
- Junior dev mới join, chưa quen Clean Architecture
- Dev đang implement/fix bug ở feature này
- PM/QA/kế toán đang review business rule hoặc workflow, không cần code

User: "Dev đang fix bug ở feature Sales."

Question 2 — Mục tiêu:
Sau khi hiểu xong, bạn cần làm gì?
- Implement UseCase mới
- Review lại logic đã có để tìm bug
- Viết test case

User: "Review lại logic đã có để tìm bug."

Question 3 — Ngữ cảnh:
Bug này liên quan tới UseCase/màn hình nào cụ thể?

User: "Flow unconfirm mới thêm."

Confirmed Understanding:
Mình sẽ giải thích cho dev đang debug flow unconfirm, mục tiêu review logic để tìm bug — bắt đầu giải thích?
```

---

## Step 2 — Read BOTH sides' feature docs, then verify against live code

**Never explain from one side only.** A workflow explanation that covers only the BE UseCase is incomplete if the WPF popup/list layers its own state on top (a client-only flag, a button not wired to what its label implies, a toolbar action the BE doesn't expose) — see [GUARDRAILS.md](GUARDRAILS.md).

1. Read the BE doc: `src/Lamour.Application/Features/[Feature]/docs/*.md`.
2. Read the WPF doc: `desktop-lamour/src/DesktopLamour/Features/HomePage/[Feature]/docs/*.md`.
3. These docs accumulate dated "Update — ..." sections that sometimes reverse earlier decisions. Read chronologically; the **most recent dated update wins** over an older summary table or PRD section in the same file.
4. **Verify the docs' claims against the actual current code** before writing anything — docs go stale or get half-updated:
   - BE: read the real `[Xxx]UseCase.cs` files (Create/Update/Delete/Confirm/Unconfirm) — confirm what each actually guards on (`Status` checks, validation order), not what the doc's summary table says.
   - WPF: read the real `[Xxx]ViewModel.cs` (computed properties like `IsEditable`/`CanEdit`, `[RelayCommand(CanExecute = ...)]` wiring) and the `.xaml` toolbar bindings (`SaveCommand`, `UnpostCommand`, `EditCommand`, list-view button `Command=` attributes) — confirm actual button labels and what they're wired to.
5. If a doc and the code disagree, the **code wins** — note the discrepancy briefly if it's relevant to the audience's goal (e.g. a QA writing test cases needs to know a rule the doc still claims that the code no longer enforces).

If no doc exists for one or both sides, proceed from live code only and say so — never fabricate doc content that doesn't exist.

---

## Step 3 — Write the explanation, calibrated to the confirmed audience

Follow the Depth Calibration table in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md). Ground every example in real Lamour entities/features (Sales, SalesReturn, Products, Suppliers, Deposits, Warehouse) — never invented modules, classes, or field names.

For a **PM / QA / business** audience specifically:
- No code, no class/method names as the primary content — describe screens, buttons (using their real on-screen labels), and outcomes.
- Structure as a numbered workflow when the feature genuinely is sequential (create → confirm → unconfirm → edit, etc.) — don't force numbering onto content that isn't a real sequence.
- Flag open business-rule questions explicitly (e.g. "hệ thống ghi sổ ngay không qua duyệt — có đúng quy trình mong muốn không?") rather than silently presenting current behavior as correct.

---

## Step 4 — Deliver the output

| Confirmed audience | Delivery |
|---|---|
| Junior / mid-level / senior dev | Reply inline in chat. Only publish as an Artifact if the user asks, or the explanation is long enough to be a standing reference doc. |
| PM / QA / business (kế toán, thủ kho, cashier, non-dev stakeholder) | **Always** invoke the `ct-review-artifact` skill to publish the finished explanation as an Artifact — do not just reply inline, and do not ask permission first. Pass it the finished explanation's content (feature name, audience, goal, the workflow steps/rules/open-questions already written in Step 3) — `ct-review-artifact` handles page design and publishing; it does not re-derive the content. |

After invoking `ct-review-artifact` (when applicable), report the published link back to the user in one short message — don't repeat the full explanation a second time in chat once it's on the page.
