using System.Linq;
using CircuitAR.AR;
using CircuitAR.Audio;
using CircuitAR.EditorUIBuilders;
using CircuitAR.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CircuitAR.EditorAR
{
    // Wires ARContent_Parallel (menu item 2) and the Canvas_ParallelStep1/2/3 screens (Tools >
    // Build UI > P1/P2/P3) into one ParallelCircuitController — same pattern as
    // SeriesCircuitWiring. Safe to re-run; re-run after rebuilding ARContent_Parallel, any
    // Canvas_ParallelStep* screen, the Workspace screen, or Canvas_FinalStepComplete.
    public static class ParallelCircuitWiring
    {
        private const string MainScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ControllerName = "ParallelCircuitController";

        [MenuItem("Tools/CircuitAR/AR/5 Wire Parallel Circuit")]
        public static void Wire()
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
                    "Couldn't find ARCamera/ImageTarget. Run Tools > CircuitAR > Vuforia > 4 Merge AR Into Main Scene first.", "OK");
                return;
            }

            var arContent = imageTarget.transform.Find("ARContent_Parallel");
            if (arContent == null)
            {
                EditorUtility.DisplayDialog("ARContent_Parallel not found",
                    "Run Tools > CircuitAR > AR > 2 Build AR Content In Scene first.", "OK");
                return;
            }

            var battery = arContent.Find("Battery")?.GetComponent<PlaceableObject>();
            var r1 = arContent.Find("Resistor1")?.GetComponent<PlaceableObject>();
            var led1 = arContent.Find("LED1")?.GetComponent<PlaceableObject>();
            var led1Glow = arContent.Find("LED1")?.GetComponent<LedGlow>();
            var r2 = arContent.Find("Resistor2")?.GetComponent<PlaceableObject>();
            var led2 = arContent.Find("LED2")?.GetComponent<PlaceableObject>();
            var led2Glow = arContent.Find("LED2")?.GetComponent<LedGlow>();
            var wBattR1 = arContent.Find("Wire_BatteryToR1")?.GetComponent<WireConnector>();
            var wR1Led1 = arContent.Find("Wire_R1ToLed1")?.GetComponent<WireConnector>();
            var wR2Led2 = arContent.Find("Wire_R2ToLed2")?.GetComponent<WireConnector>();
            var wR2R1 = arContent.Find("Wire_R2ToR1")?.GetComponent<WireConnector>();
            var wLed1Led2 = arContent.Find("Wire_Led1ToLed2")?.GetComponent<WireConnector>();
            var wBattLed1 = arContent.Find("Wire_BatteryToLed1")?.GetComponent<WireConnector>();

            var anchorBattery = arContent.Find("Anchor_Battery");
            var anchorR1 = arContent.Find("Anchor_R1");
            var anchorLed1 = arContent.Find("Anchor_Led1");
            var anchorR2 = arContent.Find("Anchor_R2");
            var anchorLed2 = arContent.Find("Anchor_Led2");
            var anchorW1 = arContent.Find("Anchor_Wire1");
            var anchorW2 = arContent.Find("Anchor_Wire2");
            var anchorW3 = arContent.Find("Anchor_Wire3");
            var anchorW4 = arContent.Find("Anchor_Wire4");
            var anchorW5 = arContent.Find("Anchor_Wire5");
            var anchorW6 = arContent.Find("Anchor_Wire6");

            var step1Root = FindRoot(active, ScreenNames.ParallelStep1);
            var step2Root = FindRoot(active, ScreenNames.ParallelStep2);
            var step3Root = FindRoot(active, ScreenNames.ParallelStep3);
            var workspaceRoot = FindRoot(active, ScreenNames.Workspace);
            var completeRoot = FindRoot(active, ScreenNames.Complete);
            if (step1Root == null || step2Root == null || step3Root == null || workspaceRoot == null || completeRoot == null)
            {
                EditorUtility.DisplayDialog("Parallel screens not found",
                    "Run Tools > Build UI > P1/P2/P3 Parallel Step 1/2/3, 03 Marker Detected Workspace, " +
                    "and 06B Final Step Complete first.", "OK");
                return;
            }

            const string s1 = "UI_ParallelStep1Root/SafeArea/";
            const string s2 = "UI_ParallelStep2Root/SafeArea/";
            const string s3 = "UI_ParallelStep3Root/SafeArea/";

            var placeBatteryCallout = step1Root.transform.Find(s1 + "Callout_PlaceBattery")?.GetComponent<PlacementCallout>();
            var placeR1Callout = step1Root.transform.Find(s1 + "Callout_PlaceR1")?.GetComponent<PlacementCallout>();
            var wireBattR1Callout = step1Root.transform.Find(s1 + "Callout_ConnectBattR1")?.GetComponent<PlacementCallout>();
            var step1Next = step1Root.transform.Find(s1 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();
            var step1Audio = step1Root.GetComponent<ScreenAudioTrigger>();

            var placeLed1Callout = step2Root.transform.Find(s2 + "Callout_PlaceLed1")?.GetComponent<PlacementCallout>();
            var wireR1Led1Callout = step2Root.transform.Find(s2 + "Callout_ConnectR1Led1")?.GetComponent<PlacementCallout>();
            var placeR2Callout = step2Root.transform.Find(s2 + "Callout_PlaceR2")?.GetComponent<PlacementCallout>();
            var placeLed2Callout = step2Root.transform.Find(s2 + "Callout_PlaceLed2")?.GetComponent<PlacementCallout>();
            var wireR2Led2Callout = step2Root.transform.Find(s2 + "Callout_ConnectR2Led2")?.GetComponent<PlacementCallout>();
            var step2Next = step2Root.transform.Find(s2 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();
            var step2Audio = step2Root.GetComponent<ScreenAudioTrigger>();

            var wireR2R1Callout = step3Root.transform.Find(s3 + "Callout_JoinPositive")?.GetComponent<PlacementCallout>();
            var wireLed1Led2Callout = step3Root.transform.Find(s3 + "Callout_JoinNegative")?.GetComponent<PlacementCallout>();
            var wireBattLed1Callout = step3Root.transform.Find(s3 + "Callout_ConnectBatteryNeg")?.GetComponent<PlacementCallout>();
            var step3Next = step3Root.transform.Find(s3 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();
            var step3Audio = step3Root.GetComponent<ScreenAudioTrigger>();

            var parallelCardButton = workspaceRoot.transform.Find("UI_MarkerDetectedWorkspaceRoot/SafeArea/Panel_SelectCircuit/Card_PARALLEL")?.GetComponent<Button>();
            var screenManager = Object.FindObjectOfType<UIScreenManager>();
            var completeTextOverride = completeRoot.GetComponent<FinalStepCompleteTextOverride>();

            if (battery == null || r1 == null || led1 == null || r2 == null || led2 == null ||
                wBattR1 == null || wR1Led1 == null || wR2Led2 == null || wR2R1 == null || wLed1Led2 == null || wBattLed1 == null ||
                anchorBattery == null || anchorR1 == null || anchorLed1 == null || anchorR2 == null || anchorLed2 == null ||
                anchorW1 == null || anchorW2 == null || anchorW3 == null || anchorW4 == null || anchorW5 == null || anchorW6 == null ||
                placeBatteryCallout == null || placeR1Callout == null || wireBattR1Callout == null || step1Next == null || step1Audio == null ||
                placeLed1Callout == null || wireR1Led1Callout == null || placeR2Callout == null || placeLed2Callout == null || wireR2Led2Callout == null || step2Next == null || step2Audio == null ||
                wireR2R1Callout == null || wireLed1Led2Callout == null || wireBattLed1Callout == null || step3Next == null || step3Audio == null ||
                parallelCardButton == null || screenManager == null || completeTextOverride == null)
            {
                Debug.LogError("[CircuitAR] Parallel wiring failed — missing piece(s): " +
                    $"Battery={battery != null}, R1={r1 != null}, LED1={led1 != null}, R2={r2 != null}, LED2={led2 != null}, " +
                    $"Wires={wBattR1 != null}/{wR1Led1 != null}/{wR2Led2 != null}/{wR2R1 != null}/{wLed1Led2 != null}/{wBattLed1 != null}, " +
                    $"Anchors={anchorBattery != null}/{anchorR1 != null}/{anchorLed1 != null}/{anchorR2 != null}/{anchorLed2 != null}/" +
                    $"{anchorW1 != null}/{anchorW2 != null}/{anchorW3 != null}/{anchorW4 != null}/{anchorW5 != null}/{anchorW6 != null}, " +
                    $"Step1={placeBatteryCallout != null}/{placeR1Callout != null}/{wireBattR1Callout != null}/{step1Next != null}/{step1Audio != null}, " +
                    $"Step2={placeLed1Callout != null}/{wireR1Led1Callout != null}/{placeR2Callout != null}/{placeLed2Callout != null}/{wireR2Led2Callout != null}/{step2Next != null}/{step2Audio != null}, " +
                    $"Step3={wireR2R1Callout != null}/{wireLed1Led2Callout != null}/{wireBattLed1Callout != null}/{step3Next != null}/{step3Audio != null}, " +
                    $"ParallelCard={parallelCardButton != null}, ScreenManager={screenManager != null}, CompleteTextOverride={completeTextOverride != null}. " +
                    "Re-run Tools > CircuitAR > AR > 1/2 and Build UI > 03/P1/P2/P3/06B, in order.");
                return;
            }

            var ctrlGO = GameObject.Find(ControllerName);
            if (ctrlGO == null) ctrlGO = new GameObject(ControllerName);
            var ctrl = ctrlGO.GetComponent<ParallelCircuitController>();
            if (ctrl == null) ctrl = ctrlGO.AddComponent<ParallelCircuitController>();

            ctrl.arContentRoot = arContent.gameObject;
            ctrl.battery = battery; ctrl.resistor1 = r1; ctrl.led1 = led1; ctrl.resistor2 = r2; ctrl.led2 = led2;
            ctrl.led1Glow = led1Glow; ctrl.led2Glow = led2Glow; // null until menu 2 is re-run with LedGlow added
            ctrl.wireBattToR1 = wBattR1; ctrl.wireR1ToLed1 = wR1Led1; ctrl.wireR2ToLed2 = wR2Led2;
            ctrl.wireR2ToR1 = wR2R1; ctrl.wireLed1ToLed2 = wLed1Led2; ctrl.wireBattToLed1 = wBattLed1;
            ctrl.placeBatteryCallout = placeBatteryCallout; ctrl.placeR1Callout = placeR1Callout; ctrl.wireBattR1Callout = wireBattR1Callout;
            ctrl.step1NextButton = step1Next;
            ctrl.placeLed1Callout = placeLed1Callout; ctrl.wireR1Led1Callout = wireR1Led1Callout;
            ctrl.placeR2Callout = placeR2Callout; ctrl.placeLed2Callout = placeLed2Callout; ctrl.wireR2Led2Callout = wireR2Led2Callout;
            ctrl.step2NextButton = step2Next;
            ctrl.wireR2R1Callout = wireR2R1Callout; ctrl.wireLed1Led2Callout = wireLed1Led2Callout; ctrl.wireBattLed1Callout = wireBattLed1Callout;
            ctrl.step3NextButton = step3Next;
            ctrl.screenManager = screenManager;
            ctrl.completeTextOverride = completeTextOverride;
            EditorUtility.SetDirty(ctrl);

            WireCallout(placeBatteryCallout, anchorBattery, ctrl.PlaceBattery);
            WireCallout(placeR1Callout, anchorR1, ctrl.PlaceR1);
            WireCallout(wireBattR1Callout, anchorW1, ctrl.ConnectBattToR1);
            WireCallout(placeLed1Callout, anchorLed1, ctrl.PlaceLed1);
            WireCallout(wireR1Led1Callout, anchorW2, ctrl.ConnectR1ToLed1);
            WireCallout(placeR2Callout, anchorR2, ctrl.PlaceR2);
            WireCallout(placeLed2Callout, anchorLed2, ctrl.PlaceLed2);
            WireCallout(wireR2Led2Callout, anchorW3, ctrl.ConnectR2ToLed2);
            WireCallout(wireR2R1Callout, anchorW4, ctrl.ConnectR2ToR1);
            WireCallout(wireLed1Led2Callout, anchorW5, ctrl.ConnectLed1ToLed2);
            WireCallout(wireBattLed1Callout, anchorW6, ctrl.ConnectBattToLed1);

            WireAudioTrigger(step1Audio, ctrl.PlayStep1Audio, ctrl.StopAudio);
            WireAudioTrigger(step2Audio, ctrl.PlayStep2Audio, ctrl.StopAudio);
            WireAudioTrigger(step3Audio, ctrl.PlayStep3Audio, ctrl.StopAudio);

            step1Next.interactable = false;
            step2Next.interactable = false;
            step3Next.interactable = false;
            EditorUtility.SetDirty(step1Next);
            EditorUtility.SetDirty(step2Next);
            EditorUtility.SetDirty(step3Next);

            ClearPersistentListeners(step3Next.onClick);
            UnityEventTools.AddPersistentListener(step3Next.onClick, ctrl.FinishCircuit);

            UnityEventTools.AddPersistentListener(parallelCardButton.onClick, ctrl.SelectParallelMode);

            var handler = imageTarget.GetComponent<DefaultObserverEventHandler>();
            if (handler != null)
            {
                UnityEventTools.AddPersistentListener(handler.OnTargetFound, ctrl.ShowARContent);
                UnityEventTools.AddPersistentListener(handler.OnTargetLost, ctrl.HideARContent);
            }
            else
            {
                Debug.LogWarning("[CircuitAR] No DefaultObserverEventHandler found on ImageTarget — Parallel ARContent show/hide was not wired.");
            }

            EditorSceneManager.MarkSceneDirty(active);
            Selection.activeGameObject = ctrlGO;
            Debug.Log("[CircuitAR] Parallel circuit wired: 11 callouts across 3 screens, ARContent_Parallel " +
                       "gated on CircuitMode.Parallel, PARALLEL card selects the mode, Step 3's NEXT finishes " +
                       "the circuit and shows the shared Complete screen. Save the scene.");
        }

        private static void WireCallout(PlacementCallout callout, Transform anchor, UnityEngine.Events.UnityAction onTap)
        {
            callout.worldAnchor = anchor;
            ClearPersistentListeners(callout.button.onClick);
            UnityEventTools.AddPersistentListener(callout.button.onClick, onTap);
            EditorUtility.SetDirty(callout);
        }

        private static void WireAudioTrigger(ScreenAudioTrigger trigger, UnityEngine.Events.UnityAction onShow, UnityEngine.Events.UnityAction onHide)
        {
            ClearPersistentListeners(trigger.onShow);
            UnityEventTools.AddPersistentListener(trigger.onShow, onShow);
            ClearPersistentListeners(trigger.onHide);
            UnityEventTools.AddPersistentListener(trigger.onHide, onHide);
            EditorUtility.SetDirty(trigger);
        }

        private static void ClearPersistentListeners(UnityEngine.Events.UnityEventBase evt)
        {
            while (evt.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(evt, 0);
        }

        private static GameObject FindRoot(Scene scene, string name) =>
            scene.GetRootGameObjects().FirstOrDefault(go => go.name == name);
    }
}
