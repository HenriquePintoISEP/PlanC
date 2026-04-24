using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {

    public class InventoryCanvasController : MonoBehaviour {

        [Tooltip("Reference to the main game controller that handles inventory mode.")]
        public Game game;

        [Tooltip("The root GameObject for the normal game canvas.")]
        public GameObject mainCanvas;

        [Tooltip("The root GameObject for the inventory canvas.")]
        public GameObject inventoryCanvas;

        [Tooltip("Optional button that opens and closes inventory mode.")]
        public Button toggleButton;

        [Tooltip("Optional label shown on the toggle button.")]
        public TextMeshProUGUI toggleButtonLabel;

        [Tooltip("Text shown when inventory is closed.")]
        public string openInventoryText = "Open Inventory";

        [Tooltip("Text shown when inventory is open.")]
        public string closeInventoryText = "Close Inventory";

        private void Awake() {
            if (toggleButton != null) {
                toggleButton.onClick.AddListener(OnToggleButtonClicked);
            }

            UpdateCanvasVisibility();
        }

        private void Update() {
            UpdateCanvasVisibility();
        }

        private void OnToggleButtonClicked() {
            if (game == null) {
                return;
            }

            if (game.InventoryModeActive) {
                game.CloseInventoryMode();
            }
            else {
                game.OpenInventoryMode();
            }

            UpdateCanvasVisibility();
        }

        private void UpdateCanvasVisibility() {
            bool inventoryActive = game != null && game.InventoryModeActive;
            bool showToggle = game != null && game.CanOpenInventory;

            if (mainCanvas != null) {
                mainCanvas.SetActive(!inventoryActive);
            }

            if (inventoryCanvas != null) {
                inventoryCanvas.SetActive(inventoryActive);
            }

            if (toggleButton != null) {
                toggleButton.gameObject.SetActive(showToggle);
            }

            if (toggleButtonLabel != null) {
                toggleButtonLabel.text = inventoryActive ? closeInventoryText : openInventoryText;
            }
        }

    }

}
