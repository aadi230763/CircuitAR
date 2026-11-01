using System.Collections;
using UnityEngine;

namespace CircuitAR.AR
{
    // Jumper wire: a loose piece sits in the parts tray; when connected, the real wire draws
    // itself along an arc from startLocal to endLocal, then a brightness/width pulse runs once
    // to read as "current connected". Points are ARContent-local (the wire sits at its origin).
    public class WireConnector : MonoBehaviour
    {
        public LineRenderer line;
        public GameObject trayPiece;
        public Vector3 startLocal;
        public Vector3 endLocal;
        public float arcHeight = 0.018f;
        public int segments = 20;
        public Color pulseColor = Color.red;

        private Transform startConnector;
        private Transform endConnector;

        public bool IsPlaced { get; private set; }

        private void Awake()
        {
            CacheConnectorVisuals();
        }

        public void ResetToTray()
        {
            IsPlaced = false;
            CacheConnectorVisuals();
            if (startConnector != null) startConnector.gameObject.SetActive(false);
            if (endConnector != null) endConnector.gameObject.SetActive(false);
            if (trayPiece != null) trayPiece.SetActive(true);
            if (line != null)
            {
                line.enabled = false;
                line.widthMultiplier = 1f;
            }
        }

        public IEnumerator AnimateConnect(float drawDuration, float pulseDuration)
        {
            if (trayPiece != null) trayPiece.SetActive(false);
            line.useWorldSpace = false;
            line.positionCount = segments + 1;
            SetProgress(0f);
            line.enabled = true;
            CacheConnectorVisuals();
            PositionConnectorVisuals();
            if (startConnector != null) startConnector.gameObject.SetActive(true);
            if (endConnector != null) endConnector.gameObject.SetActive(false);

            for (float t = 0f; t < drawDuration; t += Time.deltaTime)
            {
                SetProgress(t / drawDuration);
                yield return null;
            }
            SetProgress(1f);
            if (endConnector != null) endConnector.gameObject.SetActive(true);

            var mat = line.material; // per-instance copy, so the pulse doesn't edit the shared asset
            var baseColor = mat.color;
            for (float t = 0f; t < pulseDuration; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / pulseDuration * Mathf.PI);
                line.widthMultiplier = 1f + 0.8f * k;
                mat.color = Color.Lerp(baseColor, pulseColor, k);
                yield return null;
            }
            line.widthMultiplier = 1f;
            mat.color = baseColor;
            IsPlaced = true;
        }

        public Vector3 ArcPoint(float u)
        {
            var p = Vector3.Lerp(startLocal, endLocal, u);
            p.y += Mathf.Sin(u * Mathf.PI) * arcHeight;
            return p;
        }

        private void CacheConnectorVisuals()
        {
            if (startConnector == null) startConnector = transform.Find("Connector_Start");
            if (endConnector == null) endConnector = transform.Find("Connector_End");
        }

        private void PositionConnectorVisuals()
        {
            Vector3 forward = ArcPoint(0.02f) - ArcPoint(0f);
            Vector3 backward = ArcPoint(0.98f) - ArcPoint(1f);
            if (forward.sqrMagnitude < 0.000001f) forward = endLocal - startLocal;
            if (backward.sqrMagnitude < 0.000001f) backward = startLocal - endLocal;
            if (forward.sqrMagnitude < 0.000001f) forward = Vector3.forward;
            if (backward.sqrMagnitude < 0.000001f) backward = Vector3.back;

            if (startConnector != null)
            {
                startConnector.localPosition = ArcPoint(0f);
                startConnector.localRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            }
            if (endConnector != null)
            {
                endConnector.localPosition = ArcPoint(1f);
                endConnector.localRotation = Quaternion.LookRotation(backward.normalized, Vector3.up);
            }
        }

        // Points past the drawn "head" collapse onto it, so the line grows from start to end.
        private void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            for (int i = 0; i <= segments; i++)
                line.SetPosition(i, ArcPoint(Mathf.Min((float)i / segments, progress)));
        }
    }
}
