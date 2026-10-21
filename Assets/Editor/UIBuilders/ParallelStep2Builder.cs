using CircuitAR.Audio;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Parallel Circuit — Step 2: build both branches (resistor + LED in series, each branch).
    public static class ParallelStep2Builder
    {
        [MenuItem("Tools/Build UI/P2 Parallel Step 2")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.ParallelStep2, "UI_ParallelStep2Root", null, gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.ParallelStep1);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "PARALLEL CIRCUIT", 180f, new[] { Green, Accent, White(0.25f) });
            TrackingPill(c);

            s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();

            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Callout(c, "Callout_PlaceLed1", 195f, 400f, "PLACE LED 1", "✓ LED 1 PLACED", White(1f), blue);
            Callout(c, "Callout_ConnectR1Led1", 195f, 400f, "WIRE BRANCH 1", "✓ BRANCH 1 DONE", TextLightBlue, blue);
            Callout(c, "Callout_PlaceR2", 195f, 400f, "PLACE RESISTOR 2", "✓ RESISTOR 2 PLACED", White(1f), blue);
            Callout(c, "Callout_PlaceLed2", 195f, 400f, "PLACE LED 2", "✓ LED 2 PLACED", White(1f), blue);
            Callout(c, "Callout_ConnectR2Led2", 195f, 400f, "WIRE BRANCH 2", "✓ BRANCH 2 DONE", TextLightBlue, blue);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 571f, 358f);
            PDoneRow(p, "STEP 1 · BATTERY TO POSITIVE RAIL", "DONE");
            PStepChip(p, "STEP 2 OF 3", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "BUILD BOTH BRANCHES");
            PBody(p, "Create two separate branches, each with a resistor and an LED connected in series.");
            PButtonRow(p, "RESET", "NEXT  →", out var reset, out var next);
            EndPanel(p);

            Wire(reset, ScreenNames.ParallelStep1);
            Wire(next, ScreenNames.ParallelStep3);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
