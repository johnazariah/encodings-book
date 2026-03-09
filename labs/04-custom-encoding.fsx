(**
---
title: Custom Encoding Schemes
category: Tutorials
categoryindex: 3
index: 4
---
*)

(**
# Custom Encoding Schemes

This tutorial shows how to define your own fermion-to-qubit encoding by
implementing the `EncodingScheme` record type.

## The EncodingScheme Type

Every encoding in the Majorana decomposition family is defined by three
index-set functions:

```fsharp
type EncodingScheme = {
    Update     : int -> int -> Set<int>   // U(j, n) — qubits to flip
    Parity     : int -> Set<int>          // P(j)    — parity qubits
    Occupation : int -> Set<int>          // Occ(j)  — occupation qubits
}
```

These sets determine how the Majorana operators c_j and d_j are built:

- **c_j** = X_{j ∪ U(j)} · Z_{P(j)}
- **d_j** = Y_j · X_{U(j)} · Z_{(P(j) ⊕ Occ(j)) \ {j}}

The ladder operators follow: a†_j = ½(c_j − i·d_j), a_j = ½(c_j + i·d_j).
*)

#r "nuget: FockMap"

open Encodings

(**
## Exploring the Built-in Parity Scheme

Let's examine the built-in `parityScheme` to understand how these functions work:
*)

printfn "=== Built-in Parity Scheme ==="
printfn "parityScheme.Update 2 4     = %A" (parityScheme.Update 2 4)
printfn "parityScheme.Parity 2       = %A" (parityScheme.Parity 2)
printfn "parityScheme.Occupation 2   = %A" (parityScheme.Occupation 2)

(**
The parity scheme stores cumulative parity information:
- **Update**: All qubits after j must be flipped when n_j changes
- **Parity**: Only the immediate predecessor carries parity info
- **Occupation**: Occupation is shared between j and j-1
*)

(**
## Defining a Custom Scheme

Let's create a "local parity" scheme where each mode j only references
its immediate neighbor for parity, with no propagating updates:
*)

let localParityScheme : EncodingScheme =
    { Update     = fun _ _ -> Set.empty
      Parity     = fun j   -> if j > 0 then Set.singleton (j - 1) else Set.empty
      Occupation = fun j   -> Set.singleton j }

printfn "\n=== Custom Local Parity Scheme ==="
printfn "Update 2 4     = %A" (localParityScheme.Update 2 4)
printfn "Parity 2       = %A" (localParityScheme.Parity 2)
printfn "Occupation 2   = %A" (localParityScheme.Occupation 2)

(**
## Encoding Operators

Use `encodeOperator` to transform a fermionic ladder operator into Pauli strings:
*)

let n = 4u  // 4 qubits
let j = 2u  // Mode index 2

printfn "\n=== Encoding a†₂ (raising operator at mode 2) ==="
let raising = encodeOperator localParityScheme Raise j n
printfn "Result: %d Pauli terms" raising.SummandTerms.Length
for term in raising.SummandTerms do
    printfn "  %s  (coeff: %.2f%+.2fi)" term.Signature term.Coefficient.Real term.Coefficient.Imaginary

printfn "\n=== Encoding a₂ (lowering operator at mode 2) ==="
let lowering = encodeOperator localParityScheme Lower j n
for term in lowering.SummandTerms do
    printfn "  %s  (coeff: %.2f%+.2fi)" term.Signature term.Coefficient.Real term.Coefficient.Imaginary

(**
## Verifying the Encoding

Let's compare our custom scheme with Jordan-Wigner to see the difference:
*)

printfn "\n=== Jordan-Wigner a†₂ for comparison ==="
let jwRaising = encodeOperator jordanWignerScheme Raise j n
for term in jwRaising.SummandTerms do
    printfn "  %s  (coeff: %.2f%+.2fi)" term.Signature term.Coefficient.Real term.Coefficient.Imaginary

(**
## Creating Another Custom Scheme

Here's a scheme that spreads occupation across multiple qubits:
*)

let spreadScheme : EncodingScheme =
    { Update     = fun j n -> set [ j + 1 .. n - 1 ]  // Like parity
      Parity     = fun j   -> set [ 0 .. j - 1 ]      // Like JW
      Occupation = fun j   -> set [ max 0 (j - 1) .. j ] }

printfn "\n=== Spread Scheme a†₂ ==="
let spreadRaising = encodeOperator spreadScheme Raise j n
for term in spreadRaising.SummandTerms do
    printfn "  %s" term.Signature

(**
## Summary

Any encoding in the Majorana decomposition family can be defined by
specifying three functions: Update, Parity, and Occupation. The library
handles the rest—constructing the Majorana operators and combining them
into ladder operators with the correct phases.

Key points:
- `Update(j, n)` determines which qubits flip when occupation changes
- `Parity(j)` determines which qubits encode the parity sum
- `Occupation(j)` determines which qubits encode the occupation number
- Use `encodeOperator` to transform ladder operators to Pauli strings
*)
