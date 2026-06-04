using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PullableOpenDependant : MonoBehaviour, IPullDependent
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openStateName = "Open";   // the same door-open state your door anim uses

    private int _stateHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        _stateHash = Animator.StringToHash(openStateName);
        animator.speed = 0f;     // we drive the position ourselves, it never plays on its own
        Scrub(0f);               // start closed (frame 0 of the clip)
    }

    public void OnPullChanged(float amount) => Scrub(amount);

    private void Scrub(float amount)
    {
        animator.Play(_stateHash, 0, Mathf.Clamp01(amount));
        animator.Update(0f);     // force the pose to apply this frame, in sync with the pull
    }
}
