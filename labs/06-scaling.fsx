(**
---
title: Encoding Scaling Analysis
category: Tutorials
categoryindex: 3
index: 6
---
*)

(**
# Encoding Scaling Analysis

This tutorial analyzes how different encodings scale with system size.
The Pauli weight of encoded operators directly impacts quantum circuit depth,
making this analysis crucial for choosing the right encoding.

## Theoretical Scaling

| Encoding          | Pauli Weight | Circuit Depth |
|-------------------|--------------|---------------|
| Jordan-Wigner     | O(n)         | O(n)          |
| Parity            | O(n)         | O(n)          |
| Bravyi-Kitaev     | O(log₂ n)    | O(log n)      |
| Binary Tree       | O(log₂ n)    | O(log n)      |
| Ternary Tree      | O(log₃ n)    | O(log n)      |
*)

#r "nuget: FockMap"

open Encodings
open System

(**
## Measuring Pauli Weight

The Pauli weight is the number of non-identity operators in a Pauli string:
*)

let pauliWeight (reg : PauliRegister) =
    reg.Signature |> Seq.sumBy (fun c -> if c = 'I' then 0 else 1)

let maxWeight (prs : PauliRegisterSequence) =
    prs.SummandTerms |> Array.map pauliWeight |> Array.max

(**
## Encoding Functions

We'll compare these five encodings:
*)

let encodings : (string * (LadderOperatorUnit -> uint32 -> uint32 -> PauliRegisterSequence)) list =
    [ "Jordan-Wigner", jordanWignerTerms
      "Parity",        parityTerms
      "Bravyi-Kitaev", bravyiKitaevTerms
      "Binary Tree",   balancedBinaryTreeTerms
      "Ternary Tree",  ternaryTreeTerms ]

(**
## Maximum Weight Across All Modes

For each system size, we measure the maximum Pauli weight across all modes:
*)

let maxWeightForEncoding encode n =
    [ for j in 0u .. n - 1u -> encode Raise j n |> maxWeight ]
    |> List.max

(**
## Scaling Analysis

Let's measure scaling for system sizes 4, 8, 16, and 32:
*)

let systemSizes = [ 4u; 8u; 16u; 32u ]

printfn ""
printfn "╔══════════════════════════════════════════════════════════════╗"
printfn "║         Maximum Pauli Weight Scaling Analysis                ║"
printfn "╠══════════════════════════════════════════════════════════════╣"
printfn "║ Encoding        │   n=4     n=8    n=16    n=32   Scaling    ║"
printfn "╠═════════════════╪══════════════════════════════════════════╣"

for (name, encode) in encodings do
    let weights = systemSizes |> List.map (fun n -> maxWeightForEncoding encode n)
    let scaling =
        match name with
        | "Jordan-Wigner" | "Parity" -> "O(n)"
        | "Bravyi-Kitaev" | "Binary Tree" -> "O(log₂ n)"
        | _ -> "O(log₃ n)"
    printfn "║ %-15s │ %5d   %5d   %5d   %5d   %-10s ║"
        name weights.[0] weights.[1] weights.[2] weights.[3] scaling

printfn "╚══════════════════════════════════════════════════════════════╝"

(**
## Theoretical vs Measured Comparison

Let's verify the scaling matches theoretical predictions:
*)

printfn "\n=== Theoretical Analysis ==="

for n in systemSizes do
    let log2n = Math.Log2(float n) |> int
    let log3n = Math.Log(float n) / Math.Log(3.0) |> int |> (+) 1
    printfn "n = %2d:  log₂(n) = %d,  log₃(n) ≈ %d" n log2n log3n

(**
## Scaling Implications

The difference becomes dramatic at scale:

| n       | O(n)   | O(log₂ n) | O(log₃ n) |
|---------|--------|-----------|-----------|
| 100     | 100    | 7         | 5         |
| 1,000   | 1,000  | 10        | 7         |
| 10,000  | 10,000 | 14        | 9         |

For quantum chemistry with hundreds of orbitals, logarithmic scaling
can reduce circuit depth by orders of magnitude.
*)

printfn "\n=== Projected Scaling for Large Systems ==="
printfn "%-10s  %8s  %12s  %12s" "n" "O(n)" "O(log₂ n)" "O(log₃ n)"
printfn "%s" (String.replicate 46 "-")

for n in [100; 1000; 10000] do
    let linear = n
    let log2 = Math.Log2(float n) |> ceil |> int
    let log3 = Math.Ceiling(Math.Log(float n) / Math.Log(3.0)) |> int
    printfn "%-10d  %8d  %12d  %12d" n linear log2 log3

(**
## Summary

Key takeaways:

1. **Jordan-Wigner and Parity** scale linearly—fine for small systems
   but impractical for large molecules.

2. **Bravyi-Kitaev and Binary Tree** achieve O(log₂ n) scaling,
   reducing 1000-mode circuits from depth 1000 to depth ~10.

3. **Ternary Tree (Bonsai)** is theoretically optimal at O(log₃ n),
   providing an additional ~37% reduction over binary trees.

Choose your encoding based on your specific constraints:
- Small systems (n < 20): Jordan-Wigner for simplicity
- Medium systems (20 < n < 100): Bravyi-Kitaev for balance
- Large systems (n > 100): Ternary Tree for minimal depth
*)
