using UnityEngine;
using KeySystem;
using System.Threading.Tasks;

namespace Player.ItemOverlap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OverlapTargetCheck : OverlapObjectCheck, IOverlapTarget
    {

        

        private Bounds bounds;
        void Awake()
        {
            bounds = GetComponent<SpriteRenderer>().bounds;
            
        }

        void Start()
        {
            detectionLayer |= 0x1 << LayerMask.NameToLayer(Utilities.KeyPortLayer);

        }

        public async Task<bool> IsOnKeyPort(Utilities.KeyTypes key)
        {
            Debug.Log($"Checking lock {key}");
            SetMovingOverlappingArea(this.transform.position);
            Collider2D col = GetMostOverlappedCol();

            if (col == null)
                return false;
            Debug.Log($"Checking col {col.gameObject.name}");

            //Collider2D[] col = GetAllOverlappedCol();
            SpriteRenderer overlapField = this.GetComponent<SpriteRenderer>();
            
            KeyPort port = col.GetComponent<KeyPort>();
            if (port != null && 
                GetPercentOfOverlap(col.bounds, overlapField.bounds) > 95.0f &&
                port.Lock(key))
            {
                Debug.Log("Lock it");
                return true;

            }
            
            return false;
            
        }
        public void CleanUp()
        {

            Destroy(this.gameObject);
        }
        
        //get percentage that item overlaps
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
    }
}
