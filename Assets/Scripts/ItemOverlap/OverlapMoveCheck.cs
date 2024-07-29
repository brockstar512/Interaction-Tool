using System;
using UnityEngine;
using OverlapHelpers;

namespace Player.ItemOverlap
{
    public class OverlapMoveCheck : OverlapObjectCheck,IDoesOverlapLayer
    {
        OverlapMoveCheckHelper _helper;


        private void Start()
        {
           //if you are the player and you are interacting with a moving object. ignore the item you are interacting with and the player
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.InteractingLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.PlayerLayer));
            _helper = new OverlapMoveCheckHelper();

        }
        

        public bool DoesOverlap(Vector3 movementDirection)
        {
            this.transform.localScale = _helper.UpdateScale(movementDirection);
            this.transform.localPosition = _helper.UpdatePosition(movementDirection);
            SetMovingOverlappingArea(movementDirection);
            
            
            Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB,detectionLayer);
            Debug.Log(overlappingCols.Length);
            
            if (overlappingCols.Length > 0)
            {
                Debug.Log(overlappingCols[0].gameObject.name);
                return true;
            }

            return false;
        }
        
        //get percentage that item overlaps
        public float GetPercentOfOverlap(Bounds a,Bounds b)
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
        
            return overlappingArea/(a.extents.x * 2 * a.extents.y * 2)*100.0f;
        }

 
    }


}
