// ══════════════════════════════════════════════════════════════
// Chapter 3 Companion: From Spatial to Spin-Orbital Integrals
// ══════════════════════════════════════════════════════════════
// Run with: dotnet fsi book/code/ch03-spin-orbitals.fsx
// Prereq:   dotnet build --configuration Release

#r "nuget: FockMap"

open System.Numerics
open Encodings

// ── H₂/STO-3G integral tables (spin-orbital, physicist's convention) ──

let h2Integrals = Map [
    // One-body (spin-orbital, interleaved indexing)
    ("0,0", Complex(-1.2563390730, 0.0))
    ("1,1", Complex(-1.2563390730, 0.0))
    ("2,2", Complex(-0.4718960244, 0.0))
    ("3,3", Complex(-0.4718960244, 0.0))

    // Two-body: same-spin αα-αα
    ("0,0,0,0", Complex(0.6744887663, 0.0))
    ("2,2,2,2", Complex(0.6973979495, 0.0))
    ("0,2,2,0", Complex(0.1809312700, 0.0))
    ("2,0,0,2", Complex(0.1809312700, 0.0))
    ("0,2,0,2", Complex(0.6975782469, 0.0))
    ("2,0,2,0", Complex(0.6975782469, 0.0))
    ("0,0,2,2", Complex(0.6636340479, 0.0))
    ("2,2,0,0", Complex(0.6636340479, 0.0))

    // Two-body: same-spin ββ-ββ
    ("1,1,1,1", Complex(0.6744887663, 0.0))
    ("3,3,3,3", Complex(0.6973979495, 0.0))
    ("1,3,3,1", Complex(0.1809312700, 0.0))
    ("3,1,1,3", Complex(0.1809312700, 0.0))
    ("1,3,1,3", Complex(0.6975782469, 0.0))
    ("3,1,3,1", Complex(0.6975782469, 0.0))
    ("1,1,3,3", Complex(0.6636340479, 0.0))
    ("3,3,1,1", Complex(0.6636340479, 0.0))

    // Two-body: cross-spin αβ-αβ
    ("0,1,0,1", Complex(0.6744887663, 0.0))
    ("0,3,0,3", Complex(0.6636340479, 0.0))
    ("2,1,2,1", Complex(0.6636340479, 0.0))
    ("2,3,2,3", Complex(0.6973979495, 0.0))
    ("0,1,2,3", Complex(0.6975782469, 0.0))
    ("2,3,0,1", Complex(0.6975782469, 0.0))
    ("0,3,2,1", Complex(0.1809312700, 0.0))
    ("2,1,0,3", Complex(0.1809312700, 0.0))

    // Two-body: cross-spin βα-βα
    ("1,0,1,0", Complex(0.6744887663, 0.0))
    ("1,2,1,2", Complex(0.6636340479, 0.0))
    ("3,0,3,0", Complex(0.6636340479, 0.0))
    ("3,2,3,2", Complex(0.6973979495, 0.0))
    ("1,0,3,2", Complex(0.6975782469, 0.0))
    ("3,2,1,0", Complex(0.6975782469, 0.0))
    ("1,2,3,0", Complex(0.1809312700, 0.0))
    ("3,0,1,2", Complex(0.1809312700, 0.0))
]

let h2Factory key = h2Integrals |> Map.tryFind key

printfn "Chapter 3: Spin-Orbital Integrals"
printfn "================================="
printfn "One-body integrals (spin-orbital):"
for p in 0..3 do
    let key = sprintf "%d,%d" p p
    match h2Factory key with
    | Some v -> printfn "  h(%d,%d) = %.10f Ha" p p v.Real
    | None -> ()

printfn ""
printfn "Total two-body integrals in map: %d" (h2Integrals |> Map.filter (fun k _ -> k.Split(',').Length = 4) |> Map.count)
printfn "  Same-spin (αα + ββ): 16"
printfn "  Cross-spin (αβ + βα): 16"

// Build the Hamiltonian as a preview
let h2Hamiltonian = computeHamiltonianWith jordanWignerTerms h2Factory 4u
printfn ""
printfn "H₂ JW Hamiltonian: %d terms (preview for Chapter 6)" (h2Hamiltonian.DistributeCoefficient.SummandTerms.Length)
