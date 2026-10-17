using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.UI
{
    // The "SEARCHING FOR MARKER…" pill on the AR Camera screen (built by ARCameraBuilder).
    // Driven by real Vuforia detection via CircuitDemoController — not by a tap.
    public class DetectionStatusPill : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Image icon;

        private const string SearchingText = "SEARCHING FOR MARKER…";
        private const string FoundText = "MARKER FOUND";
        private static readonly Color Amber = new Color(255f / 255f, 176f / 255f, 32f / 255f);
        private static readonly Color Green = new Color(46f / 255f, 213f / 255f, 117f / 255f);
        private static readonly Color TextLightBlue = new Color(140f / 255f, 196f / 255f, 255f / 255f);

        private Coroutine pulseRoutine;

        public void SetSearching()
        {
            if (pulseRoutine != null) { StopCoroutine(pulseRoutine); pulseRoutine = null; }
            transform.localScale = Vector3.one;
            if (label != null) { label.text = SearchingText; label.color = TextLightBlue; }
            if (icon != null) icon.color = Amber;
        }

        // A brief colour flip + scale pulse, held for `holdSeconds` — called once per real
        // detection by CircuitDemoController, which times the screen transition separately.
        public void PlayFoundAnimation(float holdSeconds)
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(FoundRoutine(holdSeconds));
        }

        private IEnumerator FoundRoutine(float holdSeconds)
        {
            if (label != null) { label.text = FoundText; label.color = Green; }
            if (icon != null) icon.color = Green;

            const float pulseDuration = 0.25f;
            for (float t = 0f; t < pulseDuration; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / pulseDuration * Mathf.PI);
                transform.localScale = Vector3.one * (1f + 0.18f * k);
                yield return null;
            }
            transform.localScale = Vector3.one;

            yield return new WaitForSeconds(Mathf.Max(0f, holdSeconds - pulseDuration));
            pulseRoutine = null;
        }
    }
}
