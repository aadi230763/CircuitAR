using UnityEngine;

namespace CircuitAR.AR
{
    // Turns an LED's own material red + emissive once its circuit completes, and back off on
    // reset. Added to each LED GameObject by ARContentSceneBuilder (Tools > CircuitAR > AR > 2);
    // SetOn(true)/SetOn(false) is called by the owning controller's FinishCircuit()/ResetCircuit().
    // Uses Renderer.material (an automatic per-instance copy) rather than sharedMaterial, so
    // lighting up one LED never affects any other LED instance built from the same prefab.
    public class LedGlow : MonoBehaviour
    {
        public Color onColor = new Color(1f, 0.05f, 0.05f);
        public float emissionIntensity = 3f;

        private Renderer[] renderers;
        private Color[] originalColors;
        private bool capturedOriginal;

        public void SetOn(bool on)
        {
            if (renderers == null) renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            if (!capturedOriginal)
            {
                originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                    originalColors[i] = ReadBaseColor(renderers[i].material);
                capturedOriginal = true;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                var mat = renderers[i].material; // per-instance copy, never touches the shared asset
                if (mat.HasProperty("_EmissionColor"))
                {
                    if (on) mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", on ? onColor * emissionIntensity : Color.black);
                }
                SetBaseColor(mat, on ? onColor : originalColors[i]);
            }
        }

        // glTFast can generate either a Standard-shader-style material (_Color) or its own PBR
        // shader (_BaseColor/baseColorFactor) depending on import settings — try the common ones
        // rather than assuming which this particular LED material ended up with.
        private static Color ReadBaseColor(Material mat)
        {
            if (mat.HasProperty("_Color")) return mat.GetColor("_Color");
            if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
            if (mat.HasProperty("baseColorFactor")) return mat.GetColor("baseColorFactor");
            return Color.white;
        }

        private static void SetBaseColor(Material mat, Color color)
        {
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("baseColorFactor")) mat.SetColor("baseColorFactor", color);
        }
    }
}
