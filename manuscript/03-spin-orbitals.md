# Chapter 3: From Spatial to Spin-Orbital Integrals

_Every electron has a spin. Every integral must account for it. This chapter doubles the index space and fills in the complete integral tables that drive the encoding pipeline._

## In This Chapter

- **What you'll learn:** How to expand spatial-orbital integrals into spin-orbital one-body and two-body terms, including cross-spin interactions that many implementations forget.
- **Why this matters:** The encoding step (Jordan–Wigner, Bravyi–Kitaev, etc.) operates on spin-orbital creation and annihilation operators. If your integral tables are in the spatial basis, you must expand them to spin-orbitals first. Get this wrong and your Hamiltonian will be missing the off-diagonal (XX, YY) Pauli terms entirely.
- **Prerequisites:** Chapters 1–2 (you know the spatial integrals and the notation conventions).

---

## Why We Need Spin-Orbitals

In Chapter 1, we worked with **spatial orbitals** — the molecular orbitals $\sigma_g$ and $\sigma_u$, described by their spatial wavefunctions. There were two of them, and the integrals were $2 \times 2$ matrices and $2 \times 2 \times 2 \times 2$ tensors.

But an electron is not just a spatial wavefunction. It also has **spin**.

### What spin is (and isn't)

Electron spin is an intrinsic angular momentum that has no classical analogue. It is not the electron "spinning on its axis" — that mental picture, while universal in introductory courses, breaks down immediately under scrutiny (the electron would need to rotate faster than light). Spin is a purely quantum-mechanical degree of freedom that happens to behave like angular momentum: it has a magnitude ($s = 1/2$) and a projection along any chosen axis.

For a spin-$1/2$ particle, the projection along the $z$-axis can take only two values:
- $m_s = +1/2$, which we call $\alpha$ or "spin-up" ($\uparrow$)
- $m_s = -1/2$, which we call $\beta$ or "spin-down" ($\downarrow$)

This is not a choice of convention — it is a consequence of the mathematics of angular momentum in quantum mechanics. The state space of a single electron's spin is exactly two-dimensional: $\{\lvert\alpha\rangle, \lvert\beta\rangle\}$. This is, not coincidentally, the same dimension as a qubit.

### Why spin doubles the orbital space

The **Pauli exclusion principle** says that no two electrons can occupy exactly the same quantum state. A quantum state is specified by *both* the spatial orbital *and* the spin. So the spatial orbital $\sigma_g$ can hold:
- one electron with spin $\alpha$ (the spin-orbital $\sigma_g, \alpha$)
- one electron with spin $\beta$ (the spin-orbital $\sigma_g, \beta$)
- but NOT two electrons with the same spin

Each spatial orbital produces **two** spin-orbitals. Two spatial orbitals give four spin-orbitals. Seven spatial orbitals (H₂O in STO-3G) give fourteen spin-orbitals. The spin degree of freedom doubles the index space — and quadruples the two-body integral tensor.

### Spin-orbit coupling: when spin and space talk to each other

In the non-relativistic Hamiltonian we've been using, spin and spatial degrees of freedom are completely independent — the Hamiltonian contains no term that couples them. An electron in $\sigma_g$ with spin $\alpha$ has exactly the same energy as one with spin $\beta$. The one-body integrals are spin-independent, and the two-body integrals only require each electron to conserve its own spin (as we'll see below).

This is an approximation. In reality, **spin-orbit coupling** — the interaction between an electron's spin angular momentum and its orbital angular momentum — mixes the spin and spatial degrees of freedom. The coupling strength scales as $Z^4$ (the fourth power of the nuclear charge), so it is negligible for hydrogen ($Z=1$) and small for light elements like carbon and oxygen, but significant for heavy atoms (iodine, lead, actinides).

For the molecules in this book (H₂, H₂O), spin-orbit coupling is negligible and we work with the non-relativistic Hamiltonian throughout. This means:
- One-body integrals are diagonal in spin: $\langle \alpha \mid \beta \rangle = 0$
- Two-body integrals conserve each electron's spin independently
- The total spin quantum numbers ($S$, $M_S$) are good quantum numbers

When spin-orbit coupling matters (heavy-element chemistry, relativistic quantum chemistry), the spin-orbital structure becomes more complex — spin is no longer a good quantum number, and the integral expansion rules in this chapter must be modified. We won't need that here, but it's worth knowing the boundary of our approximation.

### The upshot for encoding

The second-quantized Hamiltonian

$$\hat{H} = \sum_{pq} h_{pq}\, a_p^\dagger a_q + \frac{1}{2}\sum_{pqrs} \langle pq \mid rs\rangle\, a_p^\dagger a_q^\dagger a_s a_r$$

sums over **spin-orbital** indices $p, q, r, s$. The creation operator $a_p^\dagger$ creates an electron in spin-orbital $p$ — a specific spatial orbital *with a specific spin*. This means we need integral tables indexed by spin-orbitals, not spatial orbitals.

For H₂: 2 spatial orbitals → **4 spin-orbitals**. The one-body integral matrix goes from $2 \times 2$ to $4 \times 4$. The two-body integral tensor goes from $2^4 = 16$ elements to $4^4 = 256$ elements (most of which are zero).

---

## Spin-Orbital Indexing

Each spatial orbital $\mu$ gives rise to two spin-orbitals. We use **interleaved** indexing — alternating $\alpha$ and $\beta$ for each spatial orbital:

| Spin-orbital index $p$ | Spatial orbital | Spin | Notation |
|:---:|:---:|:---:|:---:|
| 0 | $\sigma_g$ (orbital 0) | $\alpha$ | $0\alpha$ |
| 1 | $\sigma_g$ (orbital 0) | $\beta$ | $0\beta$ |
| 2 | $\sigma_u$ (orbital 1) | $\alpha$ | $1\alpha$ |
| 3 | $\sigma_u$ (orbital 1) | $\beta$ | $1\beta$ |

The conversion between spin-orbital index $p$ and spatial orbital/spin:

$$\text{spatial orbital} = \lfloor p/2 \rfloor \qquad \text{spin} = p \bmod 2 \quad (0 = \alpha,\; 1 = \beta)$$

> **Convention alert:** Some references use **blocked** indexing ($\alpha$ orbitals first, then all $\beta$), so the ordering would be $0\alpha, 1\alpha, 0\beta, 1\beta$. We use interleaved indexing because it matches PySCF's default and FockMap's convention. If your integral source uses a different spin-orbital ordering, you must permute before proceeding. This is another silent-error source.

---

## One-Body Expansion

The spin-orbital one-body integral is straightforward: the spatial integral survives only when the spins match.

$$h^{\text{spin}}_{pq} = h^{\text{spatial}}_{\lfloor p/2 \rfloor,\, \lfloor q/2 \rfloor} \times \delta(\sigma_p, \sigma_q)$$

In words: the one-body integral between spin-orbitals $p$ and $q$ equals the spatial integral between their parent spatial orbitals, *provided they have the same spin*. If the spins differ, the integral is zero.

Why? The one-body Hamiltonian (kinetic energy + electron-nucleus attraction) does not act on spin. The spin part of the wavefunction integrates to $\langle \alpha \mid \alpha \rangle = 1$ or $\langle \beta \mid \beta \rangle = 1$ for same spins, and $\langle \alpha \mid \beta \rangle = 0$ for opposite spins.

### H₂ one-body integrals (spin-orbital basis)

| $p$ | $q$ | $h^{\text{spin}}_{pq}$ (Ha) | How we got it |
|:---:|:---:|:---:|:---|
| 0 ($\sigma_g, \alpha$) | 0 ($\sigma_g, \alpha$) | $-1.2563$ | $h^{\text{spatial}}_{00}$, same spin ✓ |
| 1 ($\sigma_g, \beta$)  | 1 ($\sigma_g, \beta$)  | $-1.2563$ | $h^{\text{spatial}}_{00}$, same spin ✓ |
| 2 ($\sigma_u, \alpha$) | 2 ($\sigma_u, \alpha$) | $-0.4719$ | $h^{\text{spatial}}_{11}$, same spin ✓ |
| 3 ($\sigma_u, \beta$)  | 3 ($\sigma_u, \beta$)  | $-0.4719$ | $h^{\text{spatial}}_{11}$, same spin ✓ |

All off-diagonal entries ($p \neq q$) are zero — either because the spatial integral is zero ($h^{\text{spatial}}_{01} = 0$ by symmetry) or because the spins don't match.

This is the simple case. The two-body expansion is where the subtlety lives.

---

## Two-Body Expansion

The spin-orbital two-body integral in physicist's notation is:

$$\langle pq \mid rs\rangle_{\text{spin}} = \left[\frac{p}{2}\frac{r}{2}\,\bigg|\,\frac{q}{2}\frac{s}{2}\right]_{\text{spatial}} \times \delta(\sigma_p, \sigma_r) \times \delta(\sigma_q, \sigma_s)$$

where we've used the conversion $\langle pq \mid rs\rangle = [pr \mid qs]$ from Chapter 2 to express the result in terms of chemist's spatial integrals.

The key insight: **each electron independently conserves its spin**. Electron 1 (described by indices $p$ and $r$) must have the same spin in both orbitals. Electron 2 (indices $q$ and $s$) must also conserve its spin. But the two electrons **need not have the same spin as each other**.

This means there are **four spin blocks** that contribute non-zero integrals:

| Block | Electron 1 | Electron 2 | Contributes? |
|:---:|:---:|:---:|:---:|
| $\alpha\alpha - \alpha\alpha$ | $\sigma_p = \sigma_r = \alpha$ | $\sigma_q = \sigma_s = \alpha$ | ✓ |
| $\beta\beta - \beta\beta$ | $\sigma_p = \sigma_r = \beta$ | $\sigma_q = \sigma_s = \beta$ | ✓ |
| $\alpha\beta - \alpha\beta$ | $\sigma_p = \sigma_r = \alpha$ | $\sigma_q = \sigma_s = \beta$ | ✓ |
| $\beta\alpha - \beta\alpha$ | $\sigma_p = \sigma_r = \beta$ | $\sigma_q = \sigma_s = \alpha$ | ✓ |

All four blocks contribute. The cross-spin blocks ($\alpha\beta$ and $\beta\alpha$) are the ones that beginners most commonly forget.

> **Common Mistake #1: Omitting cross-spin integrals.** If you include only the same-spin blocks ($\alpha\alpha$ and $\beta\beta$) and forget the cross-spin blocks, your encoded Hamiltonian will contain only Z-type (diagonal) Pauli terms and no XX/YY excitation terms. The Hamiltonian will commute with all computational basis states, meaning it cannot produce entanglement or represent any quantum correlation. The eigenvalues will be the Hartree–Fock values, not the exact (full-CI) values. This was our actual first-implementation bug.

### Worked example: a cross-spin integral

Consider $\langle 0\alpha,\, 1\beta \mid 0\alpha,\, 1\beta \rangle$. In our index scheme: $p = 0, q = 3, r = 0, s = 3$.

Check spin conservation:
- Electron 1: $\sigma_p = \alpha$, $\sigma_r = \alpha$ → same spin ✓
- Electron 2: $\sigma_q = \beta$, $\sigma_s = \beta$ → same spin ✓

Spatial indices: $\lfloor p/2 \rfloor = 0$, $\lfloor q/2 \rfloor = 1$, $\lfloor r/2 \rfloor = 0$, $\lfloor s/2 \rfloor = 1$.

Convert to chemist's spatial: $[\lfloor p/2 \rfloor \lfloor r/2 \rfloor \mid \lfloor q/2 \rfloor \lfloor s/2 \rfloor] = [00 \mid 11] = 0.6636$ Ha.

This integral represents the Coulomb repulsion between a spin-$\alpha$ electron in $\sigma_g$ and a spin-$\beta$ electron in $\sigma_u$. It has nothing to do with spin flips — both electrons keep their original spins — but it is a *cross-spin interaction* because the two electrons have different spins.

---

## The Complete Integral Tables

For reference, here are all the inputs to the encoding pipeline.

### Molecular Parameters

| Parameter | Value |
|:---|:---|
| Bond length $R$ | 0.7414 Å = 1.401 Bohr |
| Nuclear repulsion $V_{nn}$ | 0.7151043391 Ha |
| Spatial orbitals | 2 ($\sigma_g$, $\sigma_u$) |
| Spin-orbitals | 4 |
| Electrons | 2 |
| Two-electron configurations | $\binom{4}{2} = 6$ |

### Spatial One-Body Integrals $h_{pq}$ (Ha)

|  | $q = 0$ ($\sigma_g$) | $q = 1$ ($\sigma_u$) |
|:---:|:---:|:---:|
| $p = 0$ | $-1.2563390730$ | $0$ |
| $p = 1$ | $0$ | $-0.4718960244$ |

### Spatial Two-Body Integrals $[pq \mid rs]$ (Ha)

| Chemist's integral | Value | Physicist's equivalent |
|:---:|:---:|:---:|
| $[00 \mid 00]$ | $0.6744887663$ | $\langle 00 \mid 00\rangle$ |
| $[11 \mid 11]$ | $0.6973979495$ | $\langle 11 \mid 11\rangle$ |
| $[00 \mid 11] = [11 \mid 00]$ | $0.6636340479$ | $\langle 01 \mid 01\rangle = \langle 10 \mid 10\rangle$ |
| $[01 \mid 10] = [10 \mid 01]$ | $0.1809312700$ | $\langle 01 \mid 10\rangle = \langle 10 \mid 01\rangle$ |
| $[01 \mid 01] = [10 \mid 10]$ | $0.6975782469$ | $\langle 00 \mid 11\rangle = \langle 11 \mid 00\rangle$ |

All other spatial two-body integrals are zero.

> **Note:** We show both conventions side by side so you can verify the conversion $\langle pq \mid rs\rangle = [pr \mid qs]$ from Chapter 2 on every line. If any entry surprises you, stop and work through the index shuffle until it doesn't.

---

## From Tables to Code

With the integral tables complete, we can build the coefficient factory that FockMap's Hamiltonian construction functions consume:

```fsharp
open System.Numerics
open Encodings

let h2Integrals = Map [
    // One-body (spin-orbital, interleaved indexing)
    ("0,0", Complex(-1.2563390730, 0.0))
    ("1,1", Complex(-1.2563390730, 0.0))
    ("2,2", Complex(-0.4718960244, 0.0))
    ("3,3", Complex(-0.4718960244, 0.0))

    // Two-body (physicist's convention, spin-orbital)
    // Same-spin αα-αα
    ("0,0,0,0", Complex(0.6744887663, 0.0))
    ("2,2,2,2", Complex(0.6973979495, 0.0))
    ("0,2,2,0", Complex(0.1809312700, 0.0))
    ("2,0,0,2", Complex(0.1809312700, 0.0))
    ("0,2,0,2", Complex(0.6975782469, 0.0))
    ("2,0,2,0", Complex(0.6975782469, 0.0))
    ("0,0,2,2", Complex(0.6636340479, 0.0))
    ("2,2,0,0", Complex(0.6636340479, 0.0))

    // Same-spin ββ-ββ (identical values, odd indices)
    ("1,1,1,1", Complex(0.6744887663, 0.0))
    ("3,3,3,3", Complex(0.6973979495, 0.0))
    ("1,3,3,1", Complex(0.1809312700, 0.0))
    ("3,1,1,3", Complex(0.1809312700, 0.0))
    ("1,3,1,3", Complex(0.6975782469, 0.0))
    ("3,1,3,1", Complex(0.6975782469, 0.0))
    ("1,1,3,3", Complex(0.6636340479, 0.0))
    ("3,3,1,1", Complex(0.6636340479, 0.0))

    // Cross-spin αβ-αβ (electron 1 = α, electron 2 = β)
    ("0,1,0,1", Complex(0.6744887663, 0.0))
    ("0,3,0,3", Complex(0.6636340479, 0.0))
    ("2,1,2,1", Complex(0.6636340479, 0.0))
    ("2,3,2,3", Complex(0.6973979495, 0.0))
    ("0,1,2,3", Complex(0.6975782469, 0.0))
    ("2,3,0,1", Complex(0.6975782469, 0.0))
    ("0,3,2,1", Complex(0.1809312700, 0.0))
    ("2,1,0,3", Complex(0.1809312700, 0.0))

    // Cross-spin βα-βα (electron 1 = β, electron 2 = α)
    ("1,0,1,0", Complex(0.6744887663, 0.0))
    ("1,2,1,2", Complex(0.6636340479, 0.0))
    ("3,0,3,0", Complex(0.6636340479, 0.0))
    ("3,2,3,2", Complex(0.6973979495, 0.0))
    ("1,0,3,2", Complex(0.6975782469, 0.0))
    ("3,2,1,0", Complex(0.6975782469, 0.0))
    ("1,2,3,0", Complex(0.1809312700, 0.0))
    ("3,0,1,2", Complex(0.1809312700, 0.0))
]

let h2Factory key = h2Integrals |> Map.tryFind key

// Build the 4-qubit H₂ Hamiltonian with Jordan-Wigner
let h2Hamiltonian = computeHamiltonianWith jordanWignerTerms h2Factory 4u
```

This is our first substantial code block — the coefficient factory that will feed the entire encoding pipeline. Notice the pattern: we built the chemistry understanding (Chapter 1), pinned down the notation (Chapter 2), expanded to spin-orbitals (this chapter), and only now convert to code. The code is evidence that the tables are correct, not a substitute for understanding them.

---

## A Preview of What's Coming

The `h2Hamiltonian` produced by the code above is a `PauliRegisterSequence` — a symbolic sum of Pauli strings with complex coefficients. It will have 15 terms (the famous H₂ Hamiltonian that appears in virtually every quantum computing tutorial). We will derive those 15 terms step by step in Chapter 5.

But first, Chapter 4 will introduce the encoding itself: what is a Pauli string, why can't we just use the occupation vectors directly, and how does Jordan–Wigner inject the fermion signs that qubits don't naturally produce?

---

## Key Takeaways

- Each spatial orbital produces two spin-orbitals ($\alpha$ and $\beta$). The spin-orbital index space is twice the spatial index space.
- One-body integrals require same-spin: $h^{\text{spin}}_{pq} = h^{\text{spatial}}_{\lfloor p/2 \rfloor, \lfloor q/2 \rfloor} \times \delta(\sigma_p, \sigma_q)$.
- Two-body integrals require each electron to independently conserve spin: $\delta(\sigma_p, \sigma_r) \times \delta(\sigma_q, \sigma_s)$. This admits **four** spin blocks, including the cross-spin blocks $\alpha\beta$ and $\beta\alpha$.
- Omitting cross-spin integrals is the most common implementation error at this stage. It produces a diagonal-only Hamiltonian that cannot represent quantum correlations.
- FockMap expects spin-orbital integrals in physicist's convention with interleaved indexing.

## Common Mistakes

1. **Omitting cross-spin blocks.** The $\alpha\beta$ and $\beta\alpha$ blocks produce the off-diagonal Pauli terms (XX, YY) that create entanglement. Without them, your quantum simulation is classical. **This is the single most common implementation failure at the integral stage** — if your encoded Hamiltonian has no off-diagonal terms, check the cross-spin blocks first.

2. **Wrong spin-orbital ordering.** Interleaved ($0\alpha, 0\beta, 1\alpha, 1\beta$) vs blocked ($0\alpha, 1\alpha, 0\beta, 1\beta$) indexing produces different integral tables. The eigenvalues are independent of indexing, but the Pauli string structure changes. Always check your convention.

3. **Spatial-to-physicist conversion errors.** This chapter combines two index transformations: spatial→spin-orbital *and* chemist→physicist. It's easy to get one right and the other wrong. Use the dual-convention table above as a cross-check.

## Exercises

1. **Cross-spin counting.** How many non-zero spin-orbital two-body integrals does H₂/STO-3G have in total? How many of those are cross-spin? (Answer: 32 total, 16 cross-spin — exactly half.)

2. **Missing block detection.** If you compute the H₂ Hamiltonian without the cross-spin blocks, how many Pauli terms do you get? What types of Pauli operators appear? (Hint: only I and Z.)

3. **Blocked indexing.** Rewrite the one-body spin-orbital table using blocked indexing ($0\alpha, 1\alpha, 0\beta, 1\beta$ → indices 0, 1, 2, 3). Which entries of the matrix change? Do the eigenvalues change?

4. **H₂O scale-up.** Water has 7 spatial orbitals in STO-3G (after frozen core), giving 14 spin-orbitals. How many one-body spin-orbital integrals can be non-zero? How many two-body? (Answer: up to $14^2 = 196$ one-body, up to $14^4 = 38{,}416$ two-body — though symmetry reduces these significantly.)

## Further Reading

- Szabo, A. and Ostlund, N. S. *Modern Quantum Chemistry.* §2.3 covers the spin-orbital expansion in detail.
- Crawford, T. D. and Schaefer, H. F. "An Introduction to Coupled Cluster Theory for Computational Chemists." *Reviews in Computational Chemistry*, Vol. 14, 2000. Appendix A gives explicit spin-orbital integral formulae.

---

**Previous:** [Chapter 2 — The Notation Minefield](02-notation.html)

**Next:** [Chapter 4 — The Quantum Computer's Vocabulary](04-qubits-gates-circuits.html)
