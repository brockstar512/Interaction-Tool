// Assets/Scripts/OverlapScripts/OverlapTargetCheck.cs
using System.Threading.Tasks;
using UnityEngine;

namespace IT.Overlap
{
    using IT.Core.Utilities;
    using IT.Interactables.Locks;

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

            var lowerMax  = Vector3.Min(maxA, maxB);
            var higherMin = Vector3.Max(minA, minB);

            Vector2 overlappingSquare = lowerMax - higherMin;
            float overlappingArea = overlappingSquare.x * overlappingSquare.y;
            return overlappingArea / (overlapping.extents.x * 2 * targetSpriteRenderer.extents.y * 2) * 100f;
        }

        // Returns the port the slidable is seated on, or null. Pure geometry — caller decides if it matches.
        public Task<KeyPort> FindKeyPort()
        {
            SetMovingOverlappingArea(transform.position);
            Collider2D col = GetMostOverlappedCol();
            if (col == null) return Task.FromResult<KeyPort>(null);

            SpriteRenderer overlapField = GetComponent<SpriteRenderer>();
            KeyPort port = col.GetComponent<KeyPort>();
            if (port == null) return Task.FromResult<KeyPort>(null);
            if (GetPercentOfOverlap(col.bounds, overlapField.bounds) <= 60f) return Task.FromResult<KeyPort>(null);

            return Task.FromResult(port);
        }

        // Kept for Moveable.CleanUp which still uses the bool form.
        public async Task<bool> IsOnKeyPort(Utilities.KeyTypes key)
        {
            KeyPort port = await FindKeyPort();
            return port != null && port.Matches(key);
        }

        public void CleanUp() => Destroy(gameObject);
    }
}