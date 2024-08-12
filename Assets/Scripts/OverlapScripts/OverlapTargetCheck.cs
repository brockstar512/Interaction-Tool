using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KeySystem;
namespace Player.ItemOverlap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OverlapTargetCheck : OverlapObjectCheck, IOverlapTarget
    {

        

        private Bounds bounds;
        void Awake()
        {
            bounds = GetComponent<SpriteRenderer>().bounds;
            
        }

        void Start()
        {
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.SlidePadLayer);
        }
        
        
        public bool CleanUp(Utilities.KeyTypes key)
        {
            SetMovingOverlappingArea(this.transform.position);
            Collider2D col = GetMostOverlappedCol();
            //Collider2D[] col = GetAllOverlappedCol();
            SpriteRenderer overlapField = this.GetComponent<SpriteRenderer>();
            KeyPort port = col.GetComponent<KeyPort>();
            if (port != null && 
                GetPercentOfOverlap(col.bounds, overlapField.bounds) > 95.0f &&
                port.Lock(key))
            {
                Debug.Log("Lock it");
                return true;

            }
            // foreach (var item in col) 
            // {
            //     if (item.GetComponent<KeyPort>() != null)
            //     {
            //         float percent = GetPercentOfOverlap(item.bounds, overlapField.bounds);
            //         result = percent > 95.0f;
            //     }
            // }

            
            
            return false;
        }
        
        //get percentage that item overlaps
        public float GetPercentOfOverlap(Bounds overlapping, Bounds targetSpriteRenderer)
        {
            // get the bounds of both colliders
            var boundsA = overlapping;
            var boundsB = targetSpriteRenderer;

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

            return overlappingArea / (overlapping.extents.x * 2 * targetSpriteRenderer.extents.y * 2) * 100.0f;
        }
    }
}
