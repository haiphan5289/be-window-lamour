# Output Schema — ct-release-note-ketoan

## Required structure

A single, complete, standalone `.html` file (`<!DOCTYPE html>` → `</html>`) — never a fragment meant for the Artifact tool's auto-wrapping. Exactly 3 content sections in this fixed order: **Tính năng mới → Đã sửa lỗi → Cần lưu ý**. See [GUARDRAILS.md](GUARDRAILS.md) for why the section set is fixed and why no subtitle paragraph appears under `<h1>`.

## Full HTML boilerplate (use verbatim — only fill `{{...}}` placeholders)

```html
<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>{{TITLE}} - Phần Mềm Lamour</title>
<link rel="preconnect" href="https://fonts.googleapis.com">
<link href="https://fonts.googleapis.com/css2?family=Fraunces:wght@500;600;700&family=Be+Vietnam+Pro:wght@400;500;600;700&display=swap" rel="stylesheet">
<style>
  :root{
    --bg:#FAF8F5; --surface:#FFFFFF; --surface-2:#F2EFE9;
    --text:#242220; --muted:#6E6A63; --border:#E4E0D8;
    --accent:#2F6B5C; --accent-soft:#E3EFEA;
    --accent-2:#B4622A; --accent-2-soft:#F5E9DE;
    --accent-3:#8A5FB0; --accent-3-soft:#EFE7F5;
  }
  @media (prefers-color-scheme: dark){
    :root{
      --bg:#1B1917; --surface:#242220; --surface-2:#2C2A27;
      --text:#EFEBE4; --muted:#A39D92; --border:#3A3733;
      --accent:#7FBFA9; --accent-soft:#28352F;
      --accent-2:#E0975A; --accent-2-soft:#3A2C1F;
      --accent-3:#C4A3E0; --accent-3-soft:#332A3D;
    }
  }
  * { box-sizing: border-box; }
  html, body { margin: 0; padding: 0; }
  body{ background: var(--bg); color: var(--text); font-family: "Be Vietnam Pro", "Segoe UI", sans-serif; padding-inline: 16px; padding-block: 32px; }
  h1,h2,h3{ font-family: "Fraunces", Georgia, serif; font-weight: 600; margin: 0; }
  .wrap{ max-width: 720px; margin-inline: auto; display: flex; flex-direction: column; gap: 40px; }
  .head{ display:flex; flex-direction: column; gap: 10px; padding-bottom: 24px; border-bottom: 1px solid var(--border); }
  .head .eyebrow{ display:flex; align-items:center; gap:10px; font-size: 13px; letter-spacing: 0.06em; text-transform: uppercase; color: var(--muted); font-weight: 600; }
  .head .eyebrow .dot{ width:6px; height:6px; border-radius:50%; background: var(--accent); }
  .head h1{ font-size: clamp(28px, 5vw, 38px); text-wrap: balance; }
  .section{ display:flex; flex-direction: column; gap: 16px; }
  .section-head{ display:flex; align-items: baseline; gap: 12px; }
  .section-head h2{ font-size: 21px; }
  .tag{ font-size: 12px; font-weight: 700; letter-spacing: 0.04em; padding: 3px 10px; border-radius: 999px; white-space: nowrap; }
  .tag.new{ background: var(--accent-soft); color: var(--accent); }
  .tag.fix{ background: var(--accent-2-soft); color: var(--accent-2); }
  .tag.note{ background: var(--accent-3-soft); color: var(--accent-3); }
  .items{ display:flex; flex-direction: column; gap: 1px; background: var(--border); border: 1px solid var(--border); border-radius: 10px; overflow: hidden; }
  .item{ background: var(--surface); padding: 16px 18px; display:flex; flex-direction: column; gap: 4px; }
  .item .title{ font-weight: 600; font-size: 15.5px; }
  .item .desc{ color: var(--muted); font-size: 14.5px; line-height: 1.6; }
  .item .desc b{ color: var(--text); font-weight: 600; }
  .callout{ background: var(--surface-2); border: 1px solid var(--border); border-radius: 10px; padding: 18px 20px; display:flex; flex-direction: column; gap: 10px; }
  .callout .title{ font-weight: 700; font-size: 14.5px; color: var(--accent-3); }
  .callout ul{ margin:0; padding-left: 20px; display:flex; flex-direction: column; gap: 8px; color: var(--muted); font-size: 14.5px; line-height: 1.6; }
  .callout li b{ color: var(--text); font-weight: 600; }
  footer{ color: var(--muted); font-size: 13px; text-align: center; padding-top: 8px; }
  @media (max-width: 480px){ .section-head{ flex-wrap: wrap; } }
</style>
</head>
<body>
<div class="wrap">

  <div class="head">
    <div class="eyebrow"><span class="dot"></span>Phần mềm Lamour · {{DATE}}</div>
    <h1>{{TITLE}}</h1>
  </div>

  <div class="section">
    <div class="section-head"><h2>Tính năng mới</h2><span class="tag new">MỚI</span></div>
    <div class="items">
      <!-- repeat per item:
      <div class="item">
        <div class="title">{{Tên tính năng ngắn}}</div>
        <div class="desc">{{Mô tả 1 câu, nói rõ ở màn nào, ảnh hưởng gì}}</div>
      </div>
      -->
    </div>
  </div>

  <div class="section">
    <div class="section-head"><h2>Đã sửa lỗi</h2><span class="tag fix">SỬA LỖI</span></div>
    <div class="items">
      <!-- repeat per item, same structure -->
    </div>
  </div>

  <!-- omit this whole block entirely if there is nothing to flag -->
  <div class="callout">
    <div class="title">Cần lưu ý khi dùng</div>
    <ul>
      <!-- <li><b>{{Tên phần chưa đầy đủ}}</b>: {{giải thích ngắn, trung thực}}.</li> -->
    </ul>
  </div>

  <footer>Có thắc mắc gì cứ nhắn lại — bản ghi chi tiết kỹ thuật lưu ở file CHANGELOG.md trong dự án.</footer>

</div>
</body>
</html>
```

## Placeholders

| Placeholder | Source | Example |
|---|---|---|
| `{{TITLE}}` | Short name for the release, 2-4 words, no date suffix needed since the eyebrow line already carries the date | "Cập nhật tháng 9" |
| `{{DATE}}` | `DD/MM/YYYY` of the newest commit in range | "18/09/2026" |

## Notes

- **No subtitle `<p>` under `<h1>`** — an earlier draft had `<p>Tóm tắt các thay đổi mới nhất trên phần mềm, viết ngắn gọn cho bộ phận kế toán — không cần đọc phần kỹ thuật.</p>` directly under the `.head` block. Removed 2026-09-18 per explicit user feedback ("bỏ text ..."). Do not re-add it or any equivalent restating-the-obvious subtitle — the eyebrow line + section tags already communicate what the page is.
- If there is nothing to put in "Cần lưu ý", omit the entire `.callout` block — don't leave an empty section with a header and no items.
