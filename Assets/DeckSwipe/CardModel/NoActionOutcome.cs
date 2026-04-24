using DeckSwipe.World;

namespace DeckSwipe.CardModel {

	public class NoActionOutcome : IActionOutcome {

		public StatsModification StatsModification => null;

		public void Perform(Game controller) {
			ProgressDisplay.ShowTimeProgress(true);
			controller.DrawNextCard();
		}

	}

}