#!/usr/bin/env bash
# Stage 01 — harden item Action methods (BellItem, GrapplingHookItem)
# Wraps each async void Action in try/catch so failures log + recover instead of vanishing.
# Run from your Unity project ROOT.
set -euo pipefail

[ -d Assets ] || { echo "ERROR: run this from your Unity project root (the folder containing Assets/)."; exit 1; }
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PY="$SCRIPT_DIR/patch_method.py"
[ -f "$PY" ] || { echo "ERROR: patch_method.py must sit next to this script."; exit 1; }
echo "== Stage 01: harden item Action methods =="

if command -v git >/dev/null 2>&1 && git rev-parse --git-dir >/dev/null 2>&1; then
  if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "WARNING: uncommitted changes present. Commit first so this is easy to undo."
    read -r -p "Continue anyway? [y/N] " ans; [ "${ans:-N}" = "y" ] || { echo "Aborted."; exit 1; }
  fi
fi

find_one() { find Assets -type f -name "$1" | head -n1; }

# ---- BellItem ----
F="$(find_one BellItem.cs)"; [ -n "$F" ] || { echo "ERROR: BellItem.cs not found"; exit 1; }
cp "$F" "$F.bak"; echo "backup: $F.bak"
"$PY" "$F" 'async void Action(PlayerStateMachineManager stateManager)' <<'NEW'
        async void Action(PlayerStateMachineManager stateManager)
        {
            if (currentBellSound != null)
            {
                PutAway();
                return;
            }

            try
            {
                currentBellSound = Instantiate(bellSoundAreaPrefab, stateManager.transform.position, Quaternion.identity).Init();
                await _animationBell.Play(stateManager);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BellItem.Action failed: {ex}");
            }
            finally
            {
                currentBellSound?.Stop();
                currentBellSound = null;
                PutAway();
            }
        }
NEW

# ---- GrapplingHookItem ----
F="$(find_one GrapplingHookItem.cs)"; [ -n "$F" ] || { echo "ERROR: GrapplingHookItem.cs not found"; exit 1; }
cp "$F" "$F.bak"; echo "backup: $F.bak"
"$PY" "$F" 'async void Action(PlayerStateMachineManager stateManager)' <<'NEW'
        async void Action(PlayerStateMachineManager stateManager)
        {
            try
            {
                await _animationGrapplingHookSetUp.Play(stateManager);
                _originPoint = stateManager.GetComponentInChildren<OriginPoint>().transform.position;
                _currentLocation = _originPoint;
                _maxLocation = (stateManager.currentState.LookDirection * MaxDistance) + (Vector2)_originPoint;
                _projectile = Instantiate(projectilePrefab, _originPoint, Quaternion.identity).Init(_originPoint, HitSomething, stateManager.transform.position);
                _projectile.SetHookSprite(stateManager.currentState.LookDirection);
                SendGrapplingHook();
                await _animationGrapplingHookFire.Play(stateManager);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GrapplingHookItem.Action failed: {ex}");
                PutAway();
            }
        }
NEW

echo
echo "== Verify =="
fail=0
for n in BellItem.cs GrapplingHookItem.cs; do
  f="$(find_one "$n")"
  if grep -q 'catch (System.Exception' "$f"; then echo "  OK: $n has try/catch"; else echo "  FAIL: $n missing catch"; fail=1; fi
done
echo
if [ "$fail" -eq 0 ]; then echo "Stage 01 OK. Open Unity and confirm the Console compiles. Delete the .bak files once happy."
else echo "Stage 01 had issues — restore from the .bak files and send me the file."; exit 1; fi
