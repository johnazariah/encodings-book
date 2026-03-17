# Appendix: Library Cookbook Reference

_A quick-reference guide to every public type and function in FockMap._

This appendix summarizes the 15-chapter Cookbook that accompanies the main text.
For full worked examples, see the [Cookbook](../docs/guides/cookbook/index.html) on
the companion website.

---

## Type System at a Glance

| Type | What it represents | Chapter |
|:---|:---|:---:|
| `Pauli` | Single-qubit operator: I, X, Y, Z | 1 |
| `Phase` | Exact phase factor: P1 (+1), M1 (-1), Pi (+i), Mi (-i) | 1 |
| `C<'T>` | Coefficient × single operator | 2 |
| `P<'T>` | Ordered product of operators | 2 |
| `S<'T>` | Sum of products (Hamiltonian shape) | 2 |
| `IxOp<'idx,'op>` | Operator tagged with a mode index | 3 |
| `LadderOperatorUnit` | Raise / Lower / Identity | 4 |
| `PauliRegister` | Fixed-width Pauli string with coefficient | 1, 6 |
| `PauliRegisterSequence` | Sum of Pauli strings (encoding output) | 1, 6 |
| `EncodingScheme` | Three index-set functions → custom encoding | 8 |
| `EncodingTree` | Tree shape for tree-based encodings | 9 |
| `FenwickTree<'a>` | Immutable binary indexed tree | 9 |
| `SymplecticVector` | Binary Pauli representation for tapering | 15 |
| `CliffordGate` | Had / Sgate / CNOT — elementary gates | 15 |
| `Z2TaperingResult` | Tapering result (v1) | 15 |
| `TaperingResult` | Tapering result (v2, with Clifford) | 15 |

## Key Functions

### Encoding

| Function | Signature | What it does |
|:---|:---|:---|
| `jordanWignerTerms` | `LadderOperatorUnit → uint32 → uint32 → PauliRegisterSequence` | JW encoding |
| `bravyiKitaevTerms` | `LadderOperatorUnit → uint32 → uint32 → PauliRegisterSequence` | BK encoding |
| `parityTerms` | `LadderOperatorUnit → uint32 → uint32 → PauliRegisterSequence` | Parity encoding |
| `balancedBinaryTreeTerms` | `LadderOperatorUnit → uint32 → uint32 → PauliRegisterSequence` | Binary tree encoding |
| `ternaryTreeTerms` | `LadderOperatorUnit → uint32 → uint32 → PauliRegisterSequence` | Ternary tree encoding |
| `vlasovTreeTerms` | `LadderOperatorUnit → uint32 → uint32 → PauliRegisterSequence` | Vlasov tree encoding |
| `encodeOperator` | `EncodingScheme → ...` | Custom encoding |

### Hamiltonian Construction

| Function | Signature | What it does |
|:---|:---|:---|
| `computeHamiltonianWith` | `EncoderFn → CoefficientFactory → uint32 → PauliRegisterSequence` | Build Hamiltonian with any encoding |
| `computeHamiltonianWithParallel` | `EncoderFn → CoefficientFactory → uint32 → PauliRegisterSequence` | Parallel version |
| `computeHamiltonianSkeleton` | `EncoderFn → uint32 → HamiltonianSkeleton` | Pre-compute Pauli structure |
| `applyCoefficients` | `HamiltonianSkeleton → CoefficientFactory → PauliRegisterSequence` | Dress skeleton with integrals |

### Tapering

| Function | Signature | What it does |
|:---|:---|:---|
| `diagonalZ2SymmetryQubits` | `PauliRegisterSequence → int[]` | Find diagonal Z₂ qubits |
| `taperDiagonalZ2` | `(int × int) list → PauliRegisterSequence → Z2TaperingResult` | Apply diagonal tapering |
| `findCommutingGenerators` | `PauliRegisterSequence → SymplecticVector[]` | Find all Z₂ symmetries |
| `taper` | `TaperingOptions → PauliRegisterSequence → TaperingResult` | Unified tapering pipeline |

### Normal Ordering

| Function | Signature | What it does |
|:---|:---|:---|
| `ConstructNormalOrdered` | `S<IxOp<uint32, LadderOperatorUnit>> → ... option` | Fermionic (CAR) normal ordering |
| `constructBosonicNormalOrdered` | `S<IxOp<uint32, LadderOperatorUnit>> → ... option` | Bosonic (CCR) normal ordering |
| `constructMixedNormalOrdered` | `S<IxOp<uint32, SectorLadderOperatorUnit>> → ... option` | Mixed fermion-boson ordering |
