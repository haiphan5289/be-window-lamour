# Prompt — ct-review-artifact

> See [GUARDRAILS.md](GUARDRAILS.md) before writing any HTML.
> Input payload is defined in [INPUT_SCHEMA.md](INPUT_SCHEMA.md).
> The design system to apply is defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

---

## Step 0 — Load the global design skill

Load `artifact-design` (and `artifact-capabilities` only if the page needs a runtime capability beyond per-viewer `localStorage` — see [GUARDRAILS.md](GUARDRAILS.md) "Don't reach for shared state"). Apply its fundamentals (theme tokens for all three viewer states, responsive gutters, real content, honored favicon-on-redeploy rule) underneath this skill's project-specific design system.

---

## Step 1 — Confirm scope: this skill designs, it does not verify

Take `FEATURE`/`AUDIENCE`/`GOAL`/`CONTENT` as given and already correct. Do not:
- Re-read BE/WPF source to double-check the content's claims (that's `ct-audience-persona-pattern`'s job, or the direct caller's).
- Add facts, rules, or open questions that aren't in `CONTENT`.
- Soften or embellish an open question into a settled fact, or vice versa.

If `CONTENT` is missing a section entirely (e.g. no workflow steps because the topic isn't a sequence), skip that component in the page — see [INPUT_SCHEMA.md](INPUT_SCHEMA.md) "When NOT to invent a section."

---

## Step 2 — Pick the page's specific identity within the design system

[OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) fixes the type pairing and token *structure* project-wide (so every review artifact reads as one family) — it does not fix one hardcoded palette for every page. For **this** `FEATURE`:

1. Pick an accent hue appropriate to the subject (e.g. a ledger-teal for accounting/kế toán topics, a warmer amber/rust for warehouse/kho topics, a cooler blue for auth/access topics) — derive the full token set from that one accent per the formula in OUTPUT_SCHEMA.md, don't reuse a previous page's exact hex values verbatim.
2. Pick a `<title>` that names the real subject as a short noun phrase (e.g. "Chứng Từ Trả Hàng Bán", not "Quy Trình Trả Hàng Bán — Review Kế Toán").
3. Write a one-sentence `description` (the Artifact gallery subtitle) from `FEATURE` + `AUDIENCE` + `GOAL` — e.g. "Quy trình Chứng từ hàng bán bị trả lại, đối chiếu code BE + WPF, để kế toán review trước khi vận hành."
4. Pick a favicon emoji that fits the specific subject (🧾 for an invoice/voucher doc, 📦 for warehouse, 🔐 for auth/permissions) — don't default to the same emoji for every page. Only set it on a genuinely new page (see Step 3.5) — omit it when updating an existing one.

---

## Step 3 — Build the page

Follow the component patterns in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md):
- Masthead (eyebrow + title + lede + meta row: Đối tượng / Mục tiêu / Nguồn / Ngày)
- **Per screen, exactly one of:**
  - **Screen mockup** — a schematic `.mockup-window` built from `CONTENT`'s Screen Layout data (title, tabs if any, fields/columns, sample values, status distinction) — used whenever that detail is available. If `CONTENT` didn't supply concrete field/column/tab names for a screen it clearly describes, ask for them rather than inventing plausible ones — do not skip silently and do not guess.
  - **Toolbar reference chips** — only for a screen where `CONTENT` has button labels but not enough detail for a mockup.
  - Never build both for the same screen — see INPUT_SCHEMA.md "Chips vs. mockup."
- Numbered step cards with inline callouts for open questions (only if `CONTENT`'s workflow is genuinely sequential)
- Rules table (only if `CONTENT` has calculation/business rules)
- Confirmation checklist (personal reading-progress tracker, `localStorage`-persisted, only if `CONTENT` has open questions) + a footer line pointing reviewers at page comments as the real feedback channel (see OUTPUT_SCHEMA.md "Real feedback")
- Footer sourcing note, naming the actual files/docs the content was verified against (from `CONTENT`, not invented)

Write the file to the session scratchpad directory, not the project repo.

---

## Step 3.5 — Check for an existing review artifact for this `FEATURE` before publishing

A feature can be reviewed more than once (behavior changes, a reviewer asks for a follow-up round) — don't mint a new link every time, or the reviewer accumulates disconnected URLs with no way to tell which is current.

1. Call `Artifact` with `action: "list"` and scan titles for this `FEATURE` (titles are per-subject noun phrases per Step 2, so a prior page for the same feature should be recognizable).
2. If found, ask the user to confirm it's the same review doc, then publish with `url` set to that artifact's URL so it updates in place (existing viewers get the new version automatically). Omit `favicon` on this call — an artifact's icon must stay stable across its life; only pass a new one if the user explicitly asks to change it.
3. If not found (or the user says it's unrelated), publish fresh per Step 4 below.

---

## Step 4 — One look, then publish

Per `artifact-design`'s process: one screenshot/preview look, one edit pass for what it shows, then publish via the `Artifact` tool with `title` and `description` set per Step 2, plus `favicon` (new page only — see Step 3.5) and `url` (update-in-place only). Don't loop on self-review.

---

## Step 5 — Report back

Report the published link in one short message, plus the one-line reminder that real feedback belongs in a comment on the page (not the checklist) and that the developer can read those back via `Artifact` `action: "comments"`. Don't re-paste the full explanation into chat — the page is now the canonical copy. Print the completion block from [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).
