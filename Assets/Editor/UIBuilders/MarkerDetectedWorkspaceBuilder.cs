using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "03 — Marker Detected · Workspace Ready"
    public static class MarkerDetectedWorkspaceBuilder
    {
        [MenuItem("Tools/Build UI/03 Marker Detected Workspace")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Workspace, "UI_MarkerDetectedWorkspaceRoot", "Plate_03_Workspace.png", gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.ARCamera);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            SessionPill(c, "AR WORKSPACE", 130f);
            TrackingPill(c);

            // "Marker detected" notification (width hugs its text, centred)
            var n = NewRect("Notification_MarkerDetected", c);
            var t1 = Txt(n, "MARKER DETECTED", 0f, 0f, 13f, W.ExtraBold, White(1f), 0.78f);
            var t2 = Txt(n, "Your AR workspace is ready.", 0f, 0f, 12f, W.Regular, TextSecondary);
            float textW = Mathf.Max(t1.rectTransform.sizeDelta.x, t2.rectTransform.sizeDelta.x);
            float h1 = t1.rectTransform.sizeDelta.y, h2 = t2.rectTransform.sizeDelta.y;
            const float padX = 14f, padY = 10f, bw = 1f, circle = 28f, gap = 10f;
            float nw = padX + bw + circle + gap + textW + padX + bw;
            float colH = h1 + 2f + h2;
            float nh = Mathf.Max(circle, colH) + 2f * (padY + bw);
            TopLeft(n, (FrameW - nw) / 2f, 146f, nw, nh);
            Decorate(n, nw, nh, 14f, NotificationBg, Rgba(46, 213, 115, 0.6f));
            var check = Circle(n, "Check", padX + bw + circle / 2f, nh / 2f, circle, Green);
            TxtCentered(check, "✓", circle, circle, 15f, W.ExtraBold, TextOnGreen, 0f, "Txt_Check");
            float tx = padX + bw + circle + gap;
            TopLeft(t1.rectTransform, tx, (nh - colH) / 2f, t1.rectTransform.sizeDelta.x, h1);
            TopLeft(t2.rectTransform, tx, (nh - colH) / 2f + h1 + 2f, t2.rectTransform.sizeDelta.x, h2);

            // World-space leaders + labels
            var lead = White(0.85f);
            Leader(c, "Leader_Notification", 195f, 201f, 195f, 221.4f, Green);
            Leader(c, "Leader_LED", 312f, 432f, 312f, 440f, lead);
            Leader(c, "Leader_Resistor", 227f, 548f, 227f, 505f, lead);
            Leader(c, "Leader_Battery", 60f, 536f, 60f, 519f, lead);
            Leader(c, "Leader_Breadboard", 294f, 230f, 287.64f, 243f, lead);
            WorldLabel(c, "Label_LED", 290f, 408f, "LED", White(1f));
            WorldLabel(c, "Label_Resistor", 160f, 548f, "RESISTOR · 470 Ω", White(1f));
            WorldLabel(c, "Label_Battery", 16f, 536f, "9V BATTERY", White(1f));
            WorldLabel(c, "Label_Breadboard", 282f, 206f, "BREADBOARD", White(1f));

            // Circuit picker
            var p = BeginPanel(c, "Panel_SelectCircuit", 16f, 653f, 358f);
            var hdr = Txt(p.rt, "SELECT A CIRCUIT", 17f, p.y, 12f, W.ExtraBold, TextSecondary, 1.44f, 0f, 0f,
                TextAlignmentOptions.TopLeft, "Txt_Header");
            p.y += hdr.rectTransform.sizeDelta.y + 10f;

            string[] names = { "LED", "SERIES", "PARALLEL" };
            string[] steps = { "4 steps", "6 steps", "7 steps" };
            float cw = (PanelInnerW(p) - 16f) / 3f;
            for (int i = 0; i < 3; i++)
            {
                bool sel = i == 0;
                float cx = 17f + i * (cw + 8f);
                var card = Box(p.rt, "Card_" + names[i], cx, p.y, cw, 100f, 14f,
                    sel ? Rgba(47, 140, 255, 0.20f) : White(0.05f),
                    sel ? Accent : White(0.12f), sel ? 1.5f : 1f);
                var ic = Box(card, "Icon", 13f, 13f, 24f, 24f, 7f, sel ? Accent : White(0.10f));
                TxtCentered(ic, sel ? "✓" : (i + 1).ToString(), 24f, 24f, 12f, W.ExtraBold, White(1f), 0f, "Txt_Icon");
                Txt(card, names[i], 13f, 40f, 14f, W.ExtraBold, White(1f), 0.56f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Name");
                Txt(card, "CIRCUIT", 13f, 59.9f, 9.5f, W.Bold, TextSecondary, 0.76f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Kind");
                Txt(card, steps[i], 13f, 74.4f, 10f, W.Regular, sel ? TextLightBlue : TextMuted, 0f, 0f, 0f,
                    TextAlignmentOptions.TopLeft, "Txt_Steps");
                var btn = MakeButton(card);
                if (sel) Wire(btn, ScreenNames.Step1);
                // TODO: Series / Parallel circuits aren't built yet.
            }
            p.y += 100f + 10f;
            EndPanel(p);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
