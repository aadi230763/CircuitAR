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
            PChecklist(p, new[] { "BATTERY", "RESISTOR", "LED", "RETURN PATH" }, new[] { true, true, true, false });
            PButtonRow(p, "RESET", "CHECK CIRCUIT", out var reset, out var check);
            EndPanel(p);

            Wire(reset, ScreenNames.Step1);
            Wire(check, ScreenNames.Complete);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
