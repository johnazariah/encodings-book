# Chapter 22: Scaling — From H₂ to FeMo-co

_H₂ was our teacher. H₂O was our first real test. Now we look at where the pipeline goes — and where it meets its limits._

## In This Chapter

- **What you'll learn:** How the pipeline's cost scales from small test systems through medium molecules to the grand challenge of quantum chemistry, and where encoding choice makes — or breaks — the simulation.
- **Why this matters:** The promise of quantum simulation is that it handles molecules classical computers cannot. This chapter asks: at what point does that promise become real, and what does the hardware need to deliver?
- **Prerequisites:** Chapters 17–19 (cost analysis, pipeline, bond angle scan).

---

## The Scaling Landscape

Several quantities grow with the number of spin-orbitals $n$, but they are not
interchangeable:

- a direct occupation encoding uses $n$ system qubits before tapering;
- the two-electron tensor has $O(n^4)$ index combinations before symmetry and
  sparsity are exploited;
- the fixed-particle-number determinant space has
  $\binom{n}{n_e}$ basis states; and
- encoded Pauli weight depends on the mapping and orbital ordering.

A quantum register avoids storing every determinant amplitude explicitly. The
difficulty moves into state preparation, Hamiltonian simulation, precision, and
measurement; compact representation alone is not an efficient ground-state
algorithm.

---

## The Reproducible Encoding-Level Result

The companion script measures the maximum creation-operator weight directly
from the pinned package:

| $n$ | JW weight | BK weight | Ternary weight | Standard staircase CNOTs (JW / BK / TT) |
|:---:|:---:|:---:|:---:|:---:|
| 4 | 4 | 3 | 2 | 6 / 4 / 2 |
| 8 | 8 | 4 | 3 | 14 / 6 / 4 |
| 16 | 16 | 5 | 4 | 30 / 8 / 6 |
| 32 | 32 | 6 | 5 | 62 / 10 / 8 |
| 64 | 64 | 7 | 6 | 126 / 12 / 10 |

Both tree families are $\Theta(\log n)$; ternary branching improves a
constant, not the asymptotic class. At $n=64$, the standard single-rotation
staircase comparison is $126/10=12.6$, not 16.

This is an **operator-level** result. A molecular Hamiltonian contains a
distribution of terms and weights, with cancellations, symmetries, tapering,
and compiler interactions. The table does not imply a 12.6-fold reduction for
an entire molecule.

---

## From Operator Weight to a Molecular Benchmark

A reproducible molecule-level comparison needs committed geometry, basis,
active space, electron/spin sector, orbital order, encoding version, Pauli
lists, tapering generators, product order, and logical-connectivity assumptions.
Those artifacts do not yet exist for the former H₂O, LiH, N₂, or FeMo-co
tables, so their projected CNOT and success-probability numbers are withheld.

Tapering must also be measured rather than assumed: each reduced spectrum must
match one identified untapered physical sector. A qubit reduction does not by
itself establish a term-count, coefficient-norm, or end-to-end depth reduction.

---

## FeMo-co: What the Active Space Tells Us

Reiher et al. use a representative active-space model with about 54 electrons
in 54 spatial orbitals (108 spin-orbitals). The corresponding determinant count
is roughly $\binom{108}{54}\approx2.5\times10^{31}$, which explains why
explicit Full CI is not an option.

That active-space size does **not** determine a quantum resource estimate.
Logical qubits, Hamiltonian representation, state preparation, precision,
simulation algorithm, error-correction code, and hardware assumptions all
enter. Until a versioned resource model is committed, this book does not quote
total T gates/CNOTs, logical qubits, physical qubits, or runtime for FeMo-co.

---

## Where Classical Methods Still Win

Classical methods remain formidable:

- Conventional Kohn-Sham DFT implementations are often dominated by a cubic
  diagonalization step, while linear-scaling variants exploit locality and
  sparsity under additional conditions (Bowler & Miyazaki, 2010).
- Canonical single-reference CCSD(T) has an $O(N^7)$ perturbative-triples
  step and is reliable primarily when a single-reference description is
  appropriate; local and reduced-scaling variants change the practical cost
  (Bartlett & Musial, 2007).
- DMRG is exceptionally effective for low-entanglement one-dimensional and
  quasi-one-dimensional structure, with efficiency degrading as entanglement
  grows (White, 1992; Schollwock, 2011).

Strongly correlated transition-metal complexes, open-shell systems, and
conical intersections are candidate quantum targets because standard
single-reference methods can struggle there. Advantage remains an
instance-specific comparison against the best classical workflow.

---

## Key Takeaways

- The reproduced **operator-level** census separates JW's linear maximum weight
  from BK/ternary logarithmic weight; it does not predict a full molecular cost.
- At $n=64$, the standard worst-case single-rotation comparison is 126 CNOTs
  for JW, 12 for BK, and 10 for the tested ternary mapping.
- Tapering benefits must be measured in an identified physical sector; qubit,
  term, coefficient-norm, and circuit reductions are distinct quantities.
- FeMo-co motivates large active spaces, but active-space size alone is not a
  logical/physical qubit or runtime estimate.
- Chemistry method, state preparation, algorithms, software validation, error
  correction, and hardware are all potential bottlenecks.

## Further Reading

- Reiher, M., Wiebe, N., Svore, K. M., Wecker, D., and Troyer, M. "Elucidating Reaction Mechanisms on Quantum Computers." *PNAS* 114, 7555 (2017). The landmark resource estimate for simulating FeMo-co on a fault-tolerant quantum computer.
- Lee, J., Berry, D. W., Gidney, C., Huggins, W. J., McClean, J. R., Wiebe, N., and Babbush, R. "Even More Efficient Quantum Computations of Chemistry Through Tensor Hypercontraction." *PRX Quantum* 2, 030305 (2021). Dramatically reduces the resource estimates for large-molecule simulation using tensor factorisation techniques.

---

**Previous:** [Chapter 21 — Speaking the Hardware's Language](21-circuit-export.html)

**Next:** [Chapter 23 — What Comes Next](23-whats-next.html)
