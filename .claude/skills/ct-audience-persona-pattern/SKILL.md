---
name: ct-audience-persona-pattern
description: "Explain a BE Window Lamour concept or feature workflow (Clean Architecture layering, EF Core, UseCase/Repository pattern, JWT auth, WPF desktop integration) tailored to a specific audience — junior dev, senior engineer, PM, QA, or a business user like kế toán — with depth, terminology, and examples calibrated to that reader instead of a generic technical dump. For a business/PM/QA audience, the finished explanation is published as an Artifact via ct-review-artifact."
argument-hint: "[TOPIC: <concept/feature>] [AUDIENCE: <reader role>] [GOAL: <what they do after>] [CONTEXT: <feature/module/doc path>]"
---

# ct-audience-persona-pattern — Audience-Tailored Explanation Skill

Adapt explanations of BE Window Lamour concepts to who's actually asking — a junior dev implementing a UseCase reads differently from a kế toán reviewing why a stock validation blocks confirming a return document. Grounds every explanation in **both** BE and WPF code (never one side only), verified against the live codebase, then calibrates depth to the reader.

> **Anti-Hallucination:** Verify every class, interface, route, EF entity, ViewModel property, and XAML binding against the codebase before generating any example. See [ct-anti-hallucination](../ct-anti-hallucination/SKILL.md).

---

## How to Use

**Full input:**
```
/ct-audience-persona-pattern
TOPIC: Chức năng Chứng từ bán hàng trả lại
AUDIENCE: Kế toán lên đơn
GOAL: Review workflow xem có đúng kế toán vận hành
CONTEXT: src/Lamour.Application/Features/SalesReturn/docs/sales-return.md
```

**Minimal — let the skill ask:**
```
/ct-audience-persona-pattern
TOPIC: Vì sao Sales Order đã confirm không cho sửa, chỉ cho unconfirm/cancel
```

All parameters besides `TOPIC` are optional — see [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) for the ask-one-at-a-time flow when omitted.

---

## File Structure

| File | Purpose |
|------|---------|
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Invocation syntax, parameters, audience-role reference |
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | Depth Calibration table + required explanation shape |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Doc-staleness, BE/WPF dual-verification, and artifact-delivery rules already learned |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

---

## Execution

Load and execute: **[spec/PROMPT.md](spec/PROMPT.md)**

Fallback — if @-references do not resolve, use the Read tool:
```
Read /Users/hai.phan/Desktop/haiphan/be-window-lamour/.claude/skills/ct-audience-persona-pattern/spec/PROMPT.md
```
