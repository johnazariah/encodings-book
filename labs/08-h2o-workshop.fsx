(**
---
title: "Workshop: Water Molecule — From Geometry to Quantum Circuit"
category: Tutorials
categoryindex: 3
index: 8
---
*)

(**
# Workshop: Water Molecule — From Geometry to Quantum Circuit

_A complete, self-contained lesson for teaching fermion-to-qubit encoding
using a real molecule (H₂O) that students already know from general chemistry._

## Learning Objectives

By the end of this workshop, you will be able to:

1. **Explain** the full quantum simulation pipeline: molecule → integrals → encoding → Hamiltonian
2. **Run** a Hartree-Fock calculation and extract molecular integrals
3. **Encode** a 12-qubit Hamiltonian using three different encodings
4. **Compare** the CNOT cost of each encoding for Trotterization
5. **Predict** the equilibrium bond angle from a potential energy surface scan

## Prerequisites

- Basic quantum mechanics (wavefunctions, Hamiltonians, eigenvalues)
- Some familiarity with molecular orbital theory (LCAO, Hartree-Fock)
- Comfort with command-line tools

## Part 1: The Molecule

Water (H₂O) has:
- 10 electrons (8 from O, 1 from each H)
- In the STO-3G minimal basis: 7 spatial molecular orbitals → 14 spin-orbitals
- We freeze the oxygen 1s core: 6 active spatial orbitals → **12 spin-orbitals → 12 qubits**

The key molecular properties we want to recover:
- **H-O-H bond angle**: 104.52° (experiment)
- **O-H bond length**: 0.9572 Å (experiment)
- **Ground-state energy**: −75.0 Ha (HF/STO-3G)

## Part 2: Generating Integrals (Python/PySCF)

Before running this script, generate the integrals:

```bash
# From the repository root:
python examples/h2o_integrals.py --output examples/h2o_integrals.json
```

This runs Hartree-Fock on H₂O at the experimental geometry, freezes the
O 1s core, and exports one-body (h_pq) and two-body (⟨pq|rs⟩) integrals
in the molecular orbital basis as JSON.

## Part 3: Loading Integrals and Encoding

Now we load those integrals into FockMap and encode the Hamiltonian:
*)

#r "nuget: FockMap"

open System
open System.IO
open System.Numerics
open System.Text.Json
open Encodings

(**
### 3a. Load integrals from JSON
*)

let jsonPath =
    let candidates = [
        Path.Combine(__SOURCE_DIRECTORY__, "h2o_integrals.json")
        Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "examples", "h2o_integrals.json")
    ]
    candidates |> List.find File.Exists

let doc = JsonDocument.Parse(File.ReadAllText(jsonPath))
let root = doc.RootElement

let nSpinOrbitals = root.GetProperty("n_spin_orbitals").GetUInt32()
let hfEnergy      = root.GetProperty("hf_energy").GetDouble()
let nucRepulsion  = root.GetProperty("nuclear_repulsion").GetDouble()
let coreEnergy    = root.GetProperty("core_energy").GetDouble()

printfn "╔══════════════════════════════════════════════════════════════╗"
printfn "║       Workshop: H₂O Encoding Pipeline                      ║"
printfn "╠══════════════════════════════════════════════════════════════╣"
printfn "║  Molecule:       H₂O (STO-3G, frozen core)                 ║"
printfn "║  Spin-orbitals:  %-3d (= qubits)                            ║" nSpinOrbitals
printfn "║  HF energy:      %.10f Ha                  ║" hfEnergy
printfn "║  Nuclear repul.: %.10f Ha                   ║" nucRepulsion
printfn "║  Core energy:    %.10f Ha                 ║" coreEnergy
printfn "╚══════════════════════════════════════════════════════════════╝"

(**
### 3b. Build the coefficient factory

FockMap's `computeHamiltonianWith` expects a function
`string -> Complex option` that returns the integral value for a
given index key (e.g., `"0,2"` for h₀₂, `"0,1,2,3"` for ⟨01|23⟩).
*)

let oneBody = root.GetProperty("one_body")
let twoBody = root.GetProperty("two_body")

let coefficientFactory (key : string) : Complex option =
    let tryGet (element : JsonElement) =
        match element.TryGetProperty(key) with
        | true, v -> Some (Complex(v.GetDouble(), 0.0))
        | _ -> None
    // Keys are comma-separated: "p,q" (one-body) or "p,q,r,s" (two-body)
    // Try one-body first, then two-body
    match tryGet oneBody with
    | Some _ as result -> result
    | None -> tryGet twoBody

printfn "\n  One-body integral count: %d"
    (oneBody.EnumerateObject() |> Seq.length)
printfn "  Two-body integral count: %d"
    (twoBody.EnumerateObject() |> Seq.length)

(**
## Part 4: Encoding the Hamiltonian

We encode H₂O under three encodings and compare:
*)

let encoders : (string * (LadderOperatorUnit -> uint32 -> uint32 -> PauliRegisterSequence)) list =
    [ "Jordan-Wigner",  jordanWignerTerms
      "Bravyi-Kitaev",  bravyiKitaevTerms
      "Ternary Tree",   ternaryTreeTerms ]

let pauliWeight (sig' : string) =
    sig' |> Seq.sumBy (fun c -> if c = 'I' then 0 else 1)

let cnotsPerRotation w = max 0 (2 * (w - 1))

printfn "\n╔══════════════════════════════════════════════════════════════════════════╗"
printfn "║       Encoding Comparison — H₂O (12 qubits)                            ║"
printfn "╠══════════════════════════════════════════════════════════════════════════╣"
printfn "║ Encoding        │ Terms │ Max w │ Avg w │ CNOTs/step │ Reduction       ║"
printfn "╠═════════════════╪═══════╪═══════╪═══════╪════════════╪═════════════════╣"

type EncodingStats = {
    Name: string
    Terms: int
    MaxWeight: int
    AvgWeight: float
    TotalCnots: int
}

let results =
    encoders |> List.map (fun (name, encode) ->
        let ham = computeHamiltonianWithParallel encode coefficientFactory nSpinOrbitals
        let terms = ham.DistributeCoefficient.SummandTerms
        let nonIdentity = terms |> Array.filter (fun t -> pauliWeight t.Signature > 0)
        let maxW = nonIdentity |> Array.map (fun t -> pauliWeight t.Signature) |> Array.max
        let avgW = nonIdentity |> Array.averageBy (fun t -> float (pauliWeight t.Signature))
        let totalCnots = nonIdentity |> Array.sumBy (fun t -> cnotsPerRotation (pauliWeight t.Signature))
        { Name = name; Terms = nonIdentity.Length; MaxWeight = maxW;
          AvgWeight = avgW; TotalCnots = totalCnots })

let jwCnots = results.[0].TotalCnots
for stats in results do
    let reduction =
        if stats.Name = "Jordan-Wigner" then "baseline"
        elif stats.TotalCnots < jwCnots then
            sprintf "%.0f%% fewer CNOTs" (100.0 * float (jwCnots - stats.TotalCnots) / float jwCnots)
        else sprintf "%.0f%% more" (100.0 * float (stats.TotalCnots - jwCnots) / float jwCnots)
    printfn "║ %-15s │  %3d  │  %3d  │ %4.1f  │   %5d    │ %-15s ║"
        stats.Name stats.Terms stats.MaxWeight stats.AvgWeight stats.TotalCnots reduction

printfn "╚══════════════════════════════════════════════════════════════════════════╝"

(**
## Part 5: What the Numbers Mean

### Discussion Questions for Students

1. **Why does Ternary Tree have fewer CNOTs?**
   Each Pauli rotation uses $2(w-1)$ CNOTs. The ternary tree's
   $O(\log_3 n)$ weight vs JW's $O(n)$ directly reduces gate count.

2. **Why is JW still popular?**
   JW preserves locality perfectly for 1D chains — important for
   lattice Hamiltonians. For molecules with all-to-all orbital
   interactions, tree encodings win.

3. **What determines the number of Pauli terms?**
   The number of non-zero integrals. All encodings produce the same
   number of terms (they represent the same physics), but the
   weight of those terms differs.

4. **How many Trotter steps would we need?**
   Depends on the desired accuracy $\epsilon$ and simulation time $t$.
   First-order Trotter error scales as $O(t^2/r)$ where $r$ is
   the number of steps. The CNOT cost per step is the multiplier.

## Part 6: Sample Pauli Terms

Let's look at a few terms from each encoding to see the structure:
*)

printfn "\n═══ Sample Pauli Terms (first 5 non-identity) ═══\n"
for (name, encode) in encoders do
    let ham = computeHamiltonianWith encode coefficientFactory nSpinOrbitals
    let terms =
        ham.DistributeCoefficient.SummandTerms
        |> Array.filter (fun t -> pauliWeight t.Signature > 0)
        |> Array.sortByDescending (fun t -> abs t.Coefficient.Real)
        |> Array.take (min 5 (ham.DistributeCoefficient.SummandTerms.Length))
    printfn "  %s:" name
    for t in terms do
        let sign = if t.Coefficient.Real >= 0.0 then "+" else ""
        printfn "    %s%.6f  %s  (weight %d, %d CNOTs)"
            sign t.Coefficient.Real t.Signature
            (pauliWeight t.Signature)
            (cnotsPerRotation (pauliWeight t.Signature))
    printfn ""

(**
## Part 7: Scaling Projection

Let's project how the encodings would compare at larger system sizes:
*)

printfn "╔══════════════════════════════════════════════════════════════╗"
printfn "║       Scaling: CNOTs per Worst-Case Rotation               ║"
printfn "╠══════════════════════════════════════════════════════════════╣"
printfn "║ System            │  n  │  JW  │  BK  │ Ternary │ Savings ║"
printfn "╠═══════════════════╪═════╪══════╪══════╪═════════╪═════════╣"

let scalingData = [
    ("H₂ (this lab)",  4,   4,  3,  2)
    ("H₂O (today!)",  12,  12,  4,  4)
    ("N₂",            20,  20,  5,  4)
    ("Fe₂S₂",         40,  40,  6,  5)
    ("FeMo-co",       100, 100, 7,  5)
]

for (name, n, jw, bk, ter) in scalingData do
    let jwC = 2 * (jw - 1)
    let terC = 2 * (ter - 1)
    let savings = sprintf "%d×" (jwC / (max 1 terC))
    printfn "║ %-17s │ %3d │ %4d │ %4d │   %4d  │   %-4s  ║"
        name n jwC (2*(bk-1)) terC savings

printfn "╚══════════════════════════════════════════════════════════════╝"

(**
## Part 8: Bond Angle Scan — Finding the Equilibrium Geometry

This is where computational quantum chemistry gets exciting. By computing
the total energy at many different H-O-H angles, we trace the
**potential energy surface** (PES). The minimum of the PES gives the
equilibrium bond angle — a prediction we can compare to experiment.

First, generate the scan data:

```bash
python examples/h2o_integrals.py --scan 60 180 25 --output examples/h2o_scan.json
```

The scan JSON is an array of integral sets, one per geometry.
The HF energies are pre-computed by PySCF — we just read them:
*)

let scanPath =
    let local = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "h2o_scan.json")
    if System.IO.File.Exists(local) then local
    else System.IO.Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "examples", "h2o_scan.json")

let scanJson = System.IO.File.ReadAllText(scanPath)
let scanDoc = JsonDocument.Parse(scanJson)
let geometries = scanDoc.RootElement.EnumerateArray() |> Seq.toArray

// ── Step 1: Extract the PES from the pre-computed HF energies ──────────

type PesPoint = { Angle: float; Energy: float }

let pesData =
    geometries |> Array.map (fun geom ->
        { Angle  = geom.GetProperty("geometry").GetProperty("angle_deg").GetDouble()
          Energy = geom.GetProperty("hf_energy").GetDouble() })

let minPoint = pesData |> Array.minBy (fun p -> p.Energy)

printfn "\n╔══════════════════════════════════════════════════════════════╗"
printfn "║       Bond Angle Scan — Potential Energy Surface            ║"
printfn "╠══════════════════════════════════════════════════════════════╣"
printfn "║  Angle (°)  │  HF Energy (Ha)   │  ΔE (mHa)              ║"
printfn "╠═════════════╪═══════════════════╪═════════════════════════╣"

for p in pesData do
    let deltaE = (p.Energy - minPoint.Energy) * 1000.0
    let marker = if abs(p.Angle - minPoint.Angle) < 0.1 then "  ◄ min" else ""
    printfn "║    %5.0f°    │  %15.8f  │   %7.2f%s              ║"
        p.Angle p.Energy deltaE marker

printfn "╚══════════════════════════════════════════════════════════════╝"
printfn ""
printfn "  Equilibrium angle: %.1f° (HF/STO-3G)" minPoint.Angle
printfn "  Experimental:      104.52°"
printfn "  Ground-state E:    %.8f Ha" minPoint.Energy

// ASCII potential energy curve
printfn "\n═══ Potential Energy Surface (ASCII) ═══\n"

let maxDelta = pesData |> Array.map (fun p -> (p.Energy - minPoint.Energy) * 1000.0) |> Array.max
let barWidth = 60

for p in pesData do
    let deltaE = (p.Energy - minPoint.Energy) * 1000.0
    let barLen = int (float barWidth * deltaE / maxDelta)
    let bar = String.replicate barLen "█"
    let marker = if abs(p.Angle - minPoint.Angle) < 0.1 then " ◄ min" else ""
    printfn "  %3.0f° │%s%s" p.Angle bar marker

printfn ""
printfn "  Energy range: %.2f mHa (= %.1f kJ/mol)" maxDelta (maxDelta * 2.6255)
printfn "  Each █ ≈ %.2f mHa" (maxDelta / float barWidth)

(**
### Step 2: Pauli skeleton precomputation

Here's the key insight: the Pauli strings for each operator product
depend ONLY on the encoding and system size — not on the integral
values (which change with geometry). We can **precompute the Pauli
structure once per encoding** and then cheaply "dress" it with
different coefficients for each bond angle.

This is `computeHamiltonianSkeleton` — it precomputes all Pauli
strings for the n² one-body and n⁴ two-body operator products,
parallelised across all CPU cores. Then `applyCoefficients` just
multiplies in the integral values — no Pauli algebra at all.
*)

let buildFactory (geom : JsonElement) =
    // Pre-convert JSON to Dictionary for O(1) lookups.
    // JsonElement.TryGetProperty does linear scans — fatal in the inner loop.
    let dict = System.Collections.Generic.Dictionary<string, Complex>()
    for prop in geom.GetProperty("one_body").EnumerateObject() do
        dict.[prop.Name] <- Complex(prop.Value.GetDouble(), 0.0)
    for prop in geom.GetProperty("two_body").EnumerateObject() do
        dict.[prop.Name] <- Complex(prop.Value.GetDouble(), 0.0)
    fun (key : string) ->
        match dict.TryGetValue key with
        | true, v -> Some v
        | _ -> None

let n_scan = geometries.[0].GetProperty("n_spin_orbitals").GetUInt32()

// Build one skeleton per encoding.  We use computeHamiltonianSkeletonFor
// with a union factory that includes ALL keys from ALL geometries.  This
// is correct because different geometries can have different non-zero
// integrals (some vanish by symmetry at certain angles).
let sw = System.Diagnostics.Stopwatch.StartNew()

let unionFactory =
    let allKeys = System.Collections.Generic.HashSet<string>()
    for geom in geometries do
        for prop in geom.GetProperty("one_body").EnumerateObject() do
            ignore <| allKeys.Add prop.Name
        for prop in geom.GetProperty("two_body").EnumerateObject() do
            ignore <| allKeys.Add prop.Name
    fun key -> if allKeys.Contains key then Some Complex.One else None

let skeletons =
    encoders |> List.map (fun (name, encode) ->
        let skel = computeHamiltonianSkeletonFor encode unionFactory n_scan
        printfn "  Skeleton %-14s built: %d one-body + %d two-body entries"
            name skel.OneBody.Length skel.TwoBody.Length
        (name, skel))

printfn "  Skeleton build: %.1f s (parallelised across all cores)" sw.Elapsed.TotalSeconds

// Now apply coefficients at EVERY angle — near-instant
type ScanResult = { Angle: float; JwCnots: int; BkCnots: int; TtCnots: int }

let sw2 = System.Diagnostics.Stopwatch.StartNew()

let scanResults =
    geometries |> Array.map (fun geom ->
        let angle = geom.GetProperty("geometry").GetProperty("angle_deg").GetDouble()
        let factory = buildFactory geom

        let cnots =
            skeletons |> List.map (fun (name, skel) ->
                let ham = applyCoefficients skel factory
                let terms = ham.DistributeCoefficient.SummandTerms
                let nonId = terms |> Array.filter (fun t -> pauliWeight t.Signature > 0)
                nonId |> Array.sumBy (fun t -> cnotsPerRotation (pauliWeight t.Signature)))

        { Angle = angle; JwCnots = cnots.[0]; BkCnots = cnots.[1]; TtCnots = cnots.[2] })

printfn "  Coefficient application: %.0f ms for %d geometries × %d encodings"
    sw2.Elapsed.TotalMilliseconds geometries.Length skeletons.Length

printfn "\n═══ CNOT Cost Across the Potential Energy Surface ═══\n"
printfn "  Angle │  JW CNOTs │  BK CNOTs │  TT CNOTs │ TT saving"
printfn "  ──────┼───────────┼───────────┼───────────┼──────────"

for r in scanResults do
    let ttSave = sprintf "%.0f%%" (100.0 * float (r.JwCnots - r.TtCnots) / float (max 1 r.JwCnots))
    printfn "  %4.0f° │    %5d  │    %5d  │    %5d  │   %s"
        r.Angle r.JwCnots r.BkCnots r.TtCnots ttSave

printfn ""
printfn "  Observation: CNOT savings are consistent across ALL %d geometries —"
    scanResults.Length
printfn "  the Ternary Tree advantage is structural, not chemistry-dependent."

// CNOT savings bar chart across the full PES
printfn "\n═══ CNOT Savings: Ternary Tree vs Jordan-Wigner ═══\n"

for r in scanResults do
    let pctSave = 100.0 * float (r.JwCnots - r.TtCnots) / float (max 1 r.JwCnots)
    let barLen = int (float 40 * pctSave / 30.0) |> min 40
    let bar = String.replicate barLen "▓"
    printfn "  %3.0f° │%s %.0f%%" r.Angle bar pctSave

printfn ""

(**
### Discussion: What Did We Learn from the Scan?

1. **The minimum is at ~100°** (HF/STO-3G), close to the experimental
   104.52°. The discrepancy comes from the minimal basis set — a better
   basis (cc-pVDZ, cc-pVTZ) would give a more accurate angle.

2. **CNOT savings are consistent across geometries**: the Ternary Tree
   advantage holds at every angle, not just the equilibrium geometry.
   This means encoding choice is geometry-independent — a structural
   property of the encoding, not an accident of the chemistry.

3. **Skeleton precomputation is the key to speed**: building the Pauli
   skeleton once per encoding and then applying coefficients for each
   geometry reduced 75 full Hamiltonian builds to 3 skeleton builds
   plus 75 near-instant coefficient applications.

4. **Energy barrier to linearity**: The energy difference between the
   equilibrium angle and 180° tells us how much energy it costs to
   straighten the molecule — a real experimental observable.
*)

(**
## Summary

You've just completed the full quantum simulation pipeline for a real molecule:

1. **PySCF** computed the Hartree-Fock wavefunction and extracted integrals
2. **FockMap** encoded those integrals as a qubit Hamiltonian
3. Three encodings produced **identical physics** but different **circuit costs**
4. The **potential energy surface** correctly predicts the bond angle
5. **CNOT savings from tree encodings are consistent across all geometries**
6. At FeMo-co scale (100 qubits), encoding choice is the difference
   between feasible and infeasible simulation

### Take-Home Messages

- **Encoding is not optional** — it's a required step between chemistry and quantum hardware
- **Encoding choice matters** — it directly determines circuit depth
- **The advantage is geometry-independent** — tree encodings win at every bond angle
- **The star-tree theorem** constrains which encodings can be constructed:
  only Construction B (path-based) can produce the optimal sub-linear encodings

### Next Steps

- **Custom trees**: Try [Lab 05](05-custom-tree.html) to build your own
  encoding tree optimised for a specific hardware connectivity.
- **Larger molecules**: Modify the PySCF script for LiH, BeH₂, or N₂ —
  the FockMap pipeline is the same, just more qubits.
- **Better basis sets**: Replace `sto-3g` with `cc-pvdz` in the Python
  script to see how the equilibrium angle prediction improves (and
  the qubit count grows from 12 to ~48).

---

**Previous:** [Trotter Cost Comparison](07-trotter-cost.html)

**Back to:** [Lab index](index.html)
*)
