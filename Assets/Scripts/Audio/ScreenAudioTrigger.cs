using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CircuitAR.Audio
{
    // Fires onShow (after an optional delay) every time this GameObject becomes active, and
    // onHide when it's deactivated. Attach to a screen's Canvas root so it fires each time
    // UIScreenManager shows/hides that screen, or to any other object whose own active state
    // marks a sub-step (e.g. a PlacementCallout becoming visible) — same pattern this project
    // already uses throughout (UIScreenManager, QuizController, CircuitDemoController itself).
    // Wired by Tools > CircuitAR > AR > 3 Wire Circuit Demo.
    public class ScreenAudioTrigger : MonoBehaviour
    {
        public float delaySeconds;
        // Must be initialized inline: unlike Unity's own components (e.g. Button.onClick), a
        // plain UnityEvent field on a MonoBehaviour added via AddComponent() at editor-script
        // time stays C# null until the object is serialized — accessing it before that (as
        // CircuitDemoWiring's ClearPersistentListeners does) throws a NullReferenceException.
        public UnityEvent onShow = new UnityEvent();
        public UnityEvent onHide = new UnityEvent();

        private Coroutine pending;

        private void OnEnable()
        {
            pending = StartCoroutine(FireAfterDelay());
        }

        private void OnDisable()
        {
            if (pending != null)
            {
                StopCoroutine(pending);
                pending = null;
            }
            onHide?.Invoke();
        }

        private IEnumerator FireAfterDelay()
        {
            if (delaySeconds > 0f) yield return new WaitForSeconds(delaySeconds);
            onShow?.Invoke();
        }
    }
}
