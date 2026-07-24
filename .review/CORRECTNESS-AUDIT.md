# CORRECTNESS AUDIT — From Molecules to Quantum Circuits

**Audit date:** 2026-07-22
**Repository:** `johnazariah/molecules-to-circuits`
**Branch:** `johnazariah-encodings-book-review`
**HEAD:** `2269f0d3eb87f0d22e1b275e56add2077ad6915d`
**Mode:** Review only; no manuscript, code, lab, build, or generated-data files were changed.

## Current finding count

| Severity | Count |
|---|---:|
| High | 10 |
| Medium | 11 |
| Low | 4 |
| **Total actionable** | **25** |

“Regression” below means that the May 2026 plan claimed the issue was fixed but the defect is present at current HEAD. “Newly identified” means absent from that plan; the available file-inspection tooling does not establish the exact commit in which it entered.

---

# Findings

## High

### CA-H001 — The canonical H₂ integral data are internally impossible and do not describe the stated 0.74 Å geometry

**Classification:** Newly identified systemic defect; supersedes the historical local HF-number fix.
**Targets:** `manuscript/02-notation.md:75-81`, `manuscript/02-notation.md:149-160`, `manuscript/03-spin-orbitals.md:152-178`, `manuscript/03-spin-orbitals.md:192-239`, `manuscript/18-complete-pipeline.md:35-56`.
**Local evidence:** `code/h2_dissociation_integrals.json:174-214`, `code/ch18-generate-h2-integrals.py:62-102`.

For real orbitals, multiplication of scalar orbital values gives

$$
[01\mid 01]=[01\mid 10].
$$

The manuscript itself states this within-bracket symmetry at `02-notation.md:115-119`, but then assigns these two expressions different values: `0.6975782469` and `0.1809312700` Ha. The extra `0.6975782469` value is not part of the committed PySCF tensor for 0.74 Å. The one-body numbers are also different from the committed 0.74 Å data.

The repository’s generated 0.74 Å record gives:

$$
\begin{aligned}
V_{nn}&=0.7151043390810812,\\
h_{00}=h_{11}&=-1.2533097866459775,\\
h_{22}=h_{33}&=-0.4750688487721778,\\
[00\mid00]&=0.6747559268144483,\\
[00\mid11]&=0.6637114013508137,\\
[01\mid01]=[01\mid10]&=0.18121046201519694,\\
[11\mid11]&=0.6976515044904634.
\end{aligned}
$$

The minimal machine-readable payload is `code/h2_0.74_fixture.json`: four
one-body entries plus the exact 32-entry two-body spin tensor, raw RHF
provenance, and checksum
`a9a85179ad7f9fef8ef2ad5a90f54ac14b1923f1775ff17fdb6d14a1e1557a14`.
The direct oracle command is `python3 code/ch09-verify-h2.py`.

**Why this matters:** These are the book’s source data. The wrong tensor propagates into the central 15-term Hamiltonian, spectrum, tapering examples, Trotter angles, and resource counts. Cross-encoding agreement cannot detect a bad common input.

**Minimal correction direction:** Choose one geometry and one generated integral artifact as canonical. For the stated 0.74 Å model, regenerate every spin-orbital key from `code/ch18-generate-h2-integrals.py`, replace the hand-maintained tables/snippets, and add an automated symmetry and PySCF parity check. Do not retain a fifth independent real-orbital ERI.

---

### CA-H002 — The worked “exchange” derivation is diagonal, and the published 15-term JW Hamiltonian has wrong coefficients

**Classification:** Regression of historical `p3-f5`, with a newly identified coefficient failure.
**Targets:** `manuscript/06-building-hamiltonian.md:150-192`, `manuscript/06-building-hamiltonian.md:196-216`, `manuscript/06-building-hamiltonian.md:240-279`, `manuscript/15-trotter-formulas.md:27-38`.

The operator used as the off-diagonal worked example,

$$
a_0^\dagger a_2^\dagger a_0 a_2,
$$

does not exchange occupations. It is a signed product of number operators and is diagonal in the occupation basis. It cannot produce `XXYY`, `XYYX`, `YXXY`, or `YYXX`. The off-diagonal H₂ coupling is the opposite-spin double excitation and its adjoint.

Using the committed 0.74 Å PySCF data and the authoritative FockMap signature
order \(P_0P_1P_2P_3\) (qubit 0 leftmost), the purely electronic JW
coefficients should be:

| String | Coefficient (Ha) |
|---|---:|
| `IIII` | -0.8121706072 |
| `IIIZ`, `IIZI` | -0.2234315369 each |
| `IZII`, `ZIII` | +0.1714128264 each |
| `IIZZ` | +0.1744128761 |
| `IZIZ`, `ZIZI` | +0.1206252348 each |
| `IZZI`, `ZIIZ` | +0.1659278503 each |
| `ZZII` | +0.1686889817 |
| four four-body strings | magnitude \(0.1812104620/4=0.0453026155\) each |

Under the current FockMap/JW sign convention, the four-body sign pattern must be regenerated and then frozen by a matrix parity test; it should not retain the current magnitude `0.1744`, which is the spurious `0.697578.../4`.

**Why this matters:** Chapter 6 is advertised as the by-hand derivation on which the rest of the book rests. The present algebra says a diagonal operator creates coherence and then reports a Hamiltonian for a different, invalid integral tensor.

**Minimal correction direction:** Replace the example with an actual pair-excitation term, derive it through all four ladder operators, regenerate the entire table from the corrected integral tensor, and verify the resulting \(16\times16\) matrix against a direct fermionic construction.

---

### CA-H003 — The H₂ eigenspectrum, HF/FCI labels, and correlation energy are wrong

**Classification:** Regression of historical `p6-f2`; historical `p3-f1` is superseded by the bad source dataset.
**Targets:** `manuscript/09-verification.md:43-76`, `manuscript/09-verification.md:82-104`, `manuscript/09-verification.md:125-152`, `manuscript/18-complete-pipeline.md:154`, `manuscript/21-circuit-export.md:242-253`.
**Local evidence:** `code/h2_dissociation.csv:1-8`.

At 0.74 Å the committed PySCF reference is:

$$
\begin{aligned}
E_\mathrm{HF}^\mathrm{total} &= -1.1167593074\ \mathrm{Ha},\\
E_\mathrm{FCI}^\mathrm{total} &= -1.1372838345\ \mathrm{Ha},\\
E_\mathrm{FCI}^\mathrm{el} &= -1.1372838345-0.7151043391\\
&= -1.8523881736\ \mathrm{Ha},\\
E_\mathrm{corr} &= -0.0205245271\ \mathrm{Ha}\\
&\approx -12.88\ \mathrm{kcal/mol}.
\end{aligned}
$$

The manuscript instead reports `-1.1422` total and `-1.8573` electronic. Those values contradict the committed dissociation dataset. The full sector table must also be recomputed: it is not enough to replace only the lowest two-electron value.

There is an additional internal check failure. Evaluating the displayed Chapter 6 Pauli table on the fully occupied state does not give the Chapter 9 four-electron eigenvalue `-2.2001` Ha.

**Why this matters:** The book repeatedly says numerical diagonalisation independently validates the pipeline. It currently validates neither the stated geometry nor the displayed Hamiltonian.

**Minimal correction direction:** After CA-H001/002, regenerate and publish all 16 eigenvalues with particle number and \(M_S\) labels. Use the values above for the 0.74 Å HF and two-electron FCI checkpoints, and update every downstream occurrence.

---

### CA-H004 — The H₂ “complete pipeline” does not compute the claimed energy, and its displayed input is incomplete

**Classification:** Newly identified prose/code contradiction.
**Targets:** `manuscript/18-complete-pipeline.md:25-97`, `manuscript/18-complete-pipeline.md:158-220`.
**Local evidence:** `code/ch18-pipeline.fsx:81-180`, `code/ch18-dissociation-scan.py:41-55`.

The manuscript’s standalone map at `18-complete-pipeline.md:37-56` contains only 12 two-body entries, not the 32 spin-orbital entries required by its own Chapter 3 expansion. It omits the opposite-spin terms that generate the physical pair coupling.

The prose says `exactGroundStateEnergy` performs exact diagonalisation and attributes the dissociation curve to the FockMap skeleton pipeline. The local F# companion instead sums only identity coefficients as “a proxy for the ground state” (`code/ch18-pipeline.fsx:172-180`). The plotted/CSV FCI values are generated separately by PySCF’s `fci.FCI`, not by that F# route.

**Why this matters:** This is the capstone and the strongest reproducibility claim in the book. A global phase coefficient is not a ground-state energy.

**Minimal correction direction:** Either implement a sector-aware exact eigensolver in the companion and demonstrate equality with `code/h2_dissociation.csv`, or state plainly that PySCF supplies the FCI reference while FockMap only constructs/costs circuits. Replace the incomplete map with generated data.

---

### CA-H005 — The Clifford-tapering algorithm and Heisenberg derivation are mathematically wrong

**Classification:** Regression of historical `p4-f8`; source implementation status is cross-repo pending.
**Targets:** `manuscript/12-clifford-tapering.md:66-106`, `manuscript/12-clifford-tapering.md:174-223`.

The null space of the term-commutation matrix is the Pauli centralizer, but an arbitrary basis of that null space need not be a mutually commuting stabilizer set. Tapering requires an independent Abelian subgroup plus a phase-consistent Clifford map. The text currently equates the whole null space with simultaneously taperable generators.

The worked CNOT conjugation also drops an \(X_0\):

$$
\operatorname{CNOT}_{0\to1}(Y_0Y_1)\operatorname{CNOT}_{0\to1}
=-X_0Z_1,
$$

not \(-Z_1\). Therefore

$$
XX+YY+ZZ \longmapsto X_0-X_0Z_1+Z_1.
$$

In sector \(Z_1=+1\), the result is the scalar \(+I\); in sector \(Z_1=-1\), it is \(2X_0-I\). Across the sectors the eigenvalues are \(\{-3,1,1,1\}\), matching the original model. The manuscript’s tapered \(X_0\) with eigenvalues \(\pm1\) does not.

**Why this matters:** The chapter claims symbolic exactness and uses this example as proof of end-to-end tapering correctness.

**Minimal correction direction:** Re-derive generator selection as a commuting stabilizer problem, retain Pauli phases during Clifford conjugation, replace the worked table/eigenvalues, and gate publication claims on the `johnazariah/encodings` tapering audit.

---

### CA-H006 — Tapering order and sector selection are described in a way that changes the physical problem

**Classification:** Unresolved historical `p4-f9`, extended by a newly identified sector error.
**Targets:** `manuscript/11-diagonal-z2.md:80-100`, `manuscript/11-diagonal-z2.md:146-159`, `manuscript/11-diagonal-z2.md:189-197`, `manuscript/13-tapering-benchmarks.md:102-123`, `manuscript/13-tapering-benchmarks.md:145-155`, `manuscript/17-cost-analysis.md:13-32`, `manuscript/17-cost-analysis.md:92-104`.

Two errors are conflated:

1. A JW qubit Hamiltonian cannot be tapered and then re-encoded with a different fermion-to-qubit map. Encoding occurs first. The valid comparison is “encode each fermionic Hamiltonian, then identify/taper symmetries in that representation.”
2. Sweeping sectors and taking the global minimum does **not** in general recover the molecular ground state with the required electron number and spin. A lower sector may describe an ion or a different spin state. The statement that a sector mismatch is “never lower” is false.

**Why this matters:** The stated optimization stack is not an executable transformation, and automatic `+1` or global-minimum sector choices can produce the wrong molecule.

**Minimal correction direction:** Rewrite every pipeline as `fermionic Hamiltonian → chosen encoding → symmetry discovery → map physical (N, M_S, point-group) quantum numbers to generator eigenvalues → taper → compile`. Use a sector sweep only as a diagnostic, never as a substitute for physical sector identification.

---

### CA-H007 — The H₂O PES is a PySCF reference scan at fixed experimental bond length, not the claimed FockMap end-to-end first-principles geometry calculation

**Classification:** Newly identified prose/code contradiction.
**Targets:** `manuscript/19-bond-angle.md:23-53`, `manuscript/19-bond-angle.md:59-107`, `manuscript/19-bond-angle.md:111-142`, `manuscript/19-bond-angle.md:146-203`, `manuscript/19-bond-angle.md:250-256`.
**Local evidence:** `code/ch19-bond-angle-scan.py:25-48`, `code/ch19-bond-angle-scan.py:80-100`, `labs/08-h2o-workshop.fsx:256-309`.

The committed scan calls PySCF RHF and `fci.FCI` directly. It does not export per-geometry integral JSON, call FockMap, taper a Hamiltonian, or diagonalise a FockMap matrix as the chapter says. The lab’s PES section reads precomputed **HF** energies, not the chapter’s FCI energies.

In addition, `BOND_LENGTH = 0.9584` Å is the experimental O–H distance. The calculation is a one-dimensional angular cut at fixed empirical bond length, not a full geometry optimization “without empirical input.” A true equilibrium geometry calculation must optimize the bond length and angle (or explicitly call the result a conditional angle scan).

The pseudocode also takes the smallest eigenvalue over a tapered qubit Hamiltonian without showing that the \(N=10\), singlet sector was selected. PySCF FCI does select the intended electron/spin sector; the displayed FockMap pseudocode does not establish that.

**Why this matters:** The bond-angle result is the book’s headline application.

**Minimal correction direction:** Relabel the existing result as “PySCF FCI angular scan at fixed experimental \(r_\mathrm{OH}\)” and describe FockMap as a separate encoding/cost exercise, or add a real sector-aware FockMap parity computation. Remove “no empirical input” unless both internal coordinates are optimized from an ab initio model.

---

### CA-H008 — The QPE resource formula and circuit-energy verification procedure are invalid

**Classification:** Historical `p6-f5` remains only partially corrected; the circuit verification error is newly identified.
**Targets:** `manuscript/20-algorithms.md:125-171`, `manuscript/21-circuit-export.md:242-253`, `manuscript/18-complete-pipeline.md:140-155`.

For \(U=e^{-iHt_0}\), QPE estimates phase

$$
\phi=-\frac{E t_0}{2\pi}\pmod 1.
$$

Ignoring confidence overhead, resolving energy to \(\epsilon\) requires approximately

$$
m\ge \left\lceil\log_2\frac{2\pi}{\epsilon t_0}\right\rceil,
$$

plus an energy shift/range choice that prevents phase aliasing. The text omits \(t_0\), then says \(\lceil\log_2(2\pi/\epsilon)\rceil\) gives 10 ancillas at \(\epsilon=1.6\) mHa; it gives 12, not 10.

Separately, simulating a Trotter step and measuring the resulting state’s energy is not a check that the circuit “produces” the FCI eigenvalue. Time evolution does not prepare a ground state, and exact \(e^{-iHt}\) preserves the energy expectation of the input state. QPE needs eigenstate-overlap preparation; VQE needs an ansatz and Hamiltonian measurements.

**Why this matters:** The current instructions would reject a correct time-evolution circuit or appear to validate an incorrect one.

**Minimal correction direction:** Specify \(t_0\), spectral shift/range, aliasing, state overlap, confidence, and controlled-simulation error in the QPE estimate. Verify export by comparing the circuit unitary/statevector to the intended product formula; verify energy only inside an explicit VQE/QPE workflow.

---

### CA-H009 — The book promises efficient ground-state extraction merely from compact state representation

**Classification:** Newly identified central overclaim.
**Targets:** `manuscript/01-electronic-structure.md:21-24`, `manuscript/09-verification.md:76-79`, `manuscript/22-scaling.md:23-27`, `manuscript/23-whats-next.md:123-131`.

Representing a state in \(n\) qubits avoids storing \(2^n\) amplitudes, but it does not by itself provide an efficient algorithm for preparing a molecular ground state or estimating its energy. General local-Hamiltonian ground-state estimation is hard, and QPE succeeds only in proportion to the trial state’s overlap with the target eigenstate. VQE has no general efficient-convergence guarantee.

Statements such as “extract the ground-state energy without the exponential cost” and “only a quantum computer can” omit these conditions.

**Why this matters:** This is the main motivation offered to readers and overstates what the subsequent pipeline establishes.

**Minimal correction direction:** Separate representation advantage from algorithmic advantage. State that quantum algorithms may offer favorable scaling for structured chemistry instances given efficient Hamiltonian access, adequate state preparation/overlap, and fault-tolerant resources; do not promise a generic exponential-cost removal.

---

### ordering-convention — String, occupation-integer, and matrix basis orders are not the same

**Classification:** Newly established cross-repository High blocker; the original
24 audit IDs remain unchanged.
**Targets:** `manuscript/05-visual-encodings.md`, `manuscript/09-verification.md`,
`manuscript/21-circuit-export.md`, `manuscript/appendix-theory.md`,
`code/ch09-verify-h2.py`, all state-labelled examples and exporters.

The verified conventions are:

1. Displayed FockMap signatures use character position \(i\) for qubit \(i\);
   qubit 0 is leftmost. For example,
   \(a_2^\dagger=\tfrac12\,\mathrm{ZZXI}-\tfrac{i}{2}\,\mathrm{ZZYI}\).
2. Displayed occupation kets use \(\lvert n_0n_1\ldots\rangle\), again with
   mode 0 leftmost.
3. The occupation integer uses place value \(2^j\) for mode \(j\). The H₂ HF
   occupation \(n_0=n_1=1\) is therefore integer `3`, conventionally printed
   as binary `0b0011`, even though the displayed ket is \(\lvert1100\rangle\).
4. The repaired dense matrix builders reverse the displayed signature and form
   \(P_{n-1}\otimes\cdots\otimes P_0\), so matrix rows equal occupation
   integers. \(\lvert1100\rangle\) is row `3` (`0b0011`).
5. Qiskit-style Pauli labels display qubit 0 on the right, so FockMap
   signatures require an explicit reversal at that boundary.

Spectrum agreement cannot detect a basis permutation. Energies may match while
eigenvectors, amplitudes, occupations, and state labels are wrong.

The repaired research tooling now verifies number-operator diagonals on all 16
states, \(a_2^\dagger=\tfrac12\,\mathrm{ZZXI}
-\tfrac{i}{2}\,\mathrm{ZZYI}\), H₂ HF label/integer/row `|1100>`/`3`/`3`,
JW-versus-direct element-wise error \(2.8\times10^{-16}\), and ground-state HF
amplitude squared `0.9873339`. BK can be isospectral without element-wise JW
equality because it uses a different encoded basis.

**Why this matters:** State preparation, sector selection, exported operators,
and every state-resolved worked example depend on the basis map, not only on
eigenvalues.

**Minimal correction direction:** Name index/place-value rules explicitly; add
number-operator tests on labelled basis states; check that the H₂ HF ket has
occupation integer and matrix row 3 plus the expected energy; and test one
FockMap-to-Qiskit-style Pauli-label reversal.

---

## Medium

### CA-M001 — “Diagonal = classical” and “correlation energy lives entirely in off-diagonal terms” are technically false

**Classification:** Newly identified.
**Targets:** `manuscript/06-building-hamiltonian.md:13-98`, `manuscript/06-building-hamiltonian.md:220-236`, `manuscript/09-verification.md:63-75`.

A diagonal Hamiltonian does not force every state’s density matrix to be diagonal; it only fails to create computational-basis coherence from a diagonal initial state. Degenerate diagonal Hamiltonians may have coherent ground states. More importantly, the energy lowering of a correlated state relative to the HF determinant contains both off-diagonal matrix-element contributions and changes in diagonal configuration populations. It is not “exactly” the expectation of the four off-diagonal Pauli terms.

The four-body H₂ terms are pair-excitation couplings, not a clean synonym for the chemistry exchange integral.

**Correction direction:** Present diagonal/off-diagonal structure as basis-dependent configuration energies and couplings. Explain that coupling enables configuration mixing, while the final correlation energy is the difference of full variational minima and includes both changed populations and interference terms.

---

### CA-M002 — The Trotter step rules and 1-norm bounds are presented as guarantees without the required hypotheses

**Classification:** Unresolved historical `p5-f7` and `p5-f11`.
**Targets:** `manuscript/15-trotter-formulas.md:95-107`, `manuscript/15-trotter-formulas.md:137-177`, `manuscript/15-trotter-formulas.md:181-195`.

\(\Delta t\le1/\|H\|_1\) is a heuristic scale, not a theorem that the approximation is accurate. The second-order `1/12` formula is not established from the cited generic nested-commutator expression for the chosen ordering. The numerical use of \(\|H\|_1\approx3.7\) also includes the identity/global-phase coefficient, which contributes no Trotter error.

Claims that second order “typically needs \(\sqrt N\) fewer steps,” that 52 steps “suffice,” and that H₂O costs 500,000 CNOTs need either an actual commutator evaluation or explicit heuristic labeling.

**Correction direction:** State a precise theorem for the exact product ordering and norm, exclude identity terms, compute the committed H₂ commutator bound, and label empirical step choices separately from rigorous upper bounds.

---

### CA-M003 — The molecular CNOT/resource tables are not reproducible and contradict one another

**Classification:** Unresolved historical systemic issue `S2` and `p4-f7`/`p5-f13`.
**Targets:** `manuscript/13-tapering-benchmarks.md:114-123`, `manuscript/17-cost-analysis.md:45-88`, `manuscript/22-scaling.md:31-45`, `manuscript/22-scaling.md:73-88`, `manuscript/22-scaling.md:113-123`.

No committed molecule-specific artifact produces the H₂O, LiH, N₂, or FeMo-co tables. Chapter 17 gives FeMo-co as roughly `500,000` JW versus `20,000` TT CNOTs/step (25×); Chapter 22 gives roughly \(10^7\) versus \(10^5\) (100×). Chapter 13 assigns about 1,800 CNOTs to a 14-qubit full-space H₂O model while Chapter 17 assigns the same number to a different 12-qubit frozen-core model.

The tables also infer full Hamiltonian cost from worst-case ladder-operator weight, which is insufficient: term distribution, orbital ordering, cancellation, tapering sector, and connectivity all matter.

**Correction direction:** Create one generated benchmark artifact with geometry, basis, active space, electron sector, orbital ordering, encoding version, tapering generators/sector, term list, Trotter ordering, and logical connectivity assumptions. All chapters should render or quote that artifact.

---

### CA-M004 — Binary and ternary trees do not have different asymptotic logarithmic classes

**Classification:** Newly identified.
**Targets:** `manuscript/05-visual-encodings.md:250-280`, `manuscript/05-visual-encodings.md:320-340`, `manuscript/07-six-encodings.md:106-117`, `manuscript/08-building-vlasov.md:194-211`.

The base of a logarithm is a constant factor:

$$
\log_3 n=\frac{\log_2 n}{\log_2 3}=\Theta(\log n).
$$

Thus “going below \(O(\log_2 n)\)” and a different “best asymptotic scaling” are misleading. Ternary-tree constructions improve the leading constant and attain a relevant Pauli-weight lower bound; they do not change \(\Theta(\log n)\) to a smaller asymptotic class.

**Correction direction:** Use \(\Theta(\log n)\) for both and state the exact/ceiling weight bounds or the approximately \(1/\log_2 3\) depth factor supported by Jiang et al.

---

### CA-M005 — Chapter 7 directly contradicts itself about H₂ Pauli strings

**Classification:** Unresolved historical `p3-f3`/`p5-f8`.
**Targets:** `manuscript/07-six-encodings.md:40-53`, `manuscript/07-six-encodings.md:187-199`.

Lines 49–53 correctly say the strings may differ, while line 191 says all encodings produce identical Pauli strings. The reported per-encoding H₂ costs in Chapter 17 also differ, which is incompatible with identical strings.

**Correction direction:** Say that this H₂ input happens to simplify to 15 terms in each of the six implementations, but the strings and weights differ. Restrict same-term-count claims to the demonstrated Clifford-equivalent implementations and tested input.

---

### CA-M006 — The greenhouse explanation makes the bent angle a necessary and nearly sufficient cause

**Classification:** Newly identified.
**Targets:** `manuscript/19-bond-angle.md:238-256`.

A permanent dipole is not required for greenhouse activity: a vibration is IR active when it changes the dipole moment, as linear CO₂ demonstrates. Water’s bent equilibrium structure gives it a permanent dipole and a strong rotational spectrum, and its vibrational modes also absorb IR, but the angle is not “literally the reason Earth has a habitable temperature.”

**Correction direction:** Describe the bend as one contributor to water’s dipole, rotational spectrum, and vibrational selection rules. Remove the sole-cause habitability claim.

---

### CA-M007 — Trust-critical hardware and resource claims lack adequate, versioned sources

**Classification:** Historical `p2-f8` and `p7-f10` remain unresolved/partial.
**Targets:** `manuscript/04-qubits-gates-circuits.md:59-101`, `manuscript/17-cost-analysis.md:60-88`, `manuscript/20-algorithms.md:145-171`, `manuscript/22-scaling.md:92-123`, `jose/paper.bib:4-11`.

The hardware gate times/error rates have no dated calibration source and line 61 applies 20–100 ns single-qubit timing across superconducting and trapped-ion devices. The QPE and physical-qubit totals are not derived from a cited resource model. `jose/paper.bib` is not a book bibliography and still contains the placeholder DOI `10.5281/zenodo.XXXXXXX`.

**Correction direction:** Add dated primary/vendor calibration citations, a reproducible resource model, and stable references for all quantitative FeMo/QPE/QEC claims. Replace or remove the placeholder DOI.

---

### CA-M008 — Appendix B is omitted from PDF/EPUB and one Pages workflow despite the project claiming two appendices

**Classification:** Newly identified publishing inconsistency.
**Targets:** `README.md:58-74`, `myst.yml:64-67`, `manuscript/Book.txt:1-25`, `.github/workflows/pages.yml:27-32`, `.github/workflows/release.yml:43-54`.

MyST includes both appendices, but `Book.txt` includes only `appendix-cookbook.md`; the release workflow builds PDF/EPUB from `Book.txt`, and `pages.yml` copies only that appendix. The repository’s own description says “2 appendices.”

**Correction direction:** Put `appendix-theory.md` in the canonical book manifest and every publishing path, or change the project description and MyST TOC to say there is only one.

---

### CA-M009 — The appendix calls the phase-free Pauli basis a group

**Classification:** Newly identified.
**Target:** `manuscript/appendix-theory.md:36-47`.

The \(4^n\) phase-free strings are an operator basis but are not closed under multiplication: \(XY=iZ\). The Pauli **group** includes phases (commonly \(\{\pm1,\pm i\}\), with convention-dependent quotients).

**Correction direction:** Distinguish the \(4^n\)-element phase-free Pauli basis from the Pauli group with phases.

---

### CA-M010 — The “previously undocumented” star-tree restriction is a novel theorem asserted without evidence

**Classification:** Newly identified; cross-repo pending.
**Targets:** `manuscript/07-six-encodings.md:123-166`, `manuscript/08-building-vlasov.md:5-19`, `labs/08-h2o-workshop.fsx:464-470`.

The manuscript elevates an internal investigation to a general theorem about the Seeley–Richard–Love index-set framework, but provides no proof, precise hypotheses, counterexample, or research citation.

**Correction direction:** Do not publish this as established fact until the `johnazariah/encodings-research` report supplies a formal statement and proof. Otherwise narrow it to a limitation of the current FockMap construction.

---

### CA-M011 — The detailed H₂ measurement-group table is not a valid qubit-wise-commuting partition

**Classification:** Newly identified; API behavior cross-repo pending.
**Targets:** `manuscript/20-algorithms.md:53-69`, `manuscript/20-algorithms.md:190-218`.

For the stated QWC rule, all non-identity Z-only H₂ terms can share one computational-basis group, while each of the four distinct full-support X/Y strings requires its own local basis. The displayed group sizes `5, 3, 3, 2, 2` do not reflect that partition and line 216 incorrectly says Group 1 contains all diagonal terms.

**Correction direction:** Print the actual `groupCommutingTerms` output from a pinned FockMap version and state whether grouping is QWC or general commuting. For QWC, expect one Z group plus four X/Y basis groups (with identity dropped or assigned arbitrarily).

---

## Low

### CA-L001 — Two spin/orbital implementation claims are overgeneralized

**Classification:** Newly identified.
**Targets:** `manuscript/03-spin-orbitals.md:38-49`, `manuscript/03-spin-orbitals.md:63-79`.

The \(Z^4\) spin-orbit scaling is a hydrogenic rule of thumb, not a universal molecular law. PySCF generally supplies spatial-orbital integrals and does not have a universal “default interleaved spin-orbital ordering”; the interleaving is imposed by conversion code.

**Correction direction:** Qualify \(Z^4\) as hydrogenic/rough and attribute interleaving to this book’s conversion/FockMap convention.

---

### CA-L002 — The stated HF share of the H₂O bending energy does not match the stated numbers

**Classification:** Newly identified.
**Target:** `manuscript/19-bond-angle.md:207-215`.

Using the values in the paragraph, \(0.113/0.126=0.897\), so HF accounts for about 90%, not 85%, of the stated bending energy.

**Correction direction:** Recompute from the underlying scan and report one consistent rounded percentage.

---

### CA-L003 — “This is exact. No approximations.” omits the model assumptions already made

**Classification:** Newly identified.
**Target:** `manuscript/01-electronic-structure.md:49-63`.

The displayed Hamiltonian is the nonrelativistic, point-charge Coulomb Hamiltonian; it omits relativistic/QED effects and other corrections. It is exact only within that model.

**Correction direction:** Replace the absolute statement with “exact within the nonrelativistic Coulomb model before Born–Oppenheimer separation.”

---

### CA-L004 — The advertised companion-script count does not match the current tree

**Classification:** Newly identified.
**Targets:** `README.md:58-74`, `Makefile:58-59`.

The repository currently contains nine executable source scripts under `code/` (five F# and four Python), while the project metadata says ten companion scripts. This may be a missing artifact or stale count.

**Correction direction:** Define what is counted and make the metadata and packaged files agree.

---

# Strengths

1. **The complex-orbital ERI symmetries are now stated correctly.** `manuscript/02-notation.md:107-131` distinguishes complex and real cases and fixes the historical conjugation error.
2. **The book now declares its bit/string ordering.** `manuscript/05-visual-encodings.md:13-16` makes the ket and Pauli-string conventions explicit.
3. **The canonical anticommutation relations and JW occupation convention are consistent.** `manuscript/05-visual-encodings.md:43-53` and `manuscript/appendix-theory.md:49-59` agree on \(a^\dagger=(X-iY)/2\) for \(|1\rangle=\) occupied.
4. **Nuclear repulsion is separated from the electronic Pauli Hamiltonian.** `manuscript/06-building-hamiltonian.md:118-128` and `:302-305` resolve the earlier \(V_{nn}\) ambiguity.
5. **The revised water narrative correctly says bending already occurs at HF.** `manuscript/19-bond-angle.md:185-215` no longer attributes the qualitative bend to correlation.
6. **The local PySCF CSV/JSON artifacts are mutually useful reference points.** In particular, `code/h2_dissociation.csv` and `code/h2_dissociation_integrals.json` make the H₂ numerical corruption diagnosable.
7. **The \(R_z\) half-angle convention is now explicit.** `manuscript/18-complete-pipeline.md:107-110` and `manuscript/21-circuit-export.md:55` agree on \(R_z(\theta)=e^{-i\theta Z/2}\).
8. **Foundational encoding, tapering, Trotter, VQE, and QPE literature is named.** The remaining citation problem is chiefly quantitative provenance and the absence of a coherent book bibliography, not a total absence of sources.

---

# Independent quantitative checks

No shell execution facility was available in this review session, so no repository commands were run and no generated data were modified. The following checks were independently re-derived from committed files:

| Check | Inputs | Result |
|---|---|---|
| Real-orbital ERI symmetry | `03-spin-orbitals.md:175-176` | `[01|01]` and `[01|10]` must be equal; current values differ by `0.5166469769` Ha |
| 0.74 Å HF energy | `h=−1.2533097866`, `[00|00]=0.6747559268`, `Vnn=0.7151043391` | \(2h+[00|00]+V_{nn}=-1.1167593074\) Ha |
| 0.74 Å electronic FCI energy | CSV total `−1.1372838345`, `Vnn=0.7151043391` | `−1.8523881736` Ha |
| Correlation energy | FCI total minus HF total | `−0.0205245271` Ha = about `−12.88 kcal/mol` |
| Correct four-body coefficient magnitude | transition ERI `0.1812104620` | `0.0453026155` Ha |
| Heisenberg CNOT conjugation | standard CNOT Pauli rules | \(YY\to-XZ\), not \(-Z\); sector spectra combine to \(\{-3,1,1,1\}\) |
| QPE ancilla arithmetic | \(\epsilon=0.0016,t_0=1\) | \(\lceil\log_2(2\pi/\epsilon)\rceil=12\), not 10 |
| H₂O bending fraction | stated `0.113/0.126` | `89.7%`, not 85% |
| FeMo determinant scale | central binomial \(\binom{108}{54}\) | about \(2.5\times10^{31}\), so “\(10^{30}\)” is only a loose order estimate |
| Appendix publication | MyST TOC vs `Book.txt`/workflows | Appendix B is absent from PDF/EPUB and one Pages path |

---

# Historical May 2026 plan status

The historical plan says 70 findings were fixed, but it does not enumerate a consistent set of 70: its severity tables, systemic-ID lists, and individual tables do not reconcile. Every item actually named in that file was nevertheless checked against current HEAD.

## Systemic items

| Historical item | Current status | Current evidence |
|---|---|---|
| S1 / `p4-f2,p5-f3,p7-f3,p1-f5` — H₂O 12 vs 14 | **Still closed** | Full 14-orbital and frozen-core 12-orbital contexts are now labeled (`17-cost-analysis.md:60-65`, `19-bond-angle.md:59-64`) |
| S2 / `p5-f1,p5-f4,p5-f5,p5-f6,p7-f1,p7-f5,p7-f6` — CNOT tables | **Unresolved** | FeMo tables still disagree; see CA-M003 |
| S3 / `p1-f1,p1-f6,p6-f1,p6-f3,h2o-correlation-fix` — correlation narrative | **Still closed** | `01-electronic-structure.md:21-24,161-164`; `19-bond-angle.md:207-215` |
| S4 / `p3-f2,p3-f9` — \(V_{nn}\) | **Still closed** | `06-building-hamiltonian.md:118-128,302-305`; vacuum energy remains electronic zero |

## Individually listed items

| Status | Historical IDs |
|---|---|
| **Still closed** | `p1-f2`, `p2-f1`, `p4-f1`, `p7-f2`, `p2-f2`, `p2-f3`, `p2-f4`, `p2-f5`, `p1-f4`, `p3-f4`, `p3-f6`, `p3-f7`, `p4-f3`, `p4-f4`, `p5-f2`, `p6-f4`, `p6-f6`, `p7-f4`, `p7-f7`, `p7-f8`, `p7-f9`, `p2-f6`, `p2-f7`, `p2-f9`, `p2-f10`, `p1-f7`, `p3-f8`, `p4-f5`, `p4-f6`, `p5-f9`, `p5-f10`, `p6-f7`, `p6-f8`, `p6-f9`, `p7-f11`, `p7-f12`, `p7-f13` |
| **Regression relative to claimed closure** | `p6-f2` (wrong H₂ values returned), `p3-f5` (exchange derivation is still wrong), `p4-f8` (Heisenberg example is completed but incorrect) |
| **Unresolved** | `p3-f3`, `p5-f7`, `p5-f8`, `p4-f7`, `p4-f9`, `p5-f11`, `p5-f13`, `p2-f8`, `p7-f14` |
| **Partially addressed, still open** | `p6-f5` (added \(2\pi\) but omitted evolution time/range), `p7-f10` (a citation was added but does not derive the stated overhead) |
| **Superseded by a broader current defect** | `p3-f1` and `p1-f3` by CA-H001/003; `p1-f8` because the lab and chapter now agree with each other but share the same invalid integral tensor |

No historical item is marked fixed merely because a number changed; the current derivation or local reference had to support it.

---

# Cross-repo pending items

These claims cannot be closed from this repository and must remain pending until the named reports are available.

## `johnazariah/encodings`

1. Verify CAR, adjoint relations, qubit ordering, and matrix equivalence for all six ladder maps over representative and boundary sizes.
2. Verify `computeHamiltonianWith` uses the manuscript’s physicist-index order, annihilator order, and \(1/2\) convention.
3. Verify the exact H₂ Pauli coefficients/signs from the corrected 0.74 Å tensor.
4. Verify `findCommutingGenerators` selects a mutually commuting independent subgroup, tracks phases, and maps physical sectors correctly.
5. Verify H₂ tapering really gives 4→2 qubits and five terms in the intended two-electron singlet sector for every encoding.
6. Verify skeleton/direct construction parity across geometries, including changing zero patterns.
7. Verify first/second-order rotation lists, identity handling, \(R_z\) angles, and reported CNOT statistics.
8. Verify `groupCommutingTerms` means QWC or general commutation and capture its actual H₂ grouping.
9. Verify `estimateShots`, `qpeResources`, OpenQASM, Q#, and JSON APIs exist with the semantics claimed by Chapters 20–21.
10. Verify custom `EncodingScheme` and arbitrary ternary-tree validation; local labs currently construct schemes without demonstrating CAR.

## `johnazariah/encodings-research`

1. Supply the precise theorem/proof, hypotheses, and literature relationship for the claimed star-tree restriction on the index-set framework.
2. Supply proof and exact constants for balanced ternary/Vlasov tree weight bounds and the claimed optimality/lower bound.
3. Establish that the implemented “Vlasov tree” corresponds to Vlasov’s construction, not only to a breadth-first tree inspired by it.
4. Supply reproducible H₂O/LiH/N₂/FeMo-co term and CNOT benchmark reports, including orbital ordering and active-space assumptions.
5. Supply any defensible fault-tolerant FeMo/QPE resource estimate used in Chapters 20 and 22.

---

# Reference gaps

1. The manuscript has chapter-level “Further Reading” lists but no coherent, linked book bibliography. `jose/paper.bib` is short, publication-specific, and contains a placeholder FockMap DOI.
2. Hardware calibration numbers in Chapter 4 need dated primary/vendor sources.
3. H₂O basis-comparison values need archived command output or generated tables, not only hard-coded summary print statements.
4. All molecule-level CNOT and fault-tolerant resource tables need a citable generated artifact.
5. The star-tree theorem and FockMap-specific design claims need a research report or must be narrowed to implementation observations.
6. The FeMo active-space citation is adequate for the approximate 54-electron/54-spatial-orbital model; it does not support the book’s new ternary-tree gate totals.

---

# Current assessment

**Not publication-ready on correctness.** The H₂ source tensor, central Hamiltonian derivation, spectrum, capstone execution story, tapering derivation, H₂O provenance, and QPE/circuit verification claims all require correction. The committed PySCF reference data and several repaired conventions provide a strong recovery path, but historical closure should not be relied upon.

---

# Cross-repository input: `johnazariah/encodings`

**Report received:** 2026-07-22
**Status:** Authoritative audit input, not fix verification. A dedicated upstream
implementation session is in progress. Items below remain open until that session
reports a corrected build, regression tests, and exact outputs.

## Confirmed blocking defects

1. **Full-Clifford tapering corrupts Pauli phases and spectra.**
   `src/Encodings/Tapering.fs:581-582` uses an incomplete CNOT phase condition.
   The current flip `cx && tz` must include the tableau factor; the audited
   condition is `cx && tz && (tx = cz)`. Minimal counterexamples are
   `XY -> -YZ` in the library versus the correct `XY -> +YZ`, and
   `YZ -> -XY` versus the correct `YZ -> +XY`, under `CNOT(0,1)`.
   On the corrected H₂/STO-3G input, current `FullClifford` tapering gives
   `-1.8319` Ha rather than `-1.8524` Ha electronic and no sector recovers the
   true ground state. `DiagonalOnly` tapering re-verifies correctly.
   **Mapped items:** CA-H005, CA-H006.
2. **The test suite does not protect tapering spectrum preservation.**
   All 794 current tests pass, but no property compares tapered spectra or
   energies with the corresponding untapered sectors; existing CNOT tapering
   tests only require non-empty output. **Mapped items:** CA-H005, CA-H006 and
   companion verification work.
3. **Book snippets need explicit feature-module imports.** `open Encodings` alone
   does not bring the feature modules into scope. Depending on the snippet, the
   required imports include `Encodings.JordanWigner`, `.Hamiltonian`,
   `.Tapering`, `.Trotterization`, `.CircuitOutput`, and the BK, tree, Majorana,
   bosonic, cost-analysis, or FCIDUMP modules actually used. Snippet parity
   remains blocked until the corrected upstream build supplies exact compile
   results.

## Authoritative convention result

`PauliRegister` stores **qubit 0 at the leftmost signature position**.
Qiskit-style Pauli labels place qubit 0 at the rightmost character, so that
string boundary requires reversal. OpenQASM gate operands are explicitly
indexed (`q[i]`) and do not define a Pauli-string display convention. The book
and source tests now use occupation-integer matrix rows and state-resolved
checks consistently; ordering-convention is implemented pending independent
final verification.

## Clean results that may be used after version pinning

- All six encodings preserve the CAR at representative sizes `n=4` and `n=6`.
- Jordan-Wigner, Bravyi-Kitaev, and ternary-tree spectra agree before broken
  Full-Clifford tapering is applied.
- Pauli multiplication phases, Majorana signs, weight scaling, Trotter
  decomposition, OpenQASM export, `DiagonalOnly` tapering, and bosonic encoders
  re-verify cleanly.
- The ladder convention is
  \(a^\dagger=\tfrac12(Z\text{-chain})X-\tfrac{i}{2}(Z\text{-chain})Y\), with
  the sign of the \(Y\) term reversed for \(a\).
- The independent chemistry anchors agree with this audit:
  \(E_\mathrm{el}=-1.852388\) Ha and
  \(E_\mathrm{total}\approx-1.137284\) Ha after adding
  \(V_{nn}=0.715104\) Ha.

The cross-repository report also observed `23` stored Pauli entries and
\(\lambda=2.6993\) for its corrected H₂ input. Local package execution resolved
the term-count discrepancy: eight entries have zero coefficients after
distribution, leaving the standard 15 nonzero JW terms. Book companions now
filter `Complex.Abs coefficient > 1e-12` before reporting counts. The
coefficient 1-norm is `2.6993` including the known identity term and
`1.8871072169` for measured non-identity terms. CA-H002/003 still require the
upstream implementation session's exact coefficient/matrix and regression-test
outputs before package parity closes.

These norms have units Ha and are distinct from Chapter 15's first-order
commutator sum
\(\Lambda_{\mathrm{comm}}=0.2861997180\ {\rm Ha}^2\). The audit rejects bare
“lambda” notation unless the definition, included terms, and units are stated.

**Superseded diagnosis retained for traceability:** a live book-companion run
with the raw tensor passed directly to the low-level factory produced:
`dotnet fsi code/ch06-building-hamiltonian.fsx`, using the canonical 32-entry
physicist tensor, produces the correct set of 15 nonzero signatures but
coefficients such as `IIII = -3.5608` and four-body magnitude `0.0906`.
The independent direct/JW reference requires `IIII = -0.8121706072` and
four-body magnitude `0.0453026155`. All six package encodings report the same
term count, so cross-encoding agreement cannot diagnose this shared
factory/index/prefactor mismatch. This exact reproduction has been sent to the
upstream implementation session; CA-H002/003 remain blocked.

An exact package trace **supersedes** the interim library-defect diagnosis.
`computeHamiltonianWith` is a low-level **operator-coefficient** factory:
package key `(i,j,k,l)` multiplies
\(a_i^\dagger a_j^\dagger a_k a_l\), with no hidden \(1/2\). It is not a raw
integral factory. FCIDUMP already converts raw integrals to that operator-level
contract; PR #5 `2e1ee0d` removes misleading dead `0.5` code and documents/tests
the distinction.

For a raw physicist tensor in the book convention,
\(\tfrac12\langle pq\mid rs\rangle
a_p^\dagger a_q^\dagger a_s a_r\), the package adapter must return
`0.5 * raw[p,q,l,k]` for package key `(p,q,k,l)`.

The controlled four-way experiment is therefore an adapter test:

| Caller adapter | `IIII` | `XXYY` | Interpretation |
|---|---:|---:|---|
| raw map passed directly | `-3.5607946918` | `+0.0906052310` | wrong API level |
| half only | `-2.6445866636` | `+0.0453026155` | magnitude fixed, sign wrong |
| swap only | `+0.1040374210` | `-0.0906052310` | sign fixed, doubled |
| half + swap | `-0.8121706072` | `-0.0453026155` | correct low-level adapter |

For the current raw path,
\(H=H_1-2H_{2,\mathrm{standard}}\), dense max error is `10.9944963384`,
and the two-electron spectrum is
`[-3.9385781287, -3.4182223622, -2.6933805142 x3, -2.2629940047]`.
With both corrections, dense max error is `2.59e-10`.

The eight additional stored signatures are exact zeros:
`XXXY`, `XXYX`, `XYXX`, `XYYY`, `YXXX`, `YXYY`, `YYXY`, `YYYX`.
They inflate unfiltered term/cost counts; package zero-pruning remains open.
Book paths filter at `1e-12`.

**Book implementation:** preserve the raw un-antisymmetrized physicist tensor,
unrestricted \(1/2\) Hamiltonian, \(a_s a_r\) order, and separate \(V_{nn}\);
adapt only at the low-level factory boundary. The explicit adapter now passes:

- all 15 coefficients and sentinels;
- JW dense matrix versus direct oracle, max error `6.661e-16`;
- all-six spectral moments, max relative error below `3e-15`;
- all numbered labs; and
- isolated Chapter 18 pipeline output.

CA-H002/003 now await final audit of `2e1ee0d` API wording and the package
zero-pruning decision, not a numerical book correction.

## Non-blocking upstream integration notes

- The README quick-start input produces five diagonal terms, not the claimed
  15-term molecular Hamiltonian.
- Upstream README/JOSS chapter and test counts are stale.
- The Trotter API drops imaginary coefficient parts and therefore needs a
  documented or enforced Hermitian-Hamiltonian precondition.
- Inclusive `0..n` Hamiltonian loops are benign but off by one; the bosonic
  argument order is easy to misuse and under-documented.

## Provisional `encodings` repair output

**Received:** 2026-07-22
**Tapering status:** Configuration table verified in PR #5 commit `b2f9bc3`.
Package Hamiltonian integration remains blocked on the missing builder/FCIDUMP
follow-up.

- The CNOT phase condition is now `cx && tz && (tx = cz)`. All 16 Pauli
  conjugations pass, and dense \(UHU^\dagger\) error is `4.4e-16`.
- The tapering tests use an oracle/adapted H₂ Hamiltonian with electronic ground
  energy `-1.852388` Ha; this is not evidence that the public raw builder is fixed.
- FullClifford's default `(+1,+1,+1)` sector reduces this JW instance from four
  to one qubit but has spectrum `[0, 0.2081]`; it is a genuine **non-ground**
  sector.
- Sector `(-1,-1,+1)` reduces the same encoded instance from four to one qubit
  and preserves `-1.852388` Ha. The union of all eight sector spectra equals
  the original 16 eigenvalues.
- JW `DiagonalOnly` is a four-to-four no-op for this full H₂ instance. Earlier
  BK diagonal reductions are encoding-specific and cannot be generalized.
- PR #5 reports 845 passing tests for the earlier tapering/docs/order scope.
  They do not cover the absent Hamiltonian/FCIDUMP/zero-pruning repair. The
  off-by-one follow-up also remains open because the attempted range underflows
  at `n=0`.

No bare “4→N” count is accepted. Every count must name the input, encoding,
symmetry method, generators, sector, and spectrum comparison. Executable book
paths currently stay untapered rather than silently using the default
non-ground sector.

The verified canonical H₂ tapering configurations are:

| Encoding | Method | Result | Removed | Sector status |
|---|---|---|---|---|
| JW | DiagonalOnly | 4→4 | none | no single-qubit Z generators |
| JW | FullClifford | 4→1 | `[0,1,2]` | default `+++` is non-ground; ground `--+` |
| BK | DiagonalOnly | 4→2 | `[1,3]` | qubits 1/3 fixed `+,+`; classic qualified H₂→2 case |
| BK | FullClifford | 4→1 | `[0,1,3]` | default `+++` not established as physical ground |

Both encodings have three Z₂ symmetries; the encoding changes which symmetries
are exposed as single-qubit Z operators. The synthetic cookbook 4→2 example is
separate and remains valid. Main baseline is 794 tests; 51 added cases give 845
on this commit.

The post-fix audit is **not closure-ready**. Remaining upstream blockers are:

1. the `0u .. n-1u` repair underflows at `n=0`; safe ranges/guards and zero-mode
   tests are required;
2. upstream README/cookbook still contain an unqualified full-H₂ 4→2 claim;
3. encoding/chapter/test counts remain stale in upstream docs;
4. “arbitrary tree” exceeds the implemented max-three-child contract and lacks
   a four-child rejection/validation explanation;
5. ordering must say Qiskit-style labels and visibly document q0-leftmost
   signatures;
6. public Trotter APIs must document/enforce the Hermitian-coefficient
   precondition; and
7. one H₂ test remains tautological and needs an independent expected result.

The book has already removed default-sector execution, bare taper counts, and
arbitrary-tree claims, but CA-H005/006, ordering-convention, and API parity stay
open until these source blockers are repaired and re-audited.

---

# Cross-repository input: `johnazariah/encodings-research`

**Report received:** 2026-07-22
**Status:** Scripts, notebook, canonical `physicist_tensor.json`, H₂/tree/BK/
Appendix/CNOT/bibliography/Δλ evidence are post-fix green. Final
Qiskit-style-label versus OpenQASM-operand wording audit, reachable artifact
delivery, and merge remain pending.

## Safe anchors

1. An exhaustive CAR census over all \(n^{n-1}\) rooted labelled trees for
   \(n=3,\ldots,6\) reproduces the star-only condition for the audited
   tree-to-index-set construction: exactly \(n\) trees pass at each size, and
   they are the stars. A ternary \(n=8\) candidate has CAR deviation `2`, while
   the star has deviation `0`. The provisional repair now tests passing stars
   and failing chain, balanced-binary, and Fenwick inputs explicitly.
2. The audited maximum-Pauli-weight table through \(n=24\) reproduces exactly
   against `Encodings.dll`. The live book companion independently extends the
   tested package values to \(n=64\): JW `64`, BK `7`, ternary `6`.
3. CAR, Majorana, and ladder-sign conventions agree between the research paper's
   Appendix A and the library. The repaired matrix tooling reverses displayed
   signatures so mode \(j\) has matrix/occupation place value \(2^j\); this now
   agrees with the book's state-resolved oracle.

## High blockers

1. `H2Demo.fsx`, `IntegralTables`, `DiagCheck`, and `Quick2x2` use the corrupted
   `0.6975782` value for `[01|01]`/`[01|10]`. The research repository's own
   `IntegralCheck` and physical benchmark require approximately `0.18121`,
   which reproduces total FCI energy near `-1.1373` Ha. This independently
   supports CA-H001/003 and disqualifies the older research chain as provenance.
2. Review section 9's total H₂ energy `-1.0471` Ha is unsupported and wrong.
   Its cross-encoding difference near `4.44e-16` proves only mutual agreement
   on a shared corrupted Hamiltonian.
3. The software-paper claim that the tree-derived index-set construction works
   for Fenwick trees conflicts with the exhaustive star-only result; the tested
   Fenwick construction fails. This does not invalidate the separately derived
   standard BK mapping, but it forbids deriving its correctness from the generic
   tree-to-index-set claim. **Mapped item:** CA-M010.
4. `papers/results/` contains no data, scripts, or figures supporting the
   molecular CNOT reductions, tapering totals, or coefficient-norm claims.
   Those numbers are not reproducible citations for this book.
   **Mapped items:** CA-M003, CA-M007.

## Medium blockers

- Appendix B's intermediate operators and anticommutators are wrong even though
  its final result is correct; do not cite that derivation.
- At \(n=64\), ternary maximum weight `6` implies `10`, not `8`, CNOTs under
  the standard staircase. The claimed `16x` reduction is about `12.6x`.
- BK Fenwick parent/update formulas need `lsb(j+1)`; the audited parity formula
  and implementation are correct.
- Research scripts have stale assembly paths/module imports, undeclared Python
  dependencies, and stale notebook outputs. They are not executable provenance
  until the repair session reproduces them.

## Book decisions pending repaired artifacts

- Preserve the directly reproduced star census and package weight anchors, but
  do not cite the inconsistent software-paper derivation as support.
- Keep the book's internal/external Pauli-order wording open until one bitstring,
  matrix, and exported-label example agrees across both repositories.
- Remove the H₂O/LiH/N₂/FeMo CNOT tables and fault-tolerant totals unless the
  repair session supplies versioned machine-readable inputs and outputs.
- Never use cross-encoding agreement as physical validation without an
  independent matrix or chemistry reference.

## Provisional labelled tapering evidence

The repaired research report now labels every row by encoding, method,
generator count, and sector:

| Encoding | Method | Result | Sector | Coefficient 1-norm |
|---|---|---|---|---:|
| JW | DiagonalOnly | 4→4, no generators | — | `2.699278` Ha |
| Ternary Tree | DiagonalOnly | 4→4, no generators | — | `2.699278` Ha |
| BK | DiagonalOnly | 4→2, two generators | `++` | `2.013074` Ha |
| Parity | DiagonalOnly | 4→2, two generators | `++` | `2.013074` Ha |
| Balanced Binary | DiagonalOnly | 4→2, two generators | `++` | `2.013074` Ha |

FullClifford rows remain upstream evidence pending the final library branch and
are not locally recomputed in `encodings-research`. No post-taper count or norm
is a general H₂ fact; all remain provisional until the final library audit.

---

# Primary-source provenance

**Audit received:** 2026-07-22
**Rule:** Citation adjacency is not support. Each entry records what the source
actually establishes and whether the book must correct, narrow, remove, or add
a generated artifact.

| ID | Claim | Primary source / artifact | What it actually supports | Decision |
|---|---|---|---|---|
| PR-001 | Canonical H₂/STO-3G integrals | Sun et al. (2020), DOI [10.1063/5.0006074](https://doi.org/10.1063/5.0006074); local PySCF 2.13 generator/verifier | PySCF method only; exact values require geometry, version, MO/tensor convention, symmetry checks, and committed output | **Correct; add artifact metadata** |
| PR-002 | H₂ HF/MP2/FCI energies | Sun et al. (2020); Seeley et al. (2012), DOI [10.1063/1.4768229](https://doi.org/10.1063/1.4768229); local direct/JW verifier | Local artifact establishes this geometry's values; cross-encoding agreement alone does not | **Correct and cite artifact/software** |
| PR-003 | “FCI is exact” | Seeley, Richard & Love (2012) plus the definition of finite-basis FCI | Exact only within the chosen finite orbital basis, Hamiltonian model, particle/spin sector | **Narrow** |
| PR-004 | Quantum representation/advantage | Kempe, Kitaev & Regev (2006), DOI [10.1137/S0097539705447256](https://doi.org/10.1137/S0097539705447256); Reiher et al. (2017), DOI [10.1073/pnas.1619152114](https://doi.org/10.1073/pnas.1619152114) | General local-Hamiltonian hardness and QPE overlap dependence; no generic chemistry speedup | **Narrow and cite** |
| PR-005 | Product-formula error | Childs et al. (2021), DOI [10.1103/PhysRevX.11.011020](https://doi.org/10.1103/PhysRevX.11.011020) | Ordering-dependent pair/nested-commutator bounds; not \(\Delta t\le1/\|H\|_1\) as a theorem or former numeric guarantees | **Correct/narrow; generated H₂ bound required** |
| PR-006 | H₂O 99° result | Sun et al. (2020); Hoy & Bunker (1979), DOI [10.1016/0022-2852(79)90019-5](https://doi.org/10.1016/0022-2852(79)90019-5); local CSV/plot/lab | PySCF method and experimental angle; exact grid minimum is a repository calculation at fixed \(r_{\rm OH}\) | **Narrow; add artifact metadata** |
| PR-007 | Larger-basis H₂O minima | No primary source located for the exact HF/MP2/CASCI table | No support for orbital selection, active space, state, scan, or exact minima | **Remove unless artifact is committed** |
| PR-008 | Water spectroscopy/climate | Shimanouchi (1972), DOI [10.6028/NBS.NSRDS.39](https://doi.org/10.6028/NBS.NSRDS.39); Shostak et al. (1991), DOI [10.1063/1.460471](https://doi.org/10.1063/1.460471); NASA water-vapour feedback | 1595 cm\(^{-1}\) bend, permanent dipole, dipole-change IR rule, qualified feedback; not sole-cause habitability | **Correct/narrow and cite** |
| PR-009 | Ternary-tree bounds | Jiang et al. (2020), DOI [10.22331/q-2020-06-04-276](https://doi.org/10.22331/q-2020-06-04-276); Vlasov (2022), DOI [10.12743/quanta.v11i1.199](https://doi.org/10.12743/quanta.v11i1.199); Seeley et al. (2012) | Jiang: Majorana weight \(\lceil\log_3(2n+1)\rceil\), average lower bound \(\log_3(2n)\), still \(\Theta(\log n)\); not FockMap correspondence | **Correct metadata/result and cite** |
| PR-010 | Star-only/tree implementation claims | Reproduced `encodings-research` exhaustive CAR census; literature does not certify FockMap correspondence | Star-only condition for the audited tree-derived index-set constructor; inconsistent paper derivation is not support | **Keep cross-repo blocked** |
| PR-011 | Tapering/sector rules | Bravyi et al. (2017), OA [arXiv:1701.08213](https://arxiv.org/abs/1701.08213) | Independent commuting symmetries and a selected eigenvalue sector; not a molecule's generator-to-\((N,M_S,\ldots)\) map | **Correct/narrow; generated sector tests required** |
| PR-012 | Shot/grouping formula | Wecker et al. (2015), DOI [10.1103/PhysRevA.92.042303](https://doi.org/10.1103/PhysRevA.92.042303); Yen et al. (2020), DOI [10.1021/acs.jctc.0c00008](https://doi.org/10.1021/acs.jctc.0c00008) | Independent-term worst-case one-norm bound; grouped shots need covariance, general commuting may need entangling measurement | **Narrow; API/group artifact required** |
| PR-013 | QPE formula/resources | Kitaev (1995), OA [arXiv:quant-ph/9511026](https://arxiv.org/abs/quant-ph/9511026); Reiher et al. (2017) | Phase-energy relation, range/aliasing, overlap and confidence conditions; no book-specific gate totals | **Correct/narrow; generated resources required** |
| PR-014 | Hardware gate table | IBM dated backend-calibration documentation; Quantinuum machine-specific validation documentation | Only named backend, revision, native gate, timestamp and benchmark definition; no platform constants | **Remove numerical table** |
| PR-015 | Molecule-level CNOT tables | None; `encodings-research/papers/results` is empty | No H₂O/LiH/N₂/FeMo benchmark provenance | **Removed** |
| PR-016 | FeMo-co active space/resources | Reiher et al. (2017); Lee et al. (2021), DOI [10.1103/PRXQuantum.2.030305](https://doi.org/10.1103/PRXQuantum.2.030305) | CAS(54e,54o) supports 108 spin orbitals; resource totals are architecture-specific and not interchangeable | **Retain active-space fact; remove mixed totals** |
| PR-017 | Generic physical/logical-qubit ratios | No supporting source; former citation was Gidney & Ekerå's RSA paper, DOI [10.22331/q-2021-04-15-433](https://doi.org/10.22331/q-2021-04-15-433) | Does not support generic chemistry error-correction ratios or totals | **Removed** |
| PR-018 | DFT/CCSD(T)/DMRG scope | Bowler & Miyazaki (2010), DOI [10.1088/0953-8984/22/7/074207](https://doi.org/10.1088/0953-8984/22/7/074207); Bartlett & Musial (2007), DOI [10.1103/RevModPhys.79.291](https://doi.org/10.1103/RevModPhys.79.291); White (1992), DOI [10.1103/PhysRevLett.69.2863](https://doi.org/10.1103/PhysRevLett.69.2863); Schollwock (2011), DOI [10.1016/j.aop.2010.09.012](https://doi.org/10.1016/j.aop.2010.09.012) | Conventional KS diagonalization is often cubic but linear-scaling variants exist; canonical single-reference CCSD(T) has an \(O(N^7)\) triples step; DMRG excels for low-entanglement 1D/quasi-1D structure | **Narrow and cite; no fixed atom limits or blanket quantum niche** |

## Citation corrections

- The relevant Jiang publication is *Quantum* **4**, 276, not *PRX Quantum*
  1, 010306; the latter DOI belongs to an atom-interferometry paper.
- The Vlasov DOI is `10.12743/quanta.v11i1.199`, not `.166`.
- The relevant general commutator-scaling source is Childs et al., *PRX* 11,
  011020 (2021). The cited `PRL 120, 250503 (2018)` is not that result;
  Childs & Su's lattice paper is *PRL* 123, 050503 (2019).
- The Tranter bibliography entry duplicates Sherrill and omits authors; correct
  DOI: [10.1002/qua.24969](https://doi.org/10.1002/qua.24969).
- The Zenodo concept DOI `10.5281/zenodo.18917465` is valid. Version `0.9.0`
  is `10.5281/zenodo.20148795`.

## Publication metadata blocker

### OWNER/RELEASE BLOCKER — mixed rights versus Zenodo archive metadata

The GitHub policy is explicit: companion code is MIT; manuscript text is
all-rights-reserved. `CITATION.cff` now points to that policy rather than
claiming one archive-wide SPDX license. The external Zenodo record still
conflicts:

- concept DOI: `10.5281/zenodo.18917465`;
- version `0.9.0` DOI: `10.5281/zenodo.20148795`;
- current Zenodo resource type: `software`;
- current Zenodo license: `MIT`;
- archive files combine manuscript and code.

The owner must choose and apply a release model. Before the next deposit:

1. decide whether to split book and code into separately licensed Zenodo
   records or use a mixed-rights archive that Zenodo can represent accurately;
2. update the version record's `resource_type`, `license`/rights field, and
   description so it does not grant MIT rights over the manuscript;
3. keep concept/version DOI relations and `version=0.9.0` accurate;
4. make GitHub README, `CITATION.cff`, Zenodo metadata, PDF/EPUB rights notice,
   and packaged code license files state the same scopes; and
5. inspect the deposited files and public landing page after publication.

This repository cannot repair the existing external record by adding another
local metadata file. Zenodo remains an owner/release blocker until the public
record is changed and checked.

Local artifacts now state the existing policy consistently:
`LICENSE-CODE`, `MANUSCRIPT-RIGHTS`, README, CFF license URL, rendered
PDF/EPUB rights notice, and release/lab packages. A package dry run contains
both rights files. This local consistency does **not** close the external
Zenodo blocker.

The FockMap placeholder DOI has been removed from `jose/paper.bib`; it must not
be replaced with the book DOI unless both citations refer to the same archived
work.

`manuscript/references.md` is the sole rendered bibliography source: it appears
once in `Book.txt`, `Sample.txt`, and `myst.yml`. `jose/paper.bib` is outside all
book manifests. The bibliography contains 18 unique DOI links and 3 additional
authoritative URLs; all 21 resolve (publisher endpoints returning HTTP 403
after DOI resolution were treated as access-controlled, not broken). No stale
Jiang, Vlasov, or placeholder DOI remains.

---

# Disposable-copy execution baseline

**Run:** 2026-07-22 against the repaired working tree; original worktree status
unchanged.

## Pass

- `make verify-data`: PySCF/tensor checksum, direct/JW reconstruction, all
  particle sectors, state ordering, coefficient norms, commutator sum, and
  byte-identical disposable regeneration.
- Labs 01, 04, 05, 06, 07, 08, and 09.
- Companions ch03, ch07, and ch11.
- `make data`: canonical CSV/JSON/PNG/metadata outputs reproduce byte-for-byte.
- MyST site: 27 pages, both appendices, References, no missing images.
- Current full PDF: 175 pages and 27 images; sample PDF: 55 pages; EPUB:
  27 content sections with 27 PNG assets.
- Local relative-link check and one Pages deployment workflow.
- Workflow artifact names match
  `molecules-to-circuits.pdf`, `molecules-to-circuits-sample.pdf`, and
  `molecules-to-circuits.epub`.

## Expected evidence gates

- Lab 02 and ch06/ch18 fail closed on the upstream H₂ coefficient mismatch.
- Labs 03/10 build dense \(16\times16\) matrices and compare all 16 spectral
  moments at relative tolerance `1e-7`; the current package fails moment 1 by
  `3.384e+00`, exposing the shared-Hamiltonian error.
- `make lab-check` executes labs and stops at the first upstream blocker.
- `make pipeline-check` proves canonical data immutability, rejects source-tree
  derived output, and propagates the upstream blocker.
- ch12 no longer presents a default-sector taper result; FullClifford closure
  remains with CA-H005/006.

No expected blocker is counted as a new finding. It remains attached to the
existing CA/CP item.

---

# Verification addendum — 2026-07-23

**Reviewed scope:** Current uncommitted remediation; all action-plan IDs;
canonical H₂ artifacts; `encodings-research` fixture commit
`1e000bbc9664b8e5cfef48608d07364279c0a54f`; and `encodings` PR #6 exact commit
`440540b54f093d7a9f259da7bf18b7df8da74270`.

## Result

**Correctness is materially closed.** All local high-severity findings except
the explicitly partial CA-H008 API round-trip gate are resolved. The FockMap
Hamiltonian/tapering/order source contract is accepted at PR #6 commit level.
CA-M010 and CA-M011 retain narrower integration/API-output gates. The book may
proceed to whole-book tone and clarity review, but it is not release-ready until
the operational package and external metadata gates below close.

## Superseded source diagnostics

Maintainer delegation to the reconciliation process superseded the earlier raw
API choice. The authoritative nonbreaking contract is now:

- legacy `computeHamiltonianWith` consumes full weighted operator coefficients
  and remains compatible;
- `rawPhysicistToWeightedFactory` maps a raw single-bar tensor query
  `(i,j,l,k)` to weighted key `(i,j,k,l)` with coefficient
  \(\tfrac12\langle ij\mid lk\rangle\);
- `computeHamiltonianFromPhysicistWith` and
  `computeHamiltonianFromPhysicist` expose that raw path directly; and
- cancellation-aware assembly returns exactly 15 stored/nonzero terms for the
  canonical H₂ input while preserving standalone tiny coefficients.

PR #5, its provisional commits, the `23 stored` diagnosis, pending zero
pruning, and the book-internal half-and-swap adapter are historical. PR #6 at
`440540b` is the accepted source. Merge and package publication are operational
gates, not open source-correctness findings.

## Canonical fixture acceptance

`code/physicist_spin_integrals.json` is byte-identical to the audited
`encodings-research` artifact:

| Identity | Value |
|---|---|
| Source commit | `1e000bbc9664b8e5cfef48608d07364279c0a54f` |
| Source path | `papers/results/h2_sto3g/physicist_spin_integrals.json` |
| Git blob SHA-1 | `e0477e70c0dfd35b865000bb23b7b31882b062d3` |
| File SHA-256 | `6539afb30a1c03ec89202a2960a06c6580a91afaebf13a6cadbcfd32c2d71812` |
| Exact 4+32 map SHA-256 | `d5d8e2f7c83b1d1322f10217198d125b7c7cae1f99fa09a6e1e232ee38dd098c` |

The local PySCF regeneration is retained independently as
`local_pyscf_spin_integrals_sha256 = a9a85179...`; its maximum difference from
the canonical bytes is `2.22e-16`. It is no longer described as the
cross-repository canonical object.

Whole-tree search outside `.review` finds no obsolete `0.6975782469`,
`-1.1422`, `-1.8573`, `-2.2001`, or `-1.0471` value.

## Execution evidence

The exact PR #6 source was checked out detached and built with .NET SDK
`10.0.201`.

| Check | Result |
|---|---|
| `dotnet test test/Test.Encodings/Test.Encodings.fsproj -c Release` | **887/887 passed**, 0 warnings, 0 errors |
| `bash scripts/check-doc-samples.sh` in PR #6 | **19/19 documents executed and asserted** |
| `make verify-data` in a disposable book copy using the exact PR #6 DLL | **Passed** |
| `make pipeline-check` in that disposable copy | **Passed** |
| `make lab-check` in that disposable copy | **All 10 numbered labs passed** |
| Every `code/ch*.fsx` companion in that disposable copy | **All six passed** |
| Fixture file SHA-256 / git blob / semantic-map hash | **Exact values above** |
| Obsolete-value search outside review history | **No matches** |

The disposable verification changed only each copy's `#r "nuget: FockMap"` to
the exact built DLL path; repository companions retain their publication-facing
reference.

The public-package gate was also reproduced directly:

```text
code/ch03-spin-orbitals.fsx(44,5): error FS0039:
The value or constructor 'rawPhysicistToWeightedFactory' is not defined.
```

As of this pass, unversioned NuGet resolves to released FockMap `0.8.0`, while
PR #6 is open and no package containing `440540b` is published.

## Closed items

| ID | Verification result |
|---|---|
| CA-H001 | **Closed.** Exact provenance-pinned 4+32 fixture; symmetry, PySCF, HF, direct matrix, and generated-data checks pass. |
| CA-H002 | **Closed (correctness).** Named raw adapter, 15-term coefficient map, dense parity, and source API contract pass at PR #6. |
| CA-H003 | **Closed (correctness).** Full particle sectors, HF/FCI/correlation values, and all-six package spectra pass. |
| CA-H004 | **Closed (correctness).** Skeleton uses the named weighted adapter; no identity-proxy energy or output collision remains. |
| CA-H005 | **Closed (correctness).** Centralizer/subgroup distinction, CNOT phases, Heisenberg arithmetic, and all-sector spectrum preservation pass. |
| CA-H006 | **Closed (correctness).** Encode-before-taper order and explicit physical-sector rules are correct; default-sector molecular execution is absent. |
| CA-H007 | **Closed.** The H₂O result is a reproducible fixed-\(r_{\mathrm{OH}}\) PySCF FCI angular scan with no FockMap energy claim. |
| CA-H009 | **Closed.** Compact representation is separated from state preparation, estimation, and possible advantage. |
| ordering-convention | **Closed (correctness).** q0-leftmost signatures, occupation-integer rows, state labels, and Qiskit-style reversal agree. |
| CA-M001, CA-M002 | **Closed.** Correlation decomposition and generated first-order commutator bound are correct. |
| CA-M003 | **Closed by removal.** Unsupported molecule/resource tables remain absent. |
| CA-M004, CA-M005, CA-M006 | **Closed.** Logarithmic scope, encoding-output claims, and spectroscopy/climate causality are corrected. |
| CA-M007 | **Closed locally.** Unsupported hardware/resources and placeholder DOI are removed; external Zenodo is separate. |
| CA-M008, CA-M009 | **Closed.** Both appendices publish and Pauli basis/group language is correct. |
| CA-L001–CA-L004 | **Closed.** All terminology, arithmetic, model-scope, and script-inventory criteria pass. |

## Remaining items

1. **CA-H008 — Partial.** The phase/energy relation, \(t_0\), range,
   aliasing, overlap, confidence, controlled-simulation error, and 12-bit
   arithmetic are corrected. Imported QASM/Q#/JSON unitary parity and the exact
   `qpeResources`/`estimateShots` semantics were not part of PR #6 acceptance.
2. **CA-M010 — Partial.** The book correctly separates the reproduced
   star-only generic construction from supported max-three-child path-based
   trees. Broader research integration/merge remains.
3. **CA-M011 — Partial.** The five-basis QWC partition is mathematically valid;
   pinned `groupCommutingTerms` output/semantics remain unverified.
4. **Benchmark backlog — Open by design.** LiH/H₂O/N₂/FeMo molecule tables and
   fault-tolerant totals lack accepted artifacts and must remain removed.

## Operational and owner release gates

1. Merge PR #6, publish a FockMap package containing exact source commit
   `440540b`, replace every unversioned NuGet reference with that package
   version, and rerun the complete book and output baseline.
2. Correct the external Zenodo mixed-rights metadata so it does not grant MIT
   rights over the manuscript.
3. Preserve the absence of unsupported benchmark/resource tables unless a
   versioned generated artifact is accepted.

## New risks

- **Double adaptation:** Passing a weighted factory through the raw adapter, or
  a raw factory directly to `computeHamiltonianWith`, silently changes the
  Hamiltonian. Current code uses the two boundaries correctly; future edits
  must preserve that distinction.
- **Unversioned dependency drift:** A future NuGet release newer than the
  accepted source could change behavior. Pin the exact released version
  corresponding to PR #6.

## Current assessment

There are no remaining findings in the closed-item verification scope.
Correctness is sufficiently closed to begin the planned whole-book tone,
pacing, and clarity pass. This does not waive the partial items or make the
current unversioned-package tree a release candidate.

## Contract-change addendum — 2026-07-23 (later owner decision)

After the verification above, the owner explicitly selected a **breaking
raw-integral API**. This supersedes the addendum's description of PR #6 as the
final authoritative nonbreaking package contract.

Current interpretation:

- PR #6 commit `440540b54f093d7a9f259da7bf18b7df8da74270` remains an
  independently verified **audited base**. Its 887-test result, strict
  documentation harness, canonical fixture lock, cancellation-aware 15-term
  assembly, tapering fixes, ordering, and safety regressions remain valid
  evidence and required behavior.
- PR #6 is back in draft and is **not** the final package API. The new stack is
  expected to make existing Hamiltonian APIs consume raw integrals directly,
  preserve explicitly named weighted migration APIs, version and document the
  breaking change, and pass an independent audit.
- The canonical direct fixture, fermionic matrix, JW oracle, sector spectra,
  and all numerical anchors remain valid. No chemistry or direct-book
  correction is reopened.
- CA-H002, CA-H003, and CA-H004 package integration is reopened. CA-H005,
  CA-H006, ordering-convention, and CA-M005 remain closed on the audited base
  but require final-stack regression confirmation.
- Current book API code is frozen. Do not replace its PR #6 named raw/adapter
  calls with direct raw ingestion until an independently accepted final package
  SHA and migration contract are available.
- Package merge/release is no longer the only operational step; **final API
  contract acceptance must occur first**.

The whole-book tone and clarity pass may proceed because it does not depend on
the package call spelling. Final runtime validation and release-candidate status
remain blocked on the accepted package SHA.

Package-independent publication validation after the style pass is green:

- full PDF: 176 pages, 27 image placements;
- sample PDF: 55 pages, 9 image placements;
- EPUB: 29 content files and 27 PNG assets;
- strict MyST HTML: 27 pages, including both appendices and References; and
- Markdown fence and diff-whitespace checks: clean.

The first local output attempt exposed a missing `mmdc` executable while still
returning success. The build was rejected, Mermaid CLI was supplied in
session-local tooling, and all outputs were rebuilt with no omitted-diagram
markers. This does not alter the pending package-runtime gate.

## Final raw-API integration addendum — 2026-07-23

The independently audited breaking package contract is accepted at local
`encodings` commit
`8e562175e809dc282b5bf2ca21f1cb311e14d5fa`, whose parent is the PR #6 audited
base `440540b54f093d7a9f259da7bf18b7df8da74270`. The source worktree is clean.
The commit is local only at this checkpoint: no push, PR, tag, or NuGet package
yet exists.

### Accepted contract

- The primary builders `computeHamiltonian`, `computeHamiltonianWith`, their
  parallel/cached variants, `computeHamiltonianSkeletonFor`, and
  `applyCoefficients` consume raw single-bar physicist integrals:
  `(p,q,r,s) -> <pq|rs>`, with no caller-supplied half or index swap.
- The library assembles
  \(\tfrac12\langle pq\mid rs\rangle
  a_p^\dagger a_q^\dagger a_s a_r\).
- Explicit `...FromWeighted...` functions preserve the legacy contract:
  weighted key `(p,q,s,r)` and value
  `0.5 * <pq|rs>` are applied verbatim.
- `weightedToRawFactory` bridges legacy weighted data to primary raw builders;
  `antisymmetrizedToRawFactory` handles the double-bar quarter convention.
- FCIDUMP adapters now return raw physicist tensors; assembled FCIDUMP physics
  remains unchanged.
- Version `0.9.0`, a breaking-change changelog, and a migration guide document
  the boundary. The obsolete named-raw aliases remain only for transition.

### Book migration

All book H₂ factories already expose the canonical raw map. The migration
therefore removes the provisional PR #6 adapter layer:

- canonical JW paths call `computeHamiltonian rawFactory`;
- multi-encoding paths call `computeHamiltonianWith encoder rawFactory`;
- the dissociation skeleton calls `applyCoefficients skeleton rawFactory`
  directly; and
- no `computeHamiltonianFromPhysicist*`,
  `rawPhysicistToWeightedFactory`, hand-written half-and-swap adapter, or
  weighted factory remains in code, labs, README, or manuscript.

All executable and reader-facing package references are pinned to
`FockMap 0.9.0`. Until publication, this intentionally fails closed rather than
resolving released `0.8.0`.

### Verification evidence

| Check | Result |
|---|---|
| Exact accepted source | `8e562175e809dc282b5bf2ca21f1cb311e14d5fa` |
| Package tests | **897/897 passed** |
| Package executable-doc harness | **19/19 passed**, exact assertions retained |
| `make verify-data` against exact DLL | **Passed** |
| `make pipeline-check` against exact DLL | **Passed** |
| `make lab-check` against exact DLL | **All 10 numbered labs passed** |
| Every `code/ch*.fsx` companion against exact DLL | **All six passed** |
| Obsolete API warnings/search | **None** |
| Canonical fixture file SHA-256 | `6539afb30a1c03ec89202a2960a06c6580a91afaebf13a6cadbcfd32c2d71812` unchanged |
| Canonical semantic-map SHA-256 | `d5d8e2f7c83b1d1322f10217198d125b7c7cae1f99fa09a6e1e232ee38dd098c` unchanged |
| Full PDF | **176 pages, 27 image placements** |
| Sample PDF | **55 pages, 9 image placements** |
| EPUB | **29 content files, 27 PNG assets** |
| Strict MyST HTML | **27 pages, 18 linked DOIs** |
| Markdown fences / diff whitespace | **Clean** |

The public-package gate currently fails as intended:

```text
error NU1102: Unable to find package FockMap with version (>= 0.9.0)
```

### Item status after final integration

- **CA-H002, CA-H003, CA-H004:** package integration now closed at exact source
  commit. Publication rerun remains operational.
- **CA-H005, CA-H006, ordering-convention, CA-M005:** required final-stack
  regressions pass.
- **CA-H008, CA-M010, CA-M011:** remain partial exactly as recorded above; the
  raw-Hamiltonian migration does not claim to close their separate API/research
  gates.
- **Zenodo mixed rights and unsupported benchmark backlog:** unchanged.

### Remaining release operations

1. Push `8e562175`, open and merge the reviewed package change.
2. Publish FockMap `0.9.0` from that accepted lineage.
3. Rerun the same book baseline against the public NuGet package and verify its
   package/source provenance.
4. Correct the external Zenodo rights metadata.

The book and local package integration are correctness-ready. The repository is
not a release candidate until the public-package and Zenodo operations close.

## Provisional-status correction — 2026-07-23

The coordination authority subsequently clarified that the independent package
auditor has **not** yet returned its exact-SHA verdict in the authoritative
thread. Therefore the preceding “Final raw-API integration addendum” records
valid **local implementation evidence**, but its package-acceptance and
item-closure wording is provisional.

Authoritative current status:

- `8e562175e809dc282b5bf2ca21f1cb311e14d5fa` is **implementation verified
  locally; external package audit pending**.
- The migrated book tree may remain as an uncommitted provisional integration,
  but it is frozen. Do not push, open a PR, commit, or mark package audit closure.
- The 897 package tests, strict documentation harness, full book runtime
  baseline, canonical fixture identity, and publication builds remain useful
  local evidence; they do not substitute for the independent exact-SHA verdict.
- CA-H002, CA-H003, and CA-H004 direct/book corrections remain closed, while
  candidate package integration is pending external audit.
- CA-H005, CA-H006, ordering-convention, and CA-M005 remain closed on the
  previously audited base; candidate-stack regression evidence is local pending
  the same verdict.
- An explicit **BREAKING-RELEASE READY** report closes the candidate gate. If
  the auditor reports a blocker, both package and book integration must follow
  the accepted successor SHA and rerun the same baseline.

Public-package publication, provenance rerun, and Zenodo mixed-rights correction
remain later operational gates. No release-candidate claim is permitted at this
stage.

## Independent package audit result — 2026-07-23

The independent auditor has now completed the `8e562175` runtime/package pass.
The raw-breaking implementation semantics pass in source inspection and probes,
and the existing suite is green. The exact SHA is still not
BREAKING-RELEASE READY because of three blockers:

1. remove the unreleased publication date from `CITATION.cff`; and
2. reconcile the test register to **25 test methods / 42 cases / suite 897**;
   and
3. add convention-sensitive Optimization regressions: raw two-body exact map,
   `weightedToRawFactory` migration parity, and a direct-weighted misuse
   negative control across the exposed routing.

This narrows, but does not remove, the provisional gate:

- the canonical fixture/oracle and current raw-primary book migration remain
  valid and frozen;
- no book code change is indicated by the audit;
- `8e562175` must not be pushed, opened, committed as package closure, or
  described as release-ready; and
- a tests-and-metadata successor SHA must receive the explicit
  **BREAKING-RELEASE READY** verdict.

After that verdict, rerun the exact package and book baselines against the
successor. If its runtime outputs are unchanged, package integration may close
at that SHA; NuGet publication/provenance and Zenodo rights remain subsequent
operational gates.

## Independent audit addendum — Optimization test coverage

The auditor identified a third release blocker. Optimization's raw semantics
are correct in source inspection and probes, but the repository tests are
currently one-body or convention-insensitive and therefore do not lock the
breaking two-body contract.

The final successor must add:

1. a raw two-body exact-map test through Optimization;
2. `weightedToRawFactory` migration-parity coverage through Optimization; and
3. a direct-weighted negative control proving that preweighted data fed to the
   raw Optimization path does not silently pass.

The successor has two metadata repairs and one test-coverage repair. This
remains package-test work only; the frozen book migration and canonical
fixture/oracle do not change.

## BREAKING-RELEASE READY successor verification — 2026-07-23

The package session delivered explicit **BREAKING-RELEASE READY** status for
successor `3e62bd71b113d78a6f5933858336e06af1e3d9c7`, whose parent is
`8e562175e809dc282b5bf2ca21f1cb311e14d5fa`.

The successor diff is exactly:

- `.project/test-register.md`;
- `CITATION.cff`; and
- `test/Test.Encodings/Optimization.fs`.

No `src/**/*.fs` file changed, so the raw-breaking runtime accepted at
`8e562175` is unchanged. The successor:

1. removes the unreleased CFF date;
2. reconciles the register to 25 Hamiltonian test methods / 42 cases and
   900 total tests (897 base + 3 new Optimization cases); and
3. locks Optimization's exposed raw routing with the required raw two-body
   exact-map, `weightedToRawFactory` migration-parity, and direct-weighted
   negative-control tests.

### Independent successor results

| Check | Result |
|---|---|
| Successor scope | **Tests and metadata only; no library source change** |
| Package tests | **900/900 passed**, 0 skipped |
| Documentation harness selftest | **Passed** |
| Executable documentation | **19/19 passed** with exact assertions |
| `dotnet fsdocs build --clean --strict` | **Passed** |
| Package worktree | **Clean** |
| CFF unreleased date | **Absent** |
| Test register | **900 total; 25 methods / 42 cases; Optimization row present** |

### Frozen-book verification against successor DLL

| Check | Result |
|---|---|
| `make verify-data` | **Passed** |
| `make pipeline-check` | **Passed** |
| `make lab-check` | **All 10 numbered labs passed** |
| Every `code/ch*.fsx` companion | **All six passed** |
| Obsolete FockMap API warnings/search | **None** |
| Canonical fixture file SHA-256 | `6539afb30a1c03ec89202a2960a06c6580a91afaebf13a6cadbcfd32c2d71812` unchanged and byte-identical to package |
| Canonical git blob | `e0477e70c0dfd35b865000bb23b7b31882b062d3` unchanged |
| Full PDF | **176 pages, 27 image placements** |
| Sample PDF | **55 pages, 9 image placements** |
| EPUB | **29 content files, 27 PNG assets** |
| Strict MyST HTML | **27 pages, 18 DOI links** |
| Markdown fences / diff whitespace | **Clean** |

The first parallel book-runtime copy overlapped Mermaid image regeneration and
made `rsync` report disappearing generated files. This was a validation-harness
race, not a source failure. Runtime verification was rerun sequentially with
generated Mermaid images excluded from the disposable copy and passed fully.

### Final status

- **CA-H002, CA-H003, CA-H004:** closed at accepted successor
  `3e62bd71`.
- **CA-H005, CA-H006, ordering-convention, CA-M005:** final successor
  regressions pass.
- **CA-H008, CA-M010, CA-M011:** remain partial under their separate,
  previously recorded gates.
- **Unsupported multi-molecule/resource claims:** remain removed.
- **Package publication:** successor is local and unpushed; FockMap `0.9.0` is
  not yet available from NuGet. The pinned public-package run therefore fails
  closed with `NU1102`, as intended.
- **Zenodo mixed rights:** remains an owner/release blocker.

All three successor blockers are resolved. Package/book integration is
correctness-accepted at `3e62bd71`. Remaining work is external release
operation and the explicitly partial items, not another local Hamiltonian
repair.

## Designated-auditor status correction — 2026-07-23

The coordination authority clarified that the package session's
**BREAKING-RELEASE READY** label is not the designated independent package
auditor's one-delta verdict. Therefore the preceding successor addendum records
complete and valid **book-side verification**, but its final package-acceptance
wording is provisional.

Authoritative current wording:

- successor `3e62bd71b113d78a6f5933858336e06af1e3d9c7` is
  **book-side verified; package auditor pending**;
- the tests-and-metadata-only scope, 900-test result, documentation gates,
  frozen-book runtime, fixture identity, and publication outputs remain valid
  evidence;
- the uncommitted book integration remains frozen;
- do not push, open a PR, commit package closure, or promote release status
  solely from book-side verification; and
- if the designated auditor reports a blocker, follow its accepted successor
  SHA and rerun the same book baseline.

Direct book corrections remain closed. Final package integration status awaits
the designated auditor's explicit one-delta verdict; public NuGet provenance and
Zenodo rights remain later gates.

## Designated package audit verdict — runtime accepted, release workflow pending

The designated independent package auditor has now accepted all raw API code
and tests at `3e62bd71b113d78a6f5933858336e06af1e3d9c7`. This supersedes the
preceding “package auditor pending” status for runtime/package semantics.

One release-engineering blocker remains. Current tooling:

1. cannot explicitly release the staged/current `0.9.0` version;
2. cannot robustly insert `date-released` when that CFF field is absent; and
3. uses nonportable `grep -P`.

The final package-repository successor must add an explicit current/staged
`0.9.0` release mode, robust CFF date insertion, portable matching, and tests.

Current authoritative status is therefore:

- **runtime/package accepted; release workflow successor pending**;
- no further raw-Hamiltonian or book code change is indicated;
- the uncommitted book integration remains frozen until the workflow successor
  passes;
- do not push/open the package PR until that release-engineering gate closes;
  and
- after closure, human approval, NuGet publication/provenance, and Zenodo
  mixed-rights correction remain.

CA-H002, CA-H003, CA-H004, CA-H005, CA-H006, ordering-convention, and CA-M005
are closed for runtime/package correctness at `3e62bd71`. CA-H008, CA-M010, and
CA-M011 remain partial under their separate gates.

## Release-workflow successor — book-side verification

Release-engineering successor
`9e1afe267efefc4056f726ea98dadfed8af0214e` is a tooling-only child of
`3e62bd71b113d78a6f5933858336e06af1e3d9c7`. Its five-file diff changes release
scripts, the dispatch workflow, and release guidance/tests only. No runtime F#,
F# test, Hamiltonian fixture, or cookbook file changed.

Book-side checks:

| Check | Result |
|---|---|
| `scripts/test-release.sh` | **39 passed, 0 failed**; worktree unchanged |
| `scripts/release.sh --dry-run current` | **Would release v0.9.0**; no bump or mutation |
| Current/staged mode | **0.9.0 > latest v0.8.0**, CFF/CHANGELOG alignment validated |
| Absent/present CFF date | **Insert/replace, duplicate/malformed rejection, YAML parse pass** |
| Changelog finalization | **Passes, idempotent, missing heading rejected** |
| Executable `grep -P` scan | **None** in shell scripts or workflow |
| ShellCheck | **No error-severity findings** |
| Workflow YAML/options | **Parses; `auto,current,patch,minor,major`** |
| Package regression | **900/900 tests; documentation harness passes; clean tree** |
| Frozen-book runtime | **verify-data, pipeline-check, all 10 labs, all six companions pass** |

The Linux-only dispatch workflow still uses `sed -i` in the branch that changes
the fsproj version. The staged/current `0.9.0` path skips that branch; local
`release.sh` and shared helpers use temp-file-plus-move and are macOS/bash-3.2
portable. This does not block the staged release book-side, but it narrows the
handoff's broad portability claim and is explicitly left for the designated
release-workflow auditor to assess.

Authoritative status:

- **runtime/package accepted at `3e62bd71`;**
- **release-workflow successor `9e1afe2` book-side verified; designated
  release-workflow auditor pending;**
- book integration remains frozen and uncommitted;
- no push/PR or release closure may occur from book-side verification alone;
  and
- after designated acceptance, human approval, NuGet publication/provenance,
  and Zenodo rights remain.

## Final book-branch verification and selective clarity pass

The designated audit subsequently declared exact release-workflow SHA
`9e1afe267efefc4056f726ea98dadfed8af0214e` BREAKING-RELEASE READY.
Package head then advanced tooling-only to portability follow-up
`660c4839223037bc69909bb8fcb7d3be0f226b20`; no F#, cookbook, Hamiltonian, or
book API changed. The follow-up is book-side verified and awaits its designated
one-delta audit. This does not reopen the accepted raw-primary book migration.

### Selective whole-book clarity pass

A final specialist pass targeted only repeated cross-chapter bridge templates,
not local taste:

- Chapter 14 now states how Trotterization resolves the just-stated
  non-commuting-exponential problem instead of announcing that it “enters.”
- Chapter 20 now contrasts the algorithmic route directly instead of repeating
  “This is where ... enters.”
- Chapter 18 replaces the duplicate “Let's take stock” recap opener.
- Chapter 23 removes a redundant “In other words” before its closing QEC
  statement.

No technical meaning, notation, equation, citation, table, code fence, numeric
anchor, raw/weighted boundary, sector qualification, or review artifact was
changed by those four prose edits.

### Exact final verification

Package head during the final book verification:
`660c4839223037bc69909bb8fcb7d3be0f226b20`.

| Check | Result |
|---|---|
| Release successor scope | **Tooling only; no runtime/test-F#/cookbook change from `9e1afe2`** |
| Release harness | **42/42 passed; worktree unchanged** |
| `release.sh --dry-run current` | **Would release v0.9.0; no mutation** |
| Portability scan | **No executable `grep -P` or `sed -i`** |
| Workflow / shell validation | **YAML choices, shell syntax, ShellCheck error severity pass** |
| Package suite | **900/900 passed** |
| Semantic documentation harness | **Selftest + 19/19 executable documents passed** |
| Strict fsdocs | **Passed** |
| `make verify-data` against exact DLL | **Passed** |
| `make pipeline-check` against exact DLL | **Passed** |
| `make lab-check` against exact DLL | **All 10 numbered labs passed** |
| Every `code/ch*.fsx` companion | **All six passed; no obsolete warnings** |
| Canonical fixture SHA-256 | `6539afb30a1c03ec89202a2960a06c6580a91afaebf13a6cadbcfd32c2d71812` |
| Canonical git blob | `e0477e70c0dfd35b865000bb23b7b31882b062d3` |
| Canonical semantic-map SHA-256 | `d5d8e2f7c83b1d1322f10217198d125b7c7cae1f99fa09a6e1e232ee38dd098c` |
| Generated oracle SHA-256 | `ab6a221add84cb6e862a6e34b696e1a1dc28d0cd41c1c09873f7248d43f0b439` |
| Full PDF | **176 pages, 27 image placements** |
| Sample PDF | **55 pages, 9 image placements** |
| EPUB | **29 content files, 27 PNG assets** |
| Strict MyST HTML | **27 pages, 18 DOI links** |
| Mermaid output | **25 rendered diagrams; no omission markers** |
| FockMap references | **12 executable references pinned to 0.9.0; no obsolete aliases** |
| Obsolete H₂ values | **Absent outside labelled review history** |
| Markdown fences / diff whitespace | **Clean** |

### Current release assessment

The book branch is verified and ready for commit/push/PR. Runtime/package
correctness is accepted; the final package release pin follows the designated
one-delta verdict for tooling-only `660c483`. FockMap `0.9.0` is **not yet
published**, so the public-package run remains fail-closed. Package
push/PR/merge, NuGet publication/provenance, Zenodo mixed-rights correction, and
the separate CA-H008/CA-M010/CA-M011 gates remain outside this book-branch
closure.

## Public FockMap 0.9.0 release verification — 2026-07-24

The package rollout gate is closed.

### Public provenance

| Artifact | Verified value |
|---|---|
| GitHub release | `v0.9.0`, published 2026-07-24 |
| Tag target / main | `96320a56786393269fd681c67c66df88058a8b8f` |
| NuGet package | `FockMap 0.9.0` present in the public v3 flat-container index |
| Public NuGet nupkg SHA-256 | `2911c0b5df790a8f7ed4b1756ca7f0af684a75f4a41b404a5ccefb713de6adf2` |
| Nuspec repository commit | `96320a56786393269fd681c67c66df88058a8b8f` |
| Published `Encodings.dll` SHA-256 | `0ba8ae967ea65d4945a336c1217f8939e41feb63b6f04af617b66537717d25c3` |

The public package exposes the accepted raw-primary contract and preserves the
named weighted migration APIs. Its canonical fixture and 15-term H₂ behavior
remain the accepted lineage.

### Public-package book verification

The repository was executed normally through its pinned
`#r "nuget: FockMap, 0.9.0"` references, with no local DLL substitution:

- `make verify-data` — passed;
- `make pipeline-check` — passed;
- `make lab-check` — all 10 numbered labs passed;
- every `code/ch*.fsx` companion — all six passed;
- obsolete API warnings — none; and
- worktree/deterministic-data drift — none.

The selective clarity pass was already complete and introduced no technical
change. Package-independent publication outputs remain verified:

- full PDF: 176 pages, 27 image placements;
- sample PDF: 55 pages, 9 image placements;
- EPUB: 29 content files, 27 PNG assets;
- strict MyST HTML: 27 pages, 18 DOI links; and
- canonical fixture/oracle hashes and Mermaid output remain unchanged.

### Final package-gate status

- CA-H002, CA-H003, CA-H004, CA-H005, CA-H006, ordering-convention, and
  CA-M005 are closed against the public released package.
- FockMap `0.9.0` is published and provenance-verified.
- CA-H008, CA-M010, and CA-M011 remain partial under their separate gates.
- Zenodo mixed-rights metadata remains an explicit owner/release blocker; this
  addendum does not claim it is fixed.

The book PR may now be marked ready and evaluated by its own CI. No remaining
package publication blocker applies to the book branch.
