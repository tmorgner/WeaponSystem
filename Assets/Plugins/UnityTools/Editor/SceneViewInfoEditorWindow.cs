using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RabbitStewdio.Unity.Plugins.UnityTools.Editor
{
    public class SceneViewInfoEditorWindow : EditorWindow
    {
        [NonSerialized]
        bool registered = false;
        LayerMask layerMask = -1;
        Vector3 position;
        Vector3 normal;
        GameObject target;

        [MenuItem("Tools/Terrain/Terrain Info")]
        static void ShowToolWindow()
        {
            GetWindow<SceneViewInfoEditorWindow>();
        }


        void OnGUI()
        {
            if (!registered)
            {
                SceneView.onSceneGUIDelegate += OnScene;
                registered = true;
            }

            layerMask = LayerMaskField("Mask", layerMask);

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
            EditorGUILayout.Vector3Field("Position", position);
            EditorGUILayout.Vector3Field("Normal", normal);
            EditorGUILayout.FloatField("Slope", Vector3.Angle(normal, Vector3.up));
            GUI.enabled = true;
        }

        static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
                else if (((1 << i) & layerMask.value) > 0)
                {
                    layers.Add("<<Undefined Layer " + i + ">>");
                    layerNumbers.Add(i);
                }
            }

            var mask = EditorGUILayout.MaskField(label, layerMask.value, layers.ToArray());
            layerMask.value = mask;
            return layerMask;
        }

        void OnScene(SceneView scene)
        {
            var ray = scene.camera.ScreenPointToRay (Event.current.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                target = hit.transform.gameObject;
                normal = hit.normal;
                position = hit.point;
                this.Repaint();
            }
        }

        void OnDestroy()
        {
                SceneView.onSceneGUIDelegate -= OnScene;
        }
    }
}
