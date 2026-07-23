(**
# Your First Encoding

This tutorial introduces fermion-to-qubit encoding using the Jordan-Wigner transformation.
You'll learn how to encode fermionic creation and annihilation operators as Pauli strings
that can be executed on a quantum computer.

## What We'll Do

1. Encode a single creation operator a†₀
2. Encode a†₂ and see how the Z-chain grows
3. Understand the output Pauli strings

## Setup

First, we reference the Encodings library and open the namespace:
*)

#r "nuget: FockMap, 0.9.0"
open Encodings
open Encodings.JordanWigner

(**
## Encoding Your First Operator

Let's encode the creation operator a†₀ on a system with 4 fermionic modes.
In Jordan-Wigner, mode 0 is the simplest case — no Z-chain is needed.
*)

let result0 = jordanWignerTerms Raise 0u 4u

printfn "a†₀ encoded (4 modes):"
for term in result0.SummandTerms do
    printfn "  %s × (%s)" (term.Coefficient.ToString()) term.Signature

(**
The output shows two Pauli strings:

- `XIII` with coefficient 0.5
- `YIII` with coefficient -0.5i

This corresponds to the Majorana decomposition:

$$a^\dagger_0 = \frac{1}{2}(X_0 - iY_0)$$

## The Growing Z-Chain

Now let's encode a†₂ — creation on mode 2:
*)

let result2 = jordanWignerTerms Raise 2u 4u

printfn "\na†₂ encoded (4 modes):"
for term in result2.SummandTerms do
    printfn "  %s × (%s)" (term.Coefficient.ToString()) term.Signature

(**
Notice the Z operators on qubits 0 and 1! The strings are now:

- `ZZXI` — Z's track parity of modes 0 and 1
- `ZZYI`

The Z-chain ensures fermionic anticommutation relations are preserved.
When we act on mode 2, we need to know the combined parity of all
preceding modes (0 and 1) to get the correct sign.

## Annihilation Operators

The annihilation operator a₂ (Lower) differs only in the phase:
*)

let annihilation = jordanWignerTerms Lower 2u 4u

printfn "\na₂ encoded (4 modes):"
for term in annihilation.SummandTerms do
    printfn "  %s × (%s)" (term.Coefficient.ToString()) term.Signature

(**
## Key Takeaways

1. **Jordan-Wigner** maps fermionic operators to sums of Pauli strings
2. **Z-chains** grow with mode index — mode j has j Z's preceding it
3. **Pauli weight** scales as O(n) for the worst case (mode n-1)

For lower Pauli weight, explore the Bravyi-Kitaev and tree-based encodings
in the next tutorials!
*)
