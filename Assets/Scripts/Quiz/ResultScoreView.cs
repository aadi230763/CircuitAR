using TMPro;
using UnityEngine;

namespace CircuitAR.UI
{
    // Reads QuizScoreStore when Canvas_Result becomes active and overwrites the built-in preview
    // text with the real score. If the student somehow reaches Result without finishing a quiz
    // (QuizScoreStore.HasResult is false), the screen's built-in placeholder values are left as-is.
    public class ResultScoreView : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI questionsStatText;
        public TextMeshProUGUI correctStatText;
        public GameObject reviewCard;
        public TextMeshProUGUI reviewQuestionText;
        public TextMeshProUGUI reviewAnswerText;

        private void OnEnable()
        {
            if (!QuizScoreStore.HasResult) return;

            if (scoreText != null) scoreText.text = $"{QuizScoreStore.CorrectCount}/{QuizScoreStore.TotalQuestions}";
            if (questionsStatText != null) questionsStatText.text = QuizScoreStore.TotalQuestions.ToString();
            if (correctStatText != null) correctStatText.text = QuizScoreStore.CorrectCount.ToString();

            bool hasMissed = QuizScoreStore.Missed.Count > 0;
            if (reviewCard != null) reviewCard.SetActive(hasMissed);
            if (hasMissed)
            {
                var first = QuizScoreStore.Missed[0];
                if (reviewQuestionText != null) reviewQuestionText.text = first.Question;
                if (reviewAnswerText != null) reviewAnswerText.text = "Correct answer: " + first.CorrectAnswerLabel;
            }
        }
    }
}
