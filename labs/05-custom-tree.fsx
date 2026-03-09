(**
---
title: Custom Tree-Based Encodings
category: Tutorials
categoryindex: 3
index: 5
---
*)

(**
# Custom Tree-Based Encodings

Tree-based encodings map fermionic modes to qubits using a rooted tree structure.
The key insight is that **any ternary tree** (at most 3 children per node) with n nodes
defines a valid encoding for n modes, and tree shape impacts Pauli weight.

## The TreeNode Type

Trees are built from `TreeNode` records:

```fsharp
type TreeNode = {
    Index    : int              // 0-based mode index
    Children : TreeNode list    // Child nodes (max 3 for ternary)
    Parent   : int option       // Parent index (None for root)
}
```
*)

#r "nuget: FockMap"

open Encodings
open System.Numerics

(**
## Helper Functions

Let's define utilities to measure Pauli weight:
*)

let pauliWeight (reg : PauliRegister) =
    reg.Signature |> Seq.sumBy (fun c -> if c = 'I' then 0 else 1)

let maxWeight (prs : PauliRegisterSequence) =
    prs.SummandTerms |> Array.map pauliWeight |> Array.max

(**
## Building a Chain Tree (Linear)

A chain tree represents the Jordan-Wigner encoding:

```
  0 (root)
  |
  1
  |
  2
  |
  3
```
*)

let chainNode3 = { Index = 3; Children = []; Parent = Some 2 }
let chainNode2 = { Index = 2; Children = [chainNode3]; Parent = Some 1 }
let chainNode1 = { Index = 1; Children = [chainNode2]; Parent = Some 0 }
let chainNode0 = { Index = 0; Children = [chainNode1]; Parent = None }

let rec collectNodes (node : TreeNode) : Map<int, TreeNode> =
    node.Children
    |> List.fold (fun acc child ->
        Map.fold (fun m k v -> Map.add k v m) acc (collectNodes child))
        (Map.ofList [(node.Index, node)])

let chainTree : EncodingTree =
    { Root = chainNode0; Nodes = collectNodes chainNode0; Size = 4 }

(**
## Building a Star Tree (Shallow)

A star tree connects all leaves directly to the root (max 3 children for ternary):

```
     0 (root)
    /|\
   1 2 3
```
*)

let starLeaf1 = { Index = 1; Children = []; Parent = Some 0 }
let starLeaf2 = { Index = 2; Children = []; Parent = Some 0 }
let starLeaf3 = { Index = 3; Children = []; Parent = Some 0 }
let starRoot = { Index = 0; Children = [starLeaf1; starLeaf2; starLeaf3]; Parent = None }

let starTree : EncodingTree =
    { Root = starRoot; Nodes = collectNodes starRoot; Size = 4 }

(**
## Building a Left-Leaning Tree

An unbalanced tree with all nodes on one branch:

```
     0 (root)
    /|
   1 3
  /
 2
```
*)

let leftNode2 = { Index = 2; Children = []; Parent = Some 1 }
let leftNode1 = { Index = 1; Children = [leftNode2]; Parent = Some 0 }
let leftNode3 = { Index = 3; Children = []; Parent = Some 0 }
let leftRoot = { Index = 0; Children = [leftNode1; leftNode3]; Parent = None }

let leftTree : EncodingTree =
    { Root = leftRoot; Nodes = collectNodes leftRoot; Size = 4 }

(**
## Comparing Tree Encodings

Let's encode operators with each tree and compare Pauli weights:
*)

let n = 4u

printfn "Pauli weight comparison for a†_j (4 modes):"
printfn "%-6s  %8s  %8s  %10s  %10s" "Mode" "Chain" "Star" "Left-Lean" "Balanced"
printfn "%s" (String.replicate 50 "-")

let balancedTree4 = balancedTernaryTree 4

for j in 0u .. n - 1u do
    let chainEnc = encodeWithTernaryTree chainTree Raise j n |> maxWeight
    let starEnc  = encodeWithTernaryTree starTree Raise j n |> maxWeight
    let leftEnc  = encodeWithTernaryTree leftTree Raise j n |> maxWeight
    let balEnc   = encodeWithTernaryTree balancedTree4 Raise j n |> maxWeight
    printfn "  %d       %4d      %4d        %4d        %4d" j chainEnc starEnc leftEnc balEnc

(**
## Understanding the Results

Notice how tree shape affects operator weights:

- **Chain trees** have increasing weight for deeper modes (like Jordan-Wigner)
- **Star trees** have uniform low weight for leaves (depth 1 from root)
- **Left-leaning trees** have higher weight for the deepest branch
- **Balanced trees** keep all weights bounded by tree height

The path from a mode to the root determines its Pauli weight. Deep modes
in unbalanced trees require longer Pauli strings.
*)

printfn ""
printfn "=== Key Insight ==="
printfn "Tree shape directly determines circuit depth."
printfn "Shallow trees minimize worst-case Pauli weight."

(**
## Summary

Any rooted tree defines a valid fermion-to-qubit encoding:
- Build nodes with `Index`, `Children`, and `Parent`
- Collect into an `EncodingTree` with `Root`, `Nodes` map, and `Size`
- Use `encodeWithTernaryTree` to encode operators
- Tree depth determines worst-case Pauli weight
*)
