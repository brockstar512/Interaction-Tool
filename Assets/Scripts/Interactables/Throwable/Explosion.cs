using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interactables.Throwable
{
    using IT.Animation.World;

    public class Explosion : MonoBehaviour, IExplosionDamage
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
            try
            {
                await _explosionAnimation.Play(_explosionAnimator);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Explosion.AnimateExplosion failed: {ex}");
            }
            finally
            {
                Destroy(this.gameObject);
            }
        }

    }
}
