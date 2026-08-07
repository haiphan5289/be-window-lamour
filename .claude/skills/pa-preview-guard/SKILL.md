---
name: pa-preview-guard
description: Tự động phát hiện, sửa, và ngăn chặn TẤT CẢ lỗi build liên quan đến
model: sonnet
effort: high
---

# Build Doctor — iOS App

**Post-generation auto-fixer** - Tự động phát hiện và sửa TẤT CẢ lỗi build sau khi generate code thành công. Không cần fix từng case thủ công nữa.

## Vấn đề giải quyết

Sau khi generate code (add field, new screen, new UseCase), thường gặp các lỗi:
- ❌ Missing argument in call (preview, test, factory)
- ❌ Type mismatch (parameter type thay đổi)
- ❌ Missing imports (Foundation, Combine, SwiftUI)
- ❌ Undefined symbols (typo, wrong class name)
- ❌ Protocol conformance errors
- ❌ Property wrapper misuse (@Published, @State)
- ❌ Async/await context errors

## Giải pháp

Skill này chạy **sau mỗi lần generate code** để:
1. **Scan** tất cả build errors từ Xcode
2. **Classify** errors theo pattern (missing param, type error, import, etc.)
3. **Auto-fix** mỗi loại error với strategy phù hợp
4. **Re-validate** cho đến khi build clean
5. **Report** kết quả: files fixed, errors resolved

## Files

| File | Purpose |
|---|---|
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow |
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Input parameters |
| [spec/ERROR_PATTERNS.md](spec/ERROR_PATTERNS.md) | Error classification & fix strategies |
| [spec/FIX_STRATEGIES.md](spec/FIX_STRATEGIES.md) | Per-error-type auto-fix logic |
| [spec/EXAMPLES.md](spec/EXAMPLES.md) | Worked examples |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Safety rules |

## Quick Start

```
# Mode 1: Auto-scan và fix tất cả errors
MODE: auto

# Mode 2: Fix specific error types only
MODE: targeted
ERROR_TYPES: missing_argument,type_mismatch

# Mode 3: Dry-run (report only, không fix)
MODE: dry-run
```

## Output

```
✅ Build Doctor Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Errors scanned: 12
🔧 Errors fixed: 11
❌ Errors remaining: 1 (manual intervention needed)

Fixed breakdown:
  ✅ Missing arguments: 5 files
  ✅ Type mismatches: 3 files
  ✅ Missing imports: 3 files

Manual fixes needed:
  ❌ ViewModel.swift:45 - Complex async/await refactor
```
