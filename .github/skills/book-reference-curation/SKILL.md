---
name: book-reference-curation
description: 'Verify quantitative claims and archive the right papers. Use for citation sweeps, reference-gap detection, resource-estimate verification, and deciding which PDFs belong in .references for a technical book repo.'
argument-hint: 'State whether you want a full citation sweep or only major claims, and whether central PDFs should be added to .references.'
user-invocable: true
---

# Book Reference Curation

Use this skill when the manuscript needs stronger support for important claims or when the repo needs a curated set of local reference PDFs.

## When to Use

- resource estimates need checking;
- hooks use strong quantitative claims;
- reality checks need hard citations;
- the repo maintains a `.references/` folder; and
- you want to know which claims feel under-supported.

## Procedure

1. Extract the strongest claims from hooks, worked examples, and reality checks.
2. Prioritize claims that affect trust: benchmark numbers, asymptotics, timelines, and empirical comparisons.
3. Match each claim to a paper, report, or clearly attributable source.
4. Check whether the PDF is already present in `.references/`.
5. Record missing or weakly supported claims.

## Rules

- Do not try to cite every sentence.
- Focus on claims a skeptical technical reader would challenge.
- Distinguish primary sources from secondary summaries.
- If no freely accessible version exists, say so clearly.

## Recommended Template

- [Reference curation checklist](./assets/reference-curation-checklist.md)
