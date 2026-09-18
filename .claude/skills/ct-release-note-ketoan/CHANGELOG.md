# Changelog — ct-release-note-ketoan

## v1.1.0 — 2026-09-18

### Changed
- Restructured from a single flat `SKILL.md` into the `ct-print-invoice-layout` convention: thin `SKILL.md` router + `spec/INPUT_SCHEMA.md` + `spec/PROMPT.md` + `spec/OUTPUT_SCHEMA.md` + `spec/GUARDRAILS.md`
- Removed the subtitle `<p>` under `<h1>` ("Tóm tắt các thay đổi mới nhất trên phần mềm...") per explicit user feedback — documented as a standing rule in `GUARDRAILS.md` and `OUTPUT_SCHEMA.md` so future runs don't re-add it

## v1.0.0 — 2026-09-18

### Added
- Initial release, extracted from the release-note session for `desktop-lamour`/`be-window-lamour`'s September report-rework commits
- Fixed 3-section HTML template (Tính năng mới / Đã sửa lỗi / Cần lưu ý) using Fraunces + Be Vietnam Pro fonts, light/dark mode tokens
- Technical → business-language translation table and menu-path-guessing caution
- Honesty rule for the "Cần lưu ý" section, motivated by an earlier undisclosed scope cut (a detail screen that only had its title changed, not its columns, without telling the user until asked)
- Standalone-HTML-only output (not a Claude Artifact) so the file can be opened/emailed without an account
