# From Molecules to Quantum Circuits

*A Computational Guide to Fermion-to-Qubit Encodings*

**John S Azariah**
Centre for Quantum Software and Information, University of Technology Sydney

---

<a class="download-btn" href="https://github.com/johnazariah/encodings-book/releases/latest">📥 Download PDF</a>
<a class="download-btn" href="https://github.com/johnazariah/encodings">💻 FockMap Library</a>

---

## About This Book

This is a self-contained, 23-chapter computational tutorial covering the complete pipeline from molecular electronic structure to quantum circuit compilation for quantum simulation.

Starting from the one-body and two-body integrals of hydrogen (H₂), we construct the qubit Hamiltonian explicitly under six fermion-to-qubit encodings, verify spectral equivalence, reduce qubit count via Z₂ symmetry tapering, decompose the tapered Hamiltonian into Trotter circuits with explicit CNOT gate counts, and export the result to OpenQASM 3.0 and Q#.

Every formula has a corresponding executable computation in the companion [FockMap](https://github.com/johnazariah/encodings) library.

---

## Contents

### Part I — The Molecule
1. [The Electronic Structure Problem](01-electronic-structure)
2. [The Notation Minefield](02-notation)
3. [From Spatial to Spin-Orbital Integrals](03-spin-orbitals)

### Part II — The Machine
4. [The Quantum Computer's Vocabulary](04-qubits-gates-circuits)

### Part III — Encoding
5. [A Visual Guide to Encodings](05-visual-encodings)
6. [Building the Qubit Hamiltonian](06-building-hamiltonian)
7. [Six Encodings, One Interface](07-six-encodings)
8. [Building a Tree Encoding](08-building-vlasov)
9. [Checking Our Answer](09-verification)

### Part IV — Tapering
10. [Why Tapering?](10-why-tapering)
11. [Diagonal Z₂ Symmetries](11-diagonal-z2)
12. [General Clifford Tapering](12-clifford-tapering)
13. [Tapering Benchmarks](13-tapering-benchmarks)

### Part V — Circuits
14. [From Hamiltonian to Time Evolution](14-time-evolution)
15. [Trotterization in Practice](15-trotter-formulas)
16. [The CNOT Staircase](16-cnot-staircase)
17. [Cost Analysis Across Encodings](17-cost-analysis)

### Part VI — The Pipeline
18. [The Question We Can Now Answer](18-complete-pipeline)
19. [The Water Bond Angle](19-bond-angle)
20. [Algorithms — VQE and QPE](20-algorithms)
21. [Speaking the Hardware's Language](21-circuit-export)

### Part VII — Horizons
22. [Scaling — From H₂ to FeMo-co](22-scaling)
23. [What Comes Next](23-whats-next)

### Appendices
- [Library Cookbook Reference](appendix-cookbook)

---

## Citation

```bibtex
@misc{azariah2026molecules,
  author    = {Azariah, John S},
  title     = {From Molecules to Quantum Circuits: A Computational Guide to Fermion-to-Qubit Encodings},
  year      = {2026},
  doi       = {10.5281/zenodo.18917465},
  url       = {https://github.com/johnazariah/encodings-book}
}
```

## License

Code: [MIT](https://github.com/johnazariah/encodings-book/blob/main/LICENSE).
Manuscript text: © 2026 John S Azariah. All rights reserved.
