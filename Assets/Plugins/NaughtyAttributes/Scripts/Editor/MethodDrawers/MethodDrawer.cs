using System.Reflection;

namespace NaughtyAttributes.Editor
{
    public abstract class MethodDrawer
    {
        public abstract bool DrawMethod(UnityEngine.Object target, MethodInfo methodInfo);
    }
}
