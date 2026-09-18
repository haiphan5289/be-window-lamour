# Prompt — ct-release-note-ketoan

> See [GUARDRAILS.md](GUARDRAILS.md) before writing any HTML — audience/jargon rules and fixed design constraints live there.
> Input parameters are defined in [INPUT_SCHEMA.md](INPUT_SCHEMA.md).
> Output shape is defined in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md).

---

## Step 1 — Gather the raw changes

Resolve `RANGE` per repo per [INPUT_SCHEMA.md](INPUT_SCHEMA.md)'s auto-detection, then run in both repos (skip a repo if its range has no commits):

```bash
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour
git log --oneline <RANGE>
git show --stat <RANGE>          # or git diff --stat for a multi-commit RANGE

cd /Users/hai.phan/Desktop/haiphan/desktop-lamour
git log --oneline <RANGE>
git show --stat <RANGE>
```

For each changed file that isn't obviously internal-only (DI wiring, `.csproj`, migration `Designer.cs` boilerplate), read the actual diff (`git show <RANGE> -- <path>`) to understand the *user-visible* effect — not just the filename.

---

## Step 2 — Classify every change into exactly one bucket

- **Tính năng mới** — a screen/field/report/button that is new or behaves in a new way the user must learn.
- **Đã sửa lỗi** — something that was wrong before and now works correctly; frame as "trước đây X, giờ đã sửa".
- **Cần lưu ý** — anything shipped incomplete, unverified, or with a known gap (e.g. a screen that only partially matches what was requested, a config not yet tested on production data). Be honest here — see [GUARDRAILS.md](GUARDRAILS.md)'s honesty rule.

Skip silently: pure refactors, internal renames, test-only changes, anything with zero effect on what the user sees or does — these have no bucket because they'd only confuse a non-technical reader.

---

## Step 3 — Rewrite each item in business language

Translate before writing anything — see the technical→business translation table and jargon rules in [GUARDRAILS.md](GUARDRAILS.md). One sentence per item is enough; two only if a caveat needs explaining. Never invent a menu path or screen name you haven't verified against the WPF views — if unsure, describe it generically ("trong màn danh sách chứng từ bán hàng") rather than guessing a wrong path.

---

## Step 4 — Generate the HTML file

Use the boilerplate in [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) **verbatim** for structure/CSS — only fill in the placeholders (`{{TITLE}}`, `{{DATE}}`, and the `.items`/`.callout` content). Do not redesign the template each time; the point is a consistent, recognizable format across releases so non-technical readers learn to scan it. Do not add the subtitle paragraph back under `<h1>` — it was deliberately removed (see [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md) note and [CHANGELOG.md](../CHANGELOG.md) v1.1.0).

Write the file as a **complete standalone HTML document** (`<!DOCTYPE html>` + `<html>` + `<head>` + `<body>` — this is a plain file the user opens by double-click or emails, not a Claude Artifact) to `OUTPUT` resolved per [INPUT_SCHEMA.md](INPUT_SCHEMA.md).

---

## Step 5 — Report back

Tell the user the file path (as a clickable relative link) and summarize in 1-2 sentences what was covered (which commits/date range). Do not auto-open a browser or auto-publish as a Claude Artifact — if the user wants a shareable web link instead of/in addition to the local file, mention that as an optional next step, but only publish one when asked:

```
💡 Muốn có link web để gửi nhanh (không cần mở file) thì nói mình publish thêm bản Artifact.
```

---

## Step 6 — Iterate on feedback

Expect the user to reply with a correction to one item's wording, one section's content, or a design tweak (like removing the subtitle paragraph). Apply it directly to the generated file **and** fold it back into [OUTPUT_SCHEMA.md](OUTPUT_SCHEMA.md)/[GUARDRAILS.md](GUARDRAILS.md) as a standing rule so the next run doesn't repeat the same fix — don't just patch the one file and forget the template.
