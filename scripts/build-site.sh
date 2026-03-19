#!/usr/bin/env bash
set -euo pipefail

# Build HTML site from manuscript chapters
# Usage: ./scripts/build-site.sh [output-dir]

SITE_DIR="${1:-_site}"
MS_DIR="manuscript"
TEMPLATE="$MS_DIR/template.html"
LUA_FILTER="$MS_DIR/mermaid.lua"
IMG_DIR="$MS_DIR/mermaid-images"

# Read chapter list
mapfile -t CHAPTERS < <(cat "$MS_DIR/Book.txt" | sed '/^$/d')
TOTAL=${#CHAPTERS[@]}

echo "Building site with $TOTAL chapters → $SITE_DIR/"
rm -rf "$SITE_DIR"
mkdir -p "$SITE_DIR"

# Copy images
if [ -d "$IMG_DIR" ]; then
  cp -r "$IMG_DIR" "$SITE_DIR/mermaid-images"
fi
if [ -d "code" ]; then
  mkdir -p "$SITE_DIR/code"
  cp code/*.png "$SITE_DIR/code/" 2>/dev/null || true
fi

# Build index page
{
  echo "# From Molecules to Quantum Circuits"
  echo ""
  echo "*A Computational Guide to Fermion-to-Qubit Encodings*"
  echo ""
  echo "**John S Azariah**"
  echo "Centre for Quantum Software and Information, University of Technology Sydney"
  echo ""
  echo "---"
  echo ""
  echo "📥 [Download PDF](https://github.com/johnazariah/encodings-book/releases/latest)"
  echo " | 💻 [Companion code](https://github.com/johnazariah/encodings)"
  echo " | 🧪 [Open in Codespaces](https://codespaces.new/johnazariah/encodings-book)"
  echo ""
  echo "---"
  echo ""
  echo "## Table of Contents"
  echo ""
  for i in "${!CHAPTERS[@]}"; do
    ch="${CHAPTERS[$i]}"
    html_name="${ch%.md}.html"
    title=$(head -1 "$MS_DIR/$ch" | sed 's/^# //')
    echo "- [$title]($html_name)"
  done
  echo ""
  echo "---"
  echo ""
  echo "*Manuscript text © John S Azariah. Code: MIT License.*"
} > /tmp/index.md

pandoc /tmp/index.md \
  -o "$SITE_DIR/index.html" \
  --template="$TEMPLATE" \
  --standalone \
  --metadata title="From Molecules to Quantum Circuits" \
  --mathjax

# Build each chapter
for i in "${!CHAPTERS[@]}"; do
  ch="${CHAPTERS[$i]}"
  html_name="${ch%.md}.html"

  # Compute prev/next
  PREV_META=""
  NEXT_META=""
  if [ "$i" -gt 0 ]; then
    prev_ch="${CHAPTERS[$((i-1))]}"
    PREV_META="--metadata=prev:${prev_ch%.md}.html"
  fi
  if [ "$i" -lt $((TOTAL-1)) ]; then
    next_ch="${CHAPTERS[$((i+1))]}"
    NEXT_META="--metadata=next:${next_ch%.md}.html"
  fi

  echo "  $html_name"

  pandoc "$MS_DIR/$ch" \
    -o "$SITE_DIR/$html_name" \
    --template="$TEMPLATE" \
    --standalone \
    --lua-filter="$LUA_FILTER" \
    --highlight-style=tango \
    --mathjax \
    $PREV_META $NEXT_META
done

echo "Site built: $(ls "$SITE_DIR"/*.html | wc -l) pages"
