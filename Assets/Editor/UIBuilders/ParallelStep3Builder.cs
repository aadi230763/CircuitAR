using CircuitAR.Audio;
using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Parallel Circuit — Step 3: join both branches to the positive/negative rails and the
    // battery's negative terminal. NEXT here calls ParallelCircuitController.FinishCircuit() (not
    // a plain screen nav) — wired by Tools > CircuitAR > AR > 5 Wire Parallel Circuit.
    public static class ParallelStep3Builder
    {
        [MenuItem("Tools/Build UI/P3 Parallel Step 3")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.ParallelStep3, "UI_ParallelStep3Root", null, gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.ParallelStep2);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "PARALLEL CIRCUIT", 180f, new[] { Green, Green, Accent });
            TrackingPill(c);

            s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();

            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Callout(c, "Callout_JoinPositive", 195f, 400f, "JOIN POSITIVE RAIL", "✓ JOINED", TextLightBlue, blue);
            Callout(c, "Callout_JoinNegative", 195f, 400f, "JOIN NEGATIVE RAIL", "✓ JOINED", TextLightBlue, blue);
            Callout(c, "Callout_ConnectBatteryNeg", 195f, 400f, "CONNECT BATTERY (−)", "✓ CIRCUIT CLOSED", TextLightBlue, blue);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 571f, 358f);
            PDoneRow(p, "STEP 2 · BUILT BOTH BRANCHES", "DONE");
            PStepChip(p, "STEP 3 OF 3", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "JOIN THE RAILS");
            PBody(p, "Connect both branches between the positive and negative rails, then connect the battery's negative terminal to the negative rail.");
            PButtonRow(p, "RESET", "NEXT  →", out var reset, out _);
            EndPanel(p);

            Wire(reset, ScreenNames.ParallelStep1);
            // NEXT wired to ParallelCircuitController.FinishCircuit() by menu item 5.

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
