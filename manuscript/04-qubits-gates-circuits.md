# Chapter 4: The Quantum Computer's Vocabulary

_We've spent three chapters learning what the molecule wants to say. Now we need to learn what the quantum computer can hear._

## In This Chapter

- **What you'll learn:** What qubits are and why they're powerful, what operations you can perform on them, why creating entanglement is the hard part, and why Pauli operators and CNOT gates are sufficient for everything we need.
- **Why this matters:** The encoding (next chapter) translates fermionic operators into qubit operations. If you don't understand the target language, you can't evaluate the translation.
- **Prerequisites:** Chapters 1–3 (you have the molecular Hamiltonian). Basic familiarity with 2×2 matrices is helpful but not essential.

---

## Two Worlds, One Problem

At this point in the story, we have two things that don't talk to each other.

On the **chemistry side**: a Hamiltonian written as a polynomial in creation and annihilation operators, $\hat{H} = \sum h_{pq} a_p^\dagger a_q + \ldots$, parameterized by molecular integrals. Finding its ground-state energy requires exploring an exponentially large space of electron configurations.

On the **computing side**: a quantum processor made of qubits — two-level quantum systems that can be in superposition and can be entangled with each other.

The promise of quantum simulation is that the second can solve the first. But to see *how*, we need to understand what a quantum computer can actually do — not in the abstract "it tries all possibilities at once" sense (which is wrong), but concretely: what are the elementary operations, what do they cost, and are they enough?

---

## Qubits: The Power of Superposition

A classical bit is 0 or 1. End of story.

A **qubit** can be in any superposition of $\lvert 0\rangle$ and $\lvert 1\rangle$:

$$\lvert\psi\rangle = \alpha\lvert 0\rangle + \beta\lvert 1\rangle$$

where $\alpha$ and $\beta$ are complex amplitudes with $\lvert\alpha\rvert^2 + \lvert\beta\rvert^2 = 1$. When you measure the qubit, you get 0 with probability $\lvert\alpha\rvert^2$ and 1 with probability $\lvert\beta\rvert^2$. The superposition collapses — you can't read out both amplitudes.

This seems limiting: if measurement destroys the superposition, what good is it? The answer is **interference**. By carefully manipulating the amplitudes before measurement, you can arrange for wrong answers to cancel (destructive interference) and correct answers to reinforce (constructive interference). This is how every quantum algorithm works — not by trying all possibilities, but by engineering interference to amplify the right answer.

For our purposes, the key fact is this: an $n$-qubit register stores $2^n$ complex amplitudes. For 4 qubits: 16 amplitudes — matching the Hilbert space of our H₂ problem. For 100 qubits: $2^{100} \approx 10^{30}$ amplitudes. A classical computer cannot even *store* this state, let alone manipulate it. A quantum computer encodes it naturally in the physical state of 100 two-level systems.

---

## Operations on a Single Qubit

If a qubit is a vector in a 2D complex space, an operation on a qubit is a $2 \times 2$ unitary matrix. The universe of single-qubit operations is rich — any unitary matrix is a valid gate — but a few show up constantly:

**The Pauli operators** — you've already met them from the chemistry side (Chapter 1's Hamiltonian lives in Pauli space after encoding). Now meet them as gates:

- **$X$** (bit-flip): swaps $\lvert 0\rangle$ and $\lvert 1\rangle$. The quantum analogue of a NOT gate.
- **$Z$** (phase-flip): leaves $\lvert 0\rangle$ alone, applies a minus sign to $\lvert 1\rangle$. This is a purely quantum operation — it changes the *phase* of the superposition without changing the measurement probabilities.
- **$Y$** = $iXZ$: both a bit-flip and a phase-flip.

Why are the Paulis special? Because they form a **basis** for all $2 \times 2$ Hermitian matrices. Any single-qubit operator — any observable, any Hamiltonian term — can be written as a linear combination of $I$, $X$, $Y$, $Z$. This is why the encoded Hamiltonian is a sum of Pauli strings: it's the natural basis for qubit operators, just as $\{\lvert 0\rangle, \lvert 1\rangle\}$ is the natural basis for qubit states.

**The Hadamard gate** ($H$) creates superposition from a definite state:

$$H\lvert 0\rangle = \frac{\lvert 0\rangle + \lvert 1\rangle}{\sqrt{2}}, \qquad H\lvert 1\rangle = \frac{\lvert 0\rangle - \lvert 1\rangle}{\sqrt{2}}$$

This is the gate that takes a classical bit and makes it quantum. Without $H$ (or something equivalent), you could never create superpositions and quantum computing would reduce to classical computing.

**Rotation gates** ($R_z(\theta)$, $R_x(\theta)$) rotate the qubit's state by a continuous angle $\theta$. These are the gates that implement the actual physics: the Hamiltonian's coefficients (like $-0.1744$ Ha for the exchange terms) become rotation angles in the circuit.

On current quantum processors — IBM's Eagle and Heron chips (superconducting transmon qubits), IonQ's Aria (trapped ytterbium ions), Quantinuum's H-series (trapped calcium ions) — single-qubit gates take 20–100 nanoseconds with error rates around $10^{-4}$. They are the cheapest thing a quantum computer does. For cost accounting, we treat them as essentially free; the expensive operations are the two-qubit gates we're about to meet.

---

## The Hard Part: Entanglement

Single-qubit operations are not enough. If each qubit could only be manipulated independently, a quantum computer would be nothing more than $n$ independent coin-flips — powerful for random sampling, useless for computation.

The power of quantum computing comes from **entanglement**: correlations between qubits that have no classical analogue.

Consider two qubits in the state $\frac{1}{\sqrt{2}}(\lvert 00\rangle + \lvert 11\rangle)$. If you measure the first qubit and get 0, the second qubit is *guaranteed* to be 0 — even though, before measurement, neither qubit had a definite value. This correlation cannot be explained by any shared classical information prepared in advance (this is the content of Bell's theorem, which we won't prove but which decades of experiments have confirmed).

Entanglement is what allows a quantum computer to explore the exponentially large Hilbert space *coherently* — maintaining phase relationships between configurations rather than just sampling them independently. And it is what allows the correlation energy (Chapter 5's off-diagonal coherences in the density matrix) to be represented on quantum hardware.

### The CNOT gate: the entanglement maker

The **controlled-NOT** (CNOT) gate is the simplest operation that creates entanglement. It acts on two qubits — a *control* and a *target* — and flips the target if the control is $\lvert 1\rangle$:

$$\text{CNOT}\lvert 00\rangle = \lvert 00\rangle, \quad \text{CNOT}\lvert 01\rangle = \lvert 01\rangle, \quad \text{CNOT}\lvert 10\rangle = \lvert 11\rangle, \quad \text{CNOT}\lvert 11\rangle = \lvert 10\rangle$$

On its own, CNOT doesn't look quantum — it's just a conditional flip. But apply it after a Hadamard:

$$\lvert 00\rangle \xrightarrow{H \otimes I} \frac{\lvert 0\rangle + \lvert 1\rangle}{\sqrt{2}} \otimes \lvert 0\rangle \xrightarrow{\text{CNOT}} \frac{\lvert 00\rangle + \lvert 11\rangle}{\sqrt{2}}$$

Two gates — one Hadamard, one CNOT — and we've created a maximally entangled state from a product state. No classical operation can do this.

### Why CNOT is expensive

On physical quantum processors, the CNOT gate is the bottleneck. Here's what it actually costs on current hardware:

| Platform | Vendor | CNOT time | CNOT error rate | Single-qubit error rate |
|:---|:---|:---:|:---:|:---:|
| Superconducting transmon | IBM (Heron) | ~300 ns | ~$5 \times 10^{-3}$ | ~$3 \times 10^{-4}$ |
| Trapped ion | Quantinuum (H2) | ~2 ms | ~$2 \times 10^{-3}$ | ~$10^{-5}$ |
| Trapped ion | IonQ (Forte) | ~600 μs | ~$4 \times 10^{-3}$ | ~$3 \times 10^{-4}$ |

The pattern: CNOT is **10–100× slower** and **10–100× noisier** than single-qubit gates, regardless of the hardware platform. The physics is different in each case — microwave cross-resonance pulses for superconducting qubits, laser-driven Mølmer–Sørensen interactions for trapped ions — but the engineering reality is the same: making two quantum systems interact controllably is harder than manipulating one.

There is also a **connectivity constraint**. On many architectures (especially superconducting), CNOT is only available between physically adjacent qubits. If you need to entangle qubit 0 with qubit 15, you must first "swap" the quantum state through the intervening qubits — and each swap costs 3 CNOTs.

> **If you don't have access to quantum hardware**, you can still run everything in this book using a **quantum simulator**. The [Quokka](https://www.quokkacomputing.com/) (by Eigensystems, co-founded by Dr Chris Ferrie at UTS) is a dedicated quantum simulation appliance — available as a standalone device or accessible over the web — that can simulate up to ~30 qubits. That's more than enough for H₂ (4 qubits), H₂O (12 qubits), and most textbook-scale problems. IBM's Qiskit Aer and Google's Cirq also provide free software simulators. The circuits we generate in Stage 5 can target any of these backends.
>
> The CNOT cost analysis is still relevant for simulated circuits: it tells you how the circuit *would* perform on real hardware, and whether your molecule is within reach of current or near-term quantum processors.

> **The bottom line:** Single-qubit gates are cheap. CNOT is expensive. Circuit cost ≈ CNOT count. Everything else follows from this.

---

## Is This Enough? Universality

A natural question: are Pauli gates and CNOT sufficient for *all* quantum computation? Or do we need exotic gates that we haven't mentioned?

The answer is yes, they are sufficient — and this is a theorem.

Any unitary operation on $n$ qubits can be decomposed into a sequence of:
- **Single-qubit rotations** ($R_x(\theta)$, $R_y(\theta)$, $R_z(\theta)$ for arbitrary $\theta$)
- **CNOT gates**

This is called **universality**. The set $\{R_z, R_x, \text{CNOT}\}$ (or equivalently $\{H, T, \text{CNOT}\}$, where $T$ is a $\pi/8$ rotation) can approximate any quantum operation to arbitrary precision. Every quantum algorithm — Shor's, Grover's, VQE, QPE — can be compiled down to these elementary gates.

For quantum simulation specifically, we need even less. The Hamiltonian is a sum of Pauli strings, and each Pauli string's time evolution $e^{-i\theta P}$ decomposes into:
- A few single-qubit gates (to rotate into the right basis)
- A chain of CNOTs (to entangle the relevant qubits)
- One $R_z$ rotation (to apply the phase)
- The reverse of the above (to undo the basis change and entanglement)

This decomposition — the **CNOT staircase** — is the circuit that appears for every term in the Hamiltonian. Its cost is $2(w-1)$ CNOT gates, where $w$ is the Pauli weight of the term. We will develop it in full detail in Stage 4 (Trotterization).

For now, the key formula is:

$$\boxed{\text{CNOTs per Pauli rotation} = 2(w - 1)}$$

This single equation connects everything: the molecular integrals (which determine the Hamiltonian terms), the encoding (which determines the Pauli weight $w$), and the circuit cost (which determines whether the simulation is feasible).

---

## The Cost Table

To make this concrete:

| Pauli weight $w$ | CNOTs | Example |
|:---:|:---:|:---|
| 1 | 0 | Single $Z$ — just an $R_z$ |
| 2 | 2 | $ZZ$ — one CNOT, one $R_z$, one reverse CNOT |
| 4 | 6 | $XXYY$ — the H₂ exchange terms |
| 12 | 22 | Typical JW term for H₂O |
| 100 | 198 | JW term for FeMo-co — infeasible |
| 5 | 8 | Same FeMo-co term under ternary tree — feasible |

The last two rows are the entire argument for encoding choice, in two numbers.

---

## Key Takeaways

- A **qubit** stores a superposition of $\lvert 0\rangle$ and $\lvert 1\rangle$. An $n$-qubit register stores $2^n$ amplitudes — an exponentially large state space.
- **Pauli operators** ($I$, $X$, $Y$, $Z$) form a basis for all single-qubit operations. Every Hamiltonian term can be written as a Pauli string.
- **CNOT** is the gate that creates entanglement — and it is 10–100× more expensive than single-qubit gates. Circuit cost ≈ CNOT count.
- The set $\{R_z, R_x, \text{CNOT}\}$ is **universal**: it can implement any quantum operation, including everything we need for quantum simulation.
- The **CNOT staircase** decomposes a Pauli rotation into $2(w-1)$ CNOTs, where $w$ is the Pauli weight. This is the conversion factor from encoding choice to circuit cost.

> **Multi-qubit states and tensor products:** An $n$-qubit state is described by the **tensor product** (Kronecker product) of individual qubit states: $\lvert\psi\rangle = \lvert q_0\rangle \otimes \lvert q_1\rangle \otimes \cdots \otimes \lvert q_{n-1}\rangle$. A Pauli string like $X_0 Z_1 I_2$ means "apply $X$ to qubit 0, $Z$ to qubit 1, and $I$ to qubit 2" — formally, the $4 \times 4 \times 2 = 8$-dimensional matrix $X \otimes Z \otimes I$. When we say two operators "commute" ($[A, B] = AB - BA = 0$) or "anti-commute" ($\{A, B\} = AB + BA = 0$), we mean these tensor-product matrices. The encoding problem (next chapter) is precisely the challenge of translating anti-commuting fermionic operators into operators built from commuting qubit tensor products.

## Common Mistakes

1. **"A quantum computer tries all possibilities at once."** It doesn't. It maintains a superposition and uses interference to amplify the right answer. The exponential speedup comes from interference, not parallelism.

2. **Counting all gates instead of CNOT gates.** Single-qubit gates are effectively free on modern hardware. Only CNOTs matter for feasibility.

3. **Forgetting that CNOT requires physical connectivity.** On most architectures, you can only CNOT adjacent qubits. Distant interactions cost additional swaps (3 CNOTs each).

## Exercises

1. **Bell state.** Starting from $\lvert 00\rangle$, apply $H$ to qubit 0, then CNOT(0→1). Write out the resulting state. Why is it entangled? (Hint: can you write it as a product $\lvert\psi_1\rangle \otimes \lvert\psi_2\rangle$?)

2. **CNOT cost.** A Hamiltonian has 100 terms. Under JW, 20 of them have weight 50 and the rest have weight 2. Under ternary tree, the 20 heavy terms have weight 5 and the rest still have weight 2. What are the total CNOT counts per Trotter step for each encoding?

3. **Universality.** Why can't you build a useful quantum computer using only single-qubit gates? What computation would it be equivalent to?

## Further Reading

- Nielsen, M. A. and Chuang, I. L. *Quantum Computation and Quantum Information.* Cambridge UP, 2000. The standard reference — we've barely scratched the surface.
- Preskill, J. "Quantum Computing in the NISQ Era and Beyond." *Quantum* 2, 79 (2018). Why gate count matters on near-term hardware.
- Mermin, N. D. *Quantum Computer Science: An Introduction.* Cambridge UP, 2007. A gentler introduction to the gate model, written for computer scientists.

---

**Previous:** [Chapter 3 — From Spatial to Spin-Orbital Integrals](03-spin-orbitals.html)

**Next:** [Chapter 5 — A Visual Guide to Encodings](05-visual-encodings.html)
