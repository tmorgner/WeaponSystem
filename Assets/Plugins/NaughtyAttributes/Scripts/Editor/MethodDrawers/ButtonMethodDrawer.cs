using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NaughtyAttributes.Editor
{
    [MethodDrawer(typeof(ButtonAttribute))]
    public class ButtonMethodDrawer : MethodDrawer
    {
        public override bool DrawMethod(UnityEngine.Object target, MethodInfo methodInfo)
        {
            if (methodInfo.GetParameters().Length == 0)
            {
                ButtonAttribute buttonAttribute = (ButtonAttribute) methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? methodInfo.Name : buttonAttribute.Text;

                if (GUILayout.Button(buttonText))
                {
                    EditorGUI.BeginChangeCheck();

                    var gameObjects = Selection.gameObjects;
                    if (gameObjects.Length <= 1 || buttonAttribute.Target == ButtonAttribute.ButtonInvocationTarget.Default)
                    {
                        methodInfo.Invoke(target, null);
                    }
                    else if (target is Component)
                    {
                        var targetType = target.GetType();
                        if (buttonAttribute.Target == ButtonAttribute.ButtonInvocationTarget.First)
                        {
                            var gameObject = Selection.gameObjects[0];
                            var targetComponent = gameObject.GetComponent(targetType);
                            methodInfo.Invoke(targetComponent, null);
                        }
                        else if (buttonAttribute.Target == ButtonAttribute.ButtonInvocationTarget.All)
                        {
                            foreach (var gameObject in Selection.gameObjects)
                            {
                                var targetComponent = gameObject.GetComponent(targetType);
                                methodInfo.Invoke(targetComponent, null);
                            }
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Method Invocation: " + buttonText);
                    }

                    return true;
                }
            }
            else
            {
                string warning = typeof(ButtonAttribute).Name + " works only on methods with no parameters";
                EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, logToConsole: true, context: target);
            }

            return false;
        }
    }
}
