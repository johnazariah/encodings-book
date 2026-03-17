#!/usr/bin/env python3
"""Replace Unicode characters in a Pandoc-generated LaTeX file with
pdflatex-compatible equivalents.

Handles text mode and code (Highlighting/Shaded) environments differently:
  - Text mode:  α → $\\alpha$,  → → $\\rightarrow$,  ₂ → \\textsubscript{2}
  - Code blocks: α → alpha,     → → ->,              ₂ → 2

Usage:
    python3 scripts/convert-to-pdflatex.py arxiv-submission/manuscript.tex

Replaces Unicode characters in text and code contexts differently.
When Pandoc targets pdflatex (--pdf-engine=pdflatex), it still passes
through Unicode from markdown sources. This script replaces those chars
with pdflatex-compatible equivalents.
"""

import sys

# ── Unicode → LaTeX mappings ──

TEXT_REPLACEMENTS = {
    "\u2080": r"\textsubscript{0}",
    "\u2082": r"\textsubscript{2}",
    "\u2083": r"\textsubscript{3}",
    "\u2084": r"\textsubscript{4}",
    "\u2192": r"$\rightarrow$",
    "\u2194": r"$\leftrightarrow$",
    "\u2248": r"$\approx$",
    "\u2264": r"$\leq$",
    "\u2265": r"$\geq$",
    "\u00d7": r"$\times$",
    "\u03b1": r"$\alpha$",
    "\u03b2": r"$\beta$",
    "\u03b4": r"$\delta$",
    "\u03c3": r"$\sigma$",
    "\u03bc": r"$\mu$",
    "\u03b8": r"$\theta$",
    "\u0394": r"$\Delta$",
    "\u27e8": r"$\langle$",
    "\u27e9": r"$\rangle$",
    "\u2713": r"$\checkmark$",
    "\u2717": r"$\times$",
    "\u2718": r"$\times$",
    "\u2020": r"\textdagger{}",
    "\u2550": "=",
    "\u2014": "---",
    "\u2013": "--",
    "\u2212": "-",
    "\u00b0": r"\textdegree{}",
}

CODE_REPLACEMENTS = {
    "\u2080": "0",
    "\u2082": "2",
    "\u2083": "3",
    "\u2084": "4",
    "\u2192": "->",
    "\u2194": "<->",
    "\u2248": "~",
    "\u2264": "<=",
    "\u2265": ">=",
    "\u00d7": "x",
    "\u03b1": "alpha",
    "\u03b2": "beta",
    "\u03b4": "delta",
    "\u03c3": "sigma",
    "\u03bc": "mu",
    "\u03b8": "theta",
    "\u0394": "Delta",
    "\u27e8": "<",
    "\u27e9": ">",
    "\u2713": "Y",
    "\u2717": "N",
    "\u2718": "N",
    "\u2020": "+",
    "\u2550": "=",
    "\u2014": "---",
    "\u2013": "--",
    "\u2212": "-",
    "\u00b0": "deg",
}

SAFE_CHARS = set("ÅäöøüÄÜ§")


def convert(path: str) -> None:
    with open(path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    new_lines = []
    in_code = False

    for line in lines:
        # Track code environments
        if "\\begin{Highlighting}" in line or "\\begin{Shaded}" in line:
            in_code = True
        if "\\end{Highlighting}" in line or "\\end{Shaded}" in line:
            in_code = False

        # Apply context-appropriate replacements
        replacements = CODE_REPLACEMENTS if in_code else TEXT_REPLACEMENTS
        for char, replacement in replacements.items():
            line = line.replace(char, replacement)

        new_lines.append(line)

    with open(path, "w", encoding="utf-8") as f:
        f.writelines(new_lines)

    # ── Verify ──
    with open(path, "r", encoding="utf-8") as f:
        text = f.read()

    remaining = sum(1 for c in text if ord(c) > 127 and c not in SAFE_CHARS)
    print(f"Conversion complete: {path}")
    print(f"  Remaining non-ASCII (excluding safe Latin-1): {remaining}")
    if remaining > 0:
        seen = set()
        for c in text:
            if ord(c) > 127 and c not in SAFE_CHARS and c not in seen:
                seen.add(c)
                print(f"  WARNING: unmapped char U+{ord(c):04X} ({c})")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print(f"Usage: {sys.argv[0]} <manuscript.tex>", file=sys.stderr)
        sys.exit(1)
    convert(sys.argv[1])
