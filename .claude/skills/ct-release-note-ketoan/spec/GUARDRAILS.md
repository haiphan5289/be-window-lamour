# Guardrails — ct-release-note-ketoan

## Audience rule (do not violate)

The reader is a non-technical business user of `desktop-lamour` (kế toán, thu ngân, quản lý cửa hàng) — not a developer. Never mention in the visible text:
- File names, class names, DTOs, migrations, entity names
- HTTP routes, JSON field names
- Commit hashes, branch names, PR numbers
- Any English technical term without a Vietnamese business equivalent

Describe every change by **screen name (menu path if known) + what changed on it + why it matters to their work**.

## Technical → business translation table (extend as new patterns appear)

| Technical (do NOT write this) | Business language (write this instead) |
|---|---|
| "Added `District`/`Ward` fields to `Customer` entity + migration" | "Form Khách hàng có thêm 2 ô nhập: Quận/Huyện, Xã/Phường" |
| "Fixed `DataGridCell` selected-state Foreground override in `SalesOrderListView.xaml`" | "Dòng chứng từ 'Treo' mất màu khi bấm chọn — đã sửa" |
| "Extended `SummaryDimension` enum + `ColumnConfigs` dictionary" | "Một vài báo cáo bán hàng được chỉnh lại đúng cột hiển thị theo mẫu chuẩn hơn" |
| "New `SalesOrderReportTypes.ByEmployeeUnit` report type" | "Thêm báo cáo mới theo 'Đơn vị kinh doanh' (phòng ban)" |

## Menu-path caution

Never invent a menu path or screen name you haven't verified against the actual WPF views. If unsure, describe the location generically ("trong màn danh sách chứng từ bán hàng") rather than guessing a wrong path — a wrong path sends a non-technical reader looking in the wrong place, which is worse than being vague.

## Fixed section set — do not add more

Exactly 3 top-level sections, always in this order: **Tính năng mới / Đã sửa lỗi / Cần lưu ý**. Don't add extra sections (e.g. "Hiệu năng", "Bảo mật", "Ghi chú kỹ thuật") even if the commits would justify one technically — extra sections dilute a page meant to be skimmed in under a minute by someone who isn't going to read a changelog carefully. Fold anything that doesn't fit one of the 3 buckets into the nearest one, or drop it (see PROMPT.md Step 2's "skip silently" list).

## No restating-the-obvious subtitle

Do not add a subtitle paragraph under `<h1>` explaining what the page is (e.g. "Tóm tắt các thay đổi..."). Removed 2026-09-18 per explicit user feedback — the eyebrow line (product name + date) and the section tags (MỚI/SỬA LỖI) already make the page's purpose obvious at a glance; a subtitle restating that is dead weight on a page meant to be scanned fast.

## Honesty rule for "Cần lưu ý"

This section exists specifically because an earlier session shipped a partial implementation (a detail screen that only had its title changed, not its columns, to match a MISA reference) without disclosing the scope cut to the user until asked directly. Always surface known gaps here, named specifically:
- ❌ "Một số phần còn hạn chế" (vague, hides the actual gap)
- ✅ "Sổ chi tiết bán hàng theo Nhân viên: hiện mới chỉ đổi đúng tên tiêu đề, các cột số liệu bên trong tạm thời vẫn giữ như màn chi tiết thông thường"

If there is genuinely nothing to flag, omit the whole callout block (see OUTPUT_SCHEMA.md) rather than inventing a caveat to fill it.

## Fixed design — do not redesign per run

- **Fonts**: "Fraunces" (headings) + "Be Vietnam Pro" (body — full Vietnamese diacritic support, unlike many Google fonts). Never swap these without the user asking.
- **Colors/layout**: use the [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) boilerplate verbatim. The point of a fixed template across releases is that readers learn to scan it — a new palette or layout each time defeats that.
- **Standalone file only**: full `<!DOCTYPE html>` document, not a Claude Artifact fragment — the user opens it by double-click or emails it directly, no account needed. Only publish via the Artifact tool if the user explicitly asks for a shareable web link (mention it as an optional next step; never do it unprompted).

## Skip silently (no bucket)

Pure refactors, internal renames, test-only changes, DI wiring, `.csproj` edits, migration `Designer.cs` boilerplate — anything with zero user-visible effect. Listing these would only confuse a non-technical reader with things that don't affect what they see or do.
