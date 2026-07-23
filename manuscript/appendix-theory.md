# Appendix B: Mathematical Background

_Formal derivations and proofs for readers who want the full treatment._

This appendix collects the mathematical background used by the main text and
makes the book's operator, encoding, and circuit conventions explicit.

---

## B.1 Why Encodings? (Theory Ch. 1)

Fermions obey anti-commutation relations: $\{a_i, a_j^\dagger\} = \delta_{ij}$.
Qubits obey commutation relations: $[\sigma_i, \sigma_j] = 0$ for $i \neq j$.
An encoding is a mapping from the first algebra to the second that preserves
the anti-commutation structure.

**Key result:** Any valid encoding must map each fermionic mode to at least one
qubit, so $n$ modes require $\geq n$ qubits. The challenge is to minimize the
Pauli weight (number of non-identity Pauli operators) per encoded operator.

## B.2 Second Quantization (Theory Ch. 2)

The occupation-number representation replaces $N$-particle wavefunctions with
Fock states $\lvert n_0, n_1, \ldots \rangle$ and ladder operators $a_p^\dagger, a_p$.
The canonical anti-commutation relations (CAR):

$$\{a_p, a_q^\dagger\} = \delta_{pq}, \quad \{a_p, a_q\} = \{a_p^\dagger, a_q^\dagger\} = 0$$

encode the Pauli exclusion principle and fermionic exchange symmetry.

The electronic Hamiltonian in second quantization:

$$\hat{H} = \sum_{pq} h_{pq}\, a_p^\dagger a_q + \frac{1}{2}\sum_{pqrs} \langle pq \mid rs\rangle\, a_p^\dagger a_q^\dagger a_s a_r$$

## B.3 Pauli Algebra (Theory Ch. 3)

The single-qubit Pauli matrices $\{I, X, Y, Z\}$ form a basis for all $2 \times 2$
Hermitian operators. Their multiplication table is closed with phases in $\{\pm 1, \pm i\}$:

$$XY = iZ, \quad YZ = iX, \quad ZX = iY$$

Any two distinct non-identity Paulis anti-commute: $XY = -YX$, etc.

An $n$-qubit Pauli string $P = P_0 \otimes P_1 \otimes \cdots \otimes P_{n-1}$ is a
tensor product of single-qubit Paulis. The $4^n$ phase-free strings form an
operator basis but are not closed under multiplication: for example, $XY=iZ$.
Including phases (commonly $\{\pm1,\pm i\}$) gives the Pauli group.

> **Index and basis order:** This book writes strings as
> $P_0P_1\ldots P_{n-1}$ and occupation labels as
> $\lvert n_0n_1\ldots n_{n-1}\rangle$, with index 0 leftmost. The occupation
> integer is $\sum_j n_j2^j$, so its conventional binary printout has mode 0
> on the right. Dense matrix builders reverse the displayed signature and form
> $P_{n-1}\otimes\cdots\otimes P_0$, making the matrix row equal to the
> occupation integer. Qiskit-style Pauli labels also put qubit 0 on the right,
> so that string boundary requires the same explicit reversal.

## B.4 Jordan–Wigner Transform (Theory Ch. 4)

The Jordan–Wigner encoding maps:

$$a_j^\dagger \to \frac{1}{2}(X_j - iY_j) \bigotimes_{k < j} Z_k$$

> **Convention:** Under the convention $|1\rangle$ = occupied, $|0\rangle$ = unoccupied, the creation operator maps $|0\rangle \to |1\rangle$ via $\frac{1}{2}(X - iY) = |1\rangle\langle 0|$. Some references use $\frac{1}{2}(X + iY)$, corresponding to the opposite occupation convention.

The Z-chain $\bigotimes_{k < j} Z_k$ tracks the parity of occupied orbitals below $j$,
enforcing the fermionic sign convention. The chain length grows linearly with $j$,
giving worst-case Pauli weight $O(n)$.

## B.5 Beyond Jordan–Wigner (Theory Ch. 5)

**Bravyi–Kitaev:** The separate canonical implementation uses corrected
Fenwick formulas in terms of $j+1$ to pre-compute partial parities, reducing
operator weight to $\Theta(\log n)$. It is not produced by the generic
tree-to-index-set constructor.

**Parity encoding:** Swaps the roles of occupation and parity information. A
qubit storing a conserved total parity can provide a tapering generator once
the physical parity sector is identified.

**Path-based encodings:** The implemented API accepts labelled trees with at
most three children per node. A validated candidate yields encoded Majorana
operators whose weights follow root-to-leaf depths. Balanced binary and ternary trees are both
$\Theta(\log n)$; ternary branching improves the depth constant and finite-size
weight bound.

**The index-set framework (Seeley–Richard–Love 2012):** Unifies JW, BK, and Parity
as instances of a common abstraction defined by three set-valued functions:
Update($j$), Parity($j$), Occupation($j$). FockMap's **generic
tree-to-index-set** helper is narrower: exhaustive tests support rooted stars
only; chain, balanced-binary, and Fenwick tree inputs fail CAR there.

## B.6 Bosonic Operators (Theory Ch. 6)

Bosonic modes obey canonical commutation relations (CCR):
$[b_i, b_j^\dagger] = \delta_{ij}$.

Bosonic Fock spaces are infinite-dimensional and must be truncated to $d$ levels
for qubit encoding. Three truncation encodings:

- **Unary:** $d$ qubits, weight ≤ 2
- **Standard binary:** $\lceil\log_2 d\rceil$ qubits
- **Gray code:** $\lceil\log_2 d\rceil$ qubits, reduced average weight

## B.7 Mixed Systems (Theory Ch. 7)

For Hamiltonians with both fermionic and bosonic modes:
- Separate operators by sector (fermionic vs bosonic)
- Cross-sector swaps are free (no sign change: $[a_f, b^\dagger_b] = 0$)
- Normal-order each sector independently using the appropriate algebra
- Encode fermionic sector with JW/BK/etc.; encode bosonic sector with
  unary/binary/Gray
