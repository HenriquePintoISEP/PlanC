using DeckSwipe.CardModel.DrawQueue;

namespace DeckSwipe.CardModel {

	public class ActionOutcome : IActionOutcome {

		private readonly StatsModification statsModification;
		private readonly IFollowup followup;

		public ActionOutcome() {
			statsModification = new StatsModification(0, 0, 0, 0);
		}

		public ActionOutcome(int healthMod, int foodMod, int coalMod, int hopeMod) {
			statsModification = new StatsModification(healthMod, foodMod, coalMod, hopeMod);
		}

		public ActionOutcome(int healthMod, int foodMod, int coalMod, int hopeMod, IFollowup followup) {
			statsModification = new StatsModification(healthMod, foodMod, coalMod, hopeMod);
			this.followup = followup;
		}

		public ActionOutcome(StatsModification statsModification, IFollowup followup) {
			this.statsModification = statsModification;
			this.followup = followup;
		}

		public void Perform(Game controller) {
			statsModification.Perform();
			if (followup != null) {
				controller.AddFollowupCard(followup);
			}
			controller.CardActionPerformed();
		}

	}

}
