using UnityEngine;

namespace IT.Effects.Flash
{
    public class CharacterFlash : FlashBase, IFlashable
    {
        public override void Awake()
        {
            base.Awake();
            SetFlashTime();
        }
        public override void SetFlashTime()
        {
            flashTime = .1f;   // per-leg oscillation speed (not the total window)
        }

        // Total window is supplied by the caller (PlayerStatusManager passes
        // Health.IFramesDuration) so the flash mirrors the invulnerability window
        // without owning or duplicating that value. StopFlash() is inherited.
        public void StartFlash(float duration) => RunFlash(duration);

        private void OnDestroy() => StopFlash();
    }
}
