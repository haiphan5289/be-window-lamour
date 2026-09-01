# Prompt — ct-ui-consistency-review

> See [GUARDRAILS.md](GUARDRAILS.md) before editing any XAML.
> Input parameters are defined in [INPUT_SCHEMA.md](INPUT_SCHEMA.md).
> The four bug classes and their exact fixes are defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

---

## Pre-flight — Anti-Hallucination Verification (MANDATORY, runs BEFORE Step 0)

Load and apply all rules from:
@.claude/skills/ct-anti-hallucination/SKILL.md

**Fallback** — if the @-reference does not resolve:
```
Read /Users/hai.phan/Desktop/haiphan/be-window-lamour/.claude/skills/ct-anti-hallucination/SKILL.md
```

Every Style key, color token, and file path used below must be verified against `desktop-lamour` before it appears in generated code — grep for it, don't recall it from a prior session.

---

## Step 0 — Identify the symptom and route to a bug class

1. If a screenshot is attached, read it first — it's the primary evidence, more reliable than the user's verbal description.
2. Map the reported symptom to a bug class using the table in [INPUT_SCHEMA.md](INPUT_SCHEMA.md) "Known Symptom → Bug Class Map".
3. If the symptom is "still broken after a fix you already applied earlier in this conversation", do NOT re-apply that fix — assume it's a different, upstream bug class (almost always Bug Class 3 if the prior fix was Bug Class 2) and re-diagnose from there (see GUARDRAILS.md Rule 2).
4. If the complaint is vague ("lạc lõng", "không giống app") with no specific screenshot detail, ask ONE targeted, falsifiable diagnosis question via `AskUserQuestion` before touching anything — see GUARDRAILS.md Rule 4 and the example question in OUTPUT_SCHEMA.md Bug Class 4.

---

## Step 1 — Audit (mandatory for `whole app` scope, recommended even for one screen)

Never fix from memory of a similar past task. Dispatch `Explore` agent(s) to build a complete inventory:

- **Bug Class 1/2 (borders/scroll):** search every `<DataGrid>` in `src/DesktopLamour/Features/**/Views/*.xaml` for the specific attribute state (missing `GridLinesVisibility="All"`, or a column with `Width="*"`) — do not hand-pick files from memory.
- **Bug Class 3 (StackPanel template):** search the WHOLE `src/` tree (not just `Features/`) for every custom `<ControlTemplate TargetType="TabControl">`, and separately verify no other `DataGrid` has a `StackPanel` ancestor between it and its `Grid`/`Window` root (walk the real XML element tree, not a line-based grep, to avoid false positives from sibling `StackPanel`s).
- **Bug Class 4 (color/shape):** grep for the control's tag name across `src/` to find every consumer before deciding it's "just this one screen".

Wait for the inventory before writing a single line of XAML. Report the inventory back to the user in a short table if the scope is `whole app` and the result is large (matches the pattern already established this session — the user responded well to seeing exact file counts before a big batch of edits).

---

## Step 2 — Confirm scope for anything non-mechanical

- Bug Class 1/2 fixes are mechanical (fixed color/pattern already established) — proceed directly once the inventory is in hand, no need to ask.
- Bug Class 3 fixes are mechanical once the affected files are found — proceed directly.
- Bug Class 4 (color/shape restyle) is a judgment call — confirm via `AskUserQuestion` per GUARDRAILS.md Rule 4 before changing a shared control's look, especially if both color AND shape might be in play.
- If a "whole app" audit turns up items that don't clearly belong (e.g. a 1–2 column grid where the star-width column is legitimately correct, per OUTPUT_SCHEMA.md's skip rules) — state which ones you're deliberately excluding and why, don't silently omit them without explanation.

---

## Step 3 — Apply fixes

Follow the exact patterns in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) for whichever bug class(es) apply. Batch same-shaped edits across files efficiently (many list screens share near-identical `DataGrid` attribute blocks — a single `Edit` with the right `old_string` often matches verbatim across several files, edit them individually since `Edit` is scoped per file-path, but the `old_string`/`new_string` pair can be reused as-is).

Apply GUARDRAILS.md Rules 3, 5, 6, 7 while editing (don't over-convert star columns, fix shared controls once, fix every template duplicate independently, verify color tokens).

---

## Step 4 — Build and clean

```bash
cd /Users/haiphan/Desktop/haiphan/desktop-lamour
dotnet build src/DesktopLamour/DesktopLamour.csproj -c Debug
```
Confirm `0 Warning(s) / 0 Error(s)`. Then, since `desktop-lamour` tracks `bin`/`obj` directly in git:
```bash
git checkout -- src/DesktopLamour/bin src/DesktopLamour/obj
git clean -fdq src/DesktopLamour/bin src/DesktopLamour/obj
git status --porcelain
```
Confirm only the intended source files remain modified.

---

## Step 5 — Report honestly, don't over-claim

A clean build proves nothing about visual correctness for any of these four bug classes (see GUARDRAILS.md "Build succeeds ≠ visually correct") — state explicitly that real verification happens on the UTM Windows VM or via the user's next screenshot. Summarize:
- Which bug class(es) were found and fixed
- The exact file list (a table if it's more than ~5 files)
- Anything deliberately skipped and why
- Whether the fix touched a shared control (and therefore affected screens beyond the one originally reported)

Never commit or push without the user's explicit go-ahead.

---

## Step 6 — Iterate

Expect a follow-up screenshot or a "still not working" report. Go back to Step 0 and re-route — per GUARDRAILS.md Rule 2, a repeat symptom after a verified fix means a different bug class, not a bad pixel value.
