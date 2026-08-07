# Input Schema — Build Doctor

## Input Block

```yaml
MODE: auto | targeted | dry-run
ERROR_TYPES: [optional] comma-separated list when MODE=targeted
MAX_ITERATIONS: [optional] default=3
```

## Field Definitions

| Field | Type | Required | Description |
|---|---|---|
| `MODE` | `Enum` | Yes | Execution mode: `auto`, `targeted`, `dry-run` |
| `ERROR_TYPES` | `String` | No | Filter specific error types (only when MODE=targeted) |
| `MAX_ITERATIONS` | `Int` | No | Max fix-validate cycles (default: 3) |

## MODE Options

### `auto` (Default)

Fix all detectable errors automatically.

```yaml
MODE: auto
```

**Behavior:**
1. Scan all build errors
2. Classify by pattern
3. Auto-fix all fixable errors (confidence ≥ 70%)
4. Re-validate after each batch
5. Repeat up to MAX_ITERATIONS
6. Report remaining errors

**Use when:** After generating new code, adding model fields, scaffolding screens.

---

### `targeted`

Fix only specific error types.

```yaml
MODE: targeted
ERROR_TYPES: missing_argument,type_mismatch
```

**Available ERROR_TYPES:**
- `missing_argument`
- `type_mismatch`
- `missing_import`
- `undefined_symbol`
- `async_context`
- `property_wrapper_misuse`
- `protocol_conformance`

**Use when:** You know the specific error pattern and want focused fixes.

---

### `dry-run`

Scan and report only — no actual fixes applied.

```yaml
MODE: dry-run
```

**Behavior:**
1. Scan all build errors
2. Classify by pattern
3. Show what WOULD be fixed
4. Report confidence scores
5. No file modifications

**Use when:** Auditing error landscape before committing to fixes.

---

## MAX_ITERATIONS

Controls how many fix-validate cycles to run.

```yaml
MODE: auto
MAX_ITERATIONS: 5
```

**Default:** 3

**Why limit iterations:**
- Prevent infinite loops if errors regenerate
- Some errors might be unfixable automatically
- Safety mechanism

**Iteration flow:**
```
Iteration 1: Fix batch → Validate → 8 errors remaining
Iteration 2: Fix batch → Validate → 3 errors remaining
Iteration 3: Fix batch → Validate → 0 errors remaining ✅
```

If after MAX_ITERATIONS errors remain:
→ Stop and report for manual intervention

---

## Examples

### Example 1: Full Auto-Fix

```yaml
MODE: auto
```

Fixes everything automatically until clean build or max iterations reached.

---

### Example 2: Fix Only Missing Arguments

```yaml
MODE: targeted
ERROR_TYPES: missing_argument
```

Only fixes missing parameter errors (like the `amount` case). Ignores all other errors.

---

### Example 3: Audit Before Fix

```yaml
MODE: dry-run
```

Shows what would be fixed without actually modifying files. Review output, then run with MODE=auto if acceptable.

---

### Example 4: Aggressive Fixing

```yaml
MODE: auto
MAX_ITERATIONS: 10
```

Allow up to 10 fix cycles. Use when dealing with cascading errors across many files.

---

## Output Schema

Regardless of MODE, output a structured report:

```yaml
status: success | partial | failed
initial_error_count: int
final_error_count: int
iterations_used: int
fixed_errors:
  - category: string
    count: int
    files: [string]
remaining_errors:
  - file: string
    line: int
    message: string
    category: string
    auto_fixable: bool
    suggestion: string
```

### Example Output

```yaml
status: success
initial_error_count: 12
final_error_count: 0
iterations_used: 2
fixed_errors:
  - category: missing_argument
    count: 5
    files:
      - CustomerDetailScreen.swift
      - CustomerRowView.swift
      - CustomerFormSheet.swift
      - TransactionRowView.swift
      - DashboardStatCard.swift
  - category: missing_import
    count: 3
    files:
      - CustomerViewModel.swift
      - TransactionViewModel.swift
      - DashboardRepository.swift
  - category: type_mismatch
    count: 4
    files:
      - CustomerFormViewModel.swift
      - TransactionFormViewModel.swift
remaining_errors: []
```

---

## Error Handling

### If skill fails to parse error output

```yaml
status: failed
error: "Unable to parse compiler errors. Check Xcode output format."
```

### If MAX_ITERATIONS exceeded

```yaml
status: partial
message: "Max iterations (3) reached. 2 errors remain unfixed."
remaining_errors: [...]
```

### If no errors found initially

```yaml
status: success
message: "Build is clean. No errors to fix."
initial_error_count: 0
final_error_count: 0
```
