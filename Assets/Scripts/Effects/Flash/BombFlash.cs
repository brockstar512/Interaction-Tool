using UnityEngine;

namespace IT.Effects.Flash
{
    public class BombFlash : FlashBase, IFlashable
    {
        public override void Awake()
        {
            base.Awake();
            StartFlash(0f);   // unbounded — blinks until the bomb is destroyed
        }
        public override void SetFlashTime()
        {
            flashTime = .25f;
        }

        public void StartFlash(float duration) => RunFlash(duration);

        // StopFlash() inherited from FlashBase satisfies IFlashable.
        private void OnDestroy() => StopFlash();
    }
}
