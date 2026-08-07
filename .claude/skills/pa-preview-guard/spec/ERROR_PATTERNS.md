# Error Patterns & Classification

## Pattern Matching Rules

Each error message from compiler follows a pattern. This file defines how to classify and extract information from each pattern.

---

## 1. Missing Argument Errors

### Pattern

```
<file>:<line>:<col>: Missing argument for parameter '<param_name>' in call
```

### Example

```
CustomerDetailScreen.swift:208:32: Missing argument for parameter 'amount' in call
```

### Extracted Info

```yaml
category: missing_argument
file: CustomerDetailScreen.swift
line: 208
col: 32
param_name: amount
auto_fixable: true
```

### Fix Strategy

→ See FIX_STRATEGIES.md → Missing Argument Strategy

---

## 2. Type Mismatch Errors

### Pattern

```
<file>:<line>:<col>: Cannot convert value of type '<actual_type>' to expected argument type '<expected_type>'
```

### Example

```
CustomerFormViewModel.swift:58:20: Cannot convert value of type 'String' to expected argument type 'Double'
```

### Extracted Info

```yaml
category: type_mismatch
file: CustomerFormViewModel.swift
line: 58
col: 20
actual_type: String
expected_type: Double
auto_fixable: true
conversion_available: true
```

### Fix Strategy

→ See FIX_STRATEGIES.md → Type Conversion Strategy

---

## 3. Missing Import Errors

### Pattern A: Cannot find type

```
<file>:<line>:<col>: Cannot find type '<symbol>' in scope
```

### Pattern B: Use of unresolved identifier

```
<file>:<line>:<col>: Use of unresolved identifier '<symbol>'
```

### Example

```
CustomerViewModel.swift:4:12: Cannot find type 'Published' in scope
DashboardRepository.swift:8:20: Use of unresolved identifier 'Date'
```

### Extracted Info

```yaml
category: missing_import
file: CustomerViewModel.swift
line: 4
col: 12
symbol: Published
required_import: Combine
auto_fixable: true
```

### Symbol → Import Mapping

| Symbol | Required Import |
|---|---|
| `Published`, `@Published` | `import Combine`|
| `PassthroughSubject`, `AnyPublisher`, `Cancellable` | `import Combine` |
| `Date`, `UUID`, `Data` | `import Foundation` |
| `View`, `Text`, `VStack`, `State` | `import SwiftUI` |
| `URLSession`, `URLRequest` | `import Foundation` |
| `ObservableObject`, `@StateObject` | `import Combine` |

### Fix Strategy

→ See FIX_STRATEGIES.md → Add Import Strategy

---

## 4. Undefined Symbol Errors (Typo)

### Pattern

```
<file>:<line>:<col>: Cannot find '<symbol>' in scope
```

### Example

```
CustomerScreen.swift:42:18: Cannot find 'Custmer' in scope
```

### Extracted Info

```yaml
category: undefined_symbol
file: CustomerScreen.swift
line: 42
col: 18
symbol: Custmer
auto_fixable: depends
confidence: requires_similarity_check
```

### Detection Logic

```python
1. Extract symbol: 'Custmer'
2. Search workspace for similar:
   - Exact match: 'Custmer' → not found
   - Fuzzy match (Levenshtein distance ≤ 2):
     - 'Customer' (distance = 1) ✅
3. If single match found with distance ≤ 2:
   → auto_fixable = true
4. Else:
   → auto_fixable = false (manual intervention)
```

### Fix Strategy

→ See FIX_STRATEGIES.md → Symbol Correction Strategy

---

## 5. Protocol Conformance Errors

### Pattern

```
<file>:<line>:<col>: Type '<type>' does not conform to protocol '<protocol>'
```

### Example

```
Customer.swift:3:8: Type 'Customer' does not conform to protocol 'Decodable'
```

### Extracted Info

```yaml
category: protocol_conformance
file: Customer.swift
line: 3
col: 8
type: Customer
protocol: Decodable
auto_fixable: false
reason: requires_manual_implementation
```

### Fix Strategy

→ Manual intervention (too complex for auto-fix)
→ Report với suggestion: "Implement required protocol methods"

---

## 6. Property Wrapper Misuse

### Pattern

```
<file>:<line>:<col>: Property wrapper '<wrapper>' can only be applied to classes
```

### Example

```
CustomerViewModel.swift:5:5: Property wrapper 'StateObject' can only be applied to classes
```

### Extracted Info

```yaml
category: property_wrapper_misuse
file: CustomerViewModel.swift
line: 5
col: 5
wrapper: StateObject
auto_fixable: true
suggested_fix: change_to_ObservedObject_or_make_class
```

### Fix Strategy

→ See FIX_STRATEGIES.md → Property Wrapper Fix Strategy

---

## 7. Async/Await Context Errors

### Pattern A: Missing await

```
<file>:<line>:<col>: Expression is 'async' but is not marked with 'await'
```

### Pattern B: Async in sync context

```
<file>:<line>:<col>: 'async' call in a function that does not support concurrency
```

### Example

```
DashboardViewModel.swift:25:10: Expression is 'async' but is not marked with 'await'
CustomerViewModel.swift:42:5: 'async' call in a function that does not support concurrency
```

### Extracted Info

```yaml
category: async_context
file: DashboardViewModel.swift
line: 25
col: 10
error_type: missing_await
auto_fixable: true
fix: add_await
```

### Fix Strategy

→ See FIX_STRATEGIES.md → Async Context Fix Strategy

---

## 8. Preview-Specific Errors

### Pattern

```
<file>:<line>:<col>: [Preview context specific error]
```

### Common Preview Errors

| Error | Category | Auto-fixable |
|---|---|---|
| Missing argument in Customer() call inside #Preview | `missing_argument` | ✅ Yes |
| Preview provider not found | `preview_config` | ❌ No |
| PreviewProvider deprecated | `preview_migration` | ✅ Yes |

### Fix Strategy

→ Same as general missing_argument strategy
→ Preview blocks should use `.preview()` factory methods

---

## Error Priority for Fixing

Fix trong thứ tự này để tránh cascade errors:

1. **Missing imports** (cao nhất - causes other errors)
2. **Undefined symbols** (typos)
3. **Missing arguments** (common after model changes)
4. **Type mismatches** (conversions)
5. **Async context** (wrapping)
6. **Property wrapper misuse** (quick fixes)
7. **Protocol conformance** (manual - lowest priority)

---

## Confidence Scoring

Mỗi error được score confidence (0-100%):

| Confidence | Action |
|---|---|
| 90-100% | Auto-fix immediately |
| 70-89% | Auto-fix with warning log |
| 50-69% | Suggest fix, require confirmation |
| < 50% | Skip, report for manual intervention |

### Scoring Factors

```python
confidence = base_score
if error in well_known_patterns:
    confidence += 30
if fix_strategy is deterministic:
    confidence += 20
if similar_fixes_succeeded_before:
    confidence += 20
if context_is_clear:
    confidence += 10
```
