using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interactables.Slidable
{
    using IT.Core.Utilities;

    public class SlidableTimed : Slidable
    {
        protected override Utilities.KeyTypes key => Utilities.KeyTypes.SlidingBlock;

        [SerializeField] private float countdownSeconds = 10f;

        private float _remaining;
        private bool  _running;

        protected override void Awake()
        {
            base.Awake();
            _remaining = countdownSeconds;
            _running   = true;
        }

        private void Update()
        {
            if (!_running) return;

            _remaining -= Time.deltaTime;
            if (_remaining > 0f) return;

            _running = false;
            Timeout();
        }

        // Empty hook — override in a subclass or add behavior here later.
        protected virtual void Timeout() { }
    }
}
