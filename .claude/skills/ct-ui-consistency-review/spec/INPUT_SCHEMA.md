# Input Schema — ct-ui-consistency-review

## Invocation Syntax

```
/ct-ui-consistency-review
SCOPE: SalesReturnWindow popup
SYMPTOM: toolbar looks out of place — plain black icons, no brand color
```
One specific screen/control, one specific symptom already identified by the user.

```
/ct-ui-consistency-review
SCOPE: whole app
SYMPTOM: missing column borders on product-line grids
```
Broad request — the user pointed at one screenshot as an *example* and said "cho toàn bộ app luôn" (apply to the whole app). This is the common case in practice: every round of this skill's originating session started scoped to one popup and was explicitly widened to "whole app" on the next message.

```
/ct-ui-consistency-review
```
Minimal — the skill asks which screen(s) and what looks wrong (see Step 0 in [PROMPT.md](PROMPT.md)).

---

## Parameters

| Parameter | Required | Auto-detect | Description |
|-----------|----------|-------------|-------------|
| `SCOPE` | No | Ask the user | `<ScreenName> popup/view` for one screen, or `whole app` / `toàn bộ app` for an app-wide sweep |
| `SYMPTOM` | No | Infer from an attached screenshot, or ask | Free text: what looks wrong — missing borders, can't scroll, colors/shape don't match, etc. |

A screenshot attached to the request is the primary signal — read it before asking anything (see PROMPT.md Step 0).

---

## Known Symptom → Bug Class Map

Use this to route straight to the right fix pattern (detailed in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md)) instead of re-diagnosing from scratch:

| User's symptom, in their own words | Likely bug class |
|---|---|
| "cột không có border/kẻ" (columns have no dividing lines) | **Bug Class 1** — missing vertical `GridLinesVisibility` |
| "cột bị bóp lại" / "không cuộn được dù đã thêm ScrollViewer" (columns squeezed / still can't scroll after adding the scroll flag) | **Bug Class 2** (star-width column) — and if the grid is inside a `TabItem`/custom-styled `TabControl`, also check **Bug Class 3** |
| "vẫn không scroll được" reported AFTER Bug Class 2's fix was already applied | **Bug Class 3** — StackPanel-rooted custom `ControlTemplate` upstream is swallowing the width constraint; Bug Class 2's fix alone cannot work until this is also fixed |
| "lạc lõng", "không giống app", "màu/khung không khớp" (looks out of place / colors or shape don't match) | **Bug Class 4** — shared control's color palette or container shape has drifted from `AppColor`/`AppButton` conventions |

---

## Scope Behavior

| `SCOPE` | Behavior |
|---|---|
| One named screen | Fix only that screen/control. Still check whether the fix lives in a **shared** control (e.g. `DocumentToolbar.xaml`) — if so, say so explicitly, since the fix will affect every consumer of that control even though the request named only one screen. |
| `whole app` | Run the full audit workflow (Step 1 in PROMPT.md) — dispatch a broad `Explore` inventory before touching any file. Never hand-fix a handful of files you happen to remember from a prior session; the inventory step exists because "whole app" audits in this codebase have repeatedly turned up 2–3x more affected files than expected on the first guess. |
