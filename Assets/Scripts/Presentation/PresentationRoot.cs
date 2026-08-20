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

        ScreenPromptModule _screenModule;

        void OnEnable()
        {
            // R3: build + register the Surface-2 module (constraint 1: the coordinator
            // gets a handle, never internals).
            if (_screenModule == null)
                _screenModule = gameObject.AddComponent<ScreenPromptModule>();
            var coordinator = IT.Boot.SystemsRoot.Instance != null
                ? IT.Boot.SystemsRoot.Instance.Presentation : null;
            coordinator?.Register((IScreenPromptModule)_screenModule);
            Debug.Log($"[Presentation] PresentationRoot ready in '{gameObject.scene.name}' — Screen module {(coordinator != null ? "registered" : "built (no coordinator — direct-play pre-boot?)")}.");
        }

        void OnDisable()
        {
            var coordinator = IT.Boot.SystemsRoot.Instance != null
                ? IT.Boot.SystemsRoot.Instance.Presentation : null;
            if (_screenModule != null) coordinator?.Deregister((IScreenPromptModule)_screenModule);
        }
    }
}
