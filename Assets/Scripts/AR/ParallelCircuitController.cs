using System.Collections;
using CircuitAR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.AR
{
    // Parallel Circuit: Battery feeds a positive junction shared by two branches
    // (Resistor1+LED1, Resistor2+LED2), which share a negative junction back to Battery(-). Same
    // reused engine as SeriesCircuitController / CircuitDemoController — see those for the
    // pattern this mirrors. Wired by Tools > CircuitAR > AR > 5 Wire Parallel Circuit — re-run
    // after rebuilding ARContent_Parallel or any Canvas_ParallelStep* screen.
    public class ParallelCircuitController : MonoBehaviour
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
        public WireConnector wireBattToR1;   // step 1: battery(+) to branch 1's positive junction
        public WireConnector wireR1ToLed1;   // step 2: branch 1 internal
        public WireConnector wireR2ToLed2;   // step 2: branch 2 internal
        public WireConnector wireR2ToR1;     // step 3: join both branches' positive side
        public WireConnector wireLed1ToLed2; // step 3: join both branches' negative side
        public WireConnector wireBattToLed1; // step 3: battery(-) to the shared negative junction

        [Header("Step 1 — battery to the positive rail")]
        public PlacementCallout placeBatteryCallout;
        public PlacementCallout placeR1Callout;
        public PlacementCallout wireBattR1Callout;
        public Button step1NextButton;

        [Header("Step 2 — build both branches")]
        public PlacementCallout placeLed1Callout;
        public PlacementCallout wireR1Led1Callout;
        public PlacementCallout placeR2Callout;
        public PlacementCallout placeLed2Callout;
        public PlacementCallout wireR2Led2Callout;
        public Button step2NextButton;

        [Header("Step 3 — join both branches to the rails")]
        public PlacementCallout wireR2R1Callout;
        public PlacementCallout wireLed1Led2Callout;
        public PlacementCallout wireBattLed1Callout;
        public Button step3NextButton;

        [Header("Audio")]
        public AudioClip audioStep1;
        public AudioClip audioStep2;
        public AudioClip audioStep3;
        public AudioClip audioStep4;
        public AudioClip audioQuiz;

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
        // onto the Parallel screens stays active (nothing hides the later-step ones until a mode is
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

        public void ShowARContent()
        {
            if (CircuitModeState.Active != CircuitMode.Parallel) return;
            if (hideArContentRoutine != null) { StopCoroutine(hideArContentRoutine); hideArContentRoutine = null; }
            if (arContentRoot != null) arContentRoot.SetActive(true);
        }

        public void HideARContent()
        {
            if (CircuitModeState.Active != CircuitMode.Parallel) return;
            if (hideArContentRoutine != null) StopCoroutine(hideArContentRoutine);
            hideArContentRoutine = StartCoroutine(HideARContentAfterDelay());
        }

        private IEnumerator HideARContentAfterDelay()
        {
            yield return new WaitForSeconds(arContentHideDelay);
            hideArContentRoutine = null;
            if (arContentRoot != null) arContentRoot.SetActive(false);
        }

        // Wired to the Workspace screen's PARALLEL card. Switches the visible 3D content
        // immediately instead of waiting for the next OnTargetFound (which won't fire again if
        // the marker is already tracked from a previous mode — this is why the board previously
        // stayed invisible after switching).
        public void SelectParallelMode()
        {
            CircuitModeState.Active = CircuitMode.Parallel;
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
            if (wireR2ToLed2 != null) wireR2ToLed2.ResetToTray();
            if (wireR2ToR1 != null) wireR2ToR1.ResetToTray();
            if (wireLed1ToLed2 != null) wireLed1ToLed2.ResetToTray();
            if (wireBattToLed1 != null) wireBattToLed1.ResetToTray();
            if (placeBatteryCallout != null) placeBatteryCallout.ResetState();
            if (placeR1Callout != null) placeR1Callout.ResetState();
            if (wireBattR1Callout != null) wireBattR1Callout.ResetState();
            if (placeLed1Callout != null) placeLed1Callout.ResetState();
            if (wireR1Led1Callout != null) wireR1Led1Callout.ResetState();
            if (placeR2Callout != null) placeR2Callout.ResetState();
            if (placeLed2Callout != null) placeLed2Callout.ResetState();
            if (wireR2Led2Callout != null) wireR2Led2Callout.ResetState();
            if (wireR2R1Callout != null) wireR2R1Callout.ResetState();
            if (wireLed1Led2Callout != null) wireLed1Led2Callout.ResetState();
            if (wireBattLed1Callout != null) wireBattLed1Callout.ResetState();
            RefreshStepUI();
        }

        private void RefreshStepUI()
        {
            if (placeBatteryCallout != null) placeBatteryCallout.SetVisible(!IsBatteryOk);
            if (placeR1Callout != null) placeR1Callout.SetVisible(IsBatteryOk && !IsR1Ok);
            if (wireBattR1Callout != null) wireBattR1Callout.SetVisible(IsR1Ok && !IsWireBattR1Ok);
            if (step1NextButton != null) step1NextButton.interactable = IsWireBattR1Ok;

            if (placeLed1Callout != null) placeLed1Callout.SetVisible(!IsLed1Ok);
            if (wireR1Led1Callout != null) wireR1Led1Callout.SetVisible(IsLed1Ok && !IsBranch1Ok);
            if (placeR2Callout != null) placeR2Callout.SetVisible(!IsR2Ok);
            if (placeLed2Callout != null) placeLed2Callout.SetVisible(IsR2Ok && !IsLed2Ok);
            if (wireR2Led2Callout != null) wireR2Led2Callout.SetVisible(IsLed2Ok && !IsBranch2Ok);
            if (step2NextButton != null) step2NextButton.interactable = IsBranch1Ok && IsBranch2Ok;

            if (wireR2R1Callout != null) wireR2R1Callout.SetVisible(!IsWireR2R1Ok);
            if (wireLed1Led2Callout != null) wireLed1Led2Callout.SetVisible(IsWireR2R1Ok && !IsWireLed1Led2Ok);
            if (wireBattLed1Callout != null) wireBattLed1Callout.SetVisible(IsWireLed1Led2Ok && !IsWireBattLed1Ok);
            if (step3NextButton != null) step3NextButton.interactable = IsWireBattLed1Ok;
        }

        public bool IsBatteryOk => battery != null && battery.IsPlaced;
        public bool IsR1Ok => resistor1 != null && resistor1.IsPlaced;
        public bool IsWireBattR1Ok => wireBattToR1 != null && wireBattToR1.IsPlaced;
        public bool IsLed1Ok => led1 != null && led1.IsPlaced;
        public bool IsBranch1Ok => wireR1ToLed1 != null && wireR1ToLed1.IsPlaced;
        public bool IsR2Ok => resistor2 != null && resistor2.IsPlaced;
        public bool IsLed2Ok => led2 != null && led2.IsPlaced;
        public bool IsBranch2Ok => wireR2ToLed2 != null && wireR2ToLed2.IsPlaced;
        public bool IsWireR2R1Ok => wireR2ToR1 != null && wireR2ToR1.IsPlaced;
        public bool IsWireLed1Led2Ok => wireLed1ToLed2 != null && wireLed1ToLed2.IsPlaced;
        public bool IsWireBattLed1Ok => wireBattToLed1 != null && wireBattToLed1.IsPlaced;

        public void PlaceBattery() { if (busy || IsBatteryOk) return; StartCoroutine(MoveRoutine(battery, placeBatteryCallout)); }
        public void PlaceR1() { if (busy || !IsBatteryOk || IsR1Ok) return; StartCoroutine(MoveRoutine(resistor1, placeR1Callout)); }
        public void ConnectBattToR1() { if (busy || !IsR1Ok || IsWireBattR1Ok) return; StartCoroutine(WireRoutine(wireBattToR1, wireBattR1Callout)); }
        public void PlaceLed1() { if (busy || IsLed1Ok) return; StartCoroutine(MoveRoutine(led1, placeLed1Callout)); }
        public void ConnectR1ToLed1() { if (busy || !IsLed1Ok || IsBranch1Ok) return; StartCoroutine(WireRoutine(wireR1ToLed1, wireR1Led1Callout)); }
        public void PlaceR2() { if (busy || IsR2Ok) return; StartCoroutine(MoveRoutine(resistor2, placeR2Callout)); }
        public void PlaceLed2() { if (busy || !IsR2Ok || IsLed2Ok) return; StartCoroutine(MoveRoutine(led2, placeLed2Callout)); }
        public void ConnectR2ToLed2() { if (busy || !IsLed2Ok || IsBranch2Ok) return; StartCoroutine(WireRoutine(wireR2ToLed2, wireR2Led2Callout)); }
        public void ConnectR2ToR1() { if (busy || !(IsBranch1Ok && IsBranch2Ok) || IsWireR2R1Ok) return; StartCoroutine(WireRoutine(wireR2ToR1, wireR2R1Callout)); }
        public void ConnectLed1ToLed2() { if (busy || !IsWireR2R1Ok || IsWireLed1Led2Ok) return; StartCoroutine(WireRoutine(wireLed1ToLed2, wireLed1Led2Callout)); }
        public void ConnectBattToLed1() { if (busy || !IsWireLed1Led2Ok || IsWireBattLed1Ok) return; StartCoroutine(WireRoutine(wireBattToLed1, wireBattLed1Callout)); }

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

        // Wired to Step 3's NEXT button.
        public void FinishCircuit()
        {
            if (led1Glow != null) led1Glow.SetOn(true);
            if (led2Glow != null) led2Glow.SetOn(true);
            if (completeTextOverride != null)
                completeTextOverride.SetText("✓ CIRCUIT COMPLETE", "BOTH LEDs ARE ON",
                    "Excellent! Your parallel circuit is complete. Check that both LEDs are glowing.",
                    "PARALLEL CIRCUIT");

            var mgr = screenManager != null ? screenManager : FindObjectOfType<UIScreenManager>();
            if (mgr == null)
            {
                Debug.LogWarning("[CircuitAR] ParallelCircuitController.FinishCircuit(): no UIScreenManager found.");
                return;
            }
            // ShowScreen deactivates Canvas_ParallelStep3 first, which fires that screen's
            // ScreenAudioTrigger.onHide -> StopAudio() on this same AudioSource — playing
            // audioStep4 BEFORE this line would get it killed the instant it started.
            mgr.ShowScreen(CompleteScreenName);
            StartCoroutine(PlayCompletionAudioSequence());
        }

        private IEnumerator PlayCompletionAudioSequence()
        {
            PlayClip(audioStep4);
            if (audioStep4 != null && !CircuitDemoController.IsAudioMuted)
                yield return new WaitForSeconds(audioStep4.length + 0.3f);
            PlayClip(audioQuiz);
        }
    }
}
