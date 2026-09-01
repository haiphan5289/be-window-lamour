---
name: ct-ui-consistency-review
description: "Audit and fix WPF UI consistency issues across desktop-lamour — DataGrid column borders, star-width columns that silently block horizontal scroll, StackPanel-vs-Grid width-constraint bugs in custom ControlTemplates, and shared-control color/shape drift from the app's AppColor/AppButton system. Use when asked to make a screen (or \"the whole app\") match the rest of the UI, add borders, fix scroll, or restyle a toolbar/control to feel consistent."
argument-hint: "[SCOPE: one screen | whole app] [SYMPTOM: missing borders | can't scroll | looks out of place]"
---

# ct-ui-consistency-review — WPF UI Consistency Audit & Fix

Find and fix the specific, recurring classes of visual/layout inconsistency in `desktop-lamour`'s WPF screens — reused across a whole-app audit that started from one popup's grid borders and ended up fixing 28+ files across three distinct bug classes. This skill exists so the next "make it consistent" request doesn't re-discover the same three gotchas from scratch.

> **Anti-Hallucination:** Verify every file path, Style key, and color token against the codebase before generating code. See [ct-anti-hallucination](../ct-anti-hallucination/SKILL.md).
> **Client-only:** all work happens in `desktop-lamour` — a visual-consistency task never needs a BE change.

---

## How to Use

**A single screen looks off:**
```
/ct-ui-consistency-review
SCOPE: SalesReturnWindow popup
SYMPTOM: toolbar looks out of place — plain black icons, no brand color
```

**Broad, whole-app request** (the common case — user says "cho toàn bộ app luôn"):
```
/ct-ui-consistency-review
SCOPE: whole app
SYMPTOM: missing column borders on product-line grids
```

**Minimal — let the skill ask:**
```
/ct-ui-consistency-review
```

---

## File Structure

| File | Purpose |
|------|---------|
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Invocation syntax and parameter reference |
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step audit → confirm → fix → verify workflow |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | The three known bug classes, exact fix patterns, and color/token reference |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Scope-creep and false-positive traps already hit once |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

---

## Execution

Load and execute: **[spec/PROMPT.md](spec/PROMPT.md)**

Fallback — if @-references do not resolve, use the Read tool:
```
Read /Users/hai.phan/Desktop/haiphan/be-window-lamour/.claude/skills/ct-ui-consistency-review/spec/PROMPT.md
```
