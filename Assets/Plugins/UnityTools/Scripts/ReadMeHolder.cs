using NaughtyAttributes;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    [CreateAssetMenu(menuName = "Unity Tools/Read Me")]
    public class ReadMeHolder : ScriptableObject
    {
        [ResizableTextArea]
        public string Documentation;
    }
}