using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static CircuitAR.EditorUIBuilders.UIBuilderCommon;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    // Figma: "01 — Home / Start"
    public static class HomeStartBuilder
    {
        [MenuItem("Tools/Build UI/01 Home Start")]
        public static void Build()
        {
            var s = BeginScreen(ScreenNames.HomeStart, "UI_HomeStartRoot", "Plate_01_Home.png", gradientBg: true);
            var c = s.content;

            StatusBar(s.root);

            // App bar
            var icon = NewRect("Img_AppIcon", c);
            TopLeft(icon, 24f, 56f, 32f, 32f);
            var iconImg = icon.gameObject.AddComponent<Image>();
            iconImg.sprite = AppIconSprite();
            iconImg.raycastTarget = false;

            Txt(c, "CircuitAR", 64f, 62f, 16f, W.Bold, White(1f), 0f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_AppName");
            MultiLabelBox(c, "Pill_ARStatus", 268f, 60f,
                new[] { Run.Icon(RoundedSprite(3, 0f), 7f, Green), Run.T("AR READY", 10f, W.Bold, White(1f), 0.8f) },
                6f, 10f, 6f, -1f, PillBg);

            // Title block
            Txt(c, "Circuit<color=#2F8CFF>AR</color>", 24f, 476f, 46f, W.ExtraBold, White(1f), -0.92f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Title");
            Txt(c, "Interactive AR Electronics Learning", 24f, 534f, 18f, W.SemiBold, White(1f), 0f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Subtitle");
            Txt(c, "Build and understand electronic circuits through interactive augmented reality.", 24f, 564f, 15f, W.Regular,
                TextSecondary, 0f, 330f, 22f, TextAlignmentOptions.TopLeft, "Txt_Description");

            // Actions
            var start = PrimaryButton(c, "Btn_StartLearning", "START LEARNING  →", 24f, 640f, 342f, 51f);
            Wire(start, ScreenNames.ARCamera);

            SecondaryButton(c, "Btn_HowItWorks", "HOW IT WORKS", 24f, 708f, 342f, 49f);
            // TODO: wire Btn_HowItWorks to an intro / explainer screen.

            var settings = Box(c, "Btn_Settings", 24f, 766f, 342f, 41f, 14f, White(0f));
            var settingsLabel = MultiLabelBox(settings, "Label", 0f, 0f,
                new[] { Run.Icon(GearIcon(), 15f, White(1f)), Run.T("SETTINGS", 14f, W.Bold, White(1f), 0.84f) },
                7f, 0f, 0f, 1f, White(0f));
            TopLeft(settingsLabel, (342f - settingsLabel.sizeDelta.x) / 2f, (41f - settingsLabel.sizeDelta.y) / 2f,
                settingsLabel.sizeDelta.x, settingsLabel.sizeDelta.y);
            MakeButton(settings);
            // TODO: wire Btn_Settings once a settings screen exists.

            HomeIndicator(s.root);
            EndScreen(s);
        }
    }
}
