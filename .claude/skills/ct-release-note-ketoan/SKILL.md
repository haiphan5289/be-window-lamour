---
name: ct-release-note-ketoan
description: "Generate a plain-language, non-technical HTML release note summarizing recent code changes (features/bug fixes/caveats) for business/accounting staff (kế toán, thu ngân) who use desktop-lamour — not developers. Use when the user asks for a changelog, release note, or update summary 'dễ hiểu cho kế toán/người dùng', or asks to translate a technical CHANGELOG into plain language."
argument-hint: "[RANGE: <git range, e.g. HEAD~1..HEAD — default: latest commit on each repo>] [OUTPUT: <file path — default: release-note-<date>.html at be-window-lamour root>]"
---

# ct-release-note-ketoan — Plain-Language Release Note for Business Users

Turn recent BE/WPF commits into a short, non-technical HTML page that accounting/cashier staff can read without knowing what a migration, a DTO, or a ViewModel is. This is the audience-translation counterpart to the developer-facing `CHANGELOG.md` — same underlying changes, described in terms of what the user sees on screen and what changed in their daily workflow.

> **Audience rule (the whole point of this skill):** the reader is a non-technical business user of `desktop-lamour` (kế toán, thu ngân, quản lý cửa hàng). Never mention file names, class names, DTOs, migrations, entities, HTTP routes, or commit hashes in the output body. Describe changes by **screen name + what changed on it + why it matters to their work**.

---

## How to Use

**Default — latest commit on each repo:**
```
/ct-release-note-ketoan
```

**Explicit range (covers several commits at once — e.g. a week's worth of fixes) + custom output path:**
```
/ct-release-note-ketoan
RANGE: HEAD~3..HEAD
OUTPUT: release-note-thang9.html
```

All parameters are optional — see [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) for auto-detection when omitted.

---

## File Structure

| File | Purpose |
|------|---------|
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Invocation syntax and parameter reference |
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | Required standalone HTML structure/template (verbatim boilerplate) |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Audience/jargon rules, fixed design constraints, honesty rules already solved once |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

---

## Execution

Load and execute: **[spec/PROMPT.md](spec/PROMPT.md)**

Fallback — if @-references do not resolve, use the Read tool:
```
Read /Users/hai.phan/Desktop/haiphan/be-window-lamour/.claude/skills/ct-release-note-ketoan/spec/PROMPT.md
```
