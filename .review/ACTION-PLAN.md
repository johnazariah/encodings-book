# ACTION PLAN — From Molecules to Quantum Circuits

**Updated:** 2026-07-24
**Authoritative audit:** `.review/CORRECTNESS-AUDIT.md`
**Baseline HEAD:** `2269f0d3eb87f0d22e1b275e56add2077ad6915d`
**Verified tree:** Current uncommitted remediation
**State:** Direct book correctness and raw-breaking runtime/package
implementation are accepted at `encodings` successor
`3e62bd71b113d78a6f5933858336e06af1e3d9c7`; designated audit also accepts
release-workflow successor `9e1afe267efefc4056f726ea98dadfed8af0214e`.
Final package lineage was merged and released publicly as FockMap `0.9.0` at
source commit `96320a56786393269fd681c67c66df88058a8b8f`; the public NuGet DLL
SHA-256 is
`0ba8ae967ea65d4945a336c1217f8939e41feb63b6f04af617b66537717d25c3`.
The pinned public-package data/pipeline/labs/companions and all book outputs
pass after the selective clarity pass. The book PR may proceed. Zenodo
mixed-rights metadata and the explicitly partial CA-H008/CA-M010/CA-M011 gates
remain. The dated audit addenda are the acceptance and change-control record.

## Status vocabulary

- **Closed (correctness):** Acceptance criteria pass on the verified tree and,
  where relevant, against the accepted source commit.
- **Partial:** The local correction is accepted but a named API/artifact parity
  check remains open.
- **Operational release gate:** Correct source exists, but merge, package
  publication, version pinning, or external metadata work remains.
- **Blocked — benchmark:** Requires a reproducible generated benchmark artifact;
  the unsupported claim remains removed.
- **Historical:** Preserved record of the May 2026 plan; not evidence of current closure.

---

# Implementation partition

This partition preserves all 24 correctness IDs. Some items have a source-independent
repair and a cross-repository acceptance gate; those IDs appear in both lists and
cannot be marked resolved until both parts pass.

## A — Source-independent repair batch

These corrections can be implemented from mathematics, generated data, scripts, and
publishing configuration in this repository once the local companion findings are
synthesized:

| IDs | Work that does not depend on a companion-repository conclusion |
|---|---|
| CA-H001, CA-H002, CA-H003 | Regenerate the 0.74 Å H₂ tensor; replace the invalid worked operator; derive the direct fermionic matrix, JW coefficient table, full sector spectrum, HF/FCI checkpoints, and one canonical machine-readable reference |
| CA-H004 | Remove the identity-coefficient energy proxy and its output collision; make the named script produce the published evidence, with an independent sector-aware matrix check rather than an asserted helper |
| CA-H005, CA-H006 | Correct the Heisenberg conjugation, centralizer-versus-Abelian-subgroup explanation, encode-before-taper pipeline, and physical-sector guidance |
| CA-H007 | Regenerate and label the committed H₂O calculation as a fixed-\(r_{\mathrm{OH}}\) angular scan; align the lab, CSVs, plot, chapter, and metadata with the actual executable backend |
| CA-H008 | Correct the QPE phase/energy relation, \(t_0\), shift/range, aliasing, overlap, confidence, and ancilla arithmetic; replace energy-from-time-evolution verification with unitary/product-formula verification |
| CA-H009 | Separate compact representation from state preparation, energy estimation, and possible algorithmic advantage |
| ordering-convention | State displayed-label, occupation-integer, Kronecker-matrix, and external-label orders; add state-resolved tests |
| CA-M001, CA-M002 | Correct the correlation-energy decomposition; replace heuristic Trotter guarantees with a stated bound and a generated H₂ commutator/error check |
| CA-M004, CA-M005, CA-M011 | Correct the logarithm/asymptotic language and internal contradictions; state the mathematically valid QWC partition while awaiting pinned package output |
| CA-M006, CA-M008, CA-M009 | Repair greenhouse/spectroscopy causality, appendix publication parity, and Pauli basis/group terminology |
| CA-M007 | Remove unsupported or time-sensitive numbers that cannot be derived locally; add primary sources only where they directly support the retained claim |
| CA-L001–CA-L004 | Apply the four local terminology, arithmetic, model-scope, and inventory corrections |

Source-independent implementation is not permission to claim source parity. In
particular, regenerated H₂ numbers required exact-source FockMap parity before
CA-H002/003 closure. Exact runtime/package parity is accepted at
`3e62bd71b113d78a6f5933858336e06af1e3d9c7`; public FockMap `0.9.0` from
source commit `96320a5` reproduces that accepted behavior.

## B — Cross-repository parity and evidence gates

These checks must remain explicitly open even after the source-independent prose,
data, code, or publishing repair lands:

| IDs | Required external evidence before closure |
|---|---|
| CA-H002, CA-H003 | `encodings`: Hamiltonian index order, \(1/2\) convention, JW signs, declared signature order, direct-matrix parity, and six-encoding spectrum parity |
| CA-H004 | `encodings`: skeleton/direct construction parity and any public matrix/eigensolver semantics used by the repaired capstone |
| CA-H005, CA-H006 | `encodings`: commuting-generator selection, Pauli phases, target synthesis, generator-to-physical-sector mapping, and tapered-sector spectrum parity |
| CA-H007 | `encodings`: required only if the book retains an end-to-end FockMap energy claim; otherwise FockMap's role must be limited to verified Hamiltonian/circuit construction |
| CA-H008 | `encodings`: QPE resource and export API existence/semantics plus imported-unitary parity |
| CA-M003, CA-M007 | `encodings-research` and generated artifacts: molecule-specific term/CNOT tables and defensible fault-tolerant resource estimates |
| CA-M004, CA-M010 | `encodings-research`: exact ternary/Vlasov bounds, implementation correspondence, and the star-tree statement/proof/literature relationship |
| CA-M005 | `encodings`: actual strings, weights, term counts, and CAR/spectrum checks for all six pinned implementations |
| CA-M011 | `encodings`: whether grouping is QWC or general commuting and the actual corrected-H₂ group output |
| ordering-convention | Both source repairs: one agreed signature, occupation integer, matrix row, eigenvector, and external-label example |

**2026-07-23 checkpoint:** `encodings` PR #6 at exact commit
`440540b54f093d7a9f259da7bf18b7df8da74270` is the audited base. Its source test
suite passes 887/887 tests and its 19-document strict harness passes; it repairs
Hamiltonian assembly, zero pruning, tapering phases, sector union, ordering,
zero-mode handling, tree scope, Hermitian preconditions, documentation, and the
independent fixture lock.

**Later owner decision — package contract reopened:** the owner selected a
breaking raw-integral contract after that verification. PR #6 returned to draft
and is no longer the final API contract. The new stack is expected to make the
existing Hamiltonian APIs consume raw integrals, preserve explicitly named
weighted migration APIs, version/document the break, and pass an independent
audit.

**Implementation semantics accepted:** commit
`8e562175e809dc282b5bf2ca21f1cb311e14d5fa` atop `440540b` implements that
contract at version `0.9.0`. It passes 897/897 tests and the strict 19-document
harness. The book now uses the raw primary builders and raw
`applyCoefficients`, pins `FockMap 0.9.0`, and passes its full runtime baseline
against the exact DLL.

**Runtime/package accepted; release-workflow successor pending:** commit
`3e62bd71b113d78a6f5933858336e06af1e3d9c7` removes the unreleased CFF date,
reconciles the register to 25 Hamiltonian test methods / 42 cases and a
900-test suite (897 base + 3 new cases), and adds Optimization raw two-body
exact-map, `weightedToRawFactory` migration-parity, and direct-weighted misuse
negative-control tests. No `src/**/*.fs` file changed. The exact successor
passes 900/900 tests, harness selftest, 19 executable docs, strict fsdocs, and
the complete frozen-book baseline. The designated audit accepts all raw API
code/tests. A final successor must add explicit current/staged-`0.9.0` release
mode, robust absent-CFF-date insertion, portable shell logic without `grep -P`,
and release-workflow tests. CA-H008 export/QPE-resource parity and CA-M011
grouping-output parity remain partial.

**Release-workflow successor designated-audit accepted:** commit
`9e1afe267efefc4056f726ea98dadfed8af0214e` is a release-tooling-only child of
`3e62bd71`; no runtime F#, F# tests, or cookbook content changed. It adds
explicit current/staged `0.9.0` mode, robust CFF date add-or-replace, portable
version extraction without executable `grep -P`, and a 39-assertion temp-copy
release harness. `release.sh --dry-run current` selects `v0.9.0` without
mutation; workflow YAML/options, shell syntax, ShellCheck error severity,
900 package tests, documentation harness, and the complete frozen-book runtime
baseline pass. The designated release-workflow auditor accepted this SHA.

**Designated workflow audit accepted `9e1afe2`; portability follow-up pending:**
follow-up `660c4839223037bc69909bb8fcb7d3be0f226b20` replaces the dispatch
workflow's remaining `sed -i` with the tested `rl_set_fsproj_version` helper.
Its four-file tooling-only diff has no runtime/cookbook change. Book-side
verification passes 42 release assertions, staged/current dry run, portable
command scans, workflow parsing, 900 package tests, and the full exact-DLL book
baseline. The designated one-delta audit of `660c483` remains an external
package gate and does not reopen book code.

**Public release closure — 2026-07-24:** PR #7 and the hosted release-workflow
follow-up merged. Tag/release `v0.9.0` points to main commit
`96320a56786393269fd681c67c66df88058a8b8f`. NuGet publishes `FockMap 0.9.0`;
its nuspec records the same source commit and its `Encodings.dll` SHA-256 is
`0ba8ae967ea65d4945a336c1217f8939e41feb63b6f04af617b66537717d25c3`.
The book's normal pinned NuGet path passes the full runtime and output baseline.

If an external report does not support a claimed capability or number, the default
repair is to remove the unsupported claim or narrow it to a tested local
illustration. Softened wording alone is not sufficient where the book promises
executable evidence.

### Source acceptance gates

1. **Hamiltonian/FCIDUMP:** accept the breaking raw-integral contract for the
   existing builders, preserve explicitly named weighted migration APIs,
   document/test FCIDUMP conversion, and retain cancellation-aware zero pruning
   plus the exact coefficient/dense/spectrum oracles.
2. **Tapering/sector:** pass Clifford phase, all-sector union, explicit physical
   sector, zero-mode, and qualified encoding/method/sector count tests.
3. **Ordering/state:** agree on q0-leftmost signatures, occupation-integer
   matrix rows, Qiskit-style reversal, number operators, and labelled H₂ states.
4. **API/tree/docs:** compile pinned snippets, document Hermitian Trotter
   preconditions, separate rooted-star generic index sets from supported
   path-based trees, and remove stale counts/claims.

Book companions must continue to enforce the raw/weighted boundary, oracle,
sector, and zero-filter gates. Under the final contract, direct raw-tensor use
through the existing builders must pass; preweighted callers must use the named
weighted migration APIs, and accidental double adaptation must fail.
All four runtime/package implementation gates pass designated audit at
`3e62bd71`; release workflow passes designated audit at `9e1afe2`.
The final release lineage is public as FockMap `0.9.0`, and public-package
execution passes.

---

# Priority 0 — Freeze trust-critical derived claims

Until the remaining CA-H008 API gate closes:

1. The direct fixture, JW coefficient table, and sector spectrum are canonical;
   do not treat live package, tapered, Trotter, or QPE outputs as final.
2. Do not publish the H₂/H₂O capstones as end-to-end FockMap energy calculations.
3. Do not publish the Clifford-tapering example or automatic-sector advice as exact.
4. Do not reuse the current molecule-level CNOT/QPE/FeMo tables in promotional material.

---

# Priority 1 — High-severity corrections

## CA-H001 — Rebuild the canonical H₂ integral dataset

**Status:** Closed (correctness)
**Files:** `manuscript/02-notation.md`, `manuscript/03-spin-orbitals.md`, `manuscript/18-complete-pipeline.md`; later implementation pass must also align `code/ch03-spin-orbitals.fsx`, `labs/02-h2-molecule.fsx`, `labs/07-trotter-cost.fsx`, `labs/10-vlasov-tree.fsx`.
**Fix direction:**

1. Choose 0.74 Å as the canonical geometry, or change every geometry label together.
2. Generate one-body/spatial ERI/spin-orbital physicist tensors from `code/ch18-generate-h2-integrals.py`.
3. Remove the impossible independent value `[01|01]=0.697578...`; enforce `[01|01]=[01|10]`.
4. Generate snippets/tables from the canonical artifact rather than hand-copying keys.

**Dependencies:** None for manuscript correction; CA-H002/003/004 depend on this.
**Acceptance criteria:**

- The values match `code/h2_dissociation_integrals.json:174-214` at the stated geometry.
- Automated checks enforce real-orbital permutation symmetries and spin delta rules.
- HF energy from the table is `-1.1167593074` Ha including \(V_{nn}\).
- No chapter or lab retains the spurious `0.6975782469` ERI.

**Implementation evidence:** `code/ch09-verify-h2.py` independently regenerates
the tensor with PySCF, constructs the direct fermionic matrix, derives the
leftmost-q0 15-term JW table, and checks every particle-number sector. The
current run matches committed integrals to `2.22e-15`, Pauli coefficients to
`4.88e-11`, HF total `-1.1167593074` Ha, and FCI total
`-1.1372838345` Ha. `make verify-data` also regenerates every canonical
CSV/JSON/PNG in a disposable copy, requires byte identity, and checks that
code/labs/manuscript snippets share the generated H₂ loader. Final FockMap
parity remains under CA-H002/003.

**Canonical upstream fixture:** `code/physicist_spin_integrals.json` is
byte-identical to `johnazariah/encodings-research` commit
`1e000bbc9664b8e5cfef48608d07364279c0a54f`, path
`papers/results/h2_sto3g/physicist_spin_integrals.json`, git blob
`e0477e70c0dfd35b865000bb23b7b31882b062d3`, and file SHA-256
`6539afb30a1c03ec89202a2960a06c6580a91afaebf13a6cadbcfd32c2d71812`.
Its exact 4+32 semantic map SHA-256 is
`d5d8e2f7c83b1d1322f10217198d125b7c7cae1f99fa09a6e1e232ee38dd098c`.
The independent local PySCF map hash `a9a85179...` is retained separately and
differs only in final bits (maximum absolute difference `2.22e-16`).
Run `python3 code/ch09-verify-h2.py` to regenerate
`code/h2_0.74_oracle.json`; Labs 03/10 consume its coefficient-independent
direct spectrum rather than duplicating literals.

**Conversion contract and artifact comparison accepted:** the vendored fixture
provides the 32-entry single-bar, non-antisymmetrized physicist spin tensor
generated from canonical chemist ERIs, with interleaved spin order, no embedded
prefactor, \(V_{nn}\) excluded, threshold metadata, and the required
\(\tfrac12\langle pq\mid rs\rangle a_p^\dagger a_q^\dagger a_s a_r\)
contract. The book and PR #6 package fixture consume the same bytes.

## CA-H002 — Re-derive and regenerate the 15-term H₂ JW Hamiltonian

**Status:** Closed; public FockMap `0.9.0` parity verified
**Files:** `manuscript/04-qubits-gates-circuits.md`, `manuscript/06-building-hamiltonian.md`, `manuscript/07-six-encodings.md`, `manuscript/15-trotter-formulas.md`; companion `code/ch06-building-hamiltonian.fsx`.
**Fix direction:**

1. Replace \(a_0^\dagger a_2^\dagger a_0a_2\) as the off-diagonal example with the opposite-spin double excitation plus adjoint.
2. Show the complete Pauli expansion and phase accounting.
3. Regenerate all coefficients from CA-H001.
4. Use electronic identity coefficient `-0.8121706072` Ha; use four-body magnitude `0.0453026155` Ha.

**Dependencies:** None for Hamiltonian/package correctness.
**Acceptance criteria:**

- The displayed Pauli matrix equals a direct fermionic matrix to numerical tolerance.
- The four-body signs agree with the declared \(P_0P_1P_2P_3\) (qubit 0 leftmost) and JW conventions.
- Full-spectrum and number-commutator tests pass.
- Every downstream H₂ table is generated from the same Pauli artifact.
- Existing FockMap Hamiltonian builders consume the declared raw single-bar
  physicist tensor and internally produce
  \(\tfrac12\langle pq\mid rs\rangle a_p^\dagger a_q^\dagger a_s a_r\).
- Explicitly named weighted migration APIs apply the caller's full operator
  coefficient verbatim, and FCIDUMP conversion is documented/tested against
  both raw and weighted boundaries without double adaptation.

**Resolved physics root cause and API integration:** the original
book passed raw integrals to a weighted operator-coefficient factory. PR #6
proved one nonbreaking repair with named raw APIs and a named raw-to-weighted
adapter. The owner subsequently chose the breaking alternative: existing
Hamiltonian APIs will consume raw integrals directly and explicitly named
weighted APIs support migration. `8e562175` implements that contract; successor
`3e62bd71` locks the exposed Optimization routing and release metadata. The book
uses direct raw ingestion throughout; no obsolete raw aliases or adapter calls
remain.

**Zero-pruning accepted on the final stack:** cancellation-aware
assembly and returns exactly 15 stored/nonzero H₂ terms while preserving
legitimate standalone tiny coefficients. Book companions retain a defensive
`1e-12` reporting filter.

**Reconciliation gate passed:** the corrected source and book reproduce:

- legacy stored entries: `23`; corrected stored entries: `15`;
- corrected nonzero Pauli terms at `1e-12`: `15`;
- coefficient 1-norm
  \(\lambda_{\mathrm{coeff}}=\sum_k|c_k|=2.6992778241\) Ha
  (and non-identity measurement norm `1.8871072169` Ha);
- first-order commutator sum
  \(\Lambda_{\mathrm{comm}}=\sum_{j<k}\|[c_jP_j,c_kP_k]\|
  =0.2861997180\) Ha\(^2\).

No source, table, or API may use bare “lambda” without the definition, included
terms, and units.

## CA-H003 — Replace the complete H₂ sector spectrum and energies

**Status:** Closed; direct and public-package parity verified
**Files:** `manuscript/09-verification.md`, `manuscript/18-complete-pipeline.md`, `manuscript/21-circuit-export.md`, README/back-cover numerical claims if affected.
**Fix direction:** Recompute all 16 eigenvalues and label \(N_e\), \(M_S\), electronic/total energies. Replace HF, FCI, MP2, and correlation comparisons from one geometry-consistent source.

**Dependencies:** CA-H001, CA-H002.
**Acceptance criteria:**

- At 0.74 Å: \(E_\mathrm{HF}=-1.1167593074\) Ha total, \(E_\mathrm{FCI}=-1.1372838345\) Ha total, \(E_\mathrm{FCI}^{el}=-1.8523881736\) Ha.
- Correlation energy is approximately `-12.88 kcal/mol`.
- The sector table is the spectrum of the displayed Hamiltonian.
- Cross-encoding equality is checked against the correct reference, not only among encodings.

## CA-H004 — Make the H₂ capstone execution story honest

**Status:** Closed; execution story and public-package API integration verified
**Files:** `manuscript/18-complete-pipeline.md`; companion `code/ch18-pipeline.fsx`, `code/ch18-dissociation-scan.py`, `code/ch18-generate-h2-integrals.py`.
**Fix direction:** Choose one of:

- implement sector-aware FockMap matrix diagonalisation and compare every scan point with PySCF FCI; or
- state that PySCF FCI produces the energy curve and FockMap produces Hamiltonian/circuit structure and costs.

Remove the identity-coefficient “ground-state proxy” and the incomplete integral map.

**Dependencies:** CA-H001–003 accepted. No FockMap energy claim is retained.
**Acceptance criteria:**

- The shown script is the script that produces the published table/figure.
- No global-phase coefficient is called a ground-state energy.
- Electron number/spin sector is explicit.
- The generated FockMap and PySCF energies agree at all scan points if end-to-end parity is claimed.

## CA-H005 — Correct general Clifford tapering and the Heisenberg example

**Status:** Closed; successor runtime regression accepted at `3e62bd71`
**Files:** `manuscript/12-clifford-tapering.md`, `manuscript/13-tapering-benchmarks.md`; companion `code/ch12-clifford-tapering.fsx`.
**Fix direction:**

1. Distinguish the Pauli centralizer from a mutually commuting independent stabilizer subgroup.
2. Specify phase tracking and generator-target selection.
3. Correct \(YY\to-XZ\) under the chosen CNOT.
4. Show \(H'=X-XZ+Z\), then both sectors and the original \(\{-3,1,1,1\}\) spectrum.

**Dependencies:** Source acceptance complete; released package pin remains operational.
**Acceptance criteria:**

- Every selected generator commutes with the Hamiltonian and all other selected generators.
- Clifford conjugation preserves coefficient phases and the full spectrum across sectors.
- The worked arithmetic is reproducible symbolically and by \(4\times4\) matrices.
- The manuscript does not claim more removable qubits than the verified stabilizer rank.
- Post-fix audit confirms the new CNOT phase rule against all Pauli and dense-matrix tests.

## CA-H006 — Repair optimization order and physical sector selection

**Status:** Closed; successor runtime regression accepted at `3e62bd71`
**Files:** `manuscript/10-why-tapering.md`, `manuscript/11-diagonal-z2.md`, `manuscript/13-tapering-benchmarks.md`, `manuscript/17-cost-analysis.md`, `manuscript/18-complete-pipeline.md`.
**Fix direction:** Use `encode → identify symmetries → map known physical quantum numbers to eigenvalues → taper → compile` for each encoding. Remove “taper then choose encoding,” automatic `+1` production advice, and global-minimum-as-molecular-sector advice.

**Dependencies:** Source acceptance complete; released package pin remains operational.
**Acceptance criteria:**

- H₂ examples explicitly choose the two-electron singlet sector.
- H₂O examples explicitly choose \(N=10\) and intended spin/point-group sector.
- A test shows each tapered spectrum equals the corresponding untapered sector.
- No text says a wrong sector can never be lower.
- No bare tapering count appears: encoding, method, generators, sector, and
  sector-spectrum comparison accompany every reduction.
- The non-ground default `(+1,+1,+1)` sector is never used as a molecular result;
  verified JW ground sector `(-1,-1,+1)` is used only after the public
  Hamiltonian builder passes the direct oracle.

**Verified taper configuration (`encodings` PR #5 `b2f9bc3`):** JW
DiagonalOnly 4→4; JW FullClifford 4→1 with ground sector `--+`; BK
DiagonalOnly 4→2 removing `[1,3]` in `++`; BK FullClifford 4→1 with physical
default sector not established. Keep executable book paths untapered until the
Hamiltonian-builder follow-up passes the direct oracle.

## CA-H007 — Reframe or implement the H₂O headline result

**Status:** Closed; no FockMap energy claim retained
**Files:** `manuscript/19-bond-angle.md`, README/back-cover copy; companion `code/ch19-bond-angle-scan.py`, `labs/08-h2o-workshop.fsx`.
**Fix direction:**

1. Label the current dataset “PySCF FCI angular scan at fixed experimental \(r_\mathrm{OH}=0.9584\) Å.”
2. Do not say the current script emits FockMap JSON or diagonally solves a FockMap Hamiltonian.
3. Align the lab’s HF scan with the manuscript’s FCI claim, or label them as different exercises.
4. If “equilibrium geometry without empirical input” is retained, perform a two-dimensional/internal-coordinate optimization and a sector-aware FockMap parity run.

**Dependencies:** `johnazariah/encodings` only if end-to-end parity remains.
**Acceptance criteria:**

- The exact committed command/script produces the displayed CSV and plot.
- Method, basis, fixed coordinates, electron/spin sector, and energy backend are stated.
- No empirical-input claim remains while experimental bond length is fixed.
- FockMap’s role is neither enlarged nor implied beyond what runs locally.

## CA-H008 — Correct QPE resources and circuit verification

**Status:** Partial — local theory/arithmetic closed; imported-unitary and
QPE-resource API parity remain open
**Files:** `manuscript/18-complete-pipeline.md`, `manuscript/20-algorithms.md`, `manuscript/21-circuit-export.md`.
**Fix direction:**

1. Introduce \(t_0\), phase wrapping, energy shift/range, state overlap, confidence, and controlled-simulation error.
2. Use \(m\ge\lceil\log_2(2\pi/(\epsilon t_0))\rceil\), subject to the stated range convention.
3. Remove the inconsistent 10-bit/12-bit arithmetic.
4. Verify exported circuits against the intended product-formula unitary, not the FCI energy.

**Dependencies:** `johnazariah/encodings` QPE/export report; CA-M002.
**Acceptance criteria:**

- A worked H₂ phase maps back to the correct energy without aliasing.
- Ancilla and controlled-evolution counts are generated from a stated algorithm.
- Energy validation includes explicit state preparation and VQE or QPE.
- Unit tests compare QASM/Q#/JSON imported unitaries with the internal gate list.

## CA-H009 — Qualify quantum-advantage claims

**Status:** Closed
**Files:** `manuscript/01-electronic-structure.md`, `manuscript/09-verification.md`, `manuscript/22-scaling.md`, `manuscript/23-whats-next.md`, README/back-cover copy.
**Fix direction:** Separate compact quantum state representation from efficient ground-state preparation/estimation. Add assumptions on Hamiltonian access, overlap, structure, precision, and fault tolerance.

**Dependencies:** None.
**Acceptance criteria:**

- No blanket claim says a quantum computer extracts arbitrary molecular ground states without exponential cost.
- “Only a quantum computer can” is replaced by a qualified potential-advantage statement.
- QPE success probability includes trial-state overlap.

## ordering-convention — Make every basis/order boundary explicit

**Status:** Closed; successor documentation/regression accepted at `3e62bd71`
**Files:** Ch5, Ch9, Ch21, Appendix B, `code/ch09-verify-h2.py`, state-labelled labs/snippets.
**Fix direction:**

1. Display operator and occupation labels with index 0 leftmost:
   \(P_0P_1\ldots\) and \(\lvert n_0n_1\ldots\rangle\).
2. Define occupation integer \(b=\sum_j n_j2^j\); H₂ HF is integer `3`
   (`0b0011`) while its displayed ket is \(\lvert1100\rangle\).
3. Reverse displayed signatures for dense matrices:
   \(P_{n-1}\otimes\cdots\otimes P_0\), so the displayed HF ket is row 3.
4. Reverse FockMap signatures at Qiskit-style label boundaries.
5. Name the index/place-value rule directly.

**Dependencies:** Final `encodings` documentation audit/merge only; the
conceptual convention and state tests agree.
**Acceptance criteria:**

- \(a_2^\dagger=\tfrac12\,\mathrm{ZZXI}-\tfrac{i}{2}\,\mathrm{ZZYI}\)
  under the displayed FockMap convention.
- Number operators return the labelled occupations for every four-mode basis state.
- H₂ HF has displayed ket \(\lvert1100\rangle\), occupation integer and matrix
  row `3`, and the verified HF determinant energy.
- JW equals the direct occupation-basis matrix element-wise; the ground-state
  HF amplitude squared is `0.9873339`.
- BK/other encodings are not required to equal JW element-wise without the
  explicit encoding basis transform; spectrum/state checks use the declared map.
- At least one external Pauli label is round-tripped with the required reversal.
- State-resolved tests pass; spectrum equality alone is not accepted.

---

# Priority 2 — Medium-severity corrections

| ID | Status | Files | Exact fix direction | Dependencies | Acceptance criteria |
|---|---|---|---|---|---|
| CA-M001 | Closed | Ch6, Ch9 | Distinguish configuration coupling, changed diagonal populations, and interference; call the four-body terms configuration couplings | CA-H002/003 | Correlation energy is defined as the full variational energy difference, not the expectation of four terms |
| CA-M002 | Closed | Ch15, `code/ch09-verify-h2.py` | State a rigorous first-order bound; remove identity from error norms; label heuristics; generate the H₂ commutator sum | CA-H002; Childs/Suzuki sources | Every “suffices” step count follows from a shown bound or is labeled empirical |
| CA-M003 | Closed by removal — must stay removed | Ch13, Ch17, Ch22 | Keep only generated H₂/operator-level data; restore molecule tables only from one canonical artifact | None; multi-molecule artifacts remain unavailable | Any restored table states basis, active space, ordering, sector, taper, term count, and logical-connectivity assumptions |
| CA-M004 | Closed | Ch5, Ch7, Ch8, Ch10, Ch17, Ch22, Ch23, Appendix B, labs 03/06/08/10 | Use \(\Theta(\log n)\) for binary and ternary; describe ternary branching as a constant/finite-size improvement | Final encodings tree/API wording | No claim treats log bases as different Big-O classes |
| CA-M005 | Closed; successor runtime regression accepted at `3e62bd71` | Ch7, Ch17 | Resolve “may differ” versus “identical”; restrict equal-term-count claims to tested cases | encodings six-map report | Printed strings/weights from pinned package match prose |
| CA-M006 | Closed | Ch19 | Use dipole-change selection rules, the linear-CO₂ counterexample, rotational spectrum, and qualified climate role | Appropriate spectroscopy/climate citation | Text no longer implies a permanent dipole is necessary for greenhouse absorption |
| CA-M007 | Closed locally; external Zenodo gate separate | Ch4, Ch17, Ch20, Ch22, bibliography | Remove unsupported resource totals and generic hardware snapshots; cite every retained active-space/QPE/product-formula claim | primary-source audit; repaired research artifacts for any future table | Every retained trust-critical number has a source date/model or generated artifact |
| CA-M008 | Closed | `Book.txt`, MyST/release manifests, project metadata | Include Appendix B everywhere and use one Pages workflow | None | PDF, EPUB, and the MyST site contain both appendices |
| CA-M009 | Closed | Appendix B | Call \(4^n\) phase-free strings a basis; include phases in the Pauli group | None | Closure under multiplication is stated correctly |
| CA-M010 | Partial — book narrowed; research integration pending | Ch7, Ch8 | Keep rooted-star generic index-set and max-three-child path-based scopes distinct | Final encodings tree/API wording | No stale monotonicity/Fenwick-general claim appears |
| CA-M011 | Partial — QWC prose accepted; pinned API output pending | Ch20 | Keep the valid direct QWC partition; add pinned API output only after its semantics are verified | encodings grouping report; CA-H002 | One Z group plus four singleton X/Y groups is shown for corrected H₂, with all terms accounted for |

---

# Priority 3 — Low-severity corrections

| ID | Status | Files | Fix direction | Acceptance criteria |
|---|---|---|---|---|
| CA-L001 | Closed | Ch3 | Qualify \(Z^4\) as hydrogenic/rough; attribute interleaving to the conversion/FockMap convention, not PySCF generally | Claims no longer imply universal molecular scaling or native PySCF spin ordering |
| CA-L002 | Closed | Ch19, Lab 08 | Compute the ratio from committed CSV values | Lab reports `0.112928/0.125873 = 89.7%` and prose says about 90% |
| CA-L003 | Closed | Ch1 | Say exact within the nonrelativistic point-charge Coulomb model | “No approximations” is not absolute |
| CA-L004 | Closed | README, Make metadata, `code/ch09-verify-h2.py` | Define and reconcile the companion-script count | Advertised count equals packaged executable scripts |

---

# Citation provenance actions

| Provenance IDs | Status | Action |
|---|---|---|
| PR-001, PR-002 | Local values and metadata implemented — verification pending | Preserve PySCF version, geometry, basis/MO convention, raw tensor data/checksum, generation command, and software citation |
| PR-003, PR-004 | Wording corrected; inline citations pending | Keep “exact” scoped to model/basis/sector; cite Local Hamiltonian hardness and QPE overlap conditions |
| PR-005 | Local bound generated; citation correction pending | Cite Childs et al. 2021; remove the misidentified PRL and every ungenerated numerical guarantee |
| PR-006 | Fixed-bond framing, lab, and metadata implemented — verification pending | Cite PySCF and experimental angle; preserve scan version, coordinates, basis, sector, grids, and output checksums |
| PR-007 | Unsupported | Remove larger-basis minima table unless a complete reproducibility artifact is committed |
| PR-008 | Corrected; inline citations pending | Cite spectroscopy/dipole/feedback sources immediately after retained quantitative/causal claims |
| PR-009 | Metadata/result correction open | Replace Jiang/Vlasov metadata; state exact Majorana weight/lower bound without changing the \(\Theta(\log n)\) class |
| PR-010, PR-011 | Cross-repository blocked | Use literature for abstract theory only; require repaired implementation and state/sector artifacts |
| PR-012 | Corrected scope; citations/API output pending | Cite Wecker and Yen; keep independent-term and grouped/covariance cases distinct |
| PR-013 | Corrected scope; citations/resource artifact pending | Cite Kitaev/Reiher; do not restore numeric resources without generated model |
| PR-014 | Too time-sensitive | Delete generic hardware numerical table; use dated named-backend data only if archived |
| PR-015, PR-017 | Removed | Do not restore unsupported molecule CNOT or generic QEC totals |
| PR-016 | Partially supported | Retain CAS(54e,54o)/108-spin-orbital fact with Reiher citation; keep resource architectures separate |
| PR-018 | Scoped wording and sources implemented — verification pending | Keep conventional DFT, canonical single-reference CCSD(T), and low-entanglement DMRG scopes explicit; no fixed atom limits or blanket quantum niche |

**Book bibliography:** Replace citation adjacency and chapter-only “Further
Reading” support with inline author-year references backed by one book
bibliography. `manuscript/references.md` is now the sole rendered source; keep
`jose/paper.bib` outside book manifests. Require unique, resolvable DOI/URLs and
no stale metadata in final verification.

**OWNER/RELEASE blocker:** Zenodo concept `10.5281/zenodo.18917465`, version
`0.9.0` / `10.5281/zenodo.20148795` still describe the combined archive as
software/MIT. The owner must choose split or accurately represented mixed
records, then update Zenodo resource type, license/rights, description, files,
and DOI/version relations. Do not mark fixed until the public landing page,
GitHub/CFF, PDF/EPUB notice, and packaged code licenses agree.

**Public execution gate:** Against public NuGet FockMap `0.9.0`, the current
tree passes
`make verify-data`, `make pipeline-check`, `make lab-check`, and every
`code/ch*.fsx` companion without obsolete-API warnings. Every NuGet reference is
pinned to `FockMap 0.9.0`. The published nuspec source commit and DLL digest
match the accepted release provenance.

**Release-tooling gate:** Against
`9e1afe267efefc4056f726ea98dadfed8af0214e`, the 39-assertion harness passes
without mutating the worktree; `--dry-run current` selects `v0.9.0`; executable
shell/workflow commands contain no `grep -P`; workflow choices parse as
`auto,current,patch,minor,major`; ShellCheck reports no errors; 900 package tests
and the frozen-book runtime baseline remain green. The Linux-only workflow
retains `sed -i` in its bumped-version branch, but the staged/current `0.9.0`
path skips that branch. This scope nuance is recorded for the designated
workflow auditor.

Against portability follow-up
`660c4839223037bc69909bb8fcb7d3be0f226b20`, the harness passes 42/42,
`current` still selects `v0.9.0`, command-only scans find neither `grep -P` nor
`sed -i`, workflow YAML/options parse, 900 package tests and strict semantic
documentation gates pass, and the frozen book's data/pipeline/labs/companions
remain green. Designated one-delta audit remains.

---

# Required verification sequence after fixes

1. **Data parity:** regenerate 0.74 Å integrals and H₂O CSVs from clean environments.
2. **Fermion matrix parity:** build direct Fock-space matrices and compare with each encoded Pauli matrix.
3. **Sector parity:** label all eigenvectors by \(N\), \(M_S\), and selected Z₂ eigenvalues.
4. **Taper parity:** compare each tapered spectrum with exactly one untapered sector.
5. **Product-formula parity:** compare internal gate-list unitaries with direct products of Pauli exponentials.
6. **Export parity:** import QASM/Q#/JSON and compare unitaries.
7. **Benchmark regeneration:** render all term/weight/CNOT tables from committed machine-readable output.
8. **Citation check:** resolve every quantitative source and remove placeholder identifiers.
9. **Whole-book search:** confirm obsolete H₂ values (`-1.1422`, `-1.8573`, `0.1744`, `0.6975782469`) no longer survive except in explicitly historical material.
10. **Norm/count reconciliation:** confirm `23 stored`, `15 nonzero`,
    \(\lambda_{\mathrm{coeff}}\) in Ha, \(\lambda_{\mathrm{meas}}\) in Ha, and
    \(\Lambda_{\mathrm{comm}}\) in Ha\(^2\) are never conflated.

---

# Cross-repo deliverables required

## From `johnazariah/encodings`

**Audited base accepted 2026-07-23 at PR #6 commit
`440540b54f093d7a9f259da7bf18b7df8da74270`; breaking runtime/package successor
`3e62bd71b113d78a6f5933858336e06af1e3d9c7` accepted by designated audit;
public release verified 2026-07-24.**
The exact base passes 887/887 tests and the strict 19-document executable
harness. Its implementation repairs remain required regressions. The owner then
selected a breaking raw-integral API, returned PR #6 to draft, and delegated a
new stack. `8e562175` supplies the versioned migration contract; successor
`3e62bd71` addresses all runtime/package metadata/test blockers and passes
900/900 tests, strict documentation gates, and exact book integration. The
release-engineering lineage was completed and published as FockMap `0.9.0` at
source commit `96320a5`; public book execution passes.
CA-H008 and CA-M011 retain their narrower API-output gates.

- Six-encoding CAR/matrix/sign and ordering report.
- Hamiltonian convention/factor report.
- Correct H₂ coefficient artifact.
- Clifford-tapering generator, phase, and physical-sector report.
- Direct-vs-skeleton parity report.
- Trotter/Rz/CNOT report.
- Measurement grouping and shot-estimator report.
- QPE resource and exporter/API report.
- Custom index-set/tree validation report.

## From `johnazariah/encodings-research`

**Post-fix repair verified 2026-07-22; canonical fixture integrated into this
book, broader integration/merge pending.** Canonical
H₂ evidence, \(n=64\) weight/CNOT results, BK formulas, Appendix B, star census,
state ordering, generated results, and `run_all.sh` re-verify. Multi-molecule
LiH/H₂O/NH₃ claims remain explicitly unreproduced and stay removed/open.

- Star-tree theorem statement/proof/literature comparison.
- Balanced ternary/Vlasov exact weight and optimality report.
- Evidence that the implementation corresponds to Vlasov’s construction.
- Reproducible H₂O/LiH/N₂/FeMo-co benchmark report.
- Defensible fault-tolerant FeMo/QPE resource report.

Only the explicitly partial source-dependent items remain pending. Closed items
are accepted at immutable source commits even while merge/release operations
remain outstanding.

---

# Whole-book tone and clarity pass — 2026-07-23

A cross-book read of the foreword, all 23 chapters, both appendices,
references, and the README/back-cover reader contract confirms that the
manuscript reads as one coherent book. It has a stable chapter scaffold, a
consistent wide-to-narrow-to-code-to-wide rhythm, consistent technical
terminology, and an honest teaching contract: code is evidence, the small
examples are intentionally small, and sector/API/operational limits are stated
rather than sold as completed capabilities.

Substantive editorial changes:

- Two stale Chapter 1 forward references now point to Chapter 5, where
  creation/annihilation operators and the fermion-sign subtlety are developed.
- Chapter 18 navigation labels now match its actual title, *The Question We Can
  Now Answer*.
- Chapter 22 navigation labels now match the em-dash punctuation of its H1,
  *Scaling — From H₂ to FeMo-co*.
- A final selective cadence pass removed four repeated bridge templates without
  changing technical meaning: “Trotterization enters,” “quantum algorithms
  enter,” a duplicate “take stock” opener, and a redundant “In other words”
  before the Chapter 23 climax.

No churn-only rewrite was made. The author's direct, conversational voice,
deliberate recap points, equations, citations, code fences, generated values,
and settled correctness content were preserved.

The package reader contract changed after the first pass. PR #6/`440540b` is the
audited base; `8e562175` implements the breaking contract; designated audit
accepts runtime/package successor `3e62bd71`. The book uses the raw primary APIs
and pins FockMap `0.9.0`; all book checks pass. The canonical direct
fixture/oracle remains valid. FockMap `0.9.0` is now public and provenance
verified. Zenodo operations remain.

---

# Historical record — May 2026 plan (preserved)

The following is retained as project history. It records what the earlier plan claimed; it is not current verification.

## 2026-05-11 — Claimed fixes

- **Commit `7003212`:** “All systemic issues (S1–S4), all HIGH-severity findings, and all MEDIUM-severity findings have been addressed. Changes span 22 manuscript files.”
- **Commit `425d054`:** Ch19→Ch20 transition rewritten, Ch20 VQE walkthrough added; Ch1 bridge, Ch16 ZZ example, and Ch23 QEC climax revised.
- **Commit `d42acf0`:** All LOW findings claimed fixed; bond length changed to 0.74 Å; Lab 02 integral table regenerated; Ch5 code snippet added; Ch6 exchange derivation changed; Ch10 table regenerated; Ch12 Heisenberg Step 4 completed; Q# updated; citations added.
- The historical “Remaining” line was: “Further Reading normalization (in progress). Then: MyST/Jupyter Book setup.”

## Historical severity claim

| Severity | Claimed count |
|---|---:|
| HIGH | 13 |
| MEDIUM-HIGH | 1 |
| MEDIUM | 31 |
| LOW | 25 |
| **Total** | **70** |

The old file did not enumerate a reconciled set of 70 IDs. Its systemic-ID lists and individual tables are preserved below so no historical item disappears.

## Historical systemic issues

### S1 — H₂O qubit count: 12 vs 14

Historical diagnosis: 14 spin-orbitals for full STO-3G versus 12 after freezing O 1s were mixed.
Historical resolution: label each context and never call 14 qubits frozen-core.
Historical IDs: `p4-f2`, `p5-f3`, `p7-f3`, `p1-f5`.
**Current re-audit:** Still closed.

### S2 — CNOT tables disagree

Historical diagnosis: Ch17/Ch22 used incompatible tapered/untapered bases, active spaces, and possibly orders.
Historical resolution: one canonical source-of-truth table with assumptions.
Historical IDs: `p5-f1`, `p5-f4`, `p5-f5`, `p5-f6`, `p7-f1`, `p7-f5`, `p7-f6`.
**Current re-audit:** Unresolved; CA-M003.

### S3 — Correlation overclaim

Historical diagnosis: exponential scaling was overgeneralized and water’s bend/error was wrongly attributed to correlation.
Historical resolution: restrict exponential claim to exact methods; credit mean-field bending and basis error.
Historical IDs: `p1-f1`, `p1-f6`, `p6-f1`, `p6-f3`, `h2o-correlation-fix`.
**Current re-audit:** The local correlation narrative remains repaired; broader quantum-efficiency overclaim is CA-H009.

### S4 — \(V_{nn}\) confusion

Historical diagnosis: Ch6 said add \(V_{nn}\) to `IIII`, while the vacuum spectrum showed it absent.
Historical resolution: keep the 15-term Hamiltonian electronic and add \(V_{nn}\) after diagonalisation.
Historical IDs: `p3-f2`, `p3-f9`.
**Current re-audit:** Still closed.

## Historical individually named findings

### High

| Historical ID | Historical title/fix | Current re-audit |
|---|---|---|
| `p1-f2` | Complex-orbital ERI symmetry formula | Still closed |
| `p2-f1` | Tensor-product arithmetic `4×4×2=8` | Still closed |
| `p3-f1` | HF energy inconsistent with book integrals | Superseded by CA-H001/003 |
| `p4-f1` | Ch13 CNOT arithmetic 14→12 | Still closed |
| `p6-f2` | Swapped/wrong energy labels and values | Regression; CA-H003 |
| `p7-f2` | Success-probability 500× vs 10× contradiction | Contexts now distinguished, though estimates remain under CA-M003/M007 |

### Medium

| Historical ID | Historical title | Current re-audit |
|---|---|---|
| `p3-f3` | Identical Pauli strings across encodings | Unresolved; CA-M005 |
| `p2-f2` | Wrong density-matrix cross-reference | Still closed |
| `p2-f3` | T gate called \(\pi/8\) rotation | Still closed |
| `p2-f4` | 16 amplitudes “matching” H₂ space | Still closed |
| `p2-f5` | Pauli basis scope | Still closed |
| `p1-f3` | \(V_{nn}\)/bond-length mismatch | Superseded by CA-H001 |
| `p1-f4` | Exchange/pair-scattering label | Still closed locally |
| `p3-f4` | Wrong orbitals in fermion-sign example | Still closed |
| `p3-f5` | Exchange derivation swaps pairings | Regression; CA-H002 |
| `p3-f6` | Number-operator weight overgeneralized | Still closed |
| `p3-f7` | Qubit ordering absent | Still closed |
| `p4-f3` | CNOT Z conjugation rules swapped | Basic rules closed; worked example fails under CA-H005 |
| `p4-f4` | H₂ Pauli column table garbled | Still closed |
| `p5-f2` | Qubitization omitted \(\alpha t\) | Still closed |
| `p5-f7` | Second-order 1-norm bound | Unresolved; CA-M002 |
| `p5-f8` | H₂ costs versus identical-string claim | Unresolved; CA-M005 |
| `p6-f4` | Unexplained Trotter factor \(1/2\) | Still closed |
| `p6-f5` | QPE ancilla formula missing \(2\pi/t\) | Partially addressed; CA-H008 |
| `p6-f6` | Q# allocation prose mismatch | Still closed |
| `p7-f4` | Ch23 certainty language | Still closed |
| `p7-f7` | JW \(X\mp iY\) convention unstated | Still closed |
| `p7-f8` | Cookbook chapter-number mismatch | Still closed |
| `p7-f9` | FeMo active-space citation | Active-space citation added; resource claims remain CA-M003/M007 |

### Low

| Historical ID | Historical title | Current re-audit |
|---|---|---|
| `p2-f6` | Z measurement claim lacked basis qualifier | Still closed |
| `p2-f7` | Commuting tensor-product oversimplification | Still closed |
| `p2-f8` | Hardware table inconsistency | Unresolved; CA-M007 |
| `p2-f9` | Bad Pauli-space cross-reference | Still closed |
| `p2-f10` | Prerequisites understated | No current correctness action |
| `p1-f7` | Physicist-symmetry count note | Still closed |
| `p1-f8` | Lab 02 table inconsistent with Ch3 | Tables now agree but share CA-H001 |
| `p3-f8` | Vlasov children off by one | Still closed |
| `p4-f5` | Pauli strings square to \(\pm I\) | Still closed |
| `p4-f6` | Null-space complexity | Prose changed; algorithm remains cross-repo pending under CA-H005 |
| `p4-f7` | Unverifiable 5× taper saving | Unresolved; CA-M003 |
| `p4-f8` | Heisenberg example unfinished | Completed incorrectly; CA-H005 |
| `p4-f9` | “Taper first, then encode” | Unresolved; CA-H006 |
| `p5-f9` | Ch14 cross-reference | Still closed |
| `p5-f10` | Nonstandard Trotter notation | Still closed |
| `p5-f11` | Uncited time-step heuristic | Unresolved; CA-M002 |
| `p5-f13` | Unspecified \(n=32\) cost table | Unresolved; CA-M003 |
| `p6-f7` | Deprecated Q# namespaces | Still closed |
| `p6-f8` | QPE “one readout” | Still closed |
| `p6-f9` | \(R_z\) convention | Still closed |
| `p7-f10` | Physical-qubit overhead uncited | Partially addressed; CA-M007 |
| `p7-f11` | “Zero runtime dependencies” | Reframed as no third-party dependencies |
| `p7-f12` | Cambridge Quantum/tket branding | Still closed |
| `p7-f13` | CCSD(T) 50-atom limit | Still closed |
| `p7-f14` | QEC-theory overstatement | Still present in softened form; address with CA-H005 and a later scope edit |
