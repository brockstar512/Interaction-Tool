#!/usr/bin/env bash
# Stage 00 — delete dead files (IAnimationState, ICommand, AnimationStateAsync, Old/ folder)
# Keeps AnimationState.cs. Run from your Unity project ROOT (the folder containing Assets/).
set -euo pipefail

[ -d Assets ] || { echo "ERROR: run this from your Unity project root (the folder containing Assets/)."; exit 1; }
echo "== Stage 00: delete dead files =="

# --- git safety ---
if command -v git >/dev/null 2>&1 && git rev-parse --git-dir >/dev/null 2>&1; then
  if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "WARNING: you have uncommitted changes. Commit first so this is easy to undo."
    read -r -p "Continue anyway? [y/N] " ans; [ "${ans:-N}" = "y" ] || { echo "Aborted."; exit 1; }
  fi
fi

del_file() {  # delete an exact-named .cs file (and its .meta) anywhere under Assets/
  local name="$1" found=0 f
  while IFS= read -r f; do
    found=1; echo "  deleting $f"; rm -f "$f" "$f.meta"
  done < <(find Assets -type f -name "$name")
  [ "$found" -eq 1 ] || echo "  (already gone: $name)"
}

del_file "IAnimationState.cs"
del_file "ICommand.cs"
del_file "AnimationStateAsync.cs"

# Old movement folder (+ its .meta)
while IFS= read -r d; do
  echo "  deleting folder $d"; rm -rf "$d" "${d}.meta"
done < <(find Assets -type d -name "Old" -path "*PlayerMovementManager*")

echo
echo "== Verify =="
fail=0
for n in IAnimationState.cs ICommand.cs AnimationStateAsync.cs; do
  if find Assets -type f -name "$n" | grep -q .; then echo "  STILL PRESENT: $n"; fail=1; else echo "  gone: $n"; fi
done
if find Assets -type f -name "AnimationState.cs" | grep -q .; then
  echo "  kept (correct): AnimationState.cs"
else
  echo "  WARNING: AnimationState.cs is missing — it should be KEPT."; fail=1
fi

echo
if [ "$fail" -eq 0 ]; then
  echo "Stage 00 OK. Open Unity and confirm the Console compiles with no errors."
else
  echo "Stage 00 had issues (see above)."; exit 1
fi
