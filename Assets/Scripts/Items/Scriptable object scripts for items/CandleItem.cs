using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Interactable;
using Unity.VisualScripting;

namespace Items.Scriptable_object_scripts_for_items
{
    public class CandleItem : Item, IButtonUp
    {
        public CandleLight candleLightPrefab;
        private ICandleLight _candleLight;
        private float _lightTime = 10f;
        private CancellationTokenSource _cancellationTokenSource;
        
        public override void Use(PlayerStateMachineManager stateManager)
        {  
             ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;

             if (_lightTime <= 0f)
             {
                 PutAway();
             }
             else
             {
                 _candleLight ??= Instantiate(candleLightPrefab, stateManager.transform);
                 Action();
             }
        }
        
        async void Action()
        {
            _candleLight.On();
            Debug.Log("Task started...");
            _cancellationTokenSource = new CancellationTokenSource();
            // Start the timer with cancellation support
            await StartTimer(_cancellationTokenSource.Token);
            Debug.Log("Task finished!");
            //if that is finished destroy/dispose
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }

        public void ButtonUp()
        {
            CancelTask();
            _candleLight.Off();
            PutAway();
        }
        
        private async Task StartTimer(CancellationToken cancellationToken)
        {
            try
            {
                while (_lightTime > 0)
                {
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();

                    // Log the remaining time
                    Debug.Log($"Time remaining: {_lightTime} seconds");
                    
                    // Decrement the timer
                    _lightTime--;

                    // Wait for 1 second (or adjust interval as needed)
                    await Task.Delay(1000, cancellationToken);

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
                Debug.Log($"An error occurred: {ex.Message}");
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
    }
}


