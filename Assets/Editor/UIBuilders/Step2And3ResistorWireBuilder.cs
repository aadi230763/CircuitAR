using CircuitAR.Audio;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "05 — Step 2 + 3: Resistor & Wire"
    public static class Step2And3ResistorWireBuilder
    {
        [MenuItem("Tools/Build UI/05 Step 2-3 Resistor Wire")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Step2, "UI_Step2And3ResistorWireRoot", "Plate_05_ResistorWire.png", gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.Step1);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "LED CIRCUIT", 180f, new[] { Green, Green, Accent, White(0.25f) });
            TrackingPill(c);

            // Plays Audio_PlaceResistor as soon as this screen appears (no delay). Wired to
            // CircuitDemoController by Tools > CircuitAR > AR > 3 Wire Circuit Demo.
            s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();

            var green = new Color(Green.r, Green.g, Green.b, 0.75f);
            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Leader(c, "Leader_Correct", 152.42f, 248f, 152.42f, 260.2f, Green);
            Leader(c, "Leader_Drop", 257.26f, 200f, 257.26f, 217.2f, Accent);
            WorldLabel(c, "Label_CorrectConnection", 30f, 222f, "✓ CORRECT CONNECTION", Green, green, 10f, W.ExtraBold, 0.6f);
            // Two callouts share the Figma "DROP ON D14" spot; CircuitDemoController shows only one
            // at a time (resistor first, then wire). Wired by Tools > CircuitAR > AR > 3 Wire Circuit Demo.
            Callout(c, "Callout_PlaceResistor", 214f, 174f, "PLACE RESISTOR · D11", "✓ RESISTOR PLACED", TextLightBlue, blue);
            var wireCallout = Callout(c, "Callout_ConnectWire", 214f, 174f, "DROP ON D14 · LED (+)", "✓ WIRE CONNECTED", TextLightBlue, blue);

            // Plays Audio_ConnectWire exactly when this callout becomes visible — i.e. once the
            // resistor is placed and the wire sub-step begins (PlacementCallout.SetVisible is a
            // plain SetActive(), so OnEnable fires at the right moment without extra bookkeeping).
            wireCallout.gameObject.AddComponent<ScreenAudioTrigger>();

            var p = BeginPanel(c, "Panel_Instruction", 16f, 571f, 358f);
            PDoneRow(p, "STEP 2 · PLACE THE RESISTOR", "DONE");
            PStepChip(p, "STEP 3 OF 4", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "CONNECT THE LED");
            PBody(p, "Tap PLACE RESISTOR, then tap DROP ON D14 to run the jumper wire from the resistor row (D11) to the LED’s + leg.");
            PButtonRow(p, "RESET", "NEXT  →", out var reset, out _);
            EndPanel(p);

            Wire(reset, ScreenNames.Step1);
            // Skips the Final Step Incomplete / CHECK CIRCUIT screen entirely: this button only
            // enables once resistor + wire are placed, and Step 1's NEXT already requires the LED
            // to be placed before this screen is even reachable — so by the time this is tappable
            // the circuit is guaranteed complete, and there's nothing left to "check." Wired to
            // CircuitDemoController.FinishLedCircuit() (not a plain screen nav) by
            // Tools > CircuitAR > AR > 3 Wire Circuit Demo, so the shared Complete screen's text
            // resets to the LED copy even if Series/Parallel last overwrote it.

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
