using System;

namespace NaughtyAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : DrawerAttribute
    {
        public enum ButtonInvocationTarget
        {
            Default,
            First,
            All
        }

        public string Text { get; private set; }

        public ButtonInvocationTarget Target { get; set; }
        
        public ButtonAttribute(string text = null)
        {
            this.Text = text;
        }
    }
}
