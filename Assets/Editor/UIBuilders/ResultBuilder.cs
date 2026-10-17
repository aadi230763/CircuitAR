using TMPro;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "08B — Result · Training Complete"
    public static class ResultBuilder
    {
        [MenuItem("Tools/Build UI/08B Result Training Complete")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Result, "UI_ResultRoot", "Plate_08B_Result.png", gradientBg: true);
            var c = s.content;

            StatusBar(s.root);

            const TextAlignmentOptions center = TextAlignmentOptions.Top;
            Txt(c, "LEARNING COMPLETE", 0f, 68f, 28f, W.ExtraBold, White(1f), 0.56f, FrameW, 0f, center, "Txt_Title");
            Txt(c, "LED Circuit · Beginner · 4 steps", 0f, 106f, 14f, W.Regular, TextSecondary, 0f, FrameW, 0f, center, "Txt_Subtitle");

            // Score (ring art comes from the plate)
            Txt(c, "80%", 0f, 214f, 52f, W.ExtraBold, White(1f), 0f, FrameW, 0f, center, "Txt_Score");
            Txt(c, "SCORE", 0f, 280f, 12f, W.ExtraBold, TextSecondary, 1.68f, FrameW, 0f, center, "Txt_ScoreLabel");

            // Stat cards
            string[] values = { "✓", "5", "4" };
            string[] labels = { "CIRCUIT BUILT", "QUESTIONS", "CORRECT ANSWERS" };
            float cw = (342f - 20f) / 3f;
            for (int i = 0; i < 3; i++)
            {
                var card = NewRect("Stat_" + labels[i].Replace(" ", ""), c);
                var v = Txt(card, values[i], 9f, 15f, 26f, W.ExtraBold, i == 0 ? Green : White(1f), 0f, cw - 18f, 0f, center, "Txt_Value");
                float vh = v.rectTransform.sizeDelta.y;
                var l = Txt(card, labels[i], 9f, 15f + vh + 4f, 9.5f, W.ExtraBold, TextSecondary, 0.76f, cw - 18f, 0f, center, "Txt_Label");
                float ch = 15f + vh + 4f + l.rectTransform.sizeDelta.y + 15f;
                TopLeft(card, 24f + i * (cw + 10f), 378f, cw, ch);
                Decorate(card, cw, ch, 14f, White(0.05f), White(0.08f));
                card.Find("Stroke").SetAsFirstSibling();
            }

            // Training complete badge (centred)
            var badge = MultiLabelBox(c, "Badge_TrainingComplete", 0f, 488f,
                new[] { Run.T("✓  TRAINING COMPLETE", 13f, W.ExtraBold, Green, 1.04f) },
                0f, 18f, 10f, -1f, Rgba(46, 213, 115, 0.14f), new Color(Green.r, Green.g, Green.b, 0.75f));
            TopLeft(badge, (FrameW - badge.sizeDelta.x) / 2f, 488f, badge.sizeDelta.x, badge.sizeDelta.y);

            // Review card
            var rv = NewRect("Panel_ReviewMissedQuestion", c);
            float y = 15f;
            var r1 = Txt(rv, "REVIEW · MISSED QUESTION", 17f, y, 10f, W.ExtraBold, Amber, 1f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_ReviewLabel");
            y += r1.rectTransform.sizeDelta.y + 4f;
            var r2 = Txt(rv, "Q3 · Which LED leg is positive (+)?", 17f, y, 14f, W.SemiBold, White(1f), 0f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_ReviewQuestion");
            y += r2.rectTransform.sizeDelta.y + 4f;
            var r3 = Txt(rv, "Answer: the longer leg (anode).", 17f, y, 13f, W.Regular, TextSecondary, 0f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_ReviewAnswer");
            float rh = y + r3.rectTransform.sizeDelta.y + 15f;
            TopLeft(rv, 24f, 548f, 342f, rh);
            Decorate(rv, 342f, rh, 14f, White(0.05f), White(0.08f));
            rv.Find("Stroke").SetAsFirstSibling();

            // Actions
            var home = PrimaryButton(c, "Btn_BackToHome", "BACK TO HOME", 24f, 690f, 342f, 49f);
            var retry = SecondaryButton(c, "Btn_Retry", "RETRY", 24f, 754f, 342f, 49f);
            Wire(home, ScreenNames.HomeStart);
            Wire(retry, ScreenNames.Step1);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
