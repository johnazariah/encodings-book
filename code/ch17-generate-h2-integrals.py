# ══════════════════════════════════════════════════════════════
# Chapter 17 Companion: Generate H₂ integrals at multiple
# bond lengths for the dissociation curve.
# ══════════════════════════════════════════════════════════════
# Run with: python book/code/ch17-generate-h2-integrals.py
# Prereq:   pip install pyscf
# Output:   book/code/h2_dissociation_integrals.json
#
# This script computes the STO-3G molecular integrals for H₂
# at a range of bond lengths, in the spin-orbital physicist's
# convention that FockMap expects. The output is a JSON file
# that the F# companion script (ch17-pipeline.fsx) reads.

import json

try:
    from pyscf import ao2mo, gto, scf

    HAS_PYSCF = True
except ImportError:
    HAS_PYSCF = False

# ── Bond lengths to scan (in Ångströms) ──
BOND_LENGTHS = [
    0.40,
    0.50,
    0.60,
    0.70,
    0.74,
    0.80,
    0.90,
    1.00,
    1.20,
    1.40,
    1.60,
    1.80,
    2.00,
    2.50,
    3.00,
    4.00,
]


def compute_integrals(R_angstrom):
    """
    Compute STO-3G integrals for H₂ at bond length R (Å).
    Returns (Vnn, one_body_dict, two_body_dict) in spin-orbital
    physicist's convention with interleaved spin indexing.
    """
    mol = gto.M(
        atom=f"H 0 0 0; H 0 0 {R_angstrom}",
        basis="sto-3g",
        symmetry=False,
        verbose=0,
    )
    mf = scf.RHF(mol)
    mf.kernel()

    Vnn = mol.energy_nuc()
    nao = mol.nao  # 2 for STO-3G H₂

    # One-body integrals in MO basis (spatial)
    h1_spatial = mf.mo_coeff.T @ mf.get_hcore() @ mf.mo_coeff

    # Two-body integrals in MO basis (spatial, chemist's notation)
    eri_spatial = ao2mo.full(mol, mf.mo_coeff)
    eri_spatial = ao2mo.restore(1, eri_spatial, nao)  # (nao,nao,nao,nao)

    # ── Convert to spin-orbital with interleaved indexing ──
    # Spatial orbital p → spin-orbitals 2p (α), 2p+1 (β)
    nso = 2 * nao  # 4 spin-orbitals

    integrals = {}

    # One-body: h(p_σ, q_σ) = h_spatial(p, q) if same spin, 0 otherwise
    for p in range(nao):
        for q in range(nao):
            val = float(h1_spatial[p, q])
            if abs(val) > 1e-12:
                # α-α
                integrals[f"{2 * p},{2 * q}"] = val
                # β-β
                integrals[f"{2 * p + 1},{2 * q + 1}"] = val

    # Two-body: physicist's convention ⟨pq|rs⟩ = (pr|qs)_chemist
    # with spin: nonzero only if σ_p=σ_r AND σ_q=σ_s
    for p in range(nao):
        for q in range(nao):
            for r in range(nao):
                for s in range(nao):
                    val = float(eri_spatial[p, r, q, s])  # chemist→physicist
                    if abs(val) > 1e-12:
                        # αα-αα
                        integrals[f"{2 * p},{2 * q},{2 * r},{2 * s}"] = val
                        # ββ-ββ
                        integrals[
                            f"{2 * p + 1},{2 * q + 1},{2 * r + 1},{2 * s + 1}"
                        ] = val
                        # αβ-αβ
                        integrals[f"{2 * p},{2 * q + 1},{2 * r},{2 * s + 1}"] = val
                        # βα-βα
                        integrals[f"{2 * p + 1},{2 * q},{2 * r + 1},{2 * s}"] = val

    return Vnn, integrals


def main():
    if not HAS_PYSCF:
        print("PySCF not installed. Install with: pip install pyscf")
        print("Then re-run this script to generate the integral files.")
        print()
        print("Alternatively, the F# companion script (ch17-pipeline.fsx)")
        print("includes hardcoded integrals for the equilibrium geometry")
        print("and will still demonstrate the full pipeline.")
        return

    results = {}
    print("Generating H₂/STO-3G integrals for the dissociation curve...")
    print()
    print(f"  {'R (Å)':>8}  {'Vnn (Ha)':>12}  {'E_HF (Ha)':>12}  {'#integrals':>10}")
    print(f"  {'─' * 8}  {'─' * 12}  {'─' * 12}  {'─' * 10}")

    for R in BOND_LENGTHS:
        mol = gto.M(
            atom=f"H 0 0 0; H 0 0 {R}",
            basis="sto-3g",
            symmetry=False,
            verbose=0,
        )
        mf = scf.RHF(mol)
        ehf = mf.kernel()
        Vnn, integrals = compute_integrals(R)

        results[f"{R:.2f}"] = {
            "bond_length_angstrom": R,
            "Vnn": Vnn,
            "E_HF": ehf,
            "integrals": integrals,
        }
        print(f"  {R:8.2f}  {Vnn:12.6f}  {ehf:12.6f}  {len(integrals):10d}")

    # Write to JSON
    import os

    out_path = os.path.join(os.path.dirname(__file__), "h2_dissociation_integrals.json")
    with open(out_path, "w") as f:
        json.dump(results, f, indent=2)

    print()
    print(f"Written to: {out_path}")
    print(f"Bond lengths: {len(BOND_LENGTHS)}")
    print("Use with:     dotnet fsi book/code/ch17-pipeline.fsx")


if __name__ == "__main__":
    main()
