# Chapter 11: Diagonal Z₂ Symmetries

_The simplest form of tapering: find qubits that are always I or Z, fix their eigenvalue, and remove them._

## In This Chapter

- **What you'll learn:** The algorithm for detecting diagonal Z₂ symmetries, how sector choice works, and how fixing a sector modifies each Pauli term.
- **Why this matters:** Diagonal tapering is fast, easy to implement, and often removes 1–3 qubits from molecular Hamiltonians with zero information loss.
- **Prerequisites:** Chapter 10 (you understand why tapering is valuable and what Z₂ symmetries are).

---

## The Detection Algorithm

Given a Hamiltonian $\hat{H} = \sum_\alpha c_\alpha \sigma_\alpha$, we say qubit $j$ is **diagonal Z₂ symmetric** if:

$$\forall \alpha:\; \sigma_\alpha[j] \in \{I, Z\}$$

The algorithm is a single scan:

```mermaid
flowchart TD
    START["For each qubit j = 0, 1, ..., n-1"] --> SCAN["Inspect every Pauli term σ_α"]
    SCAN --> CHECK{"σ_α[j] ∈ {I, Z}?"}
    CHECK -->|"All terms: yes"| DIAG["Qubit j is diagonal Z₂ ✓"]
    CHECK -->|"Any term: X or Y"| SKIP["Qubit j is NOT diagonal ✗"]
    style DIAG fill:#d1fae5,stroke:#059669
    style SKIP fill:#fee2e2,stroke:#ef4444
```

**Complexity:** $O(nm)$, where $n$ is the qubit count and $m$ is the
number of stored terms. Wall-clock time depends on representation and
implementation; this book does not infer molecule timings from the asymptotic
scan alone.

### In FockMap

```fsharp
let symQubits = diagonalZ2SymmetryQubits hamiltonian
// Returns int[] of taperable qubit indices
```

For our toy example from Chapter 10:

```fsharp
let h =
    [| PauliRegister("ZIZI", Complex(0.8, 0.0))
       PauliRegister("ZZII", Complex(-0.4, 0.0))
       PauliRegister("IIZZ", Complex(0.3, 0.0))
       PauliRegister("IZIZ", Complex(0.2, 0.0)) |]
    |> PauliRegisterSequence

diagonalZ2SymmetryQubits h
// → [| 0; 1; 2; 3 |]  — all four qubits are diagonal!
```

Every term has only I or Z at every position. The Hamiltonian is diagonal in
the computational basis, so a basis state attains its minimum eigenvalue.
Degeneracy can still permit coherent eigenstates; “diagonal” is the precise
property used by the detector.

### A more realistic example

In practice, molecular Hamiltonians are *mixed*: some qubits are diagonal, others are not. Consider a Hamiltonian where qubits 0 and 2 are always I/Z, but qubits 1 and 3 have X and Y terms:

```fsharp
let hmixed =
    [| PauliRegister("ZIZI", Complex(0.5, 0.0))   // q0=Z, q1=I, q2=Z, q3=I
       PauliRegister("IXIX", Complex(-0.3, 0.0))   // q0=I, q1=X, q2=I, q3=X
       PauliRegister("ZIIZ", Complex(0.2, 0.0))    // q0=Z, q1=I, q2=I, q3=Z
       PauliRegister("IYIY", Complex(0.1, 0.0)) |] // q0=I, q1=Y, q2=I, q3=Y
    |> PauliRegisterSequence

diagonalZ2SymmetryQubits hmixed
// → [| 0; 2 |]
```

Qubit 0 has Z, I, Z, I across the four terms — all diagonal. Qubit 2 has Z, I, I, I — also all diagonal. But qubit 1 has I, X, I, Y — the X and Y disqualify it. Qubit 3 has I, X, Z, Y — also disqualified.

Only qubits 0 and 2 satisfy this single-qubit diagonal test. Qubits 1 and 3
appear with X/Y support and cannot be removed by this rule. That statement is
about the detected symmetry form, not a classical-versus-quantum partition.

> **The intuition:** in a selected symmetry sector, a tapered generator
> eigenvalue is fixed. Removing its target qubit represents that constraint;
> it is not a partition into “classical” discarded qubits and “quantum” kept
> qubits.

---

## Sectors: Choosing Eigenvalues

For each diagonal Z₂ qubit, we **choose** whether to fix its $Z$ eigenvalue to $+1$ or $-1$. This choice is called a **sector**.

A sector is a list of (qubit, eigenvalue) pairs:

```fsharp
let sector = [ (1, +1); (3, -1) ]
// Fix qubit 1 to eigenvalue +1, qubit 3 to eigenvalue -1
```

**Physical interpretation:** Different sectors correspond to different quantum numbers. If the diagonal qubits encode particle-number parity or spin projection, then sector $+1$ vs $-1$ selects different electron counts or spin states.

For example, if qubit $j$ represents the parity of the total electron number (even vs odd), then:
- Sector $+1$ → even number of electrons
- Sector $-1$ → odd number of electrons

If you know your molecule has 2 electrons (even), you choose sector $+1$ for that qubit. This projects the Hamiltonian onto the physically relevant subspace.

For $k$ diagonal qubits, there are $2^k$ possible sectors. Each gives a valid tapered Hamiltonian with the eigenvalues of that sector, but sectors can represent different particle numbers, spins, or point-group quantum numbers. A molecular calculation must select the sector matching the intended physical system. Sweeping all sectors is useful for reconstructing or debugging the full spectrum; taking the lowest value across them may instead select an ion or a different spin state.

---

## How Sector Fixing Modifies Terms

The modification rule is simple:

For each term $c_\alpha \sigma_\alpha$ and each tapered qubit $(j, \lambda)$:

1. If $\sigma_\alpha[j] = I$: no change to the coefficient
2. If $\sigma_\alpha[j] = Z$: multiply the coefficient by $\lambda$ (the eigenvalue: $+1$ or $-1$)
3. Remove position $j$ from the Pauli string

### Worked Example

Start with:

$$\hat{H} = 0.8\,\text{ZIZI} - 0.4\,\text{ZZII} + 0.3\,\text{IIZZ} + 0.2\,\text{IZIZ}$$

Fix sector $[(1, +1),\; (3, -1)]$:

| Original term | Qubit 1 | Factor | Qubit 3 | Factor | New coeff | Remaining Pauli |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| $0.8\,\text{ZIZI}$ | I | $\times 1$ | I | $\times 1$ | $0.8$ | ZZ |
| $-0.4\,\text{ZZII}$ | Z | $\times (+1)$ | I | $\times 1$ | $-0.4$ | ZI |
| $0.3\,\text{IIZZ}$ | I | $\times 1$ | Z | $\times (-1)$ | $-0.3$ | IZ |
| $0.2\,\text{IZIZ}$ | Z | $\times (+1)$ | Z | $\times (-1)$ | $-0.2$ | II |

**Result:** $\hat{H}' = 0.8\,\text{ZZ} - 0.4\,\text{ZI} - 0.3\,\text{IZ} - 0.2\,\text{II}$

Four qubits → two qubits. The eigenvalues of $\hat{H}'$ are exactly the eigenvalues of $\hat{H}$ in the $(+1, -1)$ sector.

### In FockMap

```fsharp
let result = taperDiagonalZ2 [ (1, 1); (3, -1) ] h

printfn "%d → %d qubits" result.OriginalQubitCount result.TaperedQubitCount
// This diagonal toy: 4 → 2 after explicitly fixing q1=+1 and q3=-1

printfn "Removed: %A" result.RemovedQubits
// [| 1; 3 |]
```

---

## The Convenience Helper

For quick exploration, taper all detected qubits in the $+1$ sector:

```fsharp
let auto = taperAllDiagonalZ2WithPositiveSector hamiltonian

// Equivalent to:
// let qs = diagonalZ2SymmetryQubits hamiltonian
// let sector = qs |> Array.map (fun q -> (q, 1)) |> Array.toList
// let auto = taperDiagonalZ2 sector hamiltonian
```

This is fine for API exploration but not for a molecular result: the $+1$ sector may not have the required electron number or spin. For a physical calculation, derive each generator eigenvalue from known quantum numbers and verify the tapered spectrum against that untapered sector.

---

## Validation

FockMap validates all inputs:

```fsharp
// Non-diagonal qubit → error
taperDiagonalZ2 [(0, 1)] (prs [("XI", Complex.One)])
// → ArgumentException: "Qubit 0 is not a diagonal Z2 symmetry"

// Invalid eigenvalue → error
taperDiagonalZ2 [(0, 0)] hamiltonian
// → ArgumentException: "Sector eigenvalues must be +1 or -1"
```

These guards prevent the two most common tapering bugs: applying tapering to a qubit that has X or Y terms (which would silently drop those terms), and using an eigenvalue other than $\pm 1$ (which would corrupt the coefficients).

---

## Key Takeaways

- Detection is a single scan: $O(n \times m)$.
- Sector choice selects which quantum-number subspace to project into.
- Each term's coefficient is multiplied by $\pm 1$ per tapered qubit (depending on I vs Z), then the qubit position is removed.
- A sector sweep is a spectrum diagnostic, not a substitute for identifying the molecule's particle-number and spin sector.
- FockMap validates inputs to prevent silent errors.

## Sector Selection: A Practical Workflow

Choosing the correct sector is a common source of confusion for first-time practitioners. Here is a concrete decision procedure:

1. **If you know the conserved quantities** (e.g., particle number parity, spin projection), compute the expected eigenvalue for each generator and set the sector accordingly. For molecular ground states, the particle number is fixed, so parity-related generators should be set to match the electron count.

2. **If the generator-to-quantum-number map is unclear**, derive it before reporting a molecular energy. A sweep over all $2^k$ sectors can show how the full spectrum decomposes and can expose a sign mistake, but its global minimum need not belong to the intended molecule.

3. **For production workflows**, compare the tapered spectrum with the matching untapered sector (if computable) or with a reference labelled by the same electron number and spin. A sector mismatch can be either higher or lower than the desired molecular energy.

## Common Mistakes

1. **Assuming the $+1$ sector is physical.** Generator signs must be derived from the target electron number, spin, and other conserved quantum numbers.

2. **Tapering a non-diagonal qubit.** FockMap catches this, but if you're implementing by hand, silently dropping X/Y terms is the most dangerous bug.

3. **Forgetting to combine like terms after tapering.** Removing qubits can make previously distinct Pauli strings identical. The terms must be re-accumulated.

## Further Reading

- Bravyi, S., Gambetta, J. M., Mezzacapo, A., and Temme, K. "Tapering off qubits to simulate fermionic Hamiltonians." arXiv:1701.08213 (2017). The foundational paper on exploiting Z₂ symmetries to reduce qubit count.

---

**Previous:** [Chapter 10 — Why Tapering?](10-why-tapering.html)

**Next:** [Chapter 12 — General Clifford Tapering](12-clifford-tapering.html)
