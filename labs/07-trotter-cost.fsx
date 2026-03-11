(**
---
title: Trotter Cost Comparison
category: Tutorials
categoryindex: 3
index: 7
---
*)

(**
# Trotter Cost: From Pauli Weight to CNOT Count

In the [scaling analysis](06-scaling.html), we compared encodings by their
**Pauli weight** — the number of non-identity Pauli operators in an encoded
ladder operator. But Pauli weight is a proxy for what actually matters on
hardware: **CNOT gates**.

This lab computes the concrete CNOT cost of a single Trotter step for the
H₂ molecule under all six encodings, connecting the abstract notion of
Pauli weight to measurable circuit depth.

## The CNOT Staircase

To implement a single Pauli rotation $e^{-i\theta P}$ where $P$ is a
Pauli string of weight $w$, the standard decomposition uses:

1. A CNOT staircase (depth $w - 1$) to entangle the relevant qubits
2. A single $R_z(2\theta)$ rotation
3. A reverse CNOT staircase (depth $w - 1$) to unentangle

**Total: $2(w - 1)$ CNOT gates per Pauli rotation.**

A first-order Trotter step applies one such rotation for each term in the
Hamiltonian, so the total CNOT count per Trotter step is:

$$\text{CNOTs per step} = \sum_{k=1}^{L} 2(w_k - 1)$$

where $L$ is the number of Pauli terms and $w_k$ is the weight of term $k$.

## Setup
*)

#r "nuget: FockMap"

open Encodings
open System.Numerics

(**
## H₂ Integrals (STO-3G, R = 0.7414 Å)

These are the same integrals from the [H₂ lab](02-h2-molecule.html)
and the [grand finale](../guides/cookbook/13-grand-finale.html):
*)

let nModes = 4u

let integrals = Map [
    ("0,0", Complex(-1.2563, 0.0)); ("1,1", Complex(-1.2563, 0.0))
    ("2,2", Complex(-0.4719, 0.0)); ("3,3", Complex(-0.4719, 0.0))
    ("0,0,0,0", Complex(0.6745, 0.0)); ("1,1,1,1", Complex(0.6745, 0.0))
    ("2,2,2,2", Complex(0.6974, 0.0)); ("3,3,3,3", Complex(0.6974, 0.0))
    ("0,0,1,1", Complex(0.6745, 0.0)); ("1,1,0,0", Complex(0.6745, 0.0))
    ("0,0,2,2", Complex(0.6636, 0.0)); ("2,2,0,0", Complex(0.6636, 0.0))
    ("0,0,3,3", Complex(0.6636, 0.0)); ("3,3,0,0", Complex(0.6636, 0.0))
    ("1,1,2,2", Complex(0.6636, 0.0)); ("2,2,1,1", Complex(0.6636, 0.0))
    ("1,1,3,3", Complex(0.6636, 0.0)); ("3,3,1,1", Complex(0.6636, 0.0))
    ("2,2,3,3", Complex(0.6974, 0.0)); ("3,3,2,2", Complex(0.6974, 0.0))
    ("0,2,2,0", Complex(0.1809, 0.0)); ("2,0,0,2", Complex(0.1809, 0.0))
    ("1,3,3,1", Complex(0.1809, 0.0)); ("3,1,1,3", Complex(0.1809, 0.0))
]

let lookup key =
    match (key : string).Split(',').Length with
    | 2 | 4 -> integrals |> Map.tryFind key
    | _ -> None

(**
## CNOT Cost Computation

For each encoding, we:
1. Encode the H₂ Hamiltonian
2. Combine like terms (DistributeCoefficient)
3. Compute CNOT cost per Pauli rotation: $2(w - 1)$
4. Sum over all non-identity terms for one Trotter step
*)

let pauliWeight (signature : string) =
    signature |> Seq.sumBy (fun c -> if c = 'I' then 0 else 1)

let cnotsPerRotation w = 2 * (w - 1) |> max 0

let trotterCost (ham : PauliRegisterSequence) =
    let terms = ham.DistributeCoefficient.SummandTerms
    let nonIdentityTerms =
        terms |> Array.filter (fun t -> pauliWeight t.Signature > 0)
    let totalCnots =
        nonIdentityTerms
        |> Array.sumBy (fun t -> t.Signature |> pauliWeight |> cnotsPerRotation)
    let maxWeight =
        nonIdentityTerms
        |> Array.map (fun t -> pauliWeight t.Signature)
        |> Array.max
    let avgWeight =
        nonIdentityTerms
        |> Array.averageBy (fun t -> pauliWeight t.Signature |> float)
    {| Terms = nonIdentityTerms.Length
       MaxWeight = maxWeight
       AvgWeight = avgWeight
       TotalCnots = totalCnots |}

(**
## Encoding H₂ with All Six Encodings
*)

let encoders : (string * (LadderOperatorUnit -> uint32 -> uint32 -> PauliRegisterSequence)) list =
    [ "Jordan-Wigner",  jordanWignerTerms
      "Parity",         parityTerms
      "Bravyi-Kitaev",  bravyiKitaevTerms
      "Binary Tree",    balancedBinaryTreeTerms
      "Ternary Tree",   ternaryTreeTerms
      "Vlasov Tree",    vlasovTreeTerms ]

(**
## Results
*)

printfn ""
printfn "╔══════════════════════════════════════════════════════════════════════════╗"
printfn "║       CNOT Cost per First-Order Trotter Step — H₂ (STO-3G, 4 qubits)  ║"
printfn "╠══════════════════════════════════════════════════════════════════════════╣"
printfn "║ Encoding        │ Terms │ Max w │ Avg w │ CNOTs/step │ vs JW           ║"
printfn "╠═════════════════╪═══════╪═══════╪═══════╪════════════╪═════════════════╣"

let mutable jwCnots = 0

for (name, encode) in encoders do
    let ham = computeHamiltonianWith encode lookup nModes
    let stats = trotterCost ham
    if name = "Jordan-Wigner" then jwCnots <- stats.TotalCnots
    let comparison =
        if name = "Jordan-Wigner" then "baseline"
        elif stats.TotalCnots < jwCnots then
            sprintf "%.0f%% fewer" (100.0 * float (jwCnots - stats.TotalCnots) / float jwCnots)
        elif stats.TotalCnots > jwCnots then
            sprintf "%.0f%% more" (100.0 * float (stats.TotalCnots - jwCnots) / float jwCnots)
        else "same"
    printfn "║ %-15s │  %3d  │  %3d  │ %4.1f  │    %4d    │ %-15s ║"
        name stats.Terms stats.MaxWeight stats.AvgWeight stats.TotalCnots comparison

printfn "╚══════════════════════════════════════════════════════════════════════════╝"

(**
## Surprise: JW Wins at Small $n$!

For H₂ (4 qubits), Jordan–Wigner has the **lowest** CNOT count. Why? Because
JW's $O(n)$ weight only becomes a problem at larger $n$. At $n = 4$, the
maximum weight of any JW operator is just 4 — and most Hamiltonian terms
have even lower weight.

The sub-linear encodings pay overhead in rearranging qubits that isn't
recovered until the system is large enough for their logarithmic scaling
to dominate.

## Scaling: Where the Crossover Happens

To see the crossover, we measure the **maximum Pauli weight** of any single
encoded operator at various system sizes. Since each such operator would
become a Hamiltonian term requiring $2(w-1)$ CNOTs, this weight directly
determines per-rotation circuit depth:
*)

let maxWeightForEncoding (encode : LadderOperatorUnit -> uint32 -> uint32 -> PauliRegisterSequence) n =
    [ for j in 0u .. n - 1u ->
        let prs = encode Raise j n
        prs.SummandTerms
        |> Array.map (fun t -> pauliWeight t.Signature)
        |> Array.max ]
    |> List.max

let systemSizes = [ 4u; 8u; 16u; 32u; 64u ]

printfn ""
printfn "╔════════════════════════════════════════════════════════════════════════════════╗"
printfn "║       Maximum Operator Weight → CNOTs per Rotation: 2(w − 1)                ║"
printfn "╠════════════════════════════════════════════════════════════════════════════════╣"
printfn "║ Encoding        │  n=4 (CNOTs) │  n=8 (CNOTs) │ n=16 (CNOTs) │ n=32 (CNOTs) ║"
printfn "╠═════════════════╪══════════════╪══════════════╪══════════════╪══════════════╣"

for (name, encode) in encoders do
    let data =
        [ for n in [ 4u; 8u; 16u; 32u ] ->
            let w = maxWeightForEncoding encode n
            sprintf "%2d (%3d)" w (cnotsPerRotation w) ]
    printfn "║ %-15s │  %-11s │  %-11s │  %-11s │  %-11s ║"
        name data.[0] data.[1] data.[2] data.[3]

printfn "╚════════════════════════════════════════════════════════════════════════════════╝"

(**
## The Scaling Story

The crossover is now visible:

- At $n = 4$: JW (weight 4) and Ternary Tree (weight 2) are comparable
- At $n = 32$: JW needs **62 CNOTs** per worst-case rotation, while
  Ternary Tree needs only **6** — a **10× reduction**
- At $n = 100$: JW would need **198 CNOTs** per rotation, while
  Ternary Tree needs only **8** — a **25× reduction**

These per-rotation savings compound across every term in the Hamiltonian
and every Trotter step, making encoding choice the single largest
lever for reducing circuit depth in Trotter-based simulation.

## The Formula in Context

The $2(w-1)$ CNOT cost is a hard floor: it's the minimum number of
entangling gates needed to implement a Pauli rotation in the standard
circuit model (without ancillae). Any Trotter-based simulation must
pay this cost for every term in every time step. This is why encoding
choice is a **first-order concern** for quantum simulation — it
doesn't just affect abstract operator weight, it directly determines
the number of noisy two-qubit gates your quantum computer must execute.

---

**Previous:** [Encoding Scaling Analysis](06-scaling.html)

**Back to:** [Lab index](index.html)
*)
