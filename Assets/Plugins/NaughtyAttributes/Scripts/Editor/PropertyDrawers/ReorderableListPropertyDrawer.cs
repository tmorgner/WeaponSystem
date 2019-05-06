using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace NaughtyAttributes.Editor
{
    [PropertyDrawer(typeof(ReorderableListAttribute))]
    public class ReorderableListPropertyDrawer : PropertyDrawer
    {
        struct PropertyKey : IEquatable<PropertyKey>
        {
            readonly string fieldName;
            readonly object sourceObject;

            public PropertyKey(SerializedProperty property)
            {
                fieldName = property.name;
                sourceObject = property.serializedObject;
            }

            public bool Equals(PropertyKey other)
            {
                return string.Equals(fieldName, other.fieldName) && 
                       Equals(sourceObject, other.sourceObject);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is PropertyKey && Equals((PropertyKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (fieldName != null ? fieldName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (sourceObject != null ? sourceObject.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public static bool operator ==(PropertyKey left, PropertyKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(PropertyKey left, PropertyKey right)
            {
                return !left.Equals(right);
            }
        }

        Dictionary<PropertyKey, ReorderableList> reorderableListsByPropertyName = new Dictionary<PropertyKey, ReorderableList>();

        public override void DrawProperty(SerializedProperty property)
        {
            EditorDrawUtility.DrawHeader(property);

            if (property.isArray)
            {
                var propertyKey = new PropertyKey(property);
                if (!this.reorderableListsByPropertyName.ContainsKey(propertyKey))
                {
                    ReorderableList reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
                    {
                        drawHeaderCallback = (Rect rect) =>
                        {
                            EditorGUI.LabelField(rect, string.Format("{0}: {1}", property.displayName, property.arraySize), EditorStyles.label);
                        },

                        drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                        {
                            var element = property.GetArrayElementAtIndex(index);
                            rect.y += 2f;

                            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element);
                        }
                    };

                    this.reorderableListsByPropertyName[propertyKey] = reorderableList;
                }

                this.reorderableListsByPropertyName[propertyKey].DoLayoutList();
            }
            else
            {
                string warning = typeof(ReorderableListAttribute).Name + " can be used only on arrays or lists";
                EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, logToConsole: true, context: PropertyUtility.GetTargetObject(property));

                EditorDrawUtility.DrawPropertyField(property);
            }
        }

        public override void ClearCache()
        {
            this.reorderableListsByPropertyName.Clear();
        }
    }
}
