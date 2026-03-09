# Chapter 14: Trotterization in Practice

_Chapter 13 explained why we need to break the time-evolution operator into small, implementable pieces. This chapter actually does it — and reveals something satisfying: the structure of the Hamiltonian we've been studying since Chapter 6 determines the structure of the circuit._

## In This Chapter

- **What you'll learn:** How to apply first-order and second-order Trotter decomposition to the H₂ Hamiltonian, what the rotation list looks like, how to choose a time step, and how to estimate circuit cost before generating a single gate.
- **Why this matters:** The rotation list is the bridge between the symbolic Hamiltonian (the physicist's object) and the physical circuit (the engineer's object). This is where the two worlds meet.
- **Prerequisites:** Chapter 13 (you understand the Trotter–Suzuki formula and why it's needed).

---

## What Happens When Theory Meets H₂

In Chapter 13, we developed the Trotter formula in general terms: break $e^{-i\hat{H}t}$ into a product of single-term rotations. The formula looked clean and abstract. Now let's apply it to the actual 15-term Hamiltonian we've been carrying since Chapter 6 — the one we built integral by integral, verified against eigenvalues, and (optionally) tapered.

What comes out the other end is a **rotation list** — an ordered sequence of Pauli rotations, each with:
- a **Pauli string** (the "axis" of rotation: $XXYY$, $IIZZ$, etc.)
- a **rotation angle** (the coefficient times the time step)

This list is the intermediate representation between the symbolic world (where the Hamiltonian lives) and the gate world (where the quantum computer lives). Chapter 15 will decompose each rotation into elementary gates; here, we focus on producing the list and understanding its structure.

Remember what we learned in Chapter 6: the Hamiltonian's terms split into diagonal (classical) and off-diagonal (quantum). That split carries through to the rotation list — the diagonal rotations are cheap ($Z$-only, no CNOTs), and the off-diagonal rotations are expensive ($XXYY$-type, 6 CNOTs each). Trotterization doesn't change this fundamental economics; it just makes it executable.

---

## The H₂ Hamiltonian, One Last Time

Here are our 15 terms, grouped by character:

| Group | Pauli Strings | Coefficients (Ha) | Weight | Character |
|:---:|:---|:---|:---:|:---|
| 1 | $IIII$ | $-1.0704$ | 0 | Global phase (drop) |
| 2–5 | $IIIZ$, $IIZI$, $IZII$, $ZIII$ | $\pm 0.09$ to $\pm 0.30$ | 1 | Orbital energies |
| 6–11 | $IIZZ$, $IZIZ$, $IZZI$, $ZIIZ$, $ZIZI$, $ZZII$ | $\pm 0.01$ to $\pm 0.17$ | 2 | Coulomb |
| 12–15 | $XXYY$, $XYYX$, $YXXY$, $YYXX$ | $\pm 0.1744$ | 4 | Exchange (quantum) |

The identity term ($IIII$) contributes a global phase $e^{-i(-1.0704)t}$ which has no observable effect — it shifts all eigenvalues equally. We drop it from the circuit, leaving **14 non-identity terms** to Trotterize.

### First-order Trotter with $\Delta t = 0.1$

Each term produces one Pauli rotation. The angle is $\theta_k = c_k \times \Delta t$:

```fsharp
open Encodings

let step = firstOrderTrotter 0.1 hamiltonian

printfn "Rotations: %d" step.Rotations.Length
for r in step.Rotations do
    printfn "  angle=%+.6f  Pauli=%s  weight=%d"
        r.Angle
        r.Operator.Signature
        (r.Operator.Signature |> Seq.filter (fun c -> c <> 'I') |> Seq.length)
```

The output will show 14 rotations — one per non-identity Hamiltonian term. The angles are small (all less than 0.04 in magnitude) because we chose $\Delta t = 0.1$ and the coefficients are all less than 0.35 Ha.

### What the rotation list tells us

Each entry says: "rotate the quantum state by angle $\theta$ around the axis defined by Pauli string $P$." In physical terms, this evolves the state under the influence of one term of the Hamiltonian for a short time.

The key observation from Chapter 6 carries through: the **diagonal rotations** (weights 1–2, Z-only terms) are cheap and classical. The **off-diagonal rotations** (weight 4, XXYY-type) are expensive and quantum. The Trotter decomposition separates them — each gets its own rotation — and the encoding determines how expensive each one is.

---

## Second-Order Trotter: Symmetry for Accuracy

First-order Trotter applies the rotations in one direction:

$$e^{-ic_1 P_1 \Delta t} \cdot e^{-ic_2 P_2 \Delta t} \cdot \ldots \cdot e^{-ic_L P_L \Delta t}$$

Second-order Trotter applies them forward at half-angle, then backward at half-angle:

$$\underbrace{e^{-ic_1 P_1 \Delta t/2} \cdots e^{-ic_L P_L \Delta t/2}}_{\text{forward, half angle}} \cdot \underbrace{e^{-ic_L P_L \Delta t/2} \cdots e^{-ic_1 P_1 \Delta t/2}}_{\text{reverse, half angle}}$$

```fsharp
let step2 = secondOrderTrotter 0.1 hamiltonian

printfn "Rotations: %d (vs %d for first-order)"
    step2.Rotations.Length
    step.Rotations.Length
// → 28 rotations (2 × 14)
```

The symmetrization cancels the leading-order error terms. Intuitively: the forward pass over-approximates by a small amount, and the reverse pass under-approximates by the same amount. The errors cancel, leaving a much smaller residual.

| Property | First-order | Second-order |
|:---|:---:|:---:|
| Rotations per step | $L$ (14 for H₂) | $2L$ (28 for H₂) |
| Rotation angles | $c_k \Delta t$ | $c_k \Delta t / 2$ |
| Error per step | $O(\Delta t^2)$ | $O(\Delta t^3)$ |
| Total error for $N$ steps | $O(t^2/N)$ | $O(t^3/N^2)$ |

For the same target accuracy, second-order Trotter typically needs $\sqrt{N}$ fewer steps than first-order — which more than compensates for the doubled rotation count per step.

---

## Choosing the Time Step

The time step $\Delta t$ is the key knob in Trotterization. Too large → bad approximation. Too small → too many steps → too deep a circuit.

A practical rule of thumb: $\Delta t \leq 1 / \lVert\hat{H}\rVert_1$, where the 1-norm is $\lVert\hat{H}\rVert_1 = \sum_k |c_k|$.

For H₂: $\lVert\hat{H}\rVert_1 \approx 3.7$ Ha, giving $\Delta t \lesssim 0.27$. Our choice of $\Delta t = 0.1$ is comfortably within this bound.

For H₂O (14 spin-orbitals, or about 11 qubits after tapering): $\lVert\hat{H}\rVert_1$ is larger (~30 Ha), so $\Delta t$ must be smaller — around $0.03$. This means more Trotter steps per unit time, which means more CNOTs. This is another reason larger molecules are harder to simulate.

---

## Quick CNOT Estimate (Before Gate Decomposition)

We can estimate the CNOT cost without actually building the gate circuit, using the formula from Chapter 4:

$$\text{CNOTs per Trotter step} = \sum_{k=1}^{L} 2(w_k - 1)$$

```fsharp
let cnots = trotterCnotCount step
printfn "Estimated CNOTs per first-order step: %d" cnots
```

For H₂ (14 non-identity terms):

| Term type | Count | Weight | CNOTs each | Subtotal |
|:---:|:---:|:---:|:---:|:---:|
| Single-Z ($IIIZ$, etc.) | 4 | 1 | 0 | 0 |
| Double-Z ($IIZZ$, etc.) | 6 | 2 | 2 | 12 |
| Exchange ($XXYY$, etc.) | 4 | 4 | 6 | 24 |
| **Total** | **14** | — | — | **36** |

36 CNOTs per first-order Trotter step. Second-order: 72. For a 100-step simulation: 3,600 (first-order) or 7,200 (second-order) CNOTs total.

These are small numbers — easily within reach of current hardware for H₂. For H₂O with ~600 terms and higher weights, the numbers grow into the thousands per step — which is where encoding choice and tapering make the difference between feasible and infeasible.

---

## How Many Trotter Steps Do I Need?

The Trotter approximation introduces error. How much error, and how many steps are needed to keep it below a target $\epsilon$?

### The error bound

For first-order Trotter with $N$ steps of size $\Delta t = t/N$, the total error is bounded by:

$$\epsilon_\text{Trotter} \leq \frac{t^2}{2N} \sum_{j < k} \lVert [c_j P_j,\; c_k P_k] \rVert$$

The commutator sum $\Lambda = \sum_{j<k} \lVert [c_j P_j, c_k P_k] \rVert$ measures how "badly" the terms fail to commute. If all terms commuted, $\Lambda = 0$ and there would be no Trotter error at all — the product formula would be exact.

For the second-order formula, the error bound improves to:

$$\epsilon_\text{Trotter} \leq \frac{t^3}{12N^2} \sum_{j,k,l} \lVert [[c_j P_j, c_k P_k], c_l P_l] \rVert$$

In practice, practitioners use a simpler (looser) bound: the 1-norm $\lVert H \rVert_1 = \sum_k |c_k|$ provides a conservative estimate:

$$N \geq \frac{t^2 \lVert H \rVert_1^2}{2\epsilon} \quad \text{(first-order)} \qquad N \geq t \sqrt{\frac{t \lVert H \rVert_1^3}{12\epsilon}} \quad \text{(second-order)}$$

### Worked example: H₂ at chemical accuracy

For H₂: $\lVert H \rVert_1 \approx 3.7$ Ha. Target precision: $\epsilon = 1.6 \times 10^{-3}$ Ha (chemical accuracy). Total evolution time: $t = 1$ (one unit of atomic time).

**First-order:**

$$N \geq \frac{(1)^2 \times (3.7)^2}{2 \times 0.0016} = \frac{13.69}{0.0032} \approx 4{,}278 \text{ steps}$$

At 36 CNOTs per step: ~154,000 total CNOTs. That's conservative — the actual commutator-based bound is much tighter because many H₂ terms commute.

**Second-order:**

$$N \geq 1 \times \sqrt{\frac{1 \times (3.7)^3}{12 \times 0.0016}} = \sqrt{\frac{50.65}{0.0192}} \approx \sqrt{2{,}638} \approx 52 \text{ steps}$$

At 72 CNOTs per step: ~3,700 total CNOTs. This is why second-order Trotter is the standard choice — the step count drops by nearly 100×.

### The practical message

For H₂, even the conservative estimate says ~52 second-order steps suffice for chemical accuracy — about 3,700 CNOTs. For H₂O ($\lVert H \rVert_1 \approx 30$ Ha), the step count grows and the per-step cost is higher, pushing the total to ~500,000 CNOTs. These numbers match the resource estimates in Chapter 19.

The tighter commutator-based bounds (Childs and Su, PRL 2019) typically improve on the 1-norm estimate by a factor of 5–50×, because real molecular Hamiltonians have significant commuting structure. But the 1-norm bound is safe, easy to compute, and gives the right order of magnitude.

---

## Key Takeaways

- A Trotter step converts a Hamiltonian into a list of **Pauli rotations** — each with a Pauli string and an angle.
- First-order: $L$ rotations. Second-order: $2L$ rotations at half angle, with quadratically better error scaling.
- The time step $\Delta t$ is bounded by $1/\lVert H \rVert_1$ — larger Hamiltonians need smaller steps.
- CNOT cost is estimable from Pauli weights alone: $\sum_k 2(w_k - 1)$.
- For H₂: 36 CNOTs per first-order step. Feasible. For H₂O: thousands. That's where optimization matters.

## Common Mistakes

1. **Including the identity term.** The $IIII$ term contributes only a global phase — include it and you waste one rotation per step on something unobservable.

2. **Using first-order Trotter for production.** First-order is fine for learning but second-order is almost always better in practice — the error improvement outweighs the doubled rotation count.

3. **Choosing $\Delta t$ too large.** If $\Delta t \cdot \lVert H \rVert_1 > 1$, the Trotter approximation breaks down and the circuit does not approximate the correct time evolution.

## Exercises

1. **Rotation count.** For H₂O with 600 non-identity terms, how many rotations does a single second-order Trotter step have?

2. **Time step.** If $\lVert H \rVert_1 = 30$ Ha and you want $\Delta t = 0.01$, how many Trotter steps do you need for total evolution time $t = 1$?

3. **CNOT scaling.** If every term in a Hamiltonian has weight 5 (ternary tree on a 27-orbital system), and there are 200 non-identity terms, what is the CNOT count per first-order step?

---

**Previous:** [Chapter 13 — From Hamiltonian to Time Evolution](13-time-evolution.html)

**Next:** [Chapter 15 — The CNOT Staircase](15-cnot-staircase.html)
