---
description: "Cut a new release with PDF, EPUB, and labs. Use when: release, publish, tag, cut release, new version."
agent: "agent"
---

# Cut a Release

Prepare a release of *From Molecules to Quantum Circuits*, updating version references, building artifacts locally, and tagging for the CI release workflow.

## Inputs

Ask the user for the version number if not provided (e.g. `v1.0.0`). Follow semver:
- **Major**: structural changes (chapters added/removed/reordered)
- **Minor**: content changes (new sections, significant rewrites)
- **Patch**: fixes (typos, corrections, clarifications)

## Steps

### 1. Preflight checks

```bash
cd /Users/johnaz/PhD/encodings-book
git status              # must be clean
git pull origin main    # must be up to date
make word-count         # record for release notes
```

### 2. Update version references

Update the date and version in these files:

- **`manuscript/foreword.md`** — update the date line if present
- **`manuscript/Leanpub.yml`** — not used, skip if absent
- **`CITATION.cff`** — update `date-released` and `version` fields
- **`README.md`** — update the "At a Glance" word count and page count if they've changed

Run `make word-count` and update the README table if totals have changed significantly.

### 3. Build PDF locally (verify it compiles)

```bash
make clean && make
```

Verify:
- PDF is produced at `manuscript/molecules-to-circuits.pdf`
- Page count is reasonable (~160-170 pages)
- No LaTeX errors in the build output

### 4. Build EPUB locally (verify it renders)

```bash
pandoc $(cat manuscript/Book.txt | sed 's|^|manuscript/|') \
  -o manuscript/molecules-to-circuits.epub \
  --toc --toc-depth=2 \
  --lua-filter=manuscript/mermaid.lua \
  --highlight-style=tango \
  --metadata title="From Molecules to Quantum Circuits" \
  --metadata subtitle="A Computational Guide to Fermion-to-Qubit Encodings" \
  --metadata author="John S Azariah" \
  --top-level-division=chapter \
  --mathml
```

Verify the EPUB file is produced and is a reasonable size (>500KB).

### 5. Commit the release

```bash
git add -A
git commit -m "Release VERSION

From Molecules to Quantum Circuits VERSION

- N chapters, ~M words, ~P pages
- PDF, EPUB, and companion labs included
- [summarize key changes since last release]"
```

### 6. Tag and push

```bash
git tag VERSION
git push origin main
git push origin VERSION
```

The tag push triggers `.github/workflows/release.yml`, which:
1. Builds the PDF and sample PDF via `make` / `make sample`
2. Builds the EPUB via pandoc
3. Packages labs into a tarball
4. Creates a GitHub Release with all four artifacts attached

### 7. Verify the release

```bash
gh run list --workflow=release.yml --limit=1
```

Wait for the workflow to complete, then verify:

```bash
gh release view VERSION
```

Confirm all four artifacts are attached:
- `molecules-to-circuits.pdf`
- `molecules-to-circuits-sample.pdf`
- `molecules-to-circuits.epub`
- `labs-VERSION.tar.gz`

### 8. Post-release

Open the release page for the user to review:
```
https://github.com/johnazariah/molecules-to-circuits/releases/tag/VERSION
```

## Key warnings

- The Makefile uses `xelatex` — it must be installed locally (comes with the devcontainer).
- Mermaid diagrams require `mmdc` (mermaid-cli) to be installed for the PDF build.
- The EPUB uses `--mathml` not `--mathjax` — Kindle/e-readers need MathML for equations.
- Do NOT commit the built PDF or EPUB to the repo — they are CI-generated release artifacts only.
- The `Book.txt` file in `manuscript/` defines chapter ordering for both PDF and EPUB builds.
