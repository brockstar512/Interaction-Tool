using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapPullItemCheck : MonoBehaviour
    {
        // Start is called before the first frame update
        private OverlapCheckHelper _helper;
        private SpriteRenderer _sr;
        public Vector2 _areaTopRightCornerAABB,_areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        void Awake()
        {
            _helper = new OverlapCheckHelper();
        }
        
        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            RemoveDetectionLayers();
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
            float centerX = _sr.bounds.center.x; 
            float centerY = _sr.bounds.center.y;
            float extendsX = _sr.bounds.extents.x; 
            float extendsY = _sr.bounds.extents.y;
        
            _areaTopRightCornerAABB = new Vector2(centerX +extendsX ,centerY +extendsY);
            _areaBottomLeftCornerAABB = new Vector2(centerX -extendsX,centerY -extendsY);
        }
        
        private void SetDirectionOfOverlap(Vector3 playerLookDirection)
        {
            Debug.Log("Look direction should be this: "+playerLookDirection);
            this.transform.localScale = _helper.UpdateScale(playerLookDirection);
            this.transform.localPosition = _helper.UpdatePosition(playerLookDirection);
        }
        
        public bool DoesOverlap(Vector2 itemLocation)
        {
            SetDirectionOfOverlap(itemLocation);
            SetMovingOverlappingArea(itemLocation);
            Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB,detectionLayer);

            if (overlappingCols.Length > 0)
            {
                Debug.Log(overlappingCols[0].gameObject.name);
                return true;
            }

            return false;
        }

 

        private void OnDrawGizmos()
        {

            CustomDebug.DrawRectange(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB); 

        }
        class OverlapCheckHelper
        {
            readonly Vector2 horizontalScale = new Vector2(0.1f, 0.25f);
            readonly Vector2 verticalScale = new Vector2(0.8f, 0.1f);
            readonly Vector2 upPos = new Vector2(0, 0.3f);
            readonly Vector2 downPos = new Vector2(0, 0);
            readonly Vector2 leftPos = new Vector2(.25f, 0.15f);
            readonly Vector2 rightPos = new Vector2(.25f, 0.15f);

            public Vector2 UpdateScale(Vector2 lookDirection)
            {
                Vector2 updateScale = Vector2.zero;

                if (lookDirection == Vector2.down || lookDirection == Vector2.up)
                {
                    updateScale = verticalScale;
                }

                if (lookDirection == Vector2.right || lookDirection == Vector2.left)
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
