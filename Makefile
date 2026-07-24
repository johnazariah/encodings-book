# ══════════════════════════════════════════════════════════════
# From Molecules to Quantum Circuits — Build System
# ══════════════════════════════════════════════════════════════
#
# Usage:
#   make              Build molecules-to-circuits.pdf
#   make sample       Build molecules-to-circuits-sample.pdf (selected chapters)
#   make arxiv-pdflatex  Build pdflatex arXiv submission (tarball + PDF)
#   make clean        Remove generated files
#   make word-count   Print word counts per chapter
#   make diagrams     Render mermaid diagrams only
#   make data         Regenerate H₂ and H₂O data
#
# Prerequisites:
#   pandoc, xelatex, mmdc, python3  (all installed by devcontainer)

SHELL := /bin/bash

# ── Directories ──
MS_DIR      := manuscript
CODE_DIR    := code
IMG_DIR     := $(MS_DIR)/mermaid-images
OUT         := $(MS_DIR)/molecules-to-circuits.pdf
SAMPLE_OUT  := $(MS_DIR)/molecules-to-circuits-sample.pdf
EPUB_OUT    := $(MS_DIR)/molecules-to-circuits.epub

# ── Source files ──
CHAPTERS     := $(shell cat $(MS_DIR)/Book.txt | sed 's|^|$(MS_DIR)/|')
SAMPLE_CHAPS := $(shell cat $(MS_DIR)/Sample.txt | sed 's|^|$(MS_DIR)/|')

# ── Pandoc settings ──
PANDOC      := pandoc
LUA_FILTER  := $(MS_DIR)/mermaid.lua
PREAMBLE    := $(MS_DIR)/preamble.tex

PANDOC_COMMON := \
  --pdf-engine=xelatex \
  --lua-filter=$(LUA_FILTER) \
  --resource-path=$(MS_DIR):. \
  -H $(PREAMBLE) \
  -V geometry:margin=1in \
  -V fontsize=11pt \
  -V classoption=oneside \
  -V mainfont="DejaVu Serif" \
  -V sansfont="DejaVu Sans" \
  -V monofont="DejaVu Sans Mono" \
  -V title="From Molecules to Quantum Circuits" \
  -V subtitle="A Computational Guide to Fermion-to-Qubit Encodings" \
  -V author="John S Azariah" \
  -V date="March 2026" \
  --toc \
  --toc-depth=2 \
  --highlight-style=tango \
  --top-level-division=chapter \
  -V colorlinks=true \
  -V linkcolor=blue \
  -V urlcolor=blue

PANDOC_OPTS := $(PANDOC_COMMON) \
  --metadata=abstract:"This tutorial develops the translation layer from molecular electronic structure to logical quantum circuits. For H₂/STO-3G at 0.74 Å, generated PySCF integrals feed an independently verified fermionic matrix and Jordan-Wigner Pauli Hamiltonian before encoding, symmetry, product-formula, cost, and export concepts are applied. Separate PySCF scripts provide the H₂ dissociation reference and an H₂O FCI angular scan at fixed experimental O-H length; these chemistry references are not represented as energies produced by circuit construction alone. The tutorial covers six fermion-to-qubit encodings, physical-sector tapering requirements, Trotter decomposition, CNOT accounting, and OpenQASM/Q\# export across 23 chapters, 10 companion scripts, and 10 laboratory sessions. Companion software and source at https://github.com/johnazariah/encodings."

SAMPLE_FILTER := $(MS_DIR)/sample-filter.lua
SAMPLE_OPTS := $(PANDOC_COMMON) --lua-filter=$(SAMPLE_FILTER)

# ══════════════════════════════════════════════════════════════
#  Targets
# ══════════════════════════════════════════════════════════════

.PHONY: all clean word-count diagrams data sample epub verify-data pipeline-check

all: $(OUT)

$(OUT): $(CHAPTERS) $(LUA_FILTER) $(PREAMBLE) $(MS_DIR)/Book.txt
	@echo "Building manuscript..."
	@rm -rf $(IMG_DIR)
	$(PANDOC) $(CHAPTERS) -o $(OUT) $(PANDOC_OPTS)
	@echo "Done: $$(python3 -c "import pymupdf; d=pymupdf.open('$(OUT)'); print(f'{d.page_count} pages'); d.close()" 2>/dev/null || echo '(install pymupdf for page count)')"
	@ls -lh $(OUT)

sample: $(SAMPLE_OUT)

$(SAMPLE_OUT): $(SAMPLE_CHAPS) $(LUA_FILTER) $(PREAMBLE) $(MS_DIR)/Sample.txt
	@echo "Building sample..."
	$(PANDOC) $(SAMPLE_CHAPS) -o $(SAMPLE_OUT) $(SAMPLE_OPTS)
	@echo "Done: $$(python3 -c "import pymupdf; d=pymupdf.open('$(SAMPLE_OUT)'); print(f'{d.page_count} pages'); d.close()" 2>/dev/null || echo '(install pymupdf for page count)')"
	@ls -lh $(SAMPLE_OUT)

epub: $(EPUB_OUT)

$(EPUB_OUT): $(CHAPTERS) $(LUA_FILTER) $(MS_DIR)/Book.txt
	@echo "Building EPUB..."
	$(PANDOC) $(CHAPTERS) \
	  -o $(EPUB_OUT) \
	  --toc --toc-depth=2 \
	  --lua-filter=$(LUA_FILTER) \
	  --resource-path=$(MS_DIR):. \
	  --highlight-style=tango \
	  --metadata title="From Molecules to Quantum Circuits" \
	  --metadata subtitle="A Computational Guide to Fermion-to-Qubit Encodings" \
	  --metadata author="John S Azariah" \
	  --top-level-division=chapter \
	  --mathml
	@ls -lh $(EPUB_OUT)

# ── arXiv submission ──
ARXIV_DIR   := arxiv-submission
ARXIV_TEX   := $(ARXIV_DIR)/manuscript.tex

arxiv: $(CHAPTERS) $(LUA_FILTER) $(PREAMBLE) $(MS_DIR)/Book.txt
	@echo "Building arXiv submission package (xelatex)..."
	@rm -rf $(ARXIV_DIR) $(IMG_DIR)
	@mkdir -p $(ARXIV_DIR)
	$(PANDOC) $(CHAPTERS) -o $(ARXIV_TEX) -s $(PANDOC_OPTS)
	@if [ -d $(IMG_DIR) ] && [ "$$(ls -A $(IMG_DIR))" ]; then \
	  cp $(IMG_DIR)/*.png $(ARXIV_DIR)/; \
	fi
	@cp $(MS_DIR)/figures/*.png $(ARXIV_DIR)/ 2>/dev/null || true
	@sed -i 's|manuscript/mermaid-images/||g; s|manuscript/figures/||g; s|figures/||g' $(ARXIV_TEX)
	@cd $(ARXIV_DIR) && tar czf ../arxiv-submission.tar.gz *
	@echo "Created arxiv-submission.tar.gz with:"
	@tar tzf arxiv-submission.tar.gz | sed 's/^/  /'
	@ls -lh arxiv-submission.tar.gz

# ── arXiv submission (pdflatex — required by arXiv) ──
PREAMBLE_ARXIV := $(MS_DIR)/preamble-arxiv.tex
CONVERT_SCRIPT := scripts/convert-to-pdflatex.py

PANDOC_ARXIV_OPTS := \
  --pdf-engine=pdflatex \
  --lua-filter=$(LUA_FILTER) \
  --resource-path=$(MS_DIR):. \
  -H $(PREAMBLE_ARXIV) \
  -V geometry:margin=1in \
  -V fontsize=11pt \
  -V classoption=oneside \
  -V title="From Molecules to Quantum Circuits" \
  -V subtitle="A Computational Guide to Fermion-to-Qubit Encodings" \
  -V author="John S Azariah" \
  -V date="March 2026" \
  --toc \
  --toc-depth=2 \
  --highlight-style=tango \
  --top-level-division=chapter \
  -V colorlinks=true \
  -V linkcolor=blue \
  -V urlcolor=blue

arxiv-pdflatex: $(CHAPTERS) $(LUA_FILTER) $(PREAMBLE_ARXIV) $(MS_DIR)/Book.txt $(CONVERT_SCRIPT)
	@echo "Building arXiv submission package (pdflatex)..."
	@rm -rf $(ARXIV_DIR) $(IMG_DIR)
	@mkdir -p $(ARXIV_DIR)
	$(PANDOC) $(CHAPTERS) -o $(ARXIV_TEX) -s $(PANDOC_ARXIV_OPTS)
	@if [ -d $(IMG_DIR) ] && [ "$$(ls -A $(IMG_DIR))" ]; then \
	  cp $(IMG_DIR)/*.png $(ARXIV_DIR)/; \
	fi
	@cp $(MS_DIR)/figures/*.png $(ARXIV_DIR)/ 2>/dev/null || true
	@sed -i 's|manuscript/mermaid-images/||g; s|manuscript/figures/||g; s|figures/||g' $(ARXIV_TEX)
	@python3 $(CONVERT_SCRIPT) $(ARXIV_TEX)
	@echo "Compiling PDF (two passes)..."
	@cd $(ARXIV_DIR) && pdflatex -interaction=nonstopmode manuscript.tex > /dev/null 2>&1
	@cd $(ARXIV_DIR) && pdflatex -interaction=nonstopmode manuscript.tex > /dev/null 2>&1
	@if grep -q '^!' $(ARXIV_DIR)/manuscript.log; then \
	  echo "ERROR: LaTeX errors found:"; \
	  grep '^!' $(ARXIV_DIR)/manuscript.log; \
	  exit 1; \
	fi
	@grep "Output written" $(ARXIV_DIR)/manuscript.log
	@rm -f $(ARXIV_DIR)/manuscript.aux $(ARXIV_DIR)/manuscript.log \
	       $(ARXIV_DIR)/manuscript.out $(ARXIV_DIR)/manuscript.toc
	@cd $(ARXIV_DIR) && tar czf ../arxiv-submission.tar.gz manuscript.tex *.png
	@echo "Created arxiv-submission.tar.gz with:"
	@tar tzf arxiv-submission.tar.gz | sed 's/^/  /'
	@ls -lh arxiv-submission.tar.gz $(ARXIV_DIR)/manuscript.pdf

clean:
	rm -rf $(IMG_DIR) $(OUT) $(SAMPLE_OUT) $(EPUB_OUT) $(ARXIV_DIR) arxiv-submission.tar.gz

word-count:
	@echo "Chapter word counts:"
	@for f in $(CHAPTERS); do \
	  printf "  %-40s %5d\n" "$$(basename $$f)" "$$(wc -w < $$f)"; \
	done
	@echo "  ────────────────────────────────────────────────"
	@printf "  %-40s %5d\n" "TOTAL" "$$(cat $(CHAPTERS) | wc -w)"

diagrams:
	@echo "Rendering mermaid diagrams..."
	@rm -rf $(IMG_DIR) && mkdir -p $(IMG_DIR)
	@$(PANDOC) $(CHAPTERS) -t native --lua-filter=$(LUA_FILTER) > /dev/null 2>&1
	@echo "Rendered $$(ls $(IMG_DIR)/*.png 2>/dev/null | wc -l) diagrams"

# ── Data generation (requires requirements-data.txt) ──
data:
	python3 $(CODE_DIR)/ch18-generate-h2-integrals.py
	python3 $(CODE_DIR)/ch18-dissociation-scan.py
	python3 $(CODE_DIR)/ch19-bond-angle-scan.py
	python3 $(CODE_DIR)/ch09-verify-h2.py

verify-data:
	python3 $(CODE_DIR)/ch09-verify-h2.py
	dotnet fsi labs/03-compare-encodings.fsx
	bash scripts/check-data-idempotence.sh

pipeline-check:
	bash scripts/check-ch18-output-isolation.sh

# ── Labs ──
.PHONY: lab-check
lab-check:
	@set -euo pipefail; \
	echo "Executing labs..."; \
	for f in labs/[0-9][0-9]-*.fsx; do \
	  echo "  $$f"; \
	  dotnet fsi "$$f"; \
	done

leanpub-status: leanpub-check
	@curl -s \
	  "https://leanpub.com/$(LEANPUB_SLUG)/job_status.json?api_key=$(LEANPUB_API_KEY)" \
	  | python3 -c "import sys,json; r=json.load(sys.stdin); print(json.dumps(r, indent=2))"
