using DeckSwipe.CardModel.DrawQueue;

namespace DeckSwipe.CardModel {

	public class ActionOutcome : IActionOutcome {

		public StatsModification StatsModification => statsModification;

		private readonly StatsModification statsModification;
		private readonly IFollowup followup;

		public ActionOutcome() {
			statsModification = new StatsModification(0, 0, 0, 0);
		}

		public ActionOutcome(int healthMod, int suppliesMod, int safetyMod, int communityMod) {
			statsModification = new StatsModification(healthMod, suppliesMod, safetyMod, communityMod);
		}

		public ActionOutcome(int healthMod, int suppliesMod, int safetyMod, int communityMod, IFollowup followup) {
			statsModification = new StatsModification(healthMod, suppliesMod, safetyMod, communityMod);
			this.followup = followup;
		}

		public ActionOutcome(StatsModification statsModification, IFollowup followup) {
			this.statsModification = statsModification;
			this.followup = followup;
		}

		public void Perform(Game controller) {
			if (statsModification != null) {
				statsModification.Perform();
			}
			if (followup != null) {
				controller.AddFollowupCard(followup);
			}
			controller.CardActionPerformed();
		}

	}

}
