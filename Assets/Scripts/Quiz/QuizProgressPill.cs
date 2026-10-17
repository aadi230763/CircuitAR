using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.UI
{
    // The shared "QUIZ · n / 3" pill at the top of Canvas_Quiz — one instance reused across all
    // 3 questions, updated by QuizController as the student advances.
    public class QuizProgressPill : MonoBehaviour
    {
        private static readonly Color Green = new Color(46f / 255f, 213f / 255f, 117f / 255f);
        private static readonly Color Neutral = new Color(1f, 1f, 1f, 0.25f);

        public TextMeshProUGUI label;
        public Image[] segments;

        public void SetProgress(int currentIndex, int total)
        {
            if (label != null) label.text = $"QUIZ · {currentIndex + 1} / {total}";
            for (int i = 0; i < segments.Length; i++)
                if (segments[i] != null) segments[i].color = i <= currentIndex ? Green : Neutral;
        }
    }
}
