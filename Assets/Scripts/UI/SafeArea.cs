using UnityEngine;

namespace CircuitAR.UI
{
    // Shrinks this RectTransform's anchors to Screen.safeArea. The Figma frames already include
    // their own status-bar band, so the top inset is ignored by default (applyTop = false).
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool applyTop = false;
        [SerializeField] private bool applyBottom = true;

        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            Apply();
        }

        private void Apply()
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();

            var safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == _lastSafeArea && screenSize == _lastScreenSize) return;

            _lastSafeArea = safeArea;
            _lastScreenSize = screenSize;
            if (screenSize.x <= 0 || screenSize.y <= 0) return;

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;

            if (!applyTop) anchorMax.y = 1f;
            if (!applyBottom) anchorMin.y = 0f;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
        }
    }
}
