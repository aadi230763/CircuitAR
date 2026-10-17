using CircuitAR.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "08A — Quiz". 3 real questions, each a self-contained group (progress label,
    // question, 4 options, feedback box, its own NEXT/SEE RESULTS button) built at the same
    // Figma coordinates and toggled one at a time by QuizController — same "static content,
    // runtime show/hide" pattern as the Step 1 / Step 2-3 placement callouts.
    public static class QuizBuilder
    {
        private static readonly string[] Letters = { "A", "B", "C", "D" };

        [MenuItem("Tools/Build UI/08A Quiz")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Quiz, "UI_QuizRoot", "Plate_08A_Quiz.png", gradientBg: true);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.HowItWorks);
            Wire(IconButton(c, "Btn_Close", "×", 334f, 54f), ScreenNames.HomeStart);

            var progressPillRt = SessionPill(c, "QUIZ · 1 / 3", 188f, new[] { Accent, White(0.25f), White(0.25f) });
            var progressPill = progressPillRt.gameObject.AddComponent<QuizProgressPill>();
            progressPill.label = progressPillRt.Find("Txt_Session")?.GetComponent<TextMeshProUGUI>();
            progressPill.segments = new[]
            {
                progressPillRt.Find("Seg_1")?.GetComponent<Image>(),
                progressPillRt.Find("Seg_2")?.GetComponent<Image>(),
                progressPillRt.Find("Seg_3")?.GetComponent<Image>(),
            };

            var q1 = BuildQuestionGroup(c, 0, 3,
                "How can you visually identify the anode (positive terminal) of a standard through-hole LED?",
                new[] { "The flat edge on the plastic body.", "The longer wire lead.", "The thicker wire lead.", "The colored/tinted epoxy lens." },
                correctIndex: 1);

            var q2 = BuildQuestionGroup(c, 1, 3,
                "What is the typical forward voltage drop (Vf) for a standard bright red LED operating at regular current?",
                new[] { "0.5V", "5.0V", "2.0V", "12.0V" },
                correctIndex: 2);

            var q3 = BuildQuestionGroup(c, 2, 3,
                "You want to power a red LED (forward voltage drop of 2.0V) with a 9V battery. The LED requires 20mA (0.02A) of current. What value of series current-limiting resistor do you need?",
                new[] { "350 ohms", "180 ohms", "450 ohms", "220 ohms" },
                correctIndex: 0,
                explicitFeedback: new[]
                {
                    "Correct! Using Ohm's Law: R = (V_source - V_LED) / I = (9 - 2) / 0.02 = 7 / 0.02 = 350 Ω.",
                    "Incorrect. An 180-ohm resistor would allow too much current to flow, potentially damaging the LED. Re-check your subtraction and division.",
                    "Incorrect. 450 ohms provides a higher resistance than necessary, resulting in an overly dim LED current (~15.5mA).",
                    "Incorrect. 220 ohms is a common standard value, but it results in a higher current than the specified 20mA for a 9V supply and 2V drop.",
                });

            var controller = s.canvas.gameObject.AddComponent<QuizController>();
            controller.questionGroups = new[] { q1.root.gameObject, q2.root.gameObject, q3.root.gameObject };
            controller.questionViews = new[] { q1.view, q2.view, q3.view };
            controller.progressPill = progressPill;
            controller.screenManager = EnsureScreenManager();

            UnityEventTools.AddIntPersistentListener(q1.nextButton.onClick, controller.ShowGroup, 1);
            UnityEventTools.AddIntPersistentListener(q2.nextButton.onClick, controller.ShowGroup, 2);
            UnityEventTools.AddPersistentListener(q3.nextButton.onClick, controller.FinishQuiz);

            EditorUtility.SetDirty(controller);

            HomeIndicator(s.root);
            EndScreen(s);
        }

        private struct QuestionGroupResult
        {
            public RectTransform root;
            public QuizQuestionView view;
            public Button nextButton;
        }

        private static QuestionGroupResult BuildQuestionGroup(Transform parent, int index, int total,
            string question, string[] optionTexts, int correctIndex, string[] explicitFeedback = null)
        {
            string[] feedback = explicitFeedback ?? GenericFeedback(optionTexts, correctIndex);
            string nextLabel = index == total - 1 ? "SEE RESULTS  →" : "NEXT  →";

            var group = NewRect("Group_Q" + (index + 1), parent);
            Fill(group);

            Txt(group, $"QUESTION {index + 1} OF {total}", 24f, 170f, 12f, W.ExtraBold, Accent, 1.44f, 0f, 0f,
                TextAlignmentOptions.TopLeft, "Txt_QuestionLabel");
            var questionTxt = Txt(group, question, 24f, 192f, 20f, W.ExtraBold, White(1f), 0f, 342f, 26f,
                TextAlignmentOptions.TopLeft, "Txt_Question");
            float y = 192f + questionTxt.rectTransform.sizeDelta.y + 24f;

            var view = group.gameObject.AddComponent<QuizQuestionView>();
            view.correctIndex = correctIndex;
            view.perOptionFeedback = feedback;
            view.questionText = question;
            view.correctAnswerLabel = $"{Letters[correctIndex]}. {optionTexts[correctIndex]}";
            view.options = new QuizQuestionView.OptionRef[optionTexts.Length];

            for (int i = 0; i < optionTexts.Length; i++)
            {
                var row = Box(group, "Option_" + Letters[i], 24f, y, 342f, 58f, 14f, White(0.05f), White(0.10f));

                var badge = Circle(row, "Badge", 14f + 14f, 29f, 28f, White(0.10f));
                TxtCentered(badge, Letters[i], 28f, 28f, 13f, W.ExtraBold, White(1f), 0f, "Txt_Badge");

                var label = Txt(row, optionTexts[i], 54f, 0f, 15f, W.Bold, White(1f), 0.6f, 272f, 0f,
                    TextAlignmentOptions.TopLeft, "Txt_Option");
                TopLeft(label.rectTransform, 54f, (58f - label.rectTransform.sizeDelta.y) / 2f,
                    label.rectTransform.sizeDelta.x, label.rectTransform.sizeDelta.y);

                var button = MakeButton(row);
                int captured = i;
                UnityEventTools.AddIntPersistentListener(button.onClick, view.SelectOption, captured);

                view.options[i] = new QuizQuestionView.OptionRef
                {
                    button = button,
                    fill = row.GetComponent<Image>(),
                    stroke = row.Find("Stroke")?.GetComponent<Image>(),
                };
                y += 58f + 10f;
            }
            y += 10f; // extra gap before the feedback box

            // Fixed box height = the tallest of this question's 4 possible feedback strings, so the
            // box never overflows regardless of which option the student taps.
            float feedbackH = MeasureMaxFeedbackHeight(group, feedback, correctIndex);
            var feedbackBox = NewRect("Panel_Feedback", group);
            TopLeft(feedbackBox, 24f, y, 342f, feedbackH);
            Decorate(feedbackBox, 342f, feedbackH, 14f, White(0.05f), White(0.10f));
            feedbackBox.Find("Stroke").SetAsFirstSibling();
            var feedbackText = Txt(feedbackBox, "", 15f, 13f, 13f, W.Regular, White(1f), 0f, 312f, 19f,
                TextAlignmentOptions.TopLeft, "Txt_Feedback");
            feedbackBox.gameObject.SetActive(false);

            var nextButton = PrimaryButton(group, "Btn_Next", nextLabel, 24f, 748f, 342f, 49f);
            nextButton.gameObject.SetActive(false);

            view.feedbackBox = feedbackBox.gameObject;
            view.feedbackFill = feedbackBox.GetComponent<Image>();
            view.feedbackStroke = feedbackBox.Find("Stroke")?.GetComponent<Image>();
            view.feedbackText = feedbackText;
            view.nextButtonObject = nextButton.gameObject;

            group.gameObject.SetActive(false); // QuizController.ResetQuiz() shows only the first group
            return new QuestionGroupResult { root = group, view = view, nextButton = nextButton };
        }

        // "Correct!" / "Incorrect. The right answer is X — <text>." — used for Q1/Q2, which didn't
        // come with per-option explanations (kept brief per the brief).
        private static string[] GenericFeedback(string[] optionTexts, int correctIndex)
        {
            var result = new string[optionTexts.Length];
            for (int i = 0; i < optionTexts.Length; i++)
                result[i] = i == correctIndex
                    ? "Correct!"
                    : $"Incorrect. The right answer is {Letters[correctIndex]} — {optionTexts[correctIndex]}";
            return result;
        }

        private static float MeasureMaxFeedbackHeight(Transform scratchParent, string[] feedback, int correctIndex)
        {
            float maxH = 0f;
            for (int i = 0; i < feedback.Length; i++)
            {
                string full = (i == correctIndex ? "✓  " : "✗  ") + feedback[i];
                var t = Txt(scratchParent, full, 0f, 0f, 13f, W.Regular, White(1f), 0f, 312f, 19f,
                    TextAlignmentOptions.TopLeft, "Scratch");
                maxH = Mathf.Max(maxH, t.rectTransform.sizeDelta.y);
                Object.DestroyImmediate(t.gameObject);
            }
            return maxH + 26f; // top/bottom padding to match the old static feedback card
        }
    }
}
