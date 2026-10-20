using CircuitAR.Audio;
using CircuitAR.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "02 — Scan AR Marker"
    public static class ARCameraBuilder
    {
        [MenuItem("Tools/Build UI/02 AR Camera")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.ARCamera, "UI_ARCameraRoot", "Plate_02_Scan.png", gradientBg: false, showPlate: false);
            var c = s.content;

            StatusBar(s.root);

            Wire(IconButton(c, "Btn_Back", "‹", 16f, 54f), ScreenNames.HomeStart);
            IconButton(c, "Btn_Help", "?", 334f, 54f);
            MuteButton(c, 284f, 54f);
            SessionPill(c, "AR CAMERA", 105f);

            // Plays Audio_Welcome 2s after this screen appears — tied to the screen showing, not
            // to marker detection. onShow/onHide are wired to CircuitDemoController by
            // Tools > CircuitAR > AR > 3 Wire Circuit Demo.
            var welcomeAudio = s.canvas.gameObject.AddComponent<ScreenAudioTrigger>();
            welcomeAudio.delaySeconds = 2f;

            // Reflects real Vuforia tracking state — flips to "MARKER FOUND" and the AR Camera →
            // Marker Detected Workspace transition fires only on genuine detection. Driven by
            // CircuitDemoController, wired by Tools > CircuitAR > AR > 3 Wire Circuit Demo.
            var detectionPill = MultiLabelBox(c, "Pill_DetectionState", 101f, 150f,
                new[] { Run.Icon(RoundedSprite(3, 0f), 7f, Amber), Run.T("SEARCHING FOR MARKER…", 10f, W.Bold, TextLightBlue, 1f) },
                6f, 10f, 6f, -1f, PillBg);
            var detection = detectionPill.gameObject.AddComponent<DetectionStatusPill>();
            detection.icon = detectionPill.Find("Icon")?.GetComponent<Image>();
            detection.label = detectionPill.GetComponentInChildren<TextMeshProUGUI>();

            // Instruction panel
            var p = BeginPanel(c, "Panel_Instruction", 35f, 680f, 320f);
            var ico = Box(p.rt, "Img_ScanIcon", 17f, 17f, 40f, 40f, 12f, Rgba(47, 140, 255, 0.18f), Rgba(47, 140, 255, 0.6f));
            var ring = NewRect("Ring", ico);
            TopLeft(ring, 10f, 10f, 20f, 20f);
            var ringImg = ring.gameObject.AddComponent<Image>();
            ringImg.sprite = RoundedSprite(10, 2f);
            ringImg.type = Image.Type.Sliced;
            ringImg.color = Accent;
            ringImg.raycastTarget = false;

            Txt(p.rt, "SCAN CIRCUIT MARKER", 69f, 17f, 15f, W.ExtraBold, White(1f), 0.9f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Title");
            Txt(p.rt, "Point your camera at the marker to begin.", 69f, 38.2f, 12.5f, W.Regular, TextSecondary, 0f, 236f, 0f,
                TextAlignmentOptions.TopLeft, "Txt_Body");
            p.y += 51f + 10f;

            Solid(p.rt, "Divider", 17f, p.y, PanelInnerW(p), 1f, White(0.08f));
            p.y += 1f + 10f;

            var link = Txt(p.rt, "No marker? Print it from the Teacher Pack  →", 17f, p.y, 11.5f, W.SemiBold, TextLightBlue, 0f, 0f, 0f,
                TextAlignmentOptions.TopLeft, "Txt_TeacherPackLink");
            p.y += link.rectTransform.sizeDelta.y + 10f;
            // TODO: open the Teacher Pack (printable marker) when tapped.
            EndPanel(p);

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
