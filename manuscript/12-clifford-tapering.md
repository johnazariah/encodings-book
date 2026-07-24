# Chapter 12: General Clifford Tapering

_When no single qubit is diagonal, multi-qubit Z₂ symmetries may still exist. A Clifford rotation reveals them._

## In This Chapter

- **What you'll learn:** The symplectic representation of Pauli strings, how to find all Z₂ symmetry generators via GF(2) linear algebra, how to synthesize a Clifford circuit that makes them diagonal, and how FockMap's unified `taper` function combines everything.
- **Why this matters:** Many molecular Hamiltonians have Z₂ symmetries that diagonal-only tapering cannot exploit. Clifford tapering finds and uses them all.
- **Prerequisites:** Chapters 10–11 (diagonal tapering concepts and mechanics).

---

## The Limitation of Diagonal Tapering

Diagonal tapering requires a qubit where *every* Hamiltonian term is I or Z. But consider the 2-qubit Heisenberg model:

$$\hat{H} = J(X_0 X_1 + Y_0 Y_1 + Z_0 Z_1)$$

No single qubit is diagonal — both qubits have X, Y, and Z terms. Diagonal tapering finds nothing.

But the operator $Z_0 Z_1$ **commutes** with every term: $[Z_0 Z_1, X_0 X_1] = [Z_0 Z_1, Y_0 Y_1] = [Z_0 Z_1, Z_0 Z_1] = 0$. It is a valid Z₂ symmetry generator — it just involves *two* qubits instead of one.

If we could rotate $Z_0 Z_1$ onto a single-qubit $Z$ — say, $Z_0 I_1$ — then qubit 0 would become diagonally taperable. The Clifford rotation that achieves this is a simple CNOT.

---

## The Binary Pauli Representation

To find all Z₂ generators systematically, we need a way to represent Pauli strings that makes commutativity easy to check. The trick is to encode each Pauli operator as **two binary bits**:

| Pauli | X bit | Z bit | Think of it as... |
|:---:|:---:|:---:|:---|
| I | 0 | 0 | No flip, no phase |
| X | 1 | 0 | Bit-flip only |
| Y | 1 | 1 | Both flip and phase |
| Z | 0 | 1 | Phase-flip only |

An $n$-qubit Pauli string becomes a binary vector of length $2n$ — the X-bits followed by the Z-bits:

$$\sigma \;\leftrightarrow\; (\underbrace{x_0, x_1, \ldots, x_{n-1}}_{\text{X bits}} \mid \underbrace{z_0, z_1, \ldots, z_{n-1}}_{\text{Z bits}})$$

```fsharp
let sv = toSymplectic (PauliRegister("XYZ", Complex.One))
// sv.X = [| true; true; false |]   — X has x=1, Y has x=1, Z has x=0
// sv.Z = [| false; true; true |]   — X has z=0, Y has z=1, Z has z=1
```

> **On the name "symplectic":** The quantum computing literature calls this the *symplectic representation*, and the commutativity check below a *symplectic inner product*. The word "symplectic" comes from Greek for "intertwined" — it refers to the fact that commutativity depends on a *crosswise* pairing between the X-bits of one string and the Z-bits of the other (and vice versa). This crosswise structure is what mathematicians call a symplectic form. We'll use the name "binary Pauli representation" when the intuition matters and "symplectic" when referencing the literature.

### Why this representation?

The payoff: two Pauli strings commute if and only if their **crosswise dot product** is zero (mod 2):

$$\text{commute?} \quad \sum_{i=0}^{n-1} (a_{x_i} \cdot b_{z_i} + a_{z_i} \cdot b_{x_i}) \stackrel{?}{=} 0 \pmod{2}$$

Notice the crosswise structure: we pair the X-bits of $a$ with the Z-bits of $b$, and vice versa. If the sum is even, they commute. If odd, they anti-commute. This reduces commutativity checking to a **binary dot product** — something a computer can do in nanoseconds.

```fsharp
let a = toSymplectic (PauliRegister("XX", Complex.One))
let b = toSymplectic (PauliRegister("ZZ", Complex.One))
commutes a b  // true — XX and ZZ commute
```

---

## Finding All Symmetry Generators

A Pauli string $g$ is a Z₂ symmetry of $\hat{H}$ if it commutes with every term and squares to the identity. Since every phase-free Pauli string (a tensor product of $\{I, X, Y, Z\}$, which is how this book represents them) squares to $I$, the squaring condition is automatic.

The commutation condition $\text{crosswise dot product} = 0$ for all terms
$t_k$ is a system of binary linear equations. Its null space is the Pauli
**centralizer** of the Hamiltonian: every solution commutes with every
Hamiltonian term. An arbitrary basis of that null space need not commute
pairwise. Tapering therefore needs a second step: select an independent
mutually commuting (Abelian) subgroup and retain the Pauli phases
(Bravyi et al., 2017).

```fsharp
let centralizer = findCommutingGenerators hamiltonian
// Candidate Pauli strings that commute with every Hamiltonian term.

// A valid tapering implementation must then select independent candidates
// that also commute with one another and track their phases.
```

The null space computation uses Gaussian elimination with XOR instead of subtraction — the same row-reduction you learned in linear algebra, but in binary arithmetic. It runs in $O(L \cdot n^2)$ time, where $L$ is the number of Hamiltonian terms and $n$ is the number of qubits. In practice, this is dominated by the $O(n^4)$ cost of integral processing and is never the bottleneck. See Bravyi et al. (arXiv:1701.08213, §III) for the formal analysis.

---

## Clifford Rotation: Making Generators Diagonal

Once we have independent generators, we need a Clifford circuit $U$ such that:

$$U g_i U^\dagger = Z_{q_i} \quad \text{for each generator } g_i$$

This rotates each multi-qubit generator onto a single-qubit $Z$, making the system diagonally taperable.

> **Algorithm: Clifford Tapering**
>
> **Input:** A Pauli Hamiltonian $\hat{H} = \sum_k c_k P_k$ on $n$ qubits.
>
> **Output:** A reduced Hamiltonian on $n - m$ qubits, where $m$ is the number of independent Z₂ symmetries.
>
> 1. **Represent** each term $P_k$ as a $2n$-bit symplectic vector.
> 2. **Build** the $L \times 2n$ commutation check matrix (one row per term).
> 3. **Compute** its null space via binary Gaussian elimination to obtain the centralizer candidates.
> 4. **Select** an independent mutually commuting subgroup $g_1,\ldots,g_m$, including each generator's phase.
> 5. **Synthesize** a Clifford that maps the generators to distinct single-qubit $Z_{q_i}$ operators while preserving generators already reduced. Local H/S choices and every CNOT update must track the Pauli phase.
> 6. **Conjugate** every term $P_k$ by the collected Clifford gates.
> 7. **Fix** each target qubit $q_i$ to the eigenvalue implied by the physical sector and remove it.
>
> **Complexity:** Step 3 is $O(L \cdot n^2)$; step 5 is $O(Lm)$ where $L$ is the number of terms. Total is dominated by Hamiltonian construction, not tapering.

FockMap synthesizes this circuit using three elementary gates:

| Gate | Symbol | Effect on Pauli |
|:---|:---:|:---|
| Hadamard | $H_j$ | Swaps X↔Z on qubit $j$ |
| Phase gate | $S_j$ | Maps X→Y on qubit $j$ (Z unchanged) |
| CNOT | $\text{CNOT}_{c,t}$ | Propagates X from control to target; propagates Z from target to control |

The synthesis algorithm must choose target qubits and local Clifford gates
consistently across the whole commuting set. Mapping a Y support may introduce
a minus sign, and CNOT conjugation can also change Pauli phases. Dropping those
signs changes Hamiltonian coefficients and can corrupt the tapered spectrum.

```fsharp
let (gates, targets) = synthesizeTaperingClifford independentGens
// gates : CliffordGate list — the rotation circuit
// targets : int[] — which qubit each generator maps to
```

---

## Applying the Clifford to the Hamiltonian

The Clifford circuit is applied **symbolically** — no matrices, no state vectors. Each Pauli term is conjugated: $\sigma_\alpha \to U \sigma_\alpha U^\dagger$. This is a sequence of substitution rules applied to the symplectic vector:

```fsharp
let rotatedH = applyClifford gates hamiltonian
// Every term is now conjugated — generators have become single-qubit Zs
```

After rotation, the target qubits are diagonally taperable, and we apply the v1 diagonal tapering from Chapter 10.

---

## The Unified Pipeline

FockMap's `taper` function combines everything:

```fsharp
// Full Clifford tapering in an explicitly derived physical sector
let physicalOptions =
    { defaultTaperingOptions with Sector = [(0, 1); (1, -1)] }
let result = taper physicalOptions hamiltonian

// Diagonal-only exploration uses the same explicit sector discipline
let diagonalResult =
    taper { physicalOptions with Method = DiagonalOnly } h

// A removal cap changes cost, not the need to identify the sector
let cappedResult =
    taper { physicalOptions with MaxQubitsToRemove = Some 2 } h
```

`defaultTaperingOptions` is a configuration starting point, not evidence that
its default positive sector represents the target molecule.

The result includes everything you need:

```fsharp
result.OriginalQubitCount  // before tapering
result.TaperedQubitCount   // after tapering
result.RemovedQubits       // which qubits were removed
result.Generators          // the Z₂ generators found
result.CliffordGates       // the rotation circuit applied
result.TargetQubits        // which qubits the generators mapped to
result.Hamiltonian         // the tapered PauliRegisterSequence
```

---

## Worked Example: Heisenberg Model

$$\hat{H} = X_0X_1 + Y_0Y_1 + Z_0Z_1$$

This is a 2-qubit Hamiltonian where every qubit has X, Y, and Z terms. Diagonal tapering sees nothing at all. But there *is* a hidden symmetry — let's find it and use it.

**Step 1: Check for diagonal Z₂ qubits.**

Qubit 0 appears as: X, Y, Z across the three terms. Not diagonal (has X and Y). Qubit 1: same story. Diagonal tapering finds zero candidates.

**Step 2: Find general Z₂ generators.**

We need Pauli strings that commute with every term. Let's check $Z_0 Z_1$:

- Does $Z_0 Z_1$ commute with $X_0 X_1$? Use the crosswise dot product: X-bits of $ZZ$ are $(0,0)$, Z-bits are $(1,1)$. X-bits of $XX$ are $(1,1)$, Z-bits are $(0,0)$. Crosswise sum: $(0 \cdot 0 + 1 \cdot 1) + (0 \cdot 0 + 1 \cdot 1) = 2$, which is even → **commute** ✓
- Does $Z_0 Z_1$ commute with $Y_0 Y_1$? X-bits of $YY$ are $(1,1)$, Z-bits are $(1,1)$. Crosswise sum: $(0 \cdot 1 + 1 \cdot 1) + (0 \cdot 1 + 1 \cdot 1) = 2$ → **commute** ✓
- Does $Z_0 Z_1$ commute with itself? Always yes ✓

$Z_0 Z_1$ is a Z₂ generator. The Heisenberg Hamiltonian has additional
commuting structure, but we intentionally use this one generator to show a
single-qubit reduction and both of its sectors.

**Step 3: Clifford synthesis — rotate $Z_0 Z_1$ onto a single qubit.**

We want a circuit $U$ such that $U (Z_0 Z_1) U^\dagger = Z_0 I_1$ — the generator acts on only one qubit after rotation.

The key gate: **CNOT(1 → 0)** (qubit 1 is control, qubit 0 is target). Recall from Chapter 4 that CNOT propagates Z from target to control: $Z_\text{target} \to Z_\text{control} \cdot Z_\text{target}$, while $Z_\text{control}$ is unchanged. So:

$$\text{CNOT}_{1,0}:\quad Z_0 \to Z_0 Z_1, \quad Z_1 \to Z_1$$

Under this gate, $Z_0 Z_1 \to (Z_0 Z_1) \cdot Z_1 = Z_0 Z_1^2 = Z_0$ — we've isolated a single $Z_0$, which is good. But let's try the other direction for comparison: **CNOT(0 → 1)** (qubit 0 is control, qubit 1 is target):

$$\text{CNOT}_{0,1}:\quad Z_0 \to Z_0, \quad Z_1 \to Z_0 Z_1$$

Under this gate, $Z_0 Z_1 \to Z_0 \cdot (Z_0 Z_1) = Z_0^2 Z_1 = Z_1$ — we've isolated a single $Z_1$.

> **What just happened:** The CNOT "absorbed" the $Z_0$ factor by multiplying it with another $Z_0$, leaving only $Z_1$. This is the Clifford rotation — it's not a physical rotation in space, but an algebraic simplification achieved by conjugation with a gate.

**Step 4: Apply the CNOT to every Hamiltonian term.**

Under CNOT(0,1) conjugation, recall the full propagation rules: $Z_0 \to Z_0$, $Z_1 \to Z_0 Z_1$ (Z propagates from target to control), $X_0 \to X_0 X_1$ (X propagates from control to target), $X_1 \to X_1$. Applying these to each Heisenberg term:

| Original | Propagation | Result |
|:---:|:---|:---:|
| $X_0 X_1$ | $X_0 \to X_0 X_1$, $X_1 \to X_1$ | $(X_0 X_1)(X_1) = X_0 I_1 = X_0$ |
| $Y_0 Y_1$ | Conjugate the full Pauli product, including phase | $-X_0 Z_1$ |
| $Z_0 Z_1$ | $Z_0 \to Z_0$, $Z_1 \to Z_0 Z_1$ | $Z_0 (Z_0 Z_1) = Z_1$ |

After the CNOT rotation,

$$H'=X_0-X_0Z_1+Z_1.$$

Qubit 1 is now diagonal, but its sector still matters:

- $Z_1=+1$: $H'_+=I$, giving eigenvalues $\{1,1\}$.
- $Z_1=-1$: $H'_-=2X_0-I$, giving eigenvalues $\{-3,1\}$.

Together the sectors give $\{-3,1,1,1\}$, exactly the spectrum of
$XX+YY+ZZ$. The ground state is in the $-1$ sector. Replacing the result by
the one-qubit Hamiltonian $X_0$ would lose both the $-3$ eigenvalue and the
sector structure.

A verified one-generator reduction turns each two-dimensional symmetry sector
into a one-qubit problem. Both sectors are needed to recover the full spectrum.
Direct $4\times4$ matrix conjugation provides an independent check of every
sign in the symbolic derivation.

---

## Key Takeaways

- The **binary Pauli representation** (two bits per qubit) turns commutativity checking into a crosswise binary dot product — fast and exact.
- The null space gives the Pauli centralizer; tapering needs an independent mutually commuting subgroup with phases.
- **Clifford synthesis** rotates multi-qubit generators onto single-qubit Zs using H, S, and CNOT — no matrices needed.
- **The unified `taper` function** handles both diagonal and Clifford tapering with one API.
- Everything is symbolic and exact — no approximation, no eigensolvers, no numerical instability.

## Further Reading

- Bravyi, S. et al. "Tapering off qubits to simulate fermionic Hamiltonians." arXiv:1701.08213 (2017). The general Z₂ tapering framework.
- Aaronson, S. and Gottesman, D. "Improved simulation of stabilizer circuits." *Phys. Rev. A* 70, 052328 (2004). The tableau formalism underlying our Clifford implementation.

---

**Previous:** [Chapter 11 — Diagonal Z₂ Symmetries](11-diagonal-z2.html)

**Next:** [Chapter 13 — Tapering Benchmarks](13-tapering-benchmarks.html)
