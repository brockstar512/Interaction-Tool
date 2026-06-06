using UnityEngine;

public class PullableDoorAnimation
{
    
        readonly Animator _animator;
        readonly int _doubleDoorOpen = Animator.StringToHash("DoorOpening");
        public PullableDoorAnimation(Animator animator) { _animator = animator; _animator.speed = 0f; }
        public void SetProgress(float t) { _animator.Play(_doubleDoorOpen, 0, Mathf.Clamp01(t)); _animator.Update(0f); }
        // scrub the open clip to a 0..1 position and hold there — call as the lever moves
        public void Step(float normalized)
        {
                _animator.speed = 0f;
                _animator.Play(_doubleDoorOpen, 0, Mathf.Clamp01(normalized));
                _animator.Update(0f);
        }
}
