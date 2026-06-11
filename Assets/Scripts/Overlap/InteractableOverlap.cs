using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;
    using IT.Interactables;

    public class InteractableOverlap : ClosestOverlapChecker<Interactable>
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerIndex.Interactable;
        }
    }
}