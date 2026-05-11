# Chapter 22: Scaling — From H₂ to FeMo-co

_H₂ was our teacher. H₂O was our first real test. Now we look at where the pipeline goes — and where it meets its limits._

## In This Chapter

- **What you'll learn:** How the pipeline's cost scales from small test systems through medium molecules to the grand challenge of quantum chemistry, and where encoding choice makes — or breaks — the simulation.
- **Why this matters:** The promise of quantum simulation is that it handles molecules classical computers cannot. This chapter asks: at what point does that promise become real, and what does the hardware need to deliver?
- **Prerequisites:** Chapters 17–19 (cost analysis, pipeline, bond angle scan).

---

## The Scaling Landscape

Every quantity in our pipeline grows with the number of spin-orbitals $n$:

| Quantity | Growth | H₂ ($n{=}4$) | H₂O ($n{=}14$) | N₂ ($n{=}20$) | FeMo-co ($n{\approx}108$) |
|:---|:---|:---:|:---:|:---:|:---:|
| Qubits | $n$ | 4 | 14 | 20 | ~108 |
| Hamiltonian terms | $O(n^4)$ | 15 | ~600 | ~2,000 | ~$10^7$ |
| JW max weight | $n$ | 4 | 14 | 20 | 108 |
| TT max weight | $O(\log_3 n)$ | 3 | 4 | 5 | ~5 |
| Configurations (FCI) | $\binom{n}{n_e}$ | 6 | 1,001 | 38,760 | ~$10^{30}$ |

The configuration count is why classical methods fail. Full CI (exact diagonalisation) scales as $\binom{n}{n_e}$ — the number of ways to place $n_e$ electrons in $n$ spin-orbitals. For FeMo-co, that's roughly $10^{30}$ determinants. No classical computer will ever enumerate them.

A quantum computer doesn't enumerate configurations — it represents the quantum state directly in $n$ qubits. The cost is in the *circuit*, not the *state space*. And the circuit cost depends on the encoding.

---

## The Encoding Crossover

At 4 qubits, the encoding choice barely matters — Chapter 17 shows all encodings produce similar CNOT counts (JW: 36, TT: 40). At 12+ qubits, the differences become dramatic. The following table shows first-order Trotter CNOT counts *without tapering* — the raw encoding comparison:

| Molecule | $n$ | JW CNOTs/step | TT CNOTs/step | Ratio |
|:---|:---:|:---:|:---:|:---:|
| H₂ | 4 | 36 | 40 | 0.9× |
| LiH | 12 | ~1,200 | ~500 | 2.4× |
| H₂O (frozen core) | 12 | ~1,800 | ~600 | 3.0× |
| N₂ | 20 | ~5,000 | ~1,200 | 4.2× |
| FeMo-co | ~108 | ~$10^7$ | ~$10^5$ | ~100× |

The ratio grows because JW's Pauli weights grow linearly while TT's grow logarithmically. At FeMo-co scale, the difference is roughly two orders of magnitude for the *logical* (pre-transpilation) circuit. After hardware-specific transpilation (qubit routing, native gate decomposition), the absolute gate counts increase for all encodings, but the *relative* advantage of lighter Pauli weights is preserved.

> **Caveat on the estimates:** The CNOT counts for H₂, LiH, and H₂O are computed values from FockMap (matching the Chapter 17 tables). The estimates for N₂ and FeMo-co are order-of-magnitude projections based on scaling trends. All entries are for *unoptimised logical circuits*; hardware transpilers typically reduce gate counts by 20–40% through gate cancellation and commutation.

### Why the Ratio Matters

On near-term hardware, each CNOT gate has a finite error rate — typically 0.1–1%. The probability that a circuit executes correctly drops exponentially with the CNOT count:

$$P_\text{success} \approx (1 - \varepsilon)^{C_\text{CNOT}}$$

At $\varepsilon = 0.5\%$ and 1,800 CNOTs (JW for H₂O, untapered), $P_\text{success} \approx 0.01\%$. At 600 CNOTs (TT for H₂O, untapered), $P_\text{success} \approx 5\%$. Under these typical assumptions, that's roughly a 500× improvement in success probability from encoding choice alone — before tapering, before error mitigation, before hardware improvements. (In practice, tapering reduces both CNOT counts further, and error mitigation techniques like zero-noise extrapolation can partially compensate for low success rates.)

This is why the chapters on encoding (5–9), tapering (10–13), and cost analysis (17) matter. They're not academic exercises. They directly determine whether a simulation succeeds or fails on real hardware.

---

## The Tapering Dividend

Tapering compounds with encoding choice. At each system size, tapering removes $k$ qubits and often reduces the term count:

| Molecule | $n$ | Tapered $n{-}k$ | Terms (before) | Terms (after) |
|:---|:---:|:---:|:---:|:---:|
| H₂ | 4 | 2 | 15 | 5 |
| H₂O | 14 | ~11 | ~600 | ~300 |
| N₂ | 20 | ~16 | ~2,000 | ~1,200 |

Fewer terms means fewer Pauli rotations per Trotter step. Combined with lower Pauli weights from a good encoding, the circuit shrinks multiplicatively. The optimisation stack from Chapter 17 — encoding + tapering + Trotter order — compounds at every scale.

---

## Application: Encoding Choice at H₂O Scale

With 14 spin-orbitals (no frozen core), H₂O is the smallest molecule where encoding choice makes a practical difference. Here's the comparison *after tapering* (14→~11 qubits):

| Encoding | Tapered qubits | Max weight | CNOTs/step | Depth estimate |
|:---|:---:|:---:|:---:|:---:|
| Jordan–Wigner | ~11 | 11 | ~1,100 | ~2,200 |
| Bravyi–Kitaev | ~11 | 5 | ~500 | ~1,000 |
| Parity | ~11 | 11 | ~1,100 | ~2,200 |
| Binary Tree | ~11 | 5 | ~450 | ~900 |
| Ternary Tree | ~11 | 4 | ~380 | ~760 |
| Vlasov Tree | ~11 | 4 | ~380 | ~760 |

The ~3× reduction from JW to TT (tapered) is the difference between a circuit that fries on near-term hardware and one that might just survive. On a device with ~99.5% two-qubit gate fidelity, the TT circuit has roughly a 30× higher success probability per shot — $(0.995)^{380} \approx 15\%$ vs. $(0.995)^{1100} \approx 0.4\%$. Over millions of VQE shots, that translates directly to better energy estimates.

This is the practical answer to "which encoding should I use?" — at H₂O scale and beyond, ternary tree with tapering is the best option in the FockMap toolkit.

---

## The Grand Challenge: FeMo-co

The iron-molybdenum cofactor (FeMo-co) of nitrogenase is the molecule that launched a field. It catalyses nitrogen fixation — converting atmospheric N₂ to ammonia — and understanding its mechanism could transform fertiliser production, one of the most energy-intensive industrial processes on Earth.

FeMo-co has ~54 active electrons in ~108 active spin-orbitals (following Reiher et al., *PNAS* 114, 7555, 2017). Classical methods cannot accurately compute its electronic structure because the iron centres are **strongly correlated**: many electron configurations contribute comparably to the ground state, defeating perturbation theory and single-reference methods like coupled cluster.

### The Numbers

| Quantity | Value |
|:---|:---|
| Active electrons | ~54 |
| Active spin-orbitals | ~108 |
| Qubits (JW) | ~108 |
| JW max Pauli weight | 108 |
| TT max Pauli weight | ~5 |
| JW CNOTs per worst-case rotation | 214 |
| TT CNOTs per worst-case rotation | 8 |
| Ratio | 27× |

At 108 qubits, the JW encoding produces Pauli strings where nearly every qubit participates. Each off-diagonal rotation requires a CNOT staircase spanning the entire register. The ternary tree encoding compresses the worst-case weight to ~5 — a chain of 4 CNOTs per direction.

### How Far Away Is the Hardware?

For FeMo-co at chemical accuracy via QPE:
- **Qubits needed**: ~108 system + ~10 ancilla ≈ 120 logical qubits
- **Trotter steps**: ~$10^4$ (for the required precision)
- **CNOTs per Trotter step** (TT): ~$10^5$
- **Total CNOTs**: ~$10^9$

At current error rates, this requires full quantum error correction — perhaps 1,000–10,000 physical qubits per logical qubit, depending on the code and hardware (see Gidney and Fowler, *Quantum* 5, 433, 2021, for concrete surface-code estimates). That puts the total physical qubit count at $10^5$–$10^6$.

No device available today can do this. But the same pipeline we developed for H₂ (4 qubits, 15 terms, 12 CNOTs) is the *same code* that would generate the FeMo-co circuit. The bottleneck is hardware, not software. When the hardware arrives, the pipeline is ready.

---

## Where Classical Methods Still Win

It's worth being honest about the landscape. Quantum simulation has a theoretical advantage for strongly correlated systems, but classical methods are formidable:

- **Density functional theory (DFT)** handles hundreds of atoms routinely. It's not exact, but it's remarkably accurate for weakly correlated systems and costs $O(n^3)$.
- **Coupled cluster CCSD(T)** — the "gold standard" of quantum chemistry — handles systems of tens of atoms (depending on basis set) with chemical accuracy for single-reference systems. Local approximations extend this to hundreds of atoms, but canonical CCSD(T) scales as $O(N^7)$.
- **DMRG and tensor network methods** exploit the entanglement structure of 1D and quasi-1D systems, often matching quantum simulation accuracy for those geometries.

Quantum simulation's niche is the strongly correlated regime: transition-metal complexes, open-shell systems, conical intersections, and exotic electronic states where no classical method converges reliably. FeMo-co is the poster child, but the real impact may be in catalysis design, high-temperature superconductors, and photochemistry — areas where electron correlation defies classical approximation.

---

## Key Takeaways

- Circuit cost grows with system size, but **encoding choice** determines the growth rate: linear (JW) vs logarithmic (TT) Pauli weight.
- The **encoding crossover** becomes meaningful around 12–14 qubits — exactly the range of near-term quantum hardware.
- **Tapering compounds** with encoding choice, reducing both qubit count and term count multiplicatively.
- **FeMo-co** (~108 qubits) is the grand challenge: classically intractable, but requiring fault-tolerant hardware that doesn't yet exist.
- The same pipeline code works at every scale — from H₂ to FeMo-co. The bottleneck is hardware, not software.

## Further Reading

- Reiher, M., Wiebe, N., Svore, K. M., Wecker, D., and Troyer, M. "Elucidating Reaction Mechanisms on Quantum Computers." *PNAS* 114, 7555 (2017). The landmark resource estimate for simulating FeMo-co on a fault-tolerant quantum computer.
- Lee, J., Berry, D. W., Gidney, C., Huggins, W. J., McClean, J. R., Wiebe, N., and Babbush, R. "Even More Efficient Quantum Computations of Chemistry Through Tensor Hypercontraction." *PRX Quantum* 2, 030305 (2021). Dramatically reduces the resource estimates for large-molecule simulation using tensor factorisation techniques.

---

**Previous:** [Chapter 21 — Speaking the Hardware's Language](21-circuit-export.html)

**Next:** [Chapter 23 — What Comes Next](23-whats-next.html)
