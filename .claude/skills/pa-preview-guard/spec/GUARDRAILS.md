# Guardrails — Build Doctor

Safety rules để tránh làm hỏng code khi auto-fix.

---

## Rule 1: Never Delete User Code

```
❌ PROHIBITED
Xóa bất kỳ logic nào của user.

✅ ALLOWED
Chỉ ADD hoặc MODIFY để fix errors.
```

**Example:**

```swift
// BAD: Deleting entire function
func loadData() {
    // Delete this broken code ❌
}

// GOOD: Fixing the error within
func loadData() {
    Task {
        await fetchData()  // ✅ Added Task wrapper
    }
}
```

---

## Rule 2: Preserve Formatting & Style

```
❌ PROHIBITED
Reformat entire file, change indentation style, add/remove blank lines.

✅ ALLOWED
Match existing indentation when inserting code.
```

**Example:**

```swift
// Existing code uses 4 spaces
#Preview {
    CustomerRowView(customer: Customer(
        id: "1",
        name: "Test"
        // ← Insert here with 8 spaces (2 levels of 4)
    ))
}

// GOOD: Match indentation
        amount: 500000,  // ✅ 8 spaces

// BAD: Wrong indentation
  amount: 500000,  // ❌ 2 spaces
```

---

## Rule 3: Verify Before Insert

```
❌ PROHIBITED
Insert code without checking if it already exists.

✅ ALLOWED
Check if import/parameter already present before adding.
```

**Check logic:**

```python
# Before adding import
if "import Combine" not in file_content:
    add_import("import Combine")
else:
    skip  # Already imported

# Before adding parameter
if "amount:" not in initializer_context:
    add_parameter("amount: 500000")
else:
    skip  # Already has amount
```

---

## Rule 4: Confidence Threshold

```
❌ PROHIBITED
Auto-fix errors with confidence < 70%

✅ ALLOWED
Auto-fix only when confidence ≥ 70%
```

**Confidence scoring:**

| Confidence | Action |
|---|---|
| 90-100% | Auto-fix immediately |
| 70-89% | Auto-fix + log warning |
| 50-69% | Suggest fix, DON'T apply |
| < 50% | Skip, report for manual |

**Example:**

```python
if error.confidence >= 70:
    apply_fix(error)
else:
    report_for_manual_fix(error)
```

---

## Rule 5: Max Iterations Limit

```
❌ PROHIBITED
Infinite loop fixing → re-validating → fixing same errors

✅ ALLOWED
Max 3 iterations (configurable) then stop
```

**Safety mechanism:**

```python
for iteration in range(MAX_ITERATIONS):
    errors = scan_errors()
    if len(errors) == 0:
        break  # Success
    
    apply_fixes(errors)
    new_errors = scan_errors()
    
    if new_errors == errors:  # Stuck - same errors
        break  # Stop, report manual intervention needed
```

---

## Rule 6: Backup Original Content

```
❌ PROHIBITED
Modify file without backup

✅ ALLOWED
Store original content before any modification
```

**Implementation:**

```python
# Before fixing
original_content = read_file(file_path)
backup[file_path] = original_content

try:
    apply_fixes(file_path)
    validate()
except Exception as e:
    # Rollback if anything goes wrong
    write_file(file_path, original_content)
    raise e
```

---

## Rule 7: Type-Safe Default Values

```
❌ PROHIBITED
Generate invalid default values

✅ ALLOWED
Use type-appropriate and domain-aware defaults
```

**Type safety table:**

| Type | Default | Why |
|---|---|---|
| `String` | `""` | Empty is safe |
| `String` (name) | `"Test Name"` | Better for previews |
| `String?` | `nil` | Optional → nil is correct |
| `Int` | `0` | Numeric safe default |
| `Double` | `0.0` | Numeric safe default |
| `Double` (amount) | `500000` | Domain-aware (Laundry Dashboard) |
| `Bool` | `false` | Boolean safe default |
| `Date` | `Date()` | Current date |
| `UUID` | `UUID().uuidString` | Generate valid UUID |
| `[T]` | `[]` | Empty array |
| `Custom` | ❌ SKIP | Cannot infer, report manual |

---

## Rule 8: Preserve Imports Order

```
❌ PROHIBITED
Add import at random position

✅ ALLOWED
Follow SwiftUI conventions: Foundation → Combine → SwiftUI → custom
```

**Correct order:**

```swift
import Foundation   // 1. Standard library
import Combine      // 2. Apple frameworks
import SwiftUI      // 3. UI framework
                    // 4. Blank line
import CustomPkg    // 5. Third-party (if any)
```

**Insertion logic:**

```python
def insert_import(file, new_import):
    imports = extract_imports(file)
    order = ["Foundation", "Combine", "SwiftUI"]
    
    # Find correct position
    if new_import in order:
        idx = order.index(new_import)
        insert_after = find_last_import_before(imports, order[:idx+1])
    
    insert_at_line(file, insert_after + 1, f"import {new_import}")
```

---

## Rule 9: Validate After Each Fix

```
❌ PROHIBITED
Apply all fixes blindly then validate once

✅ ALLOWED
Apply batch → validate → apply next batch
```

**Iterative approach:**

```python
# GOOD: Validate after each priority level
fix_missing_imports()
validate()  # Check if import fixes resolved cascading errors

fix_missing_arguments()
validate()

fix_type_mismatches()
validate()

# If any step introduces new errors, stop and report
```

---

## Rule 10: Respect User's Code Context

```
❌ PROHIBITED
Change variable names, refactor logic, "improve" code style

✅ ALLOWED
Only fix the specific error - don't touch anything else
```

**Example:**

```swift
// User's code (maybe not perfect, but it's theirs)
let c = Customer(id: "1", name: "Test")  // ← Missing amount

// BAD: Over-fixing
let customer = Customer(  // ❌ Changed variable name
    id: UUID().uuidString,  // ❌ Changed id value
    name: "Nguyen Van An",  // ❌ Changed name
    amount: 500000
)

// GOOD: Minimal fix
let c = Customer(id: "1", name: "Test", amount: 500000)  // ✅ Only added amount
```

---

## Rule 11: Skip Complex Refactors

```
❌ PROHIBITED
Auto-fix protocol conformance, complex async/await refactors, architecture changes

✅ ALLOWED
Only fix mechanical/syntactic errors
```

**Complexity levels:**

| Error Type | Complexity | Auto-fix? |
|---|---|---|
| Missing argument | Low | ✅ Yes |
| Type mismatch (simple cast) | Low | ✅ Yes |
| Missing import | Low | ✅ Yes |
| Typo in symbol | Low | ✅ Yes |
| Missing `await` | Medium | ✅ Yes |
| Protocol conformance | High | ❌ No |
| Complex async refactor | High | ❌ No |
| Architecture violation | High | ❌ No |

**When to skip:**

```python
if error.category in ["protocol_conformance", "architecture_violation"]:
    report_manual_fix_needed(error)
    skip()
```

---

## Rule 12: File Paths Must Be Valid

```
❌ PROHIBITED
Modify files outside the workspace

✅ ALLOWED
Only modify files within laundry-dashboard/ directory
```

**Path validation:**

```python
def is_valid_file(file_path):
    workspace = "/Users/.../laundry-dashboard/"
    return file_path.startswith(workspace) and file_path.endswith(".swift")

if not is_valid_file(error.file):
    skip()  # Don't touch external files
```

---

## Rule 13: Report All Actions

```
❌ PROHIBITED
Silently modify files

✅ ALLOWED
Log every file modification with before/after context
```

**Logging format:**

```
✅ CustomerDetailScreen.swift:208
   Before: Customer(id: "1", name: "Test")
   After:  Customer(id: "1", name: "Test", amount: 500000)
   Reason: Missing argument 'amount' (Double)
```

---

## Rule 14: Never Modify Domain Logic

```
❌ PROHIBITED
Change business logic, validation rules, calculations

✅ ALLOWED
Only syntactic fixes to make code compile
```

**Example:**

```swift
// User's validation logic
guard amount > 0 else { return }  // ← Don't touch

// Error: amount is String, needs Double
let customer = Customer(amount: amount)  // ← Fix this line only

// GOOD: Fix type error, preserve logic
let customer = Customer(amount: Double(amount) ?? 0)

// BAD: Changing logic
guard amount >= 100 else { return }  // ❌ Changed validation threshold
```

---

## Rule 15: Fail Safe

```
❌ PROHIBITED
Leave code in broken state if fix fails

✅ ALLOWED
If anything goes wrong, rollback to original state
```

**Error handling:**

```python
try:
    apply_fixes()
    if not validate_success():
        raise FixFailedException()
except Exception:
    rollback_all_changes()
    report_error_to_user()
    # Code is back to original (albeit with errors)
```

---

## Prohibited Patterns Summary

| Never | Why |
|---|---|
| Delete user code | Data loss |
| Reformat entire files | Unwanted changes |
| Modify business logic | Domain corruption |
| Fix with confidence < 70% | Risk of wrong fix |
| Exceed MAX_ITERATIONS | Infinite loops |
| Touch files outside workspace | Security |
| Auto-fix protocol conformance | Too complex |
| Change variable names | Unexpected refactor |
| Skip validation | Compound errors |
| Operate without backup | No rollback path |

---

## Red Flags - Stop Immediately

If any of these occur, **STOP** and report for manual intervention:

```
🚨 Same errors persist after 2 iterations
🚨 Error count increases after fix
🚨 New file appears in error list (wasn't there initially)
🚨 Cannot parse error message format
🚨 Fix would delete >10 lines of code
🚨 Fix would modify >20 files at once
🚨 Confidence score < 50% for >50% of errors
```

**Action:**

```python
if red_flag_detected():
    rollback_all_changes()
    generate_manual_fix_report()
    exit_gracefully()
```
