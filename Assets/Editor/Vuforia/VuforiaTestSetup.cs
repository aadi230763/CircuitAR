using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace CircuitAR.EditorTools
{
    // Phase 1 helper: builds an isolated Vuforia image-tracking test scene and applies the Android
    // player settings Vuforia needs. Uses reflection / Vuforia's own menu items so this file compiles
    // BEFORE the Vuforia package is installed.
    public static class VuforiaTestSetup
    {
        private const string ScenePath = "Assets/Scenes/VuforiaTest.unity";
        private const string MaterialPath = "Assets/Materials/VuforiaTestCube.mat";

        private static readonly string[] ArCameraMenus =
        {
            "GameObject/Vuforia Engine/AR Camera",
            "GameObject/Vuforia Engine/ARCamera",
        };

        private static readonly string[] ImageTargetMenus =
        {
            "GameObject/Vuforia Engine/Image",
            "GameObject/Vuforia Engine/Image Target",
        };

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, false);
                if (t != null) return t;
            }
            return null;
        }

        private static bool RunFirstMenu(string[] candidates)
        {
            foreach (var path in candidates)
                if (EditorApplication.ExecuteMenuItem(path)) return true;
            return false;
        }

        [MenuItem("Tools/CircuitAR/Vuforia/1 Create Test Scene")]
        public static void CreateTestScene()
        {
            var vuforiaType = FindType("Vuforia.VuforiaBehaviour");
            var imageTargetType = FindType("Vuforia.ImageTargetBehaviour");
            if (vuforiaType == null || imageTargetType == null)
            {
                EditorUtility.DisplayDialog("Vuforia not installed",
                    "Vuforia Engine isn't in this project yet.\n\nInstall it first (Window > Package Manager), wait for the " +
                    "import + recompile to finish, then run this menu item again.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // --- AR Camera (no default Main Camera exists in an EmptyScene) ---
            Component arCamera = null;
            if (RunFirstMenu(ArCameraMenus))
                arCamera = FindFirst(vuforiaType);
            if (arCamera == null)
            {
                var camGo = new GameObject("ARCamera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 100f;
                camGo.AddComponent<AudioListener>();
                arCamera = camGo.AddComponent(vuforiaType);
            }
            arCamera.gameObject.name = "ARCamera";

            // --- Image Target (left unassigned on purpose) ---
            Component imageTarget = null;
            if (RunFirstMenu(ImageTargetMenus))
                imageTarget = FindFirst(imageTargetType);
            if (imageTarget == null)
            {
                var itGo = new GameObject("ImageTarget");
                imageTarget = itGo.AddComponent(imageTargetType);
            }
            imageTarget.gameObject.name = "ImageTarget";

            // --- Bright test cube as a child of the target ---
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "TestCube";
            cube.transform.SetParent(imageTarget.transform, false);
            cube.transform.localScale = Vector3.one * 0.1f;
            cube.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            cube.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateCubeMaterial();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = arCamera.gameObject;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

            Debug.Log("[CircuitAR] Created " + ScenePath + ". Next: paste your license key into the Vuforia configuration " +
                      "(select ARCamera > Inspector > 'Open Vuforia Engine configuration' > App License Key), then import your " +
                      "target database and assign it on ImageTarget.");
        }

        private static Component FindFirst(Type type)
        {
            var found = UnityEngine.Object.FindObjectsOfType(type);
            return found != null && found.Length > 0 ? found[0] as Component : null;
        }

        private static Material GetOrCreateCubeMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (existing != null) return existing;

            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Sprites/Default");
            var mat = new Material(shader) { color = new Color(1f, 0.1f, 0.75f) };
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.6f, 0.05f, 0.45f));
            }
            AssetDatabase.CreateAsset(mat, MaterialPath);
            return mat;
        }

        [MenuItem("Tools/CircuitAR/Vuforia/2 Apply Android Settings")]
        public static void ApplyAndroidSettings()
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            string id = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            Debug.Log("[CircuitAR] Android settings applied: min API 26, target API auto, IL2CPP, ARM64, OpenGLES3, portrait.");
            if (string.IsNullOrEmpty(id) || id.Contains("DefaultCompany"))
                Debug.LogWarning("[CircuitAR] Package name is still the default (" + id + "). Set Project Settings > Player > " +
                                 "Android > Other Settings > Package Name to something unique, e.g. com.yourname.circuitar.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                Debug.LogWarning("[CircuitAR] Active build target is " + EditorUserBuildSettings.activeBuildTarget +
                                 ". Switch to Android in File > Build Settings (Switch Platform) before building.");
        }

        [MenuItem("Tools/CircuitAR/Vuforia/3 Build Only The Vuforia Test Scene")]
        public static void UseOnlyTestSceneInBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorUtility.DisplayDialog("Scene missing", "Run '1 Create Test Scene' first.", "OK");
                return;
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("[CircuitAR] Build Settings now contains only " + ScenePath +
                      ". To go back to the app, re-add your CircuitAR scene(s) in File > Build Settings.");
        }
    }
}
