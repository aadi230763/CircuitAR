using CircuitAR.Audio;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Parallel Circuit — Step 1: battery to the positive rail (battery + first resistor + wire).
    public static class ParallelStep1Builder
    {
        [MenuItem("Tools/Build UI/P1 Parallel Step 1")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.ParallelStep1, "UI_ParallelStep1Root", null, gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.Workspace);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "PARALLEL CIRCUIT", 180f, new[] { Accent, White(0.25f), White(0.25f) });
            TrackingPill(c);

            s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();

            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Callout(c, "Callout_PlaceBattery", 195f, 400f, "PLACE BATTERY", "✓ BATTERY PLACED", White(1f), blue);
            Callout(c, "Callout_PlaceR1", 195f, 400f, "PLACE RESISTOR 1", "✓ RESISTOR 1 PLACED", White(1f), blue);
            Callout(c, "Callout_ConnectBattR1", 195f, 400f, "CONNECT TO POSITIVE RAIL", "✓ CONNECTED", TextLightBlue, blue);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 617f, 358f);
            PStepChip(p, "STEP 1 OF 3", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "CONNECT THE BATTERY");
            PBody(p, "Connect the battery's positive terminal to the positive rail of the breadboard.");
            var next = PButton(p, "NEXT  →");
            EndPanel(p);
            Wire(next, ScreenNames.ParallelStep2);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
