// Lab 10: Vlasov Tree Encoding — Comparison with Other Encodings
//
// This lab compares the Vlasov complete ternary tree encoding with
// Jordan-Wigner, Bravyi-Kitaev, and balanced ternary tree encodings.
//
// The Vlasov tree uses breadth-first (level-order) indexing:
//   node 0 is root; children of node j are 3j+1, 3j+2, 3j+3.
//
// Based on: Vlasov, "Clifford algebras, Spin groups and qubit trees"
//   arXiv:1904.09912 (2019), Quanta 11:97-114 (2022).
//
// The breadth-first ternary tree has Θ(log n) Pauli-weight scaling, like the balanced ternary tree, with a ternary depth constant. Its level-order indexing leads to different
// qubit arrangements and may have different practical performance for certain Hamiltonians.
//
// Run with: dotnet fsi labs/10-vlasov-tree.fsx

// Thanks to Dr Stephen Jordan and Dr Robin Kothari for introducing me to this encoding!


#load "../code/ch03-spin-orbitals.fsx"
#load "PauliMatrix.fsx"

open Encodings
open Encodings.TreeEncoding
open Encodings.MajoranaEncoding
open Encodings.JordanWigner
open Encodings.BravyiKitaev
open Encodings.Hamiltonian
open System.Numerics

// ═══════════════════════════════════════════════════════
//  Part 1: Tree Shapes
// ═══════════════════════════════════════════════════════

printfn "═══════════════════════════════════════════════════"
printfn " Lab 10: Vlasov Tree Encoding"
printfn "═══════════════════════════════════════════════════\n"

printfn "─── Tree Shapes (n = 8) ───\n"

let vlTree = vlasovTree 8
let btTree = balancedTernaryTree 8

let showTree (tree : EncodingTree) name =
    printfn "  %s (root = %d):" name tree.Root.Index
    for kvp in tree.Nodes |> Map.toSeq |> Seq.sortBy fst do
        let node = snd kvp
        let children = node.Children |> List.map (fun c -> string c.Index) |> String.concat ", "
        let parentStr = match node.Parent with Some p -> string p | None -> "root"
        printfn "    node %d: parent=%-4s  children=[%s]" node.Index parentStr children
    printfn ""

showTree vlTree "Vlasov (level-order)"
showTree btTree "Balanced Ternary (midpoint-split)"

// ═══════════════════════════════════════════════════════
//  Part 2: Pauli Strings for a†₃
// ═══════════════════════════════════════════════════════

let n = 8u

printfn "─── Creation Operator a†₃ (n = %d) ───\n" n

let encoders = [
    ("Jordan-Wigner",       jordanWignerTerms)
    ("Bravyi-Kitaev",       bravyiKitaevTerms)
    ("Balanced Ternary",    ternaryTreeTerms)
    ("Vlasov Tree",         vlasovTreeTerms)
]

for (name, encode) in encoders do
    let terms = encode Raise 3u n
    let d = terms.DistributeCoefficient
    printfn "  %s:" name
    for t in d.SummandTerms do
        let w = t.Signature |> Seq.filter (fun c -> c <> 'I') |> Seq.length
        printfn "    %+.4f%+.4fi  %s  (weight %d)"
            t.Coefficient.Real t.Coefficient.Imaginary t.Signature w
    printfn ""

// ═══════════════════════════════════════════════════════
//  Part 3: Weight Comparison Across All Modes
// ═══════════════════════════════════════════════════════

printfn "─── Max Pauli Weight per Mode ───\n"
printfn "  %-5s  %-5s  %-5s  %-8s  %-6s" "Mode" "JW" "BK" "TerTree" "Vlasov"
printfn "  %-5s  %-5s  %-5s  %-8s  %-6s" "────" "──" "──" "───────" "──────"

for j in 0u .. n - 1u do
    let weight (encode : LadderOperatorUnit -> uint32 -> uint32 -> PauliRegisterSequence) =
        let terms = (encode Raise j n).DistributeCoefficient
        terms.SummandTerms
        |> Array.map (fun t ->
            t.Signature |> Seq.filter (fun c -> c <> 'I') |> Seq.length)
        |> Array.max
    printfn "  %-5d  %-5d  %-5d  %-8d  %-6d" j
        (weight jordanWignerTerms)
        (weight bravyiKitaevTerms)
        (weight ternaryTreeTerms)
        (weight vlasovTreeTerms)

// ═══════════════════════════════════════════════════════
//  Part 4: H₂ Hamiltonian Comparison
// ═══════════════════════════════════════════════════════

printfn "\n─── H₂ Hamiltonian (4 spin-orbitals) ───\n"

let nH2 = 4u
let lookup = ``Ch03-spin-orbitals``.h2RawPhysicistFactory

printfn "  %-20s  %5s  %7s  %7s" "Encoding" "Terms" "MaxWt" "AvgWt"
printfn "  %-20s  %5s  %7s  %7s" "────────" "─────" "─────" "─────"

for (name, encoder) in encoders do
    let ham =
        (computeHamiltonianWith encoder lookup nH2).DistributeCoefficient
    let terms = ham.SummandTerms
                |> Array.filter (fun t -> Complex.Abs t.Coefficient > 1e-12)
    let weights = terms |> Array.map (fun t ->
        t.Signature |> Seq.filter (fun c -> c <> 'I') |> Seq.length)
    let maxW = if weights.Length > 0 then Array.max weights else 0
    let avgW = if weights.Length > 0 then Array.averageBy float weights else 0.0
    printfn "  %-20s  %5d  %7d  %7.2f" name terms.Length maxW avgW

printfn "\nThis table compares representation-level term weights only."
printfn "\n─── Full H₂ Spectrum Comparison ───\n"
for (name, encoder) in encoders do
    let ham = computeHamiltonianWith encoder lookup nH2
    PauliMatrix.assertH2Spectrum name 1e-7 ham
