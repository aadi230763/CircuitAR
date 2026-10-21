using CircuitAR.AR;
using UnityEngine;

namespace CircuitAR.UI
{
    // Plays Correct.mp3 / Wrong.mp3 the instant a quiz answer is tapped, layered on top of the
    // existing green/red visual feedback (and the per-option explanation text, where it exists —
    // this doesn't replace or interrupt that). Respects the same mute flag as the rest of the
    // app's audio (CircuitDemoController.IsAudioMuted).
    public class QuizSfx : MonoBehaviour
    {
        public AudioClip correctClip;
        public AudioClip wrongClip;

        private AudioSource source;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        public void PlayCorrect() => Play(correctClip);
        public void PlayWrong() => Play(wrongClip);

        private void Play(AudioClip clip)
        {
            if (clip == null || CircuitDemoController.IsAudioMuted || source == null) return;
            source.PlayOneShot(clip);
        }
    }
}
