using System.Collections.Generic;
using DeckSwipe.CardModel;
using DeckSwipe.Gamestate;
using Outfrost;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {
	
	public class StatsDisplay : MonoBehaviour {
		
		public Image healthBar;
		public Image suppliesBar;
		public Image safetyBar;
		public Image communityBar;

		[Header("Indicator Orbs")]
		public Image healthIndicator;
		public Image suppliesIndicator;
		public Image safetyIndicator;
		public Image communityIndicator;

		public float relativeMargin;
		
		private float minFillAmount;
		private float maxFillAmount;
		private static bool showResourceIndicators = true;
		private static readonly List<StatsDisplay> _listeners = new List<StatsDisplay>();
		
		private void Awake() {
			minFillAmount = Mathf.Clamp01(relativeMargin);
			maxFillAmount = Mathf.Clamp01(1.0f - relativeMargin);
			
			SetIndicators(null, 0f);

			if (!Util.IsPrefab(gameObject)) {
				Stats.AddChangeListener(this);
				_listeners.Add(this);
				TriggerUpdate();
			}
		}
		
		public void TriggerUpdate() {
			healthBar.fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, Stats.HealthPercentage);
			suppliesBar.fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, Stats.SuppliesPercentage);
			safetyBar.fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, Stats.SafetyPercentage);
			communityBar.fillAmount = Mathf.Lerp(minFillAmount, maxFillAmount, Stats.CommunityPercentage);
		}

		public void SetIndicators(StatsModification mod, float opacity) {
			SetIndicatorState(healthIndicator, mod?.health ?? 0, opacity);
			SetIndicatorState(suppliesIndicator, mod?.supplies ?? 0, opacity);
			SetIndicatorState(safetyIndicator, mod?.safety ?? 0, opacity);
			SetIndicatorState(communityIndicator, mod?.community ?? 0, opacity);
		}

		public static void ShowResourceIndicators(bool show) {
			showResourceIndicators = show;
			for (int i = 0; i < _listeners.Count; i++) {
				if (_listeners[i] == null) {
					_listeners.RemoveAt(i);
				}
				else {
					_listeners[i].SetResourceIndicatorVisibility(show);
				}
			}
		}

		private void SetIndicatorState(Image indicator, int statChange, float alpha) {
			if (indicator == null) {
				return;
			}

			if (!showResourceIndicators) {
				indicator.gameObject.SetActive(false);
				return;
			}

			indicator.gameObject.SetActive(true);
			Color c = indicator.color;
			c.a = statChange != 0 ? alpha : 0f;
			indicator.color = c;

			// Scale based on magnitude of change (small dot for |change| <= 1, larger for more)
			if (statChange != 0) {
				float pointScale = Mathf.Abs(statChange) > 1 ? 1.0f : 0.6f;
				pointScale *= 1.75f;
				indicator.rectTransform.localScale = new Vector3(pointScale, pointScale, 1.0f);
			}
		}

		private void SetResourceIndicatorVisibility(bool show) {
			if (healthIndicator != null) {
				healthIndicator.gameObject.SetActive(show);
			}
			if (suppliesIndicator != null) {
				suppliesIndicator.gameObject.SetActive(show);
			}
			if (safetyIndicator != null) {
				safetyIndicator.gameObject.SetActive(show);
			}
			if (communityIndicator != null) {
				communityIndicator.gameObject.SetActive(show);
			}
			if (show) {
				SetIndicators(null, 0f);
			}
		}

		private void OnDestroy() {
			_listeners.Remove(this);
		}
	}
	
}
