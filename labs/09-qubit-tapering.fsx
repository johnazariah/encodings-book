#if INTERACTIVE
#r "nuget: FockMap, 0.9.0"
#endif

open System
open System.Numerics
open Encodings
open Encodings.Tapering

printfn "============================================================"
printfn "Lab 09 — Qubit Tapering (Diagonal Z2, symbolic)"
printfn "============================================================"

// A fully Z-diagonal 4-qubit toy: all four qubits are diagonal Z2
// symmetries. Below we taper qubits 1 and 3 to illustrate a partial choice.
let hamiltonian =
    [|
        PauliRegister("ZIZI", Complex(0.8, 0.0))
        PauliRegister("ZZII", Complex(-0.4, 0.0))
        PauliRegister("IIZZ", Complex(0.3, 0.0))
        PauliRegister("IZIZ", Complex(0.2, 0.0))
    |]
    |> PauliRegisterSequence

printfn "Original Hamiltonian:"
printfn "%s\n" (hamiltonian.ToString())

let symmetryQubits = diagonalZ2SymmetryQubits hamiltonian
printfn "Detected diagonal Z2 qubits: %A" symmetryQubits

let generators = diagonalZ2Generators hamiltonian
printfn "Generators:"
generators |> Array.iter (fun g -> printfn "  %s" (g.ToString()))
printfn ""

// Compare two sectors on the same removable qubits.
let sectorPlus = [ (1, 1); (3, 1) ]
let sectorMinus = [ (1, -1); (3, 1) ]

let taperedPlus = taperDiagonalZ2 sectorPlus hamiltonian
let taperedMinus = taperDiagonalZ2 sectorMinus hamiltonian

printfn "(+1,+1) sector: removed %A, qubits %d -> %d"
    taperedPlus.RemovedQubits taperedPlus.OriginalQubitCount taperedPlus.TaperedQubitCount
printfn "  %s\n" (taperedPlus.Hamiltonian.ToString())

printfn "(-1,+1) sector: removed %A, qubits %d -> %d"
    taperedMinus.RemovedQubits taperedMinus.OriginalQubitCount taperedMinus.TaperedQubitCount
printfn "  %s\n" (taperedMinus.Hamiltonian.ToString())

// Convenience helper: taper all detected diagonal symmetries in +1 sector.
let auto = taperAllDiagonalZ2WithPositiveSector hamiltonian
printfn "Auto +1 sector result: %s" (auto.Hamiltonian.ToString())
