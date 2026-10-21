using CircuitAR.AR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CircuitAR.EditorAR
{
    // Instantiates the component prefabs (Tools > CircuitAR > AR > 1) under
    // ImageTarget/ARContent in SampleScene, sized against the real marker card (0.21m wide,
    // from the ImageTargetBehaviour already configured in Phase 3). Safe to re-run — rebuilds
    // only the "ARContent" subtree each time.
    public static class ARContentSceneBuilder
    {
        private const string MainScenePath = "Assets/Scenes/SampleScene.unity";
        private const string PrefabDir = "Assets/Prefabs/AR/";

        [MenuItem("Tools/CircuitAR/AR/2 Build AR Content In Scene")]
        public static void Build()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.path != MainScenePath)
            {
                EditorUtility.DisplayDialog("Open SampleScene first",
                    "Open " + MainScenePath + " as the active scene, then run this menu item again.", "OK");
                return;
            }

            var imageTarget = GameObject.Find("ARCamera/ImageTarget");
            if (imageTarget == null)
            {
                EditorUtility.DisplayDialog("ImageTarget not found",
                    "Couldn't find ARCamera/ImageTarget in SampleScene. Run " +
                    "Tools > CircuitAR > Vuforia > 4 Merge AR Into Main Scene first.", "OK");
                return;
            }

            var breadboardPrefab = LoadPrefab("Breadboard");
            var ledPrefab = LoadPrefab("LED");
            var resistorPrefab = LoadPrefab("Resistor");
            var wirePrefab = LoadPrefab("Wire");
            if (breadboardPrefab == null || ledPrefab == null || resistorPrefab == null || wirePrefab == null)
            {
                EditorUtility.DisplayDialog("Component prefabs missing",
                    "Run Tools > CircuitAR > AR > 1 Build Component Prefabs first.", "OK");
                return;
            }

            var existing = imageTarget.transform.Find("ARContent");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var root = new GameObject("ARContent");
            root.transform.SetParent(imageTarget.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            var breadboard = (GameObject)PrefabUtility.InstantiatePrefab(breadboardPrefab, root.transform);
            breadboard.name = "Breadboard";
            breadboard.transform.localPosition = Vector3.zero;

            // Parts start in a "tray" strip on the card between the board's front edge (z = -0.035)
            // and the card's edge (z = -0.079). Slots are on the board's top surface (y = 0.012).
            var ledGO = (GameObject)PrefabUtility.InstantiatePrefab(ledPrefab, root.transform);
            ledGO.name = "LED";
            // Tray: lying on its side (raised by its radius so it rests on the card); slot: upright.
            AttachPlaceable(ledGO, LedTrayPos, new Vector3(0f, 0f, 90f), LedSlotPos, Vector3.zero);

            var resistorGO = (GameObject)PrefabUtility.InstantiatePrefab(resistorPrefab, root.transform);
            resistorGO.name = "Resistor";
            AttachPlaceable(resistorGO, ResistorTrayPos, new Vector3(0f, 30f, 0f), ResistorSlotPos, Vector3.zero);

            var wireTray = (GameObject)PrefabUtility.InstantiatePrefab(wirePrefab, root.transform);
            wireTray.name = "WireTray";
            wireTray.transform.localPosition = Vector3.zero;
            var trayLine = wireTray.GetComponent<LineRenderer>();
            trayLine.useWorldSpace = false;
            trayLine.positionCount = 4; // loose zig-zag so it reads as an unconnected jumper
            trayLine.SetPosition(0, new Vector3(-0.078f, 0.002f, -0.052f));
            trayLine.SetPosition(1, new Vector3(-0.066f, 0.002f, -0.064f));
            trayLine.SetPosition(2, new Vector3(-0.054f, 0.002f, -0.052f));
            trayLine.SetPosition(3, new Vector3(-0.042f, 0.002f, -0.064f));

            var wireGO = (GameObject)PrefabUtility.InstantiatePrefab(wirePrefab, root.transform);
            wireGO.name = "Wire";
            wireGO.transform.localPosition = Vector3.zero;
            var wire = wireGO.AddComponent<WireConnector>();
            wire.line = wireGO.GetComponent<LineRenderer>();
            wire.trayPiece = wireTray;
            wire.startLocal = WireStart;
            wire.endLocal = WireEnd;
            wire.ResetToTray();

            // Invisible points the on-screen callouts float above (see PlacementCallout).
            AddAnchor(root.transform, "Anchor_LEDSlot", LedSlotPos + new Vector3(0f, 0.025f, 0f));
            AddAnchor(root.transform, "Anchor_ResistorSlot", ResistorSlotPos + new Vector3(0f, 0.012f, 0f));
            AddAnchor(root.transform, "Anchor_WireMid", wire.ArcPoint(0.5f));

            // Hidden until Vuforia reports the marker tracked (wired in menu item 3).
            root.SetActive(false);

            EditorSceneManager.MarkSceneDirty(active);
            Selection.activeGameObject = root;
            Debug.Log("[CircuitAR] Built ImageTarget/ARContent (empty Breadboard, LED/Resistor/Wire in the parts tray). " +
                       "Next: Tools > CircuitAR > AR > 3 Wire Circuit Demo. Save the scene when done.");
        }

        // DECISION: default poses (ARContent-local metres). Tweak in the Inspector on each part's
        // PlaceableObject / WireConnector, or change here and re-run this menu item.
        private static readonly Vector3 LedSlotPos = new Vector3(0.035f, 0.012f, 0f);
        private static readonly Vector3 LedTrayPos = new Vector3(0.05f, 0.006f, -0.058f);
        private static readonly Vector3 ResistorSlotPos = new Vector3(-0.02f, 0.012f, 0f);
        private static readonly Vector3 ResistorTrayPos = new Vector3(-0.005f, 0f, -0.058f);
        private static readonly Vector3 WireStart = new Vector3(-0.0025f, 0.018f, 0f); // resistor's right-hand end
        private static readonly Vector3 WireEnd = new Vector3(0.03f, 0.016f, 0f);      // the LED's leg

        private static void AttachPlaceable(GameObject go, Vector3 trayPos, Vector3 trayEuler, Vector3 slotPos, Vector3 slotEuler)
        {
            var placeable = go.AddComponent<PlaceableObject>();
            placeable.trayLocalPosition = trayPos;
            placeable.trayLocalEuler = trayEuler;
            placeable.slotLocalPosition = slotPos;
            placeable.slotLocalEuler = slotEuler;
            placeable.ResetToTray();
        }

        private static void AddAnchor(Transform parent, string name, Vector3 localPos)
        {
            var anchor = new GameObject(name);
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = localPos;
        }

        private static GameObject LoadPrefab(string name) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + name + ".prefab");
    }
}
