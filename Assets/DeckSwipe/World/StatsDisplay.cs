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
		
		private void Awake() {
			minFillAmount = Mathf.Clamp01(relativeMargin);
			maxFillAmount = Mathf.Clamp01(1.0f - relativeMargin);
			
			SetIndicators(null, 0f);

			if (!Util.IsPrefab(gameObject)) {
				Stats.AddChangeListener(this);
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

		private void SetIndicatorState(Image indicator, int statChange, float alpha) {
			if (indicator != null) {
				// Fade out entirely if no change
				Color c = indicator.color;
				c.a = statChange != 0 ? alpha : 0f;
				indicator.color = c;

				// Scale based on magnitude of change (small dot for |change| <= 1, larger for more)
				if (statChange != 0) {
					float pointScale = Mathf.Abs(statChange) > 1 ? 1.0f : 0.6f;
					indicator.rectTransform.localScale = new Vector3(pointScale, pointScale, 1.0f);
				}
			}
		}
		
	}
	
}
