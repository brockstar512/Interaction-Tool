using System;
using System.Threading.Tasks;
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
        
        
        private void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.DepthLayer);
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

            CustomDebug.DrawRectange(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB);

        }
        

    }
}
