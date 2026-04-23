using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {

    public class PowerOutageDisplay : MonoBehaviour {

        [Tooltip("The CanvasGroup containing the UI elements that should dim during a power outage.")]
        public CanvasGroup uiCanvasGroup;

        [Tooltip("Alpha to use when the UI is dimmed. Set to 1 for no dimming, 0 for fully dark.")]
        [Range(0.0f, 1.0f)]
        public float dimAlpha = 0.35f;

        private float originalAlpha = 1.0f;
        private static PowerOutageDisplay instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Debug.LogWarning("Multiple PowerOutageDisplay instances found. Only the first will be used.");
            }

            if (uiCanvasGroup == null) {
                uiCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (uiCanvasGroup != null) {
                originalAlpha = uiCanvasGroup.alpha;
            }
        }

        public static void SetDimmed(bool isDimmed) {
            if (instance == null || instance.uiCanvasGroup == null) {
                return;
            }

            instance.uiCanvasGroup.alpha = isDimmed ? instance.dimAlpha : instance.originalAlpha;
        }
    }
}
