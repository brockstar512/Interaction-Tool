using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapTargetCheck : OverlapObjectCheck, IOverlapTarget
    {
        //get percentage that item overlaps
        public float GetPercentOfOverlap(Bounds a, Bounds b)
        {
            // get the bounds of both colliders
            var boundsA = a;
            var boundsB = b;

            // get min and max point of both
            var minA = boundsA.min; //(basically the bottom-left-back corner point)
            var maxA = boundsA.max; //(basically the top-right-front corner point)

            var minB = boundsB.min;
            var maxB = boundsB.max;

            // we want the smaller of the max and the higher of the min points
            var lowerMax = Vector3.Min(maxA, maxB);
            var higherMin = Vector3.Max(minA, minB);

            // the delta between those is now your overlapping area
            Vector2 overlappingSqaure = lowerMax - higherMin;
            float overlappingArea = overlappingSqaure.x * overlappingSqaure.y;

            return overlappingArea / (a.extents.x * 2 * a.extents.y * 2) * 100.0f;
        }
        //if overlap target

        public void CleanUp()
        {
            Destroy(this.gameObject);
        }
    }
}
