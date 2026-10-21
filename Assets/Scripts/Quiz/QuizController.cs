using CircuitAR.AR;
using UnityEngine;

namespace CircuitAR.UI
{
    // Orchestrates all 3 modes' question sets (9 groups total, built by QuizBuilder under one
    // Canvas_Quiz): picks whichever set matches CircuitModeState.Active, shows one group of that
    // set at a time, resets the whole quiz whenever Canvas_Quiz becomes active, and hands the
    // final score off to QuizScoreStore before navigating to the Result screen.
    public class QuizController : MonoBehaviour
    {
        private const string ResultScreenName = "Canvas_Result"; // must match ScreenNames.Result

        [System.Serializable]
        public class QuestionSet
        {
            public GameObject[] questionGroups;
            public QuizQuestionView[] questionViews;
        }

        public QuestionSet ledQuestions;
        public QuestionSet seriesQuestions;
        public QuestionSet parallelQuestions;
        public QuizProgressPill progressPill;
        public UIScreenManager screenManager;

        private QuestionSet active;

        private void OnEnable()
        {
            ResetQuiz();
        }

        public void ResetQuiz()
        {
            active = CircuitModeState.Active switch
            {
                CircuitMode.Series => seriesQuestions,
                CircuitMode.Parallel => parallelQuestions,
                _ => ledQuestions,
            };

            // Hide every group of every set first, so switching modes never leaves a stale
            // question from a previous mode visible underneath the active set.
            HideAll(ledQuestions);
            HideAll(seriesQuestions);
            HideAll(parallelQuestions);

            if (active?.questionViews == null) return;
            foreach (var v in active.questionViews)
                if (v != null) v.ResetState();
            ShowGroup(0);
        }

        private static void HideAll(QuestionSet set)
        {
            if (set?.questionGroups == null) return;
            foreach (var g in set.questionGroups)
                if (g != null) g.SetActive(false);
        }

        // Wired to each question's own NEXT button (for questions before the last one).
        public void ShowGroup(int index)
        {
            if (active?.questionGroups == null) return;
            for (int i = 0; i < active.questionGroups.Length; i++)
                if (active.questionGroups[i] != null) active.questionGroups[i].SetActive(i == index);
            if (progressPill != null) progressPill.SetProgress(index, active.questionGroups.Length);
        }

        // Wired to the last question's SEE RESULTS button.
        public void FinishQuiz()
        {
            if (active?.questionViews == null) return;

            QuizScoreStore.Reset();
            foreach (var v in active.questionViews)
            {
                if (v == null) continue;
                if (v.Answered && v.WasCorrect) QuizScoreStore.CorrectCount++;
                else QuizScoreStore.Missed.Add(new QuizScoreStore.MissedQuestion
                {
                    Question = v.questionText,
                    CorrectAnswerLabel = v.correctAnswerLabel
                });
            }
            QuizScoreStore.TotalQuestions = active.questionViews.Length;
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
