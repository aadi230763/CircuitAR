using CircuitAR.Audio;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Series Circuit — Step 3: second resistor, LED 2, and back to the battery to close the loop.
    // NEXT here calls SeriesCircuitController.FinishCircuit() (not a plain screen nav) — wired by
    // Tools > CircuitAR > AR > 4 Wire Series Circuit.
    public static class SeriesStep3Builder
    {
        [MenuItem("Tools/Build UI/S3 Series Step 3")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.SeriesStep3, "UI_SeriesStep3Root", null, gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.SeriesStep2);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "SERIES CIRCUIT", 180f, new[] { Green, Green, Accent });
            TrackingPill(c);

            s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();

            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Callout(c, "Callout_PlaceLed2", 195f, 400f, "PLACE LED 2", "✓ LED 2 PLACED", White(1f), blue);
            Callout(c, "Callout_ConnectR2Led2", 195f, 400f, "CONNECT TO LED 2", "✓ CONNECTED", TextLightBlue, blue);
            Callout(c, "Callout_ConnectLed2Battery", 195f, 400f, "CONNECT TO BATTERY (−)", "✓ CIRCUIT CLOSED", TextLightBlue, blue);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 571f, 358f);
            PDoneRow(p, "STEP 2 · WIRED THE MIDDLE", "DONE");
            PStepChip(p, "STEP 3 OF 3", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "CLOSE THE LOOP");
            PBody(p, "Connect the second resistor to the negative terminal of the battery to complete the circuit.");
            PButtonRow(p, "RESET", "NEXT  →", out var reset, out _);
            EndPanel(p);

            Wire(reset, ScreenNames.SeriesStep1);
            // NEXT wired to SeriesCircuitController.FinishCircuit() by menu item 4 — only enables
            // once every connection above is made, so there's nothing left to "check."

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
