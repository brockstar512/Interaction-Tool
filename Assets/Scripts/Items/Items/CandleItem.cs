using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using Interactable;

namespace Items.Scripts
{
    public class CandleItem : Item, IButtonUp
    {
        private readonly CandleState _candleState = new CandleState();
        private CancellationTokenSource _cancellationTokenSource;
        
        public override void Use(PlayerStateMachineManager stateManager)
        {  
             ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
             if (_candleState.CurrentTime <= 0f)
             {
                 PutAway();
             }
             Action();
        }
        async void Action()
        {
            Debug.Log("Task started...");
            _cancellationTokenSource = new CancellationTokenSource();
            // Start the timer with cancellation support
            await StartTimer(_cancellationTokenSource.Token);
            
            Debug.Log("Task finished!");
            //if that is finished destroy/dispose
            PutAway();
        }
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
        
        private async Task StartTimer( CancellationToken cancellationToken)
        {
            try
            {
                while (_candleState.CurrentTime > 0)
                {
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();

                    // Log the remaining time
                    Debug.Log($"Time remaining: {_candleState.CurrentTime} seconds");

                    // Wait for 1 second (or adjust interval as needed)
                    await Task.Delay(1000, cancellationToken);

                    // Decrement the timer
                    _candleState.CurrentTime--;
                }

                // Timer completed
                Debug.Log("Time is up!");
            }
            catch (TaskCanceledException)
            {
                // If the task was cancelled, log this
                Debug.Log("Task was cancelled!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred: {ex.Message}");
            }
        }

        private void CancelTask()
        {
            // Only cancel if the CancellationTokenSource exists
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public void ButtonUp()
        {
            CancelTask();
            PutAway();
        }
    }
}


public class CandleState
{
    public float CurrentTime = 10f;
}