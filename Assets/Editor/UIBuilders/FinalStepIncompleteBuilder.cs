using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "06A — Final Step: Circuit Incomplete"
    public static class FinalStepIncompleteBuilder
    {
        [MenuItem("Tools/Build UI/06A Final Step Incomplete")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Incomplete, "UI_FinalStepIncompleteRoot", "Plate_06A_Incomplete.png", gradientBg: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.Step2);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            SessionPill(c, "LED CIRCUIT", 180f, new[] { Green, Green, Green, Accent });
            TrackingPill(c);
            CircuitStatusPill(c, complete: false);

            var amber = new Color(Amber.r, Amber.g, Amber.b, 0.75f);
            Leader(c, "Leader_MissingWire", 233.95f, 436f, 233.95f, 387.4f, Amber);
            WorldLabel(c, "Label_CheckConnection", 123f, 436f, "⚠ CHECK THE HIGHLIGHTED CONNECTION", Amber, amber, 10f, W.ExtraBold, 0.6f);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 549f, 358f);
            PStepChip(p, "FINAL STEP", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "COMPLETE THE CIRCUIT");
            PBody(p, "Connection incomplete. Connect the final wire: LED (−) leg → negative rail.");
            // Initial true/false values here are just the Figma placeholder look at build time —
            // CircuitDemoController overwrites them at runtime to match actual placement state
            // (wired by Tools > CircuitAR > AR > 3 Wire Circuit Demo).
            PDynamicChecklist(p, new[] { "Battery", "Resistor", "LED", "ReturnPath" },
                new[] { "BATTERY", "RESISTOR", "LED", "RETURN PATH" }, new[] { true, true, true, false });
            PButtonRow(p, "RESET", "CHECK CIRCUIT", out var reset, out _);
            EndPanel(p);

            Wire(reset, ScreenNames.Step1);
            // "CHECK CIRCUIT" is wired to CircuitDemoController.CheckCircuit() by
            // Tools > CircuitAR > AR > 3 Wire Circuit Demo — it decides whether to advance to
            // Canvas_FinalStepComplete based on actual placement state, not unconditionally.

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
