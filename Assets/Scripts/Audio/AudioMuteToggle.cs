using CircuitAR.AR;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitAR.Audio
{
    // The speaker/mute icon button built by UIBuilderCommon.MuteButton. Mute state is a static
    // PlayerPref (CircuitDemoController.IsAudioMuted) shared by every instance of this button
    // across screens, so muting on one screen stays muted everywhere else too.
    public class AudioMuteToggle : MonoBehaviour
    {
        public Image icon;
        public Sprite onSprite;
        public Sprite offSprite;

        private void OnEnable() => Refresh();

        // Wired to this button's own onClick at build time.
        public void Toggle()
        {
            CircuitDemoController.IsAudioMuted = !CircuitDemoController.IsAudioMuted;
            if (CircuitDemoController.IsAudioMuted)
            {
                var ctrl = FindObjectOfType<CircuitDemoController>();
                if (ctrl != null) ctrl.StopAudio();
            }
            Refresh();
        }

        private void Refresh()
        {
            if (icon != null) icon.sprite = CircuitDemoController.IsAudioMuted ? offSprite : onSprite;
        }
    }
}
