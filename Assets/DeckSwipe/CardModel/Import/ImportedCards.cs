using System.Collections.Generic;
using DeckSwipe.CardModel;

namespace DeckSwipe.CardModel.Import {

	public struct ImportedCards {

		public readonly Dictionary<int, Card> cards;
		public readonly Dictionary<string, SpecialCard> specialCards;
		public readonly Dictionary<int, Character> characters;

		public ImportedCards(
				Dictionary<int, Card> cards,
				Dictionary<string, SpecialCard> specialCards,
				Dictionary<int, Character> characters) {
			this.cards = cards;
			this.specialCards = specialCards;
			this.characters = characters;
		}

	}

}
