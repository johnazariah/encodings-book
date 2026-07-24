# Chapter 15: Trotterization in Practice

_Chapter 14 explained why we need to break the time-evolution operator into small, implementable pieces. This chapter actually does it — and reveals something satisfying: the structure of the Hamiltonian we've been studying since Chapter 6 determines the structure of the circuit._

## In This Chapter

- **What you'll learn:** How to apply first-order and second-order Trotter decomposition to the H₂ Hamiltonian, what the rotation list looks like, how to choose a time step, and how to estimate circuit cost before generating a single gate.
- **Why this matters:** The rotation list is the bridge between the symbolic Hamiltonian (the physicist's object) and the physical circuit (the engineer's object). This is where the two worlds meet.
- **Prerequisites:** Chapter 14 (you understand the Trotter–Suzuki formula and why it's needed).

---

## What Happens When Theory Meets H₂

In Chapter 14, we developed the Trotter formula in general terms: break $e^{-i\hat{H}t}$ into a product of single-term rotations. The formula looked clean and abstract. Now let's apply it to the actual 15-term Hamiltonian we've been carrying since Chapter 6 — the one we built integral by integral, verified against eigenvalues, and (optionally) tapered.

What comes out the other end is a **rotation list** — an ordered sequence of Pauli rotations, each with:
- a **Pauli string** (the "axis" of rotation: $XXYY$, $IIZZ$, etc.)
- a **rotation angle** (the coefficient times the time step)

This list is the intermediate representation between the symbolic world (where the Hamiltonian lives) and the gate world (where the quantum computer lives). Chapter 16 will decompose each rotation into elementary gates; here, we focus on producing the list and understanding its structure.

Chapter 6 separated diagonal configuration energies from off-diagonal
configuration couplings. In the JW rotation list, weight-1/2 Z terms use fewer
standard-staircase CNOTs than the weight-4 coupling terms. This is a
representation-specific logical cost statement.

---

## The H₂ Hamiltonian, One Last Time

Here are our 15 terms, grouped by character:

| Group | Pauli Strings | Coefficients (Ha) | Weight | Character |
|:---:|:---|:---|:---:|:---|
| 1 | $IIII$ | $-0.8121706072$ | 0 | Energy shift |
| 2–5 | $IIIZ$, $IIZI$, $IZII$, $ZIII$ | $-0.2234315369$ to $+0.1714128264$ | 1 | Diagonal |
| 6–11 | $IIZZ$, $IZIZ$, $IZZI$, $ZIIZ$, $ZIZI$, $ZZII$ | $+0.1206252348$ to $+0.1744128761$ | 2 | Diagonal |
| 12–15 | $XXYY$, $XYYX$, $YXXY$, $YYXX$ | $\pm 0.0453026155$ | 4 | Configuration coupling |

The identity term contributes a global phase $e^{+i0.8121706072t}$. It can be
dropped when simulating observables, leaving 14 non-identity rotations. For QPE,
that energy shift must still be tracked and restored when converting phase to an
absolute energy.

### First-order Trotter with $\Delta t = 0.1$

Each term produces one Pauli rotation. The angle is $\theta_k = c_k \times \Delta t$:

```fsharp
open Encodings
open Encodings.Trotterization

let step = firstOrderTrotter 0.1 hamiltonian

printfn "Rotations: %d" step.Rotations.Length
for r in step.Rotations do
    printfn "  angle=%+.6f  Pauli=%s  weight=%d"
        r.Angle
        r.Operator.Signature
        (r.Operator.Signature |> Seq.filter (fun c -> c <> 'I') |> Seq.length)
```

The output will show 14 rotations — one per non-identity Hamiltonian term. At
$\Delta t=0.1$, the largest $\lvert c_k\Delta t\rvert$ is about $0.0223$.

### What the rotation list tells us

Each entry says: "rotate the quantum state by angle $\theta$ around the axis
defined by Pauli string $P$." The weight-1 and weight-2 Z rotations need fewer
entangling gates than the weight-4 coupling rotations under the standard
staircase decomposition.

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

Second order improves the asymptotic dependence on step count, but its prefactor
depends on the ordered nested commutators. Whether it wins at a particular
accuracy must be checked for the chosen Hamiltonian, ordering, and compiler.

---

## Choosing the Time Step

The time step $\Delta t$ is the key knob in Trotterization. Too large → bad approximation. Too small → too many steps → too deep a circuit.

A useful dimensionless scale is
$\Delta t\sum_{k\ne I}|c_k|$, but requiring it to be below one is a heuristic,
not an accuracy theorem. The identity coefficient is excluded because it
commutes with every term and contributes no product-formula error.

For the corrected H₂ Hamiltonian,

$$\sum_{k\ne I}|c_k|=1.8871072169\ \text{Ha}.$$

Thus $\Delta t=0.1$ gives a scale of $0.1887$. Accuracy still has to be
established from commutators or direct unitary comparison.

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
| Coupling ($XXYY$, etc.) | 4 | 4 | 6 | 24 |
| **Total** | **14** | — | — | **36** |

36 CNOTs per first-order Trotter step. Second-order: 72. For a 100-step simulation: 3,600 (first-order) or 7,200 (second-order) CNOTs total.

These are unoptimized logical counts before routing, native-gate lowering,
state preparation, repetition, and error handling. They describe this H₂
product formula, not hardware feasibility or a larger molecule's cost.

---

## How Many Trotter Steps Do I Need?

The Trotter approximation introduces error. How much error, and how many steps are needed to keep it below a target $\epsilon$?

### A rigorous first-order bound

For first-order Trotter with $N$ steps of size $\Delta t = t/N$, the total error is bounded by:

$$\epsilon_\text{Trotter} \leq \frac{t^2}{2N} \sum_{j < k} \lVert [c_j P_j,\; c_k P_k] \rVert$$

This is the pair-commutator form used here; tighter ordering-dependent results
and higher-order bounds are developed by Childs et al. (2021).

Define the first-order commutator sum

$$\Lambda_{\mathrm{comm}}=\sum_{j<k}\lVert[c_jP_j,c_kP_k]\rVert.$$

measures the ordering error. Two Pauli strings either commute, contributing
zero, or anticommute, contributing $2|c_jc_k|$. For the table above, 16 pairs
anticommute and

$$\Lambda_{\mathrm{comm}}=0.2861997180\ \text{Ha}^2.$$

For $t=1$ and a target unitary-operator error
$\eta=1.6\times10^{-3}$, the bound gives

$$N\geq\frac{t^2\Lambda_{\mathrm{comm}}}{2\eta}=89.44,$$

so 90 first-order steps are sufficient under this norm bound. This $\eta$ is
a dimensionless simulation error, not automatically an energy error of
1.6 mHa; a QPE or observable-estimation error budget must connect the two.

Do not confuse $\Lambda_{\mathrm{comm}}$, which has units Ha$^2$, with the
coefficient 1-norm
$\lambda_{\mathrm{coeff}}=\sum_k|c_k|=2.6992778241$ Ha or the
non-identity measurement norm $1.8871072169$ Ha.

Second-order bounds involve ordered nested commutators and their constants
depend on the exact symmetric formula. We therefore do not infer a numerical
second-order step count from $\lVert H\rVert_1$ alone. Compute the applicable
nested-commutator bound or compare the product-formula unitary directly with
$e^{-iHt}$.

---

## Key Takeaways

- A Trotter step converts a Hamiltonian into a list of **Pauli rotations** — each with a Pauli string and an angle.
- First-order: $L$ rotations. Second-order: $2L$ rotations at half angle, with quadratically better error scaling.
- $\Delta t\lVert H\rVert_1$ is a scale, not an accuracy guarantee; commutator bounds or direct unitary checks set the step count.
- CNOT cost is estimable from Pauli weights alone: $\sum_k 2(w_k - 1)$.
- For H₂: 36 CNOTs per first-order step. Feasible. For H₂O: thousands. That's where optimization matters.

## Common Mistakes

1. **Including the identity term.** The $IIII$ term contributes only a global phase — include it and you waste one rotation per step on something unobservable.

2. **Assuming second order always wins.** It has better asymptotic error scaling, but ordering, commutator prefactors, and compiler cancellations determine the actual crossover.

3. **Treating a norm heuristic as a proof.** A small $\Delta t\lVert H\rVert_1$ is useful intuition, but only an applicable bound or direct comparison certifies the approximation.

## Exercises

1. **Rotation count.** For H₂O with 600 non-identity terms, how many rotations does a single second-order Trotter step have?

2. **Time step.** If $\lVert H \rVert_1 = 30$ Ha and you want $\Delta t = 0.01$, how many Trotter steps do you need for total evolution time $t = 1$?

3. **CNOT scaling.** If every term in a Hamiltonian has weight 5 (ternary tree on a 27-orbital system), and there are 200 non-identity terms, what is the CNOT count per first-order step?

## Further Reading

- Childs, A. M., Su, Y., Tran, M. C., Wiebe, N., and Zhu, S. "Theory of Trotter Error with Commutator Scaling." *Phys. Rev. X* 11, 011020 (2021). DOI: 10.1103/PhysRevX.11.011020.
- Suzuki, M. "General Decomposition Theory of Ordered Exponentials of Time-Dependent Operators." *J. Math. Phys.* 32, 400 (1991). The original higher-order decomposition formulas used throughout this chapter.

---

**Previous:** [Chapter 14 — From Hamiltonian to Time Evolution](14-time-evolution.html)

**Next:** [Chapter 16 — The CNOT Staircase](16-cnot-staircase.html)
