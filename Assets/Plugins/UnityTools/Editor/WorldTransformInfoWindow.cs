using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace RabbitStewdio.Unity.Plugins.UnityTools.Editor
{
    public class WorldTransformInfoWindow : EditorWindow
    {
        [MenuItem("Window/Show World Transform")]
        static void ShowToolWindow()
        {
            GetWindow<WorldTransformInfoWindow>();
            
        }

        Transform selectedObject;
        bool locked;

        void Awake()
        {
            titleContent = new GUIContent("World Position");
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (!locked)
            {
                var go = Selection.activeGameObject;
                Transform t;
                if (go)
                {
                    selectedObject = go.transform;
                }
                else
                {
                    selectedObject = null;
                }
            }

            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = 61;
            GUI.enabled = false;
            if (selectedObject)
            {
                EditorGUILayout.ObjectField("Object", selectedObject.gameObject, typeof(GameObject), true);
                GUI.enabled = true;
                locked = EditorGUILayout.Toggle("Lock", locked);
                GUI.enabled = false;
                EditorGUILayout.Vector3Field("Position", selectedObject.position);
                EditorGUILayout.Vector3Field("Rotation", selectedObject.eulerAngles);
                EditorGUILayout.Vector3Field("Scale", selectedObject.lossyScale);
            }
            else
            {
                EditorGUILayout.ObjectField("Object", null, typeof(GameObject), true);
                GUILayout.Label("Select a scene game object");
            }

            GUI.enabled = true;
        }
    }
}
