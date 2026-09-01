# Output Schema — ct-ui-consistency-review

Four known bug classes, in the order they're usually discovered (each one was found by fixing the previous one and having the user report the screen still looked/behaved wrong).

---

## Bug Class 1 — Missing vertical column borders

**Symptom:** a `DataGrid` shows horizontal row lines but no vertical lines between columns.

**Fix:** on the `<DataGrid>` element:
```xml
GridLinesVisibility="All"
HorizontalGridLinesBrush="#9CB8D4"
VerticalGridLinesBrush="#9CB8D4"
```
`#9CB8D4` (a muted blue-gray) is the color already established for this across the app — reuse it verbatim, don't invent a new shade. Leave `BorderBrush`/`BorderThickness` (the grid's own outer frame) untouched; those are a separate, already-correct concern.

**Skip this fix** for a grid with only 1–2 columns where the last column's `Width="*"` is legitimately just "fill remaining space" (e.g. a simple `Code | Name` reference list) — adding borders there is fine and cheap, but don't also apply Bug Class 2's width fix (see below).

---

## Bug Class 2 — Star-width column blocks horizontal scroll

**Symptom:** a `DataGrid` with many columns either (a) visibly squeezes all columns to fit the viewport instead of scrolling, or (b) has `ScrollViewer.HorizontalScrollBarVisibility="Auto"` already set yet still doesn't scroll.

**Root cause:** a trailing `DataGridTextColumn`/`DataGridTemplateColumn` with `Width="*"` absorbs all remaining space, so the grid's total content width always exactly equals the viewport — it never perceives itself as overflowing, so its internal `ScrollViewer` never shows a scrollbar. Adding `ScrollViewer.HorizontalScrollBarVisibility="Auto"` alone does nothing without also converting the star column.

**Fix (both parts required):**
1. Convert the star column to a reasonable fixed pixel width (`200`–`250` for a "Name"/"Description"/"Address" style column is the range already used across the app — match sibling columns' proportions rather than inventing a value).
2. Add to the `<DataGrid>` element:
   ```xml
   ScrollViewer.HorizontalScrollBarVisibility="Auto"
   ScrollViewer.VerticalScrollBarVisibility="Auto"
   ```

**When to skip:** a grid with only 2–4 short fixed-width columns plus one flexible "fill" column, where total content comfortably fits any reasonable window width — converting the star column there only produces dead whitespace with no scroll ever needed. Rule of thumb used in practice: skip if the grid has ≤4 columns and the non-star columns already sum to a modest width; fix if it has 6+ columns or the star column is genuinely long text (Diễn giải/Tên sản phẩm/Địa chỉ) next to many other columns.

**If this fix is applied and the user reports scroll STILL doesn't work** — do not re-apply the same fix or second-guess the width value. Go straight to Bug Class 3.

---

## Bug Class 3 — StackPanel-rooted custom `ControlTemplate` swallows the width constraint

**Symptom:** Bug Class 2's fix (star column converted, `ScrollViewer.HorizontalScrollBarVisibility="Auto"` set) is verifiably in place, yet the grid still silently overflows past the window edge with no scrollbar — and the grid lives inside a `TabItem` whose `TabControl` uses a **custom** `Style`/`ControlTemplate` (not the default WPF one).

**Root cause:** a vertical `StackPanel` gives its children **infinite available width** during layout — it only constrains the stacking (vertical) axis. If a custom `TabControl` `ControlTemplate` is rooted in `<StackPanel>` instead of `<Grid>`, every `TabItem`'s content (including a wide `DataGrid`) is measured with infinite width, so it grows to its full desired size instead of being clipped/scrolled by the real window width. The DataGrid's own internal `ScrollViewer` never activates, because from its own perspective it was never actually squeezed.

**Where this hides in `desktop-lamour`:** a Style key `AppTabControl.Modern` (or similarly-named custom tab styles) gets **copy-pasted locally into each window file** rather than shared via a common `ResourceDictionary` (a known, accepted pattern in this codebase per multiple doc comments) — so the same bug can exist independently in several files. Search for every local `<ControlTemplate TargetType="TabControl">` definition in the whole `src/` tree, not just the one file you're fixing.

**Fix:** change the template's root panel from `StackPanel` to `Grid` with two rows:
```xml
<ControlTemplate TargetType="TabControl">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>   <!-- tab header strip -->
            <RowDefinition Height="*"/>      <!-- selected tab content -->
        </Grid.RowDefinitions>
        <Border Grid.Row="0"> <!-- ... tab header Border, unchanged ... --> </Border>
        <Border Grid.Row="1"> <!-- ... content Border, unchanged ... --> </Border>
    </Grid>
</ControlTemplate>
```
Add `Grid.Row="0"`/`Grid.Row="1"` to the two existing `Border` elements; nothing else in the template needs to change — visual appearance (colors, corner radius, padding) is identical, only the width-propagation behavior changes.

**Verification note:** a `Grid`-rooted `TabControl` template with NO custom `Style` at all (plain `<TabControl>`) is never at risk — WPF's own default template is already `Grid`-based. Only check files that define their own custom `ControlTemplate TargetType="TabControl"`.

---

## Bug Class 4 — Shared control's color/shape has drifted from the app's design tokens

**Symptom:** user says a screen/control "looks out of place" / "doesn't match the rest of the app" without a specific technical complaint.

**Diagnosis checklist** — compare the flagged control against these established tokens (`Themes/DefaultTheme.xaml`) and patterns:

| Token | Value | Used for |
|---|---|---|
| `AppColor.TextBrand` / `AppColor.BorderBrand` / `AppColor.BorderActive` | `#F28A00` | Primary/brand accent — main actions (Thêm, Ghi sổ, Lập PN-style commit actions) |
| `AppColor.ButtonPrimary` | `#F28A00` | Filled primary button background |
| `AppColor.BackgroundBrand` | `#FFF3E0` | Light brand tint — hover state for brand-colored ribbon/ghost buttons |
| `AppColor.TextError` / `AppColor.ButtonDestructive` | `#D0021B` | Destructive actions (Xóa) |
| `AppColor.BackgroundErrorLight` | `#FDECEA` | Light red tint — hover state for destructive ghost buttons |
| `AppColor.TextSecondary` | `#6B6B6B` | Neutral/utility actions (navigation, In, Đóng, Treo, Bỏ ghi) |
| `AppColor.BorderThin` | `#E0E0E0` | Default hairline borders, card outlines |

Common drift found in practice: a locally-styled control (ribbon toolbar, custom button group) renders every icon/label in the same flat black/gray with no role-based coloring, while the rest of the app colors primary actions brand-orange and destructive actions red via `AppButton.Primary.Medium`/`AppButton.Destructive.Medium`/`AppButton.Secondary.Medium`. Also check the **container shape**: list-screen toolbars use a rounded card (`Margin="24,16,24,0"`, `CornerRadius="8"`, `BorderThickness="1"`, `BorderBrush="{StaticResource AppColor.BorderThin}"`) — a popup or embedded control using an edge-to-edge strip with only a single-side border reads as a different, older UI paradigm even if the colors are fixed.

**Fix:** add role-based `Style` variants (e.g. `X.Brand`, `X.Destructive`) for the icon/label/button-hover treatment, keyed to the semantic role of each action — don't recolor everything the same brand orange (that reads as noisy/wrong too; only genuinely primary actions get the accent, utility actions stay neutral gray). Match the container's margin/corner-radius/border to the rounded-card convention above unless the control is deliberately a top-level ribbon bar the user wants to keep (confirm via `AskUserQuestion` before changing shape — color and shape are separable asks, don't assume both are wanted from one vague complaint, see GUARDRAILS.md).

If the control is a **shared** `UserControl` (referenced by multiple screens), fix it once there — never duplicate the fix per-consumer.
