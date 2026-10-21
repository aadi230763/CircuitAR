using CircuitAR.AR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CircuitAR.EditorAR
{
    // Instantiates the component prefabs (Tools > CircuitAR > AR > 1) under ImageTarget as THREE
    // sibling content trees — ARContent (LED, untouched from before), ARContent_Series, and
    // ARContent_Parallel — sized against the real marker card (0.21m wide, from the
    // ImageTargetBehaviour already configured in Phase 3). Only one is ever visible at a time at
    // runtime (CircuitModeState — see CircuitDemoController / SeriesCircuitController /
    // ParallelCircuitController), but all three get built here so a card tap can switch cleanly.
    // Safe to re-run — rebuilds all three subtrees each time.
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
            var batteryPrefab = LoadPrefab("Battery");
            if (breadboardPrefab == null || ledPrefab == null || resistorPrefab == null || wirePrefab == null || batteryPrefab == null)
            {
                EditorUtility.DisplayDialog("Component prefabs missing",
                    "Run Tools > CircuitAR > AR > 1 Build Component Prefabs first.", "OK");
                return;
            }

            BuildLedContent(imageTarget.transform, breadboardPrefab, ledPrefab, resistorPrefab, wirePrefab);
            BuildSeriesContent(imageTarget.transform, breadboardPrefab, ledPrefab, resistorPrefab, wirePrefab, batteryPrefab);
            BuildParallelContent(imageTarget.transform, breadboardPrefab, ledPrefab, resistorPrefab, wirePrefab, batteryPrefab);

            EditorSceneManager.MarkSceneDirty(active);
            Debug.Log("[CircuitAR] Built ImageTarget/ARContent, ARContent_Series, and ARContent_Parallel " +
                       "(all hidden until a mode is selected and the marker is tracked). " +
                       "Next: Tools > CircuitAR > AR > 3 Wire Circuit Demo (LED), 4 Wire Series Circuit, " +
                       "5 Wire Parallel Circuit. Save the scene when done.");
        }

        // ------------------------------------------------------------------
        // LED Circuit — unchanged from before this feature; same prefabs, same poses, same names.
        // ------------------------------------------------------------------

        private static readonly Vector3 LedSlotPos = new Vector3(0.035f, 0.012f, 0f);
        private static readonly Vector3 LedTrayPos = new Vector3(0.05f, 0.006f, -0.058f);
        private static readonly Vector3 ResistorSlotPos = new Vector3(-0.02f, 0.012f, 0f);
        private static readonly Vector3 ResistorTrayPos = new Vector3(-0.005f, 0f, -0.058f);
        private static readonly Vector3 WireStart = new Vector3(-0.0025f, 0.018f, 0f); // resistor's right-hand end
        private static readonly Vector3 WireEnd = new Vector3(0.03f, 0.016f, 0f);      // the LED's leg

        private static void BuildLedContent(Transform imageTarget, GameObject breadboardPrefab, GameObject ledPrefab,
            GameObject resistorPrefab, GameObject wirePrefab)
        {
            var existing = imageTarget.Find("ARContent");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var root = NewContentRoot(imageTarget, "ARContent");
            InstantiateAt(breadboardPrefab, root, "Breadboard", Vector3.zero);

            var ledGO = InstantiateAt(ledPrefab, root, "LED", Vector3.zero);
            AttachPlaceable(ledGO, LedTrayPos, new Vector3(0f, 0f, 90f), LedSlotPos, Vector3.zero);

            var resistorGO = InstantiateAt(resistorPrefab, root, "Resistor", Vector3.zero);
            AttachPlaceable(resistorGO, ResistorTrayPos, new Vector3(0f, 30f, 0f), ResistorSlotPos, Vector3.zero);

            var wireTray = InstantiateAt(wirePrefab, root, "WireTray", Vector3.zero);
            var trayLine = wireTray.GetComponent<LineRenderer>();
            trayLine.useWorldSpace = false;
            trayLine.positionCount = 4; // loose zig-zag so it reads as an unconnected jumper
            trayLine.SetPosition(0, new Vector3(-0.078f, 0.002f, -0.052f));
            trayLine.SetPosition(1, new Vector3(-0.066f, 0.002f, -0.064f));
            trayLine.SetPosition(2, new Vector3(-0.054f, 0.002f, -0.052f));
            trayLine.SetPosition(3, new Vector3(-0.042f, 0.002f, -0.064f));

            var wire = AttachWire(wirePrefab, root, "Wire", wireTray, WireStart, WireEnd);

            AddAnchor(root, "Anchor_LEDSlot", LedSlotPos + new Vector3(0f, 0.025f, 0f));
            AddAnchor(root, "Anchor_ResistorSlot", ResistorSlotPos + new Vector3(0f, 0.012f, 0f));
            AddAnchor(root, "Anchor_WireMid", wire.ArcPoint(0.5f));

            root.gameObject.SetActive(false); // shown by CircuitDemoController once LED mode + tracked
        }

        // ------------------------------------------------------------------
        // Series Circuit: Battery(+) -> R1 -> LED1 -> R2 -> LED2 -> Battery(-), a single chain.
        // DECISION: tray spreads 5 parts left-to-right; slot/wire points are approximate, hand-
        // placed (same "fixed constants" approach the LED circuit already uses, not per-instance
        // measured) — nudge these and re-run this menu item if anything looks off on-device.
        // ------------------------------------------------------------------

        private static readonly Vector3 SeriesBatteryTray = new Vector3(-0.08f, 0f, -0.058f);
        private static readonly Vector3 SeriesR1Tray = new Vector3(-0.04f, 0f, -0.058f);
        private static readonly Vector3 SeriesLed1Tray = new Vector3(0f, 0.006f, -0.058f);
        private static readonly Vector3 SeriesR2Tray = new Vector3(0.04f, 0f, -0.058f);
        private static readonly Vector3 SeriesLed2Tray = new Vector3(0.08f, 0.006f, -0.058f);

        private static readonly Vector3 SeriesBatterySlot = new Vector3(-0.1f, 0f, -0.04f); // beside the board, not on it
        private static readonly Vector3 SeriesR1Slot = new Vector3(-0.06f, 0.012f, 0f);
        private static readonly Vector3 SeriesLed1Slot = new Vector3(-0.025f, 0.012f, 0f);
        private static readonly Vector3 SeriesR2Slot = new Vector3(0.015f, 0.012f, 0f);
        private static readonly Vector3 SeriesLed2Slot = new Vector3(0.05f, 0.012f, 0f);

        private static void BuildSeriesContent(Transform imageTarget, GameObject breadboardPrefab, GameObject ledPrefab,
            GameObject resistorPrefab, GameObject wirePrefab, GameObject batteryPrefab)
        {
            var existing = imageTarget.Find("ARContent_Series");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var root = NewContentRoot(imageTarget, "ARContent_Series");
            InstantiateAt(breadboardPrefab, root, "Breadboard", Vector3.zero);

            var battery = InstantiateAt(batteryPrefab, root, "Battery", Vector3.zero);
            AttachPlaceable(battery, SeriesBatteryTray, Vector3.zero, SeriesBatterySlot, Vector3.zero);

            var r1 = InstantiateAt(resistorPrefab, root, "Resistor1", Vector3.zero);
            AttachPlaceable(r1, SeriesR1Tray, new Vector3(0f, 30f, 0f), SeriesR1Slot, Vector3.zero);

            var led1 = InstantiateAt(ledPrefab, root, "LED1", Vector3.zero);
            AttachPlaceable(led1, SeriesLed1Tray, new Vector3(0f, 0f, 90f), SeriesLed1Slot, Vector3.zero);

            var r2 = InstantiateAt(resistorPrefab, root, "Resistor2", Vector3.zero);
            AttachPlaceable(r2, SeriesR2Tray, new Vector3(0f, 30f, 0f), SeriesR2Slot, Vector3.zero);

            var led2 = InstantiateAt(ledPrefab, root, "LED2", Vector3.zero);
            AttachPlaceable(led2, SeriesLed2Tray, new Vector3(0f, 0f, 90f), SeriesLed2Slot, Vector3.zero);

            var wireTray = InstantiateAt(wirePrefab, root, "WireTray", Vector3.zero);
            SetLooseTrayLine(wireTray, new Vector3(-0.078f, 0.002f, -0.052f));

            var w1 = AttachWire(wirePrefab, root, "Wire_BatteryToR1", wireTray,
                new Vector3(-0.095f, 0.03f, -0.04f), new Vector3(-0.06f, 0.018f, 0f));
            var w2 = AttachWire(wirePrefab, root, "Wire_R1ToLed1", wireTray,
                new Vector3(-0.06f, 0.018f, 0f), new Vector3(-0.025f, 0.02f, 0f));
            var w3 = AttachWire(wirePrefab, root, "Wire_Led1ToR2", wireTray,
                new Vector3(-0.025f, 0.02f, 0f), new Vector3(0.015f, 0.018f, 0f));
            var w4 = AttachWire(wirePrefab, root, "Wire_R2ToLed2", wireTray,
                new Vector3(0.015f, 0.018f, 0f), new Vector3(0.05f, 0.02f, 0f));
            var w5 = AttachWire(wirePrefab, root, "Wire_Led2ToBattery", wireTray,
                new Vector3(0.05f, 0.02f, 0f), new Vector3(-0.105f, 0.03f, -0.04f));

            AddAnchor(root, "Anchor_Battery", SeriesBatterySlot + new Vector3(0f, 0.04f, 0f));
            AddAnchor(root, "Anchor_R1", SeriesR1Slot + new Vector3(0f, 0.012f, 0f));
            AddAnchor(root, "Anchor_Led1", SeriesLed1Slot + new Vector3(0f, 0.025f, 0f));
            AddAnchor(root, "Anchor_R2", SeriesR2Slot + new Vector3(0f, 0.012f, 0f));
            AddAnchor(root, "Anchor_Led2", SeriesLed2Slot + new Vector3(0f, 0.025f, 0f));
            AddAnchor(root, "Anchor_Wire1", w1.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire2", w2.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire3", w3.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire4", w4.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire5", w5.ArcPoint(0.5f));

            root.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------
        // Parallel Circuit: two branches (R1+LED1, R2+LED2) bridging a shared positive/negative
        // connection, battery feeding the positive side. Same "fixed, hand-placed" approach.
        // ------------------------------------------------------------------

        private static readonly Vector3 ParallelBatteryTray = new Vector3(-0.08f, 0f, -0.058f);
        private static readonly Vector3 ParallelR1Tray = new Vector3(-0.04f, 0f, -0.058f);
        private static readonly Vector3 ParallelLed1Tray = new Vector3(0f, 0.006f, -0.058f);
        private static readonly Vector3 ParallelR2Tray = new Vector3(0.04f, 0f, -0.058f);
        private static readonly Vector3 ParallelLed2Tray = new Vector3(0.08f, 0.006f, -0.058f);

        private static readonly Vector3 ParallelBatterySlot = new Vector3(-0.1f, 0f, -0.04f);
        private static readonly Vector3 ParallelR1Slot = new Vector3(-0.03f, 0.012f, -0.015f);
        private static readonly Vector3 ParallelLed1Slot = new Vector3(-0.03f, 0.012f, 0.015f);
        private static readonly Vector3 ParallelR2Slot = new Vector3(0.03f, 0.012f, -0.015f);
        private static readonly Vector3 ParallelLed2Slot = new Vector3(0.03f, 0.012f, 0.015f);

        private static void BuildParallelContent(Transform imageTarget, GameObject breadboardPrefab, GameObject ledPrefab,
            GameObject resistorPrefab, GameObject wirePrefab, GameObject batteryPrefab)
        {
            var existing = imageTarget.Find("ARContent_Parallel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var root = NewContentRoot(imageTarget, "ARContent_Parallel");
            InstantiateAt(breadboardPrefab, root, "Breadboard", Vector3.zero);

            var battery = InstantiateAt(batteryPrefab, root, "Battery", Vector3.zero);
            AttachPlaceable(battery, ParallelBatteryTray, Vector3.zero, ParallelBatterySlot, Vector3.zero);

            var r1 = InstantiateAt(resistorPrefab, root, "Resistor1", Vector3.zero);
            AttachPlaceable(r1, ParallelR1Tray, new Vector3(0f, 30f, 0f), ParallelR1Slot, new Vector3(0f, 90f, 0f));

            var led1 = InstantiateAt(ledPrefab, root, "LED1", Vector3.zero);
            AttachPlaceable(led1, ParallelLed1Tray, new Vector3(0f, 0f, 90f), ParallelLed1Slot, Vector3.zero);

            var r2 = InstantiateAt(resistorPrefab, root, "Resistor2", Vector3.zero);
            AttachPlaceable(r2, ParallelR2Tray, new Vector3(0f, 30f, 0f), ParallelR2Slot, new Vector3(0f, 90f, 0f));

            var led2 = InstantiateAt(ledPrefab, root, "LED2", Vector3.zero);
            AttachPlaceable(led2, ParallelLed2Tray, new Vector3(0f, 0f, 90f), ParallelLed2Slot, Vector3.zero);

            var wireTray = InstantiateAt(wirePrefab, root, "WireTray", Vector3.zero);
            SetLooseTrayLine(wireTray, new Vector3(-0.078f, 0.002f, -0.052f));

            var w1 = AttachWire(wirePrefab, root, "Wire_BatteryToR1", wireTray,
                new Vector3(-0.095f, 0.03f, -0.04f), ParallelR1Slot + new Vector3(0f, 0.012f, 0f));
            var w2 = AttachWire(wirePrefab, root, "Wire_R1ToLed1", wireTray,
                ParallelR1Slot + new Vector3(0f, 0.012f, 0f), ParallelLed1Slot + new Vector3(0f, 0.02f, 0f));
            var w3 = AttachWire(wirePrefab, root, "Wire_R2ToLed2", wireTray,
                ParallelR2Slot + new Vector3(0f, 0.012f, 0f), ParallelLed2Slot + new Vector3(0f, 0.02f, 0f));
            var w4 = AttachWire(wirePrefab, root, "Wire_R2ToR1", wireTray,
                ParallelR2Slot + new Vector3(0f, 0.012f, 0f), ParallelR1Slot + new Vector3(0f, 0.012f, 0f));
            var w5 = AttachWire(wirePrefab, root, "Wire_Led1ToLed2", wireTray,
                ParallelLed1Slot + new Vector3(0f, 0.02f, 0f), ParallelLed2Slot + new Vector3(0f, 0.02f, 0f));
            var w6 = AttachWire(wirePrefab, root, "Wire_BatteryToLed1", wireTray,
                new Vector3(-0.105f, 0.03f, -0.04f), ParallelLed1Slot + new Vector3(0f, 0.02f, 0f));

            AddAnchor(root, "Anchor_Battery", ParallelBatterySlot + new Vector3(0f, 0.04f, 0f));
            AddAnchor(root, "Anchor_R1", ParallelR1Slot + new Vector3(0f, 0.012f, 0f));
            AddAnchor(root, "Anchor_Led1", ParallelLed1Slot + new Vector3(0f, 0.025f, 0f));
            AddAnchor(root, "Anchor_R2", ParallelR2Slot + new Vector3(0f, 0.012f, 0f));
            AddAnchor(root, "Anchor_Led2", ParallelLed2Slot + new Vector3(0f, 0.025f, 0f));
            AddAnchor(root, "Anchor_Wire1", w1.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire2", w2.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire3", w3.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire4", w4.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire5", w5.ArcPoint(0.5f));
            AddAnchor(root, "Anchor_Wire6", w6.ArcPoint(0.5f));

            root.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------
        // Shared helpers
        // ------------------------------------------------------------------

        private static Transform NewContentRoot(Transform imageTarget, string name)
        {
            var root = new GameObject(name);
            root.transform.SetParent(imageTarget, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root.transform;
        }

        private static GameObject InstantiateAt(GameObject prefab, Transform parent, string name, Vector3 localPos)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.localPosition = localPos;
            return go;
        }

        private static void AttachPlaceable(GameObject go, Vector3 trayPos, Vector3 trayEuler, Vector3 slotPos, Vector3 slotEuler)
        {
            var placeable = go.AddComponent<PlaceableObject>();
            placeable.trayLocalPosition = trayPos;
            placeable.trayLocalEuler = trayEuler;
            placeable.slotLocalPosition = slotPos;
            placeable.slotLocalEuler = slotEuler;
            placeable.ResetToTray();
        }

        private static WireConnector AttachWire(GameObject wirePrefab, Transform parent, string name, GameObject trayPiece,
            Vector3 start, Vector3 end)
        {
            var go = InstantiateAt(wirePrefab, parent, name, Vector3.zero);
            var wire = go.AddComponent<WireConnector>();
            wire.line = go.GetComponent<LineRenderer>();
            wire.trayPiece = trayPiece;
            wire.startLocal = start;
            wire.endLocal = end;
            wire.ResetToTray();
            return wire;
        }

        private static void SetLooseTrayLine(GameObject wireTrayGO, Vector3 start)
        {
            var trayLine = wireTrayGO.GetComponent<LineRenderer>();
            trayLine.useWorldSpace = false;
            trayLine.positionCount = 4;
            trayLine.SetPosition(0, start);
            trayLine.SetPosition(1, start + new Vector3(0.012f, 0f, -0.012f));
            trayLine.SetPosition(2, start + new Vector3(0.024f, 0f, 0f));
            trayLine.SetPosition(3, start + new Vector3(0.036f, 0f, -0.012f));
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
