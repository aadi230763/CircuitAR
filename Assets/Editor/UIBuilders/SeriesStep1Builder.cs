using CircuitAR.Audio;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Series Circuit — Step 1: battery to the first resistor.
    public static class SeriesStep1Builder
    {
        [MenuItem("Tools/Build UI/S1 Series Step 1")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.SeriesStep1, "UI_SeriesStep1Root", null, gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.Workspace);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "SERIES CIRCUIT", 180f, new[] { Accent, White(0.25f), White(0.25f) });
            TrackingPill(c);

            // Plays Audio_Step1 as soon as this screen appears. Wired to SeriesCircuitController
            // by Tools > CircuitAR > AR > 4 Wire Series Circuit.
            s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();

            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Callout(c, "Callout_PlaceBattery", 195f, 400f, "PLACE BATTERY", "✓ BATTERY PLACED", White(1f), blue);
            Callout(c, "Callout_PlaceR1", 195f, 400f, "PLACE RESISTOR 1", "✓ RESISTOR 1 PLACED", White(1f), blue);
            Callout(c, "Callout_ConnectBattR1", 195f, 400f, "CONNECT TO RESISTOR 1", "✓ CONNECTED", TextLightBlue, blue);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 617f, 358f);
            PStepChip(p, "STEP 1 OF 3", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "CONNECT THE BATTERY");
            PBody(p, "Connect the positive terminal of the battery to the first resistor using a wire.");
            var next = PButton(p, "NEXT  →");
            EndPanel(p);
            Wire(next, ScreenNames.SeriesStep2);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
