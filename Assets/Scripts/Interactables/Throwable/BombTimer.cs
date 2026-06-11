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
        private CancellationTokenSource _cancellationTokenSource;
        [SerializeField] private ExplosionEffect explosion;

        // This will be called when the game starts
        async void Start()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await StartTimer(_timer, _cancellationTokenSource.Token);
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
                // Wait for the specified time or until cancellation
                await Task.Delay((int)(waitTime * 1000), cancellationToken);

                // If the task completes, execute the explosion
                Explode();
            }
            catch (TaskCanceledException)
            {
                // If the task was cancelled, log this
                Debug.Log("Task was cancelled!");
            }

            Debug.Log($"Waited for {waitTime} seconds!");
        }

        private void CancelTask()
        {
            // Only cancel if the CancellationTokenSource exists
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private void OnDestroy()
        {
            // Make sure that cancellation is requested when the object is destroyed
            if (_cancellationTokenSource != null)
            {
                CancelTask();
                _cancellationTokenSource.Dispose();
            }
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
