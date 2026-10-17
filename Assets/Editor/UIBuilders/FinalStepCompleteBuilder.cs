using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "06B — Circuit Complete: LED ON"
    public static class FinalStepCompleteBuilder
    {
        [MenuItem("Tools/Build UI/06B Final Step Complete")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Complete, "UI_FinalStepCompleteRoot", "Plate_06B_Complete.png", gradientBg: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.Incomplete);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            SessionPill(c, "LED CIRCUIT", 180f, new[] { Green, Green, Green, Green });
            TrackingPill(c);
            CircuitStatusPill(c, complete: true);

            var red = new Color(Red.r, Red.g, Red.b, 0.75f);
            var green = new Color(Green.r, Green.g, Green.b, 0.75f);
            Leader(c, "Leader_LEDOn", 262f, 203f, 228.9f, 228.2f, Red);
            Leader(c, "Leader_FinalWire", 233.95f, 436f, 233.95f, 387.4f, Green);
            MultiLabelBox(c, "Label_LEDOn", 250f, 176f,
                new[] { Run.Icon(BulbIcon(), 12f, White(1f)), Run.T("LED ON", 11f, W.ExtraBold, White(1f), 0.66f) },
                5f, 10f, 6f, -1f, PillBg, red);
            WorldLabel(c, "Label_FinalWireConnected", 176f, 436f, "✓ FINAL WIRE CONNECTED", Green, green, 10f, W.ExtraBold, 0.6f);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 549f, 358f);
            PStepChip(p, "✓ CIRCUIT COMPLETE", Green, Rgba(46, 213, 115, 0.18f));
            PTitle(p, "THE LED IS ON");
            PBody(p, "All required connections were detected. The loop is closed, so the LED switches on.");
            PChecklist(p, new[] { "BATTERY", "RESISTOR", "LED", "RETURN PATH" }, new[] { true, true, true, true });
            PButtonRow(p, "RESET", "HOW IT WORKS  →", out var reset, out var how);
            EndPanel(p);

            Wire(reset, ScreenNames.Step1);
            Wire(how, ScreenNames.HowItWorks);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
