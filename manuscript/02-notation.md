# Chapter 2: The Notation Minefield

_Two communities, two integral conventions, one factor-of-two error that will derail your entire Hamiltonian if you get it wrong. This chapter exists to make sure you don't._

## In This Chapter

- **What you'll learn:** The difference between chemist's (Mulliken) and physicist's (Dirac) two-electron integral notation, how to convert between them, and the three most common errors that produce plausible-but-wrong Hamiltonians.
- **Why this matters:** Every sign, every coefficient, every eigenvalue in the rest of this book depends on getting the integral convention right *here*. The errors are insidious: wrong-convention Hamiltonians have the right structure, the right symmetry, and the right number of terms. Only the numbers are wrong.
- **Prerequisites:** Chapter 1 (you know what $h_{pq}$ and the two-body integrals represent physically).

---

## The Problem

There are two standard notations for two-electron repulsion integrals. They are both in widespread use. They are both perfectly valid. And they index the same physical quantity with the indices in **different positions**.

If you take integrals from a chemistry code (which typically outputs chemist's notation) and plug them into a physics formula (which uses physicist's notation) without converting, you will get the wrong Hamiltonian. You will not get an error message. Your code will run. Your Pauli strings will have the right structure. Your eigenvalues will be wrong — silently, plausibly, maddeningly wrong.

This chapter names the two conventions, shows you exactly how they differ, gives you the conversion rule, and then catalogues the three errors that trap essentially every student who encounters this material for the first time. If you remember nothing else from this chapter, remember the boxed equation at the centre.

---

## Convention 1: Chemist's Notation (Mulliken)

Chemist's notation — also called Mulliken notation or charge-density notation — groups indices by **spatial coordinate**. The integral is written with square brackets:

$$[pq \mid rs] = \iint \phi_p^*(\mathbf{r}_1)\,\phi_q(\mathbf{r}_1)\;\frac{1}{r_{12}}\;\phi_r^*(\mathbf{r}_2)\,\phi_s(\mathbf{r}_2)\;d\mathbf{r}_1\,d\mathbf{r}_2$$

Read it as: "the charge density $\phi_p^* \phi_q$ at position $\mathbf{r}_1$ interacts via Coulomb repulsion with the charge density $\phi_r^* \phi_s$ at position $\mathbf{r}_2$."

The bracket $[pq\mid$ contains the two orbitals that share electron 1's coordinate. The bracket $\mid rs]$ contains the two orbitals that share electron 2's coordinate. Within each bracket: complex conjugate (bra) first, ket second.

**Why chemists like it:** The indices are grouped the way charge densities are: $\rho_1(\mathbf{r}_1) = \phi_p^*(\mathbf{r}_1)\phi_q(\mathbf{r}_1)$, and the integral is just the Coulomb interaction between two charge distributions. It makes the *physics* of repulsion visually clear.

> I first encountered this notation in Dr. Robert Ditchfield's Quantum Chemistry course (Chem 81) at Dartmouth in 1992. He would write the charge-density form on the blackboard and say, "This is the natural way to think about it." He was right — it *is* the natural way to think about it. It's just not the way the second-quantized Hamiltonian is written.

**Where you'll encounter it:** Most quantum chemistry codes (PySCF, Gaussian, ORCA, NWChem) output integrals natively in chemist's notation. OpenFermion's `MolecularData` object stores them this way. If you import integrals from a chemistry pipeline, assume chemist's notation unless the documentation explicitly says otherwise.

---

## Convention 2: Physicist's Notation (Dirac)

Physicist's notation — sometimes called Dirac notation for integrals (not to be confused with Dirac bra-ket notation for states) — groups indices by **particle identity**:

$$\langle pq \mid rs\rangle = \iint \phi_p^*(\mathbf{r}_1)\,\phi_q^*(\mathbf{r}_2)\;\frac{1}{r_{12}}\;\phi_r(\mathbf{r}_1)\,\phi_s(\mathbf{r}_2)\;d\mathbf{r}_1\,d\mathbf{r}_2$$

Read it as: "electron 1 scatters from orbital $p$ to orbital $r$, while electron 2 scatters from orbital $q$ to orbital $s$."

Here $p$ and $r$ both belong to electron 1 (bra and ket respectively), while $q$ and $s$ both belong to electron 2. The angle brackets $\langle\,\rangle$ signal physicist's convention; the square brackets $[\,]$ signal chemist's.

**Why physicists like it:** The operator ordering in second quantization directly mirrors the integral indices: $\langle pq \mid rs\rangle \, a_p^\dagger a_q^\dagger a_s a_r$. The bra indices ($p, q$) match the creation operators; the ket indices ($r, s$) match the annihilation operators (in reverse order — more on this below).

**Where you'll encounter it:** Most quantum computing papers, including the foundational encoding papers (Jordan–Wigner, Bravyi–Kitaev, Seeley–Richard–Love), use physicist's notation. The second-quantized Hamiltonian is almost always written with angle brackets.

---

## The Conversion: One Equation to Rule Them All

Comparing the two definitions term by term — matching which orbital sits at which coordinate — yields:

$$\boxed{\langle pq \mid rs\rangle = [pr \mid qs]}$$

That's it. Memorize this. Tattoo it if necessary.

The indices are **shuffled**, not just relabelled. The physicist's pair $(p, r)$ — the two orbitals for electron 1 — becomes the *left* bracket of the chemist's notation, with $p$ in the bra position and $r$ in the ket position. The physicist's pair $(q, s)$ — the two orbitals for electron 2 — becomes the *right* bracket.

### Worked example

Suppose you have the chemist's integral $[00 \mid 00] = 0.6745$ Ha (the Coulomb self-repulsion of two electrons both in the $\sigma_g$ orbital). What is the corresponding physicist's integral?

Use the conversion: $\langle pq \mid rs\rangle = [pr \mid qs]$. We need $[pr \mid qs] = [00 \mid 00]$, so $p = 0, r = 0, q = 0, s = 0$. Therefore:

$$\langle 00 \mid 00\rangle = [00 \mid 00] = 0.6745 \text{ Ha}$$

That one was trivial — all indices are the same. Now consider a more revealing example: $[00 \mid 11] = 0.6636$ Ha (the Coulomb repulsion between an electron in $\sigma_g$ and one in $\sigma_u$). We need $[pr \mid qs] = [00 \mid 11]$, so $p = 0, r = 0, q = 1, s = 1$. Therefore:

$$\langle 01 \mid 01\rangle = [00 \mid 11] = 0.6636 \text{ Ha}$$

Notice: $\langle 01 \mid 01\rangle \neq \langle 00 \mid 11\rangle$ in general. The integral $\langle 00 \mid 11\rangle = [01 \mid 01]$ is a different integral (the exchange integral). For H₂ in STO-3G, $[01 \mid 01] = 0.6976$ Ha — a different value from $[00 \mid 11] = 0.6636$ Ha.

> **Common Mistake #1: Using the wrong convention.** If you take $[00 \mid 11]$ from PySCF and use it as $\langle 00 \mid 11\rangle$ in the physicist's Hamiltonian, you have swapped a Coulomb integral for an exchange integral. The Hamiltonian will have plausible structure, but the eigenvalues will be wrong. There is no error message. The only way to catch it is to verify against a known result.

---

## The Hamiltonian: Which Convention Goes Where?

The standard second-quantized electronic Hamiltonian is written in **physicist's** notation:

$$\hat{H} = \sum_{pq} h_{pq}\, a_p^\dagger a_q + \frac{1}{2}\sum_{pqrs} \langle pq \mid rs\rangle\, a_p^\dagger a_q^\dagger a_s a_r + V_{nn}$$

Note three things:

1. **The one-body term** $h_{pq}\, a_p^\dagger a_q$ is convention-independent — there's only one standard for one-electron integrals.

2. **The two-body term** uses physicist's notation $\langle pq \mid rs\rangle$. If you have chemist's integrals, convert first: $\langle pq \mid rs\rangle = [pr \mid qs]$.

3. **The operator ordering is $a_p^\dagger a_q^\dagger a_s a_r$** — the annihilation operators are in *reverse* order relative to the ket indices of the integral ($r, s$ become $a_s a_r$, not $a_r a_s$). This is not a typo. It follows from the definition of the antisymmetrized matrix element and the anti-commutation relations. Getting this order wrong flips the sign on certain exchange terms.

If you prefer to work entirely in chemist's notation, the Hamiltonian reads:

$$\hat{H} = \sum_{pq} h_{pq}\, a_p^\dagger a_q + \frac{1}{2}\sum_{pqrs} [pr \mid qs]\, a_p^\dagger a_q^\dagger a_s a_r + V_{nn}$$

This is the same equation — we just substituted the conversion rule. Use whichever form matches your integral source, but be consistent.

---

## The Symmetries That Save You (and Can Mislead You)

Two-electron integrals have symmetries that reduce the number of independent values:

### Chemist's notation symmetries

$$[pq \mid rs] = [qp \mid rs]^* = [pq \mid sr]^* = [rs \mid pq]$$

For **real** orbitals (which is the case for all our examples):

$$[pq \mid rs] = [qp \mid rs] = [pq \mid sr] = [qp \mid sr] = [rs \mid pq] = [sr \mid pq] = [rs \mid qp] = [sr \mid qp]$$

That's 8-fold symmetry — the number of independent integrals is roughly $n^4/8$ rather than $n^4$.

### Physicist's notation symmetries

$$\langle pq \mid rs\rangle = \langle qp \mid sr\rangle = \langle rs \mid pq\rangle^*$$

For real orbitals:

$$\langle pq \mid rs\rangle = \langle qp \mid sr\rangle = \langle rs \mid pq\rangle = \langle sr \mid qp\rangle$$

Note the difference: the symmetry in physicist's notation swaps *both* bra indices and *both* ket indices simultaneously, while in chemist's notation you can swap within each bracket independently.

> **Common Mistake #2: Applying chemist's symmetries to physicist's integrals.** If you assume $\langle pq \mid rs\rangle = \langle qp \mid rs\rangle$ (swapping only the bra indices), you are applying a chemist's symmetry to a physicist's integral. In physicist's notation, the correct symmetry is $\langle pq \mid rs\rangle = \langle qp \mid sr\rangle$ — you must swap *both* bra indices and *both* ket indices together.

---

## The $\frac{1}{2}$ Prefactor

The factor of $\frac{1}{2}$ in front of the two-body term:

$$\frac{1}{2}\sum_{pqrs} \langle pq \mid rs\rangle\, a_p^\dagger a_q^\dagger a_s a_r$$

exists because the sum over all four indices counts every pair of electrons twice (once as $(p, q)$ and once as $(q, p)$). The $\frac{1}{2}$ corrects for this double-counting.

Some references absorb the $\frac{1}{2}$ into the integrals, defining $g_{pqrs} = \frac{1}{2}\langle pq \mid rs\rangle$. Others restrict the summation to $p < q$ and $r < s$ to avoid double-counting, eliminating the prefactor. Both are correct. But if you mix conventions — using the unrestricted sum without the $\frac{1}{2}$, or using the restricted sum with it — your two-body energy will be off by a factor of 2.

> **Common Mistake #3: Losing the $\frac{1}{2}$.** This one produces eigenvalues that are in the right ballpark but wrong in the second decimal place. Because the one-body terms are unaffected, the error is systematic and can look like a "correlation energy correction" rather than a bug. It is a bug.

---

## A Complete Example: H₂ Two-Body Integrals

Here are all the non-zero two-body integrals for H₂ in STO-3G, in *both* conventions, so you can verify conversions:

| Chemist's $[pq \mid rs]$ | Value (Ha) | Physicist's $\langle pq \mid rs\rangle$ | Conversion used |
|:---:|:---:|:---:|:---|
| $[00 \mid 00]$ | $0.6745$ | $\langle 00 \mid 00\rangle$ | $\langle 00 \mid 00\rangle = [00 \mid 00]$ |
| $[11 \mid 11]$ | $0.6974$ | $\langle 11 \mid 11\rangle$ | $\langle 11 \mid 11\rangle = [11 \mid 11]$ |
| $[00 \mid 11]$ | $0.6636$ | $\langle 01 \mid 01\rangle$ | $\langle 01 \mid 01\rangle = [00 \mid 11]$ |
| $[01 \mid 01]$ | $0.6976$ | $\langle 00 \mid 11\rangle$ | $\langle 00 \mid 11\rangle = [01 \mid 01]$ |
| $[01 \mid 10]$ | $0.1809$ | $\langle 01 \mid 10\rangle$ | $\langle 01 \mid 10\rangle = [01 \mid 10]$ |

Study this table. Make sure you can derive each entry in the rightmost column from the boxed conversion rule. If you can do that, you will never make error #1.

---

## The Companion Library: FockMap

Throughout this book, we verify every formula and every table against executable code. The library we use is **FockMap** — an open-source F# framework for symbolic Fock-space operator algebra.

A few things worth knowing about it:

- **It is symbolic, not numerical.** When FockMap multiplies two Pauli strings, it uses the exact Pauli multiplication table with algebraic phase tracking — not floating-point matrix multiplication. Intermediate results are inspectable and exact down to the sign.

- **It is functional.** FockMap is written in F#, a functional-first language on .NET. If you've never used F#, don't worry — the code snippets in this book are short and read like mathematical notation. You don't need to be an F# programmer to follow them; you only need to be able to read `let x = ...` as "define x to be ..."

- **It runs anywhere.** FockMap targets .NET 10 and runs on Windows, macOS, and Linux. Install it with `dotnet add package FockMap`, or clone the repository and build from source.

- **It is the subject of the JOSS paper** cited in the preface. If you want to understand the library's design — the type system, the two encoding frameworks, the test suite — that paper is the place to look. This book focuses on *using* the library to learn the physics.

When you see code in this book, it serves one purpose: to show that the mathematics we just derived actually computes the right answer. The code is evidence, not the lesson.

---

## FockMap's Convention

FockMap uses **physicist's notation** throughout. The `computeHamiltonianWith` function expects integrals in physicist's convention:

- One-body key `"p,q"` → $h_{pq}$
- Two-body key `"p,q,r,s"` → $\langle pq \mid rs\rangle$

If your integral source provides chemist's notation (most do), convert the indices before building the coefficient factory:

```fsharp
// chemist_integrals stores [pq|rs] indexed as (p,q,r,s)
// physicist's convention: ⟨pq|rs⟩ = [pr|qs]
let physicistFactory (key : string) =
    let parts = key.Split(',') |> Array.map int
    match parts.Length with
    | 2 -> // one-body: same convention
        let p, q = parts.[0], parts.[1]
        chemist_integrals |> Map.tryFind (sprintf "%d,%d" p q)
    | 4 -> // two-body: convert ⟨pq|rs⟩ → [pr|qs]
        let p, q, r, s = parts.[0], parts.[1], parts.[2], parts.[3]
        chemist_integrals |> Map.tryFind (sprintf "%d,%d,%d,%d" p r q s)
    | _ -> None
```

This is the single most important function in any quantum chemistry pipeline. Get it right and everything downstream works. Get it wrong and everything downstream is silently, plausibly wrong.

---

## Key Takeaways

- Two-electron integrals come in two flavours: chemist's $[pq \mid rs]$ (grouped by coordinate) and physicist's $\langle pq \mid rs\rangle$ (grouped by particle).
- The conversion is $\langle pq \mid rs\rangle = [pr \mid qs]$ — indices are shuffled, not just relabelled.
- The second-quantized Hamiltonian uses physicist's notation. If your integrals come from a chemistry code, convert first.
- The three most common errors are: (1) using integrals in the wrong convention, (2) applying chemist's symmetries to physicist's integrals, and (3) mishandling the $\frac{1}{2}$ prefactor.
- When in doubt, verify against known eigenvalues. There is no other reliable check.

## Common Mistakes

1. **Wrong convention.** Using $[pq \mid rs]$ where $\langle pq \mid rs\rangle$ is expected (or vice versa). Produces plausible but wrong Hamiltonians.

2. **Wrong symmetries.** Physicist's integrals have 4-fold symmetry ($\langle pq \mid rs\rangle = \langle qp \mid sr\rangle$), not 8-fold. Apply the wrong symmetry rule and you'll generate integrals that don't exist.

3. **The missing $\frac{1}{2}$.** Double-counts the two-body interaction. The error looks like a systematic energy shift, not a bug.

## Exercises

1. **Conversion drill.** Given $[01 \mid 10] = 0.1809$ Ha, what is $\langle 00 \mid 11\rangle$? What is $\langle 01 \mid 10\rangle$? (Hint: one of these equals $[01 \mid 10]$; the other does not.)

2. **Symmetry check.** Starting from $\langle 01 \mid 01\rangle = 0.6636$ Ha, use the physicist's symmetry $\langle pq \mid rs\rangle = \langle qp \mid sr\rangle$ to find $\langle 10 \mid 10\rangle$. Now convert both to chemist's notation and verify they correspond to the same integral.

3. **Error detection.** You compute a 4-qubit H₂ Hamiltonian and get a ground-state energy of $-1.72$ Ha (before adding $V_{nn}$). The correct value is $-1.89$ Ha. Which of the three errors is most likely, and why?

## Further Reading

- Szabo, A. and Ostlund, N. S. *Modern Quantum Chemistry.* Chapter 2, §2.3.2–2.3.3 gives both conventions and the conversion explicitly.
- Helgaker, T., Jørgensen, P., and Olsen, J. *Molecular Electronic-Structure Theory.* Chapter 9 is the definitive treatment of two-electron integrals, including symmetry properties.
- PySCF documentation on `mol.intor('int2e')` — returns integrals in chemist's notation.

---

**Previous:** [Chapter 1 — The Electronic Structure Problem](01-electronic-structure.html)

**Next:** [Chapter 3 — From Spatial to Spin-Orbital Integrals](03-spin-orbitals.html)
