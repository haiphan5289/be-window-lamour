# Output Schema — ct-review-artifact

## Design System — "Lamour Review Doc"

Established from the first real page built with this skill (SalesReturn workflow review). Fixed project-wide so every review artifact reads as one family: **type pairing and token structure are fixed; the accent hue is picked per page** (see PROMPT.md Step 2).

### Typography

| Role | Face | Notes |
|---|---|---|
| Display / body | `Be Vietnam Pro` (400/500/600/700/800, italic 400) | Chosen deliberately: a typeface designed for Vietnamese typography, for a Vietnamese-language business document — not a generic default. Full Vietnamese diacritic coverage. |
| Codes, amounts, dates | `IBM Plex Mono` (400/500/600) | Document numbers (`BTL00001`), account codes (`5212`), and money amounts render in mono with `font-variant-numeric: tabular-nums` — these are literally codes/ledger figures in the domain, not prose. |
| Fallback stacks | `"Be Vietnam Pro", "Segoe UI", system-ui, sans-serif` / `"IBM Plex Mono", ui-monospace, monospace` | Always declared. |

Load via: `<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:ital,wght@0,400;0,500;0,600;0,700;0,800;1,400&family=IBM+Plex+Mono:wght@400;500;600&display=swap">` (or the equivalent `@import` inside `<style>`).

### Color token formula

Derive from **one accent hue chosen per page** (Step 2 of PROMPT.md), not a fixed hex set:

| Token | Role |
|---|---|
| `--bg` | Page ground — a neutral with a faint hue bias toward the accent, not pure white/grey |
| `--surface` / `--surface-2` | Card/table ground, one step lighter/darker than `--bg` |
| `--text` / `--text-muted` | Body text / secondary text |
| `--border` / `--border-strong` | Hairlines and card edges |
| `--accent` / `--accent-ink` / `--accent-soft` / `--accent-soft-border` | The page's chosen hue — ink (dark, for text-on-soft), soft (pale fill for badges/callouts) |
| `--flag` / `--flag-soft` / `--flag-soft-border` | A second, distinct hue for "cần xác nhận" open-question callouts — never the same hue as `--accent` (they must be visually distinguishable at a glance) |
| `--draft` / `--draft-soft` | A third, distinct hue only if the domain has a real secondary status to show (e.g. Nháp vs. Đã ghi sổ) — omit if not applicable |

Full light-mode `:root` block, then `@media (prefers-color-scheme: dark)` under `:root:not([data-theme="light"])`, then `:root[data-theme="dark"]` — per `artifact-design`'s three-state rule. Every token must be declared in bare `:root` first.

### Layout

- Single column, `max-width: 880px`, centered, side gutter `max(16px, ...)`.
- **Masthead**: bordered card, dashed divider between title block and a 4-column meta grid (Đối tượng / Mục tiêu / Nguồn / Ngày) — collapses to 2 columns under ~700px.
- **Toolbar reference chips**: for a screen with no mockup only (see INPUT_SCHEMA.md "Chips vs. mockup") — a card per such screen, a wrapped row of pill chips using the **real** button label/icon from the `.xaml`. Never built for a screen that already has a mockup.
- **Screen mockup** (see "Screen Mockup Component" below): a schematic, labeled-as-such visual of the actual WPF window, so a non-dev reader can picture the screen instead of inferring it from a button list.
- **Step cards**: numbered box (mono digit, accent-soft fill) + body, one per genuinely sequential step. Inline `.callout` blocks (left border in `--flag`, tag label "Xác nhận" or similar) for open questions attached to that step.
- **Rules table**: plain two-column `rule → value` table inside a bordered wrapper, mono for codes/amounts.
- **Confirmation checklist**: custom checkbox rows, `localStorage`-persisted per item (key scoped to the page, e.g. `<feature-slug>-review:<item-id>`), a live progress label ("N/total đã xem qua"). This is a personal reading-progress tracker only, not a feedback channel — see "Real feedback: point reviewers at page comments" below.
- **Footer**: sourcing note (real file paths from `CONTENT`) + a one-line note that checklist state is local to the viewer's browser + a one-line pointer to leave real feedback as a comment on the page (see below).

### Real feedback: point reviewers at page comments, not just the checklist

The checklist above only tracks what one viewer has personally looked at — it is invisible to the developer and to other reviewers, so it cannot carry the actual "kế toán reviews this and asks for changes" feedback the page exists for. Every published review artifact must therefore end with a short, explicit line telling the reader **how to actually send feedback**: leave a comment directly on the page (the Claude Artifacts comment feature), naming which open question or section it's about. Do not silently rely on the checklist to stand in for this — it can't.

The publishing session can read those comments back later with the `Artifact` tool's `action: "comments"` — mention this to the user (Hai) once, right after publishing, so they know to check back rather than assuming silence means approval.

---

## Screen Mockup Component

**Purpose:** a kế toán/business reader pictures a screen far more easily than they infer one from a bullet list of button labels. This component is a **schematic wireframe**, not a screenshot — CSS/HTML boxes standing in for the real WPF chrome — and must always be visibly labeled as illustrative (a small "Minh họa giao diện — không phải ảnh chụp thật" caption above or on the frame).

Build one instance per real screen `CONTENT` describes (a feature typically has both a list screen and a popup form — mock up both, each in its own `.mockup-window` block).

### Shape

```
.mockup-window (bordered card, --border-strong, subtle --shadow, radius 6-8px — slightly rounder
                than the page's other cards, so it reads as "a window" not "a content card")
  .mockup-titlebar   — the window/screen title text only, e.g. "Chứng từ hàng bán bị trả lại"
                       (bold, --text). Do NOT add decorative window-chrome buttons/dots — this
                       project's WPF windows are never verified against a specific OS's title-bar
                       convention, and fake traffic-light/minimize-maximize-close icons are exactly
                       the kind of generic "browser mockup" cliché artifact-design warns against.
                       If the real title bar has a genuinely verified detail (a real icon, a real
                       document-number field shown there), include only that — nothing invented.
  .mockup-tabs       — ONLY if CONTENT's Screen Layout names real tabs (e.g. SalesReturnWindow's
                       "1. Hàng tiền / 2. Thuế / 3. Giá vốn / 4. Thống kê"): a row of tab-shaped
                       labels, one visually active (accent underline/fill) matching whichever tab
                       .mockup-body actually shows. Caption or footnote which tab is shown and that
                       the others exist but aren't pictured — never silently flatten a tabbed
                       screen's fields into one untabbed block.
  .mockup-toolbar    — flex row of small button-shaped blocks (icon + label), using that screen's
                       real button labels from CONTENT. This is the ONLY place those labels render
                       for this screen — see INPUT_SCHEMA.md "Chips vs. mockup" (never also build a
                       standalone toolbar-chips row for the same screen).
  .mockup-body       — EITHER:
      (a) form layout: CSS grid of label-over-value pairs, grouped the way the real .xaml groups
          them (e.g. header fields in one row-group, line items below) — labels in --text-muted,
          small caps; values in --text, using real field labels from CONTENT. If tabbed, only the
          active tab's fields render here.
      (b) grid/list layout: a header row (real column names, in the real order) + 2-3 sample data
          rows, monospace for numeric/code columns, tabular-nums. If the real screen has a status
          distinction (row highlight, badge), show it on exactly one sample row — same technique
          as the Status Demo pattern already used for a whole-row highlight (see the SalesReturn
          reference build): background tint + colored text via the same --draft/--accent tokens,
          not a new ad hoc color.
  .mockup-footer     — only if the real screen has one (e.g. a totals row) — small, right-aligned,
                       mono figures
  .mockup-caption    — small, --text-muted, italic: "Minh họa giao diện — không phải ảnh chụp thật"
                       (+ which tab is shown, if applicable) — placed just above or below the frame
```

### Reference CSS (copy and adapt — don't redesign the shape each time)

```css
.mockup-window{ border:1px solid var(--border-strong); border-radius:8px; background:var(--surface);
  box-shadow:var(--shadow); overflow:hidden; }
.mockup-titlebar{ padding:10px 16px; font-weight:700; font-size:.92rem; border-bottom:1px solid var(--border); }
.mockup-tabs{ display:flex; gap:2px; padding:0 12px; background:var(--surface-2); border-bottom:1px solid var(--border); }
.mockup-tabs span{ padding:8px 12px; font-size:.8rem; color:var(--text-muted); border-bottom:2px solid transparent; }
.mockup-tabs span.active{ color:var(--accent-ink); border-bottom-color:var(--accent); font-weight:600; }
.mockup-toolbar{ display:flex; flex-wrap:wrap; gap:8px; padding:10px 16px; border-bottom:1px solid var(--border); background:var(--surface-2); }
.mockup-toolbar .btn{ display:inline-flex; gap:6px; align-items:center; font-size:.82rem; font-weight:600;
  padding:5px 10px; border:1px solid var(--border-strong); border-radius:4px; background:var(--surface); }
.mockup-body{ padding:16px; }
.mockup-body.form{ display:grid; grid-template-columns:repeat(auto-fill, minmax(160px,1fr)); gap:12px 20px; }
.mockup-body.form .field label{ display:block; font-size:.68rem; text-transform:uppercase; letter-spacing:.04em; color:var(--text-muted); }
.mockup-body.form .field .value{ font-size:.9rem; font-weight:600; }
.mockup-body.grid{ overflow-x:auto; }
.mockup-body.grid table{ width:100%; border-collapse:collapse; font-size:.84rem; }
.mockup-body.grid th{ text-align:left; padding:8px 10px; background:var(--surface-2); color:var(--text-muted);
  font-size:.68rem; text-transform:uppercase; }
.mockup-body.grid td{ padding:8px 10px; border-top:1px solid var(--border); }
.mockup-footer{ padding:10px 16px; border-top:1px solid var(--border); text-align:right; font-size:.86rem; }
.mockup-caption{ font-size:.76rem; font-style:italic; color:var(--text-muted); margin-top:6px; }
```

### Rules

- Every label, column name, tab name, and sample field must come from `CONTENT`'s Screen Layout section (ultimately the real `.xaml`/ViewModel) — never invent a plausible-looking field, column, or tab.
- Sample values are illustrative — obviously placeholder-shaped (a generic name, a round document number) — never real customer/business data, and the `.mockup-caption` already flags the whole thing as illustrative.
- Keep it flat and static — no fake cursor, no hover states, no interactive affordance, no invented window chrome (see `.mockup-titlebar` above). It's a diagram of a screen, not a working replica.
- On narrow width, the grid/list body scrolls horizontally inside its own container (per `artifact-design`'s wide-content rule) — the mockup frame itself never forces the page to scroll sideways.
- Reuse the page's own tokens (`--surface`, `--border`, `--accent-soft`, etc.) — the mockup must sit inside the page's theme (light/dark), not hardcode a separate "always light" chrome look.

---

## Completion Block

Print this after publishing:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-review-artifact COMPLETE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Feature:     <FEATURE>
Audience:    <AUDIENCE>
Sections:    <which components were included — chips / mockup(s) / steps / rules table / checklist>
Artifact:    <published URL — new page, or updated existing page (see PROMPT.md Step 3.5)>
Feedback:    Kế toán để lại comment trực tiếp trên trang — check qua Artifact action "comments"
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```
