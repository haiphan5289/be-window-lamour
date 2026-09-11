# Changelog — ct-review-artifact

## v1.2.0 — 2026-09-10

### Fixed
- Completion block's `Sections:` line never mentioned the mockup component added in v1.1.0 — updated, plus a new `Feedback:` line.
- Removed the mockup's decorative 3-dot title-bar chrome — unverified against the real WPF app and exactly the "browser mockup" cliché `artifact-design` warns against. `.mockup-titlebar` now shows only the verified screen title.
- Resolved duplication between standalone toolbar chips and a mockup's own toolbar bar — they're now mutually exclusive per screen ("Chips vs. mockup" in `INPUT_SCHEMA.md`), never both for the same screen.

### Added
- Tabbed-screen support: `.mockup-tabs` renders real tab names from `CONTENT` (e.g. SalesReturnWindow's 4 tabs) instead of silently flattening a tabbed screen's fields into one block.
- Concrete reference CSS for the full mockup component set (`.mockup-window/-titlebar/-tabs/-toolbar/-body/-footer/-caption`) — copy-and-adapt instead of re-deriving the shape from prose each time, matching `ct-print-invoice-layout`'s "Required Constants" rigor.
- Real-feedback guidance: the confirmation checklist is a personal reading-progress tracker only (invisible to the developer) — every page now points reviewers at leaving a page comment instead, and the publishing session is told to check `Artifact action: "comments"` after publishing.
- Redeploy lifecycle: Step 3.5 checks `Artifact action: "list"` for an existing page for the same `FEATURE` before publishing, and updates it in place (`url=`, `favicon` omitted) instead of minting a new link every review round.
- `INPUT_SCHEMA.md`: Missing-Parameter Handling table for direct invocation with an incomplete `FEATURE`/`AUDIENCE`/`GOAL`/`CONTENT`.
- `description` guidance (Artifact gallery subtitle), derived from `FEATURE` + `AUDIENCE` + `GOAL`.

### Learned from
- Self-review requested by the user immediately after v1.1.0 shipped — surfaced that the checklist doesn't actually deliver reviewer feedback anywhere, the title-bar dots were an ungrounded cliché, chips and mockup duplicated each other, and multi-round review had no update story.

## v1.1.0 — 2026-09-10

### Added
- **Screen mockup component** — a schematic, always-labeled-as-illustrative wireframe of the real WPF window (`.mockup-window`: title bar + toolbar reusing the real button labels + form-field or grid-column body + optional totals footer), built per real screen `CONTENT` describes. Added after a kế toán reviewer asked to see the WPF UI to understand a workflow that had only been described in button-label chips and prose.
- `INPUT_SCHEMA.md`: new "Screen layout" content-payload section (window/screen title, real field/column names, 2-3 illustrative sample values, any status-distinction styling) that `ct-audience-persona-pattern` now supplies alongside the rest of its content payload.
- Guardrail: the mockup must never claim pixel-fidelity to the real desktop app, never invent a field/column name `CONTENT` didn't supply, never use real business data as sample rows, and stays static (no fake interactivity).

## v1.0.0 — 2026-09-10

### Added
- Initial release, extracted from the SalesReturn ("Chứng từ hàng bán bị trả lại") workflow review Artifact built for a kế toán audience.
- File structure per `ct-ai-document`/`ct-print-invoice-layout` convention: thin `SKILL.md` router + `spec/INPUT_SCHEMA.md` + `spec/PROMPT.md` + `spec/OUTPUT_SCHEMA.md` + `spec/GUARDRAILS.md`.
- Wired as the automatic delivery step for `ct-audience-persona-pattern` when the confirmed audience is PM/QA/business — see that skill's `spec/PROMPT.md` Step 4.
- Documented the "Lamour Review Doc" design system: Be Vietnam Pro + IBM Plex Mono type pairing, a fixed token structure with a per-page accent hue, and the component set (masthead, toolbar reference chips, numbered step cards with open-question callouts, rules table, localStorage-persisted confirmation checklist).
- Guardrail: this skill designs/publishes only — never re-verifies or invents content; that responsibility stays with the caller.
- Guardrail: derive a fresh accent hue per subject rather than reusing the previous page's exact palette, so the "family" stays a design system rather than one hardcoded template.
- Guardrail: confirmation checklist state is per-viewer `localStorage` by default; shared/multi-reviewer sign-off state requires explicit intent + the `db` runtime capability, not a silent upgrade.
