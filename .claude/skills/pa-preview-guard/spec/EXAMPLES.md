# Examples — Build Doctor

## Example 1: Auto-Fix After Adding Model Field

### Scenario

You added `amount: Double` to the `Customer` model. Now 5 preview blocks break.

### Input

```yaml
MODE: auto
```

### Execution Log

```
🔍 Stage 1: Scanning build errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 5 errors:
  ✗ CustomerDetailScreen.swift:208 - Missing argument 'amount'
  ✗ CustomerRowView.swift:42 - Missing argument 'amount'
  ✗ CustomerFormSheet.swift:88 - Missing argument 'amount'
  ✗ TransactionRowView.swift:43 - Missing argument 'amount'
  ✗ DashboardHomeScreen.swift:188 - Missing argument 'amount'

🔧 Stage 2: Classifying errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • missing_argument: 5 errors (confidence: 95%)

🛠️  Stage 3: Applying fixes (Iteration 1)...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✅ CustomerDetailScreen.swift:208 → Added 'amount: 500000'
  ✅ CustomerRowView.swift:42 → Added 'amount: 500000'
  ✅ CustomerFormSheet.swift:88 → Added 'amount: 500000'
  ✅ TransactionRowView.swift:43 → Added 'amount: 500000'
  ✅ DashboardHomeScreen.swift:188 → Added 'amount: 500000'

✓ Stage 4: Re-validating...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Build errors: 0

✅ SUCCESS: All errors fixed in 1 iteration
```

### Output Report

```yaml
status: success
initial_error_count: 5
final_error_count: 0
iterations_used: 1
fixed_errors:
  - category: missing_argument
    count: 5
    files:
      - CustomerDetailScreen.swift
      - CustomerRowView.swift
      - CustomerFormSheet.swift
      - TransactionRowView.swift
      - DashboardHomeScreen.swift
remaining_errors: []
```

---

## Example 2: Multiple Error Types

### Scenario

After scaffolding a new screen, you have:
- 3 missing imports
- 2 type mismatches
- 1 undefined symbol (typo)

### Input

```yaml
MODE: auto
```

### Execution Log

```
🔍 Stage 1: Scanning build errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 6 errors:
  ✗ NewScreen.swift:1 - Cannot find 'Published'
  ✗ NewScreen.swift:2 - Cannot find 'Date'
  ✗ NewViewModel.swift:1 - Cannot find 'ObservableObject'
  ✗ NewViewModel.swift:42 - Type mismatch (String vs Double)
  ✗ NewScreen.swift:58 - Type mismatch (Int vs String)
  ✗ NewScreen.swift:72 - Cannot find 'Custmer'

🔧 Stage 2: Classifying errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • missing_import: 3 errors
  • type_mismatch: 2 errors
  • undefined_symbol: 1 error

🛠️  Stage 3: Applying fixes (Iteration 1)...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[Priority 1: Missing Imports]
  ✅ NewScreen.swift → Added 'import Combine'
  ✅ NewScreen.swift → Added 'import Foundation'
  ✅ NewViewModel.swift → Added 'import Combine'

✓ Stage 4: Re-validating...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Build errors: 3 (imports resolved cascading errors)

🛠️  Stage 3: Applying fixes (Iteration 2)...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[Priority 2: Undefined Symbols]
  ✅ NewScreen.swift:72 → Corrected 'Custmer' → 'Customer'

[Priority 3: Type Mismatches]
  ✅ NewViewModel.swift:42 → Added 'Double(amount) ?? 0'
  ✅ NewScreen.swift:58 → Added 'String(count)'

✓ Stage 4: Re-validating...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Build errors: 0

✅ SUCCESS: All errors fixed in 2 iterations
```

### Output Report

```yaml
status: success
initial_error_count: 6
final_error_count: 0
iterations_used: 2
fixed_errors:
  - category: missing_import
    count: 3
    files:
      - NewScreen.swift
      - NewViewModel.swift
  - category: undefined_symbol
    count: 1
    files:
      - NewScreen.swift
  - category: type_mismatch
    count: 2
    files:
      - NewViewModel.swift
      - NewScreen.swift
remaining_errors: []
```

---

## Example 3: Targeted Fix (Missing Arguments Only)

### Scenario

You only want to fix missing argument errors, leave everything else.

### Input

```yaml
MODE: targeted
ERROR_TYPES: missing_argument
```

### Execution Log

```
🔍 Stage 1: Scanning build errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 8 errors:
  ✗ CustomerDetailScreen.swift:208 - Missing argument 'amount'
  ✗ CustomerRowView.swift:42 - Missing argument 'amount'
  ✗ NewScreen.swift:1 - Cannot find 'Published'
  ✗ NewScreen.swift:72 - Cannot find 'Custmer'
  ✗ NewViewModel.swift:42 - Type mismatch (String vs Double)
  ... (3 more)

🔧 Stage 2: Filtering errors (targeted mode)...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Included: missing_argument (2 errors)
  Skipped: missing_import, undefined_symbol, type_mismatch (6 errors)

🛠️  Stage 3: Applying fixes (Iteration 1)...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✅ CustomerDetailScreen.swift:208 → Added 'amount: 500000'
  ✅ CustomerRowView.swift:42 → Added 'amount: 500000'

✓ Stage 4: Re-validating...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Build errors: 6 (only missing_argument fixed)

✅ PARTIAL SUCCESS: Fixed 2 of 2 targeted errors. 6 other errors remain.
```

### Output Report

```yaml
status: partial
initial_error_count: 8
final_error_count: 6
iterations_used: 1
fixed_errors:
  - category: missing_argument
    count: 2
    files:
      - CustomerDetailScreen.swift
      - CustomerRowView.swift
remaining_errors:
  - file: NewScreen.swift
    line: 1
    message: "Cannot find 'Published'"
    category: missing_import
    auto_fixable: true
  - file: NewScreen.swift
    line: 72
    message: "Cannot find 'Custmer'"
    category: undefined_symbol
    auto_fixable: true
  - file: NewViewModel.swift
    line: 42
    message: "Type mismatch (String vs Double)"
    category: type_mismatch
    auto_fixable: true
  # ... (3 more)
```

---

## Example 4: Dry-Run (Audit Mode)

### Scenario

You want to see what would be fixed WITHOUT actually modifying files.

### Input

```yaml
MODE: dry-run
```

### Execution Log

```
🔍 Stage 1: Scanning build errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 5 errors.

🔧 Stage 2: Classifying errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • missing_argument: 3 errors
  • type_mismatch: 2 errors

📋 Dry-Run Report (no files modified)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

WOULD FIX (confidence ≥ 70%):

[missing_argument] CustomerDetailScreen.swift:208
  Fix: Add 'amount: 500000' to Customer() initializer
  Confidence: 95%

[missing_argument] CustomerRowView.swift:42
  Fix: Add 'amount: 500000' to Customer() initializer
  Confidence: 95%

[missing_argument] CustomerFormSheet.swift:88
  Fix: Add 'amount: 500000' to Customer() initializer
  Confidence: 95%

[type_mismatch] CustomerFormViewModel.swift:58
  Fix: Wrap 'amount' with 'Double(amount) ?? 0'
  Confidence: 90%

[type_mismatch] TransactionFormViewModel.swift:62
  Fix: Wrap 'amount' with 'Double(amount) ?? 0'
  Confidence: 90%

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Summary: 5 errors would be auto-fixed

Run with MODE=auto to apply these fixes.
```

### Output Report

```yaml
status: success
mode: dry-run
initial_error_count: 5
fixable_errors: 5
unfixable_errors: 0
proposed_fixes:
  - category: missing_argument
    count: 3
    confidence: 95%
  - category: type_mismatch
    count: 2
    confidence: 90%
```

---

## Example 5: Partial Success (Some Errors Unfixable)

### Scenario

Some errors can't be auto-fixed (e.g., protocol conformance).

### Input

```yaml
MODE: auto
```

### Execution Log

```
🔍 Stage 1: Scanning build errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 6 errors:
  ✗ CustomerDetailScreen.swift:208 - Missing argument 'amount'
  ✗ Customer.swift:3 - Type does not conform to protocol 'Decodable'
  ✗ NewScreen.swift:42 - Cannot find 'Published'
  ... (3 more)

🔧 Stage 2: Classifying errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  • missing_argument: 1 error (auto-fixable)
  • missing_import: 1 error (auto-fixable)
  • protocol_conformance: 1 error (NOT auto-fixable)
  • type_mismatch: 3 errors (auto-fixable)

🛠️  Stage 3: Applying fixes (Iteration 1)...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✅ CustomerDetailScreen.swift:208 → Added 'amount: 500000'
  ✅ NewScreen.swift → Added 'import Combine'
  ✅ NewViewModel.swift:42 → Added type conversion
  ✅ NewViewModel.swift:58 → Added type conversion
  ✅ NewScreen.swift:72 → Added type conversion
  ⏭️  Customer.swift:3 → Skipped (protocol conformance - manual fix needed)

✓ Stage 4: Re-validating...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Build errors: 1

⚠️  PARTIAL SUCCESS: Fixed 5 of 6 errors. 1 requires manual intervention.
```

### Output Report

```yaml
status: partial
initial_error_count: 6
final_error_count: 1
iterations_used: 1
fixed_errors:
  - category: missing_argument
    count: 1
  - category: missing_import
    count: 1
  - category: type_mismatch
    count: 3
remaining_errors:
  - file: Customer.swift
    line: 3
    message: "Type 'Customer' does not conform to protocol 'Decodable'"
    category: protocol_conformance
    auto_fixable: false
    suggestion: "Implement required protocol methods or add 'Codable' conformance"
```

---

## Example 6: Max Iterations Exceeded

### Scenario

Errors keep regenerating or new errors appear after fixes.

### Input

```yaml
MODE: auto
MAX_ITERATIONS: 3
```

### Execution Log

```
🔍 Stage 1: Scanning build errors...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Found 10 errors.

🛠️  Iteration 1: Fixed 5 errors → 5 remaining
🛠️  Iteration 2: Fixed 3 errors → 2 remaining
🛠️  Iteration 3: Fixed 0 errors → 2 remaining (stuck)

⚠️  MAX ITERATIONS REACHED: 2 errors remain unfixed after 3 cycles.
```

### Output Report

```yaml
status: partial
initial_error_count: 10
final_error_count: 2
iterations_used: 3
message: "Max iterations (3) reached. Manual intervention needed."
remaining_errors:
  - file: ComplexScreen.swift
    line: 120
    message: "Complex async/await refactor needed"
    category: async_context
    auto_fixable: false
  - file: DataRepository.swift
    line: 58
    message: "Ambiguous reference to member"
    category: unknown
    auto_fixable: false
```
