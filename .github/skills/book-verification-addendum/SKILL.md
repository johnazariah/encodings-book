---
name: book-verification-addendum
description: 'Verify follow-up manuscript fixes against an existing action plan. Use for staged verification, append-only review updates, commit-by-commit review, and avoiding standalone verification markdown files.'
argument-hint: 'State which fixes or commits to verify and where the authoritative audit file lives.'
user-invocable: true
---

# Book Verification Addendum

Use this skill when the user has already made fixes and wants a disciplined verification pass rather than a fresh review from scratch.

## When to Use

- after a tier of fixes lands;
- when reviewing a specific commit or set of changed files;
- when checking whether an action-plan item is actually closed; and
- when the user wants follow-up verification folded into the main audit file.

## Procedure

1. Read the relevant section of the action plan.
2. Inspect the changed files or commit diff.
3. Verify the exact issue that was previously raised.
4. Check for repeated instances elsewhere in the manuscript.
5. Append a dated addendum to the existing audit file.

## Rules

- Do not create a new markdown file for each verification pass.
- Scope the pass to the requested fixes unless new evidence forces a broader note.
- Mark clearly whether items are resolved, partially resolved, or still open.

## Recommended Template

- [Verification addendum template](./assets/verification-addendum-template.md)
