using System;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapMoveCheck : MonoBehaviour
    {


        OverlapMoveCheckHelper _helper;
        public Vector2 _areaTopRightCornerAABB,_areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        
        private void Awake()
        {
            _helper = new OverlapMoveCheckHelper();
        }

        private void Start()
        {
           RemoveDetectionLayers();
        }

        void AddDetectionLayers()
        {
            
        }
        
        private void RemoveDetectionLayers()
        {
            //removes these layers from detection
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.InteractingLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.PlayerLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.KeyPortLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.TargetOverlapLayer));   
        }
        
        private void SetMovingOverlappingArea(Vector2 characterPos)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            float centerX = sr.bounds.center.x; 
            float centerY = sr.bounds.center.y;
            float extendsX = sr.bounds.extents.x; 
            float extendsY = sr.bounds.extents.y;
        
            _areaTopRightCornerAABB = new Vector2(centerX +extendsX ,centerY +extendsY);
            _areaBottomLeftCornerAABB = new Vector2(centerX -extendsX,centerY -extendsY);
        }
        
        private void OnDrawGizmos()
        {

            CustomDebug.DrawRectange(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB); 

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

            if (overlappingCols.Length > 0)
            {
                Debug.Log(overlappingCols[0].gameObject.name);
                return true;
            }

            return false;
        }
        
        class OverlapMoveCheckHelper 
            {
                readonly Vector2 verticalScale = new Vector2(1.0f, .1f);
                readonly Vector2 horizontalScale = new Vector2(0.1f,0.75f);
                readonly Vector2 upPos = new Vector2(0,-0.7f);
                readonly Vector2 downPos= new Vector2(0,0);
                readonly Vector2  leftPos = new Vector2(.5f,-.34f);
                readonly Vector2 rightPos = new Vector2(-.5f,-.34f);
                
                public Vector2 UpdateScale(Vector2 lookDirection)
                {
                    Vector2 updateScale = Vector2.zero;
                    
                    if (lookDirection == Vector2.down || lookDirection == Vector2.up)
                    {
                        updateScale = verticalScale;
                    }
                    if (lookDirection == Vector2.right ||lookDirection == Vector2.left)
                    {
                        updateScale = horizontalScale;
                    }
                    
                    return updateScale;
                }
                
                public Vector2 UpdatePosition(Vector2 lookDirection)
                {
                    //Debug.Log(lookDirection);
                    Vector2 updatePosition = Vector2.zero;
                    
                    if (lookDirection == Vector2.down)
                    {
                        updatePosition = downPos;
                    }
                    if (lookDirection == Vector2.right)
                    {
                        updatePosition = rightPos;
                    }
        
                    if (lookDirection == Vector2.left)
                    {
                        updatePosition = leftPos;
                    }
                    if (lookDirection == Vector2.up)
                    {
                        updatePosition = upPos;
                    }
        
            
                    return updatePosition;
                }
            }
    }
    

    
}
