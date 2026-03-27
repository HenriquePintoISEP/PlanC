using DeckSwipe.CardModel.DrawQueue;

namespace DeckSwipe.CardModel {

	public interface IActionOutcome {

		StatsModification StatsModification { get; }
		void Perform(Game controller);

	}

}
