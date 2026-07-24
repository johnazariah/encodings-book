# From Molecules to Quantum Circuits

*A Computational Guide to Fermion-to-Qubit Encodings*

**John S Azariah**
Centre for Quantum Software and Information, University of Technology Sydney

[![DOI](https://zenodo.org/badge/1176397444.svg)](https://doi.org/10.5281/zenodo.18917465)
[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/johnazariah/molecules-to-circuits)

**[📖 Read Online](https://johnazariah.github.io/molecules-to-circuits)** · **[📥 Download PDF](https://github.com/johnazariah/molecules-to-circuits/releases/latest)**

---

## Short Blurb

> Every chemistry textbook tells you that water's bond angle is 104.5°. Every quantum computing textbook tells you that fermions must be encoded as qubits. This book connects the two by making the translation layer explicit: molecular data become encoded Hamiltonians, symmetry reductions, product-formula circuits, and portable circuit descriptions under six encoding schemes.

---

## Book Description

**From Molecules to Quantum Circuits** is a self-contained computational guide
to the translation layer from molecular electronic structure to logical quantum
circuits.

Core transformations are paired with executable FockMap companions, while independent PySCF scripts generate the classical chemistry references used to check them. Signs, coefficients, and intermediate Pauli strings are exposed rather than hidden behind a black-box workflow.

The guide covers six fermion-to-qubit encodings (Jordan-Wigner, Bravyi-Kitaev, Parity, balanced binary tree, balanced ternary tree, Vlasov tree), qubit tapering via diagonal and Clifford Z₂ symmetries, Trotter decomposition with CNOT cost analysis, and circuit output in OpenQASM and Q#.

Two running examples separate construction from reference chemistry: H₂ is
derived and checked from generated integrals to a JW matrix and circuit, while
H₂O supplies a PySCF FCI angular scan at fixed experimental O–H length.

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
> *Starting from generated H₂ integrals, we build the 15-term JW Hamiltonian,
> verify its matrix, labelled states, and spectrum against an independent
> reference, and use that result as the acceptance target for six encoding
> interfaces. We develop physical-sector tapering requirements, product-formula
> circuits, logical CNOT accounting, and circuit export without treating any
> one of those layers as a ground-state algorithm.*
>
> *For water, we trace a PySCF FCI angular cut at fixed O–H length and state exactly which parts of the quantum circuit pipeline remain separate.*
>
> *The core transformations are executable. Intermediate results are inspectable. FockMap constructs encoded Hamiltonians and logical circuits; state preparation and energy estimation remain separate algorithmic tasks.*
>
> *The translation layer is necessary. It is not the whole computation.*

---

- **23 chapters + 2 appendices** (~45,960 words)
- **Two running examples**: H₂ (the teacher) and H₂O (the real test)
- **Computed results**: H₂ dissociation curve; fixed-bond H₂O angular scans (99° sampled minimum in STO-3G)
- **FockMap library**: 6 encodings, Z₂ tapering, Trotterization, circuit export

## At a Glance

| | |
|---|---|
| **Words** | ~45,960 |
| **Pages** | 175 |
| **Chapters** | 23 + 2 appendices |
| **Mermaid diagrams** | 25 |
| **Companion scripts** | 10 (F# + Python) |
| **Lab exercises** | 10 interactive `.fsx` scripts |
| **Encodings covered** | 6 (JW, BK, Parity, Binary Tree, Ternary Tree, Vlasov) |
| **Molecules** | H₂ (4 qubits) and H₂O (12–14 qubits) |

### Chapter Breakdown

| Stage | Chapters | Words |
|---|---|---|
| Foreword | — | 815 |
| The Molecule (Ch 1–3) | 3 | 8,291 |
| The Machine (Ch 4) | 1 | 2,208 |
| Encoding (Ch 5–9) | 5 | 10,943 |
| Tapering (Ch 10–13) | 4 | 6,420 |
| Circuits (Ch 14–17) | 4 | 5,103 |
| The Pipeline (Ch 18–21) | 4 | 7,903 |
| Horizons (Ch 22–23) | 2 | 2,515 |
| Appendices | 2 | 1,249 |
| References | — | 514 |

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
docs/           GitHub Pages site (symlinks to manuscript/)
labs/           10 interactive F# scripts — run with dotnet fsi
code/           Python data generation + F# companion scripts
.devcontainer/  One-click Codespaces/Dev Container setup
Makefile        Build PDF, sample, word counts
```

## Make Targets

| Target | Description |
|---|---|
| `make` | Build full manuscript PDF |
| `make sample` | Build sample PDF (selected chapters) |
| `make epub` | Build the EPUB release artifact |
| `make word-count` | Per-chapter and total word counts |
| `make data` | Regenerate H₂/H₂O data (requires PySCF) |
| `make verify-data` | Independently verify H₂ integrals, JW coefficients, and sector spectra |
| `make pipeline-check` | Verify Chapter 18 writes only ignored derived output and leaves canonical data unchanged |
| `make clean` | Remove generated files |

## The FockMap Library

The code in this book uses the [FockMap](https://github.com/johnazariah/encodings)
library, published on NuGet. Labs reference it via:

```fsharp
#r "nuget: FockMap, 0.9.0"
open Encodings
open Encodings.JordanWigner
```

Release provenance: FockMap `0.9.0` was published from repository commit
`96320a56786393269fd681c67c66df88058a8b8f`; the public NuGet
`Encodings.dll` has SHA-256
`0ba8ae967ea65d4945a336c1217f8939e41feb63b6f04af617b66537717d25c3`.

FockMap feature modules are explicit rather than `AutoOpen`; Hamiltonian,
tapering, Trotter, circuit-export, and other examples open their corresponding
`Encodings.*` modules.

## License

Manuscript and rendered book: © 2026 John S Azariah, all rights reserved
([MANUSCRIPT-RIGHTS](MANUSCRIPT-RIGHTS)).

Companion code and build automation: MIT License
([LICENSE-CODE](LICENSE-CODE)).
