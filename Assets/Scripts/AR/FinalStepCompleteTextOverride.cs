using TMPro;
using UnityEngine;

namespace CircuitAR.AR
{
    // Lets the LED/Series/Parallel controllers reuse the one "THE LED IS ON" / Final Step Complete
    // screen with mode-appropriate text, instead of building three separate completion screens.
    // CircuitDemoController.CheckCircuit() resets it to the LED text; Series/ParallelCircuitController
    // .FinishCircuit() set their own — so whichever mode finishes last always shows the right copy.
    public class FinalStepCompleteTextOverride : MonoBehaviour
    {
        public TextMeshProUGUI chipText;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI bodyText;

        public void SetText(string chip, string title, string body)
        {
            if (chipText != null) chipText.text = chip;
            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;
        }
    }
}
