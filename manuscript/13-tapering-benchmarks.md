# Chapter 13: Tapering Benchmarks

_Numbers, not promises. This chapter measures exactly how much tapering saves on real Hamiltonians._

## In This Chapter

- **What you'll learn:** Concrete before-and-after measurements of qubit count, term count, Pauli weight, and estimated CNOT cost for tapering on multiple test systems.
- **Why this matters:** Tapering sounds good in theory. This chapter shows exactly how good — and where the limits are.
- **Prerequisites:** Chapters 10–12 (you understand both diagonal and Clifford tapering).

---

## Methodology

For each test system, we:
1. Build the encoded Hamiltonian (JW encoding)
2. Count: qubits, terms, max weight, total CNOT cost per Trotter step
3. Select verified commuting symmetries and their physical-sector eigenvalues
4. Re-count the same metrics on the tapered Hamiltonian
5. Report the reduction

CNOT cost per Trotter step is estimated as $\sum_k 2(w_k - 1)$ where $w_k$ is the weight of term $k$.

---

## Benchmark Results

### Fully diagonal 6-qubit Hamiltonian

A synthetic Hamiltonian where all terms are I/Z only — the best case for tapering.

```fsharp
let h6 =
    [| PauliRegister("ZIIIII", Complex(0.5, 0.0))
       PauliRegister("IZIIII", Complex(-0.3, 0.0))
       PauliRegister("IIZIII", Complex(0.8, 0.0))
       PauliRegister("IIIZII", Complex(0.2, 0.0))
       PauliRegister("IIIIZI", Complex(-0.4, 0.0))
       PauliRegister("IIIIIZ", Complex(0.7, 0.0))
       PauliRegister("ZZZZII", Complex(0.1, 0.0))
       PauliRegister("IIZZZZ", Complex(-0.2, 0.0)) |]
    |> PauliRegisterSequence
```

| Metric | Before | After | Reduction |
|:---|:---:|:---:|:---:|
| Qubits | 6 | 0 | 100% |
| Terms | 8 | 1 | 88% |
| Hilbert space | 64 | 1 | 64× |
| CNOTs/step | 12 | 0 | 100% |

**Interpretation:** In the chosen sector, every qubit was fixed and the
Hamiltonian reduced to a scalar. That is a property of this diagonal toy and
sector, not a general classical/quantum classification.

### Mixed Hamiltonian (partial tapering)

A 4-qubit system where qubits 0 and 2 are diagonal but qubits 1 and 3 have X/Y terms.

```fsharp
let hmixed =
    [| PauliRegister("ZIZI", Complex(0.5, 0.0))
       PauliRegister("IXIX", Complex(-0.3, 0.0))
       PauliRegister("ZIIZ", Complex(0.2, 0.0))
       PauliRegister("IYIY", Complex(0.1, 0.0)) |]
    |> PauliRegisterSequence
```

| Metric | Before | After | Reduction |
|:---|:---:|:---:|:---:|
| Qubits | 4 | 2 | 50% |
| Terms | 4 | 4 | 0% |
| Hilbert space | 16 | 4 | 4× |
| Max weight | 2 | 2 | 0% |

**Interpretation:** Half the qubits removed, but the term count and weight stay the same — the remaining terms still have structure. The Hilbert space shrank by 4×, which is the primary gain.

### Heisenberg model (Clifford needed)

$\hat{H} = XX + YY + ZZ$ on 2 qubits. No diagonal Z₂ symmetries, but $Z_0Z_1$ is a general Z₂ generator.

| Metric | Diagonal only | Clifford |
|:---|:---:|:---:|
| Symmetries found | 0 | ≥1 |
| Qubits removed | 0 | ≥1 |

**Interpretation:** Diagonal-only tapering misses the symmetry entirely. Clifford tapering finds and uses it. This is why the general method matters.

---

## The Impact on Circuit Cost

The real payoff of tapering shows in the **CNOT staircase** (Chapter 15). Each Pauli rotation $e^{-i\theta P}$ with weight $w$ costs $2(w-1)$ CNOTs. Tapering reduces both term count and weight:

| System | Terms before | Terms after | CNOTs/step before | CNOTs/step after | Savings |
|:---|:---:|:---:|:---:|:---:|:---:|
| 6-qubit diagonal | 8 | 1 | 12 | 0 | 100% |
| 4-qubit mixed | 4 | 4 | 8 | 4 | 50% |

These savings multiply across every Trotter step. For a simulation with 1000 Trotter steps, the 50% reduction in the mixed case means 4000 fewer CNOTs total.

---

## Tapering + Encoding: How to Benchmark the Combination

Tapering and encoding choice address different aspects of circuit cost, and they **compound**:

```mermaid
flowchart LR
    F["Fermionic Hamiltonian"]
    F --> E["Choose encoding<br/>n-qubit Pauli Hamiltonian"]
    E --> T["Identify symmetries<br/>select physical sector<br/>taper to n-k qubits"]
    T --> C["Compile the reduced Hamiltonian"]
    style C fill:#d1fae5,stroke:#059669
```

The earlier H₂O CNOT table has been removed because no committed
molecule-specific input/output artifact produced it. A defensible comparison
must record, for every row:

1. geometry, basis, frozen-core and active-space choices;
2. orbital and qubit ordering plus the pinned encoding version;
3. nonzero Pauli terms before and after tapering;
4. commuting generators and the physical sector;
5. product-formula ordering and logical connectivity assumptions; and
6. generated term-weight and CNOT totals.

Worst-case ladder-operator weight is not enough to infer a full molecular
Hamiltonian's cost.

---

## Tapering Stage Complete

```mermaid
flowchart LR
    S1["The Molecule<br/>(Ch.1–3)"]
    S2["The Machine<br/>(Ch.4)"]
    S3["Encoding<br/>(Ch.5–9)"]
    S4["Tapering ✓<br/>(Ch.10–13)"]
    S5["Circuits<br/>(Ch.14–17)"]
    S1 --> S2 --> S3 --> S4 --> S5
    style S4 fill:#d1fae5,stroke:#059669
    style S5 fill:#fde68a,stroke:#d97706
```

We now have the criteria for a verified tapered Hamiltonian: a commuting
generator set, a physical sector, phase-preserving Clifford conjugation, and a
sector-spectrum parity test. The next stage turns a Hamiltonian that passes
those checks into gates.

---

## How Much Tapering Is Enough?

A natural question: should you always taper everything you can, or is there a point of diminishing returns?

The safe rule is: **use every verified symmetry whose sector you can identify and whose downstream transformations you can validate.** Unlike basis-set truncation, correct tapering introduces no approximation within that sector. But an invalid generator, a phase error, or the wrong sector changes the spectrum, and transformed observables and state preparation may add practical overhead.

The real questions are whether the generators form a valid independent commuting set, which physical sector they label, and whether the implementation preserves phases and sector spectra. Diagonal tapering (Chapter 11) catches qubits that are individually frozen. Clifford tapering (Chapter 12) can expose symmetries hidden across combinations of qubits, provided those checks pass.

Could you do more? In principle, yes. The Hamiltonian has other symmetries beyond Z₂ — particle-number conservation (U(1)), spin symmetry (SU(2)) — that could remove additional qubits. But exploiting them requires different mathematical machinery (symmetry-adapted encodings, qubit-efficient mappings) that goes beyond the stabiliser framework we've developed here. This is an active area of research; Chapter 23 touches on it briefly.

Encoding and tapering must be evaluated together. Starting from the same fermionic Hamiltonian, build each candidate encoded Hamiltonian, identify its symmetry generators, map the target quantum numbers to sector eigenvalues, taper, and only then compare circuit costs. You cannot taper a Jordan–Wigner Hamiltonian and then re-encode the reduced Pauli operator with a different fermion mapping.

---

## Key Takeaways

- Tapering reduces qubit count, Hilbert space size, and often term count and Pauli weight.
- Diagonal tapering handles the easy cases; Clifford tapering catches multi-qubit symmetries that diagonal misses.
- Taper only verified commuting symmetries in an identified physical sector; then check sector-spectrum preservation.
- Encoding and tapering compound, but tapering is performed separately after each fermion-to-qubit encoding.
- The real metric is **CNOTs per Trotter step** — tapering can reduce this by 50% or more.
- At large scale, encoding and tapering benefits must be generated separately for the stated Hamiltonian and physical sector.

## Further Reading

- Bravyi, S., Gambetta, J. M., Mezzacapo, A., and Temme, K. "Tapering off qubits to simulate fermionic Hamiltonians." arXiv:1701.08213 (2017). The original tapering paper, including the benchmarks that motivate this chapter.
- Setia, K., Bravyi, S., Mezzacapo, A., and Whitfield, J. D. "Reducing qubit requirements for quantum simulations using molecular point group symmetries." *J. Chem. Theory Comput.* 16, 6091 (2020). Extends tapering to exploit molecular point group symmetries for further qubit reductions.

---

**Previous:** [Chapter 12 — General Clifford Tapering](12-clifford-tapering.html)

**Next:** [Chapter 14 — From Hamiltonian to Time Evolution](14-time-evolution.html)
