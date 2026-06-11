using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;
    using IT.Interactables;

    public class OverlapObjectCheck : OverlapMostObjectChecker<InteractableBase>
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << Layers.Interactable;
        }
    }
}