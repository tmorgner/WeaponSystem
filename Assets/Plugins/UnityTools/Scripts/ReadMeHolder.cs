using NaughtyAttributes;
using UnityEngine;

namespace VRKitchenSimulator.Scripts
{
    [CreateAssetMenu(menuName = "Unity Tools/Read Me")]
    public class ReadMeHolder : ScriptableObject
    {
        [ResizableTextArea]
        public string Documentation;
    }
}