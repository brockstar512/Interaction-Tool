using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace IT.Interactables.Throwable
{
    public class BombTimer : MonoBehaviour
    {
        [SerializeField] private Transform parentBody;
        private readonly float _timer = 10f;
        [SerializeField] private ExplosionEffect explosion;
        async void Start()
        {
            try
            {
                // destroyCancellationToken fires automatically on destroy (Async convention §11) —
                // no hand-rolled CancellationTokenSource needed.
                _ = LogRemaining(destroyCancellationToken);   // fire-and-forget logger
                await StartTimer(_timer, destroyCancellationToken);
            }
            catch (System.OperationCanceledException) { }
            catch (System.Exception ex)
            {
                Debug.LogError($"BombExplode.Start failed: {ex}");
            }
        }

        private async Task StartTimer(float waitTime, CancellationToken cancellationToken)
        {
            try
            {
                // Wait for the specified time or until cancellation (lifetime-bound)
                await Awaitable.WaitForSecondsAsync(waitTime, cancellationToken);

                // If the wait completes, execute the explosion
                Explode();
            }
            catch (OperationCanceledException)
            {
                // Cancelled (object destroyed before the fuse finished) — silent stop.
            }

            Debug.Log($"Waited for {waitTime} seconds!");
        }
        
        private async Task LogRemaining(CancellationToken token)
        {
            float remaining = _timer;
            while (remaining > 0 && !token.IsCancellationRequested)
            {
                Debug.Log($"Bomb fuse: {remaining:F1}s remaining");
                await Awaitable.WaitForSecondsAsync(0.5f, token);
                remaining -= 0.5f;
            }
        }

        private void OnDestroy()
        {
            // destroyCancellationToken already cancels the fuse/logger on destroy; just tidy up the body.
            Destroy(parentBody.gameObject);
        }

        private void Explode()
        {
            // Perform the explosion
            Debug.Log($"Exploding!");
            
            IExplosionEffect explode = Instantiate(explosion,transform.position,quaternion.identity).GetComponent<IExplosionEffect>();
            if (explode != null)
            {
                explode.AnimateExplosion();
            }
            // else
            // {
            //     Destroy();
            // }

            Destroy(this.gameObject);
        }


        
    }
}
