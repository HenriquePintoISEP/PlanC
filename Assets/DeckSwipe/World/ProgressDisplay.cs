using System.Collections.Generic;
using Outfrost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {
	
	public class ProgressDisplay : MonoBehaviour {
		
		private static readonly List<ProgressDisplay> _changeListeners = new List<ProgressDisplay>();
		
		[Header("Day Display")]
		public TextMeshProUGUI daysSurvivedText;
		public TextMeshProUGUI dayTitleText;
		public bool showDayPrefix = true;

		[Header("Energy Display")]
		public TextMeshProUGUI energyText;
		public Image energyProgressBar;
		public Image energyProgressBarBackground;
		
		public float fillAnimationSpeed = 5.0f; // Speed multiplier for the bar animation
		private float targetFillAmount = 0.0f;
		private bool showTimeProgress = true;
		
		private void Awake() {
			if (!Util.IsPrefab(gameObject)) {
				_changeListeners.Add(this);
				SetDisplay(1, 1, 3); // Defaults, overridden by Game.cs on start
			}
		}

		private void Update() {
			// Smoothly animate the energy progress bar towards its target fill
			if (energyProgressBar != null && Mathf.Abs(energyProgressBar.fillAmount - targetFillAmount) > 0.001f) {
				energyProgressBar.fillAmount = Mathf.Lerp(energyProgressBar.fillAmount, targetFillAmount, Time.deltaTime * fillAnimationSpeed);
			}
		}
		
		public static void UpdateTimeProgress(int days, int currentEnergy, int maxEnergy) {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].SetDisplay(days, currentEnergy, maxEnergy);
				}
			}
		}

		public static void ShowTimeProgress(bool show) {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].SetTimeProgressVisibility(show);
				}
			}
		}

		// Keep this for backwards compatibility with GameStartOverlay animations
		public static void SetDaysSurvived(int days) {
			currentDayNumber = days;
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].UpdateDayOnly(days);
				}
			}
		}

		private static int currentDayNumber = 1;
		private static string currentDayName = string.Empty;

		public static void SetCurrentDayName(string dayName) {
			currentDayName = dayName ?? string.Empty;
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].UpdateDayName();
				}
			}
		}

		public static void SetDayLabelText(string labelText) {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].UpdateDayLabel(labelText ?? string.Empty);
				}
			}
		}

		private void UpdateDayOnly(int days) {
			if (daysSurvivedText != null) {
				daysSurvivedText.text = (showDayPrefix ? "Day " : "") + days.ToString();
			}
		}

		private void UpdateDayName() {
			if (dayTitleText != null) {
				dayTitleText.text = currentDayName;
			}
		}

		private void UpdateDayLabel(string labelText) {
			if (daysSurvivedText != null) {
				daysSurvivedText.text = labelText;
			}
		}
		
		private void SetDisplay(int days, int currentEnergy, int maxEnergy) {
			UpdateDayOnly(days);
			if (showTimeProgress) {
				if (energyText != null) {
					energyText.gameObject.SetActive(true);
					energyText.text = $"{currentEnergy}/{maxEnergy}";
				}

				if (energyProgressBar != null) {
					energyProgressBar.gameObject.SetActive(true);
					targetFillAmount = (float)currentEnergy / maxEnergy;

					// If resetting to the start of the day (e.g. 1/3), snap the bar to 0 immediately so it visually "loads up" to 1 instead of shrinking backwards.
					if (currentEnergy == 1 && energyProgressBar.fillAmount > targetFillAmount) {
						energyProgressBar.fillAmount = 0.0f;
					}
				}
			}
			else {
				if (energyText != null && energyText.gameObject != null) {
					energyText.gameObject.SetActive(false);
				}
				if (energyProgressBar != null && energyProgressBar.gameObject != null) {
					energyProgressBar.gameObject.SetActive(false);
				}
				if (energyProgressBarBackground != null && energyProgressBarBackground.gameObject != null) {
					energyProgressBarBackground.gameObject.SetActive(false);
				}
			}
		}

		private void SetTimeProgressVisibility(bool show) {
			showTimeProgress = show;
			if (energyText != null && energyText.gameObject != null) {
				energyText.gameObject.SetActive(show);
			}
			if (energyProgressBar != null && energyProgressBar.gameObject != null) {
				energyProgressBar.gameObject.SetActive(show);
			}
			if (energyProgressBarBackground != null && energyProgressBarBackground.gameObject != null) {
				energyProgressBarBackground.gameObject.SetActive(show);
			}
		}
		
	}
	
}
