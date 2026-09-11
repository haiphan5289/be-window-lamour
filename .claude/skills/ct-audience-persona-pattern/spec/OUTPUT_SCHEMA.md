# Output Schema — ct-audience-persona-pattern

## Depth Calibration by Audience Role

| Audience | Code Depth | Terminology | Examples |
|---|---|---|---|
| **Junior dev** | Full snippet, step-by-step per layer | Plain language, spell out layer names | Simple CRUD UseCase (e.g. Suppliers) |
| **Mid-level dev** | Key methods only, skip boilerplate | Standard EF Core / ASP.NET terms | Real feature scenario (Sales, SalesReturn) |
| **Senior dev** | Architecture trade-offs, DI lifetime, query perf | Advanced (tracking behavior, transaction scope) | Edge cases, migration/rollback concerns |
| **PM / QA / business** | No code — flow diagram or table only | Business language (invoice, stock, VAT, ghi sổ, bỏ ghi) | User-facing scenario + expected outcome, real on-screen button labels |

---

## Required Explanation Shape

Every explanation, regardless of audience, must contain:

1. **Confirmed framing** — one line restating audience + goal (already confirmed in Step 1 of PROMPT.md), so the reader knows why the depth/terminology was chosen this way.
2. **Grounded content** — every class, route, button label, or business rule traced to a real file (BE UseCase, WPF ViewModel/`.xaml`, or a feature doc) verified in Step 2 of PROMPT.md. No invented entities.
3. **Audience-appropriate depth** — per the table above. A dev explanation may include file:line references; a PM/QA/business explanation never does.
4. **Open questions surfaced, not buried** — when current behavior looks like something the audience should actively confirm or challenge (a business rule with no approval gate, an inconsistency between two ways of reaching the same screen), call it out explicitly rather than describing it as settled.

---

## PM / QA / Business Deliverable — Handoff to ct-review-artifact

When the confirmed audience is PM / QA / business, the explanation produced in Step 3 of PROMPT.md becomes the **content payload** handed to the `ct-review-artifact` skill (see PROMPT.md Step 4). That payload should already be organized as:

- **Feature name** (for the artifact's title/masthead)
- **Audience + goal** (for the masthead meta row)
- **Real on-screen controls** — button labels per screen, exactly as they appear in the `.xaml` (`➕ Thêm`, `Cất`, `Bỏ ghi`, etc.) — not invented or paraphrased
- **Screen layout** — for each real WPF screen involved (popup form and/or list grid): window/screen title, form field labels or grid column names in their real order, 2-3 illustrative sample values, and any visual status distinction (badge, row highlight) the screen actually uses. Pulled from the same `.xaml`/ViewModel read in PROMPT.md Step 2 — `ct-review-artifact` turns this into a schematic mockup so the audience can picture the screen, not just read a button list.
- **Workflow steps** — only numbered if the feature is genuinely sequential
- **Calculation/business rules** — as a flat list of rule → value pairs (suitable for a reference table)
- **Open questions** — the confirm/sign-off items from Required Explanation Shape §4

`ct-review-artifact` owns page design, theming, and publishing — do not hand it pre-styled HTML or presentational instructions, only the structured content above.

---

## Completion Block

Print this after the explanation has been delivered (inline, or via `ct-review-artifact`):

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ ct-audience-persona-pattern COMPLETE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Topic:      <TOPIC>
Audience:   <confirmed AUDIENCE>
Goal:       <confirmed GOAL>
Sources:    <BE doc path> + <WPF doc path> (or "code only — no doc found")
Verified:   <UseCase files> / <ViewModel + .xaml files> read directly, not just the doc
Delivery:   <inline reply | Artifact link via ct-review-artifact>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```
