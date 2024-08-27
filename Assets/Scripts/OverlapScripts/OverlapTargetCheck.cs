using System;
using UnityEngine;
using KeySystem;
using System.Threading.Tasks;

namespace Player.ItemOverlap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OverlapTargetCheck : MonoBehaviour
    {
        public Vector2 _areaTopRightCornerAABB,_areaBottomLeftCornerAABB = Vector2.zero;
        [SerializeField] protected LayerMask detectionLayer;
        private Bounds bounds;
        void Awake()
        {
            bounds = GetComponent<SpriteRenderer>().bounds;
            
        }

        void Start()
        {
            AddDetectionLayers();
            UpdateLayerName();
        }

        private void UpdateLayerName()
        {
            this.gameObject.layer = LayerMask.NameToLayer(Utilities.TargetOverlapLayer);
        }

        private void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.KeyPortLayer);

        }
        
        private Collider2D GetMostOverlappedCol()
        {
            // Physics2D.queriesStartInColliders = false;
            Collider2D[] overlappingCols = Physics2D.OverlapAreaAll(_areaTopRightCornerAABB, _areaBottomLeftCornerAABB,detectionLayer);
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
            foreach(var col in lib)
            {
                float currentArea = GetOverlappingArea(col);

                if ( currentArea > currentResult)
                {
                    currentResult = currentArea;
                    result = col;

                }
            }

            return result;
        }
        
        private float GetOverlappingArea(Collider2D overlappingObject)
        { 
        
            (Vector2 overlappingTopRightCornerAABB,Vector2 overlappingBottomLeftCornerAABB) = GetAABBCorners(overlappingObject);

            float xLength = Mathf.Min(_areaTopRightCornerAABB.x,overlappingTopRightCornerAABB.x)-Mathf.Max(_areaBottomLeftCornerAABB.x,overlappingTopRightCornerAABB.x);
            float yLength = Mathf.Min(_areaTopRightCornerAABB.y,overlappingBottomLeftCornerAABB.y)-Mathf.Max(_areaBottomLeftCornerAABB.y,overlappingBottomLeftCornerAABB.y);
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

        public float GetPercentOfOverlap(Bounds overlapping, Bounds targetSpriteRenderer)
        {
            // get the bounds of both colliders
            var boundsA = overlapping;
            var boundsB = targetSpriteRenderer;

            // get min and max point of both
            var minA = boundsA.min; //(basically the bottom-left-back corner point)
            var maxA = boundsA.max; //(basically the top-right-front corner point)

            var minB = boundsB.min;
            var maxB = boundsB.max;

            // we want the smaller of the max and the higher of the min points
            var lowerMax = Vector3.Min(maxA, maxB);
            var higherMin = Vector3.Max(minA, minB);

            // the delta between those is now your overlapping area
            Vector2 overlappingSqaure = lowerMax - higherMin;
            float overlappingArea = overlappingSqaure.x * overlappingSqaure.y;

            return overlappingArea / (overlapping.extents.x * 2 * targetSpriteRenderer.extents.y * 2) * 100.0f;
        }
        public async Task<bool> IsOnKeyPort(Utilities.KeyTypes key)
        {
            SetMovingOverlappingArea(this.transform.position);
            Collider2D col = GetMostOverlappedCol();

            if (col == null)
                return await Task.FromResult(false);

            SpriteRenderer overlapField = this.GetComponent<SpriteRenderer>();
            Debug.Log(overlapField.gameObject.name);
            
            KeyPort port = col.GetComponent<KeyPort>();
            Debug.Log($"percentage  {GetPercentOfOverlap(col.bounds, overlapField.bounds)}");

            if (port != null && 
                GetPercentOfOverlap(col.bounds, overlapField.bounds) > 90.0f &&
                port.Lock(key))
            {
                Debug.Log("Lock it");
                return await Task.FromResult(true);

            }
            
            return await Task.FromResult(false);;
            
        }
        
        public void CleanUp()
        {

            Destroy(this.gameObject);
        }
        

    }
}
