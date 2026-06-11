// Assets/Scripts/Interface/InterfaceReferenceDrawer.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IT.Editor
{
    using IT.Core;

    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty target = property.FindPropertyRelative("target");
            Type interfaceType = GetInterfaceType(fieldInfo.FieldType);

            EditorGUI.BeginProperty(position, label, property);

            UnityEngine.Object current = target.objectReferenceValue;
            UnityEngine.Object assigned = EditorGUI.ObjectField(
                position, label, current, typeof(UnityEngine.Object), true);

            if (assigned != current)
                target.objectReferenceValue = Validate(assigned, interfaceType);

            EditorGUI.EndProperty();
        }

        // Walks past List<>/array wrappers to find InterfaceReference<TInterface>,
        // then returns TInterface.
        private static Type GetInterfaceType(Type fieldType)
        {
            Type t = fieldType;

            if (t.IsArray)
                t = t.GetElementType();
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                t = t.GetGenericArguments()[0];

            if (t != null && t.IsGenericType &&
                t.GetGenericTypeDefinition() == typeof(InterfaceReference<>))
                return t.GetGenericArguments()[0];

            return null;
        }

        private UnityEngine.Object Validate(UnityEngine.Object obj, Type interfaceType)
        {
            if (obj == null || interfaceType == null) return null;

            if (interfaceType.IsInstanceOfType(obj)) return obj;

            if (obj is GameObject go)
            {
                foreach (var c in go.GetComponents<Component>())
                    if (c != null && interfaceType.IsInstanceOfType(c))
                        return c;

                var components = string.Join(", ", System.Array.ConvertAll(
                    go.GetComponents<Component>(),
                    c => c == null ? "<missing>" : c.GetType().Name));

                Debug.LogWarning(
                    $"'{go.name}' has no component implementing {PrettyName(interfaceType)}. " +
                    $"Components on this GameObject: [{components}]");
                return null;
            }

            Debug.LogWarning(
                $"'{obj.name}' ({obj.GetType().Name}) does not implement {PrettyName(interfaceType)}");
            return null;
        }

        private static string PrettyName(Type t)
        {
            if (!t.IsGenericType) return t.Name;
            var args = string.Join(", ", System.Array.ConvertAll(
                t.GetGenericArguments(), PrettyName));
            var baseName = t.Name.Substring(0, t.Name.IndexOf('`'));
            return $"{baseName}<{args}>";
        }
    }
}
