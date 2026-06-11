// Assets/Scripts/OverlapScripts/KeyPortOverlap.cs
using System.Threading.Tasks;
using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;
    using IT.Interactables.Locks;

    [RequireComponent(typeof(SpriteRenderer))]
    public class KeyPortOverlap : OverlapCheckerBase
    {
        protected override void AddDetectionLayers()
        {
            detectionLayer |= 0x1 << LayerIndex.KeyPort;
        }

        protected override void Start()
        {
            base.Start();
            gameObject.layer = LayerIndex.TargetOverlap;
        }

        public float GetPercentOfOverlap(Bounds overlapping, Bounds targetSpriteRenderer)
        {
            var minA = overlapping.min;
            var maxA = overlapping.max;
            var minB = targetSpriteRenderer.min;
            var maxB = targetSpriteRenderer.max;

            var lowerMax  = Vector3.Min(maxA, maxB);
            var higherMin = Vector3.Max(minA, minB);

            Vector2 overlappingSquare = lowerMax - higherMin;
            float overlappingArea = overlappingSquare.x * overlappingSquare.y;
            return overlappingArea / (overlapping.extents.x * 2 * targetSpriteRenderer.extents.y * 2) * 100f;
        }

        // Returns the port the slidable is seated on, or null. Pure geometry — caller decides if it matches.
        public Task<KeyPortBase> FindKeyPort()
        {
            SetMovingOverlappingArea(transform.position);
            Collider2D col = GetMostOverlappedCol();
            if (col == null) return Task.FromResult<KeyPortBase>(null);

            SpriteRenderer overlapField = GetComponent<SpriteRenderer>();
            KeyPortBase port = col.GetComponent<KeyPortBase>();
            if (port == null) return Task.FromResult<KeyPortBase>(null);
            if (GetPercentOfOverlap(col.bounds, overlapField.bounds) <= 60f) return Task.FromResult<KeyPortBase>(null);

            return Task.FromResult(port);
        }

        // Kept for PushBlock.CleanUp which still uses the bool form.
        public async Task<bool> IsOnKeyPort(GameUtilities.KeyTypes key)
        {
            KeyPortBase port = await FindKeyPort();
            return port != null && port.Matches(key);
        }

        public void CleanUp() => Destroy(gameObject);
    }
}