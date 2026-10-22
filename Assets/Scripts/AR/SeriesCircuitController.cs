using System.Collections;
using CircuitAR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.AR
{
    // Series Circuit: Battery(+) -> Resistor1 -> LED1 -> Resistor2 -> LED2 -> Battery(-), a single
    // chain. Same tray/tap-callout/animate-to-slot and wire-draw engine as CircuitDemoController's
    // LED Circuit (PlaceableObject / WireConnector / PlacementCallout / ScreenAudioTrigger reused
    // as-is) — this class is just the per-mode orchestration, same pattern as CircuitDemoController
    // itself. Wired by Tools > CircuitAR > AR > 4 Wire Series Circuit — re-run after rebuilding
    // ARContent_Series or any Canvas_SeriesStep* screen.
    public class SeriesCircuitController : MonoBehaviour
    {
        private const string CompleteScreenName = "Canvas_FinalStepComplete";

        [Header("Animation")]
        public float moveDuration = 0.65f;
        public float moveArcHeight = 0.03f;
        public float wireDrawDuration = 0.8f;
        public float wirePulseDuration = 0.35f;
        public float confirmHoldDuration = 0.6f;
        public float arContentHideDelay = 1.0f;

        [Header("AR content")]
        public GameObject arContentRoot;
        public PlaceableObject battery;
        public PlaceableObject resistor1;
        public PlaceableObject led1;
        public LedGlow led1Glow;
        public PlaceableObject resistor2;
        public PlaceableObject led2;
        public LedGlow led2Glow;
        public WireConnector wireBattToR1;
        public WireConnector wireR1ToLed1;
        public WireConnector wireLed1ToR2;
        public WireConnector wireR2ToLed2;
        public WireConnector wireLed2ToBatt;

        [Header("Step 1 — battery to first resistor")]
        public PlacementCallout placeBatteryCallout;
        public PlacementCallout placeR1Callout;
        public PlacementCallout wireBattR1Callout;
        public Button step1NextButton;

        [Header("Step 2 — resistor to LED to resistor")]
        public PlacementCallout placeLed1Callout;
        public PlacementCallout wireR1Led1Callout;
        public PlacementCallout placeR2Callout;
        public PlacementCallout wireLed1R2Callout;
        public Button step2NextButton;

        [Header("Step 3 — complete the circuit")]
        public PlacementCallout placeLed2Callout;
        public PlacementCallout wireR2Led2Callout;
        public PlacementCallout wireLed2BattCallout;
        public Button step3NextButton;

        [Header("Audio")]
        public AudioClip audioStep1;
        public AudioClip audioStep2;
        public AudioClip audioStep3;
        public AudioClip audioStep4;

        public UIScreenManager screenManager;
        public FinalStepCompleteTextOverride completeTextOverride;

        private AudioSource audioSource;
        private Coroutine hideArContentRoutine;
        private bool busy;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        // Mirrors CircuitDemoController.Start()/ResetDemo() — without this, every callout built
        // onto the Series screens stays active (nothing hides the later-step ones until a mode is
        // selected), so they all stack at their shared default position instead of only the current
        // sub-step's callout showing.
        private void Start()
        {
            ResetCircuit();
        }

        public void PlayClip(AudioClip clip)
        {
            if (clip == null || CircuitDemoController.IsAudioMuted || audioSource == null) return;
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void StopAudio()
        {
            if (audioSource != null) audioSource.Stop();
        }

        public void PlayStep1Audio() => PlayClip(audioStep1);
        public void PlayStep2Audio() => PlayClip(audioStep2);
        public void PlayStep3Audio() => PlayClip(audioStep3);

        // Mirrors CircuitDemoController.ShowARContent/HideARContent exactly, gated on
        // CircuitModeState so this only shows the series board when Series mode is selected.
        public void ShowARContent()
        {
            if (CircuitModeState.Active != CircuitMode.Series) return;
            if (hideArContentRoutine != null) { StopCoroutine(hideArContentRoutine); hideArContentRoutine = null; }
            if (arContentRoot != null) arContentRoot.SetActive(true);
        }

        public void HideARContent()
        {
            if (CircuitModeState.Active != CircuitMode.Series) return;
            if (hideArContentRoutine != null) StopCoroutine(hideArContentRoutine);
            hideArContentRoutine = StartCoroutine(HideARContentAfterDelay());
        }

        private IEnumerator HideARContentAfterDelay()
        {
            yield return new WaitForSeconds(arContentHideDelay);
            hideArContentRoutine = null;
            if (arContentRoot != null) arContentRoot.SetActive(false);
        }

        // Wired to the Workspace screen's SERIES card.
        // Switches the visible 3D content immediately instead of waiting for the next
        // OnTargetFound (which won't fire again if the marker is already tracked from a
        // previous mode — this is why the board previously stayed invisible after switching).
        public void SelectSeriesMode()
        {
            CircuitModeState.Active = CircuitMode.Series;
            HideSiblingContent();
            ResetCircuit();
            ShowARContent();
        }

        // ARContent / ARContent_Series / ARContent_Parallel are all direct siblings under
        // ImageTarget (built by ARContentSceneBuilder) — deactivating everyone except our own root
        // this way needs no cross-controller references and works regardless of wiring order.
        private void HideSiblingContent()
        {
            if (arContentRoot == null) return;
            var parent = arContentRoot.transform.parent;
            if (parent == null) return;
            foreach (Transform sibling in parent)
                if (sibling.gameObject != arContentRoot) sibling.gameObject.SetActive(false);
        }

        public void ResetCircuit()
        {
            StopAllCoroutines();
            busy = false;
            if (battery != null) battery.ResetToTray();
            if (resistor1 != null) resistor1.ResetToTray();
            if (led1 != null) led1.ResetToTray();
            if (resistor2 != null) resistor2.ResetToTray();
            if (led2 != null) led2.ResetToTray();
            if (led1Glow != null) led1Glow.SetOn(false);
            if (led2Glow != null) led2Glow.SetOn(false);
            if (wireBattToR1 != null) wireBattToR1.ResetToTray();
            if (wireR1ToLed1 != null) wireR1ToLed1.ResetToTray();
            if (wireLed1ToR2 != null) wireLed1ToR2.ResetToTray();
            if (wireR2ToLed2 != null) wireR2ToLed2.ResetToTray();
            if (wireLed2ToBatt != null) wireLed2ToBatt.ResetToTray();
            if (placeBatteryCallout != null) placeBatteryCallout.ResetState();
            if (placeR1Callout != null) placeR1Callout.ResetState();
            if (wireBattR1Callout != null) wireBattR1Callout.ResetState();
            if (placeLed1Callout != null) placeLed1Callout.ResetState();
            if (wireR1Led1Callout != null) wireR1Led1Callout.ResetState();
            if (placeR2Callout != null) placeR2Callout.ResetState();
            if (wireLed1R2Callout != null) wireLed1R2Callout.ResetState();
            if (placeLed2Callout != null) placeLed2Callout.ResetState();
            if (wireR2Led2Callout != null) wireR2Led2Callout.ResetState();
            if (wireLed2BattCallout != null) wireLed2BattCallout.ResetState();
            RefreshStepUI();
        }

        private void RefreshStepUI()
        {
            if (placeBatteryCallout != null) placeBatteryCallout.SetVisible(!IsBatteryOk);
            if (placeR1Callout != null) placeR1Callout.SetVisible(IsBatteryOk && !IsR1Ok);
            if (wireBattR1Callout != null) wireBattR1Callout.SetVisible(IsR1Ok && !IsWireBattR1Ok);
            if (step1NextButton != null) step1NextButton.interactable = IsWireBattR1Ok;

            if (placeLed1Callout != null) placeLed1Callout.SetVisible(!IsLed1Ok);
            if (wireR1Led1Callout != null) wireR1Led1Callout.SetVisible(IsLed1Ok && !IsWireR1Led1Ok);
            if (placeR2Callout != null) placeR2Callout.SetVisible(IsWireR1Led1Ok && !IsR2Ok);
            if (wireLed1R2Callout != null) wireLed1R2Callout.SetVisible(IsR2Ok && !IsWireLed1R2Ok);
            if (step2NextButton != null) step2NextButton.interactable = IsWireLed1R2Ok;

            if (placeLed2Callout != null) placeLed2Callout.SetVisible(!IsLed2Ok);
            if (wireR2Led2Callout != null) wireR2Led2Callout.SetVisible(IsLed2Ok && !IsWireR2Led2Ok);
            if (wireLed2BattCallout != null) wireLed2BattCallout.SetVisible(IsWireR2Led2Ok && !IsWireLed2BattOk);
            if (step3NextButton != null) step3NextButton.interactable = IsWireLed2BattOk;
        }

        public bool IsBatteryOk => battery != null && battery.IsPlaced;
        public bool IsR1Ok => resistor1 != null && resistor1.IsPlaced;
        public bool IsWireBattR1Ok => wireBattToR1 != null && wireBattToR1.IsPlaced;
        public bool IsLed1Ok => led1 != null && led1.IsPlaced;
        public bool IsWireR1Led1Ok => wireR1ToLed1 != null && wireR1ToLed1.IsPlaced;
        public bool IsR2Ok => resistor2 != null && resistor2.IsPlaced;
        public bool IsWireLed1R2Ok => wireLed1ToR2 != null && wireLed1ToR2.IsPlaced;
        public bool IsLed2Ok => led2 != null && led2.IsPlaced;
        public bool IsWireR2Led2Ok => wireR2ToLed2 != null && wireR2ToLed2.IsPlaced;
        public bool IsWireLed2BattOk => wireLed2ToBatt != null && wireLed2ToBatt.IsPlaced;

        // One tap handler per callout, each gated on the previous sub-step so only one callout is
        // ever actionable at a time (matches RefreshStepUI's visibility gating above).
        public void PlaceBattery() { if (busy || IsBatteryOk) return; StartCoroutine(MoveRoutine(battery, placeBatteryCallout)); }
        public void PlaceR1() { if (busy || !IsBatteryOk || IsR1Ok) return; StartCoroutine(MoveRoutine(resistor1, placeR1Callout)); }
        public void ConnectBattToR1() { if (busy || !IsR1Ok || IsWireBattR1Ok) return; StartCoroutine(WireRoutine(wireBattToR1, wireBattR1Callout)); }
        public void PlaceLed1() { if (busy || IsLed1Ok) return; StartCoroutine(MoveRoutine(led1, placeLed1Callout)); }
        public void ConnectR1ToLed1() { if (busy || !IsLed1Ok || IsWireR1Led1Ok) return; StartCoroutine(WireRoutine(wireR1ToLed1, wireR1Led1Callout)); }
        public void PlaceR2() { if (busy || !IsWireR1Led1Ok || IsR2Ok) return; StartCoroutine(MoveRoutine(resistor2, placeR2Callout)); }
        public void ConnectLed1ToR2() { if (busy || !IsR2Ok || IsWireLed1R2Ok) return; StartCoroutine(WireRoutine(wireLed1ToR2, wireLed1R2Callout)); }
        public void PlaceLed2() { if (busy || IsLed2Ok) return; StartCoroutine(MoveRoutine(led2, placeLed2Callout)); }
        public void ConnectR2ToLed2() { if (busy || !IsLed2Ok || IsWireR2Led2Ok) return; StartCoroutine(WireRoutine(wireR2ToLed2, wireR2Led2Callout)); }
        public void ConnectLed2ToBattery() { if (busy || !IsWireR2Led2Ok || IsWireLed2BattOk) return; StartCoroutine(WireRoutine(wireLed2ToBatt, wireLed2BattCallout)); }

        private IEnumerator MoveRoutine(PlaceableObject part, PlacementCallout callout)
        {
            busy = true;
            if (callout != null) callout.SetBusy();
            yield return StartCoroutine(part.AnimateToSlot(moveDuration, moveArcHeight));
            yield return ConfirmRoutine(callout);
        }

        private IEnumerator WireRoutine(WireConnector wire, PlacementCallout callout)
        {
            busy = true;
            if (callout != null) callout.SetBusy();
            yield return StartCoroutine(wire.AnimateConnect(wireDrawDuration, wirePulseDuration));
            yield return ConfirmRoutine(callout);
        }

        private IEnumerator ConfirmRoutine(PlacementCallout callout)
        {
            if (callout != null) callout.ShowConfirmed();
            yield return new WaitForSeconds(confirmHoldDuration);
            busy = false;
            RefreshStepUI();
        }

        // Wired to Step 3's NEXT button (only enabled once the whole chain is connected, so
        // there's nothing left to "check" — same reasoning as the LED Circuit's Step 2-3 NEXT).
        public void FinishCircuit()
        {
            if (led1Glow != null) led1Glow.SetOn(true);
            if (led2Glow != null) led2Glow.SetOn(true);
            if (completeTextOverride != null)
                completeTextOverride.SetText("✓ CIRCUIT COMPLETE", "THE LEDs ARE ON",
                    "Great! Your series circuit is complete. Observe the glowing LEDs.",
                    "SERIES CIRCUIT");

            var mgr = screenManager != null ? screenManager : FindObjectOfType<UIScreenManager>();
            if (mgr == null)
            {
                Debug.LogWarning("[CircuitAR] SeriesCircuitController.FinishCircuit(): no UIScreenManager found.");
                return;
            }
            // ShowScreen deactivates Canvas_SeriesStep3 first, which fires that screen's
            // ScreenAudioTrigger.onHide -> StopAudio() on this same AudioSource — playing
            // audioStep4 BEFORE this line used to get it killed the instant it started.
            mgr.ShowScreen(CompleteScreenName);
            PlayClip(audioStep4);
        }
    }
}
