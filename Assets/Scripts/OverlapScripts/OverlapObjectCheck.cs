using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapObjectCheck : OverlapMostObjectChecker<InteractableBase>
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << Layers.Interactable;
        }
    }
}