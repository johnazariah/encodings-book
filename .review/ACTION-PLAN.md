# ACTION PLAN — From Molecules to Quantum Circuits

**Generated**: 2026-05-11
**Source**: Full 7-part correctness audit across all 23 chapters + appendices + foreword
**Trigger**: Reviewer feedback on Ch19 H₂O correlation claim → full book scrub

---

## Severity Summary

| Severity | Count |
|---|---|
| HIGH | 13 |
| MEDIUM-HIGH | 1 |
| MEDIUM | 31 |
| LOW | 25 |
| **Total** | **70** |

---

## Systemic Issues (fix these and many findings resolve together)

### S1 — H₂O qubit count: 12 vs 14 (affects Ch3, Ch13, Ch17, Ch19, Ch22)

The book uses two different H₂O models interchangeably:
- **14 spin-orbitals** (no frozen core, STO-3G full) — used in Ch19 bond angle scan
- **12 spin-orbitals** (frozen O 1s core) — used in Ch17 cost analysis

Neither convention is wrong, but the book switches between them without flagging it, and some chapters (Ch13) claim "14 qubits, frozen core" which is self-contradictory. CNOT cost numbers computed at 12 qubits are attributed to 14-qubit systems.

**Resolution**: Pick one convention per context (full basis for Ch19 PES scan; frozen core for cost comparisons) and label every table explicitly. Never say "frozen core" with 14 qubits.

**Findings resolved**: p4-f2, p5-f3, p7-f3, p1-f5

### S2 — CNOT scaling tables disagree between Ch17 and Ch22 (affects H₂, LiH, N₂, FeMo-co)

Ch17 and Ch22 both present CNOT-cost comparison tables for the same molecules, but the numbers differ by factors of 2–20× with no explanation. Root causes: mixed tapered/untapered bases, different active spaces, and possibly different Trotter orders.

**Resolution**: Create one canonical source-of-truth table (or recompute from FockMap). State assumptions (tapered vs untapered, basis set, active space, Trotter order) in every table. Cross-reference between chapters.

**Findings resolved**: p5-f1, p5-f4, p5-f5, p5-f6, p7-f1, p7-f5, p7-f6

### S3 — Correlation overclaim narrative (affects Ch1, Ch19, and the book's rhetorical arc)

The book overclaims the role of electron correlation at multiple points:
- Ch1: says classical correlation methods scale exponentially (CCSD(T) does not)
- Ch1: says correlation "determines" qualitative chemistry (HF captures many qualitative features)
- Ch19: attributes water's bend to correlation (HF already predicts it)
- Ch19: attributes STO-3G angle error to correlation (it's mostly a mean-field basis-set issue)

These are connected — the Ch1 overclaim primes the reader for the Ch19 error.

**Resolution**: Restrict exponential-scaling claim to Full CI / exact methods. Add qualifier that HF captures many qualitative features. Rewrite "Why Does Water Bend?" to credit mean-field effects, with correlation as quantitative refinement. Show HF and FCI minima side by side.

**Findings resolved**: p1-f1, p1-f6, p6-f1, p6-f3, h2o-correlation-fix (reviewer feedback)

### S4 — V_nn inclusion/exclusion confusion (affects Ch6, Ch9)

Ch6 recipe says "add V_nn to IIII coefficient." Ch9 eigenvalue table has 0-electron eigenvalue = 0, proving V_nn was NOT added. The "Common Mistakes" box reinforces the wrong advice.

**Resolution**: Clarify that the 15-term Hamiltonian is the purely electronic Hamiltonian (no V_nn). Update recipe, Common Mistakes box, and eigenvalue labels. Show V_nn addition as a separate post-diagonalization step.

**Findings resolved**: p3-f2, p3-f9

---

## HIGH-severity Findings (individual)

### H1 — Ch2: Complex-orbital symmetry formula is wrong
**ID**: p1-f2
**Fix**: Replace `[pq|rs] = [qp|rs]* = [pq|sr]*` with `[pq|rs] = [qp|sr]* = [rs|pq]`

### H2 — Ch4: Tensor-product arithmetic error (4×4×2=8)
**ID**: p2-f1
**Fix**: Replace "4×4×2=8-dimensional" with "2×2×2=8-dimensional" (or "8×8 matrix")

### H3 — Ch9: HF energy inconsistent with book's own integrals
**ID**: p3-f1
**Fix**: Replace E_HF = −1.1168 Ha with value computed from book's integrals (−1.1230 Ha). Change 15.9 kcal/mol correlation energy to ~12 kcal/mol. Reconcile with Exercise 2.

### H4 — Ch13: CNOT count arithmetic wrong (14 should be 12)
**ID**: p4-f1
**Fix**: Replace 14 with 12 in both tables (6-qubit benchmark and Impact on Circuit Cost).

### H5 — Ch18/21: Swapped energy labels + wrong value in verification checklist
**ID**: p6-f2
**Fix**: −1.1373 Ha is total (not electronic-only). −1.8572 Ha should be −1.8524 Ha. Fix in both Ch18 and Ch21.

### H6 — Ch22: Internal contradiction — success probability 500× vs 10×
**ID**: p7-f2
**Fix**: Verify which CNOT counts the "Application" section compares. Unify the improvement factor.

---

## MEDIUM-severity Findings

| ID | Chapter | Title |
|---|---|---|
| p3-f3 | Ch7 | False claim: identical Pauli strings across encodings at n=4 |
| p2-f2 | Ch4 | Wrong cross-reference to Ch5 density matrix discussion |
| p2-f3 | Ch4 | T gate described as π/8 rotation (it's π/4) |
| p2-f4 | Ch4 | "16 amplitudes matching H₂ Hilbert space" (embedded, not matching) |
| p2-f5 | Ch4 | Pauli basis scope creep: Hermitian → "any operator" |
| p1-f3 | Ch1-3 | V_nn inconsistent with stated bond length (0.7414 Å vs actual ~0.74 Å) |
| p1-f4 | Ch2 | "Exchange integral" mislabelling (should be pair-scattering) |
| p3-f4 | Ch5 | Fermion sign explanation identifies wrong orbitals |
| p3-f5 | Ch6 | Exchange-term derivation swaps integral/operator pairings |
| p3-f6 | Ch5 | Number-operator weight-1 overgeneralized to all encodings |
| p3-f7 | Ch5-9 | Qubit ordering convention never declared |
| p4-f3 | Ch12 | CNOT conjugation rules for Z are swapped |
| p4-f4 | Ch10 | H₂ Pauli-operator column table is garbled |
| p5-f2 | Ch14 | Qubitization error scaling omits dominant αt term |
| p5-f7 | Ch15 | Second-order 1-norm bound presented as standard |
| p5-f8 | Ch7-17 | H₂ CNOT counts differ across encodings despite "identical strings" claim |
| p6-f4 | Ch18 | Unexplained 1/2 factor in Trotter exponent |
| p6-f5 | Ch20 | QPE ancilla formula missing 2π/t correction |
| p6-f6 | Ch21 | Q# TrotterStep allocates own qubits; prose says caller does |
| p7-f4 | Ch23 | Honesty-policy violations: uses "will" three times |
| p7-f7 | appendix | JW mapping convention (X−iY vs X+iY) not stated |
| p7-f8 | appendix | Cookbook chapter numbers don't match manuscript |
| p7-f9 | Ch22 | FeMo-co active-space numbers need citation |

---

## LOW-severity Findings

| ID | Chapter | Title |
|---|---|---|
| p2-f6 | Ch4 | Z gate measurement probability claim lacks basis qualifier |
| p2-f7 | Ch4 | "Commuting qubit tensor products" oversimplification |
| p2-f8 | Ch4 | Hardware table internal inconsistency (Quantinuum ratio) |
| p2-f9 | Ch4 | Backward cross-ref to Ch1 Pauli space that doesn't exist |
| p2-f10 | Ch4 | Prerequisites understated |
| p1-f7 | Ch2 | Physicist's notation symmetry count needs clarifying note |
| p1-f8 | lab | Lab 02 integral table inconsistent with Ch3 code |
| p3-f8 | Ch8 | Vlasov tree children: (9,10,11) should be (10,11,12) |
| p4-f5 | Ch12 | "Pauli strings square to ±I" — should be just I |
| p4-f6 | Ch12 | Null-space complexity O(n³) is wrong (should be O(L·n²)) |
| p4-f7 | Ch13 | "5× total reduction" unverifiable due to qubit-count issue |
| p4-f8 | Ch12 | Heisenberg worked example abandoned at Step 4 |
| p4-f9 | Ch10-13 | "Taper first, then encode" misleading pipeline ordering |
| p5-f9 | Ch14 | Cross-reference numbering off by one |
| p5-f10 | Ch14 | Trotter error notation non-standard |
| p5-f11 | Ch15 | Time-step heuristic uncited |
| p5-f13 | Ch16 | n=32 cost table values unverifiable without molecule spec |
| p6-f7 | Ch21 | Q# namespaces deprecated (pre-2024 QDK) |
| p6-f8 | Ch20 | "One readout" for QPE overstates determinism |
| p6-f9 | Ch21 | Rz angle convention not reconciled with standard definition |
| p7-f10 | Ch22 | Physical qubit overhead range uncited |
| p7-f11 | foreword | "Zero runtime dependencies" technically incorrect |
| p7-f12 | Ch23 | "Cambridge Quantum's tket" — outdated branding |
| p7-f13 | Ch22 | CCSD(T) ~50 atom limit imprecise |
| p7-f14 | Ch23 | "Half of QEC theory" overstatement |

---

## Recommended Fix Order

1. **Systemic S3** (correlation narrative) — rewrite Ch1 claims + Ch19 "Why Does Water Bend?" + Ch19 basis-set paragraph. Highest-impact single fix.
2. **Systemic S2** (CNOT tables) — reconcile Ch17 and Ch22 from a single canonical computation. Second-highest trust risk.
3. **Systemic S4** (V_nn) — clarify in Ch6, update Ch9 labels. Prevents reader debugging confusion.
4. **Systemic S1** (H₂O qubit count) — label every table. Straightforward but widespread.
5. **Individual HIGHs** (H1–H6) — each is a local fix.
6. **MEDIUM findings** — batch by chapter for efficiency.
7. **LOW findings** — batch in a polish pass.
