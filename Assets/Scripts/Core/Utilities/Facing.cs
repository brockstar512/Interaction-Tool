using UnityEngine;

namespace IT.Core.Utilities
{
    public enum Facing { Up, Down, Left, Right }

    // One direction utility (refactor WS5.1 / Story 1.5). The four-way branching lives here so
    // call sites don't repeat it. Camera look-ahead, segment edges, and sprite/anim selection
    // are intended future consumers (architecture FR-4).
    public static class FacingExtensions
    {
        // Exact cardinal only: diagonal / zero input returns null so callers can leave facing unchanged.
        public static Facing? FromVector(Vector2 v)
        {
            if (v == Vector2.up)    return Facing.Up;
            if (v == Vector2.down)  return Facing.Down;
            if (v == Vector2.left)  return Facing.Left;
            if (v == Vector2.right) return Facing.Right;
            return null;
        }

        // Dominant-axis snap for consumers that want a Facing for ANY vector (zero / tie → Down).
        public static Facing FromVectorSnapped(Vector2 v)
        {
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) return v.x >= 0f ? Facing.Right : Facing.Left;
            return v.y > 0f ? Facing.Up : Facing.Down;
        }

        public static Vector2 ToVector(this Facing f) => f switch
        {
            Facing.Up    => Vector2.up,
            Facing.Down  => Vector2.down,
            Facing.Left  => Vector2.left,
            Facing.Right => Vector2.right,
            _            => Vector2.down,
        };

        public static bool IsHorizontal(this Facing f) => f == Facing.Left || f == Facing.Right;
    }
}
