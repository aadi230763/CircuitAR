using System.Collections;
using UnityEngine;

namespace CircuitAR.AR
{
    // A component (LED, resistor) that rests in a "parts tray" beside the breadboard and tweens
    // onto its breadboard slot when placed. Poses are ARContent-local and set by
    // Tools > CircuitAR > AR > 2 — tweak them in the Inspector to adjust visually (re-running
    // menu item 2 resets them to the defaults in ARContentSceneBuilder).
    public class PlaceableObject : MonoBehaviour
    {
        public Vector3 trayLocalPosition;
        public Vector3 trayLocalEuler;
        public Vector3 slotLocalPosition;
        public Vector3 slotLocalEuler;

        public bool IsPlaced { get; private set; }

        public void ResetToTray()
        {
            IsPlaced = false;
            transform.localPosition = trayLocalPosition;
            transform.localRotation = Quaternion.Euler(trayLocalEuler);
        }

        // Run via the (always-active) CircuitDemoController's StartCoroutine, so it still works
        // while ARContent is hidden because the marker isn't tracked.
        public IEnumerator AnimateToSlot(float duration, float arcHeight)
        {
            var fromPos = transform.localPosition;
            var fromRot = transform.localRotation;
            var toRot = Quaternion.Euler(slotLocalEuler);

            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                var p = Vector3.Lerp(fromPos, slotLocalPosition, k);
                p.y += Mathf.Sin(k * Mathf.PI) * arcHeight; // lift up and over, then settle
                transform.localPosition = p;
                transform.localRotation = Quaternion.Slerp(fromRot, toRot, k);
                yield return null;
            }

            transform.localPosition = slotLocalPosition;
            transform.localRotation = toRot;
            IsPlaced = true;
        }
    }
}
