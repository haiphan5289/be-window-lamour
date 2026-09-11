---
name: ct-review-artifact
description: "Publish a finished, audience-tailored explanation (feature workflow, business-rule review, architecture note) as a Claude Artifact page, styled with the Lamour project's review-doc design system (Be Vietnam Pro + IBM Plex Mono, ledger-inspired palette, light/dark tokens). Invoked automatically by ct-audience-persona-pattern after it produces a PM/QA/business-audience explanation — never regenerates or re-verifies content, only designs and publishes it."
argument-hint: "[FEATURE: <name>] [AUDIENCE: <reader role>] [GOAL: <reviewer's goal>] [CONTENT: <structured payload — see spec/INPUT_SCHEMA.md>]"
---

# ct-review-artifact — Business Review Artifact Publisher

Turn a finished, already-verified explanation into a published Artifact page — a reviewable document for a non-dev audience (kế toán, thủ kho, PM, QA) to read, tick off, and circulate. This skill owns **presentation and publishing only**: page design, theming, layout, and the `Artifact` tool call. It never invents content, never re-reads BE/WPF code, and never second-guesses what `ct-audience-persona-pattern` (or the user) already verified — it trusts the content payload it's given.

> Load `artifact-design` before writing any HTML — the global skill governs typographic/layout/theme fundamentals. This skill's spec on top of it is the project-specific design system (palette, type pairing, component shapes) so every review artifact in this project reads as one family.

---

## How to Use

**Invoked automatically**, at the end of `ct-audience-persona-pattern`, whenever the confirmed audience is PM/QA/business — see that skill's `spec/PROMPT.md` Step 4. The content payload it hands over follows [OUTPUT_SCHEMA.md](../ct-audience-persona-pattern/spec/OUTPUT_SCHEMA.md)'s "PM / QA / Business Deliverable" shape.

**Direct invocation** (any already-finished business-facing write-up, not only from ct-audience-persona-pattern):
```
/ct-review-artifact
FEATURE: Chứng từ hàng bán bị trả lại
AUDIENCE: Kế toán lên đơn
GOAL: Review workflow trước khi vận hành
CONTENT: <paste the structured explanation — real button labels, workflow steps, rules, open questions>
```

---

## File Structure

| File | Purpose |
|------|---------|
| [spec/INPUT_SCHEMA.md](spec/INPUT_SCHEMA.md) | Expected content payload shape and parameters |
| [spec/PROMPT.md](spec/PROMPT.md) | Step-by-step execution workflow |
| [spec/OUTPUT_SCHEMA.md](spec/OUTPUT_SCHEMA.md) | The Lamour review-doc design system: tokens, type pairing, component patterns |
| [spec/GUARDRAILS.md](spec/GUARDRAILS.md) | Rules already learned about scope, sourcing, and honest presentation |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

---

## Execution

Load and execute: **[spec/PROMPT.md](spec/PROMPT.md)**

Fallback — if @-references do not resolve, use the Read tool:
```
Read /Users/hai.phan/Desktop/haiphan/be-window-lamour/.claude/skills/ct-review-artifact/spec/PROMPT.md
```
