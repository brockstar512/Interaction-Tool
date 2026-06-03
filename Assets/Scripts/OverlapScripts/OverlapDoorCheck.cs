using UnityEngine;
using Player.ItemOverlap;

namespace Doors
{
    public class OverlapDoorCheck : OverlapMostObjectChecker<ILocked>
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << Layers.Locked;
        }
    }
}