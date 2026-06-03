using Items.SubItems;
using UnityEngine;

namespace Player.ItemOverlap
{
    //todo evenutally i will need to clean and oranize all of these overlap scripts

    public class OverlapHookCheck : MonoBehaviour
    {
        [SerializeField] Vector2 areaTopRightCornerAABB,areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        private SpriteRenderer _sr;

        // Start is called before the first frame update
        void Start()
        {
            AddDetectionLayers();
            _sr = GetComponent<SpriteRenderer>();
        }

        
        private void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.InteractableLayer);
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.SlidableObstructionLayer);
        }
        
        private void Update()
        {
            SetMovingOverlappingArea(this.transform.position);
        }

        private void SetMovingOverlappingArea(Vector2 characterPos)
        {
            float centerX = _sr.bounds.center.x;
            float centerY = _sr.bounds.center.y;
            float extendsX = _sr.bounds.extents.x;
            float extendsY = _sr.bounds.extents.y;

            areaTopRightCornerAABB = new Vector2(centerX + extendsX, centerY + extendsY);
            areaBottomLeftCornerAABB = new Vector2(centerX - extendsX, centerY - extendsY);
        }

        public Collider2D GetMostOverlappedCol()
        {
            // Physics2D.queriesStartInColliders = false;
            Collider2D[] overlappingCols =
                Physics2D.OverlapAreaAll(areaTopRightCornerAABB, areaBottomLeftCornerAABB, detectionLayer);
            if (overlappingCols.Length == 0)
                return null;

            Collider2D col = DetermineMostOverlap(overlappingCols);
 
            //Debug.Log(col.gameObject.name);
            return col;
        }

        private Collider2D DetermineMostOverlap(Collider2D[] lib)
        {
            Collider2D result = lib[0];
            float currentResult = 0;
            foreach (var col in lib)
            {
                float currentArea = GetOverlappingArea(col);

                if (currentArea > currentResult)
                {
                    currentResult = currentArea;
                    result = col;

                }
            }

            return result;
        }

        private float GetOverlappingArea(Collider2D overlappingObject)
        {

            (Vector2 overlappingTopRightCornerAABB, Vector2 overlappingBottomLeftCornerAABB) =
                GetAABBCorners(overlappingObject);

            float xLength = Mathf.Min(areaTopRightCornerAABB.x, overlappingTopRightCornerAABB.x) -
                            Mathf.Max(areaBottomLeftCornerAABB.x, overlappingTopRightCornerAABB.x);
            float yLength = Mathf.Min(areaTopRightCornerAABB.y, overlappingBottomLeftCornerAABB.y) -
                            Mathf.Max(areaBottomLeftCornerAABB.y, overlappingBottomLeftCornerAABB.y);
            return xLength * yLength;
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

            CustomDebug.DrawRectange(areaTopRightCornerAABB, areaBottomLeftCornerAABB);

        }
        
    }
}