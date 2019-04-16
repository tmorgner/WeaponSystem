using System.Reflection;
using UnityEditor;

namespace NaughtyAttributes.Editor
{
    public abstract class PropertyDrawer
    {
        public virtual void DrawProperty(FieldInfo fieldInfo, SerializedProperty property)
        {
            DrawProperty(property);
        }

        public virtual void DrawProperty(SerializedProperty property)
        {
            EditorDrawUtility.DrawPropertyField(property);
        }

        public virtual void ClearCache()
        {

        }
    }
}
