# Chapter 1: The Electronic Structure Problem

_We have a molecule. We want a quantum circuit. This chapter is about the first question: what, exactly, are we trying to compute?_

## In This Chapter

- **What you'll learn:** How a molecule becomes a finite mathematical problem — from the Schrödinger equation, through the Born–Oppenheimer approximation, to a small set of numbers called integrals.
- **Why this matters:** Every step in the rest of this book — encoding, tapering, Trotterization, circuit compilation — operates on the output of this chapter. If you don't understand what the integrals represent, you can't understand what the quantum computer is computing.
- **Prerequisites:** Linear algebra (matrices, eigenvalues). Introductory quantum mechanics (wavefunctions, bra-ket notation). No prior knowledge of molecular orbital theory or second quantization is assumed.

---

## The Question

Here is a question that a first-year chemistry student can state but no classical computer can answer exactly for anything larger than helium:

> **Given a molecule — its atoms and their positions — what is its ground-state energy?**

The ground-state energy determines whether a chemical reaction will happen, how strong a bond is, what shape a molecule takes, and why water boils at 100°C rather than −50°C. It is arguably the single most important quantity in all of chemistry. And for any system with more than one electron, we cannot compute it analytically — the electron–electron repulsion couples the electrons' coordinates, making the Schrödinger equation non-separable. Even helium, the simplest multi-electron atom, has no closed-form solution. Perturbation theory can improve the picture incrementally — each order of the expansion buys another digit or two of accuracy — but it never closes the gap to an exact result, and it converges poorly (or not at all) for strongly correlated systems.

Classical computational chemistry has developed an extraordinary arsenal of approximation methods — Hartree–Fock, density functional theory, coupled cluster, configuration interaction — each trading accuracy for tractability in a different way. These methods have transformed chemistry and earned multiple Nobel Prizes. But they all share a fundamental limitation: the computational cost of capturing electron correlation grows exponentially with the number of electrons.

This is where quantum simulation enters the picture. A quantum computer can, in principle, represent the quantum state of the electrons directly — using qubits in place of orbitals — and extract the ground-state energy without the exponential cost. The catch is that translating the molecular problem into a form that a quantum computer can execute requires a specific sequence of mathematical transformations, each with its own conventions, sign choices, and opportunities for error.

This chapter covers the first transformation: turning the continuous, infinite-dimensional molecular problem into a finite-dimensional matrix problem. The result will be a set of numbers — the **molecular integrals** — that encode everything we need to know about the molecule.

We will do this for the hydrogen molecule, H₂. Not because H₂ is interesting in itself (it isn't — any laptop can solve H₂ exactly in milliseconds), but because H₂ is small enough that we can see every step, check every number, and build intuition for what happens at larger scale. Later, when we work with H₂O, the same pipeline will produce larger numbers but the same kinds of objects.

---

## A Molecule Is a Collection of Charges

Strip away the language of orbitals and bonds and wavefunctions, and what remains is electrostatics: a molecule is a collection of positively charged nuclei and negatively charged electrons, interacting via Coulomb's law.

For H₂, this means:
- **Two protons** (charge $+e$ each), separated by a distance $R$
- **Two electrons** (charge $-e$ each), somewhere in the space around them

The total energy of this system depends on five types of interaction:

| Interaction | Formula | Sign | Strength |
|:---|:---|:---:|:---|
| Proton kinetic energy | $-\frac{\hbar^2}{2M_p}\nabla_A^2$ | — | Tiny (protons are heavy) |
| Electron kinetic energy | $-\frac{\hbar^2}{2m_e}\nabla_i^2$ | — | Significant |
| Proton–proton repulsion | $\frac{e^2}{\lvert\mathbf{R}_A - \mathbf{R}_B\rvert}$ | $+$ | Repulsive |
| Electron–proton attraction | $-\frac{Z_A e^2}{\lvert\mathbf{r}_i - \mathbf{R}_A\rvert}$ | $-$ | Attractive (this is what holds the molecule together) |
| Electron–electron repulsion | $\frac{e^2}{\lvert\mathbf{r}_i - \mathbf{r}_j\rvert}$ | $+$ | Repulsive (and this is what makes the problem hard) |

The full Hamiltonian is the sum of all five:

$$
\hat{H} = \underbrace{-\sum_{A=1}^{M} \frac{\hbar^2}{2M_A} \nabla_A^2}_{\text{nuclear KE}}
         \underbrace{-\sum_{i=1}^{N} \frac{\hbar^2}{2m_e} \nabla_i^2}_{\text{electronic KE}}
         + \underbrace{\sum_{A<B} \frac{Z_A Z_B e^2}{\lvert\mathbf{R}_A - \mathbf{R}_B\rvert}}_{\text{nuclear repulsion}}
         \underbrace{- \sum_{i,A} \frac{Z_A e^2}{\lvert\mathbf{r}_i - \mathbf{R}_A\rvert}}_{\text{electron-nuclear attraction}}
         + \underbrace{\sum_{i<j} \frac{e^2}{\lvert\mathbf{r}_i - \mathbf{r}_j\rvert}}_{\text{electron repulsion}}
$$

This is exact. No approximations. If we could solve this equation, we would have the exact energy of any molecule.

We can't. Not for H₂, not for anything with more than one electron. The electron–electron repulsion term couples the coordinates of every pair of electrons, making the equation non-separable.

> **Common Mistake #1:** Students sometimes think the difficulty is the number of particles. It isn't — classical N-body problems with gravitational interactions are also hard. The quantum difficulty is that we must find the *wavefunction* $\Psi(\mathbf{r}_1, \mathbf{r}_2, \ldots, \mathbf{r}_N)$, which lives in a $3N$-dimensional space. For a modest molecule with 50 electrons, this is a function of 150 continuous variables. No grid-based method can touch that.

---

## The Born–Oppenheimer Approximation: Freezing the Nuclei

Protons are 1836 times heavier than electrons. On the timescale of electronic motion, the nuclei are essentially stationary. This observation — due to Born and Oppenheimer (1927) — is the first simplification, and by far the most consequential: we treat the nuclear positions $\{\mathbf{R}_A\}$ as fixed parameters, not dynamical variables.

The result is the **electronic Hamiltonian**:

$$
\hat{H}_\text{el} = -\sum_{i=1}^{N} \frac{\hbar^2}{2m_e} \nabla_i^2
                     - \sum_{i,A} \frac{Z_A e^2}{\lvert\mathbf{r}_i - \mathbf{R}_A\rvert}
                     + \sum_{i<j} \frac{e^2}{\lvert\mathbf{r}_i - \mathbf{r}_j\rvert}
$$

The nuclear repulsion energy becomes a constant for a given geometry:

$$V_{nn} = \frac{Z_A Z_B e^2}{R}$$

For H₂ at the equilibrium bond length $R = 0.7414$ Å (= 1.401 Bohr): $V_{nn} = 0.7151$ Ha. We add this constant back at the end.

What remains is a problem in the electronic coordinates alone. Solve it for one nuclear geometry, and you get the electronic energy $E_\text{el}(R)$. Repeat for many geometries, and you trace out the **potential energy surface** — the curve that tells you bond lengths, bond angles, and vibrational frequencies.

> **Why this matters for quantum simulation:** The Born–Oppenheimer approximation is not unique to quantum computing — every classical electronic structure method uses it too. It means the quantum computer's job is to solve the electronic problem at a *fixed* geometry. If you want a potential energy surface, you run the quantum computer many times, once per geometry. (This is exactly what our H₂O bond-angle scan will do in later chapters.)

---

## Basis Sets: Making the Infinite Finite

The electronic Hamiltonian acts on wavefunctions $\Psi(\mathbf{r}_1, \mathbf{r}_2)$ — functions of continuous 3D coordinates. A computer (classical or quantum) cannot represent a continuous function exactly. We need to discretize.

The standard approach: expand each molecular orbital as a **linear combination of known functions**, called basis functions. This is the same idea as representing a vector in a finite basis, except the "vectors" are functions and the "basis" is a set of atomic orbital shapes.

### The Hydrogen Atom: Exact Solutions We Can't Use Directly

For a single hydrogen atom, the Schrödinger equation has exact solutions: the familiar $1s$, $2s$, $2p$, $3d$, … orbitals. These have the form

$$\phi_{1s}(r) \propto e^{-\zeta r}$$

where $\zeta$ determines how tightly the electron is bound. These Slater-type orbitals are physically correct, but they have a computational problem: the integrals involving products of Slater functions on *different* atomic centres cannot be evaluated in closed form.

### Gaussians: The Practical Compromise

The solution, introduced by S. F. Boys in 1950, is to replace Slater-type orbitals with sums of Gaussian functions:

$$g(r) \propto e^{-\alpha r^2}$$

The product of two Gaussians centred at different points is another Gaussian centred at a third point — a property that makes all the necessary integrals analytically tractable. The price: Gaussians decay too quickly at large $r$ and have the wrong cusp at $r = 0$. The fix: use several Gaussians to approximate each Slater orbital.

### STO-3G: The Smallest Meaningful Basis

The "Slater-Type Orbital approximated by 3 Gaussians" (STO-3G) basis set fits three Gaussians to each atomic orbital. For hydrogen, STO-3G provides one basis function per atom: an approximation to the $1s$ orbital.

Is STO-3G a good basis set? No — it is the smallest possible choice, and it captures only the crudest features of the electronic structure. Serious computational chemistry uses much larger basis sets (cc-pVDZ, cc-pVTZ, aug-cc-pVQZ, …). But STO-3G is perfect for *learning*, because it keeps the numbers small enough to track by hand while still exhibiting all the essential structure.

> **Common Mistake #2:** Confusing "basis set" with "basis states." The basis set (STO-3G) determines which *orbitals* we use. The basis states (the 6 configurations in the table below) are the many-electron states built from those orbitals. A bigger basis set gives more orbitals, which gives exponentially more configurations — and this is where the computational hardness lives.

> **What "exact" means in this book:** When we say a computation is "exact" (e.g., Full Configuration Interaction), we mean exact *within the chosen basis set*. STO-3G H₂ has only 4 spin-orbitals and 6 configurations, so FCI is trivial and gives the exact answer for that basis. But STO-3G itself is a crude approximation to the true electronic wavefunction — a larger basis set (cc-pVDZ, cc-pVTZ) would give a more accurate energy. The quantum simulation pipeline operates at the basis-set level: it solves the finite-dimensional problem exactly, but the finite-dimensional problem is only as good as the basis set that defines it. Throughout this book, "exact" always means "exact within the basis."

---

## Molecular Orbitals for H₂

With one STO-3G basis function on each hydrogen atom ($1s_A$ and $1s_B$), the Linear Combination of Atomic Orbitals (LCAO) procedure gives two molecular orbitals:

$$\sigma_g = \frac{1s_A + 1s_B}{\sqrt{2(1+S)}} \qquad \text{(bonding)}$$

$$\sigma_u = \frac{1s_A - 1s_B}{\sqrt{2(1-S)}} \qquad \text{(antibonding)}$$

where $S = \langle 1s_A \mid 1s_B \rangle$ is the overlap integral between the two atomic orbitals.

The **bonding** orbital $\sigma_g$ has its electron density concentrated *between* the nuclei — this is what holds the molecule together. The **antibonding** orbital $\sigma_u$ has a nodal plane at the midpoint — electron density here pushes the nuclei apart.

Each spatial orbital can hold one electron of each spin ($\alpha$ = spin-up, $\beta$ = spin-down), giving us 2 spatial orbitals × 2 spins = **4 spin-orbitals**:

| Index $p$ | Spatial orbital | Spin |
|:---:|:---:|:---:|
| 0 | $\sigma_g$ | $\alpha$ |
| 1 | $\sigma_g$ | $\beta$ |
| 2 | $\sigma_u$ | $\alpha$ |
| 3 | $\sigma_u$ | $\beta$ |

---

## The Six Configurations of H₂

Two electrons distributed among 4 spin-orbitals can occupy $\binom{4}{2} = 6$ distinct configurations. We write each as an occupation vector $\lvert n_0 n_1 n_2 n_3\rangle$, where $n_p \in \{0, 1\}$ indicates whether spin-orbital $p$ is occupied:

| Configuration | Occupation | Description |
|:---:|:---:|:---|
| $\lvert 1100\rangle$ | $\sigma_{g\alpha}\, \sigma_{g\beta}$ | Both electrons in the bonding orbital (ground state in Hartree–Fock) |
| $\lvert 1010\rangle$ | $\sigma_{g\alpha}\, \sigma_{u\alpha}$ | One in each orbital, same spin (triplet) |
| $\lvert 1001\rangle$ | $\sigma_{g\alpha}\, \sigma_{u\beta}$ | One in each, opposite spin |
| $\lvert 0110\rangle$ | $\sigma_{g\beta}\, \sigma_{u\alpha}$ | One in each, opposite spin |
| $\lvert 0101\rangle$ | $\sigma_{g\beta}\, \sigma_{u\beta}$ | One in each, same spin (triplet) |
| $\lvert 0011\rangle$ | $\sigma_{u\alpha}\, \sigma_{u\beta}$ | Both in antibonding orbital (highest energy) |

The **exact** ground state of H₂ is a *superposition* of some of these configurations. The Hartree–Fock approximation uses only the first ($\lvert 1100\rangle$), capturing about 99% of the ground-state energy. The remaining ~1% is the **correlation energy** — the part that arises from electron–electron interactions that a single-configuration picture misses.

This 1% sounds small. It isn't. The correlation energy determines whether a reaction happens, which isomer is more stable, and what the dissociation curve looks like. Classical methods that capture correlation (CCSD(T), full CI) scale exponentially. This is precisely the gap that quantum simulation aims to fill.

> **A hint of what's coming:** Those occupation vectors $\lvert n_0 n_1 n_2 n_3\rangle$ look exactly like qubit computational basis states $\lvert q_0 q_1 q_2 q_3\rangle$. Four spin-orbitals → four qubits. Six configurations → a 16-dimensional Hilbert space (of which 6 states have two electrons). A quantum computer can represent superpositions of these configurations natively.
>
> The mapping is less straightforward than it appears, though — we'll see why in Chapter 4.

---

## Second Quantization: A Preview

At this point we could write down the $6 \times 6$ Hamiltonian matrix in the configuration basis and diagonalize it. For H₂, that would work fine. But it wouldn't scale — for H₂O with 14 spin-orbitals, the configuration space has $\binom{14}{10} = 1001$ states, and for larger molecules the dimension grows combinatorially.

There is a more compact way to write the Hamiltonian — using **creation and annihilation operators** rather than wavefunctions. We will develop this formalism properly in Chapter 4, where we'll need it to understand encoding. For now, the key result is that the electronic Hamiltonian can be written as:

$$\hat{H} = \sum_{pq} h_{pq}\, a_p^\dagger a_q + \frac{1}{2}\sum_{pqrs} \langle pq \mid rs\rangle\, a_p^\dagger a_q^\dagger a_s a_r + V_{nn}$$

where $h_{pq}$ are **one-body integrals**, $\langle pq \mid rs\rangle$ are **two-body integrals**, and $V_{nn}$ is the nuclear repulsion constant. The operators $a_p^\dagger$ and $a_p$ create and destroy electrons in specific spin-orbitals, and they obey algebraic rules (the canonical anti-commutation relations) that automatically enforce the Pauli exclusion principle.

This is the object that the rest of the book operates on. Everything that follows — encoding, tapering, Trotterization — takes this Hamiltonian and transforms it into something a quantum computer can execute.

---

## The Numbers: H₂ Integrals in STO-3G

For H₂ in STO-3G at the equilibrium bond length, the non-zero one-body integrals (in the spatial orbital basis) are:

| Integral | Value (Hartree) | Physical meaning |
|:---:|:---:|:---|
| $h_{00}$ | $-1.2563$ | $\sigma_g$ orbital energy (kinetic + nuclear attraction) |
| $h_{11}$ | $-0.4719$ | $\sigma_u$ orbital energy |

The off-diagonal elements $h_{01} = h_{10} = 0$ by symmetry ($\sigma_g$ and $\sigma_u$ are orthogonal).

The two-body integrals are a $2 \times 2 \times 2 \times 2$ tensor — 16 elements, of which only a few are distinct by symmetry. We will develop these fully in Chapter 3, after sorting out the notation conventions in Chapter 2.

The nuclear repulsion constant is $V_{nn} = 0.7151$ Ha.

These numbers — the integrals and the nuclear repulsion — are the *output* of this chapter and the *input* to everything that follows. A classical electronic structure code (PySCF, Gaussian, ORCA) computes them from the molecular geometry and basis set. We will treat them as given.

---

## Key Takeaways

- A molecule is a collection of charged particles interacting via Coulomb's law. The ground-state energy is the lowest eigenvalue of the molecular Hamiltonian.
- The **Born–Oppenheimer approximation** fixes the nuclear positions, reducing the problem to the electronic Hamiltonian at a specific geometry.
- A **basis set** (STO-3G) discretizes the continuous orbital space into a finite set of molecular orbitals. For H₂, this gives 2 spatial orbitals → 4 spin-orbitals → 6 two-electron configurations.
- **Second quantization** rewrites the Hamiltonian in terms of creation and annihilation operators, encoding the Pauli exclusion principle through anti-commutation relations (CAR).
- The result is a Hamiltonian specified by one-body integrals $h_{pq}$, two-body integrals $\langle pq \mid rs\rangle$, and a nuclear repulsion constant $V_{nn}$. These numbers are the starting point for encoding.

## Common Mistakes

1. **Confusing basis set with basis states.** The basis set (STO-3G) determines the *orbitals*. The basis states ($\lvert 1100\rangle$, etc.) are many-electron configurations built from those orbitals. More orbitals → exponentially more configurations.

2. **Thinking the difficulty is the particle count.** The quantum difficulty is not that we have many particles — it's that the wavefunction lives in an exponentially large Hilbert space. 50 electrons in 100 spin-orbitals gives $\binom{100}{50} \approx 10^{29}$ configurations.

3. **Forgetting $V_{nn}$.** The nuclear repulsion energy is a constant, not an operator, but it must be added back to get the total energy. Many encoding tutorials drop it, leading to energies that are off by ~0.7 Ha for H₂.

## Exercises

1. **Configuration counting.** How many two-electron configurations exist for H₂O in a minimal (STO-3G) basis with 7 spatial orbitals (14 spin-orbitals, 10 electrons)? How does this compare with H₂?

2. **Born–Oppenheimer curve.** If you vary the bond length $R$ from 0.5 to 3.0 Å and compute the electronic energy $E_\text{el}(R)$ at each point, what shape does the curve $E_\text{el}(R) + V_{nn}(R)$ have? Where is the minimum? (You don't need to compute this — sketch it from physical reasoning.)

3. **Basis set scaling.** If we used the cc-pVDZ basis instead of STO-3G, hydrogen would have 5 basis functions per atom instead of 1. How many spatial orbitals, spin-orbitals, and two-electron configurations would H₂ have?

## Further Reading

- Szabo, A. and Ostlund, N. S. *Modern Quantum Chemistry: Introduction to Advanced Electronic Structure Theory.* Dover, 1996. Chapters 1–3 cover the material in this chapter at greater depth.
- Helgaker, T., Jørgensen, P., and Olsen, J. *Molecular Electronic-Structure Theory.* Wiley, 2000. The definitive reference for basis sets and integral evaluation.

---

**Next:** [Chapter 2 — The Notation Minefield](02-notation.html)
