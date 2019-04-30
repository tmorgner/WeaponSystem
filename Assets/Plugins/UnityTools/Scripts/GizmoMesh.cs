using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    [CreateAssetMenu(menuName = "Tools/Unity Tools/Debug/Gizmo Mesh")]
    public class GizmoMesh : ScriptableObject
    {
        public Mesh Mesh;
        public Color Color;
        public bool WireFrame;

        public Vector3 Translation;
        public Vector3 Rotation;
        public Vector3 Scale;

        void Reset()
        {
            Translation = Vector3.zero;
            Rotation = Vector3.zero;
            Scale = Vector3.one;
        }
    }
}