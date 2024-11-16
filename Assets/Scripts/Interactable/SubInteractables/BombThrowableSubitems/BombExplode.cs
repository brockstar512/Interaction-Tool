using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Explode
{
    public class BombExplode : MonoBehaviour
    {
        [SerializeField] private Transform parentBody;
        private readonly float _timer = 10f;
        private CancellationTokenSource _cancellationTokenSource;

        // This will be called when the game starts
        async void Start()
        {
            Debug.Log("Task started...");
            _cancellationTokenSource = new CancellationTokenSource();

            // Start the timer with cancellation support
            await StartTimer(_timer, _cancellationTokenSource.Token);

            Debug.Log("Task finished!");
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
            Destroy(this.gameObject);
        }
    }
}
