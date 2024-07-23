using System;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapMoveCheck : OverlapObjectCheck,IDoesOverlapLayer
    {


        private void Start()
        {
           //if you are the player and you are interacting with a moving object. ignore the item you are interacting with and the player
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.InteractingLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.PlayerLayer));
        }
        
            private void Update()
            {
                DoesOverlap();
            }

            public bool DoesOverlap()
        {
            SetOverlappingArea();
            
            Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB,detectionLayer);
            Debug.Log(overlappingCols.Length);
            foreach (Collider2D layer in overlappingCols)
            {
                Debug.Log(layer);
            }
            
            if (overlappingCols.Length > 0)
            {
                return true;
            }
            // SetMovingOverlappingArea(characterPos);
            // Collider2D overlappingObject = GetMostOverlappedCol();
            // return overlappingObject?.GetComponent<InteractableBase>();
            return false;
        }
    }
    

}
