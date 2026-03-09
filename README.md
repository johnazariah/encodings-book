# From Molecules to Quantum Circuits

*A Practical Guide to Fermion-to-Qubit Encodings*

**John S Azariah**

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/johnazariah/encodings-book)

---

## What This Is

A self-contained guide covering the complete pipeline from molecular electronic
structure to quantum circuit compilation — with executable code at every step.

- **22 chapters + 2 appendices** (~44,000 words, ~160 pages)
- **Two running examples**: H₂ (the teacher) and H₂O (the real test)
- **Computed results**: H₂ dissociation curve, H₂O bond angle scan (99° in STO-3G)
- **FockMap library**: 5 encodings, Z₂ tapering, Trotterization, circuit export

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
manuscript/     22 chapters + appendices (Markdown source)
labs/           Interactive F# scripts — run with dotnet fsi
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
