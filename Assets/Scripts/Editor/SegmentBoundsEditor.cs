using UnityEngine;
using UnityEditor;

namespace IT.Editor
{
    using IT.Segments;
    using IT.Core.Shapes;

    /// <summary>
    /// Scene-view resize handles for a RectShape-backed SegmentBounds (Story 5.1, Step 3).
    /// Move = drag the GameObject (standard transform handle); resize = drag the edge dots here.
    /// Editor-only — excluded from builds via the Assets/Scripts/Editor folder. Writes serialized
    /// component data only, no prefab wiring (C-B). No Physics2D / Collider2D anywhere (C-E).
    /// Only RectShape gets resize handles for now; other Shape2D kinds simply skip them.
    /// </summary>
    [CustomEditor(typeof(SegmentBounds))]
    public class SegmentBoundsEditor : UnityEditor.Editor
    {
        void OnSceneGUI()
        {
            var seg = (SegmentBounds)target;
            if (seg.Shape is not RectShape rect) return;

            Vector2 origin = seg.Origin;
            Vector2 center = origin + rect.Center;
            Vector2 half   = rect.Size * 0.5f;

            EditorGUI.BeginChangeCheck();
            // Axis-locked so each edge only slides along its own axis; we read the relevant component only.
            float right = EdgeHandle(center + Vector2.right * half.x, Vector2.right).x;
            float left  = EdgeHandle(center + Vector2.left  * half.x, Vector2.right).x;
            float up    = EdgeHandle(center + Vector2.up    * half.y, Vector2.up).y;
            float down  = EdgeHandle(center + Vector2.down  * half.y, Vector2.up).y;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(seg, "Resize Segment");

                float minX = Mathf.Min(left, right);
                float maxX = Mathf.Max(left, right);
                float minY = Mathf.Min(down, up);
                float maxY = Mathf.Max(down, up);

                Vector2 newCenterWorld = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
                rect.Size   = new Vector2(Mathf.Max(0.1f, maxX - minX), Mathf.Max(0.1f, maxY - minY));
                rect.Center = newCenterWorld - origin;   // keep the opposite edge fixed while resizing

                EditorUtility.SetDirty(seg);
            }
        }

        static Vector2 EdgeHandle(Vector2 world, Vector2 axis)
        {
            float size = HandleUtility.GetHandleSize(world) * 0.08f;
            return Handles.Slider(world, axis, size, Handles.DotHandleCap, 0f);
        }
    }
}
