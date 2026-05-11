---
name: book-audit-workflow
description: 'Run a disciplined technical-book review workflow. Use for correctness audits, action plans, single-audit-file discipline, staged verification, and keeping review artifacts under control in a manuscript repo.'
argument-hint: 'State the book title, intended audience, and whether to start with correctness, verification, or whole-book review.'
user-invocable: true
---

# Book Audit Workflow

Use this skill when you need to review a technical manuscript with the same discipline used for a serious book project rather than a casual proofread.

## When to Use

- starting a fresh correctness audit;
- setting up a review process for a new book repo;
- preventing review-file sprawl;
- converting findings into a ranked action plan; and
- keeping later verification in the same authoritative audit file.

## Procedure

1. Inspect the repo structure, manuscript layout, build config, notebooks, and any existing `.review` or `.references` directories.
2. Reuse existing authoritative review files if they already exist.
3. Run correctness first: factual claims, maths, algorithms, internal consistency, and prose/notebook alignment.
4. Write findings to one authoritative audit file.
5. Create or update one authoritative action-plan file.
6. After user fixes land, append dated verification addenda to the audit file instead of creating standalone follow-up markdown files.
7. Only once correctness is substantially closed, move on to whole-book style review and production planning.

## Artifact Rules

- One main correctness audit file in `.review/`
- One main action-plan file in `.review/ACTION-PLAN.md`
- No review-file sprawl unless the user explicitly asks for it
- `.references/` is for central papers that materially support review findings

## Recommended Templates

- [Correctness audit template](./assets/correctness-audit-template.md)
- [Action plan template](./assets/action-plan-template.md)
