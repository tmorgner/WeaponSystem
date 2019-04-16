using UnityEditor;
using UnityEngine;

namespace UnityTools.SmartVars
{
  public class SmartReferenceDrawerBase : PropertyDrawer 
  {
    /// <summary>
    /// Options to display in the popup to select constant or variable.
    /// </summary>
    readonly string[] popupOptions = {"Use Inline", "Use Constant", "Use Variable"};

    /// <summary> Cached style to use to draw the popup button. </summary>
    GUIStyle popupStyle;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      if (popupStyle == null)
      {
        popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
        {
          imagePosition = ImagePosition.ImageOnly
        };
      }
    
      // public SmartReferenceType ReferenceType;
    // public TVar Variable;
    // public TConst ConstantValue;
    // public TDataType InlineValue;

      var referenceType = property.FindPropertyRelative("ReferenceType");
      var inlineValue = property.FindPropertyRelative("InlineValue");
      var constantValue = property.FindPropertyRelative("ConstantValue");
      var variable = property.FindPropertyRelative("Variable");

      if (referenceType == null || constantValue == null || variable == null || inlineValue == null)
      {
        Debug.LogWarning($"NULL ENCOUNTERED: {referenceType} {constantValue} {variable} {inlineValue}" );
        return;
      }

      label = EditorGUI.BeginProperty(position, label, property);
      position = EditorGUI.PrefixLabel(position, label);

      EditorGUI.BeginChangeCheck();

      // Get properties

      // Calculate rect for configuration button
      var buttonRect = new Rect(position);
      buttonRect.yMin += popupStyle.margin.top;
      buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
      position.xMin = buttonRect.xMax;

      // Store old indent level and set it to 0, the PrefixLabel takes care of it
      var indent = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;

      var result = EditorGUI.Popup(buttonRect, referenceType.intValue, popupOptions, popupStyle);

      referenceType.intValue = result;

      switch (referenceType.intValue)
      {
        case 0:
        {
          EditorGUI.PropertyField(position, inlineValue, GUIContent.none);
          break;
        }
        case 1:
        {
          EditorGUI.PropertyField(position, constantValue, GUIContent.none);
          break;
        }
        case 2:
        {
          EditorGUI.PropertyField(position, variable, GUIContent.none);
          break;
        }
      }

      if (EditorGUI.EndChangeCheck())
      {
        property.serializedObject.ApplyModifiedProperties();
      }
       
      EditorGUI.indentLevel = indent;
      EditorGUI.EndProperty();
    }
  }
}
