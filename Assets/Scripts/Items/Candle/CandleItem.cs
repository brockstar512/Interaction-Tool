using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IT.Items.Candle
{
    using IT.Interactables;
    using IT.Player.Control;

    public class CandleItem : ItemBase, IButtonUp, ISerializableItem
    {
         public CandleLightController candleLightPrefab;
        private ICandleLight _candleLight;
        private float _lightTime = 10f;
        private CancellationTokenSource _cancellationTokenSource;

        // Story PB.3: the authored full burn, and the only fact that persists besides
        // lit-ness. _lightTime is the REMAINDER; the blob stores elapsed = Duration - remainder
        // so the shape mirrors PoisonEffect's elapsed/duration exactly (spec DD4).
        private const float Duration = 10f;

        // Is the candle burning right now? Set by Action(), cleared by ButtonUp. NOT serialized
        // — lit-ness is held-down state and does not persist (DD7 fallback); kept as internal
        // burning-state tracking (option a).
        private bool _isLit;

        // Cached so the fuel clock can be gated by the owning player's WrapperState, the
        // same way StatusController.Tick is (DD4). A DROPPED candle has no wrapper in its
        // parents — that is treated as Active, so a dropped lit candle still burns down.
        private PlayerWrapper _wrapper;
        private bool _wrapperLookedUp;

        public override void Use(IInteractionContext context)
        {
            ItemFinishedCallback = context.EndInteraction;

            if (_lightTime <= 0f)
            {
                PutAway();
            }
            else
            {
                Light(transform);
            }
        }

        // The context-free half of ignition (spec DD7). Use() calls it with the candle's own
        // transform. RestoreState does NOT call it — lit-ness is held-down state and does not
        // persist (DD7 fallback); the player relights on the far side.
        //
        // The light parents to the CANDLE, not to the player: positionally identical (the
        // candle sits at the player's origin via TakeChild), and it means a candle dropped
        // while lit takes its light with it instead of orphaning it on the player.
        private void Light(Transform lightParent)
        {
            _candleLight ??= Instantiate(candleLightPrefab, lightParent);
            Action();
        }

        async void Action()
        {
            _candleLight.On();
            _isLit = true;

            _cancellationTokenSource?.Cancel();      // cancel + dispose any previous burn
            _cancellationTokenSource?.Dispose();
            // Linked to destroyCancellationToken so BOTH ButtonUp (mid-burn) and object-destroy
            // stop the timer (Async convention §11).
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            await StartTimer(_cancellationTokenSource.Token);
        }

        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }

        public void ButtonUp()
        {
            CancelTask();
            _candleLight.Off();
            _isLit = false;
            PutAway();
        }

        private async Task StartTimer(CancellationToken cancellationToken)
        {
            try
            {
                while (_lightTime > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // PB.3 (DD4): the fuel clock only runs while the owning player is Active,
                    // mirroring StatusController.Tick's WrapperState gate. A suspended player
                    // (canvas open) does not burn fuel; a dropped candle has no wrapper and
                    // keeps burning.
                    if (IsOwnerActive()) _lightTime--;
                    await Awaitable.WaitForSecondsAsync(1f, cancellationToken);
                }
            }
            catch (OperationCanceledException) { }   // catches both cancel paths
            catch (Exception ex)
            {
                Debug.Log($"Candle timer error: {ex.Message}");
            }
        }

        // Null wrapper (candle dropped in the world, or not yet parented) counts as Active.
        private bool IsOwnerActive()
        {
            if (!_wrapperLookedUp)
            {
                _wrapper = GetComponentInParent<PlayerWrapper>();
                _wrapperLookedUp = true;
            }
            return _wrapper == null || _wrapper.State == WrapperState.Active;
        }

        // --- Story PB.3 (ISerializableItem). The candle is the ONE runtime-stateful item:
        // its fuel is a real player-visible fact ("my candle is half burned"), so it crosses
        // as data. Lit-ness does NOT cross — it's held-down state (lit only while [USE] held),
        // so RestoreState restores fuel only and the player relights on the far side
        // (DD7 fallback / interaction-shape taxonomy). ---

        [Serializable]
        private struct Params
        {
            public float elapsed;    // Duration - remaining fuel (PoisonEffect mirror, DD4)
            public float duration;   // authored full burn, so an authoring change stays readable
        }

        public bool IsStateful => true;

        public string CaptureState() => JsonUtility.ToJson(new Params
        {
            elapsed = Duration - _lightTime,
            duration = Duration,
        });

        public void RestoreState(string state)
        {
            if (string.IsNullOrEmpty(state)) return;   // fail alive: keep the prefab's fuel

            var p = JsonUtility.FromJson<Params>(state);
            var duration = p.duration > 0f ? p.duration : Duration;
            _lightTime = Mathf.Clamp(duration - p.elapsed, 0f, duration);
            // Fuel only — candle lit-ness is HELD-DOWN state and does not persist (DD7 fallback).
            // The restored candle is UNLIT; the player presses [USE] to relight on the far side.
        }

        private void CancelTask()
        {
            if (_cancellationTokenSource == null) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}


