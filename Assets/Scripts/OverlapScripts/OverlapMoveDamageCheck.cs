using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapMoveDamageCheck : MonoBehaviour, IDamage
    {
        public Vector2 _areaTopRightCornerAABB,_areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        OverlapMoveCheckHelper _helper;
        
        
        private void Awake()
        {
            _helper = new OverlapMoveCheckHelper();
            //RemoveDetectionLayers();
        }

        private void Start()
        {
            // detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.InteractableLayer);
            //
            // detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.InteractingLayer));
            // detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.PlayerLayer));
            // detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.KeyPortLayer));
            // detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.TargetOverlapLayer));
            RemoveDetectionLayers();
        }

        private void RemoveDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.InteractableLayer);
            
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.InteractingLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.PlayerLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.KeyPortLayer));
            detectionLayer &= ~(1 << LayerMask.NameToLayer(Utilities.TargetOverlapLayer));

        }

        // Update is called once per frame
        private void Update()
        {
            SetMovingOverlappingArea(this.transform.position);
        }
        private void FixedUpdate()
        {
            
            Collider2D[] col = GetAllOverlappedCol();
            if (col.Length < 1)
            {
                return;
            }

            foreach (Collider2D collision in col)
            {
                SlideCollision(collision);
            }
            
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
        private Collider2D[] GetAllOverlappedCol()
        {

            Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB,detectionLayer);
            return overlappingCols;
        }
 

        private void SlideCollision(Collider2D collision)
        {
            IHurt collidedSubject = collision.GetComponent<IHurt>();
            if (collidedSubject is not null)
            {
                
                collidedSubject.ApplyDamage(this);
                
            }
            Destroy(collision.gameObject);
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

        public void CleanUp()
        {
            Destroy(this.gameObject);
        }
        
         private class OverlapMoveCheckHelper 
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
