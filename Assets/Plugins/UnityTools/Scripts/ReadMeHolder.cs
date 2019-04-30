using NaughtyAttributes;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    [CreateAssetMenu(menuName = "Tools/Unity Tools/Read Me", fileName = "README")]
    public class ReadMeHolder : ScriptableObject
    {
        [ResizableTextArea]
        public string Documentation;
    }
}