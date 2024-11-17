using System.Collections;
using System.Collections.Generic;
using Animation.PlayerAnimation.AnimationStates;
using UnityEngine;

namespace Explode
{
    public class Explosion : MonoBehaviour, IExlosionDamage
    {
        private ExplosionAnimation _explosionAnimation;
        private Animator _explosionAnimator;

        private void Awake()
        {
            _explosionAnimator = GetComponent<Animator>();
            _explosionAnimation = new ExplosionAnimation();
        }

        public async void AnimateExplosion()
        {
            await _explosionAnimation.Play(_explosionAnimator);
            Destroy(this.gameObject);
        }
    }
}
