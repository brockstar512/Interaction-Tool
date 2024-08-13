using System;
using UnityEngine;
using OverlapHelpers;

namespace Player.ItemOverlap
{
    public class OverlapMoveCheck : OverlapObjectCheck,IDoesOverlapLayer
    {
        OverlapMoveCheckHelper _helper;

        protected void Awake()
        {
            _helper = new OverlapMoveCheckHelper();
        }

        protected void Start()
        {
           //if you are the player and you are interacting with a moving object. ignore the item you are interacting with and the player
           //removes these layers from detection
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.InteractingLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.PlayerLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.KeyPortLayer));
            detectionLayer &= 0x1 << LayerMask.NameToLayer(Utilities.TargetOverlapLayer);


        }
        public void SetDirectionOfOverlap(Vector3 playerLookDirection)
        {
            this.transform.localScale = _helper.UpdateScale(playerLookDirection);
            this.transform.localPosition = _helper.UpdatePosition(playerLookDirection);
        }
        
        public bool DoesOverlap(Vector2 itemLocation)
        {
            SetMovingOverlappingArea(itemLocation);
            Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB,detectionLayer);
            //Debug.Log(overlappingCols.Length);
            
            if (overlappingCols.Length > 0)
            {
                Debug.Log(overlappingCols[0].gameObject.name);
                return true;
            }

            return false;
        }
        
    }
    
}
