# Input Schema — ct-audience-persona-pattern

## Invocation Syntax

```
/ct-audience-persona-pattern
TOPIC: Clean Architecture layering (Api → Application → Domain → Infrastructure)
AUDIENCE: Junior .NET developer new to Clean Architecture
GOAL: Implement a new UseCase without breaking layer boundaries
CONTEXT: Adding a new operation to the Suppliers feature
```

```
/ct-audience-persona-pattern
TOPIC: Chức năng Chứng từ bán hàng trả lại
AUDIENCE: Kế toán lên đơn
GOAL: Review workflow xem có đúng kế toán vận hành
CONTEXT: Kế toán review workflow để đưa vào vận hành
```

```
/ct-audience-persona-pattern
TOPIC: Vì sao Sales Order đã confirm không cho sửa, chỉ cho unconfirm/cancel
```
Minimal — the skill asks for `AUDIENCE`, `GOAL`, and `CONTEXT` one at a time (see Step 1 in [PROMPT.md](PROMPT.md)).

---

## Parameters

| Parameter | Required | Auto-detect | Description |
|-----------|----------|-------------|-------------|
| `TOPIC` | Yes | — | The BE/WPF concept or feature workflow to explain |
| `AUDIENCE` | No | Ask, one question at a time | Reader role — junior dev, mid-level dev, senior dev, PM, QA, or a business user (kế toán, warehouse staff, cashier) |
| `GOAL` | No | Ask | What the reader needs to be able to do after understanding (implement, review a PR, debug, sign off, write test cases) |
| `CONTEXT` | No | Infer from `TOPIC`, else ask | Which feature/module, or a direct path to a feature doc under `docs/` |

---

## Audience Role Reference

| Role signal in the user's answer | Bucket | Depth row (see OUTPUT_SCHEMA.md) |
|---|---|---|
| "mới join", "chưa quen", "junior" | Junior dev | Full snippet, step-by-step per layer |
| "đang fix bug", "đang implement", mid-level dev language | Mid-level dev | Key methods only, skip boilerplate |
| "senior", "review PR", "trade-off", "migration" | Senior dev | Architecture trade-offs, DI lifetime, query perf |
| "PM", "QA", "kế toán", "thủ kho", "review workflow", any non-dev business role | PM / QA / business | No code — flow diagram, table, numbered steps only |

If the answer doesn't clearly match a row, ask a follow-up rather than guessing — see [PROMPT.md](PROMPT.md) Step 1.

---

## `CONTEXT` Resolution

`CONTEXT` should resolve to **one feature name**, then both of its docs must be located before writing anything:

| Side | Path pattern |
|---|---|
| BE | `src/Lamour.Application/Features/[Feature]/docs/*.md` |
| WPF | `desktop-lamour/src/DesktopLamour/Features/HomePage/[Feature]/docs/*.md` |

If `CONTEXT` names only one side (e.g. a BE doc path), still locate and read the other side — see [GUARDRAILS.md](GUARDRAILS.md) "BE-only is incomplete".

If neither doc exists for the named feature, say so and proceed from live code only (Controller/UseCase + ViewModel/`.xaml`) — never fabricate doc content.
