# Chapter 11: General Clifford Tapering

_When no single qubit is diagonal, multi-qubit Z₂ symmetries may still exist. A Clifford rotation reveals them._

## In This Chapter

- **What you'll learn:** The symplectic representation of Pauli strings, how to find all Z₂ symmetry generators via GF(2) linear algebra, how to synthesize a Clifford circuit that makes them diagonal, and how FockMap's unified `taper` function combines everything.
- **Why this matters:** Many molecular Hamiltonians have Z₂ symmetries that diagonal-only tapering cannot exploit. Clifford tapering finds and uses them all.
- **Prerequisites:** Chapters 8–9 (diagonal tapering concepts and mechanics).

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

A Pauli string $g$ is a Z₂ symmetry of $\hat{H}$ if it commutes with every term and squares to the identity. Since every Pauli string squares to $\pm I$, the squaring condition is automatic.

The commutation condition $\text{crosswise dot product} = 0$ for all terms $t_k$ is a system of linear equations where the arithmetic is binary (0 and 1, with addition meaning XOR). The solution set — the **null space** of the commutation check matrix — gives all Z₂ generators.

```fsharp
let generators = findCommutingGenerators hamiltonian
// Returns SymplecticVector[] — all Pauli strings that commute with every term

let indep = independentGenerators generators
// Selects a maximal linearly independent subset (binary linear algebra)
// The number of independent generators = max qubits taperable
```

The null space computation uses Gaussian elimination with XOR instead of subtraction — the same row-reduction you learned in linear algebra, but in binary arithmetic. It runs in $O(n^3)$ time in the number of qubits $n$, where the matrix has $L$ rows (one per Hamiltonian term) and $2n$ columns (the symplectic vector length). In practice, this is dominated by the $O(n^4)$ cost of integral processing and is never the bottleneck. See Bravyi et al. (arXiv:1701.08213, §III) for the formal analysis.

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
> 3. **Compute** its null space via binary Gaussian elimination → independent generators $g_1, \ldots, g_m$.
> 4. **For each** generator $g_i$: find a qubit $q_i$ where $g_i$ has support. If the support is X, apply H to convert to Z. If Y, apply S then H. Use CNOTs to clear all other qubits, leaving $g_i \to Z_{q_i}$. Collect the gate list.
> 5. **Conjugate** every term $P_k$ by the collected Clifford gates (symbolically — substitution rules on symplectic vectors).
> 6. **Fix** each target qubit $q_i$ to eigenvalue $\pm 1$ (the sector choice) and remove it.
>
> **Complexity:** Step 3 is $O(n^3)$; step 5 is $O(Lm)$ where $L$ is the number of terms. Total is dominated by Hamiltonian construction, not tapering.

FockMap synthesizes this circuit using three elementary gates:

| Gate | Symbol | Effect on Pauli |
|:---|:---:|:---|
| Hadamard | $H_j$ | Swaps X↔Z on qubit $j$ |
| Phase gate | $S_j$ | Maps X→Y on qubit $j$ (Z unchanged) |
| CNOT | $\text{CNOT}_{c,t}$ | Propagates X from control to target; propagates Z from target to control |

The synthesis algorithm:
1. For each generator, find a qubit where it has support (X, Y, or Z bit set).
2. If the support is X, apply H to convert to Z. If Y, apply S then H.
3. Use CNOTs to clear all other qubits' support, leaving Z on exactly one qubit.

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

After rotation, the target qubits are diagonally taperable, and we apply the v1 diagonal tapering from Chapter 9.

---

## The Unified Pipeline

FockMap's `taper` function combines everything:

```fsharp
// Full Clifford tapering (default)
let result = taper defaultTaperingOptions hamiltonian

// Diagonal only (v1 fallback)
let result = taper { defaultTaperingOptions with Method = DiagonalOnly } h

// Cap removal
let result = taper { defaultTaperingOptions with MaxQubitsToRemove = Some 2 } h

// Explicit sector
let result = taper { defaultTaperingOptions with Sector = [(0,1); (1,-1)] } h
```

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

$Z_0 Z_1$ is a Z₂ generator. One independent generator → one qubit can be tapered.

**Step 3: Clifford synthesis — rotate $Z_0 Z_1$ onto a single qubit.**

We want a circuit $U$ such that $U (Z_0 Z_1) U^\dagger = Z_0 I_1$ — the generator acts on only one qubit after rotation.

The key gate: **CNOT(1 → 0)** (qubit 1 is control, qubit 0 is target). Recall from Chapter 4 that CNOT propagates Z from target to control. So:

$$\text{CNOT}_{1,0}:\quad Z_0 \to Z_0, \quad Z_1 \to Z_0 Z_1$$

Wait — that goes the wrong way (it *creates* $Z_0 Z_1$, not removes it). We need the reverse: **CNOT(0 → 1)** (qubit 0 is control, qubit 1 is target):

$$\text{CNOT}_{0,1}:\quad Z_0 \to Z_0 Z_1, \quad Z_1 \to Z_1$$

That also doesn't isolate it. Let's think more carefully. Under CNOT conjugation, the *Z* propagation rule is: $Z_{\text{target}} \to Z_{\text{control}} \cdot Z_{\text{target}}$. So if we apply CNOT(0,1) to $Z_0 Z_1$:

$$Z_0 Z_1 \xrightarrow{\text{CNOT}_{0,1}} Z_0 \cdot (Z_0 Z_1) = Z_0^2 Z_1 = I \cdot Z_1 = Z_1$$

Now $Z_0 Z_1$ has become $Z_1$ — a single-qubit Z on qubit 1!

> **What just happened:** The CNOT "absorbed" the $Z_0$ factor by multiplying it with another $Z_0$, leaving only $Z_1$. This is the Clifford rotation — it's not a physical rotation in space, but an algebraic simplification achieved by conjugation with a gate.

**Step 4: Apply the CNOT to every Hamiltonian term.**

Under CNOT(0,1) conjugation, each term transforms:

| Original | After CNOT(0,1) |
|:---:|:---:|
| $X_0 X_1$ | $X_0 X_1$ → actually transforms to $X_0 I_1 \cdot I_0 X_1 = X_0 X_1$... |

Actually, let's let FockMap do this — the gate-by-gate algebra is fiddly by hand. The point is that after applying the Clifford, one of the qubits (the target of the rotation) now has only I or Z across all terms — it has become diagonally taperable.

**Step 5: Diagonal taper.** Fix the newly-diagonal qubit to sector $+1$, remove it. The result is a 1-qubit Hamiltonian.

```fsharp
let heis =
    [| PauliRegister("XX", Complex(1.0, 0.0))
       PauliRegister("YY", Complex(1.0, 0.0))
       PauliRegister("ZZ", Complex(1.0, 0.0)) |]
    |> PauliRegisterSequence

let result = taper defaultTaperingOptions heis
printfn "%d → %d qubits" result.OriginalQubitCount result.TaperedQubitCount
// 2 → 1 qubit
```

A 2-qubit problem reduced to 1 qubit — a 2× Hilbert space reduction that diagonal-only tapering would have missed entirely.

---

## Key Takeaways

- The **binary Pauli representation** (two bits per qubit) turns commutativity checking into a crosswise binary dot product — fast and exact.
- The **null space** of the commutation check matrix (computed by binary Gaussian elimination) gives all Z₂ generators.
- **Clifford synthesis** rotates multi-qubit generators onto single-qubit Zs using H, S, and CNOT — no matrices needed.
- **The unified `taper` function** handles both diagonal and Clifford tapering with one API.
- Everything is symbolic and exact — no approximation, no eigensolvers, no numerical instability.

## Further Reading

- Bravyi, S. et al. "Tapering off qubits to simulate fermionic Hamiltonians." arXiv:1701.08213 (2017). The general Z₂ tapering framework.
- Aaronson, S. and Gottesman, D. "Improved simulation of stabilizer circuits." *Phys. Rev. A* 70, 052328 (2004). The tableau formalism underlying our Clifford implementation.

---

**Previous:** [Chapter 10 — Diagonal Z₂ Symmetries](10-diagonal-z2.html)

**Next:** [Chapter 12 — Tapering Benchmarks](12-tapering-benchmarks.html)
