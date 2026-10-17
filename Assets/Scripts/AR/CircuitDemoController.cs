using System.Collections;
using CircuitAR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.AR
{
    // Placeholder-pass demo controller: shows/hides the 3D AR content with the marker, runs the
    // guided placement sequence (tap a callout → animate the part from the tray onto the board →
    // confirm → enable NEXT), and decides whether "CHECK CIRCUIT" on the Final Step is complete.
    // Wired up by Tools > CircuitAR > AR > 3 Wire Circuit Demo — the references below go stale
    // if ARContent or the Step1/Step2/Final Step Incomplete canvases are rebuilt, so re-run that
    // menu item after rebuilding any of them.
    public class CircuitDemoController : MonoBehaviour
    {
        private const string CompleteScreenName = "Canvas_FinalStepComplete";   // must match ScreenNames.Complete
        private const string ARCameraScreenName = "Canvas_ARCamera";           // must match ScreenNames.ARCamera
        private const string WorkspaceScreenName = "Canvas_MarkerDetectedWorkspace"; // must match ScreenNames.Workspace

        [Header("Animation")]
        public float moveDuration = 0.65f;
        public float moveArcHeight = 0.03f;
        public float wireDrawDuration = 0.8f;
        public float wirePulseDuration = 0.35f;
        public float confirmHoldDuration = 0.6f;
        public float detectionHoldDuration = 0.8f;

        [Header("AR content")]
        public GameObject arContentRoot;
        public PlaceableObject led;
        public PlaceableObject resistor;
        public WireConnector wire;

        [Header("AR Camera screen")]
        public DetectionStatusPill detectionPill;

        [Header("Step screens")]
        public PlacementCallout ledCallout;
        public PlacementCallout resistorCallout;
        public PlacementCallout wireCallout;
        public Button step1NextButton;
        public Button step2NextButton;

        // Final Step Incomplete checklist chips. Battery has no placement step — it's always ok.
        [Header("Final step")]
        public ChecklistItemView batteryChecklistItem;
        public ChecklistItemView resistorChecklistItem;
        public ChecklistItemView ledChecklistItem;
        public ChecklistItemView returnPathChecklistItem;
        public UIScreenManager screenManager;

        private bool busy;
        private bool arCameraTransitionPending;

        private void Start()
        {
            ResetDemo();
        }

        public void ResetDemo()
        {
            StopAllCoroutines();
            busy = false;
            arCameraTransitionPending = false;
            if (led != null) led.ResetToTray();
            if (resistor != null) resistor.ResetToTray();
            if (wire != null) wire.ResetToTray();
            if (ledCallout != null) ledCallout.ResetState();
            if (resistorCallout != null) resistorCallout.ResetState();
            if (wireCallout != null) wireCallout.ResetState();
            if (detectionPill != null) detectionPill.SetSearching();
            RefreshStepUI();
        }

        // Wired to the ImageTarget's DefaultObserverEventHandler.OnTargetFound / OnTargetLost
        // (alongside HandleMarkerFound/HandleMarkerLost below — both fire on the same events).
        public void ShowARContent()
        {
            if (arContentRoot != null) arContentRoot.SetActive(true);
        }

        public void HideARContent()
        {
            if (arContentRoot != null) arContentRoot.SetActive(false);
        }

        // Real detection driving the AR Camera screen: only acts while that screen is the one
        // currently shown, and only once per detection (guarded so a flickering OnTargetFound
        // during the hold can't start a second overlapping transition).
        public void HandleMarkerFound()
        {
            if (arCameraTransitionPending) return;
            if (screenManager == null || !screenManager.IsScreenActive(ARCameraScreenName)) return;

            arCameraTransitionPending = true;
            if (detectionPill != null) detectionPill.PlayFoundAnimation(detectionHoldDuration);
            StartCoroutine(ArCameraDetectedRoutine());
        }

        private IEnumerator ArCameraDetectedRoutine()
        {
            yield return new WaitForSeconds(detectionHoldDuration);
            arCameraTransitionPending = false;
            if (screenManager != null) screenManager.ShowScreen(WorkspaceScreenName);
        }

        // While still on the AR Camera screen (i.e. before the transition above has fired), losing
        // tracking just reverts the pill to "searching" — it never navigates, so once the player
        // has moved past this screen, losing tracking elsewhere is untouched by this method.
        public void HandleMarkerLost()
        {
            if (screenManager != null && screenManager.IsScreenActive(ARCameraScreenName) && detectionPill != null)
                detectionPill.SetSearching();
        }

        // Wired to Callout_PlaceLED (screen 04).
        public void PlaceLed()
        {
            if (busy || led == null || led.IsPlaced) return;
            StartCoroutine(MoveRoutine(led, ledCallout));
        }

        // Wired to Callout_PlaceResistor (screen 05, shown first).
        public void PlaceResistor()
        {
            if (busy || resistor == null || resistor.IsPlaced) return;
            StartCoroutine(MoveRoutine(resistor, resistorCallout));
        }

        // Wired to Callout_ConnectWire (screen 05, shown once the resistor is placed).
        public void ConnectWire()
        {
            if (busy || wire == null || wire.IsPlaced || !IsResistorOk) return;
            StartCoroutine(WireRoutine());
        }

        private IEnumerator MoveRoutine(PlaceableObject part, PlacementCallout callout)
        {
            busy = true;
            if (callout != null) callout.SetBusy();
            yield return StartCoroutine(part.AnimateToSlot(moveDuration, moveArcHeight));
            yield return ConfirmRoutine(callout);
        }

        private IEnumerator WireRoutine()
        {
            busy = true;
            if (wireCallout != null) wireCallout.SetBusy();
            yield return StartCoroutine(wire.AnimateConnect(wireDrawDuration, wirePulseDuration));
            yield return ConfirmRoutine(wireCallout);
        }

        private IEnumerator ConfirmRoutine(PlacementCallout callout)
        {
            if (callout != null) callout.ShowConfirmed();
            yield return new WaitForSeconds(confirmHoldDuration);
            busy = false;
            RefreshStepUI();
        }

        // One callout per step at a time; NEXT stays disabled until that screen's placement is done.
        private void RefreshStepUI()
        {
            if (ledCallout != null) ledCallout.SetVisible(!IsLedOk);
            if (resistorCallout != null) resistorCallout.SetVisible(!IsResistorOk);
            if (wireCallout != null) wireCallout.SetVisible(IsResistorOk && !IsReturnPathOk);
            if (step1NextButton != null) step1NextButton.interactable = IsLedOk;
            if (step2NextButton != null) step2NextButton.interactable = IsResistorOk && IsReturnPathOk;
        }

        public bool IsLedOk => led != null && led.IsPlaced;
        public bool IsResistorOk => resistor != null && resistor.IsPlaced;
        public bool IsReturnPathOk => wire != null && wire.IsPlaced;
        public bool IsCircuitComplete => IsLedOk && IsResistorOk && IsReturnPathOk;

        // Wired to the Final Step Incomplete screen's "CHECK CIRCUIT" button.
        public void CheckCircuit()
        {
            UpdateChecklist();
            if (!IsCircuitComplete) return;

            var mgr = screenManager != null ? screenManager : FindObjectOfType<UIScreenManager>();
            if (mgr == null)
            {
                Debug.LogWarning("[CircuitAR] CircuitDemoController.CheckCircuit(): no UIScreenManager found — can't show the Complete screen.");
                return;
            }
            mgr.ShowScreen(CompleteScreenName);
        }

        private void UpdateChecklist()
        {
            if (batteryChecklistItem != null) batteryChecklistItem.SetOk(true);
            if (resistorChecklistItem != null) resistorChecklistItem.SetOk(IsResistorOk);
            if (ledChecklistItem != null) ledChecklistItem.SetOk(IsLedOk);
            if (returnPathChecklistItem != null) returnPathChecklistItem.SetOk(IsReturnPathOk);
        }
    }
}
