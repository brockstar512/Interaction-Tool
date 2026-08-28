using UnityEngine;
using UnityEngine.UI;
using IT.Player.Control;

namespace IT.Presentation
{
    // 4.6.4 W-R2 (DD4, anchor-scoped per the minigame record's req. 3): Surface 1
    // — world-space prompts. v1 renders ONE reused label honoring the latest
    // ShowPrompt (two simultaneous co-op prompts = recorded limitation; W-4 runs
    // solo); the CONTRACT stays owner-scoped so an entity-supplied renderer can
    // replace this implementation without touching callers.
    //
    // Driver (OQ-B census verdict: reuse the source of truth): polls each
    // wrapper's PlayerStateMachine.PeekInteractable() per frame — the same
    // IBestOverlap call Interact() uses; no second proximity logic exists.
    public class WorldPromptModule : MonoBehaviour, IWorldPromptModule
    {
        Canvas _worldCanvas;
        Text _label;
        string _activeOwner;

        void Awake()
        {
            // Code-built world-space canvas (OQ-G(3) posture; reskin = named debt).
            var canvasGo = new GameObject("WorldPromptCanvas");
            canvasGo.transform.SetParent(transform, false);
            _worldCanvas = canvasGo.AddComponent<Canvas>();
            _worldCanvas.renderMode = RenderMode.WorldSpace;
            var rect = (RectTransform)canvasGo.transform;
            rect.sizeDelta = new Vector2(4f, 1f);
            rect.localScale = Vector3.one * 0.02f;   // world units — readable at 2D cam scale

            var labelGo = new GameObject("Prompt", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(canvasGo.transform, false);
            _label = labelGo.GetComponent<Text>();
            _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _label.fontSize = 64;
            _label.alignment = TextAnchor.MiddleCenter;
            _label.color = Color.white;
            ((RectTransform)labelGo.transform).sizeDelta = new Vector2(200f, 50f);
            canvasGo.SetActive(false);
        }

        public void ShowPrompt(string ownerId, string text, Vector3 worldPosition)
        {
            _activeOwner = ownerId;
            _label.text = text;
            _worldCanvas.transform.position = worldPosition + Vector3.up * 0.75f;
            if (!_worldCanvas.gameObject.activeSelf) _worldCanvas.gameObject.SetActive(true);
        }

        public void HidePrompt(string ownerId)
        {
            // Owner-scoped: only the owner whose prompt is showing may hide it —
            // a second player's Hide must not kill the first player's prompt.
            if (_activeOwner != ownerId) return;
            _activeOwner = null;
            _worldCanvas.gameObject.SetActive(false);
        }

        void Update()
        {
            // The poll driver: first wrapper (roster order) with a peeked
            // interactable wins the one label; none → hide. Deterministic and
            // stuck-proof (recomputed every frame, no latch).
            var roster = PlayerRoster.TryGetInstance();
            if (roster == null) return;
            foreach (var w in roster.Wrappers)
            {
                if (w == null) continue;
                var sm = w.GetComponent<IT.Player.StateMachine.PlayerStateMachine>();
                var item = sm != null ? sm.PeekInteractable() : null;
                if (item != null)
                {
                    ShowPrompt(w.PlayerId, item.name, item.transform.position);
                    return;
                }
            }
            if (_activeOwner != null) HidePrompt(_activeOwner);
        }
    }
}
