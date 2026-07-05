using UnityEngine;
using UnityEditor;

namespace IT.Editor
{
    using IT.Segments;

    /// <summary>
    /// Inspector polish for <see cref="SegmentEdgeConfig"/> (Story 5.2, Rung 3): a foldout that shows
    /// only the destination fields relevant to the chosen <see cref="EdgeBehavior"/> —
    ///   • Transport → targetSegment + spawnPointId
    ///   • Scene     → targetSceneAsset + spawnPointId (+ read-only resolved targetScenePath)
    ///   • Seamless / Slide → behaviour only.
    /// Editor-only (Assets/Scripts/Editor); no runtime coupling. Mirrors the InterfaceRefDrawer pattern.
    /// </summary>
    [CustomPropertyDrawer(typeof(SegmentEdgeConfig))]
    public class SegmentEdgeConfigDrawer : PropertyDrawer
    {
        const float Pad = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty behavior = property.FindPropertyRelative("behavior");

            Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                line = NextLine(line);
                EditorGUI.PropertyField(line, behavior);

                switch ((EdgeBehavior)behavior.enumValueIndex)
                {
                    case EdgeBehavior.Transport:
                        line = NextLine(line);
                        EditorGUI.PropertyField(line, property.FindPropertyRelative("targetSegment"));
                        line = NextLine(line);
                        EditorGUI.PropertyField(line, property.FindPropertyRelative("spawnPointId"));
                        break;

                    case EdgeBehavior.Scene:
                        line = NextLine(line);
                        EditorGUI.PropertyField(line, property.FindPropertyRelative("targetSceneAsset"));
                        line = NextLine(line);
                        EditorGUI.PropertyField(line, property.FindPropertyRelative("spawnPointId"));
                        line = NextLine(line);
                        using (new EditorGUI.DisabledScope(true))
                            EditorGUI.PropertyField(line, property.FindPropertyRelative("targetScenePath"));
                        break;

                    // Seamless / Slide: behaviour only — no destination fields.
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;            // foldout row
            if (!property.isExpanded) return h;

            h += Line();                                            // behaviour
            switch ((EdgeBehavior)property.FindPropertyRelative("behavior").enumValueIndex)
            {
                case EdgeBehavior.Transport: h += 2 * Line(); break;   // targetSegment + spawnPointId
                case EdgeBehavior.Scene:     h += 3 * Line(); break;   // asset + spawnPointId + path
            }
            return h;
        }

        static float Line() => EditorGUIUtility.singleLineHeight + Pad;

        static Rect NextLine(Rect r) =>
            new Rect(r.x, r.y + EditorGUIUtility.singleLineHeight + Pad, r.width, EditorGUIUtility.singleLineHeight);
    }
}
