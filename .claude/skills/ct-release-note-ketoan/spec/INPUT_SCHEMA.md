# Input Schema — ct-release-note-ketoan

## Invocation Syntax

```
/ct-release-note-ketoan
```
Minimal — diffs the latest commit on `be-window-lamour` and its counterpart on `desktop-lamour` against their parent commit, writes to `release-note-<YYYYMMDD>.html` at the `be-window-lamour` repo root.

```
/ct-release-note-ketoan
RANGE: HEAD~3..HEAD
```
Explicit commit range — covers several commits at once (e.g. a week's worth of fixes) instead of just the latest.

```
/ct-release-note-ketoan
OUTPUT: release-note-thang9.html
```
Custom output filename/path (still written under the `be-window-lamour` repo root unless an absolute/relative path elsewhere is given).

---

## Parameters

| Parameter | Required | Auto-detect | Description |
|-----------|----------|-------------|-------------|
| `RANGE` | No | `HEAD~1..HEAD` on each repo independently | Git ref range passed to `git log`/`git diff --stat`/`git show`. The two repos are versioned independently — a range that has commits on one repo may have none on the other; skip the repo with nothing in range rather than erroring. |
| `OUTPUT` | No | `release-note-<YYYYMMDD>.html` (today's date) at the `be-window-lamour` repo root | Path to the output HTML file. Relative paths resolve against the `be-window-lamour` repo root. |

---

## Auto-Detection Logic

### `RANGE` — when not given

```bash
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour && git log --oneline -1
cd /Users/hai.phan/Desktop/haiphan/desktop-lamour  && git log --oneline -1
```
Use each repo's own latest commit vs. its parent (`<hash>~1..<hash>`, equivalently `HEAD~1..HEAD` if HEAD wasn't moved since). Do not assume the two repos are at "the same version" — they are separate git histories; always resolve the range per-repo.

### `OUTPUT` — when not given

```bash
date +%Y%m%d
```
→ `release-note-<result>.html` at `/Users/hai.phan/Desktop/haiphan/be-window-lamour/`.

If a file already exists at that path from an earlier run today, ask before overwriting rather than silently replacing it — it may already have been sent to someone.
