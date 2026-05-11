#!/usr/bin/env bash
set -e
cd "$(dirname "$0")/.."

# Build HTML site using Jupyter Book / MyST
# Usage: ./scripts/build-site.sh
# Output: _build/html/

# Clean generated site outputs so stale hashed exports do not survive rebuilds.
rm -rf _build/html _build/site

# Fix npm cache permissions if needed (common after sudo installs)
if [ -d "$HOME/.npm/_cacache" ]; then
    find "$HOME/.npm/_cacache" -user root -exec chown "$(whoami)" {} + 2>/dev/null || true
fi

HOST="${HOST:-127.0.0.1}" python3 -m jupyter_book build --html --force
