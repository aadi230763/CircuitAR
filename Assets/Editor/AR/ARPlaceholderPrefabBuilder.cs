using System.IO;
using UnityEditor;
using UnityEngine;

namespace CircuitAR.EditorAR
{
    // Phase 4 helper: builds simple primitive-based placeholder prefabs (breadboard, LED,
    // resistor, wire) — no .fbx/.obj art exists yet, so these stand in for real models.
    // Safe to re-run: overwrites the same prefab/material/texture assets each time.
    public static class ARPlaceholderPrefabBuilder
    {
        private const string PrefabDir = "Assets/Prefabs/AR/";
        private const string GenDir = "Assets/Prefabs/AR/Generated/";

        // Placeholder-pass colours. Not design tokens shared with the 2D UI kit — these are
        // just plausible electronics-component colours.
        private static readonly Color BreadboardBody = new Color(0.86f, 0.80f, 0.65f);
        private static readonly Color GhostColor = new Color(0.55f, 0.75f, 1f, 0.35f);
        private static readonly Color LedRed = new Color(0.85f, 0.12f, 0.12f);
        private static readonly Color ResistorBody = new Color(0.82f, 0.70f, 0.45f);
        private static readonly Color BandBrown = new Color(0.33f, 0.2f, 0.1f);
        private static readonly Color BandBlack = new Color(0.08f, 0.08f, 0.08f);
        private static readonly Color BandRed = new Color(0.7f, 0.1f, 0.1f);
        private static readonly Color WireColor = new Color(0.85f, 0.35f, 0.1f);

        [MenuItem("Tools/CircuitAR/AR/1 Build Placeholder Prefabs")]
        public static void Build()
        {
            EnsureFolders();

            var ghostMat = CreateMaterial("Mat_Ghost", GhostColor, transparent: true);
            var breadboardBodyMat = CreateMaterial("Mat_BreadboardBody", BreadboardBody);
            var gridMat = CreateGridOverlayMaterial();
            var ledMat = CreateMaterial("Mat_LEDPlacedRed", LedRed);
            var resistorBodyMat = CreateMaterial("Mat_ResistorBody", ResistorBody);
            var bandBrownMat = CreateMaterial("Mat_ResistorBand_Brown", BandBrown);
            var bandBlackMat = CreateMaterial("Mat_ResistorBand_Black", BandBlack);
            var bandRedMat = CreateMaterial("Mat_ResistorBand_Red", BandRed);
            var wireMat = CreateMaterial("Mat_Wire", WireColor);

            // These aren't referenced directly below but keep the "unused variable" warnings quiet
            // and make the ghost material's purpose explicit at a glance.
            _ = ghostMat;

            BuildBreadboardPrefab(breadboardBodyMat, gridMat);
            BuildLedPrefab(ledMat);
            BuildResistorPrefab(resistorBodyMat, bandBrownMat, bandBlackMat, bandRedMat);
            BuildWirePrefab(wireMat);

            AssetDatabase.SaveAssets();
            Debug.Log("[CircuitAR] Placeholder AR prefabs built in " + PrefabDir +
                       ". Next: Tools > CircuitAR > AR > 2 Build AR Content In Scene.");
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildBreadboardPrefab(Material bodyMat, Material gridMat)
        {
            var root = new GameObject("Breadboard");

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.006f, 0f);
            body.transform.localScale = new Vector3(0.16f, 0.012f, 0.07f);
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Thin quad overlay facing up, tiled with a dot-grid texture to suggest holes.
            var grid = GameObject.CreatePrimitive(PrimitiveType.Quad);
            grid.name = "GridOverlay";
            grid.transform.SetParent(root.transform, false);
            grid.transform.localPosition = new Vector3(0f, 0.0121f, 0f);
            grid.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            grid.transform.localScale = new Vector3(0.16f, 0.07f, 1f);
            var gridRenderer = grid.GetComponent<Renderer>();
            gridRenderer.sharedMaterial = gridMat;
            gridRenderer.sharedMaterial.mainTextureScale = new Vector2(10f, 5f);
            Object.DestroyImmediate(grid.GetComponent<Collider>());

            SaveAndDestroy(root, "Breadboard");
        }

        private static void BuildLedPrefab(Material ledMat)
        {
            // Root origin sits at the LED's base (on the board surface); the capsule is offset
            // up by half its own height so placing the root at board-surface height is correct.
            var root = new GameObject("LED");

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.0125f, 0f);
            body.transform.localScale = new Vector3(0.012f, 0.0125f, 0.012f);
            body.GetComponent<Renderer>().sharedMaterial = ledMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            SaveAndDestroy(root, "LED");
        }

        private static void BuildResistorPrefab(Material bodyMat, Material brownMat, Material blackMat, Material redMat)
        {
            // Root origin at the resistor's base too, lying on its side along local X.
            var root = new GameObject("Resistor");

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.006f, 0f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            body.transform.localScale = new Vector3(0.012f, 0.0175f, 0.012f);
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            AddBand(root.transform, "Band_1_Brown", -0.008f, brownMat);
            AddBand(root.transform, "Band_2_Black", 0f, blackMat);
            AddBand(root.transform, "Band_3_Red", 0.008f, redMat);

            SaveAndDestroy(root, "Resistor");
        }

        private static void AddBand(Transform parent, string name, float xOffset, Material mat)
        {
            var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = name;
            band.transform.SetParent(parent, false);
            band.transform.localPosition = new Vector3(xOffset, 0.006f, 0f);
            band.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            band.transform.localScale = new Vector3(0.0135f, 0.004f, 0.0135f);
            band.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(band.GetComponent<Collider>());
        }

        private static void BuildWirePrefab(Material wireMat)
        {
            var root = new GameObject("Wire", typeof(LineRenderer));
            var lr = root.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth = lr.endWidth = 0.004f;
            lr.numCapVertices = 4;
            lr.sharedMaterial = wireMat;
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, new Vector3(0.03f, 0.01f, 0f));

            SaveAndDestroy(root, "Wire");
        }

        private static void SaveAndDestroy(GameObject root, string prefabName)
        {
            PrefabUtility.SaveAsPrefabAsset(root, PrefabDir + prefabName + ".prefab");
            Object.DestroyImmediate(root);
        }

        // ------------------------------------------------------------------
        // Materials / textures
        // ------------------------------------------------------------------

        private static Shader FindBestShader() =>
            Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Sprites/Default");

        private static Material CreateMaterial(string name, Color color, bool transparent = false)
        {
            string path = GenDir + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(FindBestShader());
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            if (transparent) MakeTransparent(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateGridOverlayMaterial()
        {
            const string path = GenDir + "Mat_BreadboardGrid.mat";
            var tex = GenerateGridTexture();
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(FindBestShader());
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = Color.white;
            mat.mainTexture = tex;
            MakeTransparent(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void MakeTransparent(Material mat)
        {
            if (mat.shader.name != "Standard") return; // fallback shaders are left opaque
            mat.SetFloat("_Mode", 3f); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        private static Texture2D GenerateGridTexture()
        {
            const string path = GenDir + "BreadboardGrid.png";
            const int w = 64, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var bg = new Color(0f, 0f, 0f, 0f);
            var dot = new Color(0.22f, 0.16f, 0.09f, 0.9f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, (x % 4 == 2 && y % 4 == 2) ? dot : bg);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/AR")) AssetDatabase.CreateFolder("Assets/Prefabs", "AR");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/AR/Generated")) AssetDatabase.CreateFolder("Assets/Prefabs/AR", "Generated");
        }
    }
}
