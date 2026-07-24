#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

canonical_files=(
  code/h2_dissociation.csv
  code/h2_dissociation_integrals.json
  code/h2_0.74_fixture.json
  code/h2_0.74_oracle.json
  code/h2o_bond_angle_coarse.csv
  code/h2o_bond_angle_fine.csv
  code/h2o_bond_angle_metadata.json
  manuscript/figures/h2_dissociation.png
  manuscript/figures/h2o_bond_angle.png
)

hashes() {
  python3 - "$@" <<'PY'
import hashlib
import sys
from pathlib import Path

for raw_path in sys.argv[1:]:
    path = Path(raw_path)
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    print(f"{path}\t{digest}")
PY
}

before="$(mktemp)"
after="$(mktemp)"
log="$(mktemp)"
trap 'rm -f "$before" "$after" "$log"' EXIT

hashes "${canonical_files[@]}" > "$before"

set +e
dotnet fsi code/ch18-pipeline.fsx > "$log" 2>&1
pipeline_status=$?
set -e

hashes "${canonical_files[@]}" > "$after"

if ! cmp -s "$before" "$after"; then
  echo "ERROR: Chapter 18 modified canonical chemistry data." >&2
  diff -u "$before" "$after" >&2 || true
  exit 1
fi

if [[ -e code/h2_circuit_costs.csv ]]; then
  echo "ERROR: Chapter 18 wrote derived output into code/." >&2
  exit 1
fi

if [[ $pipeline_status -ne 0 ]]; then
  cat "$log" >&2
  echo "Canonical data remained unchanged, but the FockMap pipeline is blocked." >&2
  exit "$pipeline_status"
fi

test -f _build/data/h2_circuit_costs.csv

python3 - <<'PY'
import csv
from pathlib import Path

path = Path("_build/data/h2_circuit_costs.csv")
rows = list(csv.DictReader(path.open()))
if len(rows) != 18:
    raise SystemExit(f"Unexpected Chapter 18 row count: {len(rows)}")
if any(int(row["terms"]) != 15 for row in rows):
    raise SystemExit("Chapter 18 did not prune to 15 nonzero terms")
if any(int(row["cnots_per_step"]) > 84 for row in rows):
    raise SystemExit("Chapter 18 cost exceeds the four-qubit logical maximum")
equilibrium = next(row for row in rows if row["R_angstrom"] == "0.74")
if int(equilibrium["cnots_per_step"]) != 36:
    raise SystemExit("Chapter 18 equilibrium JW CNOT count is not 36")
PY

echo "Chapter 18 output is isolated and canonical data are unchanged."
