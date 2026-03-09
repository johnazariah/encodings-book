# From Molecules to Quantum Circuits

*A Computational Guide to Fermion-to-Qubit Encodings*

**John S Azariah**

Centre for Quantum Software and Information
University of Technology Sydney

---


---

## Preface

Every chemistry textbook tells you that water's bond angle is 104.5°
and waves at VSEPR theory. Every quantum computing textbook tells you
that fermions need to be encoded as qubits and shows you the
Jordan-Wigner transform. Very few resources connect these two worlds
with enough detail that you could actually *compute* the bond angle
from the quantum simulation, or understand *why* the encoding choice
matters for the circuit you'll run on a quantum computer.

This book fills that gap.

We walk through the complete pipeline — from molecular geometry to
quantum circuit — with two running examples: the hydrogen molecule
(H₂), because it is the simplest system that exhibits all the
essential structure, and the water molecule (H₂O), because it is
the most important molecule on Earth and rich enough to make the
engineering trade-offs tangible.

Every formula in this book has a corresponding executable computation
in the FockMap library, an open-source F# framework for symbolic
Fock-space operator algebra. Every sign, every coefficient, every
intermediate Pauli string is computed explicitly. Nothing is left as
an exercise for the reader — though there are plenty of exercises at
the end of each chapter for readers who want to deepen their
understanding.

The book follows a deliberate pedagogical ordering: chemistry and
physics first, mathematical formalism second, executable code third.
We believe the question "why does this matter?" should always be
answered before the question "how does this work?" — and both should
be answered before "how do I compute it?"

### Who This Book Is For

- **Graduate students** starting in quantum chemistry simulation who
  need to understand the encoding layer between chemistry and circuits
- **Physicists** crossing into quantum computing who know Hamiltonians
  but not encodings
- **Software engineers** building quantum simulation pipelines who
  need the physics explained carefully
- **Lecturers** building a course module on quantum simulation who
  want homework exercises with verifiable answers

### What You Need to Know

We assume familiarity with linear algebra (vectors, matrices,
eigenvalues) and introductory quantum mechanics (wavefunctions,
bra-ket notation, the hydrogen atom). We do *not* assume prior
knowledge of:

- Second quantization or Fock space
- Pauli algebra or qubit representations
- Fermion-to-qubit encodings
- F# or functional programming

All of these are developed from scratch within the book.

### How to Read This Book

The 21 chapters are organized into six stages, following the quantum
simulation pipeline:

1. **The Electronic Structure Problem** (Chapters 1–3) — from
   molecules to integrals
2. **Encoding** (Chapters 4–7) — from fermions to qubits
3. **Tapering** (Chapters 8–11) — removing redundant qubits
4. **Trotterization** (Chapters 12–15) — from Hamiltonian to gates
5. **Circuit Output** (Chapters 16–18) — generating executable code
6. **Putting It Together** (Chapters 19–21) — the complete pipeline
   and what comes next

You can read straight through (recommended for first reading), or
jump to a specific stage if you already know the earlier material.
Each chapter begins with "In This Chapter" learning objectives and
ends with "Key Takeaways" and exercises.

### The Companion Software

All computations in this book are reproducible using the FockMap
library:

- **Source code:** https://github.com/johnazariah/encodings
- **NuGet package:** `dotnet add package FockMap`
- **Web documentation:** https://johnazariah.github.io/encodings/

The library is open-source (MIT license), runs on Windows, macOS,
and Linux via .NET 10, and has zero runtime dependencies.

### Acknowledgements

This work grew out of research at the Centre for Quantum Software
and Information at the University of Technology Sydney. I am grateful
to the F# Software Foundation and the .NET open-source community for
the language and runtime ecosystem that made this library possible.

*Sydney, March 2026*
