# Guardrails — ct-review-artifact

> This skill was extracted from the first real page it produced (SalesReturn workflow review for kế toán). Read these before writing HTML.

---

## Core Rules

### 1. This skill designs and publishes — it does not verify or invent content

All factual claims (button labels, business rules, open questions) come from `CONTENT`, which was already verified against BE/WPF code by the caller (typically `ct-audience-persona-pattern`). Never add a rule, a step, or a confirmation item that isn't in `CONTENT` just because the page layout "wants" one more row — an empty/padded section is worse than a shorter, accurate one.

### 2. Don't reuse the exact previous palette — derive fresh from the subject's accent

The type pairing and token *structure* (Be Vietnam Pro + IBM Plex Mono, the `--bg/--surface/--accent/--flag/--draft` token set) are fixed project-wide so pages feel like a family. The **hex values** are not — pick an accent hue that fits the specific `FEATURE` (see PROMPT.md Step 2) each time. Copy-pasting the exact SalesReturn teal onto an unrelated warehouse or auth page is the generic-template failure mode this skill exists to avoid.

### 3. Real button labels, not paraphrases

Toolbar chips must use the exact string a `.xaml` renders (`➕ Thêm`, `Cất`, `Bỏ ghi`) as handed over in `CONTENT` — don't "clean up" a label's casing/wording/emoji to look more polished. If `CONTENT` didn't supply exact labels, ask the caller for them rather than guessing a plausible one.

### 4. Don't force a numbered sequence onto non-sequential content

The SalesReturn page numbered 6 real workflow steps because the feature genuinely has an order (create → view/reopen → bỏ ghi/sửa/cất → xóa → in/lập PN → lọc). A future topic that's just a flat set of independent facts (e.g. "5 things this validation checks, in no particular order") should render as a plain list or table, not a fake "Bước 1/2/3."

### 5. Confirmation checklist state is per-viewer only — don't reach for shared state

The checklist in the SalesReturn page persists to `localStorage`, scoped per browser/viewer, explicitly not shared across reviewers (footer says so). This is correct for a personal "have I looked at this" tracker. If a future request genuinely needs **shared** sign-off state across multiple reviewers (e.g. "show which of 3 accountants has approved"), that requires the `db` runtime capability — load `artifact-capabilities` and get the user's explicit intent before reaching for it; don't silently upgrade a personal checklist into shared state, and don't silently downgrade a genuine multi-reviewer request into a personal-only checklist either.

### 6. Title and favicon are per-page, not inherited

Each page's `<title>` names its own subject (a short noun phrase) and its favicon emoji fits that subject specifically. Don't default to reusing the last page's title pattern or emoji (see PROMPT.md Step 2) — a gallery of review artifacts should be distinguishable at a glance.

### 7. The screen mockup is a labeled schematic, never presented as a real screenshot

Added after a kế toán reviewer asked to "see the WPF UI" to understand a workflow described only in button-label chips and prose. The fix is a wireframe (`.mockup-window` — see OUTPUT_SCHEMA.md), always carrying a visible "Minh họa giao diện — không phải ảnh chụp thật" caption. Never:
- Claim or imply pixel-fidelity to the real WPF window (fonts, exact spacing, exact colors of the desktop app are out of scope — this is a structural diagram).
- Invent a field label, grid column, or tab name that CONTENT didn't supply — ask for the real ones (from the `.xaml`/ViewModel) instead of filling in a plausible-looking placeholder.
- Use real customer/business data as the sample rows — illustrative, obviously-placeholder values only.
- Add interactivity (hover states, a fake cursor, clickable-looking affordances) — it's a diagram, not a working replica; the page's *other* interactive element (the confirmation checklist) is the only thing that should feel operable.

### 8. No invented window chrome — the mockup shows only what's verified

The first draft of this component put 3 decorative dots in the title bar to "evoke title-bar chrome." That's an unverified guess (this project's WPF windows were never checked against a specific title-bar convention) and it's also exactly the kind of generic "browser mockup" visual cliché `artifact-design` warns against. The `.mockup-titlebar` shows the real screen title and nothing else, unless `CONTENT` names a genuinely real title-bar detail. Same standard applies to tabs (`.mockup-tabs`): only render tab names `CONTENT` actually supplied, never a plausible-sounding guess at how many tabs a screen "probably" has.

### 9. Chips and mockup are mutually exclusive per screen

Never render a screen's button labels twice on the same page (once as standalone chips, once inside its mockup's toolbar) — pick whichever INPUT_SCHEMA.md's "Chips vs. mockup" resolves to for that screen. A page with 2 screens can legitimately mix (mockup for one, chips for the other) — what's never legitimate is both for the *same* screen.

### 10. The checklist tracks reading, comments carry feedback — don't conflate the two

A checked box only proves one viewer opened the page — it reaches no one else and cannot carry "please change X" back to the developer. Every page must tell the reader to leave real feedback as a page comment, and the publishing session should read comments back (`Artifact` `action: "comments"`) rather than assuming a fully-ticked checklist means sign-off.

### 11. Reuse the same Artifact URL across review rounds for the same feature

Check `Artifact action: "list"` for an existing page named after this `FEATURE` before publishing (PROMPT.md Step 3.5). Publishing a fresh URL every round scatters a reviewer's feedback history across disconnected links with no way to tell which is current; updating in place (`url=`) keeps one link the reviewer can bookmark, and existing viewers get the update automatically. Never pass `favicon` on an update — an artifact's icon is meant to stay stable for its whole life.

---

## Common Pitfalls

### Treating this skill as the place to re-derive facts

If asked to publish a review artifact and the content feels thin, unverified, or possibly stale, the fix is to send it back through `ct-audience-persona-pattern` (or ask the direct caller to re-verify against code) — not to fill the gap here with a plausible-sounding addition.

### Over-designing a utilitarian document

This is a memo/review-doc treatment (`artifact-design`'s "polished, not editorial" branch) — real typographic hierarchy and a considered palette, but no giant hero, no gratuitous animation, no flourish that doesn't serve the reviewer's actual task of reading and ticking off items.

### Skipping the one-look-then-publish discipline

Don't loop screenshots against the local file trying to perfect spacing — one look, one edit pass, publish. The live Artifact is the real review surface; further polish is the user's to request.
