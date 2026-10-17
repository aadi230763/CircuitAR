using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CircuitAR.UI;
using static CircuitAR.EditorUIBuilders.UIStyleConstants;

namespace CircuitAR.EditorUIBuilders
{
    public enum W { Regular, Medium, SemiBold, Bold, ExtraBold }

    public static class ScreenNames
    {
        public const string HomeStart = "Canvas_HomeStart";
        public const string ARCamera = "Canvas_ARCamera";
        public const string Workspace = "Canvas_MarkerDetectedWorkspace";
        public const string Step1 = "Canvas_Step1PlaceLED";
        public const string Step2 = "Canvas_Step2And3ResistorWire";
        public const string Incomplete = "Canvas_FinalStepIncomplete";
        public const string Complete = "Canvas_FinalStepComplete";
        public const string HowItWorks = "Canvas_HowItWorks";
        public const string Quiz = "Canvas_Quiz";
        public const string Result = "Canvas_Result";
    }

    /// One piece of content inside a pill / chip: either a text run or an icon.
    public struct Run
    {
        public string text; public float size; public W weight; public Color color; public float tracking;
        public Sprite icon; public float iconSize;

        public static Run T(string text, float size, W weight, Color color, float tracking = 0f)
            => new Run { text = text, size = size, weight = weight, color = color, tracking = tracking };

        public static Run Icon(Sprite sprite, float size, Color tint)
            => new Run { icon = sprite, iconSize = size, color = tint };
    }

    public sealed class ScreenCtx
    {
        public string canvasName;
        public Canvas canvas;
        public RectTransform root;
        public RectTransform content;
    }

    public sealed class PanelCtx
    {
        public RectTransform rt;
        public float x, top, w, y, height;
    }

    // Shared UI kit. Everything is laid out in Figma pixels on a 390x844 reference canvas, so
    // coordinates copied from Figma (top-left origin) map 1:1 to canvas units.
    public static class UIBuilderCommon
    {
        public const float FrameW = 390f;
        public const float FrameH = 844f;

        private const string FigmaDir = "Assets/UI/Figma/";
        private const string GenDir = "Assets/UI/Generated/";
        private const string SpriteDir = GenDir + "Sprites/";

        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<W, TMP_FontAsset> FontCache = new Dictionary<W, TMP_FontAsset>();

        // ------------------------------------------------------------------
        // Canvas / screen scaffold
        // ------------------------------------------------------------------

        public static Canvas CreateScreenCanvas(string canvasName)
        {
            // GameObject.Find skips inactive objects, and UIScreenManager deactivates every screen
            // except the one currently shown — so a plain Find here would miss a hidden screen's old
            // canvas and leave a duplicate behind. Search scene roots (active or not) instead.
            GameObject existing = null;
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in activeScene.GetRootGameObjects())
            {
                if (root.name == canvasName) { existing = root; break; }
            }
            if (existing != null) Object.DestroyImmediate(existing);

            if (Object.FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var go = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(FrameW, FrameH);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            return canvas;
        }

        /// plateFile: file name inside Assets/UI/Figma/Plates (null = none).
        /// gradientBg: navy gradient behind (Home/Quiz/Result); otherwise the plate's own bottom colour fills any extra height.
        /// showPlate: false on AR screens once a real camera feed (Vuforia ARCamera) renders behind the UI —
        /// skips the plate image and leaves "Bg" fully transparent so the feed shows through instead of plate art.
        public static ScreenCtx BeginScreen(string canvasName, string rootName, string plateFile, bool gradientBg, bool showPlate = true)
        {
            AssetDatabase.Refresh();
            var canvas = CreateScreenCanvas(canvasName);
            var root = NewRect(rootName, canvas.transform);
            Fill(root);

            var bg = NewRect("Bg", root);
            Fill(bg);
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.raycastTarget = false;
            if (!showPlate)
            {
                bgImg.color = new Color(0f, 0f, 0f, 0f);
            }
            else if (gradientBg)
            {
                bgImg.sprite = GradientSprite(BgTop, BgBottom);
                bgImg.color = Color.white;
            }
            else
            {
                bgImg.color = plateFile != null ? SamplePlateBottom(FigmaDir + "Plates/" + plateFile) : BgBottom;
            }

            if (plateFile != null && showPlate)
            {
                var plate = NewRect("Img_Plate", root);
                plate.anchorMin = plate.anchorMax = new Vector2(0.5f, 1f);
                plate.pivot = new Vector2(0.5f, 1f);
                plate.sizeDelta = new Vector2(FrameW, FrameH);
                plate.anchoredPosition = Vector2.zero;
                var pi = plate.gameObject.AddComponent<Image>();
                pi.sprite = PlateSprite(plateFile);
                pi.raycastTarget = false;
            }

            var content = NewRect("SafeArea", root);
            Fill(content);
            content.gameObject.AddComponent<SafeArea>();

            return new ScreenCtx { canvasName = canvasName, canvas = canvas, root = root, content = content };
        }

        public static void EndScreen(ScreenCtx s)
        {
            RegisterScreen(s.canvasName, s.canvas.gameObject);
            Selection.activeGameObject = s.canvas.gameObject;
        }

        // ------------------------------------------------------------------
        // Rect helpers (Figma coordinates: origin top-left, y down)
        // ------------------------------------------------------------------

        public static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform TopLeft(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform BottomLeft(RectTransform rt, float x, float bottom, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(x, bottom);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Fill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform Solid(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rt = NewRect(name, parent);
            TopLeft(rt, x, y, w, h);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        // ------------------------------------------------------------------
        // Text (Inter via TextMeshPro)
        // ------------------------------------------------------------------

        public static TMP_FontAsset GetFont(W weight)
        {
            if (FontCache.TryGetValue(weight, out var cached) && cached != null) return cached;

            string face = "Inter-" + weight;
            string assetPath = GenDir + "Fonts/" + face + " SDF.asset";
            EnsureDir(GenDir + "Fonts");

            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (fa == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(FigmaDir + "Fonts/" + face + ".otf");
                if (font == null)
                {
                    Debug.LogWarning("[CircuitAR] Missing font " + face + ".otf in Assets/UI/Figma/Fonts — falling back to the default TMP font.");
                    return TMP_Settings.defaultFontAsset;
                }

                fa = TMP_FontAsset.CreateFontAsset(font);
                AssetDatabase.CreateAsset(fa, assetPath);
                if (fa.material != null)
                {
                    fa.material.name = face + " Material";
                    AssetDatabase.AddObjectToAsset(fa.material, fa);
                }
                if (fa.atlasTexture != null)
                {
                    fa.atlasTexture.name = face + " Atlas";
                    AssetDatabase.AddObjectToAsset(fa.atlasTexture, fa);
                }
                EditorUtility.SetDirty(fa);
                AssetDatabase.SaveAssets();
            }

            FontCache[weight] = fa;
            return fa;
        }

        /// Single text line (or wrapped block if width > 0), placed by its top-left corner.
        public static TextMeshProUGUI Txt(Transform parent, string s, float x, float y, float size, W weight, Color color,
            float tracking = 0f, float width = 0f, float lineHeight = 0f,
            TextAlignmentOptions align = TextAlignmentOptions.TopLeft, string name = null)
        {
            var rt = NewRect(name ?? "Txt_" + Sanitize(s), parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = GetFont(weight);
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.characterSpacing = tracking / size * 100f;
            t.enableWordWrapping = width > 0f;
            t.overflowMode = TextOverflowModes.Overflow;
            t.margin = Vector4.zero;
            t.richText = true;
            t.raycastTarget = false;
            if (lineHeight > 0f) t.lineSpacing = (lineHeight - size * 1.21f) / size * 100f;
            t.text = s;

            float w = width;
            float h;
            if (width > 0f)
            {
                h = t.GetPreferredValues(s, width, 0f).y;
            }
            else
            {
                var pv = t.GetPreferredValues(s);
                w = pv.x;
                h = pv.y;
            }
            TopLeft(rt, x, y, w, h);
            return t;
        }

        /// Text centred inside a w x h area at (0,0) of `parent`.
        public static TextMeshProUGUI TxtCentered(Transform parent, string s, float w, float h, float size, W weight,
            Color color, float tracking = 0f, string name = null)
        {
            var rt = NewRect(name ?? "Txt_" + Sanitize(s), parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = GetFont(weight);
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.characterSpacing = tracking / size * 100f;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow;
            t.margin = Vector4.zero;
            t.richText = true;
            t.raycastTarget = false;
            t.text = s;
            TopLeft(rt, 0f, 0f, w, h);
            return t;
        }

        private static string Sanitize(string s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                if (sb.Length >= 24) break;
            }
            return sb.Length == 0 ? "Text" : sb.ToString();
        }

        // ------------------------------------------------------------------
        // Shapes: rounded boxes, strokes, shadows
        // ------------------------------------------------------------------

        /// radius < 0 = capsule (fully rounded).
        public static void Decorate(RectTransform rt, float w, float h, float radius, Color fill, Color? stroke = null, float strokeW = 1f)
        {
            int maxR = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w, h) / 2f));
            int r = radius < 0f ? maxR : Mathf.Clamp(Mathf.RoundToInt(radius), 1, maxR);

            var img = rt.gameObject.GetComponent<Image>();
            if (img == null) img = rt.gameObject.AddComponent<Image>();
            img.sprite = RoundedSprite(r, 0f);
            img.type = Image.Type.Sliced;
            img.color = fill;
            img.raycastTarget = false;

            if (stroke.HasValue)
            {
                var st = NewRect("Stroke", rt);
                Fill(st);
                var si = st.gameObject.AddComponent<Image>();
                si.sprite = RoundedSprite(r, strokeW);
                si.type = Image.Type.Sliced;
                si.color = stroke.Value;
                si.raycastTarget = false;
            }
        }

        public static RectTransform Box(Transform parent, string name, float x, float y, float w, float h, float radius, Color fill,
            Color? stroke = null, float strokeW = 1f, float shadowBlur = 0f, float shadowY = 0f, Color? shadowColor = null)
        {
            if (shadowBlur > 0f)
            {
                int maxR = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w, h) / 2f));
                int r = radius < 0f ? maxR : Mathf.Clamp(Mathf.RoundToInt(radius), 1, maxR);
                AddShadow(parent, name + "_Shadow", x, y, w, h, r, shadowBlur, shadowY, shadowColor ?? Black(0.4f));
            }
            var rt = NewRect(name, parent);
            TopLeft(rt, x, y, w, h);
            Decorate(rt, w, h, radius, fill, stroke, strokeW);
            return rt;
        }

        public static RectTransform Circle(Transform parent, string name, float cx, float cy, float d, Color fill)
            => Box(parent, name, cx - d / 2f, cy - d / 2f, d, d, -1f, fill);

        public static RectTransform AddShadow(Transform parent, string name, float x, float y, float w, float h, int radius,
            float blur, float offsetY, Color color)
        {
            float pad = blur * 2f;
            var rt = NewRect(name, parent);
            TopLeft(rt, x - pad, y - pad + offsetY, w + pad * 2f, h + pad * 2f);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = ShadowSprite(radius, Mathf.RoundToInt(blur));
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        /// Thin straight line between two Figma-space points.
        public static RectTransform Line(Transform parent, string name, float x1, float y1, float x2, float y2, float thickness, Color color)
        {
            float dx = x2 - x1, dy = y2 - y1;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            var rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(len, thickness);
            rt.anchoredPosition = new Vector2((x1 + x2) / 2f, -(y1 + y2) / 2f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        /// Leader line from a label down/up to a 5px anchor dot centred on (x2, y2).
        public static void Leader(Transform parent, string name, float x1, float y1, float x2, float y2, Color color)
        {
            Line(parent, name + "_Line", x1, y1, x2, y2, 1f, color);
            Circle(parent, name + "_Dot", x2, y2, 5f, color);
        }

        public static Button MakeButton(RectTransform rt)
        {
            var img = rt.gameObject.GetComponent<Image>();
            img.raycastTarget = true;
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var cb = b.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            cb.fadeDuration = 0.08f;
            b.colors = cb;
            return b;
        }

        // ------------------------------------------------------------------
        // Pills / labels
        // ------------------------------------------------------------------

        public static RectTransform MultiLabelBox(Transform parent, string name, float x, float y, Run[] runs, float gap,
            float padX, float padY, float radius, Color fill, Color? stroke = null, float strokeW = 1f)
        {
            float bw = stroke.HasValue ? strokeW : 0f;
            var rt = NewRect(name, parent);
            var items = new List<RectTransform>();
            var ws = new List<float>();
            var hs = new List<float>();

            foreach (var r in runs)
            {
                if (r.icon != null)
                {
                    var ir = NewRect("Icon", rt);
                    var im = ir.gameObject.AddComponent<Image>();
                    im.sprite = r.icon;
                    im.type = Image.Type.Sliced;
                    im.color = r.color;
                    im.raycastTarget = false;
                    TopLeft(ir, 0f, 0f, r.iconSize, r.iconSize);
                    items.Add(ir); ws.Add(r.iconSize); hs.Add(r.iconSize);
                }
                else
                {
                    var t = Txt(rt, r.text, 0f, 0f, r.size, r.weight, r.color, r.tracking);
                    items.Add(t.rectTransform);
                    ws.Add(t.rectTransform.sizeDelta.x);
                    hs.Add(t.rectTransform.sizeDelta.y);
                }
            }

            float cw = 0f, ch = 0f;
            for (int i = 0; i < items.Count; i++) { cw += ws[i]; ch = Mathf.Max(ch, hs[i]); }
            cw += gap * (items.Count - 1);

            float w = cw + 2f * (padX + bw);
            float h = ch + 2f * (padY + bw);
            float cx = padX + bw;
            for (int i = 0; i < items.Count; i++)
            {
                TopLeft(items[i], cx, (h - hs[i]) / 2f, ws[i], hs[i]);
                cx += ws[i] + gap;
            }

            TopLeft(rt, x, y, w, h);
            Decorate(rt, w, h, radius, fill, stroke, strokeW);
            // fill + stroke were added after the children, so push them behind
            var stroker = rt.Find("Stroke");
            if (stroker != null) stroker.SetAsFirstSibling();
            return rt;
        }

        public static RectTransform LabelBox(Transform parent, string name, float x, float y, string text, float size, W weight,
            Color color, float tracking, float padX, float padY, float radius, Color fill, Color? stroke = null)
            => MultiLabelBox(parent, name, x, y, new[] { Run.T(text, size, weight, color, tracking) }, 0f, padX, padY, radius, fill, stroke);

        /// Floating AR world-space label ("BREADBOARD", "LED · SELECTED" ...).
        public static RectTransform WorldLabel(Transform parent, string name, float x, float y, string text, Color textColor,
            Color? border = null, float size = 10f, W weight = W.Bold, float tracking = 0.6f)
            => LabelBox(parent, name, x, y, text, size, weight, textColor, tracking, 10f, 6f, -1f, PillBg, border);

        // ------------------------------------------------------------------
        // Shared chrome
        // ------------------------------------------------------------------

        public static void StatusBar(Transform parent)
        {
            var g = NewRect("StatusBar", parent);
            TopLeft(g, 0f, 0f, FrameW, 50f);
            Txt(g, "9:41", 30f, 15f, 16f, W.SemiBold, White(1f), 0f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Time");
            Box(g, "Signal1", 296f, 25f, 3.2f, 4f, 1f, White(1f));
            Box(g, "Signal2", 301f, 22.4f, 3.2f, 6.6f, 1f, White(1f));
            Box(g, "Signal3", 306f, 19.8f, 3.2f, 9.2f, 1f, White(1f));
            Box(g, "Signal4", 311f, 17.2f, 3.2f, 11.8f, 1f, White(1f));
            Box(g, "Battery", 322f, 19f, 25f, 12f, 4f, White(0f), White(0.55f));
            Box(g, "BatteryLevel", 324f, 21f, 18f, 8f, 2f, White(1f));
            Box(g, "BatteryTip", 348f, 23f, 2f, 4f, 1f, White(0.55f));
        }

        public static void HomeIndicator(Transform parent)
        {
            var rt = NewRect("HomeIndicator", parent);
            BottomLeft(rt, 128f, 8f, 134f, 5f);
            Decorate(rt, 134f, 5f, 3f, White(0.9f));
        }

        public static Button IconButton(Transform parent, string name, string glyph, float x, float y)
        {
            var rt = Box(parent, name, x, y, 40f, 40f, 20f, IconButtonBg, White(0.14f));
            TxtCentered(rt, glyph, 40f, 40f, 18f, W.SemiBold, White(1f), 0f, "Txt_Glyph");
            return MakeButton(rt);
        }

        public static RectTransform SessionPill(Transform parent, string text, float w, Color[] segments = null)
        {
            const float h = 31f;
            var rt = Box(parent, "Pill_Session", (FrameW - w) / 2f, 58f, w, h, -1f, PillBg);
            Txt(rt, text, 14f, (h - 13.3f) / 2f, 11f, W.ExtraBold, White(1f), 1.1f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Session");
            if (segments != null)
            {
                float total = segments.Length * 14f + (segments.Length - 1) * 3f;
                float sx = w - 14f - total;
                for (int i = 0; i < segments.Length; i++)
                    Box(rt, "Seg_" + (i + 1), sx + i * 17f, (h - 4f) / 2f, 14f, 4f, 2f, segments[i]);
            }
            return rt;
        }

        public static RectTransform TrackingPill(Transform parent)
            => MultiLabelBox(parent, "Pill_MarkerTracked", 16f, 108f,
                new[] { Run.Icon(RoundedSprite(3, 0f), 7f, Green), Run.T("MARKER TRACKED", 9.5f, W.Bold, White(1f), 0.76f) },
                6f, 10f, 6f, -1f, PillBg);

        public static RectTransform CircuitStatusPill(Transform parent, bool complete)
        {
            Color c = complete ? Green : Amber;
            string word = complete ? "COMPLETE" : "INCOMPLETE";
            var rt = MultiLabelBox(parent, "Pill_CircuitStatus", 0f, 108f,
                new[]
                {
                    Run.Icon(RoundedSprite(3, 0f), 7f, c),
                    Run.T("CIRCUIT", 9.5f, W.Bold, TextSecondary, 0.76f),
                    Run.T(word, 10f, W.ExtraBold, c, 0.8f),
                }, 6f, 10f, 6f, -1f, PillBg, new Color(c.r, c.g, c.b, 0.75f));
            TopLeft(rt, FrameW - 16f - rt.sizeDelta.x, 108f, rt.sizeDelta.x, rt.sizeDelta.y);
            return rt;
        }

        // ------------------------------------------------------------------
        // Buttons
        // ------------------------------------------------------------------

        public static Button PrimaryButton(Transform parent, string name, string label, float x, float y, float w, float h)
        {
            var rt = Box(parent, name, x, y, w, h, 14f, Accent, null, 1f, 8f, 6f, new Color(Accent.r, Accent.g, Accent.b, 0.35f));
            TxtCentered(rt, label, w, h, 14f, W.Bold, White(1f), 0.84f, "Txt_Label");
            return MakeButton(rt);
        }

        public static Button SecondaryButton(Transform parent, string name, string label, float x, float y, float w, float h)
        {
            var rt = Box(parent, name, x, y, w, h, 14f, White(0.08f), White(0.18f));
            TxtCentered(rt, label, w, h, 14f, W.Bold, White(1f), 0.84f, "Txt_Label");
            return MakeButton(rt);
        }

        /// Invisible full-frame tap target (used for "tap to simulate marker detection").
        public static Button Hotspot(Transform parent, string name)
        {
            var rt = NewRect(name, parent);
            Fill(rt);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.transition = Selectable.Transition.None;
            return b;
        }

        // ------------------------------------------------------------------
        // Instruction panel (bottom card). Positioned by its Figma top, anchored to the bottom.
        // ------------------------------------------------------------------

        public static PanelCtx BeginPanel(Transform parent, string name, float x, float top, float w)
            => new PanelCtx { rt = NewRect(name, parent), x = x, top = top, w = w, y = 17f };

        public static float PanelInnerW(PanelCtx p) => p.w - 34f;

        public static void EndPanel(PanelCtx p)
        {
            float h = p.y - 10f + 17f;
            p.height = h;
            BottomLeft(p.rt, p.x, FrameH - (p.top + h), p.w, h);

            var bg = NewRect("Bg", p.rt);
            Fill(bg);
            Decorate(bg, p.w, h, 22f, PanelBg, White(0.10f));
            bg.SetAsFirstSibling();

            var sh = AddShadow(p.rt, "Shadow", 0f, 0f, p.w, h, 22, 15f, 10f, Black(0.4f));
            sh.SetAsFirstSibling();
        }

        public static RectTransform PStepChip(PanelCtx p, string text, Color fg, Color bg)
        {
            var chip = LabelBox(p.rt, "Chip_Step", 17f, p.y, text, 11f, W.ExtraBold, fg, 0.88f, 8f, 4f, 6f, bg);
            p.y += chip.sizeDelta.y + 10f;
            return chip;
        }

        public static TextMeshProUGUI PTitle(PanelCtx p, string text)
        {
            var t = Txt(p.rt, text, 17f, p.y, 21f, W.ExtraBold, White(1f), 0.42f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Title");
            p.y += t.rectTransform.sizeDelta.y + 10f;
            return t;
        }

        public static TextMeshProUGUI PBody(PanelCtx p, string text, Color? color = null, float size = 14f, float lineHeight = 20f, W weight = W.Regular)
        {
            var t = Txt(p.rt, text, 17f, p.y, size, weight, color ?? TextSecondary, 0f, 326f, lineHeight, TextAlignmentOptions.TopLeft, "Txt_Body");
            p.y += t.rectTransform.sizeDelta.y + 10f;
            return t;
        }

        public static RectTransform PDoneRow(PanelCtx p, string label, string status)
        {
            var row = Box(p.rt, "Row_StepDone", 17f, p.y, PanelInnerW(p), 34f, 10f, Rgba(46, 213, 115, 0.12f));
            var c = Circle(row, "Check", 10f + 9f, 17f, 18f, Green);
            TxtCentered(c, "✓", 18f, 18f, 11f, W.ExtraBold, TextOnGreen, 0f, "Txt_Check");
            var t1 = Txt(row, label, 36f, (34f - 13.3f) / 2f, 11f, W.Bold, Green, 0.44f);
            Txt(row, status, 36f + t1.rectTransform.sizeDelta.x + 8f, (34f - 12.1f) / 2f, 10f, W.ExtraBold, Green, 1f, 0f, 0f, TextAlignmentOptions.TopLeft, "Txt_Done");
            p.y += 34f + 10f;
            return row;
        }

        public static void PChecklist(PanelCtx p, string[] labels, bool[] ok)
        {
            float maxW = PanelInnerW(p);
            float cx = 0f, cy = p.y, rowH = 0f;
            for (int i = 0; i < labels.Length; i++)
            {
                Color c = ok[i] ? Green : Amber;
                string text = (ok[i] ? "✓ " : "✗ ") + labels[i];
                var chip = LabelBox(p.rt, "Chip_" + Sanitize(labels[i]), 0f, 0f, text, 10.5f, W.Bold, c, 0.42f, 8f, 5f, 8f,
                    new Color(c.r, c.g, c.b, 0.14f), new Color(c.r, c.g, c.b, 0.45f));
                float w = chip.sizeDelta.x, h = chip.sizeDelta.y;
                if (cx > 0f && cx + w > maxW) { cx = 0f; cy += rowH + 6f; rowH = 0f; }
                TopLeft(chip, 17f + cx, cy, w, h);
                cx += w + 6f;
                rowH = Mathf.Max(rowH, h);
            }
            p.y = cy + rowH + 10f;
        }

        public static void PButtonRow(PanelCtx p, string leftLabel, string rightLabel, out Button left, out Button right)
        {
            left = SecondaryButton(p.rt, "Btn_" + Sanitize(leftLabel), leftLabel, 17f, p.y, 158f, 47f);
            right = PrimaryButton(p.rt, "Btn_" + Sanitize(rightLabel), rightLabel, 185f, p.y, 156f, 45f);
            p.y += 47f + 10f;
        }

        public static Button PButton(PanelCtx p, string label)
        {
            var b = PrimaryButton(p.rt, "Btn_" + Sanitize(label), label, 17f, p.y, PanelInnerW(p), 45f);
            p.y += 45f + 10f;
            return b;
        }

        // ------------------------------------------------------------------
        // Sprites
        // ------------------------------------------------------------------

        private static void EnsureDir(string assetDir)
        {
            if (!Directory.Exists(assetDir)) Directory.CreateDirectory(assetDir);
        }

        private static Sprite SaveSprite(string path, Texture2D tex, int ppu, Vector4 border)
        {
            EnsureDir(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.spriteBorder = border;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CachedSprite(string path)
        {
            if (SpriteCache.TryGetValue(path, out var s) && s != null) return s;
            s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) SpriteCache[path] = s;
            return s;
        }

        private static bool InRR(float fx, float fy, float x0, float y0, float x1, float y1, float rad)
        {
            if (fx < x0 || fx > x1 || fy < y0 || fy > y1) return false;
            float cx = Mathf.Clamp(fx, x0 + rad, x1 - rad);
            float cy = Mathf.Clamp(fy, y0 + rad, y1 - rad);
            float dx = fx - cx, dy = fy - cy;
            return dx * dx + dy * dy <= rad * rad;
        }

        /// 9-sliced rounded rectangle. strokeW = 0 → filled, > 0 → outline ring of that thickness.
        public static Sprite RoundedSprite(int r, float strokeW)
        {
            r = Mathf.Max(r, 1);
            string key = strokeW <= 0f ? "fill_r" + r : "stroke_r" + r + "_w" + Mathf.RoundToInt(strokeW * 10f);
            string path = SpriteDir + "Round/" + key + ".png";
            var cached = CachedSprite(path);
            if (cached != null) return cached;

            const int S = 4;
            int px = (2 * r + 2) * S;
            float R = r * S, t = strokeW * S;
            var pix = new Color32[px * px];
            for (int y = 0; y < px; y++)
            {
                for (int x = 0; x < px; x++)
                {
                    int hit = 0;
                    for (int sy = 0; sy < 3; sy++)
                    {
                        for (int sx = 0; sx < 3; sx++)
                        {
                            float fx = x + (sx + 0.5f) / 3f, fy = y + (sy + 0.5f) / 3f;
                            bool v = InRR(fx, fy, 0, 0, px, px, R);
                            if (v && t > 0f) v = !InRR(fx, fy, t, t, px - t, px - t, Mathf.Max(R - t, 0f));
                            if (v) hit++;
                        }
                    }
                    pix[y * px + x] = new Color32(255, 255, 255, (byte)(hit * 255 / 9));
                }
            }
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false);
            tex.SetPixels32(pix);
            tex.Apply();
            var sp = SaveSprite(path, tex, 100 * S, new Vector4(R, R, R, R));
            SpriteCache[path] = sp;
            return sp;
        }

        /// Soft drop-shadow, 9-sliced. blur = Figma blur radius.
        public static Sprite ShadowSprite(int radius, int blur)
        {
            radius = Mathf.Max(radius, 1);
            blur = Mathf.Max(blur, 1);
            string path = SpriteDir + "Shadow/shadow_r" + radius + "_b" + blur + ".png";
            var cached = CachedSprite(path);
            if (cached != null) return cached;

            const int S = 2;
            int pad = blur * 2;
            int border = radius + pad;
            int px = (2 * border + 2) * S;
            var a = new float[px * px];
            float x0 = pad * S, x1 = px - pad * S, R = radius * S;
            for (int y = 0; y < px; y++)
                for (int x = 0; x < px; x++)
                    a[y * px + x] = InRR(x + 0.5f, y + 0.5f, x0, x0, x1, x1, R) ? 1f : 0f;

            float sigma = blur / 2f * S;
            int k = Mathf.Max(1, Mathf.RoundToInt((Mathf.Sqrt(1f + 4f * sigma * sigma) - 1f) / 2f));
            var tmp = new float[a.Length];
            for (int pass = 0; pass < 3; pass++)
            {
                BoxBlur(a, tmp, px, k, true);
                BoxBlur(tmp, a, px, k, false);
            }

            var pix = new Color32[a.Length];
            for (int i = 0; i < a.Length; i++)
                pix[i] = new Color32(255, 255, 255, (byte)Mathf.Clamp(Mathf.RoundToInt(a[i] * 255f), 0, 255));
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false);
            tex.SetPixels32(pix);
            tex.Apply();
            float B = border * S;
            var sp = SaveSprite(path, tex, 100 * S, new Vector4(B, B, B, B));
            SpriteCache[path] = sp;
            return sp;
        }

        private static void BoxBlur(float[] src, float[] dst, int n, int k, bool horizontal)
        {
            float inv = 1f / (2 * k + 1);
            for (int line = 0; line < n; line++)
            {
                float sum = 0f;
                for (int i = -k; i <= k; i++) sum += src[Idx(line, Mathf.Clamp(i, 0, n - 1), n, horizontal)];
                for (int i = 0; i < n; i++)
                {
                    dst[Idx(line, i, n, horizontal)] = sum * inv;
                    int add = Mathf.Clamp(i + k + 1, 0, n - 1);
                    int rem = Mathf.Clamp(i - k, 0, n - 1);
                    sum += src[Idx(line, add, n, horizontal)] - src[Idx(line, rem, n, horizontal)];
                }
            }
        }

        private static int Idx(int line, int i, int n, bool horizontal) => horizontal ? line * n + i : i * n + line;

        public static Sprite GradientSprite(Color top, Color bottom)
        {
            string key = ColorUtility.ToHtmlStringRGB(top) + "_" + ColorUtility.ToHtmlStringRGB(bottom);
            string path = SpriteDir + "Gradient/grad_" + key + ".png";
            var cached = CachedSprite(path);
            if (cached != null) return cached;

            const int H = 256;
            var tex = new Texture2D(4, H, TextureFormat.RGBA32, false);
            for (int y = 0; y < H; y++)
            {
                var c = Color.Lerp(bottom, top, y / (H - 1f));
                for (int x = 0; x < 4; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            var sp = SaveSprite(path, tex, 100, Vector4.zero);
            SpriteCache[path] = sp;
            return sp;
        }

        /// Imports a Figma plate/component PNG as a crisp sprite and returns it.
        public static Sprite PlateSprite(string fileName)
        {
            string path = FigmaDir + "Plates/" + fileName;
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null)
            {
                Debug.LogError("[CircuitAR] Missing plate: " + path);
                return null;
            }
            if (ti.textureType != TextureImporterType.Sprite || ti.mipmapEnabled || ti.maxTextureSize < 2048)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.mipmapEnabled = false;
                ti.alphaIsTransparency = true;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.filterMode = FilterMode.Bilinear;
                ti.maxTextureSize = 2048;
                ti.textureCompression = TextureImporterCompression.CompressedHQ;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Color SamplePlateBottom(string path)
        {
            try
            {
                var tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(path));
                var row = tex.GetPixels(0, 0, tex.width, 1);
                float r = 0, g = 0, b = 0;
                foreach (var c in row) { r += c.r; g += c.g; b += c.b; }
                Object.DestroyImmediate(tex);
                return new Color(r / row.Length, g / row.Length, b / row.Length, 1f);
            }
            catch
            {
                return BgBottom;
            }
        }

        // ---- procedural icons Inter lacks (gear, bulb) + the app icon ----

        private delegate bool Shape(float x, float y);

        private static Sprite IconSprite(string key, Shape shape)
        {
            string path = SpriteDir + "Icons/" + key + ".png";
            var cached = CachedSprite(path);
            if (cached != null) return cached;

            const int N = 128;
            var pix = new Color32[N * N];
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    int hit = 0;
                    for (int sy = 0; sy < 3; sy++)
                        for (int sx = 0; sx < 3; sx++)
                            if (shape(x + (sx + 0.5f) / 3f, y + (sy + 0.5f) / 3f)) hit++;
                    pix[y * N + x] = new Color32(255, 255, 255, (byte)(hit * 255 / 9));
                }
            }
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
            tex.SetPixels32(pix);
            tex.Apply();
            var sp = SaveSprite(path, tex, 100, Vector4.zero);
            SpriteCache[path] = sp;
            return sp;
        }

        public static Sprite GearIcon() => IconSprite("gear", (x, y) =>
        {
            float dx = x - 64f, dy = y - 64f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            if (r < 17f) return false;
            if (r <= 38f) return true;
            if (r > 56f) return false;
            float phase = Mathf.Repeat(Mathf.Atan2(dy, dx) / (2f * Mathf.PI) * 8f, 1f);
            return phase > 0.2f && phase < 0.7f;
        });

        public static Sprite BulbIcon() => IconSprite("bulb", (x, y) =>
        {
            float dx = x - 64f;
            float dyc = y - 78f; // bulb centre (y measured upward)
            if (dx * dx + dyc * dyc <= 36f * 36f) return true;
            if (y >= 20f && y <= 46f) return Mathf.Abs(dx) <= 16f + (y - 20f) / 26f * 6f;
            if (y >= 14f && y < 20f) return Mathf.Abs(dx) <= 16f;
            if (y >= 4f && y < 12f) return Mathf.Abs(dx) <= 11f;
            return false;
        });

        public static Sprite AppIconSprite()
        {
            string path = SpriteDir + "Icons/app_icon.png";
            var cached = CachedSprite(path);
            if (cached != null) return cached;

            const int N = 128;
            float R = 36f;
            var c0 = new Color(90f / 255f, 176f / 255f, 1f);
            var c1 = new Color(31f / 255f, 111f / 255f, 235f / 255f);
            var pix = new Color32[N * N];
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    int hit = 0, ring = 0;
                    for (int sy = 0; sy < 3; sy++)
                    {
                        for (int sx = 0; sx < 3; sx++)
                        {
                            float fx = x + (sx + 0.5f) / 3f, fy = y + (sy + 0.5f) / 3f;
                            if (InRR(fx, fy, 0, 0, N, N, R)) hit++;
                            float dx = fx - 64f, dy = fy - 64f;
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d >= 20f && d <= 28f) ring++;
                        }
                    }
                    float tt = (x + (N - y)) / (2f * N);
                    var col = Color.Lerp(c0, c1, tt);
                    float rr = ring / 9f;
                    col = Color.Lerp(col, Color.white, rr);
                    pix[y * N + x] = new Color32((byte)(col.r * 255), (byte)(col.g * 255), (byte)(col.b * 255), (byte)(hit * 255 / 9));
                }
            }
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
            tex.SetPixels32(pix);
            tex.Apply();
            var sp = SaveSprite(path, tex, 400, Vector4.zero);
            SpriteCache[path] = sp;
            return sp;
        }

        // ------------------------------------------------------------------
        // Demo screen switching
        // ------------------------------------------------------------------

        private const string ScreenManagerName = "UIScreenManager";

        public static UIScreenManager EnsureScreenManager()
        {
            var go = GameObject.Find(ScreenManagerName);
            if (go == null) go = new GameObject(ScreenManagerName);
            var mgr = go.GetComponent<UIScreenManager>();
            if (mgr == null) mgr = go.AddComponent<UIScreenManager>();
            return mgr;
        }

        public static void RegisterScreen(string canvasName, GameObject canvasGO)
        {
            var mgr = EnsureScreenManager();
            mgr.RegisterScreen(canvasName, canvasGO);
            EditorUtility.SetDirty(mgr);
        }

        public static void Wire(Button button, string targetCanvasName)
        {
            if (button == null) return;
            var mgr = EnsureScreenManager();
            UnityEventTools.AddStringPersistentListener(button.onClick, mgr.ShowScreen, targetCanvasName);
        }
    }
}
