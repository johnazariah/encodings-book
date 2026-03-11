# ══════════════════════════════════════════════════════════════
# From Molecules to Quantum Circuits — Build System
# ══════════════════════════════════════════════════════════════
#
# Usage:
#   make              Build manuscript.pdf
#   make sample       Build sample.pdf (first 7 chapters)
#   make clean        Remove generated files
#   make word-count   Print word counts per chapter
#   make diagrams     Render mermaid diagrams only
#   make data         Regenerate H₂ and H₂O data
#   make leanpub      Trigger Leanpub publish
#   make preview      Trigger Leanpub preview
#
# Prerequisites:
#   pandoc, xelatex, mmdc, python3  (all installed by devcontainer)
#   LEANPUB_API_KEY env var (for leanpub/preview targets)

SHELL := /bin/bash

# ── Directories ──
MS_DIR      := manuscript
CODE_DIR    := code
IMG_DIR     := $(MS_DIR)/mermaid-images
OUT         := $(MS_DIR)/manuscript.pdf
SAMPLE_OUT  := $(MS_DIR)/sample.pdf

# ── Leanpub ──
LEANPUB_SLUG := from-molecules-to-quantum-circuits

# ── Source files ──
CHAPTERS     := $(shell cat $(MS_DIR)/Book.txt | sed 's|^|$(MS_DIR)/|')
SAMPLE_CHAPS := $(shell cat $(MS_DIR)/Sample.txt | sed 's|^|$(MS_DIR)/|')

# ── Pandoc settings ──
PANDOC      := pandoc
LUA_FILTER  := $(MS_DIR)/mermaid.lua
PREAMBLE    := $(MS_DIR)/preamble.tex

PANDOC_OPTS := \
  --pdf-engine=xelatex \
  --lua-filter=$(LUA_FILTER) \
  -H $(PREAMBLE) \
  -V geometry:margin=1in \
  -V fontsize=11pt \
  -V classoption=oneside \
  -V mainfont="Latin Modern Roman" \
  -V sansfont="Latin Modern Sans" \
  -V monofont="Latin Modern Mono" \
  -V mathfont="Latin Modern Math" \
  -V title="From Molecules to Quantum Circuits" \
  -V subtitle="A Practical Guide to Fermion-to-Qubit Encodings" \
  -V author="John S Azariah" \
  -V date="March 2026" \
  --toc \
  --toc-depth=2 \
  --highlight-style=tango \
  --top-level-division=chapter \
  -V colorlinks=true \
  -V linkcolor=blue \
  -V urlcolor=blue

# ══════════════════════════════════════════════════════════════
#  Targets
# ══════════════════════════════════════════════════════════════

.PHONY: all clean word-count diagrams data sample leanpub preview leanpub-status

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
	@rm -rf $(IMG_DIR)
	$(PANDOC) $(SAMPLE_CHAPS) -o $(SAMPLE_OUT) $(PANDOC_OPTS)
	@echo "Done: $$(python3 -c "import pymupdf; d=pymupdf.open('$(SAMPLE_OUT)'); print(f'{d.page_count} pages'); d.close()" 2>/dev/null || echo '(install pymupdf for page count)')"
	@ls -lh $(SAMPLE_OUT)

clean:
	rm -rf $(IMG_DIR) $(OUT) $(SAMPLE_OUT)

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

# ── Data generation (requires pyscf) ──
data: $(CODE_DIR)/h2_dissociation.csv $(CODE_DIR)/h2o_bond_angle_coarse.csv

$(CODE_DIR)/h2_dissociation.csv: $(CODE_DIR)/ch18-dissociation-scan.py
	python3 $<

$(CODE_DIR)/h2o_bond_angle_coarse.csv: $(CODE_DIR)/ch19-bond-angle-scan.py
	python3 $<

# ── Labs ──
.PHONY: lab-check
lab-check:
	@echo "Checking labs..."
	@for f in labs/*.fsx; do \
	  echo "  $$f"; \
	done

# ── Leanpub ──
.PHONY: leanpub-check
leanpub-check:
	@if [ -z "$(LEANPUB_API_KEY)" ]; then \
	  echo "Error: LEANPUB_API_KEY not set"; \
	  echo "  export LEANPUB_API_KEY=your-key-here"; \
	  exit 1; \
	fi

leanpub: $(OUT) $(SAMPLE_OUT) leanpub-check
	@echo "Publishing to Leanpub ($(LEANPUB_SLUG))..."
	@curl -s -X POST \
	  "https://leanpub.com/$(LEANPUB_SLUG)/publish.json" \
	  -d "api_key=$(LEANPUB_API_KEY)" \
	  | python3 -c "import sys,json; r=json.load(sys.stdin); print(r.get('message', r))"
	@echo "Publish triggered. Check https://leanpub.com/$(LEANPUB_SLUG)"

preview: leanpub-check
	@echo "Triggering Leanpub preview ($(LEANPUB_SLUG))..."
	@curl -s -X POST \
	  "https://leanpub.com/$(LEANPUB_SLUG)/preview.json" \
	  -d "api_key=$(LEANPUB_API_KEY)" \
	  | python3 -c "import sys,json; r=json.load(sys.stdin); print(r.get('message', r))"
	@echo "Preview triggered. Check https://leanpub.com/$(LEANPUB_SLUG)"

leanpub-status: leanpub-check
	@curl -s \
	  "https://leanpub.com/$(LEANPUB_SLUG)/job_status.json?api_key=$(LEANPUB_API_KEY)" \
	  | python3 -c "import sys,json; r=json.load(sys.stdin); print(json.dumps(r, indent=2))"
