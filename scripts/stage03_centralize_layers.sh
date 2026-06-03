#!/usr/bin/env bash
# Stage 03 (QoL) — centralize layer lookups.
# Creates a cached `Layers` class and replaces every scattered
# LayerMask.NameToLayer(Utilities.*Layer) with Layers.<Name>.
# Rename a layer once in Layers.cs instead of hunting ~20 call sites.
# Run from your Unity project ROOT.
set -euo pipefail

[ -d Assets ] || { echo "ERROR: run this from your Unity project root (the folder containing Assets/)."; exit 1; }
echo "== Stage 03: centralize layer lookups =="

if command -v git >/dev/null 2>&1 && git rev-parse --git-dir >/dev/null 2>&1; then
  if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "WARNING: uncommitted changes present. Commit first so this is easy to undo."
    read -r -p "Continue anyway? [y/N] " ans; [ "${ans:-N}" = "y" ] || { echo "Aborted."; exit 1; }
  fi
fi

find_one() { find Assets -type f -name "$1" | head -n1; }

# 1) Replace the scattered calls (literal string replace; safe in comments too).
python3 - <<'PY'
import glob, os
# longer keys first so no shorter key matches inside a longer one
MAP = [
    ("LayerMask.NameToLayer(Utilities.SlidableObstructionLayer)", "Layers.SlidableObstruction"),
    ("LayerMask.NameToLayer(Utilities.TargetOverlapLayer)",       "Layers.TargetOverlap"),
    ("LayerMask.NameToLayer(Utilities.SocketUnusedLayer)",        "Layers.SocketUnused"),
    ("LayerMask.NameToLayer(Utilities.SocketUsedLayer)",          "Layers.SocketUsed"),
    ("LayerMask.NameToLayer(Utilities.InteractableLayer)",        "Layers.Interactable"),
    ("LayerMask.NameToLayer(Utilities.InteractingLayer)",         "Layers.Interacting"),
    ("LayerMask.NameToLayer(Utilities.ObstructionLayer)",         "Layers.Obstruction"),
    ("LayerMask.NameToLayer(Utilities.KeyPortLayer)",             "Layers.KeyPort"),
    ("LayerMask.NameToLayer(Utilities.PlayerLayer)",              "Layers.Player"),
    ("LayerMask.NameToLayer(Utilities.LockedLayer)",              "Layers.Locked"),
    ("LayerMask.NameToLayer(Utilities.DepthLayer)",               "Layers.Depth"),
    ("LayerMask.NameToLayer(Utilities.NoneLayer)",                "Layers.None"),
]
changed = 0
for path in glob.glob("Assets/Scripts/**/*.cs", recursive=True):
    if os.path.basename(path) == "Layers.cs":      # never rewrite the generated file
        continue
    src = open(path, encoding="utf-8").read()
    out = src
    for a, b in MAP:
        out = out.replace(a, b)
    if out != src:
        open(path + ".bak", "w", encoding="utf-8").write(src)
        open(path, "w", encoding="utf-8").write(out)
        changed += 1
        print("  edited:", path)
print("  files changed:", changed)
PY

# 2) Create the cached Layers class next to Utilities.cs.
UTIL="$(find_one Utilities.cs)"; [ -n "$UTIL" ] || { echo "ERROR: Utilities.cs not found"; exit 1; }
UTILDIR="$(dirname "$UTIL")"
cat > "$UTILDIR/Layers.cs" <<'CS'
using UnityEngine;

/// Cached layer indices. Replaces scattered LayerMask.NameToLayer(Utilities.*Layer) calls.
/// Add a layer here once; every call site reads from this.
public static class Layers
{
    public static readonly int None                = LayerMask.NameToLayer(Utilities.NoneLayer);
    public static readonly int Interactable         = LayerMask.NameToLayer(Utilities.InteractableLayer);
    public static readonly int Interacting          = LayerMask.NameToLayer(Utilities.InteractingLayer);
    public static readonly int SlidableObstruction  = LayerMask.NameToLayer(Utilities.SlidableObstructionLayer);
    public static readonly int Player               = LayerMask.NameToLayer(Utilities.PlayerLayer);
    public static readonly int KeyPort              = LayerMask.NameToLayer(Utilities.KeyPortLayer);
    public static readonly int TargetOverlap        = LayerMask.NameToLayer(Utilities.TargetOverlapLayer);
    public static readonly int SocketUsed           = LayerMask.NameToLayer(Utilities.SocketUsedLayer);
    public static readonly int SocketUnused         = LayerMask.NameToLayer(Utilities.SocketUnusedLayer);
    public static readonly int Locked               = LayerMask.NameToLayer(Utilities.LockedLayer);
    public static readonly int Obstruction          = LayerMask.NameToLayer(Utilities.ObstructionLayer);
    public static readonly int Depth                = LayerMask.NameToLayer(Utilities.DepthLayer);
}
CS
echo "  created: $UTILDIR/Layers.cs"

echo
echo "== Verify =="
remaining="$(grep -rl 'LayerMask.NameToLayer(Utilities\.' Assets/Scripts --include='*.cs' | grep -v '/Layers.cs$' || true)"
if [ -z "$remaining" ]; then
  echo "  OK: no scattered LayerMask.NameToLayer(Utilities.*) calls remain."
else
  echo "  Note: these files still reference Utilities layer lookups (check manually):"
  echo "$remaining" | sed 's/^/    /'
fi
echo "  (Utilities.PutObjectOnLayer keeps its generic string-based NameToLayer — that's intended.)"
echo
echo "Stage 03 done. Open Unity, confirm the Console compiles. Delete the .bak files once happy."
