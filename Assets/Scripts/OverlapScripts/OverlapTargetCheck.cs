using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player.ItemOverlap
{
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
        

        void FixedUpdate()
        {
            
        }

        public void CleanUp()
        {
            SetMovingOverlappingArea(this.transform.position);
            //Collider2D col = GetMostOverlappedCol();
            Collider2D[] col = GetAllOverlappedCol();
            Debug.Log($"Clean up on slideable {col.Length}");//is 0
            Debug.Log($"Clean up on slideable {col[0].gameObject.name}");//is 0


            foreach (var item in col)
            {
                SlideKey key = item.GetComponent<SlideKey>();
                if (key != null)
                {
                    Debug.Log(GetPercentOfOverlap(bounds, key.GetBounds));

                }
            }
            // if (key != null && key is SlideKey)
            // {
            //     Debug.Log("over 95%");
            //     Debug.Log(GetPercentOfOverlap(bounds, key.GetBounds));
            // }
  
            //Destroy(this.gameObject);
        }
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
    }
}
