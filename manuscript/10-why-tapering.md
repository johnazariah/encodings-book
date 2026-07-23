# Chapter 10: Why Tapering?

_The Hamiltonian is correct and verified. But it may be bigger than it needs to be. This chapter shows why encoded Hamiltonians often contain redundant qubits — and how to detect them._

## In This Chapter

- **What you'll learn:** How fermion-to-qubit encoding creates Z₂ symmetries that allow safe qubit removal, and why this is one of the highest-leverage optimizations in the entire pipeline.
- **Why this matters:** Removing even one qubit halves the Hilbert space. For near-term quantum hardware with limited qubit counts and high error rates, tapering can mean the difference between a feasible and an infeasible simulation.
- **Prerequisites:** Chapters 1–9 (you have a verified qubit Hamiltonian and understand Pauli strings).

---

## The Observation

Look again at the H₂ Hamiltonian from Chapter 6. Focus on the Pauli operators at each qubit position (in FockMap notation $ABCD$, the leftmost character is qubit 0):

| Term | String | q0 | q1 | q2 | q3 |
|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | $IIII$ | I | I | I | I |
| 2 | $IIIZ$ | I | I | I | **Z** |
| 3 | $IIZI$ | I | I | **Z** | I |
| 4 | $IZII$ | I | **Z** | I | I |
| 5 | $ZIII$ | **Z** | I | I | I |
| 6 | $IIZZ$ | I | I | **Z** | **Z** |
| 7 | $IZIZ$ | I | **Z** | I | **Z** |
| 8 | $IZZI$ | I | **Z** | **Z** | I |
| 9 | $ZIIZ$ | **Z** | I | I | **Z** |
| 10 | $ZIZI$ | **Z** | I | **Z** | I |
| 11 | $ZZII$ | **Z** | **Z** | I | I |
| 12 | $XXYY$ | **X** | **X** | **Y** | **Y** |
| 13 | $XYYX$ | **X** | **Y** | **Y** | **X** |
| 14 | $YXXY$ | **Y** | **X** | **X** | **Y** |
| 15 | $YYXX$ | **Y** | **Y** | **X** | **X** |

Every qubit has at least some terms with X or Y — no qubit is purely diagonal (I/Z only) across all 15 terms. So H₂ under JW has **no diagonal Z₂ symmetries** at first glance.

But this is specific to H₂'s structure and the JW encoding. Many molecular Hamiltonians — especially larger ones — *do* have qubits where every term is I or Z. And even when no single qubit is diagonal, there may be **multi-qubit** Z₂ symmetries (like $Z_0 Z_1$) that a Clifford rotation can exploit.

### Where diagonal symmetries come from

Particle-number conservation is the most common source. In many encodings, the total electron number operator $\hat{N} = \sum_j \hat{n}_j$ commutes with the Hamiltonian. Under JW, $\hat{N}$ maps to a sum of $Z$ operators. If certain linear combinations of these $Z$ operators commute with every Hamiltonian term, the corresponding qubits can be fixed and removed.

The Parity encoding makes this especially transparent: the last qubit often stores the total parity of the electron number, which is conserved — making it immediately taperable.

---

## What Tapering Gains You

The benefits are concrete and multiplicative:

```mermaid
flowchart TD
    BQ["n qubits"] --- BT["m Pauli terms"] --- BH["2ⁿ Hilbert space"]
    BT -->|"Taper<br/>(remove k qubits)"| AT
    AQ["n−k qubits"] --- AT["≤ m terms"] --- AH["2ⁿ⁻ᵏ Hilbert space"]
    style BQ fill:#e8ecf1,stroke:#6b7280
    style BT fill:#e8ecf1,stroke:#6b7280
    style BH fill:#e8ecf1,stroke:#6b7280
    style AQ fill:#d1fae5,stroke:#059669
    style AT fill:#d1fae5,stroke:#059669
    style AH fill:#d1fae5,stroke:#059669
```

| What shrinks | Factor | Example (12 → 9 qubits) |
|:---|:---:|:---|
| Qubit count | $-k$ | 3 fewer physical qubits needed |
| Hilbert space | $2^{-k}$ | $4096 \to 512$ (8× smaller) |
| Circuit width | $-k$ | 3 fewer wires in every gate layer |
| Pauli weight | often reduces | Shorter Z-chains after qubit removal |
| Term count | often reduces | Some terms collapse to identity |

And these savings **compound** with encoding choice. A ternary-tree encoding can combine logarithmic-weight operators with a smaller, correctly tapered register.

---

## The Two Levels of Tapering

FockMap implements two levels of increasing generality:

### Level 1: Diagonal Z₂

A qubit $j$ is *diagonally taperable* if every term in the Hamiltonian has only I or Z at position $j$ — never X or Y. This means qubit $j$'s value is determined by symmetry: it is always $+1$ or always $-1$ in the sector we care about, and it never gets flipped during the simulation. We can fix it to that value and remove it from the problem entirely.

**How you detect it:** Look at each qubit position across all Pauli terms. If you never see X or Y in that column, the qubit is taperable. It's a simple scan — FockMap checks this in one pass over the Hamiltonian.

### Level 2: General Clifford

Sometimes no *single* qubit is purely diagonal, but a *combination* of qubits is. For example, $Z_0 Z_1$ might commute with every Hamiltonian term even though $Z_0$ alone does not. This means the *product* of qubits 0 and 1 is conserved, even though neither one is individually.

In this case, we can apply a small rotation circuit (built from Hadamard, S, and CNOT gates — the same gates from Chapter 4) that rearranges the Hamiltonian so that the conserved combination ends up on a single qubit. After the rotation, that qubit is diagonally taperable, and we remove it just like in Level 1.

**How you detect it:** FockMap searches for Pauli operators that commute with every term in the Hamiltonian — these are the symmetry generators. The mathematical machinery behind this search involves binary linear algebra, which we'll develop step by step in Chapter 12.

The important thing at this stage is the *idea*: even when the symmetry isn't visible on a single qubit, it may be hiding in a combination of qubits, and a rotation can expose it.

We'll work through the diagonal case in Chapter 11, the Clifford generalization in Chapter 12, and concrete benchmarks in Chapter 13.

---

## When Tapering Is Exact

With a verified symmetry generator and the correct sector, tapering does not approximate or truncate the Hamiltonian in that sector.

When we remove a tapered qubit, we are removing a degree of freedom whose value was *already determined* — fixed by a conservation law (like particle number or spin parity) that the Hamiltonian respects. The qubit was never free to vary in the first place; it was constrained by symmetry to a single eigenvalue in the sector we're studying. Removing it simply acknowledges this constraint explicitly.

The eigenvalues of the tapered Hamiltonian are a *subset* of the eigenvalues of the original — specifically, the eigenvalues in the chosen symmetry sector. Eigenvalues in other sectors are intentionally excluded. Choosing the wrong sector therefore changes the physical problem rather than giving a harmless approximation.

This is why we taper *before* Trotterization, not after: the circuit should operate on the physically relevant Hilbert space from the start, not carry redundant qubits through every gate layer.

---

## Key Takeaways

- Encoded Hamiltonians often have Z₂ symmetries — qubits whose value is determined by conservation laws rather than dynamics.
- Removing a qubit with a verified generator and physical sector preserves that sector's spectrum exactly while reducing downstream Hamiltonian-simulation cost.
- Diagonal Z₂ symmetries (single-qubit Z generators) are the simplest case. General Z₂ symmetries require Clifford rotation.
- Encoding and tapering compound, but in a fixed order: encode the fermionic Hamiltonian, identify symmetries in that qubit representation, select the physical sector, taper, then compile.

## Common Mistakes

1. **Confusing diagonal candidates with all symmetries.** H₂/JW has no
   single-qubit diagonal candidate because the coupling terms put X/Y on every
   qubit, but multi-qubit symmetries may still exist.

2. **Confusing tapering with truncation.** Truncation (e.g., active-space reduction) discards orbitals and loses information. Tapering removes redundant *qubits* without losing any information — the eigenvalues are exactly preserved.

3. **Tapering after circuit compilation.** Taper before Trotterization, not after. The circuit should be built on the smaller Hamiltonian.

## Exercises

1. **Conservation laws.** For a molecule with $N$ electrons, what conservation laws might produce Z₂ symmetries? (Hint: total electron number, spin projection $S_z$, point-group symmetry.)

2. **Parity encoding candidate.** Under the Parity encoding, identify the qubit
   storing total electron-number parity for a particle-conserving Hamiltonian.
   Derive its eigenvalue for the target electron number before tapering it.

3. **Scaling impact.** A hypothetical 14-to-11 reduction shrinks the Hilbert
   space by what factor? Explain why the CNOT reduction cannot be inferred
   without the post-taper term and weight distribution.

## Further Reading

- Bravyi, S., Gambetta, J. M., Mezzacapo, A., and Temme, K. "Tapering off qubits to simulate fermionic Hamiltonians." arXiv:1701.08213 (2017). The foundational paper on qubit tapering.

---

**Previous:** [Chapter 9 — Checking Our Answer](09-verification.html)

**Next:** [Chapter 11 — Diagonal Z₂ Symmetries](11-diagonal-z2.html)
