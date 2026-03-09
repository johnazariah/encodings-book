// ══════════════════════════════════════════════════════════════
// Chapter 11 Companion: General Clifford Tapering
// ══════════════════════════════════════════════════════════════
// Run with: dotnet fsi book/code/ch11-clifford-tapering.fsx
// Prereq:   dotnet build --configuration Release

#r "nuget: FockMap"

open System.Numerics
open Encodings

printfn ""
printfn "Chapter 11: General Clifford Tapering"
printfn "======================================"

// ── Symplectic representation ──
printfn ""
printfn "Symplectic representation:"
let sv = toSymplectic (PauliRegister("XYZ", Complex.One))
printfn "  XYZ → X bits = %A, Z bits = %A" sv.X sv.Z

// ── Commutativity check ──
printfn ""
printfn "Commutativity:"
let a = toSymplectic (PauliRegister("XX", Complex.One))
let b = toSymplectic (PauliRegister("ZZ", Complex.One))
printfn "  XX and ZZ commute? %b" (commutes a b)

let c = toSymplectic (PauliRegister("XZ", Complex.One))
let d = toSymplectic (PauliRegister("YI", Complex.One))
printfn "  XZ and YI commute? %b" (commutes c d)

// ── Heisenberg model: Clifford finds symmetries diagonal can't ──
printfn ""
printfn "Heisenberg model (XX + YY + ZZ + ZI + IZ):"

let heis =
    [| PauliRegister("XX", Complex(0.5, 0.0))
       PauliRegister("YY", Complex(0.5, 0.0))
       PauliRegister("ZZ", Complex(-0.3, 0.0))
       PauliRegister("ZI", Complex(0.2, 0.0))
       PauliRegister("IZ", Complex(0.2, 0.0)) |]
    |> PauliRegisterSequence

let diagCount = (diagonalZ2SymmetryQubits heis).Length
let fullCount = z2SymmetryCount heis
printfn "  Diagonal Z₂ qubits: %d" diagCount
printfn "  General Z₂ symmetries: %d" fullCount
printfn ""

// ── Unified taper pipeline ──
let result = taper defaultTaperingOptions heis
printfn "  After Clifford tapering: %d → %d qubits" result.OriginalQubitCount result.TaperedQubitCount
printfn "  Generators found: %d" result.Generators.Length
printfn "  Clifford gates applied: %d" result.CliffordGates.Length
printfn "  Tapered Hamiltonian: %s" (result.Hamiltonian.ToString())
