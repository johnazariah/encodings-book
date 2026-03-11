# From Molecules to Quantum Circuits

*A Computational Guide to Fermion-to-Qubit Encodings*

**John S Azariah**

Centre for Quantum Software and Information
University of Technology Sydney

---

*For my parents, who believed before I did.*

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

The 23 chapters are organized into seven stages, following the quantum
simulation pipeline:

1. **The Molecule** (Chapters 1–3) — from molecules to integrals
2. **The Machine** (Chapter 4) — qubits, gates, and circuits
3. **Encoding** (Chapters 5–9) — from fermions to qubits
4. **Tapering** (Chapters 10–13) — removing redundant qubits
5. **Circuits** (Chapters 14–17) — Trotterization and cost analysis
6. **The Pipeline** (Chapters 18–21) — complete pipeline, bond angle, algorithms, export
7. **Horizons** (Chapters 22–23) — scaling and what comes next

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
and Information at the University of Technology Sydney.

I am grateful to Dr Chris Ferrie for his patient guidance of my
exploration, to Dr Guang Hao Low for sparking the journey, and to
Dr Stephen Jordan and Dr Helmut Katzgraber for their continued
encouragement.

And to my family, for bearing all my burdens.

*Sydney, March 2026*

---

### About the Author

John S Azariah is a software engineer, language designer, and quantum
computing researcher whose work sits at the intersection of chemistry,
computation, and executable mathematics. He earned a BA in Computer
Science and Chemistry from Dartmouth College, a combination that now
finds a natural expression in quantum simulation.

Over more than three decades in professional software development, he
has worked through successive technology shifts including desktop
software, the internet, cloud computing, quantum programming, and AI.
His career has included roles on Microsoft Excel and Microsoft Project,
early SharePoint prototypes, Visual J#.NET, large-scale Azure systems,
and principal engineering work across Azure Batch, Azure Kubernetes
Service, and AI systems in APAC. He was also compiler lead and one of
the language designers behind Microsoft Q#.

Alongside industry work, he has maintained a longstanding interest in
functional programming, scientific computing, and software craft. He is
currently a Principal AI SME at Microsoft and is pursuing a PhD in
Quantum Computing at the University of Technology Sydney under
Dr. Chris Ferrie.
