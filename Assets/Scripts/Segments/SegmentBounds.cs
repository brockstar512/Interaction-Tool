using UnityEngine;
using IT.Core.Shapes;

namespace IT.Segments
{
    /// <summary>
    /// A scene-authored level segment (Story 5.1): a gizmo-drawn rectangle whose membership is
    /// decided by pure point-in-rect math — NO trigger colliders, NO Physics2D (C-E / NFR-3).
    ///
    /// Holds a serialized <see cref="Shape2D"/> (a <see cref="RectShape"/> today; a CircleShape can
    /// drop in later through the same seam with no change here — the reason this is a Shape2D
    /// reference, not a raw Rect). The shape is authored in LOCAL space (its center is an offset from
    /// this transform); world placement comes from the GameObject transform, so dragging the object
    /// moves the segment. Segments are scene objects with serialized component data — no prefab
    /// wiring (C-B).
    ///
    /// Transform rotation/scale are intentionally ignored: segment math is a world-space AABB anchored
    /// at the transform position (C-E). Membership itself is decided by SegmentManager (Story 5.1
    /// Step 5), which reads this component's query surface.
    /// </summary>
    public class SegmentBounds : MonoBehaviour
    {
        [SerializeField] string _segmentId = "segment";
        [SerializeReference] Shape2D _shape = new RectShape(Vector2.zero, new Vector2(6f, 6f));

        /// <summary>Stable identity — membership events, 5.2 edge routing, and 5.3 locks all key off this.</summary>
        public string SegmentId => _segmentId;

        /// <summary>The authored shape, in local space. Exposed for the Scene-view resize handles.</summary>
        public Shape2D Shape => _shape;

        /// <summary>World origin the local shape is offset from (this transform's position).</summary>
        public Vector2 Origin => transform.position;

        /// <summary>
        /// Membership query surface — the primitive all of WS4 reads (NFR-10). The half-open boundary
        /// rule itself lives in the shape (<see cref="RectShape.Contains"/>, Story 5.1 DD3); here we only
        /// translate the world point into the shape's local frame and delegate.
        /// </summary>
        public bool Contains(Vector2 worldPoint) => _shape != null && _shape.Contains(worldPoint - Origin);

        /// <summary>World-space AABB (shape bounds shifted to the transform origin) — for gizmos / broad checks.</summary>
        public Bounds WorldBounds
        {
            get
            {
                if (_shape == null) return new Bounds(Origin, Vector3.zero);
                Bounds b = _shape.WorldBounds;
                b.center += (Vector3)Origin;
                return b;
            }
        }

        static readonly Color IdleColor     = new Color(0.25f, 0.80f, 1f, 0.9f);
        static readonly Color SelectedColor = new Color(1f, 0.85f, 0.20f, 1f);

        void OnDrawGizmos()         => DrawGizmo(IdleColor);
        void OnDrawGizmosSelected() => DrawGizmo(SelectedColor);

        void DrawGizmo(Color color)
        {
            if (_shape == null) return;
            Bounds b = WorldBounds;
            Gizmos.color = color;
            Gizmos.DrawWireCube(b.center, b.size);
        }

        // Right-click Reset / Add Component gives a usable default rect even if SerializeReference
        // did not run the field initializer.
        void Reset()
        {
            _shape = new RectShape(Vector2.zero, new Vector2(6f, 6f));
        }
    }
}
