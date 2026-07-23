#!/bin/bash
set -e

echo "Setting up the book development environment..."

# Python packages for integral generation and plotting
pip install --quiet --break-system-packages -r requirements-data.txt

# Mermaid CLI for diagram rendering
sudo npm install -g @mermaid-js/mermaid-cli

# Playwright for ARM-native Chrome (used by mmdc)
pip install --quiet --break-system-packages playwright
python -m playwright install --with-deps chromium

# LaTeX for PDF generation
sudo apt-get update -qq
sudo apt-get install -y -qq texlive-xetex texlive-latex-extra lmodern pandoc

# Restore .NET tools if any
dotnet tool restore 2>/dev/null || true

echo ""
echo "✓ Environment ready!"
echo "  make              — build molecules-to-circuits.pdf"
echo "  make sample       — build molecules-to-circuits-sample.pdf"
echo "  make word-count   — chapter word counts"
echo "  dotnet fsi labs/01-first-encoding.fsx  — run a lab"
