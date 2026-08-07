# Execution Workflow — Build Doctor

## Prerequisites

Skill này chạy **SAU KHI** generate code thành công (screen, UseCase, model, etc.)

## Stage 1: Scan All Build Errors

```bash
# Get all compile errors
get_errors()
```

Parse output và extract:
- File path + line number
- Error message
- Error code (if available)

## Stage 2: Classify Errors

Với mỗi error, match với các patterns trong ERROR_PATTERNS.md:

| Error Pattern | Category | Auto-fixable |
|---|---|---|
| `Missing argument for parameter` | `missing_argument` | ✅ Yes |
| `Cannot convert value of type` | `type_mismatch` | ✅ Yes |
| `Cannot find type/function in scope` | `missing_import` | ✅ Yes |
| `Cannot find 'X' in scope` | `undefined_symbol` | ⚠️ Depends |
| `Type 'X' does not conform to protocol` | `protocol_conformance` | ⚠️ Depends |
| `Property wrapper 'X' applied to` | `property_wrapper_misuse` | ✅ Yes |
| `Expression is 'async' but is not marked` | `async_context` | ✅ Yes |

## Stage 3: Auto-Fix by Category

### 3.1. Missing Argument Errors

**Strategy:** Tìm model definition → lấy default value → inject vào call site

```bash
# Example error:
# CustomerDetailScreen.swift:208:32: Missing argument for parameter 'amount'

1. Extract: parameter name = 'amount', file = CustomerDetailScreen.swift, line = 208
2. Tìm Customer model definition → read Customer.swift
3. Tìm field 'amount' → type = Double
4. Generate default value based on type:
   - String → ""
   - Int/Double → 0
   - Bool → false
   - Date → Date()
   - Optional → nil
5. Read line 208 context → tìm Customer( initializer
6. Insert 'amount: [default_value]' vào đúng vị trí alphabetical
7. Write file
```

### 3.2. Type Mismatch Errors

**Strategy:** Cast hoặc convert sang type đúng

```bash
# Example error:
# Cannot convert value of type 'String' to expected argument type 'Double'

1. Extract: expected type = Double, actual = String
2. If conversion available:
   - String → Double: Double(value) ?? 0
   - Int → Double: Double(value)
   - etc.
3. Rewrite expression with conversion
```

### 3.3. Missing Import Errors

**Strategy:** Add import statement at top of file

```bash
# Example error:
# Cannot find type 'Combine' in scope

1. Extract: missing symbol = 'Combine'
2. Determine import needed:
   - Published, @Published → import Combine
   - Date, UUID → import Foundation
   - View, Text → import SwiftUI
   - (check common mappings in ERROR_PATTERNS.md)
3. Read file → add import if not exists
```

### 3.4. Undefined Symbol Errors

**Strategy:** Check typo hoặc suggest correct symbol

```bash
# Example error:
# Cannot find 'Custmer' in scope (typo)

1. Extract symbol: 'Custmer'
2. Search similar symbols in workspace:
   grep -r "class Cust" **/*.swift
3. If match found (e.g., 'Customer'):
   - Auto-fix: rename 'Custmer' → 'Customer'
4. If no match:
   - Flag for manual intervention
```

### 3.5. Async Context Errors

**Strategy:** Wrap in Task or mark function async

```bash
# Example error:
# Expression is 'async' but is not marked with 'await'

1. Locate async call
2. If inside synchronous function:
   - Wrap in Task { await ... }
3. If inside async function:
   - Add 'await' keyword
```

## Stage 4: Re-validate

```bash
# After all fixes applied
get_errors()

If errors.count == 0:
  → SUCCESS
Else if errors.count < previous_count:
  → REPEAT Stage 2-3 (iterative fixing)
Else:
  → STOP and report remaining errors (manual intervention needed)
```

## Stage 5: Report Results

Output structured report:

```
✅ Build Doctor Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Initial errors: 12
🔧 Auto-fixed: 11
❌ Remaining: 1

Fixed breakdown:
  ✅ Missing arguments (5 files):
     - CustomerDetailScreen.swift:208 (added 'amount')
     - CustomerRowView.swift:42 (added 'amount')
     - CustomerFormSheet.swift:88 (added 'amount')
     - TransactionRowView.swift:43 (added 'customerId')
     - DashboardStatCard.swift:36 (added 'icon')
  
  ✅ Missing imports (3 files):
     - CustomerViewModel.swift (added 'import Combine')
     - TransactionViewModel.swift (added 'import Combine')
     - DashboardRepository.swift (added 'import Foundation')

  ✅ Type mismatches (3 files):
     - CustomerFormViewModel.swift:58 (String → Double conversion)
     - TransactionFormViewModel.swift:62 (String → Double conversion)
     - DashboardScreen.swift:120 (Int → String conversion)

Manual fixes needed:
  ❌ DashboardViewModel.swift:45
     Error: Complex async/await refactor needed
     Suggestion: Wrap fetchData() in Task { }
```

## Safety Guardrails

1. **Never delete code** - only add/modify
2. **Backup before fix** - store original file content
3. **Max 3 iterations** - prevent infinite loop
4. **Preserve formatting** - match existing code style
5. **Validate imports** - only add standard library imports
6. **Flag uncertain fixes** - if confidence < 80%, skip and report
