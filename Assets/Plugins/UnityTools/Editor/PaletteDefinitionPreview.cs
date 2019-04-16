using UnityEditor;
using UnityEngine;
using UnityTools;

namespace Plugins.UnityTools.Editor
{
  [CustomPreview(typeof(PaletteDefinition))]
  public class PaletteDefinitionPreview : ObjectPreview
  {
    public override bool HasPreviewGUI()
    {
      return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
      var t = target as PaletteDefinition;
      if (t == null)
      {
        return;
      }

      GUI.Label(r, target.name + " is being previewed");
      GUI.DrawTexture(r, t.Texture);
    }
  }
}