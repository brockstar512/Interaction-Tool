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

 
    }


}
