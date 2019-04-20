using UnityEngine;
using UnityEngine.UI;

namespace RabbitStewdio.Unity.UnityTools.UI
{
    /// <summary>
    ///     Unity's built in layout components behave rather erratically when asked to deal with
    ///     flexible layouts. Instead of trying to fix what is not fixable, lets quickly implement
    ///     a simple and working layout manager.
    /// </summary>
    public class SaneHBoxLayout : LayoutGroup
    {
        const int AXIS_HORIZONTAL = 0;
        const int AXIS_VERTICAL = 1;

        public override void CalculateLayoutInputHorizontal()
        {
            // Mandatory..
            base.CalculateLayoutInputHorizontal();
            var minSize = (float)padding.horizontal;
            var prefSize = (float)padding.horizontal;
            var flexSize = (float) padding.horizontal;
            foreach (var c in rectChildren)
            {
                minSize += LayoutUtility.GetMinWidth(c);
                prefSize += LayoutUtility.GetPreferredWidth(c);
                flexSize += LayoutUtility.GetFlexibleWidth(c);
            }

            SetLayoutInputForAxis(minSize, prefSize, flexSize, AXIS_HORIZONTAL);
        }

        public override void CalculateLayoutInputVertical()
        {
            var paddingVertical = (float) padding.vertical;
            var minSize = 0f;
            var prefSize = 0f;
            var flexSize = 0f;
            foreach (var c in rectChildren)
            {
                minSize = Mathf.Max(LayoutUtility.GetMinHeight(c), minSize);
                prefSize = Mathf.Max(LayoutUtility.GetPreferredHeight(c), prefSize);
                flexSize = Mathf.Max(LayoutUtility.GetFlexibleHeight(c), flexSize);
            }

            SetLayoutInputForAxis(minSize + paddingVertical, prefSize + paddingVertical, flexSize + paddingVertical, AXIS_VERTICAL);
        }

        public override void SetLayoutHorizontal()
        {
            var prefSize = 0f;
            var space = Mathf.Max(0, rectTransform.rect.size[AXIS_HORIZONTAL] - padding.horizontal);
            foreach (var c in rectChildren)
            {
                var pref = LayoutUtility.GetPreferredWidth(c);
                var min = LayoutUtility.GetMinWidth(c);
                var availSpace = Mathf.Max(0, space - prefSize);
                var allocatedSpace = Mathf.Max(min, Mathf.Min(availSpace, pref));

                SetChildAlongAxis(c, AXIS_HORIZONTAL, prefSize + padding.left, allocatedSpace);
                prefSize += allocatedSpace;
            }
        }

        public override void SetLayoutVertical()
        {
            foreach (var c in rectChildren)
            {
                var min = LayoutUtility.GetPreferredHeight(c);
                SetChildAlongAxis(c, AXIS_VERTICAL, padding.top, min);
            }
        }
    }
}