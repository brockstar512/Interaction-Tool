using UnityEngine;

namespace IT.Core.Shapes
{
    /// <summary>
    /// Axis-aligned world-space rectangle (center + size). Pure math — no BoxCollider2D,
    /// no Physics2D (C-E / NFR-3).
    ///
    /// Contains is EXPLICITLY half-open (inclusive-min, exclusive-max) so a point on a shared
    /// edge resolves to exactly ONE segment — never both, never neither. This is the single
    /// highest-value invariant in Story 5.1 (NFR-10). We do NOT delegate to
    /// UnityEngine.Rect.Contains, whose half-open behavior is an undocumented implementation
    /// detail; membership is load-bearing, so we own the boundary rule here.
    /// </summary>
    [System.Serializable]
    public class RectShape : Shape2D
    {
        [SerializeField] Vector2 _center;
        [SerializeField] Vector2 _size = Vector2.one;

        public RectShape() { }

        public RectShape(Vector2 center, Vector2 size)
        {
            _center = center;
            _size = size;
        }

        public Vector2 Center { get => _center; set => _center = value; }
        public Vector2 Size   { get => _size;   set => _size = value; }

        /// <summary>Inclusive lower corner (center − size/2).</summary>
        public Vector2 Min => _center - _size * 0.5f;

        /// <summary>Exclusive upper corner (center + size/2).</summary>
        public Vector2 Max => _center + _size * 0.5f;

        /// <summary>
        /// Half-open containment: inclusive-min, exclusive-max — owned explicitly, NOT
        /// delegated to UnityEngine.Rect.Contains. A point exactly on the min edge is IN;
        /// a point exactly on the max edge is OUT. So a point on a boundary shared by two
        /// rects belongs to the one whose inclusive-min edge it sits on — exactly one segment.
        /// </summary>
        public override bool Contains(Vector2 worldPoint)
        {
            Vector2 min = Min;
            Vector2 max = Max;
            return worldPoint.x >= min.x && worldPoint.x < max.x
                && worldPoint.y >= min.y && worldPoint.y < max.y;
        }

        public override Bounds WorldBounds => new Bounds(_center, _size);
    }
}
