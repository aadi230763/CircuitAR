using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CircuitAR.EditorTools
{
    // Phase 3 helper: copies the already-working ARCamera + ImageTarget (configured against the
    // CircuitARTest database / 3d_marker target) out of the isolated VuforiaTest.unity scene and
    // into the main SampleScene, without touching VuforiaTest.unity or any UI builder logic.
    public static class VuforiaMainSceneMerge
    {
        private const string MainScenePath = "Assets/Scenes/SampleScene.unity";
        private const string TestScenePath = "Assets/Scenes/VuforiaTest.unity";

        [MenuItem("Tools/CircuitAR/Vuforia/4 Merge AR Into Main Scene")]
        public static void Merge()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.path != MainScenePath)
            {
                EditorUtility.DisplayDialog("Open SampleScene first",
                    "Open " + MainScenePath + " as the active scene (and save/discard any changes you want to keep), " +
                    "then run this menu item again.", "OK");
                return;
            }

            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(TestScenePath))
            {
                EditorUtility.DisplayDialog("VuforiaTest scene missing",
                    TestScenePath + " was not found. Run the Phase 1 setup first.", "OK");
                return;
            }

            var vuforiaBehaviourType = FindType("Vuforia.VuforiaBehaviour");
            bool packageReady = vuforiaBehaviourType != null;
            if (!packageReady)
                Debug.LogWarning("[CircuitAR] Vuforia.VuforiaBehaviour type not found via reflection — the package may still be " +
                                  "importing/compiling. The merge will continue by name, but double-check the result once Vuforia has compiled.");

            // --- Idempotency: skip creating a duplicate ARCamera if one is already merged in ---
            bool alreadyMerged = active.GetRootGameObjects()
                .Any(go => go.name == "ARCamera" && HasComponentNamed(go, "Vuforia"));
            if (alreadyMerged)
            {
                Debug.Log("[CircuitAR] An ARCamera with a Vuforia component already exists in SampleScene — skipping copy. " +
                          "Only re-running the Main Camera disable + canvas check.");
            }
            else
            {
                MergeArCameraAndImageTarget(active);
            }

            DisablePlainMainCameras(active);

            EditorSceneManager.MarkSceneDirty(active);
            Selection.activeGameObject = GameObject.Find("ARCamera");
            Debug.Log("[CircuitAR] Merge step finished. Review the Hierarchy/Scene view, then save SampleScene " +
                      "(File > Save, or Ctrl/Cmd+S). Nothing was auto-saved.");
        }

        private static void MergeArCameraAndImageTarget(Scene mainScene)
        {
            var testScene = EditorSceneManager.OpenScene(TestScenePath, OpenSceneMode.Additive);
            try
            {
                var arCameraSrc = testScene.GetRootGameObjects().FirstOrDefault(go => go.name == "ARCamera");
                if (arCameraSrc == null)
                {
                    Debug.LogError("[CircuitAR] Could not find a root GameObject named 'ARCamera' in " + TestScenePath + ".");
                    return;
                }

                // ImageTarget is already parented under ARCamera in VuforiaTest.unity, so instantiating the
                // ARCamera subtree brings the correctly-configured ImageTarget (database/target/tracking refs) with it.
                var clone = UnityEngine.Object.Instantiate(arCameraSrc);
                clone.name = "ARCamera";
                SceneManager.MoveGameObjectToScene(clone, mainScene);

                // Drop the Phase 1 test cube — it was only there to visually confirm tracking in the isolated scene.
                var testCube = clone.transform.Find("ImageTarget/TestCube");
                if (testCube != null) UnityEngine.Object.DestroyImmediate(testCube.gameObject);

                var imageTarget = clone.transform.Find("ImageTarget");
                if (imageTarget != null) imageTarget.name = "ImageTarget";

                Debug.Log("[CircuitAR] Copied ARCamera (+ ImageTarget: CircuitARTest / 3d_marker) into SampleScene.");
            }
            finally
            {
                EditorSceneManager.CloseScene(testScene, true); // unload only — never saved, so VuforiaTest.unity is untouched
            }
        }

        private static void DisablePlainMainCameras(Scene scene)
        {
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "ARCamera") continue;
                var cam = go.GetComponent<Camera>();
                if (cam == null) continue;
                if (HasComponentNamed(go, "Vuforia")) continue; // a Vuforia-driven camera — leave it alone

                if (go.activeSelf)
                {
                    go.SetActive(false);
                    Debug.Log("[CircuitAR] Disabled plain camera '" + go.name + "' so it doesn't conflict with ARCamera. " +
                              "It was deactivated, not deleted — delete it yourself once you've confirmed AR works.");
                }
            }
        }

        private static bool HasComponentNamed(GameObject go, string typeNameContains)
        {
            return go.GetComponents<Component>().Any(c => c != null && c.GetType().Name.IndexOf(typeNameContains, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, false);
                if (t != null) return t;
            }
            return null;
        }
    }
}
