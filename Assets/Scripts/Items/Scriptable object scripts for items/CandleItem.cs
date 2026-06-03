using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Interactable;

namespace Items
{
    public class CandleItem : Item, IButtonUp
    {
         public CandleLight candleLightPrefab;
        private ICandleLight _candleLight;
        private float _lightTime = 10f;
        private CancellationTokenSource _cancellationTokenSource;

        public override void Use(IInteractionContext context)
        {
            ItemFinishedCallback = context.EndInteraction;

            if (_lightTime <= 0f)
            {
                PutAway();
            }
            else
            {
                _candleLight ??= Instantiate(candleLightPrefab, context.Transform);
                Action();
            }
        }

        async void Action()
        {
            _candleLight.On();

            _cancellationTokenSource?.Cancel();      // cancel + dispose any previous token
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            await StartTimer(_cancellationTokenSource.Token);
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
                    cancellationToken.ThrowIfCancellationRequested();
                    _lightTime--;
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException) { }   // catches both cancel paths
            catch (Exception ex)
            {
                Debug.Log($"Candle timer error: {ex.Message}");
            }
        }

        private void CancelTask()
        {
            if (_cancellationTokenSource == null) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}


