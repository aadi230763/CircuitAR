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
    // Wires ARContent_Series (menu item 2) and the Canvas_SeriesStep1/2/3 screens (Tools > Build
    // UI > S1/S2/S3) into one SeriesCircuitController. Also adds SeriesCircuitController.SelectSeriesMode
    // as a second onClick listener on the Workspace screen's SERIES card (its ShowScreen nav from
    // Build UI > 03 is left alone). Safe to re-run; re-run after rebuilding ARContent_Series, any
    // Canvas_SeriesStep* screen, the Workspace screen, or Canvas_FinalStepComplete.
    public static class SeriesCircuitWiring
    {
        private const string MainScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ControllerName = "SeriesCircuitController";

        [MenuItem("Tools/CircuitAR/AR/4 Wire Series Circuit")]
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

            var arContent = imageTarget.transform.Find("ARContent_Series");
            if (arContent == null)
            {
                EditorUtility.DisplayDialog("ARContent_Series not found",
                    "Run Tools > CircuitAR > AR > 2 Build AR Content In Scene first.", "OK");
                return;
            }

            var battery = arContent.Find("Battery")?.GetComponent<PlaceableObject>();
            var r1 = arContent.Find("Resistor1")?.GetComponent<PlaceableObject>();
            var led1 = arContent.Find("LED1")?.GetComponent<PlaceableObject>();
            var r2 = arContent.Find("Resistor2")?.GetComponent<PlaceableObject>();
            var led2 = arContent.Find("LED2")?.GetComponent<PlaceableObject>();
            var wBattR1 = arContent.Find("Wire_BatteryToR1")?.GetComponent<WireConnector>();
            var wR1Led1 = arContent.Find("Wire_R1ToLed1")?.GetComponent<WireConnector>();
            var wLed1R2 = arContent.Find("Wire_Led1ToR2")?.GetComponent<WireConnector>();
            var wR2Led2 = arContent.Find("Wire_R2ToLed2")?.GetComponent<WireConnector>();
            var wLed2Batt = arContent.Find("Wire_Led2ToBattery")?.GetComponent<WireConnector>();

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

            var step1Root = FindRoot(active, ScreenNames.SeriesStep1);
            var step2Root = FindRoot(active, ScreenNames.SeriesStep2);
            var step3Root = FindRoot(active, ScreenNames.SeriesStep3);
            var workspaceRoot = FindRoot(active, ScreenNames.Workspace);
            var completeRoot = FindRoot(active, ScreenNames.Complete);
            if (step1Root == null || step2Root == null || step3Root == null || workspaceRoot == null || completeRoot == null)
            {
                EditorUtility.DisplayDialog("Series screens not found",
                    "Run Tools > Build UI > S1/S2/S3 Series Step 1/2/3, 03 Marker Detected Workspace, " +
                    "and 06B Final Step Complete first.", "OK");
                return;
            }

            const string s1 = "UI_SeriesStep1Root/SafeArea/";
            const string s2 = "UI_SeriesStep2Root/SafeArea/";
            const string s3 = "UI_SeriesStep3Root/SafeArea/";

            var placeBatteryCallout = step1Root.transform.Find(s1 + "Callout_PlaceBattery")?.GetComponent<PlacementCallout>();
            var placeR1Callout = step1Root.transform.Find(s1 + "Callout_PlaceR1")?.GetComponent<PlacementCallout>();
            var wireBattR1Callout = step1Root.transform.Find(s1 + "Callout_ConnectBattR1")?.GetComponent<PlacementCallout>();
            var step1Next = step1Root.transform.Find(s1 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();
            var step1Audio = step1Root.GetComponent<ScreenAudioTrigger>();

            var placeLed1Callout = step2Root.transform.Find(s2 + "Callout_PlaceLed1")?.GetComponent<PlacementCallout>();
            var wireR1Led1Callout = step2Root.transform.Find(s2 + "Callout_ConnectR1Led1")?.GetComponent<PlacementCallout>();
            var placeR2Callout = step2Root.transform.Find(s2 + "Callout_PlaceR2")?.GetComponent<PlacementCallout>();
            var wireLed1R2Callout = step2Root.transform.Find(s2 + "Callout_ConnectLed1R2")?.GetComponent<PlacementCallout>();
            var step2Next = step2Root.transform.Find(s2 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();
            var step2Audio = step2Root.GetComponent<ScreenAudioTrigger>();

            var placeLed2Callout = step3Root.transform.Find(s3 + "Callout_PlaceLed2")?.GetComponent<PlacementCallout>();
            var wireR2Led2Callout = step3Root.transform.Find(s3 + "Callout_ConnectR2Led2")?.GetComponent<PlacementCallout>();
            var wireLed2BattCallout = step3Root.transform.Find(s3 + "Callout_ConnectLed2Battery")?.GetComponent<PlacementCallout>();
            var step3Next = step3Root.transform.Find(s3 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();
            var step3Audio = step3Root.GetComponent<ScreenAudioTrigger>();

            var seriesCardButton = workspaceRoot.transform.Find("UI_MarkerDetectedWorkspaceRoot/SafeArea/Panel_SelectCircuit/Card_SERIES")?.GetComponent<Button>();
            var screenManager = Object.FindObjectOfType<UIScreenManager>();
            var completeTextOverride = completeRoot.GetComponent<FinalStepCompleteTextOverride>();

            if (battery == null || r1 == null || led1 == null || r2 == null || led2 == null ||
                wBattR1 == null || wR1Led1 == null || wLed1R2 == null || wR2Led2 == null || wLed2Batt == null ||
                anchorBattery == null || anchorR1 == null || anchorLed1 == null || anchorR2 == null || anchorLed2 == null ||
                anchorW1 == null || anchorW2 == null || anchorW3 == null || anchorW4 == null || anchorW5 == null ||
                placeBatteryCallout == null || placeR1Callout == null || wireBattR1Callout == null || step1Next == null || step1Audio == null ||
                placeLed1Callout == null || wireR1Led1Callout == null || placeR2Callout == null || wireLed1R2Callout == null || step2Next == null || step2Audio == null ||
                placeLed2Callout == null || wireR2Led2Callout == null || wireLed2BattCallout == null || step3Next == null || step3Audio == null ||
                seriesCardButton == null || screenManager == null || completeTextOverride == null)
            {
                Debug.LogError("[CircuitAR] Series wiring failed — missing piece(s): " +
                    $"Battery={battery != null}, R1={r1 != null}, LED1={led1 != null}, R2={r2 != null}, LED2={led2 != null}, " +
                    $"Wires={wBattR1 != null}/{wR1Led1 != null}/{wLed1R2 != null}/{wR2Led2 != null}/{wLed2Batt != null}, " +
                    $"Anchors={anchorBattery != null}/{anchorR1 != null}/{anchorLed1 != null}/{anchorR2 != null}/{anchorLed2 != null}/" +
                    $"{anchorW1 != null}/{anchorW2 != null}/{anchorW3 != null}/{anchorW4 != null}/{anchorW5 != null}, " +
                    $"Step1={placeBatteryCallout != null}/{placeR1Callout != null}/{wireBattR1Callout != null}/{step1Next != null}/{step1Audio != null}, " +
                    $"Step2={placeLed1Callout != null}/{wireR1Led1Callout != null}/{placeR2Callout != null}/{wireLed1R2Callout != null}/{step2Next != null}/{step2Audio != null}, " +
                    $"Step3={placeLed2Callout != null}/{wireR2Led2Callout != null}/{wireLed2BattCallout != null}/{step3Next != null}/{step3Audio != null}, " +
                    $"SeriesCard={seriesCardButton != null}, ScreenManager={screenManager != null}, CompleteTextOverride={completeTextOverride != null}. " +
                    "Re-run Tools > CircuitAR > AR > 1/2 and Build UI > 03/S1/S2/S3/06B, in order.");
                return;
            }

            var ctrlGO = GameObject.Find(ControllerName);
            if (ctrlGO == null) ctrlGO = new GameObject(ControllerName);
            var ctrl = ctrlGO.GetComponent<SeriesCircuitController>();
            if (ctrl == null) ctrl = ctrlGO.AddComponent<SeriesCircuitController>();

            ctrl.arContentRoot = arContent.gameObject;
            ctrl.battery = battery; ctrl.resistor1 = r1; ctrl.led1 = led1; ctrl.resistor2 = r2; ctrl.led2 = led2;
            ctrl.wireBattToR1 = wBattR1; ctrl.wireR1ToLed1 = wR1Led1; ctrl.wireLed1ToR2 = wLed1R2;
            ctrl.wireR2ToLed2 = wR2Led2; ctrl.wireLed2ToBatt = wLed2Batt;
            ctrl.placeBatteryCallout = placeBatteryCallout; ctrl.placeR1Callout = placeR1Callout; ctrl.wireBattR1Callout = wireBattR1Callout;
            ctrl.step1NextButton = step1Next;
            ctrl.placeLed1Callout = placeLed1Callout; ctrl.wireR1Led1Callout = wireR1Led1Callout;
            ctrl.placeR2Callout = placeR2Callout; ctrl.wireLed1R2Callout = wireLed1R2Callout;
            ctrl.step2NextButton = step2Next;
            ctrl.placeLed2Callout = placeLed2Callout; ctrl.wireR2Led2Callout = wireR2Led2Callout; ctrl.wireLed2BattCallout = wireLed2BattCallout;
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
            WireCallout(wireLed1R2Callout, anchorW3, ctrl.ConnectLed1ToR2);
            WireCallout(placeLed2Callout, anchorLed2, ctrl.PlaceLed2);
            WireCallout(wireR2Led2Callout, anchorW4, ctrl.ConnectR2ToLed2);
            WireCallout(wireLed2BattCallout, anchorW5, ctrl.ConnectLed2ToBattery);

            WireAudioTrigger(step1Audio, ctrl.PlayStep1Audio, ctrl.StopAudio);
            WireAudioTrigger(step2Audio, ctrl.PlayStep2Audio, ctrl.StopAudio);
            WireAudioTrigger(step3Audio, ctrl.PlayStep3Audio, ctrl.StopAudio);

            step1Next.interactable = false;
            step2Next.interactable = false;
            step3Next.interactable = false;
            EditorUtility.SetDirty(step1Next);
            EditorUtility.SetDirty(step2Next);
            EditorUtility.SetDirty(step3Next);

            // Step 3's NEXT calls FinishCircuit() directly (no build-time ShowScreen listener was
            // ever added for it, unlike Step 1/2's NEXT which keep their plain nav from Build UI).
            ClearPersistentListeners(step3Next.onClick);
            UnityEventTools.AddPersistentListener(step3Next.onClick, ctrl.FinishCircuit);

            // Additive — the SERIES card's existing ShowScreen nav from Build UI > 03 stays.
            UnityEventTools.AddPersistentListener(seriesCardButton.onClick, ctrl.SelectSeriesMode);

            var handler = imageTarget.GetComponent<DefaultObserverEventHandler>();
            if (handler != null)
            {
                UnityEventTools.AddPersistentListener(handler.OnTargetFound, ctrl.ShowARContent);
                UnityEventTools.AddPersistentListener(handler.OnTargetLost, ctrl.HideARContent);
            }
            else
            {
                Debug.LogWarning("[CircuitAR] No DefaultObserverEventHandler found on ImageTarget — Series ARContent show/hide was not wired.");
            }

            EditorSceneManager.MarkSceneDirty(active);
            Selection.activeGameObject = ctrlGO;
            Debug.Log("[CircuitAR] Series circuit wired: 10 callouts across 3 screens, ARContent_Series " +
                       "gated on CircuitMode.Series, SERIES card selects the mode, Step 3's NEXT finishes " +
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
