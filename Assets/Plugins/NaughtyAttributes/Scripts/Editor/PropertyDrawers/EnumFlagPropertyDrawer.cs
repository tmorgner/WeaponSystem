using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NaughtyAttributes.Editor
{
    [PropertyDrawer(typeof(EnumFlagAttribute))]
    public class EnumFlagPropertyDrawer : PropertyDrawer
    {
        public override void DrawProperty(FieldInfo fieldInfo, SerializedProperty property)
        {
            EditorDrawUtility.DrawHeader(property);
            Enum targetEnum = (Enum)fieldInfo.GetValue(property.serializedObject.targetObject);
            Enum enumNew = EditorGUILayout.EnumFlagsField(property.displayName, targetEnum);
            property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());

        }
    }
}