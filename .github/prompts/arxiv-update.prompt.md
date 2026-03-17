---
description: "Rebuild and update the arXiv submission package. Use when: arxiv, submission, upload, update tarball, rebuild manuscript."
agent: "agent"
---

# Update arXiv Submission

Rebuild the arXiv submission package from the current manuscript sources and prepare it for upload.

## Steps

1. **Regenerate TeX from markdown** — Run `make arxiv` in `/workspaces/encodings-book` to rebuild `arxiv-submission/manuscript.tex` from the markdown sources via Pandoc.

2. **Convert xelatex → pdflatex** — The `make arxiv` target produces xelatex-flavored TeX (Unicode chars, DejaVu font fallbacks, `\newunicodechar`). arXiv uses pdflatex. Run the conversion script at [scripts/convert-to-pdflatex.py](../../scripts/convert-to-pdflatex.py) to:
   - Remove the `iftex`/xelatex conditional preamble and replace with simple pdflatex setup (`fontenc`, `inputenc`, `textcomp`, `lmodern`, `textgreek`)
   - Remove the `newunicodechar`/DejaVu fallback block
   - Replace Unicode chars with LaTeX equivalents — **differently in text vs code blocks**:
     - **Text mode**: math-mode commands (`α` → `$\alpha$`, `→` → `$\rightarrow$`, `₂` → `\textsubscript{2}`)
     - **Code blocks** (Highlighting/Shaded environments): ASCII approximations (`α` → `alpha`, `→` → `->`, `₂` → `2`)
   - Keep Latin-1 chars (Å, ä, ö, ø, ü) that pdflatex handles natively

3. **Compile with pdflatex** — Run `pdflatex -interaction=nonstopmode manuscript.tex` twice (for cross-references) in the `arxiv-submission/` directory. Verify:
   - Exit code 0
   - Zero lines matching `^!` in `manuscript.log`
   - PDF is produced (expect ~160-170 pages)

4. **Rebuild tarball** — From inside `arxiv-submission/`, create the tarball:
   ```bash
   tar czf ../arxiv-submission.tar.gz manuscript.tex *.png
   ```
   The tarball should contain `manuscript.tex` + 27 PNG figures. **Do not include the PDF** — arXiv compiles from source.

5. **Verify end-to-end** — Extract to a temp directory and compile from scratch:
   ```bash
   cd /tmp && rm -rf arxiv-test && mkdir arxiv-test && cd arxiv-test
   tar xzf /workspaces/encodings-book/arxiv-submission.tar.gz
   pdflatex -interaction=nonstopmode manuscript.tex > /dev/null 2>&1
   pdflatex -interaction=nonstopmode manuscript.tex > /dev/null 2>&1
   grep '^!' manuscript.log | wc -l  # must be 0
   grep "Output written" manuscript.log
   ```

6. **Commit and push** — Stage and commit all three artifacts:
   ```bash
   git add arxiv-submission/manuscript.tex arxiv-submission/manuscript.pdf arxiv-submission.tar.gz
   git commit -m "arxiv: update submission package"
   git push origin main
   ```

7. **Open PDF for proofing** — Open `arxiv-submission/manuscript.pdf` for the user to review.

## Key warnings

- The `make arxiv` target uses `sed -i` to strip image path prefixes — this is already handled.
- The `textgreek` package must be available for `\textalpha`, `\textbeta`, etc.
- `hyperref` will produce ~39 "Token not allowed in PDF string" warnings — these are harmless.
- The `\texttimes{}` command in code blocks should become `x` not `$\times$`.
- Watch for `\checkmark` — it requires `amssymb` which is loaded via `amsmath,amssymb`.
