using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {

    public class InventoryController : MonoBehaviour {

        [Tooltip("Main game object that tracks held items and card drawing.")]
        public Game game;

        [Tooltip("Optional toggle button for inventory mode.")]
        public Button toggleButton;

        [Tooltip("Optional text that updates when inventory mode is open or closed.")]
        public TextMeshProUGUI toggleButtonLabel;

        [Tooltip("Optional UI panel shown only while inventory mode is open.")]
        public GameObject inventoryPanel;

        [Tooltip("Optional header text for inventory mode.")]
        public TextMeshProUGUI inventoryTitleText;

        [Tooltip("Text shown when no items are currently held.")]
        public string emptyInventoryText = "No items held.";

        public bool IsOpen { get; private set; }

        private void Awake() {
            if (toggleButton != null) {
                toggleButton.onClick.AddListener(ToggleInventory);
            }

            if (inventoryPanel != null) {
                inventoryPanel.SetActive(false);
            }

            RefreshButtonLabel();
        }

        private void Update() {
            UpdateToggleButtonVisibility();
        }

        private void UpdateToggleButtonVisibility() {
            if (toggleButton == null || game == null) {
                return;
            }

            toggleButton.gameObject.SetActive(game.CanOpenInventory);
        }

        public void ToggleInventory() {
            if (game == null) {
                return;
            }

            if (game.InventoryModeActive) {
                game.CloseInventoryMode();
                IsOpen = false;
                if (inventoryPanel != null) {
                    inventoryPanel.SetActive(false);
                }
            }
            else {
                if (game.HeldItems == null || game.HeldItems.Count == 0) {
                    if (inventoryPanel != null) {
                        inventoryPanel.SetActive(true);
                    }
                    if (inventoryTitleText != null) {
                        inventoryTitleText.text = emptyInventoryText;
                    }
                    return;
                }

                game.OpenInventoryMode();
                IsOpen = true;
                if (inventoryPanel != null) {
                    inventoryPanel.SetActive(true);
                }
                if (inventoryTitleText != null) {
                    inventoryTitleText.text = "Inventory";
                }
            }

            RefreshButtonLabel();
        }

        private void RefreshButtonLabel() {
            if (toggleButtonLabel == null) {
                return;
            }

            toggleButtonLabel.text = IsOpen ? "Close Inventory" : "Open Inventory";
        }

    }

}
