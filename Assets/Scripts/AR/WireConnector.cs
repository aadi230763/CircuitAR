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
        public Color pulseColor = new Color(1f, 0.9f, 0.45f);

        public bool IsPlaced { get; private set; }

        public void ResetToTray()
        {
            IsPlaced = false;
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

            for (float t = 0f; t < drawDuration; t += Time.deltaTime)
            {
                SetProgress(t / drawDuration);
                yield return null;
            }
            SetProgress(1f);

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

        // Points past the drawn "head" collapse onto it, so the line grows from start to end.
        private void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            for (int i = 0; i <= segments; i++)
                line.SetPosition(i, ArcPoint(Mathf.Min((float)i / segments, progress)));
        }
    }
}
