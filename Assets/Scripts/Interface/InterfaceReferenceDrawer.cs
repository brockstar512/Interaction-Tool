using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InterfaceReference<>))]
public class InterfaceReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty target = property.FindPropertyRelative("target");
        Type interfaceType = fieldInfo.FieldType.GetGenericArguments()[0];

        EditorGUI.BeginProperty(position, label, property);

        UnityEngine.Object current = target.objectReferenceValue;
        UnityEngine.Object assigned = EditorGUI.ObjectField(
            position, label, current, typeof(UnityEngine.Object), true);

        if (assigned != current)
            target.objectReferenceValue = Validate(assigned, interfaceType);

        EditorGUI.EndProperty();
    }

    private UnityEngine.Object Validate(UnityEngine.Object obj, Type interfaceType)
    {
        if (obj == null) return null;

        // ScriptableObject or Component that implements the interface
        if (interfaceType.IsInstanceOfType(obj)) return obj;

        // Dragged a GameObject -> grab the first matching component
        if (obj is GameObject go)
        {
            foreach (var c in go.GetComponents<Component>())
                if (interfaceType.IsInstanceOfType(c))
                    return c;
        }

        Debug.LogWarning($"Assigned object does not implement {interfaceType.Name}");
        return null;
    }
}