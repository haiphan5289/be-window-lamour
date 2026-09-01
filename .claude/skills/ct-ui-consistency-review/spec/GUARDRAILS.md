# Guardrails — ct-ui-consistency-review

## Core Rules

### 1. Never fix from memory — always audit first, even on a "whole app" request you've handled before

Every round of the session that produced this skill under-estimated the blast radius on the first pass: a request scoped to "these columns" turned into 7 grids, then into 28 files, then into a completely different bug class (StackPanel) that a naive re-application of the same fix would not have caught. Dispatch an `Explore` agent (or several, for genuinely distinct sub-questions) to build a complete inventory before editing anything on a `whole app` scope. Do not trust a prior session's file list as complete for a new symptom — Bug Class 3 was only found because the user reported the SAME screen still broken after Bug Class 2's fix was verifiably applied everywhere the first audit found.

### 2. Distinguish "still broken" from "not applied yet"

If the user reports a symptom persisting after you already applied a known fix, do not assume you mis-applied it or picked the wrong pixel value. Re-verify the fix is actually in place (read the file back), and if it is, look for a **different, upstream** cause (see Bug Class 3) rather than iterating on the same fix. Two consecutive fixes to the same file for the "same" symptom is a signal you're treating the wrong layer.

### 3. Don't convert every star-width column reflexively

A grid with 1–4 short columns where the last one is `Width="*"` is very often already correct — that's the intended "fill remaining space" behavior for a simple list, and converting it to a fixed width just leaves dead whitespace with zero scroll benefit. Check total column count and realistic content width before converting; see the skip rule in OUTPUT_SCHEMA.md Bug Class 2. Blanket-converting every `Width="*"` found by a grep is over-fixing.

### 4. A vague "looks out of place" complaint is at least two separable asks — confirm before doing both

"Lạc lõng" / "doesn't match the app" can mean color, shape, or both. In practice this session, the user was asked one targeted yes/no question ("is the color the issue?") and it turned out to be color **and** shape both — but that only came out because the question was asked, not assumed. Use `AskUserQuestion` with a specific, falsifiable diagnosis ("X uses flat black icons while the rest of the app uses brand orange for primary actions — is that it?") rather than silently redesigning a shared control's full look. A shared control's restyle affects every screen that consumes it — get the direction confirmed before touching it.

### 5. Fix shared controls once, at the source

Before editing color/shape on what looks like a single screen's toolbar/control, check whether it's actually a shared `UserControl` (e.g. `Shared/Controls/DocumentToolbar.xaml`) consumed by multiple windows. Grep for the control's tag name across `src/` first. Fixing it in the shared file automatically propagates to every consumer — don't hunt down and patch each window individually, and don't be surprised when a "one popup" fix visibly changes 4 other popups too; that's correct, say so explicitly in the summary.

### 6. Custom per-file `ControlTemplate`/`Style` duplication is a known pattern here, not a mistake to "clean up" mid-task

This codebase deliberately copy-pastes some styles (e.g. `AppTabControl.Modern`) locally into each window file instead of centralizing them in a shared `ResourceDictionary`, per existing doc comments explaining this was a conscious choice. When Bug Class 3 requires editing this style, fix **every duplicate independently** (grep for the `x:Key` across all of `src/`) — do not take the opportunity to also refactor them into a shared dictionary unless the user asked for that; that's a separate, larger change with its own risk profile.

### 7. Verify colors/tokens against `Themes/DefaultTheme.xaml`, never guess a hex value

Every color used in a fix must be a token already defined in `Themes/DefaultTheme.xaml` (or the exact `#9CB8D4` border color already established for grid lines across the app) — grep for the token name to confirm its value before using it, never invent a new shade that happens to look similar. Introducing a slightly-off new color is its own form of inconsistency.

## Common Pitfalls

### Build succeeds ≠ visually correct

`dotnet build` catches XAML parse/compile errors only. None of the four bug classes above produce a build error — they're all purely visual/runtime layout behavior, verifiable only by the user's screenshot or a real run on the UTM Windows VM (a Mac cannot render the WPF UI). Never declare a UI-consistency fix "done" on the strength of a clean build alone; say plainly that UTM/screenshot verification is the real check.

### Repo hygiene: `desktop-lamour` tracks `bin`/`obj` in git

After every local build, restore + clean tracked build artifacts before reviewing the diff:
```bash
git checkout -- src/DesktopLamour/bin src/DesktopLamour/obj
git clean -fdq src/DesktopLamour/bin src/DesktopLamour/obj
git status --porcelain
```
Never commit/push without the user's explicit go-ahead.
