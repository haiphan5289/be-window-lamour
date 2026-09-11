# Input Schema — ct-review-artifact

## Invocation Syntax

**From `ct-audience-persona-pattern` (typical path):** no separate invocation syntax — the calling skill passes the content payload it already assembled per its `spec/OUTPUT_SCHEMA.md` "PM / QA / Business Deliverable" section directly as this skill's input.

**Direct invocation:**
```
/ct-review-artifact
FEATURE: Chứng từ hàng bán bị trả lại
AUDIENCE: Kế toán lên đơn
GOAL: Review workflow trước khi vận hành
CONTENT: <the finished explanation>
```

---

## Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `FEATURE` | Yes | Real Lamour feature/module name — becomes the page's subject and title seed |
| `AUDIENCE` | Yes | Who the page is for (kế toán, thủ kho, PM, QA...) — drives tone and the masthead meta row |
| `GOAL` | Yes | Why they're reading it (review before vận hành, sign off, understand a decision) |
| `CONTENT` | Yes | The structured, already-verified explanation — see Content Payload Shape below |

---

## Missing-Parameter Handling (direct invocation only)

When invoked from `ct-audience-persona-pattern`, all four parameters always arrive together (that skill won't reach Step 4 without them confirmed). Only a **direct** `/ct-review-artifact` call can arrive incomplete — handle it like this, don't guess:

| Missing | Action |
|---|---|
| `FEATURE` | Ask: "Trang này review chức năng nào?" — don't infer from `CONTENT` prose alone if it names more than one feature. |
| `AUDIENCE` or `GOAL` | Ask directly — these set the masthead meta row and cannot be inferred from `CONTENT` reliably. |
| `CONTENT` entirely absent | This skill has nothing to design from — tell the user it needs a finished, already-verified explanation first (point them at `ct-audience-persona-pattern` if one hasn't been produced yet). Never generate placeholder content to fill the page. |
| `CONTENT` present but thin/unverified-looking (raw prose with no concrete button labels, field names, or file references) | Restructure what's given (see below), but flag to the user that it reads unverified — don't silently publish a page that looks authoritative if the underlying facts were never checked against code. |

---

## Content Payload Shape

This skill does not verify facts and does not fill gaps in missing content — if a section below is genuinely not applicable to the topic (e.g. a pure architecture-decision explainer with no on-screen buttons), omit that section rather than inventing filler.

| Section | Used for |
|---|---|
| **Screen layout** (window/screen type, tab names if any, form field labels or grid column names, 2-3 illustrative sample values, any status distinction) | **Screen mockup** — schematic visual of the actual WPF window (see OUTPUT_SCHEMA.md). Its own toolbar row already shows that screen's real button labels. |
| Real on-screen controls (exact button labels per screen) | **Toolbar reference chips** — standalone pill row, built **only** for a screen that has no mockup (see "Chips vs. mockup" below). Never build both for the same screen. |
| Workflow steps (only if genuinely sequential) | Numbered step cards |
| Calculation / business rules (rule → value pairs) | Reference rules table |
| Open questions needing sign-off | Confirmation checklist (tickable, localStorage-persisted) + a pointer to leave real feedback as a page comment (see GUARDRAILS.md) |
| Sourcing note (which files were verified, when) | Footer |

If the payload arrives as unstructured prose instead of this shape, restructure it into these sections before designing the page — don't publish a wall of prose inside artifact styling.

### Chips vs. mockup — pick one per screen, never both

A standalone toolbar-chip row and a mockup's own toolbar bar say the same thing twice on one page — that's bloat, not clarity, on what should stay a utilitarian document. Resolve per screen:

- `CONTENT` has full Screen Layout detail for that screen (title + fields/columns) → build the **mockup** (its toolbar covers the buttons); skip the standalone chips row for that screen entirely.
- `CONTENT` only has the button list for that screen, no field/column detail → build **chips only** (a mockup with an empty/invented body would be worse than no mockup).
- A feature with multiple screens can end up with a mockup for one and chips for another — that's fine, each screen gets whichever it has enough detail for, never both.

### Screen layout — what to ask for if missing

A kế toán/business reader pictures a *screen*, not a class diagram. Whenever `CONTENT` describes a WPF popup or list screen, it should include enough concrete detail to draw one, sourced from the real `.xaml`/`ViewModel`, not paraphrased:

- Window/screen title (e.g. "Chứng từ hàng bán bị trả lại")
- Screen type: **form popup** (field labels, grouped how) or **list/grid** (column headers, in order) — a feature usually has both, and each gets its own mockup
- **Tab names, in order, if the real screen has a `TabControl`** (e.g. SalesReturnWindow's "1. Hàng tiền / 2. Thuế / 3. Giá vốn / 4. Thống kê") — never flatten a tabbed screen's fields into one untabbed block, that misrepresents the screen's real structure
- 2-3 illustrative sample rows/values per screen — clearly derived as examples, not real customer data
- Any visual status distinction the real screen uses (a badge, a whole-row highlight/color, a locked/greyed-out state) and what triggers it

If the caller (`ct-audience-persona-pattern` or a direct invoker) didn't supply this, ask for the field/column/tab list rather than guessing plausible ones — see GUARDRAILS.md.

---

## When NOT to invent a section

- No numbered steps if the content isn't a real sequence — a flat list of independent facts stays a list, not a fake "Bước 1/2/3."
- No confirmation checklist if the source explanation didn't surface open questions — an empty or padded checklist is worse than no checklist.
- No toolbar chips section for a screen that already has a mockup (see "Chips vs. mockup" above), and none at all for a topic that isn't a screen/workflow (e.g. "why EF Core uses AsNoTracking" explained for a QA audience has no buttons to show).
- No screen mockup for a topic with no real screen behind it (an architecture/decision explainer) — and never a mockup with invented field/column/tab names when `CONTENT` didn't supply the real ones.
