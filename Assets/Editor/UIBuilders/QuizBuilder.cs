using TMPro;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "08A — Quiz"
    public static class QuizBuilder
    {
        [MenuItem("Tools/Build UI/08A Quiz")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Quiz, "UI_QuizRoot", "Plate_08A_Quiz.png", gradientBg: true);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.HowItWorks);
            Wire(IconButton(c, "Btn_Close", "×", 334f, 54f), ScreenNames.HomeStart);
            SessionPill(c, "QUIZ · 5 / 5", 188f, new[] { Green, Green, Green, Green, Accent });

            Txt(c, "QUESTION 5 OF 5", 24f, 256f, 12f, W.ExtraBold, Accent, 1.44f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_QuestionLabel");
            Txt(c, "WHAT DOES THE RESISTOR DO?", 24f, 278f, 26f, W.ExtraBold, White(1f), 0f, 342f, 32f,
                TextAlignmentOptions.TopLeft, "Txt_Question");

            string[] options = { "LIMIT CURRENT", "STORE ENERGY", "PRODUCE LIGHT", "GENERATE VOLTAGE" };
            string[] letters = { "A", "B", "C", "D" };
            for (int i = 0; i < options.Length; i++)
            {
                bool correct = i == 0;
                float y = 352f + i * 68f;
                var row = Box(c, "Option_" + letters[i], 24f, y, 342f, 58f, 14f,
                    correct ? Rgba(46, 213, 115, 0.14f) : White(0.05f),
                    correct ? Green : White(0.10f), correct ? 1.5f : 1f);

                var badge = Circle(row, "Badge", 14f + 14f, 29f, 28f, correct ? Green : White(0.10f));
                TxtCentered(badge, correct ? "✓" : letters[i], 28f, 28f, 13f, W.ExtraBold,
                    correct ? TextOnGreen : White(1f), 0f, "Txt_Badge");

                var label = Txt(row, options[i], 54f, 0f, 15f, W.Bold, White(1f), 0.6f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Option");
                TopLeft(label.rectTransform, 54f, (58f - label.rectTransform.sizeDelta.y) / 2f,
                    label.rectTransform.sizeDelta.x, label.rectTransform.sizeDelta.y);

                if (correct)
                {
                    var tag = Txt(row, "CORRECT", 0f, 0f, 10f, W.ExtraBold, Green, 1f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Correct");
                    float tw = tag.rectTransform.sizeDelta.x, th = tag.rectTransform.sizeDelta.y;
                    TopLeft(tag.rectTransform, 342f - 14f - tw, (58f - th) / 2f, tw, th);
                }
                // TODO: wire answer selection (correct / incorrect feedback) when quiz logic exists.
            }

            // Feedback card (height hugs its text)
            var fb = NewRect("Panel_Feedback", c);
            var f1 = Txt(fb, "✓  Correct!", 17f, 15f, 14f, W.ExtraBold, Green, 0f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_FeedbackTitle");
            float f1h = f1.rectTransform.sizeDelta.y;
            var f2 = Txt(fb, "A resistor limits the current in the circuit, protecting the LED from too much current.",
                17f, 15f + f1h + 4f, 13f, W.Regular, TextSecondary, 0f, 310f, 19f, TextAlignmentOptions.TopLeft, "Txt_FeedbackBody");
            float fh = 15f + f1h + 4f + f2.rectTransform.sizeDelta.y + 15f;
            TopLeft(fb, 24f, 622f, 342f, fh);
            Decorate(fb, 342f, fh, 14f, Rgba(46, 213, 115, 0.08f), Rgba(46, 213, 115, 0.30f));
            fb.Find("Stroke").SetAsFirstSibling();

            var results = PrimaryButton(c, "Btn_SeeResults", "SEE RESULTS  →", 24f, 748f, 342f, 49f);
            Wire(results, ScreenNames.Result);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
