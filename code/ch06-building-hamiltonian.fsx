// ══════════════════════════════════════════════════════════════
// Chapter 6 Companion: Building the Qubit Hamiltonian
// ══════════════════════════════════════════════════════════════
// Run with: dotnet fsi book/code/ch06-building-hamiltonian.fsx
// Prereq:   dotnet build --configuration Release

#r "nuget: FockMap"
#load "ch03-spin-orbitals.fsx"

open System.Numerics
open Encodings

printfn ""
printfn "Chapter 6: Building the Qubit Hamiltonian"
printfn "=========================================="

// Build the JW Hamiltonian on 4 qubits
let hamiltonian = computeHamiltonianWith jordanWignerTerms Ch03_spin_orbitals.h2Factory 4u

printfn ""
printfn "The complete 15-term H₂ Hamiltonian (Jordan-Wigner):"
printfn ""
let mutable i = 0
for t in hamiltonian.DistributeCoefficient.SummandTerms do
    i <- i + 1
    let character =
        if t.Signature |> Seq.forall (fun c -> c = 'I' || c = 'Z') then "diagonal"
        else "off-diagonal (exchange)"
    printfn "  %2d. %+.4f  %s    [%s]" i t.Coefficient.Real t.Signature character

printfn ""
let diagCount = hamiltonian.DistributeCoefficient.SummandTerms |> Array.filter (fun t -> t.Signature |> Seq.forall (fun c -> c = 'I' || c = 'Z')) |> Array.length
let offDiagCount = hamiltonian.DistributeCoefficient.SummandTerms.Length - diagCount
printfn "  Diagonal terms (classical): %d" diagCount
printfn "  Off-diagonal terms (quantum): %d" offDiagCount
printfn ""
printfn "  The %d off-diagonal terms generate coherences in the density matrix." offDiagCount
printfn "  They are the entire reason a quantum computer is needed."

// Try other encodings
printfn ""
printfn "Same Hamiltonian, different encodings:"
let encoders = [
    ("Jordan-Wigner",         jordanWignerTerms)
    ("Bravyi-Kitaev",         bravyiKitaevTerms)
    ("Parity",                parityTerms)
    ("Balanced Binary Tree",  balancedBinaryTreeTerms)
    ("Balanced Ternary Tree", ternaryTreeTerms)
    ("Vlasov Tree",           vlasovTreeTerms)
]

for (name, encoder) in encoders do
    let ham = computeHamiltonianWith encoder Ch03_spin_orbitals.h2Factory 4u
    let terms = ham.DistributeCoefficient.SummandTerms
    printfn "  %-25s  %d terms" name terms.Length
