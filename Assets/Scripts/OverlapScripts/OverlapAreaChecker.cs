using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;

    // Shared "aim a box in a direction and find the collider with the most area overlap".
    public abstract class OverlapAreaChecker : MonoBehaviour
    {
        protected Vector2 _areaTopRightCornerAABB, _areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        private SpriteRenderer _sr;
        private readonly OverlapCheckHelper _helper = new OverlapCheckHelper();

        protected abstract void AddDetectionLayers();

        protected virtual void Start()
        {
            AddDetectionLayers();
            _sr = GetComponent<SpriteRenderer>();
        }

        protected Collider2D GetMostOverlappedCol(Vector2 characterPos, Vector2 lookDirection)
        {
            transform.localScale = _helper.UpdateScale(lookDirection);
            transform.localPosition = _helper.UpdatePosition(lookDirection);
            SetMovingOverlappingArea(characterPos);
            return GetMostOverlappedCol();
        }

        public Collider2D GetMostOverlappedCol()
        {
            Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB, detectionLayer);
            if (overlappingCols.Length == 0)
                return null;
            return DetermineMostOverlap(overlappingCols);
        }

        protected void SetMovingOverlappingArea(Vector2 characterPos)
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            float centerX = _sr.bounds.center.x;
            float centerY = _sr.bounds.center.y;
            float extendsX = _sr.bounds.extents.x;
            float extendsY = _sr.bounds.extents.y;
            _areaTopRightCornerAABB = new Vector2(centerX + extendsX, centerY + extendsY);
            _areaBottomLeftCornerAABB = new Vector2(centerX - extendsX, centerY - extendsY);
        }

        Collider2D DetermineMostOverlap(Collider2D[] lib)
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

        float GetOverlappingArea(Collider2D overlappingObject)
        {
            (Vector2 overlappingTopRightCornerAABB, Vector2 overlappingBottomLeftCornerAABB) = GetAABBCorners(overlappingObject);
            float xLength = Mathf.Min(_areaTopRightCornerAABB.x, overlappingTopRightCornerAABB.x) - Mathf.Max(_areaBottomLeftCornerAABB.x, overlappingTopRightCornerAABB.x);
            float yLength = Mathf.Min(_areaTopRightCornerAABB.y, overlappingBottomLeftCornerAABB.y) - Mathf.Max(_areaBottomLeftCornerAABB.y, overlappingBottomLeftCornerAABB.y);
            return xLength * yLength;
        }

        (Vector2, Vector2) GetAABBCorners(Collider2D overlappingObject)
        {
            Bounds objectsBound = overlappingObject.bounds;
            Vector2 topRightCorner = new Vector2(objectsBound.center.x + objectsBound.extents.x, objectsBound.center.y + objectsBound.extents.y);
            Vector2 bottomLeftCorner = new Vector2(objectsBound.center.x - objectsBound.extents.x, objectsBound.center.y - objectsBound.extents.y);
            return (topRightCorner, bottomLeftCorner);
        }

        protected virtual void OnDrawGizmos()
        {
            CustomDebug.DrawRectange(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB);
        }

        protected class OverlapCheckHelper
        {
            readonly Vector2 verticalScale = new Vector2(.5f, .25f);
            readonly Vector2 horizontalScale = new Vector2(0.3f, 0.15f);
            readonly Vector2 upPos = new Vector2(0, 0.5f);
            readonly Vector2 downPos = new Vector2(0, 0);
            readonly Vector2 rightPos = new Vector2(.23f, .15f);
            readonly Vector2 leftPos = new Vector2(.2f, .2f);

            public Vector2 UpdateScale(Vector2 lookDirection)
            {
                Vector2 updateScale = Vector2.zero;
                if (lookDirection == Vector2.down || lookDirection == Vector2.up) updateScale = verticalScale;
                if (lookDirection == Vector2.right || lookDirection == Vector2.left) updateScale = horizontalScale;
                return updateScale;
            }

            public Vector2 UpdatePosition(Vector2 lookDirection)
            {
                Vector2 updatePosition = Vector2.zero;
                if (lookDirection == Vector2.down) updatePosition = downPos;
                if (lookDirection == Vector2.right || lookDirection == Vector2.left) updatePosition = rightPos;
                if (lookDirection == Vector2.up) updatePosition = upPos;
                return updatePosition;
            }
        }
    }
}