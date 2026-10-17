using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.UI
{
    // Tappable instruction callout ("PLACE HERE · C14–C15"). Built by UIBuilderCommon.Callout;
    // the world anchor and click target are wired by Tools > CircuitAR > AR > 3 Wire Circuit Demo.
    // While the marker is tracked it floats above its breadboard slot; otherwise it sits at its
    // Figma position so the flow can still be tested without a marker.
    public class PlacementCallout : MonoBehaviour
    {
        public Button button;
        public TextMeshProUGUI label;
        public Image fill;
        public Image stroke;
        public string idleText;
        public string confirmText = "✓ PLACED";
        public Color idleTextColor = Color.white;
        public Color idleFillColor;
        public Color idleStrokeColor;
        public Vector2 defaultAnchoredPosition;

        public Transform worldAnchor;
        public float gapPx = 10f;
        public float pulseAmount = 0.06f;
        public float pulseSpeed = 4f;

        private static readonly Color Green = new Color(46f / 255f, 213f / 255f, 117f / 255f);

        private Camera cam;
        private bool confirmed;
        private float confirmedAt;

        public void ResetState()
        {
            confirmed = false;
            if (label != null)
            {
                label.text = idleText;
                label.color = idleTextColor;
            }
            if (fill != null) fill.color = idleFillColor;
            if (stroke != null) stroke.color = idleStrokeColor;
            if (button != null) button.interactable = true;
            transform.localScale = Vector3.one;
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }

        public void SetBusy()
        {
            if (button != null) button.interactable = false;
            transform.localScale = Vector3.one;
        }

        public void ShowConfirmed()
        {
            confirmed = true;
            confirmedAt = Time.unscaledTime;
            if (label != null)
            {
                label.text = confirmText;
                label.color = Green;
                label.alignment = TextAlignmentOptions.Center;
            }
            if (fill != null) fill.color = new Color(Green.r, Green.g, Green.b, 0.25f);
            if (stroke != null) stroke.color = new Color(Green.r, Green.g, Green.b, 0.9f);
        }

        private void LateUpdate()
        {
            var rt = (RectTransform)transform;

            if (confirmed)
            {
                float k = Mathf.Clamp01(1f - (Time.unscaledTime - confirmedAt) / 0.25f);
                rt.localScale = Vector3.one * (1f + 0.15f * k); // quick "pop" on confirm
            }
            else if (button != null && button.interactable)
            {
                rt.localScale = Vector3.one * (1f + pulseAmount * Mathf.Sin(Time.unscaledTime * pulseSpeed));
            }

            if (TryProjectAnchor(rt, out var screenPos)) rt.position = screenPos;
            else rt.anchoredPosition = defaultAnchoredPosition;
        }

        // Screen Space Overlay: a RectTransform's world position is its screen pixel position.
        private bool TryProjectAnchor(RectTransform rt, out Vector3 screenPos)
        {
            screenPos = default;
            if (worldAnchor == null || !worldAnchor.gameObject.activeInHierarchy) return false;
            if (cam == null) cam = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            if (cam == null) return false;

            var sp = cam.WorldToScreenPoint(worldAnchor.position);
            if (sp.z <= 0f) return false;

            float canvasScale = rt.parent != null ? rt.parent.lossyScale.y : 1f;
            float halfH = rt.rect.height * 0.5f * canvasScale;
            screenPos = new Vector3(sp.x, sp.y + halfH + gapPx * canvasScale, 0f);
            return true;
        }
    }
}
