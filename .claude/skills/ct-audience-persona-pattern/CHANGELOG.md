# Changelog — ct-audience-persona-pattern

## v2.1.0 — 2026-09-10

### Added
- The content payload handed to `ct-review-artifact` now includes a **Screen layout** section per real WPF screen (window/screen title, real field/column names in order, 2-3 illustrative sample values, any status-distinction styling) — sourced from the same `.xaml`/ViewModel read in PROMPT.md Step 2, so `ct-review-artifact` can render a schematic mockup of the actual screen instead of the audience having to infer it from a button-label list alone.

## v2.0.0 — 2026-09-10

### Changed
- Restructured to the `ct-ai-document`/`ct-print-invoice-layout` convention: thin `SKILL.md` router + `spec/INPUT_SCHEMA.md` + `spec/PROMPT.md` + `spec/OUTPUT_SCHEMA.md` + `spec/GUARDRAILS.md`, replacing the previous single-file skill.
- `AUDIENCE` now explicitly includes a "business" bucket (kế toán, thủ kho, cashier, non-dev stakeholder) alongside PM/QA, with its own row in the Audience Role Reference and Depth Calibration tables.

### Added
- Mandatory dual-doc + dual-code verification: read **both** the BE feature doc (`src/Lamour.Application/Features/[Feature]/docs/`) and the WPF feature doc (`desktop-lamour/.../Features/HomePage/[Feature]/docs/`), then verify both against the live UseCase and ViewModel/`.xaml` code — never explain a workflow from the BE side alone.
- Guardrail: feature docs accumulate dated "Update — ..." sections that reverse earlier decisions; the most recent dated entry wins over an older summary/PRD table in the same file.
- Guardrail: docs are a hypothesis, not a source of truth — a doc's business-rules table can silently go stale relative to what the UseCase/ViewModel code actually does; always open the code before relaying a guard condition to the audience.
- Output delivery rule: for a confirmed PM/QA/business audience, the finished explanation is **always** published as an Artifact via the new `ct-review-artifact` skill (see that skill's `CHANGELOG.md`) — not offered as an afterthought, not left inline only.
- Real-button-label rule: describe WPF screens using the exact `Content=`/label string from the `.xaml`, never a paraphrase of the bound command name.

### Learned from
- Live session: SalesReturn ("Chứng từ hàng bán bị trả lại") workflow review for kế toán — first pass was BE-only and had to be redone after reading the WPF `SalesReturnViewModel`/`SalesReturnListViewModel`/`.xaml`, which revealed client-only lock/unlock state and button wiring the BE contract alone could not show.

## v1.0.0 — 2026-08-xx

### Added
- Initial single-file release: ask-audience-goal-context-one-at-a-time pattern, Depth Calibration table (Junior/Mid/Senior dev, PM/QA), BE-doc-only verification step.
