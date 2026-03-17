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
#
# Prerequisites:
#   pandoc, xelatex, mmdc, python3  (all installed by devcontainer)

SHELL := /bin/bash

# ── Directories ──
MS_DIR      := manuscript
CODE_DIR    := code
IMG_DIR     := $(MS_DIR)/mermaid-images
OUT         := $(MS_DIR)/manuscript.pdf
SAMPLE_OUT  := $(MS_DIR)/sample.pdf

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
  -V subtitle="A Computational Guide to Fermion-to-Qubit Encodings" \
  -V author="John S Azariah" \
  -V thanks="Centre for Quantum Software and Information, University of Technology Sydney. Email: john.azariah@student.uts.edu.au" \
  -V date="March 2026" \
  --metadata=abstract:"This tutorial covers the complete pipeline from molecular electronic structure to quantum circuit compilation for quantum simulation. Starting from one-body and two-body integrals of the hydrogen molecule (H₂) in the STO-3G basis, we construct the qubit Hamiltonian explicitly under six fermion-to-qubit encodings (Jordan-Wigner, Bravyi-Kitaev, Parity, balanced binary tree, balanced ternary tree, and a Vlasov complete-ternary-tree encoding), verify spectral equivalence across encodings, reduce qubit count via diagonal and Clifford Z₂ symmetry tapering, decompose the tapered Hamiltonian into Trotter circuits with explicit CNOT gate counts, and export the result to OpenQASM 3.0 and Q\#. Every formula has a corresponding executable computation in the companion FockMap library, an open-source F\# framework for symbolic Fock-space operator algebra. Two running examples — H₂ (4 qubits) and H₂O (12–14 qubits) — are developed from molecular geometry to quantum circuit, including computing the H₂ dissociation curve and the H-O-H bond angle from first principles. The tutorial comprises 23 chapters with exercises, 10 companion scripts, and 10 interactive laboratory sessions. Companion software and source at https://github.com/johnazariah/encodings." \
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

.PHONY: all clean word-count diagrams data sample

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

# ── arXiv submission ──
ARXIV_DIR   := arxiv-submission
ARXIV_TEX   := $(ARXIV_DIR)/manuscript.tex

arxiv: $(CHAPTERS) $(LUA_FILTER) $(PREAMBLE) $(MS_DIR)/Book.txt
	@echo "Building arXiv submission package..."
	@rm -rf $(ARXIV_DIR) $(IMG_DIR)
	@mkdir -p $(ARXIV_DIR)
	$(PANDOC) $(CHAPTERS) -o $(ARXIV_TEX) -s $(PANDOC_OPTS)
	@if [ -d $(IMG_DIR) ] && [ "$$(ls -A $(IMG_DIR))" ]; then \
	  cp $(IMG_DIR)/*.png $(ARXIV_DIR)/; \
	fi
	@cp $(CODE_DIR)/*.png $(ARXIV_DIR)/ 2>/dev/null || true
	@sed -i 's|manuscript/mermaid-images/||g; s|code/||g' $(ARXIV_TEX)
	@cd $(ARXIV_DIR) && tar czf ../arxiv-submission.tar.gz *
	@echo "Created arxiv-submission.tar.gz with:"
	@tar tzf arxiv-submission.tar.gz | sed 's/^/  /'
	@ls -lh arxiv-submission.tar.gz

clean:
	rm -rf $(IMG_DIR) $(OUT) $(SAMPLE_OUT) $(ARXIV_DIR) arxiv-submission.tar.gz

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

leanpub-status: leanpub-check
	@curl -s \
	  "https://leanpub.com/$(LEANPUB_SLUG)/job_status.json?api_key=$(LEANPUB_API_KEY)" \
	  | python3 -c "import sys,json; r=json.load(sys.stdin); print(json.dumps(r, indent=2))"
