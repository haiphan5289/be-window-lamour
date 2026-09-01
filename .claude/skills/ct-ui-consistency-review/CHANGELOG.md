# Changelog — ct-ui-consistency-review

## v1.0.0 — 2026-09-01

### Added
- Initial release, extracted from a real multi-round WPF UI consistency session that started at one popup's grid borders and expanded to a 28-file whole-app audit plus two deeper bug classes
- File structure per `ct-print-invoice-layout`/`ct-ai-document` convention: thin `SKILL.md` router + `spec/INPUT_SCHEMA.md` + `spec/PROMPT.md` + `spec/OUTPUT_SCHEMA.md` + `spec/GUARDRAILS.md`
- Bug Class 1: missing vertical `GridLinesVisibility` on product/line-item and list/report `DataGrid`s — fixed with the established `#9CB8D4` border color
- Bug Class 2: trailing `Width="*"` `DataGrid` column silently blocks horizontal scroll even after `ScrollViewer.HorizontalScrollBarVisibility="Auto"` is set — requires converting the star column to a fixed width too, both parts required
- Bug Class 3 (found the hard way, after Bug Class 2's fix was verified in place and the symptom persisted): a `StackPanel`-rooted custom `TabControl` `ControlTemplate` (`AppTabControl.Modern`, duplicated locally across 4 files) gives infinite width to tab content, so no downstream `DataGrid` fix can ever activate its scrollbar — fix is `StackPanel` → `Grid` with `Auto`/`*` rows in the template
- Bug Class 4: a shared control's (e.g. `DocumentToolbar.xaml`) color palette or container shape drifts from the app's `AppColor`/`AppButton` design tokens, reported by users as "lạc lõng"/"doesn't match the app" — documented the exact token reference table and the role-based (not blanket) coloring approach
- Guardrail: always audit before fixing on a "whole app" scope — this session's actual file counts (7 → 28 → 4) came in progressively larger than any single guess
- Guardrail: a repeated symptom after a verified fix means a different, upstream bug class — not a wrong pixel value
- Guardrail: shared controls get fixed once at the source; per-file duplicated styles (a known, accepted pattern in this codebase) get fixed at every duplicate independently, never centralized as a side effect
- Explicit reminder that none of these four bug classes produce a build error — `dotnet build` succeeding proves nothing about the actual fix; real verification is UTM/screenshot-based
