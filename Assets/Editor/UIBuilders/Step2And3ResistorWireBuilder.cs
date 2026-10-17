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
            SessionPill(c, "LED CIRCUIT", 180f, new[] { Green, Green, Accent, White(0.25f) });
            TrackingPill(c);

            var green = new Color(Green.r, Green.g, Green.b, 0.75f);
            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Leader(c, "Leader_Correct", 152.42f, 248f, 152.42f, 260.2f, Green);
            Leader(c, "Leader_Drop", 257.26f, 200f, 257.26f, 217.2f, Accent);
            WorldLabel(c, "Label_CorrectConnection", 30f, 222f, "✓ CORRECT CONNECTION", Green, green, 10f, W.ExtraBold, 0.6f);
            WorldLabel(c, "Label_DropOnD14", 214f, 174f, "DROP ON D14 · LED (+)", TextLightBlue, blue);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 571f, 358f);
            PDoneRow(p, "STEP 2 · PLACE THE RESISTOR", "DONE");
            PStepChip(p, "STEP 3 OF 4", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "CONNECT THE LED");
            PBody(p, "Drag the jumper wire from the resistor row (D11) to the LED’s + leg (D14).");
            PButtonRow(p, "RESET", "NEXT  →", out var reset, out var next);
            EndPanel(p);

            Wire(reset, ScreenNames.Step1);
            Wire(next, ScreenNames.Incomplete);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
