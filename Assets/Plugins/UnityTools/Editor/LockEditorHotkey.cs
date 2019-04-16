using UnityEditor;

namespace SeekingRainbow.Scripts
{
  public static class LockEditorHotkey
  {
    const string MENU_NAME = "Edit/Toggle Inspector Lock %l";

    static bool enabled;

    /// Called on load thanks to the InitializeOnLoad attribute
    static LockEditorHotkey()
    {
      enabled = EditorPrefs.GetBool(MENU_NAME, false);

      // Delaying until first editor tick so that the menu
      // will be populated before setting check state, and
      // re-apply correct action
      EditorApplication.delayCall += () => { PerformAction(enabled); };
    }

    [MenuItem(MENU_NAME, false, 241)]
    static void ToggleAction()
    {
      PerformAction(!enabled);
    }

    public static void PerformAction(bool enabledState)
    {
      enabled = enabledState;
      Menu.SetChecked(MENU_NAME, enabledState);
      EditorPrefs.SetBool(MENU_NAME, enabledState);
      ActiveEditorTracker.sharedTracker.isLocked = enabledState;
    }
  }
}
