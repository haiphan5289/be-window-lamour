---
name: ct-semantic-filter
description: Semantically filter and extract BE-relevant content from a feature description, PRD, or business requirement. Removes UI/frontend details, extracts API contracts, business rules, data models, and validation constraints. Use before ct-feature-pipeline to clean up the input.
---

# BE Semantic Filter — Extract Backend Requirements

> Strips frontend/UI details from a requirement and outputs only what the backend needs to implement.

---

## Input

Paste raw requirement, PRD section, user story, or business description.

---

## Filtering Rules

**KEEP (backend-relevant):**
- API endpoints (method, path, auth requirement)
- Request/response field names and types
- Business rules and validation constraints
- Domain entity fields and relationships
- Status flows (Draft → Confirmed → Cancelled)
- Error conditions and exception types
- Role-based access rules
- Data uniqueness constraints
- Computed values (subtotal, tax, total)
- Stock guard conditions
- Invoice numbering format

**REMOVE (frontend-only — not needed in BE):**
- UI layout descriptions
- Screen/page names
- Color, font, spacing
- Animation, transitions
- Error message display (keep the error condition, not the display text)
- Loading state behavior
- Navigation flows
- Button labels and placement
- Form validation UX

---

## Output Format

```markdown
## Filtered BE Requirements: [Feature]

### API Contract
- `[METHOD] [/path]` — [description]
- Auth: [None | Bearer]
- Input: `{field: type, ...}`
- Output: `{field: type, ...}`

### Domain Entity Fields
| Field | Type | Constraint |
|-------|------|-----------|
| code | string | Required, unique (case-insensitive) |
| status | enum | Draft/Confirmed/Cancelled |

### Business Rules
1. [Rule with enforcement location]
2. [Rule with enforcement location]

### Validation Constraints
- [Field]: [constraint]

### Error Conditions
- [Condition] → throw [ExceptionType]

### Status Flow
`Draft` → `Confirmed` (via confirm action)
`Draft` → `Cancelled` (via cancel action)
`Confirmed` → `Cancelled` (cancellation only)
```

---

## Example

**Input (raw PRD):**
> "The export invoice screen shows a list of products. User can add products to the invoice, enter quantity and price. A discount field is available. The total includes 10% VAT. When user clicks Confirm, the stock must be checked. If not enough stock, show a red error message. The invoice number format is XK-YYYYMMDD-NNN."

**Output (BE-relevant only):**
```markdown
### API Contract
- `POST /api/v1/export-invoices` — Create draft invoice
- `POST /api/v1/export-invoices/{id}/confirm` — Confirm invoice

### Domain Entity Fields
| Field | Type | Constraint |
|-------|------|-----------|
| invoice_number | string | Auto-generated: XK-YYYYMMDD-NNN |
| status | enum | Draft/Confirmed/Cancelled |
| discount_amount | decimal | >= 0 |
| tax_rate | decimal | Default 0.10 (10%) |
| lines | List<Line> | Must not be empty on confirm |

### Business Rules
1. Stock guard: each line's quantity <= product.StockQuantity (UseCase)
2. Invoice immutability: Status must be Draft before confirm (UseCase)
3. Stock decrements only on confirm, not on draft save (UseCase)

### Computed Values
- sub_total = sum(line.quantity * line.unit_cost)
- tax_amount = (sub_total - discount_amount) * tax_rate
- total_amount = sub_total - discount_amount + tax_amount

### Error Conditions
- quantity > stock → throw InsufficientStockException(product, available, requested)
- status != Draft → throw DomainException("Only draft invoices can be confirmed")
```
