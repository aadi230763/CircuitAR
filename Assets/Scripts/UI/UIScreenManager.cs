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
