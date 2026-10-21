using CircuitAR.Audio;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Series Circuit — Step 2: first resistor to the LED(s) to the second resistor.
    public static class SeriesStep2Builder
    {
        [MenuItem("Tools/Build UI/S2 Series Step 2")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.SeriesStep2, "UI_SeriesStep2Root", null, gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.SeriesStep1);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "SERIES CIRCUIT", 180f, new[] { Green, Accent, White(0.25f) });
            TrackingPill(c);

            s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();

            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Callout(c, "Callout_PlaceLed1", 195f, 400f, "PLACE LED 1", "✓ LED 1 PLACED", White(1f), blue);
            Callout(c, "Callout_ConnectR1Led1", 195f, 400f, "CONNECT TO LED 1", "✓ CONNECTED", TextLightBlue, blue);
            Callout(c, "Callout_PlaceR2", 195f, 400f, "PLACE RESISTOR 2", "✓ RESISTOR 2 PLACED", White(1f), blue);
            Callout(c, "Callout_ConnectLed1R2", 195f, 400f, "CONNECT TO RESISTOR 2", "✓ CONNECTED", TextLightBlue, blue);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 571f, 358f);
            PDoneRow(p, "STEP 1 · BATTERY TO RESISTOR 1", "DONE");
            PStepChip(p, "STEP 2 OF 3", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "WIRE THE MIDDLE");
            PBody(p, "Connect the first resistor to the LED, then connect the LED to the second resistor.");
            PButtonRow(p, "RESET", "NEXT  →", out var reset, out var next);
            EndPanel(p);

            Wire(reset, ScreenNames.SeriesStep1);
            Wire(next, ScreenNames.SeriesStep3);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
