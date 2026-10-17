using System.Collections.Generic;
using UnityEngine;

namespace CircuitAR.UI
{
    // Holds references to every screen's root Canvas GameObject and shows exactly one at a time.
    // Populated by the Tools > Build UI editor scripts via RegisterScreen; buttons are wired to
    // ShowScreen(string) at edit time so the flow is clickable in Play mode / Device Simulator.
    public class UIScreenManager : MonoBehaviour
    {
        [System.Serializable]
        public class ScreenEntry
        {
            public string screenName;
            public GameObject screenRoot;
        }

        [SerializeField] private List<ScreenEntry> screens = new List<ScreenEntry>();
        [SerializeField] private string startScreen = "Canvas_HomeStart";

        private void OnEnable()
        {
            ShowScreen(startScreen);
        }

        public void ShowScreen(string screenName)
        {
            // If the target isn't registered (e.g. its Tools > Build UI step was never run) or its
            // reference went missing after a rebuild, bail out instead of deactivating every screen
            // and leaving nothing shown — that used to fail silently with no Console error.
            bool targetExists = screens.Exists(e => e.screenName == screenName && e.screenRoot != null);
            if (!targetExists)
            {
                Debug.LogWarning($"[CircuitAR] UIScreenManager.ShowScreen(\"{screenName}\") — no registered screen " +
                                   "with that name (or its reference is missing). Staying on the current screen. " +
                                   "Run that screen's Tools > Build UI step, then re-run the wiring that calls ShowScreen.");
                return;
            }

            foreach (var entry in screens)
            {
                if (entry.screenRoot == null) continue;
                entry.screenRoot.SetActive(entry.screenName == screenName);
            }
        }

        public void RegisterScreen(string screenName, GameObject screenRoot)
        {
            var existing = screens.Find(e => e.screenName == screenName);
            if (existing != null) existing.screenRoot = screenRoot;
            else screens.Add(new ScreenEntry { screenName = screenName, screenRoot = screenRoot });
        }

        // TODO: back/secondary navigation (Btn_Back, Btn_Reset, the home screen's "HOW IT WORKS"
        // link button, and Btn_Retry on the Result screen) isn't wired to ShowScreen yet — the
        // screenshots don't make the intended back-target obvious for every one of these.
    }
}
