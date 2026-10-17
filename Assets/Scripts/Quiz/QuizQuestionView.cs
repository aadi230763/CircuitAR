using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.UI
{
    // One question's options + feedback, built by QuizBuilder. Tapping an option (wired to
    // SelectOption(index) at build time) locks the question, colours the tapped/correct rows,
    // shows that option's feedback text, and reveals this question's own NEXT/SEE RESULTS button.
    public class QuizQuestionView : MonoBehaviour
    {
        [Serializable]
        public class OptionRef
        {
            public Button button;
            public Image fill;
            public Image stroke;
        }

        // Matches UIStyleConstants.Green (#2ED573) / .Red (#FF6B5E) — kept as plain constants here
        // since this runtime assembly doesn't reference the editor-only UIStyleConstants class.
        private static readonly Color Green = new Color(46f / 255f, 213f / 255f, 117f / 255f);
        private static readonly Color Red = new Color(1f, 107f / 255f, 94f / 255f);
        private static readonly Color NeutralFill = new Color(1f, 1f, 1f, 0.05f);
        private static readonly Color NeutralStroke = new Color(1f, 1f, 1f, 0.10f);

        public OptionRef[] options = new OptionRef[4];
        public int correctIndex;
        // Index-aligned with options — shown verbatim (with a ✓/✗ prefix) once that option is tapped.
        public string[] perOptionFeedback = new string[4];

        // Used by Canvas_Result's review card if this question is missed.
        public string questionText;
        public string correctAnswerLabel;

        public GameObject feedbackBox;
        public Image feedbackFill;
        public Image feedbackStroke;
        public TextMeshProUGUI feedbackText;
        public GameObject nextButtonObject;

        public bool Answered { get; private set; }
        public bool WasCorrect { get; private set; }

        public void ResetState()
        {
            Answered = false;
            WasCorrect = false;
            foreach (var o in options)
            {
                if (o == null) continue;
                if (o.button != null) o.button.interactable = true;
                if (o.fill != null) o.fill.color = NeutralFill;
                if (o.stroke != null) o.stroke.color = NeutralStroke;
            }
            if (feedbackBox != null) feedbackBox.SetActive(false);
            if (nextButtonObject != null) nextButtonObject.SetActive(false);
        }

        // Wired directly to each option Button's onClick via UnityEventTools.AddIntPersistentListener.
        public void SelectOption(int index)
        {
            if (Answered) return;
            Answered = true;
            WasCorrect = index == correctIndex;

            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == null) continue;
                if (options[i].button != null) options[i].button.interactable = false;

                Color? c = i == correctIndex ? Green : (i == index ? Red : (Color?)null);
                if (c.HasValue)
                {
                    if (options[i].fill != null) options[i].fill.color = new Color(c.Value.r, c.Value.g, c.Value.b, 0.14f);
                    if (options[i].stroke != null) options[i].stroke.color = c.Value;
                }
            }

            var feedbackColor = WasCorrect ? Green : Red;
            if (feedbackText != null)
                feedbackText.text = (WasCorrect ? "✓  " : "✗  ") + perOptionFeedback[index];
            if (feedbackFill != null) feedbackFill.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 0.08f);
            if (feedbackStroke != null) feedbackStroke.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 0.30f);
            if (feedbackBox != null) feedbackBox.SetActive(true);
            if (nextButtonObject != null) nextButtonObject.SetActive(true);
        }
    }
}
