using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;

    public class OverlapHookCheck : OverlapAreaChecker
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << Layers.Interactable;
            detectionLayer |= 0x1 << Layers.SlidableObstruction;
        }

        // Hook continuously refreshes its overlap box.
        private void Update()
        {
            SetMovingOverlappingArea(transform.position);
        }
    }
}