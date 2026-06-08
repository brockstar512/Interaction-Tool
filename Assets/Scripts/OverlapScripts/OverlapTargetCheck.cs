using UnityEngine;
using KeySystem;
using System.Threading.Tasks;

namespace Player.ItemOverlap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OverlapTargetCheck : OverlapAreaChecker
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << Layers.KeyPort;
        }

        protected override void Start()
        {
            base.Start();
            gameObject.layer = Layers.TargetOverlap;
        }

        public float GetPercentOfOverlap(Bounds overlapping, Bounds targetSpriteRenderer)
        {
            var minA = overlapping.min;
            var maxA = overlapping.max;
            var minB = targetSpriteRenderer.min;
            var maxB = targetSpriteRenderer.max;

            var lowerMax = Vector3.Min(maxA, maxB);
            var higherMin = Vector3.Max(minA, minB);

            Vector2 overlappingSquare = lowerMax - higherMin;
            float overlappingArea = overlappingSquare.x * overlappingSquare.y;
            Debug.Log($"percentage of overlap? {overlappingArea}");

            return overlappingArea / (overlapping.extents.x * 2 * targetSpriteRenderer.extents.y * 2) * 100.0f;
        }

        public async Task<bool> IsOnKeyPort(Utilities.KeyTypes key)
        {
            Debug.Log("running key is on port");
            SetMovingOverlappingArea(transform.position);
            Collider2D col = GetMostOverlappedCol();
            if (col == null)
                return await Task.FromResult(false);
            
            Debug.Log("found a collider");
            SpriteRenderer overlapField = GetComponent<SpriteRenderer>();
            KeyPort port = col.GetComponent<KeyPort>();
            Debug.Log($"did I find a port? {port != null}");

            if (port != null &&
                GetPercentOfOverlap(col.bounds, overlapField.bounds) > 60.0f &&
                port.Lock(key))
            {
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        public void CleanUp()
        {
            Destroy(gameObject);
        }
    }
}