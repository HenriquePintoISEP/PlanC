namespace DeckSwipe.CardModel {

	public class GameOverOutcome : IActionOutcome {

		public StatsModification StatsModification => null;

		public void Perform(Game controller) {
			controller.RestartGame();
		}

	}

}
