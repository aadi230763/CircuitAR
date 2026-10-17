using UnityEditor;
using UnityEngine;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "04 — Step 1: Place LED"
    public static class Step1PlaceLEDBuilder
    {
        [MenuItem("Tools/Build UI/04 Step 1 Place LED")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.Step1, "UI_Step1PlaceLEDRoot", "Plate_04_PlaceLED.png", gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);
            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.Workspace);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            SessionPill(c, "LED CIRCUIT", 180f, new[] { Accent, White(0.25f), White(0.25f), White(0.25f) });
            TrackingPill(c);

            var blue = new Color(Accent.r, Accent.g, Accent.b, 0.75f);
            Leader(c, "Leader_LED", 157f, 196f, 204.5f, 198.2f, Accent);
            WorldLabel(c, "Label_LEDSelected", 60f, 196f, "LED · SELECTED", TextLightBlue, blue);
            // Tapping this callout animates the LED from the parts tray onto the board. Its click
            // and AR anchor are wired by Tools > CircuitAR > AR > 3 Wire Circuit Demo.
            Callout(c, "Callout_PlaceLED", 254.46f, 152.2f, "PLACE HERE · C14–C15", "✓ LED PLACED",
                White(1f), blue, 9.5f, W.ExtraBold, 0.57f);

            var p = BeginPanel(c, "Panel_Instruction", 16f, 617f, 358f);
            PStepChip(p, "STEP 1 OF 4", Accent, Rgba(47, 140, 255, 0.18f));
            PTitle(p, "PLACE THE LED");
            PBody(p, "Tap PLACE HERE to move the LED into the highlighted holes — the longer leg (+) goes in C14.");
            var next = PButton(p, "NEXT  →");
            EndPanel(p);
            Wire(next, ScreenNames.Step2);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
