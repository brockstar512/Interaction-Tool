using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IT.Player.Control;
using IT.Player.Status;

namespace IT.Presentation
{
    // 4.6.4 W-R2 (DD3, OQ-A confirmed minimum set): Surface 4 — screen effects.
    // v1 ships EXACTLY poison tint + damage flash; anything more = named debt.
    // Tint is a RECOUNT over live controllers on every status event (never a
    // set/unset latch) — a stuck tint after ClearAll has no storage to be stuck
    // in (the R1 answer's belt-by-construction), and the module ALSO hears
    // StatusCleared as the ruled belt. Flash runs UNSCALED (DQ-7 obligation).
    public class ScreenFxModule : MonoBehaviour, IScreenFxModule
    {
        Image _tint;    // poison: green, steady while ≥1 player is poisoned
        Image _flash;   // damage: red, one unscaled fade per hit

        readonly Dictionary<PlayerWrapper, int> _lastHealth = new();
        // Handler references so Unhook removes EXACTLY what Hook added (no
        // dangling lambda can outlive the module and call into it destroyed).
        readonly Dictionary<PlayerWrapper, System.Action<int, int>> _healthHandlers = new();

        void Awake()
        {
            var root = GetComponent<PresentationRoot>();
            var canvas = root != null ? root.ScreenCanvas : GetComponentInChildren<Canvas>();
            _tint = BuildOverlay(canvas, "PoisonTint", new Color(0.2f, 0.8f, 0.2f, 0.15f));
            _flash = BuildOverlay(canvas, "DamageFlash", new Color(0.9f, 0.1f, 0.1f, 0.25f));
        }

        static Image BuildOverlay(Canvas canvas, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;   // an overlay must never eat clicks
            go.SetActive(false);
            return image;
        }

        void OnEnable()
        {
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null) return;
            roster.PlayerJoined += Hook;
            roster.PlayerLeft += Unhook;
            foreach (var w in roster.Wrappers) Hook(w);   // wrappers that joined before us
        }

        void OnDisable()
        {
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null) return;
            roster.PlayerJoined -= Hook;
            roster.PlayerLeft -= Unhook;
            foreach (var w in roster.Wrappers) Unhook(w);
        }

        void Hook(PlayerWrapper wrapper)
        {
            if (wrapper == null) return;
            var status = wrapper.GetComponent<StatusController>();
            if (status != null)
            {
                status.StatusApplied += OnStatusEvent;
                status.StatusExpired += OnStatusEvent;
                status.StatusCleared += Recount;        // the ruled ClearAll belt
            }
            var health = wrapper.GetComponent<IT.Core.Combat.Health>();
            if (health != null && !_healthHandlers.ContainsKey(wrapper))
            {
                _lastHealth[wrapper] = int.MaxValue;    // first change seeds; no flash on seed
                System.Action<int, int> handler = (current, max) => OnHealthChanged(wrapper, current);
                _healthHandlers[wrapper] = handler;     // HealthChanged args = (current, max) — Health.cs invoke sites
                health.HealthChanged += handler;
            }
            Recount();
        }

        void Unhook(PlayerWrapper wrapper)
        {
            if (wrapper == null) return;
            var status = wrapper.GetComponent<StatusController>();
            if (status != null)
            {
                status.StatusApplied -= OnStatusEvent;
                status.StatusExpired -= OnStatusEvent;
                status.StatusCleared -= Recount;
            }
            _lastHealth.Remove(wrapper);
            if (_healthHandlers.TryGetValue(wrapper, out var handler))
            {
                _healthHandlers.Remove(wrapper);
                var health = wrapper.GetComponent<IT.Core.Combat.Health>();
                if (health != null) health.HealthChanged -= handler;
            }
            Recount();
        }

        void OnStatusEvent(StatusEffectBase _) => Recount();

        // The tint truth: recomputed from LIVE controller state on every event —
        // apply, expire, cure, ClearAll, join, leave. No latch to stick (W-2/W-1).
        void Recount()
        {
            bool anyPoison = false;
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null)
                foreach (var w in roster.Wrappers)
                {
                    var status = w != null ? w.GetComponent<StatusController>() : null;
                    if (status == null) continue;
                    foreach (var effect in status.Active)
                        if (effect is PoisonEffect) { anyPoison = true; break; }
                    if (anyPoison) break;
                }
            if (_tint != null) _tint.gameObject.SetActive(anyPoison);
        }

        void OnHealthChanged(PlayerWrapper wrapper, int current)
        {
            _lastHealth.TryGetValue(wrapper, out var last);
            _lastHealth[wrapper] = current;
            if (current < last && last != int.MaxValue)   // a decrease = damage → flash
            {
                StopCoroutine(nameof(FlashRoutine));
                StartCoroutine(nameof(FlashRoutine));
            }
        }

        IEnumerator FlashRoutine()
        {
            _flash.gameObject.SetActive(true);
            var baseColor = new Color(0.9f, 0.1f, 0.1f, 0.25f);
            float t = 0f;
            const float duration = 0.25f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;   // DQ-7: fx clocks are unscaled
                _flash.color = new Color(baseColor.r, baseColor.g, baseColor.b,
                    Mathf.Lerp(baseColor.a, 0f, t / duration));
                yield return null;
            }
            _flash.gameObject.SetActive(false);
        }
    }
}
