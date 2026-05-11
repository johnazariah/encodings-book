# What Quantum Computers Are Actually For — Agent Handoff

> **Project:** What Quantum Computers Are Actually For
> **Subtitle:** Eight problems from logistics to climate — and the algorithms that could solve them
> **Owner:** John Azariah (johnazariah)
> **Repo:** https://github.com/johnazariah/quantum-bottleneck (private)
> **Status:** All 8 units drafted, pre-publication

---

## What this project is

A book (blog-first, then expanded to print) that teaches quantum computing **application-first**. Eight units, each built around a real industry problem:

| # | Unit | Hook | Quantum Angle |
|---|------|------|---------------|
| 1 | Logistics | UPS saves $50M/year shaving 1 mile off routes (TSP/VRP) | QAOA on graph problems |
| 2 | Cryptography | RSA trapdoor that quantum can kick open | Shor's / period-finding |
| 3 | Drug Discovery | $2B and 12 years per new drug | VQE / molecular simulation |
| 4 | Machine Learning | 10⁸ features in a recommender | Quantum kernels / sampling |
| 5 | Finance | Monte Carlo pricing of derivatives | Quantum amplitude estimation |
| 6 | Supply Chains | Scheduling 10,000 nurses across 50 hospitals | QUBO / quantum annealing |
| 7 | Materials Science | Room-temp superconductivity prediction | QPE / Hubbard models |
| 8 | Climate & Energy | Catalyst design for carbon capture | Quantum simulation / embedding |

The core thesis: **start with a problem people already care about, then pull in only the quantum machinery needed to solve it.** No qubit-first pedagogy.

## Current state

- **SPEC.md** — Complete and detailed. Contains unit breakdowns, pedagogical principles, format strategy, and open questions. **Read this first.**
- **README.md** — Project overview with unit table and repo structure.
- **manuscript/** — All 8 units drafted (~15,500 words total, ~2,000 words per unit). First drafts — each needs expansion to ~5,000 words.
- **notebooks/** — 3 companion notebooks (Units 1–3). All run on cloud Quokka via HTTP POST.
- **figures/** — Empty.

This project is in the **first draft review phase**. Next steps: expand units, create remaining notebooks (4–8), set up Jupyter Book for publishing.

## The spec (SPEC.md) — key decisions

### Unit template (every chapter follows this)

1. **The Hook** (~800 words) — Industry problem. Real numbers, real stakes. No quantum yet.
2. **The Bottleneck** (~1000 words) — Mathematical structure that makes it hard classically.
3. **The Quantum Angle** (~1500 words) — The algorithm that addresses this bottleneck. Quantum concepts introduced only as needed.
4. **Worked Example** (~1000 words) — Toy-sized but complete. Runnable Jupyter notebook.
5. **Reality Check** (~500 words) — Honest: what works on NISQ today vs. what needs fault tolerance. Cite resource estimates.
6. **Chef's Notes** (~500 words) — Connections to other units, deeper references, open problems.

Target: ~5,000–5,500 words per unit + 1 companion notebook.

### Concept introduction policy

No concept is introduced before needed. The order matters:

- Qubits as binary variables → Unit 1 (Logistics)
- Superposition, interference, phase kickback, QFT → Unit 2 (Crypto)
- Fermions, Hamiltonians, VQE → Unit 3 (Drug Discovery)
- Hilbert space as feature space → Unit 4 (ML)
- Grover / amplitude amplification → Unit 5 (Finance)
- QUBO, annealing, tunnelling → Unit 6 (Supply Chains)
- QPE, Trotter decomposition → Unit 7 (Materials)
- Active-space methods, embedding → Unit 8 (Climate)

### Honesty policy

Every unit has a **Reality Check** box. Rules:
1. State the current best classical algorithm
2. State resource estimates for practical quantum advantage (cite papers)
3. Acknowledge dequantisation results where they exist
4. Never use "will" or "soon" — use "could", "in principle", "if error rates improve"

### Publication strategy

- **Phase 1 (months 1–8):** Blog series — one unit/month as a standalone ~5,000-word post with companion notebook
- **Phase 2 (months 9–12):** Expand to book — add depth, exercises, connective tissue
- **Phase 3:** Course integration with UTS quantum computing curriculum

### Open questions (from SPEC.md §8)

These are unresolved. Ask John before making decisions on:
- Unit ordering (current: increasing quantum sophistication; alternative: by closeness to practical advantage)
- Notebook platform (Qiskit only vs. best-tool-per-unit)
- Coupling with Quokka Cookbook (shared site? shared repo? independent?)
- Co-authorship (solo? with Chris Ferrie? with Chris and Simon Devitt?)
- Whether to keep the ML unit (most contentious application area)
- Blog hosting platform

## Priority work items

1. **Draft Unit 1 (Logistics/QAOA)** — this is the natural starting point. It introduces qubits, cost Hamiltonians, and variational loops. Write `manuscript/01-logistics.md` following the unit template. Create `notebooks/01-logistics.ipynb` with a MaxCut QAOA worked example.

2. **Draft Unit 2 (Cryptography/Shor)** — introduces superposition, interference, QFT, phase kickback. The heaviest "new machinery" chapter.

3. **Set up the blog infrastructure** — if going with GitHub Pages (likely mkdocs-material, same as Quokka Cookbook), configure `mkdocs.yml`, set up GH Actions for deploy.

## Style guide

- **Tone:** Confident, direct, opinionated about pedagogy. Conversational but never dumbed down.
- **The zoom-in/zoom-out rhythm:** WIDE (industry) → NARROW (math bottleneck) → QUANTUM (structural advantage) → CODE (worked example) → WIDE (so what?). This is the book's signature.
- **Math:** KaTeX. Introduce notation in words before using it.
- **Worked examples:** Must be runnable. Use Jupyter notebooks. Toy-sized (like H₂ in the encodings book) — small enough to execute on a laptop, large enough to show the real algorithm at work.
- **Figures:** Scaling plots (log-log), circuit diagrams, cost landscape visualisations. Save to `figures/`.
- **No hype:** See honesty policy above. Every Reality Check must cite specific papers.

## Related projects

- **[quantum-workbooks](https://github.com/johnazariah/quantum-workbooks)** — public umbrella repo for runnable companions and open teaching material. `bottleneck/` is the export surface for this project; `cookbook/` is the former Quokka Cookbook surface for runnable QASM recipes.
- **[encodings-book](../encodings-book/)** — fermion-to-qubit encodings textbook. Unit 3 (Drug Discovery) bridges to this. Unit 8 (Climate) uses the full pipeline.
- **[encodings](https://github.com/johnazariah/encodings)** — F# library for fermion-to-qubit encodings (companion code to the encodings book).

## Context about John

- PhD student at UTS, Centre for Quantum Software & Information
- Supervisor: Dr Christopher Ferrie
- Research: computational mechanics, quantum emergence, fermion-to-qubit encodings
- Working style: discusses ideas and writes specs/documents/papers; delegates code implementation to agents
- Prefers LaTeX for papers, Markdown for blogs, shared bibliography approach
