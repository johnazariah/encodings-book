(**
# Encoding the H₂ Molecule

This tutorial walks through encoding the electronic Hamiltonian of molecular
hydrogen (H₂) using the Jordan-Wigner transformation. This is the canonical
example for quantum chemistry on quantum computers.

## Physical Setup

We use the STO-3G minimal basis set, which gives:
- **2 spatial orbitals**: σg (bonding) and σu (antibonding)
- **4 spin-orbitals**: σg↑, σg↓, σu↑, σu↓
- **4 qubits** after encoding

The electronic Hamiltonian in second quantization is:

$$H = \sum_{pq} h_{pq} \, a^\dagger_p a_q + \frac{1}{2} \sum_{pqrs} \langle pq | rs \rangle \, a^\dagger_p a^\dagger_q a_s a_r$$

where:
- $h_{pq}$ are one-body (kinetic + nuclear) integrals
- $\langle pq | rs \rangle$ are two-body (electron-electron) integrals

## Setup
*)

#r "nuget: FockMap"

open System.Numerics
open Encodings

(**
## Molecular Integrals

These are the STO-3G integrals for H₂ at bond length
R = 0.74 Å, matching the manuscript's Chapter 3 tables.

### Spin-Orbital Indexing

| Index | Spin-Orbital |
|-------|--------------|
| 0     | σg↑          |
| 1     | σg↓          |
| 2     | σu↑          |
| 3     | σu↓          |

### One-Body Integrals h_{pq}

These include kinetic energy and electron-nuclear attraction.
In a spin-orbital basis with real spatial orbitals, only diagonal-in-spin
terms are nonzero:
*)

let nSpinOrbitals = 4u

let oneBodyIntegrals = Map.ofList [
    // Diagonal elements (same spatial orbital)
    ("0,0", Complex(-1.2563390730, 0.0))  // h₀₀ = ⟨σg↑|h|σg↑⟩
    ("1,1", Complex(-1.2563390730, 0.0))  // h₁₁ = ⟨σg↓|h|σg↓⟩
    ("2,2", Complex(-0.4718960244, 0.0))  // h₂₂ = ⟨σu↑|h|σu↑⟩
    ("3,3", Complex(-0.4718960244, 0.0))  // h₃₃ = ⟨σu↓|h|σu↓⟩
]

(**
### Two-Body Integrals ⟨pq|rs⟩

These are the electron-electron repulsion integrals in physicist's notation.
Symmetries reduce the number of unique values significantly.
*)

let twoBodyIntegrals = Map.ofList [
    // Same-spin αα-αα
    ("0,0,0,0", Complex(0.6744887663, 0.0))
    ("2,2,2,2", Complex(0.6973979495, 0.0))
    ("0,2,2,0", Complex(0.1809312700, 0.0))
    ("2,0,0,2", Complex(0.1809312700, 0.0))
    ("0,2,0,2", Complex(0.6975782469, 0.0))
    ("2,0,2,0", Complex(0.6975782469, 0.0))
    ("0,0,2,2", Complex(0.6636340479, 0.0))
    ("2,2,0,0", Complex(0.6636340479, 0.0))

    // Same-spin ββ-ββ
    ("1,1,1,1", Complex(0.6744887663, 0.0))
    ("3,3,3,3", Complex(0.6973979495, 0.0))
    ("1,3,3,1", Complex(0.1809312700, 0.0))
    ("3,1,1,3", Complex(0.1809312700, 0.0))
    ("1,3,1,3", Complex(0.6975782469, 0.0))
    ("3,1,3,1", Complex(0.6975782469, 0.0))
    ("1,1,3,3", Complex(0.6636340479, 0.0))
    ("3,3,1,1", Complex(0.6636340479, 0.0))

    // Cross-spin αβ-αβ
    ("0,1,0,1", Complex(0.6744887663, 0.0))
    ("0,3,0,3", Complex(0.6636340479, 0.0))
    ("2,1,2,1", Complex(0.6636340479, 0.0))
    ("2,3,2,3", Complex(0.6973979495, 0.0))
    ("0,1,2,3", Complex(0.6975782469, 0.0))
    ("2,3,0,1", Complex(0.6975782469, 0.0))
    ("0,3,2,1", Complex(0.1809312700, 0.0))
    ("2,1,0,3", Complex(0.1809312700, 0.0))

    // Cross-spin βα-βα
    ("1,0,1,0", Complex(0.6744887663, 0.0))
    ("1,2,1,2", Complex(0.6636340479, 0.0))
    ("3,0,3,0", Complex(0.6636340479, 0.0))
    ("3,2,3,2", Complex(0.6973979495, 0.0))
    ("1,0,3,2", Complex(0.6975782469, 0.0))
    ("3,2,1,0", Complex(0.6975782469, 0.0))
    ("1,2,3,0", Complex(0.1809312700, 0.0))
    ("3,0,1,2", Complex(0.1809312700, 0.0))
]

(**
## Coefficient Lookup Function

The `computeHamiltonian` function needs a way to look up coefficients
for arbitrary index combinations. We provide a factory function:
*)

let coefficientFactory (key : string) : Complex option =
    match key.Split(',').Length with
    | 2 -> oneBodyIntegrals.TryFind key   // One-body: "p,q" key
    | 4 -> twoBodyIntegrals.TryFind key   // Two-body: "p,q,r,s" key
    | _ -> None

(**
## Computing the Hamiltonian

Now we call `computeHamiltonian` with Jordan-Wigner encoding (the default).
This function:
1. Iterates over all one-body index pairs (p,q)
2. Iterates over all two-body index quadruples (p,q,r,s)
3. Looks up coefficients using our factory
4. Encodes each term using Jordan-Wigner
5. Combines and simplifies the result
*)

printfn "Computing H₂ Hamiltonian with Jordan-Wigner encoding...\n"

let hamiltonian = computeHamiltonian coefficientFactory nSpinOrbitals
let terms = hamiltonian.SummandTerms

(**
## Examining the Results

Let's display the qubit Hamiltonian:
*)

printfn "════════════════════════════════════════════════════════════"
printfn " H₂ Qubit Hamiltonian (Jordan-Wigner Encoding)"
printfn "════════════════════════════════════════════════════════════\n"
printfn " Number of Pauli terms: %d\n" terms.Length

printfn " Pauli terms (coefficient × Pauli string):"
printfn " ───────────────────────────────────────────"

for term in terms do
    let c = term.Coefficient
    let sign = if c.Real >= 0.0 then "+" else ""
    printfn "   %s%.6f  %s" sign c.Real term.Signature

(**
## Understanding the Output

The H₂ Hamiltonian typically produces 15 unique Pauli terms:

| Pattern | Physical Origin |
|---------|-----------------|
| `IIII`  | Constant energy offset |
| `ZZII`, `IZZI`, etc. | Number operators from h_{pp} a†_p a_p |
| `XXYY`, `YYXX`, etc. | Hopping terms from h_{pq} a†_p a_q |
| `ZZZZ` patterns | Two-body Coulomb repulsion |

## Why This Matters

The qubit Hamiltonian H can be measured on a quantum computer:
1. Each Pauli string is a tensor product of single-qubit observables
2. Measure each term separately (or use grouping strategies)
3. Combine measurements weighted by coefficients

This is the foundation for the Variational Quantum Eigensolver (VQE)
algorithm, which finds the ground state energy of molecules.

## Next Steps

Try modifying this tutorial to:
- Change the bond length and see how coefficients change
- Use `computeHamiltonianWith` with different encodings
- Compare the number of terms across encodings
*)

printfn "\n════════════════════════════════════════════════════════════"
printfn " Tutorial complete! H₂ encoded into %d Pauli terms." terms.Length
printfn "════════════════════════════════════════════════════════════"
