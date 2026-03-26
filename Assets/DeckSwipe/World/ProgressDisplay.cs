using System.Collections.Generic;
using Outfrost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {
	
	public class ProgressDisplay : MonoBehaviour {
		
		private static readonly List<ProgressDisplay> _changeListeners = new List<ProgressDisplay>();
		
		public TextMeshProUGUI daysSurvivedText;
		public TextMeshProUGUI energyText;
		public Image energyProgressBar;
		public bool showDayPrefix = true;
		
		public float fillAnimationSpeed = 5.0f; // Speed multiplier for the bar animation
		private float targetFillAmount = 0.0f;
		
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

		// Keep this for backwards compatibility with GameStartOverlay animations
		public static void SetDaysSurvived(int days) {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].UpdateDayOnly(days);
				}
			}
		}
		
		private void UpdateDayOnly(int days) {
			if (daysSurvivedText != null) {
				daysSurvivedText.text = (showDayPrefix ? "Day " : "") + days.ToString();
			}
		}
		
		private void SetDisplay(int days, int currentEnergy, int maxEnergy) {
			UpdateDayOnly(days);
			if (energyText != null) {
				energyText.text = $"{currentEnergy}/{maxEnergy}";
			}
			
			// Calculate the new target and handle edge-cases like resetting back to day 1 quickly
			if (energyProgressBar != null) {
				targetFillAmount = (float)currentEnergy / maxEnergy;
				
				// If resetting to the start of the day (e.g. 1/3), snap the bar to 0 immediately so it visually "loads up" to 1 instead of shrinking backwards.
				if (currentEnergy == 1 && energyProgressBar.fillAmount > targetFillAmount) {
					energyProgressBar.fillAmount = 0.0f;
				}
			}
		}
		
	}
	
}
