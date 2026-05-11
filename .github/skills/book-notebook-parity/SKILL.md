---
name: book-notebook-parity
description: 'Audit notebook and manuscript alignment. Use for companion notebook credibility, worked-example parity, execution blockers, keep-or-rewrite decisions, and ordering notebook repairs in a book repo.'
argument-hint: 'State whether the notebooks are meant to be faithful implementations, pedagogical demos, or a mix.'
user-invocable: true
---

# Book Notebook Parity

Use this skill when notebooks are part of the book's argument and need to be reviewed as seriously as the prose.

## When to Use

- companion notebooks exist for chapters or units;
- the manuscript makes claims about notebook outputs;
- notebooks may be stale or simplified; and
- you need a keep / relabel / rewrite recommendation.

## Procedure

1. Read the surrounding chapter before judging the notebook.
2. Check whether the notebook's title and framing match what it actually does.
3. Separate execution blockers from conceptual blockers.
4. Classify each notebook:
   - keep and validate;
   - relabel and repair; or
   - rewrite.
5. Recommend a repair order based on trust risk.

## Review Questions

- Does the notebook implement the algorithm it claims to implement?
- Does the prose overpromise relative to the notebook?
- Is the notebook runnable in the current environment?
- Are the simplifications honest and visible?
- Are chapter links and references current?

## Recommended Template

- [Notebook parity audit template](./assets/notebook-parity-template.md)
