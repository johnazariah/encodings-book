# From Molecules to Quantum Circuits

*A Computational Guide to Fermion-to-Qubit Encodings*

**John S Azariah**
Centre for Quantum Software and Information, University of Technology Sydney

[![DOI](https://zenodo.org/badge/1176397444.svg)](https://doi.org/10.5281/zenodo.18917465)
[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/johnazariah/encodings-book)

---

## Short Blurb

> Every chemistry textbook tells you that water's bond angle is 104.5°. Every quantum computing textbook tells you that fermions must be encoded as qubits. This book connects the two: a complete, executable pipeline from molecular geometry to quantum circuit, with six encoding schemes, qubit tapering, Trotterization, and circuit export — all computed explicitly, nothing left as an exercise.

---

## Book Description

**From Molecules to Quantum Circuits** is a self-contained computational guide covering the complete pipeline from molecular electronic structure to quantum circuit compilation for quantum simulation.

Every formula has a corresponding executable computation in the FockMap library, an open-source F# framework for symbolic Fock-space operator algebra. Every sign, every coefficient, every intermediate Pauli string is computed explicitly.

The guide covers six fermion-to-qubit encodings (Jordan-Wigner, Bravyi-Kitaev, Parity, balanced binary tree, balanced ternary tree, Vlasov tree), qubit tapering via diagonal and Clifford Z₂ symmetries, Trotter decomposition with CNOT cost analysis, and circuit output in OpenQASM and Q#.

Two running examples — H₂ (the simplest molecule) and H₂O (the most important molecule) — are developed from geometry to quantum circuit, including computing the H-O-H bond angle from first principles.

## Who This Book Is For

- **Graduate students** starting in quantum chemistry simulation who need to understand the encoding layer between chemistry and circuits
- **Physicists** crossing into quantum computing who know Hamiltonians but not encodings
- **Software engineers** building quantum simulation pipelines who need the physics explained carefully
- **Lecturers** building a course module on quantum simulation who want homework exercises with verifiable answers

**What we assume:** linear algebra (vectors, matrices, eigenvalues) and introductory quantum mechanics (wavefunctions, bra-ket notation).

**What we don't assume:** second quantization, Pauli algebra, fermion-to-qubit encodings, or F#. All are developed from scratch.

## Back Cover Copy

> *You know Hamiltonians. You know qubits. But between the molecule and the circuit lies a translation layer — fermion-to-qubit encoding — that most resources treat as a black box.*
>
> *This book opens the box.*
>
> *Starting from the one-body and two-body integrals of H₂, we build the 15-term qubit Hamiltonian by hand, verify it against exact eigenvalues, and then show how six different encodings produce the same physics with dramatically different circuit costs. We taper away redundant qubits using Z₂ symmetries, decompose the Hamiltonian into a Trotter circuit, count every CNOT gate, and export the result to OpenQASM and Q#.*
>
> *Then we do it again for water — and compute its bond angle from first principles.*
>
> *Every formula is executable. Every intermediate result is inspectable. The companion FockMap library runs the same pipeline on any molecule you give it.*
>
> *The quantum computer isn't ready yet. The pipeline is.*

---

- **23 chapters + 2 appendices** (~47,000 words, 166 pages)
- **Two running examples**: H₂ (the teacher) and H₂O (the real test)
- **Computed results**: H₂ dissociation curve, H₂O bond angle scan (99° in STO-3G)
- **FockMap library**: 6 encodings, Z₂ tapering, Trotterization, circuit export

## At a Glance

| | |
|---|---|
| **Words** | ~47,000 |
| **Pages** | 166 |
| **Chapters** | 23 + 2 appendices |
| **Mermaid diagrams** | 25 |
| **Companion scripts** | 10 (F# + Python) |
| **Lab exercises** | 10 interactive `.fsx` scripts |
| **Encodings covered** | 6 (JW, BK, Parity, Binary Tree, Ternary Tree, Vlasov) |
| **Molecules** | H₂ (4 qubits) and H₂O (12–14 qubits) |

### Chapter Breakdown

| Stage | Chapters | Words |
|---|---|---|
| Foreword | — | 830 |
| The Molecule (Ch 1–3) | 3 | 8,210 |
| The Machine (Ch 4) | 1 | 2,380 |
| Encoding (Ch 5–9) | 5 | 10,520 |
| Tapering (Ch 10–13) | 4 | 6,140 |
| Circuits (Ch 14–17) | 4 | 5,450 |
| The Pipeline (Ch 18–21) | 4 | 8,040 |
| Horizons (Ch 22–23) | 2 | 2,980 |
| Appendices | 2 | 990 |

## Quick Start

1. Click **Open in Codespaces** above (or clone and open in VS Code with Dev Containers)
2. Wait for the environment to set up (~2 minutes)
3. Run your first lab:
   ```bash
   dotnet fsi labs/01-first-encoding.fsx
   ```
4. Build the PDF:
   ```bash
   make
   ```

## Repository Structure

```
manuscript/     23 chapters + 2 appendices (Markdown source)
labs/           10 interactive F# scripts — run with dotnet fsi
code/           Python data generation + F# companion scripts
.devcontainer/  One-click Codespaces/Dev Container setup
Makefile        Build PDF, sample, word counts, Leanpub deploy
```

## Make Targets

| Target | Description |
|---|---|
| `make` | Build full manuscript PDF |
| `make sample` | Build sample PDF (foreword + Ch 1–6) |
| `make word-count` | Per-chapter and total word counts |
| `make data` | Regenerate H₂/H₂O data (requires PySCF) |
| `make clean` | Remove generated files |
| `make preview` | Trigger Leanpub preview |
| `make leanpub` | Trigger Leanpub publish |

## The FockMap Library

The code in this book uses the [FockMap](https://github.com/johnazariah/encodings)
library, published on NuGet. Labs reference it via:

```fsharp
#r "nuget: FockMap"
open Encodings
```

## License

Manuscript text: All rights reserved.
Code (labs, scripts): MIT License.
