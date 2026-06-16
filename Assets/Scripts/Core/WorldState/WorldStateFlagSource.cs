using IT.Core.Dependency;
using IT.Boot;
using UnityEngine;

namespace IT.Core.WorldState
{
    // Scene-placed bridge: registers a named WorldState flag and exposes it as
    // IDependencySource<bool> so any Dependent<bool> can subscribe via InterfaceRef.
    // [ContextMenu] helpers let you drive the flag manually during playtests.
    public class WorldStateFlagSource : MonoBehaviour, IDependencySource<bool>
    {
        [SerializeField] string _flagId;
        [SerializeField] FlagScope _scope = FlagScope.Permanent;

        IWorldFlag _flag;

        public bool Value => _flag?.Value ?? false;
        public event System.Action<bool> Changed;

        void Awake()
        {
            if (SystemsRoot.Instance == null) { Debug.LogError("[WorldStateFlagSource] SystemsRoot not ready."); return; }
            _flag = SystemsRoot.Instance.WorldState.GetOrRegister(_flagId, _scope);
            _flag.Changed += v => Changed?.Invoke(v);
        }

        [ContextMenu("Set True")]  void SetTrue()  => SystemsRoot.Instance?.WorldState.Set(_flagId, true);
        [ContextMenu("Set False")] void SetFalse() => SystemsRoot.Instance?.WorldState.Set(_flagId, false);
    }
}
