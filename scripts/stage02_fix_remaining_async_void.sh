#!/usr/bin/env bash
# Stage 02 — harden the remaining async void methods (close the silent-failure surface)
#   Moveable.CleanUp, Slidable.SlideItem, Slidable.CleanUp,
#   BombExplode.Start, Explosion.AnimateExplosion, HookProjectile.CheckForStartPin
# Logic is unchanged; each body is wrapped so a throw logs instead of vanishing.
# (Note: the long bodies keep their logic identical; original inline comments are in the .bak.)
# Run from your Unity project ROOT.
set -euo pipefail

[ -d Assets ] || { echo "ERROR: run this from your Unity project root (the folder containing Assets/)."; exit 1; }
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PY="$SCRIPT_DIR/patch_method.py"
[ -f "$PY" ] || { echo "ERROR: patch_method.py must sit next to this script."; exit 1; }
echo "== Stage 02: harden remaining async void =="

if command -v git >/dev/null 2>&1 && git rev-parse --git-dir >/dev/null 2>&1; then
  if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "WARNING: uncommitted changes present. Commit first so this is easy to undo."
    read -r -p "Continue anyway? [y/N] " ans; [ "${ans:-N}" = "y" ] || { echo "Aborted."; exit 1; }
  fi
fi

find_one() { find Assets -type f -name "$1" | head -n1; }
backup() { cp "$1" "$1.bak"; echo "backup: $1.bak"; }

# ---- Moveable.CleanUp ----
F="$(find_one Moveable.cs)"; [ -n "$F" ] || { echo "ERROR: Moveable.cs not found"; exit 1; }
backup "$F"
"$PY" "$F" 'async void CleanUp()' <<'NEW'
    async void CleanUp()
    {
        try
        {
            bool isPlaced = await _targetCheck.IsOnKeyPort(key);
            if (isPlaced)
            {
                _targetCheck.CleanUp();
                moverCheck.CleanUp();
                Destroy(this);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Moveable.CleanUp failed: {ex}");
        }
    }
NEW

# ---- Slidable.SlideItem ----
F="$(find_one Slidable.cs)"; [ -n "$F" ] || { echo "ERROR: Slidable.cs not found"; exit 1; }
backup "$F"
"$PY" "$F" 'async void SlideItem(Vector2 direction, ClosestContactPointHelper hit)' <<'NEW'
    async void SlideItem(Vector2 direction, ClosestContactPointHelper hit)
    {
        try
        {
            Vector3 currColSize = _col.bounds.extents;
            Vector3 targetColSize = hit.Col.bounds.extents;
            float travelDistance = hit.Distance;
            Vector2 sideOfDestination = direction * -1;

            Vector3 destination = Vector3.zero;
            Vector3 currentLocation = this.transform.position;

            if (direction == Vector2.down || direction == Vector2.up)
            {
                float bufferMargin = currColSize.y + targetColSize.y;
                bufferMargin *= sideOfDestination.y;

                if (direction == Vector2.down)
                {
                    bufferMargin -= (currColSize.x / 2);
                }

                destination = new Vector2(currentLocation.x, currentLocation.y + (travelDistance * direction.y) + bufferMargin);
            }
            if (direction == Vector2.right || direction == Vector2.left)
            {
                float bufferMargin = currColSize.x + targetColSize.x;
                bufferMargin *= sideOfDestination.x;
                destination = new Vector2(currentLocation.x + (travelDistance * direction.x) + bufferMargin, currentLocation.y);
            }

            float time = MeasureTime(hit.Distance);
            await Task.Delay(animationDelay);
            _moverCheck = Instantiate(moverCheckPrefab, moverCheckPrefab.transform.position, Quaternion.identity, this.transform);
            _moverCheck.SetDirectionOfOverlap(direction * -1);
            _moverCheck.SetEmergencyStop(EmergencyStopTween);
            _targetCheck = Instantiate(targetCheckPrefab, this.transform.position + targetCheckPrefab.transform.position, Quaternion.identity, this.transform);
            slideAnimation = transform.DOMove(destination, time);
            slideAnimation.onComplete = CleanUp;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Slidable.SlideItem failed: {ex}");
        }
    }
NEW

# ---- Slidable.CleanUp ----
"$PY" "$F" 'async void CleanUp()' <<'NEW'
    async void CleanUp()
    {
        bool isPlaced = false;
        try
        {
            isPlaced = await _targetCheck.IsOnKeyPort(key);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Slidable.CleanUp failed: {ex}");
        }
        _moverCheck.CleanUp();
        _targetCheck.CleanUp();
        if (isPlaced)
        {
            Destroy(this);
        }
    }
NEW

# ---- BombExplode.Start ----
F="$(find_one BombExplode.cs)"; [ -n "$F" ] || { echo "ERROR: BombExplode.cs not found"; exit 1; }
backup "$F"
"$PY" "$F" 'async void Start()' <<'NEW'
        async void Start()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await StartTimer(_timer, _cancellationTokenSource.Token);
            }
            catch (System.OperationCanceledException) { }
            catch (System.Exception ex)
            {
                Debug.LogError($"BombExplode.Start failed: {ex}");
            }
        }
NEW

# ---- Explosion.AnimateExplosion ----
F="$(find_one Explosion.cs)"; [ -n "$F" ] || { echo "ERROR: Explosion.cs not found"; exit 1; }
backup "$F"
"$PY" "$F" 'public async void AnimateExplosion()' <<'NEW'
        public async void AnimateExplosion()
        {
            try
            {
                await _explosionAnimation.Play(_explosionAnimator);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Explosion.AnimateExplosion failed: {ex}");
            }
            finally
            {
                Destroy(this.gameObject);
            }
        }
NEW

# ---- HookProjectile.CheckForStartPin ----
F="$(find_one HookProjectile.cs)"; [ -n "$F" ] || { echo "ERROR: HookProjectile.cs not found"; exit 1; }
backup "$F"
"$PY" "$F" 'async void CheckForStartPin()' <<'NEW'
        async void CheckForStartPin()
        {
            try
            {
                hookConnectorStartPin = await _hookStartOverlap.GetMostOverlappedHookStartCol(playerPos);
                if (hookConnectorStartPin is null)
                    return;
                Utilities.PutObjectOnLayer(Utilities.SocketUsedLayer, hookConnectorStartPin.gameObject);
                _hasStartPin = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"HookProjectile.CheckForStartPin failed: {ex}");
            }
        }
NEW

echo
echo "== Verify =="
fail=0
check() { local f; f="$(find_one "$1")"; if grep -q "$2" "$f"; then echo "  OK: $1"; else echo "  FAIL: $1 ($2 not found)"; fail=1; fi; }
check Moveable.cs       'Moveable.CleanUp failed'
check Slidable.cs       'Slidable.SlideItem failed'
check Slidable.cs       'Slidable.CleanUp failed'
check BombExplode.cs    'BombExplode.Start failed'
check Explosion.cs      'Explosion.AnimateExplosion failed'
check HookProjectile.cs 'CheckForStartPin failed'
echo
if [ "$fail" -eq 0 ]; then echo "Stage 02 OK. Open Unity and confirm the Console compiles. No async void left except the safe ButtonUp-driven item ones."
else echo "Stage 02 had issues — restore from .bak and send me the offending file."; exit 1; fi
