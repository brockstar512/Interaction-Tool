using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.Presentation
{
    // 4.6.1 (DD2, DQ-2(c) ruled): the scene-local presentation host — dies with its
    // scene, re-created on load. ENSURED PER SCENE BY CODE: the OQ-G(3) consequence
    // of the scene-placed ruling (substance preserved — direct-play retains
    // presentation + scene-local lifetime — with ZERO owner authoring; the ensure
    // hook is armed once by Presentation.Awake). R2 ships the Canvas skeleton;
    // the Surface-2 window module arrives at R3 and registers through here.
    // Visual reskin of all code-built UI = NAMED DEBT (owner-ruled, post-v1/polish).
    public class PresentationRoot : MonoBehaviour
    {
        public Canvas ScreenCanvas { get; private set; }

        static bool _ensureArmed;
        internal static void ArmEnsurePerScene()
        {
            if (_ensureArmed) return;   // domain reload resets — armed once per session
            _ensureArmed = true;
            SceneManager.sceneLoaded += (scene, mode) => Ensure();
            Ensure();   // the scene we are already in (direct-play retention, live)
        }

        static void Ensure()
        {
            if (FindFirstObjectByType<PresentationRoot>() != null) return;
            new GameObject("PresentationRoot").AddComponent<PresentationRoot>();
        }

        void Awake()
        {
            var canvasGo = new GameObject("ScreenCanvas");
            canvasGo.transform.SetParent(transform, false);
            ScreenCanvas = canvasGo.AddComponent<Canvas>();
            ScreenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        void OnEnable()
        {
            // R2 scaffold observability — the module registration line replaces this
            // log's meaning at R3 (the root then registers its Surface-2 module here).
            Debug.Log($"[Presentation] PresentationRoot ready in '{gameObject.scene.name}' (Surface-2 module registers at R3+).");
        }

        void OnDisable()
        {
            // Module deregistration arrives WITH the module (R3) — scaffold has
            // nothing registered yet.
        }
    }
}
