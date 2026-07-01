using UnityEngine;

namespace IT.Core.Shapes
{
    /// <summary>
    /// Pure-math 2D geometry seam for segment membership (Story 5.1).
    /// Point containment ONLY — no Collider2D, no Physics2D (C-E / NFR-3).
    ///
    /// Deliberately NOT named Overlap*: the existing collider-backed IT.Overlap system
    /// (interaction targeting) is a separate concern, and naming this Overlap* would invite
    /// "parallel competing system" confusion. Shape-vs-shape Overlaps is intentionally
    /// deferred — no 5.1 consumer needs it (see Story 5.1 Design Decisions). The seam here is
    /// just Contains(point) + WorldBounds, which keeps adding CircleShape/PolygonShape cheap
    /// later without an Overlaps surface.
    /// </summary>
    [System.Serializable]
    public abstract class Shape2D
    {
        /// <summary>
        /// Half-open point containment in world space (inclusive-min, exclusive-max in
        /// concrete shapes). Load-bearing for all of WS4 membership (NFR-10).
        /// </summary>
        public abstract bool Contains(Vector2 worldPoint);

        /// <summary>World-space axis-aligned bounds — for gizmo drawing and broad checks.</summary>
        public abstract Bounds WorldBounds { get; }
    }
}
