using UnityEngine;

namespace CircuitAR.UI
{
    // Orchestrates the 3 question groups built by QuizBuilder: shows one at a time, resets the
    // whole quiz whenever Canvas_Quiz becomes active, and hands the final score off to
    // QuizScoreStore before navigating to the Result screen.
    public class QuizController : MonoBehaviour
    {
        private const string ResultScreenName = "Canvas_Result"; // must match ScreenNames.Result

        public GameObject[] questionGroups;
        public QuizQuestionView[] questionViews;
        public QuizProgressPill progressPill;
        public UIScreenManager screenManager;

        private void OnEnable()
        {
            ResetQuiz();
        }

        public void ResetQuiz()
        {
            foreach (var v in questionViews)
                if (v != null) v.ResetState();
            ShowGroup(0);
        }

        // Wired to each question's own NEXT button (for questions before the last one).
        public void ShowGroup(int index)
        {
            for (int i = 0; i < questionGroups.Length; i++)
                if (questionGroups[i] != null) questionGroups[i].SetActive(i == index);
            if (progressPill != null) progressPill.SetProgress(index, questionGroups.Length);
        }

        // Wired to the last question's SEE RESULTS button.
        public void FinishQuiz()
        {
            QuizScoreStore.Reset();
            foreach (var v in questionViews)
            {
                if (v == null) continue;
                if (v.Answered && v.WasCorrect) QuizScoreStore.CorrectCount++;
                else QuizScoreStore.Missed.Add(new QuizScoreStore.MissedQuestion
                {
                    Question = v.questionText,
                    CorrectAnswerLabel = v.correctAnswerLabel
                });
            }
            QuizScoreStore.TotalQuestions = questionViews.Length;
            QuizScoreStore.HasResult = true;

            if (screenManager == null)
            {
                Debug.LogWarning("[CircuitAR] QuizController.FinishQuiz(): no UIScreenManager assigned — can't show the Result screen.");
                return;
            }
            screenManager.ShowScreen(ResultScreenName);
        }
    }
}
