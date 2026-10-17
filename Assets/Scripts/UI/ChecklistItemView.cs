using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.UI
{
    // One chip in the Final Step checklist (built by UIBuilderCommon.PDynamicChecklist).
    // Lets a runtime controller flip a chip between its "ok" (green, checkmark) and
    // "missing" (amber, cross) look without rebuilding the UI.
    public class ChecklistItemView : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Image fill;
        public Image stroke;
        public string itemLabel;

        private static readonly Color Green = new Color(46f / 255f, 213f / 255f, 117f / 255f);
        private static readonly Color Amber = new Color(255f / 255f, 176f / 255f, 32f / 255f);

        public void SetOk(bool ok)
        {
            var c = ok ? Green : Amber;
            if (label != null)
            {
                label.text = (ok ? "✓ " : "✗ ") + itemLabel;
                label.color = c;
            }
            if (fill != null) fill.color = new Color(c.r, c.g, c.b, 0.14f);
            if (stroke != null) stroke.color = new Color(c.r, c.g, c.b, 0.45f);
        }
    }
}
