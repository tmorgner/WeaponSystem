using System;
using System.Reflection;
using UnityEditor;

namespace NaughtyAttributes.Editor
{
    [NativePropertyDrawer(typeof(ShowNativePropertyAttribute))]
    public class ShowNativePropertyNativePropertyDrawer : NativePropertyDrawer
    {
        public override void DrawNativeProperty(UnityEngine.Object target, PropertyInfo property)
        {
            try
            {

                object value = property.GetValue(target, null);

                if (!EditorDrawUtility.DrawLayoutField(value, ObjectNames.NicifyVariableName(property.Name),
                                                       property.PropertyType))
                {
                    string warning = string.Format("{0} doesn't support {1} types",
                                                   typeof(ShowNativePropertyNativePropertyDrawer).Name,
                                                   property.PropertyType.Name);
                    EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, logToConsole: true, context: target);
                }
            }
            catch(Exception e)
            {
                EditorDrawUtility.DrawHelpBox("Error: " + e, MessageType.Error, logToConsole: true, context: target);
            }
        }
    }
}
