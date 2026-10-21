using TMPro;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "07 — Circuit Explanation"
    public static class HowItWorksBuilder
    {
        [MenuItem("Tools/Build UI/07 How It Works")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.HowItWorks, "UI_HowItWorksRoot", "Plate_07_Explanation.png", gradientBg: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.Complete);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            SessionPill(c, "LED CIRCUIT", 180f, new[] { Green, Green, Green, Green });
            TrackingPill(c);
            CircuitStatusPill(c, complete: true);

            MultiLabelBox(c, "Pill_CurrentFlow", 36f, 146f,
                new[]
                {
                    Run.T("CURRENT FLOW", 9.5f, W.ExtraBold, TextLightBlue, 0.76f),
                    Run.T("BATTERY → RESISTOR → LED → BATTERY", 9.5f, W.Bold, White(1f), 0.38f),
                }, 6f, 10f, 6f, -1f, PillBg, new Color(Accent.r, Accent.g, Accent.b, 0.75f));

            // Numbered markers on the circuit
            var centers = new[] { new Vector2(118f, 449f), new Vector2(82.04f, 326.85f), new Vector2(166.32f, 246.2f), new Vector2(248.9f, 218.2f) };
            for (int i = 0; i < centers.Length; i++)
            {
                AddShadow(c, "MarkerGlow_" + (i + 1), centers[i].x - 10f, centers[i].y - 10f, 20f, 20f, 10, 4f, 0f,
                    new Color(Accent.r, Accent.g, Accent.b, 0.8f));
                var m = Circle(c, "Marker_" + (i + 1), centers[i].x, centers[i].y, 20f, Accent);
                TxtCentered(m, (i + 1).ToString(), 20f, 20f, 11f, W.ExtraBold, White(1f), 0f, "Txt_Number");
            }

            var p = BeginPanel(c, "Panel_Explanation", 16f, 528f, 358f);

            var chk = Circle(p.rt, "Check", 17f + 11f, p.y + 11f, 22f, Green);
            TxtCentered(chk, "✓", 22f, 22f, 12f, W.ExtraBold, TextOnGreen, 0f, "Txt_Check");
            var head = Txt(p.rt, "CIRCUIT COMPLETE", 17f + 22f + 8f, 0f, 15f, W.ExtraBold, Green, 0.9f, 0f, 0f,
                TextAlignmentOptions.TopLeft, "Txt_Header");
            TopLeft(head.rectTransform, 17f + 22f + 8f, p.y + (22f - head.rectTransform.sizeDelta.y) / 2f,
                head.rectTransform.sizeDelta.x, head.rectTransform.sizeDelta.y);
            p.y += 22f + 10f;

            PBody(p, "Great job! The circuit is correctly connected.", White(1f), 14f, 0f, W.Medium);

            Solid(p.rt, "Divider", 17f, p.y, PanelInnerW(p), 1f, White(0.10f));
            p.y += 1f + 10f;

            var how = Txt(p.rt, "HOW IT WORKS", 17f, p.y, 11f, W.ExtraBold, TextSecondary, 1.32f, 0f, 0f,
                TextAlignmentOptions.TopLeft, "Txt_HowItWorks");
            p.y += how.rectTransform.sizeDelta.y + 10f;

            string[] steps =
            {
                "The battery provides electrical energy.",
                "Current flows through the circuit.",
                "The resistor limits the current.",
                "The LED converts electrical energy into light.",
            };
            for (int i = 0; i < steps.Length; i++)
            {
                var num = Circle(p.rt, "Num_" + (i + 1), 17f + 10f, p.y + 10f, 20f, Rgba(47, 140, 255, 0.22f));
                TxtCentered(num, (i + 1).ToString(), 20f, 20f, 11f, W.ExtraBold, TextLightBlue, 0f, "Txt_Number");
                var st = Txt(p.rt, steps[i], 17f + 20f + 10f, 0f, 13f, W.Regular, TextBody, 0f, 0f, 0f,
                    TextAlignmentOptions.TopLeft, "Txt_Step" + (i + 1));
                TopLeft(st.rectTransform, 17f + 20f + 10f, p.y + (20f - st.rectTransform.sizeDelta.y) / 2f,
                    st.rectTransform.sizeDelta.x, st.rectTransform.sizeDelta.y);
                p.y += 20f + 10f;
            }

            var quiz = PButton(p, "TAKE QUIZ  →");
            EndPanel(p);
            Wire(quiz, ScreenNames.Quiz);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
