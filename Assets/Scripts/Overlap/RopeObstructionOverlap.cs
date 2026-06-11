using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;

    public class RopeObstructionOverlap : MonoBehaviour, IAllOverlap<Collider2D>
    {
        public Vector2 _areaTopRightCornerAABB, _areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        LineRenderer lr;

        private void Awake()
        {
            lr= GetComponent<LineRenderer>();
        }
        
        
        private void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerIndex.Depth;
        }
        private void SetMovingOverlappingArea(Bounds bounds)
        {
            //get line rendere
            float centerX = bounds.center.x;
            float centerY = bounds.center.y;
            float extendsX = bounds.extents.x;
            float extendsY = bounds.extents.y;

            _areaTopRightCornerAABB = new Vector2(centerX + extendsX, centerY + extendsY);
            _areaBottomLeftCornerAABB = new Vector2(centerX - extendsX, centerY - extendsY);
        }
        
        public Collider2D[] GetAllOverlapObject(Bounds areaChecker)
        {
            AddDetectionLayers();
            SetMovingOverlappingArea(areaChecker);

            var overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB, detectionLayer);
            
            return overlappingCols ?? Array.Empty<Collider2D>();
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

            DebugDraw.DrawRectange(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB);

        }
        

    }
}
