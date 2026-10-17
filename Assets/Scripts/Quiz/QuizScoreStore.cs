using System.Collections.Generic;

namespace CircuitAR.UI
{
    // Simple hand-off between Canvas_Quiz and Canvas_Result — set once by QuizController.FinishQuiz()
    // when the student finishes the 3 questions, read by ResultScoreView when the Result screen shows.
    public static class QuizScoreStore
    {
        public struct MissedQuestion
        {
            public string Question;
            public string CorrectAnswerLabel; // e.g. "B. The longer wire lead."
        }

        public static int CorrectCount;
        public static int TotalQuestions = 3;
        public static readonly List<MissedQuestion> Missed = new List<MissedQuestion>();
        public static bool HasResult; // false until a quiz attempt has actually finished

        public static void Reset()
        {
            CorrectCount = 0;
            Missed.Clear();
            HasResult = false;
        }
    }
}
