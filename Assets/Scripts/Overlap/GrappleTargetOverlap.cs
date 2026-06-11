using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;

    public class GrappleTargetOverlap : OverlapCheckerBase
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerIndex.Interactable;
            detectionLayer |= 0x1 << LayerIndex.SlidableObstruction;
        }

        // Hook continuously refreshes its overlap box.
        private void Update()
        {
            SetMovingOverlappingArea(transform.position);
        }
    }
}