using UnityEngine;

namespace CircuitAR.EditorUIBuilders
{
    // Design tokens read straight from the CircuitAR Figma file (390x844 frames, Inter).
    public static class UIStyleConstants
    {
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static Color White(float a) => new Color(1f, 1f, 1f, a);
        public static Color Black(float a) => new Color(0f, 0f, 0f, a);
        public static Color Rgba(int r, int g, int b, float a) => new Color(r / 255f, g / 255f, b / 255f, a);

        public static readonly Color Accent = Hex("#2F8CFF");
        public static readonly Color Green = Hex("#2ED573");
        public static readonly Color Amber = Hex("#FFB020");
        public static readonly Color Red = Hex("#FF6B5E");

        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = Hex("#AEB6C2");
        public static readonly Color TextMuted = Hex("#7C8594");
        public static readonly Color TextLightBlue = Hex("#8CC4FF");
        public static readonly Color TextBody = Hex("#E6EAF0");
        public static readonly Color TextOnGreen = Hex("#06140B");

        public static readonly Color BgTop = Hex("#0F1829");
        public static readonly Color BgBottom = Hex("#070B13");

        public static readonly Color PanelBg = Rgba(16, 21, 29, 0.84f);
        public static readonly Color PillBg = Rgba(12, 17, 24, 0.80f);
        public static readonly Color IconButtonBg = Rgba(12, 17, 24, 0.60f);
        public static readonly Color NotificationBg = Rgba(12, 17, 24, 0.82f);
    }
}
