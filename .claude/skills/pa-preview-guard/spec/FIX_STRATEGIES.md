# Fix Strategies — Build Doctor

Detailed strategies for auto-fixing each error category.

---

## Strategy 1: Missing Argument Fix

### Input

```yaml
category: missing_argument
file: CustomerDetailScreen.swift
line: 208
param_name: amount
context: Customer( initialization
```

### Execution Steps

```python
# Step 1: Locate the model definition
model_name = extract_type_from_line(file, line)  # → "Customer"
model_file = find_file_by_pattern(f"**/*{model_name}.swift")

# Step 2: Read model to get parameter type
model_content = read_file(model_file)
param_type = extract_param_type(model_content, param_name)  # → "Double"

# Step 3: Generate default value by type
default_value = generate_default(param_type)
# String → ""
# Int → 0
# Double → 0.0 (or sensible default like 500000 for amount)
# Bool → false
# Date → Date()
# Optional<T> → nil
# Custom type → Requires factory or .preview()

# Step 4: Read the initialization context
context_lines = read_file(file, line - 2, line + 10)
# Example:
#     CustomerDetailScreen(customer: Customer(
#         id: "1",
#         name: "Nguyen Van An",
#         phone: "0901234567",
#         email: "an@example.com",
#         ...
#     ))

# Step 5: Find insertion point (alphabetical or last parameter)
insertion_point = find_insertion_point(context_lines, param_name)

# Step 6: Insert parameter with proper indentation
new_line = f"        {param_name}: {default_value},"
insert_at_line(file, insertion_point, new_line)
```

### Default Value Heuristics

For common Laundry Dashboard domain:

| Parameter Name | Type | Smart Default |
|---|---|---|
| `amount` | `Double` | `500000` (typical laundry amount) |
| `customerId` | `String` | `"c1"` or `UUID().uuidString` |
| `uid` | `String` | `"u1"` or `UUID().uuidString` |
| `createdAt` | `Date` | `Date()` |
| `id` | `String` | `UUID().uuidString` |
| `name` | `String` | `"Test Name"` |
| `phone` | `String` | `"0901234567"` |
| `email` | `String?` | `nil` or `"test@example.com"` |

### Example Fix

**Before:**

```swift
#Preview {
    CustomerDetailScreen(customer: Customer(
        id: "1",
        name: "Nguyen Van An",
        phone: "0901234567",
        email: "an@example.com",
        createdAt: Date()
    ))
}
```

**After:**

```swift
#Preview {
    CustomerDetailScreen(customer: Customer(
        id: "1",
        name: "Nguyen Van An",
        phone: "0901234567",
        amount: 500000,  // ← Added
        email: "an@example.com",
        createdAt: Date()
    ))
}
```

---

## Strategy 2: Type Conversion Fix

### Input

```yaml
category: type_mismatch
file: CustomerFormViewModel.swift
line: 58
actual_type: String
expected_type: Double
```

### Execution Steps

```python
# Step 1: Read the problematic line
line_content = read_file(file, line)
# Example: "amount: amount,"  (where amount is String but expects Double)

# Step 2: Determine conversion function
conversion = get_conversion(actual_type, expected_type)
# String → Double: "Double(value) ?? 0"
# Int → Double: "Double(value)"
# String → Int: "Int(value) ?? 0"
# Double → String: "String(value)"
# etc.

# Step 3: Wrap the variable with conversion
old_str = f"amount: amount,"
new_str = f"amount: Double(amount) ?? 0,"
replace_in_file(file, old_str, new_str)
```

### Conversion Table

| From | To | Conversion Code |
|---|---|---|
| `String` | `Double` | `Double(value) ?? 0` |
| `String` | `Int` | `Int(value) ?? 0` |
| `Int` | `Double` | `Double(value)` |
| `Double` | `Int` | `Int(value)` |
| `T` | `String` | `String(value)` or `"\(value)"` |
| `T` | `Optional<T>` | Just wrap: `value` → `Optional(value)` |
| `Optional<T>` | `T` | `value ?? defaultValue` |

### Example Fix

**Before:**

```swift
let customer = Customer(
    id: id,
    name: name,
    phone: phone,
    amount: amount,  // ← amount is String, expects Double
    email: email,
    createdAt: Date()
)
```

**After:**

```swift
let customer = Customer(
    id: id,
    name: name,
    phone: phone,
    amount: Double(amount) ?? 0,  // ← Converted
    email: email,
    createdAt: Date()
)
```

---

## Strategy 3: Add Import Fix

### Input

```yaml
category: missing_import
file: CustomerViewModel.swift
line: 4
symbol: Published
required_import: Combine
```

### Execution Steps

```python
# Step 1: Read file header
header = read_file(file, 1, 20)

# Step 2: Check if import already exists
if "import Combine" in header:
    return  # Already imported

# Step 3: Find insertion point (after existing imports)
last_import_line = find_last_import_line(header)

# Step 4: Insert import
insert_at_line(file, last_import_line + 1, "import Combine")
```

### Import Order Convention

Follow SwiftUI standard:

```swift
import Foundation   // 1. Standard library
import Combine      // 2. Apple frameworks
import SwiftUI      // 3. UI framework
// blank line
import CustomPkg    // 4. Third-party (if any)
```

### Example Fix

**Before:**

```swift
import Foundation

@MainActor
final class CustomerViewModel: ObservableObject {
    @Published var customers: [Customer] = []  // ← Error: Cannot find 'Published'
}
```

**After:**

```swift
import Foundation
import Combine  // ← Added

@MainActor
final class CustomerViewModel: ObservableObject {
    @Published var customers: [Customer] = []
}
```

---

## Strategy 4: Symbol Correction (Typo Fix)

### Input

```yaml
category: undefined_symbol
file: CustomerScreen.swift
line: 42
symbol: Custmer
suggested_correction: Customer
similarity_score: 95%
```

### Execution Steps

```python
# Step 1: Search for similar symbols
candidates = fuzzy_search_symbols(workspace, symbol)
# Use Levenshtein distance, case-insensitive

# Step 2: Rank candidates by similarity
ranked = rank_by_similarity(symbol, candidates)
# 'Custmer' → 'Customer' (score: 95%)

# Step 3: If top match > 80% confident
if ranked[0].score >= 80:
    auto_fix = true
    correction = ranked[0].symbol
else:
    auto_fix = false
    return  # Report for manual fix

# Step 4: Replace symbol
line_content = read_file(file, line)
new_content = line_content.replace(symbol, correction)
replace_line(file, line, new_content)
```

### Common Typos in Laundry Dashboard

| Typo | Correct | Pattern |
|---|---|---|
| `Custmer` | `Customer` | Missing 'o' |
| `Trasaction` | `Transaction` | Swapped letters |
| `Dashbord` | `Dashboard` | Missing 'a' |
| `Repositry` | `Repository` | Missing 'o' |
| `ViewMdoel` | `ViewModel` | Swapped letters |

### Example Fix

**Before:**

```swift
let customer: Custmer = ...  // ← Typo
```

**After:**

```swift
let customer: Customer = ...  // ← Corrected
```

---

## Strategy 5: Async Context Fix

### Input

```yaml
category: async_context
file: DashboardViewModel.swift
line: 25
error_type: missing_await
function_context: async_function
```

### Execution Steps

```python
# Case A: Missing 'await' in async function
if function_is_async(file, line):
    line_content = read_file(file, line)
    # Add 'await' before async call
    new_content = line_content.replace("fetchData()", "await fetchData()")
    replace_line(file, line, new_content)

# Case B: Async call in sync function
else:
    # Wrap in Task
    line_content = read_file(file, line)
    indentation = get_indentation(line_content)
    new_content = f"{indentation}Task {{\n{indentation}    await {line_content.strip()}\n{indentation}}}"
    replace_line(file, line, new_content)
```

### Example Fix A: Add await

**Before:**

```swift
func loadCustomers() async {
    let data = fetchCustomers()  // ← Missing await
}
```

**After:**

```swift
func loadCustomers() async {
    let data = await fetchCustomers()  // ← Added await
}
```

### Example Fix B: Wrap in Task

**Before:**

```swift
func onAppear() {
    fetchCustomers()  // ← Async call in sync context
}
```

**After:**

```swift
func onAppear() {
    Task {
        await fetchCustomers()  // ← Wrapped in Task
    }
}
```

---

## Strategy 6: Property Wrapper Fix

### Input

```yaml
category: property_wrapper_misuse
file: CustomerViewModel.swift
line: 5
wrapper: StateObject
issue: applied_to_struct
```

### Execution Steps

```python
# Check if class or struct
type_kind = get_type_declaration(file)

if type_kind == "struct":
    # Change @StateObject → @ObservedObject
    replace_in_file(file, "@StateObject", "@ObservedObject")
elif type_kind == "class":
    # Keep @StateObject (correct usage)
    pass
```

### Wrapper Rules for SwiftUI

| Wrapper | Valid On | Purpose |
|---|---|---|
| `@StateObject` | `class` (view owner) | Create & own ObservableObject |
| `@ObservedObject` | `class` (passed in) | Observe external ObservableObject |
| `@State` | `struct` | Local view state |
| `@Binding` | `struct` | Two-way binding |
| `@Published` | `class` property | Publish changes |

### Example Fix

**Before:**

```swift
struct CustomerScreen: View {
    @StateObject var viewModel: CustomerViewModel  // ← Error: struct can't use @StateObject
}
```

**After:**

```swift
struct CustomerScreen: View {
    @ObservedObject var viewModel: CustomerViewModel  // ← Changed to @ObservedObject
}
```

---

## Strategy 7: Protocol Conformance (Report Only)

### Input

```yaml
category: protocol_conformance
file: Customer.swift
line: 3
type: Customer
protocol: Decodable
```

### Action

**Cannot auto-fix** — too complex. Report with suggestions:

```
❌ Manual fix needed: Customer.swift:3
   Error: Type 'Customer' does not conform to protocol 'Decodable'
   
   Suggestions:
   1. Add 'Codable' to struct declaration if all properties are Codable
   2. Implement custom 'init(from decoder: Decoder)' if needed
   3. Check if all properties conform to Decodable
```

---

## Fix Priority & Dependencies

Some fixes enable others. Fix in this order:

```
1. Missing imports          (enables symbol resolution)
   ↓
2. Undefined symbols        (fixes typos)
   ↓
3. Missing arguments        (most common after model change)
   ↓
4. Type mismatches          (conversions)
   ↓
5. Async context            (wrapping/await)
   ↓
6. Property wrapper misuse  (quick syntax fixes)
   ↓
7. Protocol conformance     (manual - report only)
```

After each batch of fixes, re-run `get_errors()` to see if cascading errors resolved.
