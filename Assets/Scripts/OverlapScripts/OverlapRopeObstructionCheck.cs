using System;
using UnityEngine;

namespace Player.ItemOverlap
{
    public class OverlapRopeObstructionCheck : MonoBehaviour, IGetAllOverlap<Collider2D>
    {
        public Vector2 _areaTopRightCornerAABB, _areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        LineRenderer lr;

        private void Awake()
        {
            lr= GetComponent<LineRenderer>();
        }

        // Start is called before the first frame update
        void Start()
        {
            AddDetectionLayers();
        }

        public void Hello()
        {
            
        }

        private void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.DepthLayer);
        }

        private void SetMovingOverlappingArea(Vector2 areaCheckerBounds)
        {
            //get line rendere
            float centerX = lr.bounds.center.x;
            float centerY = lr.bounds.center.y;
            float extendsX = lr.bounds.extents.x;
            float extendsY = lr.bounds.extents.y;

            _areaTopRightCornerAABB = new Vector2(centerX + extendsX, centerY + extendsY);
            _areaBottomLeftCornerAABB = new Vector2(centerX - extendsX, centerY - extendsY);
        }
        
        public Collider2D[] GetAllOverlapObject(Vector2 areaCheckerBounds)
        {
            Debug.Break();
            SetMovingOverlappingArea(areaCheckerBounds);

            Collider2D[] overlappingCols =
                Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB, detectionLayer);
            if (overlappingCols.Length == 0)
                return null;
            return overlappingCols;
        }
        

        private (Vector2, Vector2) GetAABBCorners(Collider2D overlappingObject)
        {

            Bounds objectsBound = overlappingObject.bounds;

            Vector2 topRightCorner = new Vector2(
                objectsBound.center.x + objectsBound.extents.x,
                objectsBound.center.y + objectsBound.extents.y);

            Vector2 bottomLeftCorner = new Vector2(
                objectsBound.center.x - objectsBound.extents.x,
                objectsBound.center.y - objectsBound.extents.y);

            return (topRightCorner, bottomLeftCorner);
        }

        private void OnDrawGizmos()
        {

            CustomDebug.DrawRectange(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB);

        }
        

    }
}
