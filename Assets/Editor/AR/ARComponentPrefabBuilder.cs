using UnityEditor;
using UnityEngine;

namespace CircuitAR.EditorAR
{
    // Builds the breadboard, LED, and resistor prefabs by extracting those three named nodes out
    // of the combined Sketchfab asset (Assets/Models/ElectronicsSet/*.glb — a full electronics
    // kit with many components) and discarding everything else. Each is fitted by measuring its
    // real mesh bounds after import, then positioned/scaled to the SAME root-pivot convention the
    // old primitive prefabs used — root origin at the breadboard's bottom-centre, and at the LED's
    // / resistor's base — so ARContentSceneBuilder's tray/slot poses and the placement animation
    // system work completely unchanged. The wire stays a LineRenderer prefab (no model involved).
    // Safe to re-run — overwrites the same prefab/material assets each time.
    public static class ARComponentPrefabBuilder
    {
        private const string PrefabDir = "Assets/Prefabs/AR/";
        private const string GenDir = "Assets/Prefabs/AR/Generated/";
        private const string SourceAssetPath = "Assets/Models/ElectronicsSet/cc0_set_of_electronic_components.glb";

        // DECISION: node names as authored in the source asset — see the glTF inspection in chat.
        private const string BreadboardNodeName = "Breadboard_100";
        private const string LedNodeName = "Diode-Light-Emitting_2";
        private const string ResistorNodeName = "Resistor-Axial Blue_9"; // picked over "Resistor-Axial_30"

        // Target footprint (metres) — unchanged from the old primitive prefabs, so every existing
        // tray/slot position and the fixed wire endpoints in ARContentSceneBuilder still line up.
        private const float BoardWidth = 0.16f;
        private const float BoardDepth = 0.07f;
        private const float BoardThickness = 0.012f;
        private const float LedTotalHeight = 0.025f;      // matches the old capsule's total height
        private const float ResistorTotalLength = 0.035f; // matches the old cylinder body length

        private static readonly Color WireColor = new Color(0.85f, 0.35f, 0.1f);

        // Battery: no battery exists in the source asset, so this stays primitive-built (same
        // approach the wire already uses). Proportioned like a real 9V: ~22mm x 35mm x 14mm.
        private const float BatteryWidth = 0.022f;
        private const float BatteryHeight = 0.035f;
        private const float BatteryDepth = 0.014f;
        private static readonly Color BatteryBodyColor = new Color(0.08f, 0.07f, 0.07f);
        private static readonly Color BatteryCapColor = new Color(0.65f, 0.63f, 0.6f);
        private static readonly Color BatteryTerminalColor = new Color(0.75f, 0.73f, 0.65f);

        [MenuItem("Tools/CircuitAR/AR/1 Build Component Prefabs")]
        public static void Build()
        {
            var glbAsset = AssetDatabase.LoadAssetAtPath<GameObject>(SourceAssetPath);
            if (glbAsset == null)
            {
                EditorUtility.DisplayDialog("Source model not found",
                    "Couldn't load " + SourceAssetPath + ". Check the file is imported and the path is correct.", "OK");
                return;
            }

            EnsureFolders();

            BuildBreadboardPrefab(glbAsset);
            BuildLedPrefab(glbAsset);
            BuildResistorPrefab(glbAsset);
            BuildWirePrefab(CreateSimpleMaterial("Mat_Wire", WireColor));
            BuildBatteryPrefab();

            AssetDatabase.SaveAssets();
            Debug.Log("[CircuitAR] Component prefabs built from " + SourceAssetPath + " into " + PrefabDir +
                       ". Next: Tools > CircuitAR > AR > 2 Build AR Content In Scene.");
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildBreadboardPrefab(GameObject glbAsset)
        {
            var root = new GameObject("Breadboard");
            // Non-uniform scale lives on "Fit" (unrotated), not on the rotated model, so it
            // stretches only along the board's own width/depth axes without skewing it.
            var fit = new GameObject("Fit").transform;
            fit.SetParent(root.transform, false);

            var model = InstantiateNode(glbAsset, BreadboardNodeName, fit);
            if (model == null) { Object.DestroyImmediate(root); return; }

            // DECISION (per chat): the source board's real proportions don't match the old
            // footprint — rotated 90°, it measures ~8.1cm x 5.42cm vs the target 16cm x 7cm.
            // "Mild stretch" was chosen: keep the exact old footprint/tray layout, accept a
            // ~14% non-uniform stretch rather than resize everything around a bigger/smaller board.
            model.localRotation = Quaternion.Euler(0f, 90f, 0f);
            var b = LocalBounds(fit, model.GetComponentsInChildren<Renderer>());
            model.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);

            fit.localScale = new Vector3(BoardWidth / b.size.x, BoardThickness / b.size.y, BoardDepth / b.size.z);

            Debug.Log($"[CircuitAR] Breadboard fitted to {BoardWidth:0.000}m x {BoardThickness:0.000}m x {BoardDepth:0.000}m " +
                       $"(stretch: x×{BoardWidth / b.size.x:0.00}, y×{BoardThickness / b.size.y:0.00}, z×{BoardDepth / b.size.z:0.00}).");
            SaveAndDestroy(root, "Breadboard");
        }

        private static void BuildLedPrefab(GameObject glbAsset)
        {
            var root = new GameObject("LED");
            var model = InstantiateNode(glbAsset, LedNodeName, root.transform);
            if (model == null) { Object.DestroyImmediate(root); return; }

            var renderers = model.GetComponentsInChildren<Renderer>();

            // DECISION (per chat): scaling the WHOLE node (dome + legs together) to match the old
            // placeholder's total height — rather than matching the old 12mm body diameter — since
            // this model's legs are proportionally much longer than the old capsule's; matching
            // diameter would make the LED float ~5cm above the board. This lands the body around a
            // realistic ~5mm diameter, close to a real LED anyway.
            var b = LocalBounds(root.transform, renderers);
            model.localScale = Vector3.one * (LedTotalHeight / b.size.y);

            b = LocalBounds(root.transform, renderers); // re-measure at the new scale
            model.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);

            Debug.Log($"[CircuitAR] LED fitted to {LedTotalHeight:0.000}m tall (body ends up ~{b.size.x * 1000f:0.0}mm across).");
            SaveAndDestroy(root, "LED");
        }

        private static void BuildResistorPrefab(GameObject glbAsset)
        {
            var root = new GameObject("Resistor");
            var model = InstantiateNode(glbAsset, ResistorNodeName, root.transform);
            if (model == null) { Object.DestroyImmediate(root); return; }

            var renderers = model.GetComponentsInChildren<Renderer>();

            // The source resistor is authored standing upright; rotate whichever axis measures
            // longest onto local X, matching how ARContentSceneBuilder places this prefab with no
            // extra slot rotation (it assumes the prefab already lies flat along X).
            var b = LocalBounds(root.transform, renderers);
            if (b.size.y >= b.size.x && b.size.y >= b.size.z)
                model.localRotation = Quaternion.Euler(0f, 0f, 90f);
            else if (b.size.z >= b.size.x && b.size.z >= b.size.y)
                model.localRotation = Quaternion.Euler(0f, 90f, 0f);

            b = LocalBounds(root.transform, renderers);
            model.localScale = Vector3.one * (ResistorTotalLength / b.size.x);

            b = LocalBounds(root.transform, renderers); // re-measure at the new scale/rotation
            model.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);

            Debug.Log($"[CircuitAR] Resistor fitted to {ResistorTotalLength:0.000}m long " +
                       $"(body ends up ~{b.size.z * 1000f:0.0}mm / {b.size.y * 1000f:0.0}mm across).");
            SaveAndDestroy(root, "Resistor");
        }

        // Unchanged from the placeholder pass: the wire stays a LineRenderer (no wire model needed
        // or used from the source asset).
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

        // Simple primitive 9V-style battery (Series/Parallel circuits): a dark body, a lighter cap,
        // and two small terminal nubs. Root origin at the base, matching every other component.
        private static void BuildBatteryPrefab()
        {
            var root = new GameObject("Battery");
            var bodyMat = CreateSimpleMaterial("Mat_BatteryBody", BatteryBodyColor);
            var capMat = CreateSimpleMaterial("Mat_BatteryCap", BatteryCapColor);
            var terminalMat = CreateSimpleMaterial("Mat_BatteryTerminal", BatteryTerminalColor);

            float capHeight = BatteryHeight * 0.15f;
            float bodyHeight = BatteryHeight - capHeight;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, bodyHeight / 2f, 0f);
            body.transform.localScale = new Vector3(BatteryWidth, bodyHeight, BatteryDepth);
            body.GetComponent<Renderer>().sharedMaterial = bodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "Cap";
            cap.transform.SetParent(root.transform, false);
            cap.transform.localPosition = new Vector3(0f, bodyHeight + capHeight / 2f, 0f);
            cap.transform.localScale = new Vector3(BatteryWidth * 0.9f, capHeight, BatteryDepth * 0.9f);
            cap.GetComponent<Renderer>().sharedMaterial = capMat;
            Object.DestroyImmediate(cap.GetComponent<Collider>());

            float terminalD = BatteryWidth * 0.18f;
            float terminalH = capHeight * 0.6f;
            AddTerminal(root.transform, "Terminal_Positive", -BatteryWidth * 0.22f, BatteryHeight, terminalD, terminalH, terminalMat);
            AddTerminal(root.transform, "Terminal_Negative", BatteryWidth * 0.22f, BatteryHeight, terminalD, terminalH, terminalMat);

            SaveAndDestroy(root, "Battery");
        }

        private static void AddTerminal(Transform parent, string name, float xOffset, float baseY, float diameter, float height, Material mat)
        {
            var t = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            t.name = name;
            t.transform.SetParent(parent, false);
            t.transform.localPosition = new Vector3(xOffset, baseY + height / 2f, 0f);
            t.transform.localScale = new Vector3(diameter, height / 2f, diameter); // cylinder default height = 2
            t.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(t.GetComponent<Collider>());
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        // Instantiates the whole combined asset, finds the one named node we want anywhere in its
        // hierarchy, detaches it under `parent`, and discards the rest (all the other components).
        private static Transform InstantiateNode(GameObject glbAsset, string nodeName, Transform parent)
        {
            var temp = (GameObject)PrefabUtility.InstantiatePrefab(glbAsset);
            // Instantiating a model asset creates a linked prefab instance, and Unity refuses to
            // SetParent() any of its children (silently logging an error, not throwing) until the
            // instance is unpacked into plain GameObjects.
            PrefabUtility.UnpackPrefabInstance(temp, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var found = FindDeepChild(temp.transform, nodeName);
            if (found == null)
            {
                Debug.LogError($"[CircuitAR] Couldn't find a node named \"{nodeName}\" inside {glbAsset.name} — " +
                                 "the source asset's hierarchy may have changed since this script was written.");
                Object.DestroyImmediate(temp);
                return null;
            }

            found.SetParent(parent, false);
            found.localPosition = Vector3.zero;
            found.localRotation = Quaternion.identity;
            found.localScale = Vector3.one;
            found.name = "Model";
            Object.DestroyImmediate(temp); // everything else in the combined scene
            return found;
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                var result = FindDeepChild(child, name);
                if (result != null) return result;
            }
            return null;
        }

        // Combined mesh bounds of `renderers`, expressed in `space`'s local coordinates.
        private static Bounds LocalBounds(Transform space, Renderer[] renderers)
        {
            bool any = false;
            var result = new Bounds();
            foreach (var r in renderers)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var toSpace = space.worldToLocalMatrix * r.transform.localToWorldMatrix;
                var mb = mf.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3(
                        (i & 1) == 0 ? mb.min.x : mb.max.x,
                        (i & 2) == 0 ? mb.min.y : mb.max.y,
                        (i & 4) == 0 ? mb.min.z : mb.max.z);
                    var p = toSpace.MultiplyPoint3x4(corner);
                    if (!any) { result = new Bounds(p, Vector3.zero); any = true; }
                    else result.Encapsulate(p);
                }
            }
            return result;
        }

        private static void SaveAndDestroy(GameObject root, string prefabName)
        {
            PrefabUtility.SaveAsPrefabAsset(root, PrefabDir + prefabName + ".prefab");
            Object.DestroyImmediate(root);
        }

        private static Material CreateSimpleMaterial(string name, Color color)
        {
            string path = GenDir + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Sprites/Default");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/AR")) AssetDatabase.CreateFolder("Assets/Prefabs", "AR");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/AR/Generated")) AssetDatabase.CreateFolder("Assets/Prefabs/AR", "Generated");
        }
    }
}
