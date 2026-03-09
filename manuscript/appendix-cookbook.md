# Appendix A: Library Cookbook Reference

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
| `bravyiKitaevTerms` | same | BK encoding |
| `parityTerms` | same | Parity encoding |
| `balancedBinaryTreeTerms` | same | Binary tree encoding |
| `ternaryTreeTerms` | same | Ternary tree encoding |
| `encodeOperator` | `EncodingScheme → ...` | Custom encoding |

### Hamiltonian Construction

| Function | What it does |
|:---|:---|
| `computeHamiltonianWith` | Build Hamiltonian with any encoding |
| `computeHamiltonianWithParallel` | Parallel version |
| `computeHamiltonianSkeleton` | Pre-compute Pauli structure |
| `applyCoefficients` | Dress skeleton with integrals |

### Tapering

| Function | What it does |
|:---|:---|
| `diagonalZ2SymmetryQubits` | Find diagonal Z₂ qubits |
| `taperDiagonalZ2` | Apply diagonal tapering |
| `findCommutingGenerators` | Find all Z₂ symmetries |
| `taper` | Unified tapering pipeline |

### Normal Ordering

| Function | What it does |
|:---|:---|
| `ConstructNormalOrdered` | Fermionic (CAR) normal ordering |
| `constructBosonicNormalOrdered` | Bosonic (CCR) normal ordering |
| `constructMixedNormalOrdered` | Mixed fermion-boson ordering |
