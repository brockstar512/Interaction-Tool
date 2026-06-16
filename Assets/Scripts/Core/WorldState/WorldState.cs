using System.Collections.Generic;
using IT.Core.Dependency;
using UnityEngine;

namespace IT.Core.WorldState
{
    public class WorldState
    {
        readonly Dictionary<string, WorldFlag> _flags = new();

        // Register a flag. Idempotent for the same scope. First registration wins on conflict (D3).
        public IWorldFlag GetOrRegister(string id, FlagScope scope)
        {
            if (_flags.TryGetValue(id, out WorldFlag existing))
            {
                if (existing.Scope != scope)
                    Debug.LogWarning($"[WorldState] Flag '{id}' already registered as {existing.Scope}; ignoring new scope {scope}.");
                return existing;
            }
            var flag = new WorldFlag(id, scope);
            _flags[id] = flag;
            return flag;
        }

        // Set a flag value. Warns and no-ops if the flag was never registered.
        public void Set(string id, bool value)
        {
            if (_flags.TryGetValue(id, out WorldFlag flag))
                flag.SetValue(value);
            else
                Debug.LogWarning($"[WorldState] Flag '{id}' is not registered. Call GetOrRegister first.");
        }

        // Get current value. Returns false (default) if the flag is unregistered.
        public bool Get(string id) =>
            _flags.TryGetValue(id, out WorldFlag f) ? f.Value : false;

        // Set all flags of the given scope to false, firing Changed on each that was true.
        // SessionOnly: call from the save system on load only — not segment/scene events (D2).
        // SegmentScoped: call from OnSegmentReEntered (Epic 5 wiring).
        // SceneScoped: call from OnSceneUnloaded (Epic 5 wiring).
        public void ClearScope(FlagScope scope)
        {
            foreach (var flag in _flags.Values)
                if (flag.Scope == scope) flag.SetValue(false);
        }

        // TODO (§9.2 save system): serialize to persistent storage.
        public IReadOnlyDictionary<string, bool> GetPermanentSnapshot()
        {
            var snap = new Dictionary<string, bool>();
            foreach (var kvp in _flags)
                if (kvp.Value.Scope == FlagScope.Permanent)
                    snap[kvp.Key] = kvp.Value.Value;
            return snap;
        }

        // TODO (§9.2 save system): restore from persistent storage.
        // Call ClearScope(Permanent) + ClearScope(SessionOnly) before this on load.
        public void RestorePermanentFlags(IReadOnlyDictionary<string, bool> saved)
        {
            foreach (var kvp in saved)
            {
                if (_flags.TryGetValue(kvp.Key, out WorldFlag flag) && flag.Scope == FlagScope.Permanent)
                    flag.SetValue(kvp.Value);
                else
                    Debug.LogWarning($"[WorldState] RestorePermanentFlags: '{kvp.Key}' not found or not Permanent — skipped.");
            }
        }

        // TODO (Epic 5): wire SegmentManager.OnSegmentReEntered → this method.
        public void OnSegmentReEntered(string segmentId) => ClearScope(FlagScope.SegmentScoped);

        // TODO (Epic 5): wire SceneManager.sceneUnloaded → this method.
        public void OnSceneUnloaded() => ClearScope(FlagScope.SceneScoped);

        // ── Private implementation ────────────────────────────────────────────

        sealed class WorldFlag : IWorldFlag
        {
            readonly Observable<bool> _obs = new(false);

            public string Id { get; }
            public FlagScope Scope { get; }

            // IDependencySource<bool>
            public bool Value => _obs.Value;
            public event System.Action<bool> Changed
            {
                add    => _obs.Changed += value;
                remove => _obs.Changed -= value;
            }

            public WorldFlag(string id, FlagScope scope) { Id = id; Scope = scope; }
            public void SetValue(bool v) => _obs.Value = v;
        }
    }
}
