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
using Vuforia;

namespace CircuitAR.EditorAR
{
    // Wires the already-built ARContent (menu item 2), the Step1/Step2 placement callouts and NEXT
    // buttons (Tools > Build UI > 04 / 05) and the Final Step checklist (06A) into one
    // CircuitDemoController, and wires ImageTarget's DefaultObserverEventHandler
    // (OnTargetFound/OnTargetLost) to show/hide the AR content.
    //
    // Safe to re-run, and MUST be re-run any time ARContent or those canvases are rebuilt —
    // rebuilding destroys and recreates those GameObjects, which breaks the direct references
    // this script assigns.
    public static class CircuitDemoWiring
    {
        private const string MainScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ControllerName = "CircuitDemoController";

        [MenuItem("Tools/CircuitAR/AR/3 Wire Circuit Demo")]
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

            var arContent = imageTarget.transform.Find("ARContent");
            if (arContent == null)
            {
                EditorUtility.DisplayDialog("ARContent not found",
                    "Run Tools > CircuitAR > AR > 2 Build AR Content In Scene first.", "OK");
                return;
            }

            var led = arContent.Find("LED")?.GetComponent<PlaceableObject>();
            var ledGlow = arContent.Find("LED")?.GetComponent<LedGlow>();
            var resistor = arContent.Find("Resistor")?.GetComponent<PlaceableObject>();
            var wire = arContent.Find("Wire")?.GetComponent<WireConnector>();
            var ledAnchor = arContent.Find("Anchor_LEDSlot");
            var resistorAnchor = arContent.Find("Anchor_ResistorSlot");
            var wireAnchor = arContent.Find("Anchor_WireMid");

            var arCameraRoot = FindRoot(active, ScreenNames.ARCamera);
            var workspaceRoot = FindRoot(active, ScreenNames.Workspace);
            var step1Root = FindRoot(active, ScreenNames.Step1);
            var step2Root = FindRoot(active, ScreenNames.Step2);
            var incompleteRoot = FindRoot(active, ScreenNames.Incomplete);
            var completeRoot = FindRoot(active, ScreenNames.Complete);
            if (arCameraRoot == null || workspaceRoot == null || step1Root == null || step2Root == null ||
                incompleteRoot == null || completeRoot == null)
            {
                EditorUtility.DisplayDialog("Step UI not found",
                    "Run Tools > Build UI > 02 AR Camera, 03 Marker Detected Workspace, 04 Step 1 Place LED, " +
                    "05 Step 2-3 Resistor Wire, 06A Final Step Incomplete, and 06B Final Step Complete first.", "OK");
                return;
            }

            var detectionPill = arCameraRoot.transform.Find("UI_ARCameraRoot/SafeArea/Pill_DetectionState")?.GetComponent<DetectionStatusPill>();

            const string step1 = "UI_Step1PlaceLEDRoot/SafeArea/";
            const string step2 = "UI_Step2And3ResistorWireRoot/SafeArea/";
            var ledCallout = step1Root.transform.Find(step1 + "Callout_PlaceLED")?.GetComponent<PlacementCallout>();
            var step1Next = step1Root.transform.Find(step1 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();
            var resistorCallout = step2Root.transform.Find(step2 + "Callout_PlaceResistor")?.GetComponent<PlacementCallout>();
            var wireCallout = step2Root.transform.Find(step2 + "Callout_ConnectWire")?.GetComponent<PlacementCallout>();
            var step2Next = step2Root.transform.Find(step2 + "Panel_Instruction/Btn_NEXT")?.GetComponent<Button>();

            // Audio triggers: the Welcome/PlaceLED/PlaceResistor ones sit on their screen's own
            // canvas root; the ConnectWire one sits on Callout_ConnectWire itself (fires when that
            // callout becomes visible, i.e. once the resistor is placed).
            var welcomeAudio = arCameraRoot.GetComponent<ScreenAudioTrigger>();
            var placeLedAudio = step1Root.GetComponent<ScreenAudioTrigger>();
            var placeResistorAudio = step2Root.GetComponent<ScreenAudioTrigger>();
            var connectWireAudio = step2Root.transform.Find(step2 + "Callout_ConnectWire")?.GetComponent<ScreenAudioTrigger>();

            var ledCardButton = workspaceRoot.transform.Find("UI_MarkerDetectedWorkspaceRoot/SafeArea/Panel_SelectCircuit/Card_LED")?.GetComponent<Button>();
            var workspaceAudio = workspaceRoot.GetComponent<ScreenAudioTrigger>();

            var circuitCompleteAudio = completeRoot.GetComponent<ScreenAudioTrigger>();
            var completeTextOverride = completeRoot.GetComponent<FinalStepCompleteTextOverride>();

            const string incompletePanel = "UI_FinalStepIncompleteRoot/SafeArea/Panel_Instruction/";
            var checkButton = incompleteRoot.transform.Find(incompletePanel + "Btn_CHECKCIRCUIT")?.GetComponent<Button>();
            var batteryItem = incompleteRoot.transform.Find(incompletePanel + "Chip_Battery")?.GetComponent<ChecklistItemView>();
            var resistorItem = incompleteRoot.transform.Find(incompletePanel + "Chip_Resistor")?.GetComponent<ChecklistItemView>();
            var ledItem = incompleteRoot.transform.Find(incompletePanel + "Chip_LED")?.GetComponent<ChecklistItemView>();
            var returnPathItem = incompleteRoot.transform.Find(incompletePanel + "Chip_ReturnPath")?.GetComponent<ChecklistItemView>();
            var screenManager = Object.FindObjectOfType<UIScreenManager>();

            if (led == null || resistor == null || wire == null || ledAnchor == null || resistorAnchor == null || wireAnchor == null ||
                detectionPill == null || ledCallout == null || step1Next == null || resistorCallout == null || wireCallout == null || step2Next == null ||
                checkButton == null || batteryItem == null || resistorItem == null || ledItem == null || returnPathItem == null || screenManager == null ||
                welcomeAudio == null || placeLedAudio == null || placeResistorAudio == null || connectWireAudio == null ||
                ledCardButton == null || workspaceAudio == null || circuitCompleteAudio == null || completeTextOverride == null)
            {
                Debug.LogError("[CircuitAR] Wiring failed — missing piece(s): " +
                    $"LED={led != null}, Resistor={resistor != null}, Wire={wire != null}, " +
                    $"Anchors={ledAnchor != null}/{resistorAnchor != null}/{wireAnchor != null}, " +
                    $"DetectionPill={detectionPill != null}, " +
                    $"LedCallout={ledCallout != null}, Step1Next={step1Next != null}, ResistorCallout={resistorCallout != null}, " +
                    $"WireCallout={wireCallout != null}, Step2Next={step2Next != null}, " +
                    $"CheckButton={checkButton != null}, BatteryChip={batteryItem != null}, ResistorChip={resistorItem != null}, " +
                    $"LEDChip={ledItem != null}, ReturnPathChip={returnPathItem != null}, ScreenManager={screenManager != null}, " +
                    $"WelcomeAudio={welcomeAudio != null}, PlaceLedAudio={placeLedAudio != null}, " +
                    $"PlaceResistorAudio={placeResistorAudio != null}, ConnectWireAudio={connectWireAudio != null}, " +
                    $"LedCardButton={ledCardButton != null}, WorkspaceAudio={workspaceAudio != null}, " +
                    $"CircuitCompleteAudio={circuitCompleteAudio != null}, CompleteTextOverride={completeTextOverride != null}. " +
                    "Re-run the build steps named in the dialogs for menu items 1/2 and Build UI 02/03/04/05/06A/06B, in order.");
                return;
            }

            var ctrlGO = GameObject.Find(ControllerName);
            if (ctrlGO == null) ctrlGO = new GameObject(ControllerName);
            var ctrl = ctrlGO.GetComponent<CircuitDemoController>();
            if (ctrl == null) ctrl = ctrlGO.AddComponent<CircuitDemoController>();

            ctrl.arContentRoot = arContent.gameObject;
            ctrl.led = led;
            ctrl.ledGlow = ledGlow; // null until Tools > CircuitAR > AR > 2 is re-run with LedGlow added
            ctrl.resistor = resistor;
            ctrl.wire = wire;
            ctrl.ledCallout = ledCallout;
            ctrl.resistorCallout = resistorCallout;
            ctrl.wireCallout = wireCallout;
            ctrl.step1NextButton = step1Next;
            ctrl.step2NextButton = step2Next;
            ctrl.batteryChecklistItem = batteryItem;
            ctrl.resistorChecklistItem = resistorItem;
            ctrl.ledChecklistItem = ledItem;
            ctrl.returnPathChecklistItem = returnPathItem;
            ctrl.screenManager = screenManager;
            ctrl.detectionPill = detectionPill;
            ctrl.workspaceLedCardButton = ledCardButton;
            ctrl.completeTextOverride = completeTextOverride;
            EditorUtility.SetDirty(ctrl);

            WireCallout(ledCallout, ledAnchor, ctrl.PlaceLed);
            WireCallout(resistorCallout, resistorAnchor, ctrl.PlaceResistor);
            WireCallout(wireCallout, wireAnchor, ctrl.ConnectWire);

            WireAudioTrigger(welcomeAudio, ctrl.PlayWelcomeAudio, ctrl.StopAudio);
            WireAudioTrigger(placeLedAudio, ctrl.PlayPlaceLedAudio, ctrl.StopAudio);
            WireAudioTrigger(placeResistorAudio, ctrl.PlayPlaceResistorAudio, ctrl.StopAudio);
            WireAudioTrigger(connectWireAudio, ctrl.PlayConnectWireAudio, ctrl.StopAudio);
            WireAudioTrigger(workspaceAudio, ctrl.PlayWorkspaceAudio, ctrl.StopWorkspaceAudio);
            WireAudioTrigger(circuitCompleteAudio, ctrl.PlayCircuitCompleteAudio, ctrl.StopAudio);

            // Step 1's NEXT keeps its existing ShowScreen listener from Build UI 04; the controller
            // only toggles interactable. Step 2-3's NEXT has no build-time listener any more — it's
            // wired here to FinishLedCircuit() instead of a plain screen nav (see that builder's
            // comment). The LED card keeps its existing ShowScreen listener too, plus a second
            // listener added here to reset the active mode. Start disabled so the saved scene
            // matches the runtime gate. (TAKE QUIZ on screen 07 is no longer gated — left interactable.)
            step1Next.interactable = false;
            step2Next.interactable = false;
            ledCardButton.interactable = false;
            EditorUtility.SetDirty(step1Next);
            EditorUtility.SetDirty(step2Next);
            EditorUtility.SetDirty(ledCardButton);

            ClearPersistentListeners(step2Next.onClick);
            UnityEventTools.AddPersistentListener(step2Next.onClick, ctrl.FinishLedCircuit);

            UnityEventTools.AddPersistentListener(ledCardButton.onClick, ctrl.SelectLedMode);

            ClearPersistentListeners(checkButton.onClick);
            UnityEventTools.AddPersistentListener(checkButton.onClick, ctrl.CheckCircuit);

            var handler = imageTarget.GetComponent<DefaultObserverEventHandler>();
            if (handler != null)
            {
                // Two listeners each: the Phase 4 ARContent show/hide, plus the AR Camera screen's
                // real-detection-driven pill + transition (replacing the old tap-to-simulate hotspot).
                ClearPersistentListeners(handler.OnTargetFound);
                UnityEventTools.AddPersistentListener(handler.OnTargetFound, ctrl.ShowARContent);
                UnityEventTools.AddPersistentListener(handler.OnTargetFound, ctrl.HandleMarkerFound);

                ClearPersistentListeners(handler.OnTargetLost);
                UnityEventTools.AddPersistentListener(handler.OnTargetLost, ctrl.HideARContent);
                UnityEventTools.AddPersistentListener(handler.OnTargetLost, ctrl.HandleMarkerLost);
            }
            else
            {
                Debug.LogWarning("[CircuitAR] No DefaultObserverEventHandler found on ImageTarget — " +
                    "ARContent show/hide was not wired. Add it manually (Vuforia adds this automatically " +
                    "to an Image Target by default) and re-run this menu item.");
            }

            EditorSceneManager.MarkSceneDirty(active);
            Selection.activeGameObject = ctrlGO;
            Debug.Log("[CircuitAR] LED circuit wired: real-detection pill + transition on screen 02, " +
                       "ARContent show/hide gated on CircuitMode.Led, guided placement on screens 04/05, " +
                       "Step 2-3 NEXT now calls FinishLedCircuit() (sets the shared Complete screen's " +
                       "text, skips 06A), all audio triggers wired, the LED circuit card (03) stays " +
                       "disabled until its clip finishes and also resets the active mode. Run menu items " +
                       "4 (Series) and 5 (Parallel) too if you haven't. Save the scene.");
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

        // Canvas_* roots are scene-root GameObjects that UIScreenManager deactivates when not the
        // current screen, and GameObject.Find skips inactive objects — so search roots directly.
        private static GameObject FindRoot(Scene scene, string name) =>
            scene.GetRootGameObjects().FirstOrDefault(go => go.name == name);
    }
}
