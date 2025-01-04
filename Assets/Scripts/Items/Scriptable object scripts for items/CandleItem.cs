using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using Interactable;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "CandleItemObject", menuName = "ScriptableObjects/Candle")]
    public class CandleItem : Item, IButtonUp
    {
        private float _currentTime = 10f;
        private CancellationTokenSource _cancellationTokenSource;
        
        
        //what if this had a timed usage so if it's dark you have to use it
        //but like bell it attracts back guys in the dark
        public override void Use(PlayerStateMachineManager stateManager)
        {  
            Debug.Log($"Candle {_currentTime}");
             ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
             if (_currentTime <= 0f)
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
                while (_currentTime > 0)
                {
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();

                    // Log the remaining time
                    Debug.Log($"Time remaining: {_currentTime} seconds");

                    // Wait for 1 second (or adjust interval as needed)
                    await Task.Delay(1000, cancellationToken);

                    // Decrement the timer
                    _currentTime--;
                }

                // Timer completed
                Debug.Log("Time is up! Executing explosion...");
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


