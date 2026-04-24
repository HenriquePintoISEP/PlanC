using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeckSwipe;
using DeckSwipe.CardModel;
using DeckSwipe.CardModel.Import;
using DeckSwipe.CardModel.Import.Resource;
using DeckSwipe.CardModel.Prerequisite;
using UnityEngine;

namespace DeckSwipe.Gamestate {

	public class CardStorage {

		private static readonly Character _defaultGameOverCharacter = new Character("", null);

		private readonly Sprite defaultSprite;
		private readonly bool loadRemoteCollectionFirst;

		public Dictionary<int, Card> Cards { get; private set; }
		public Dictionary<string, SpecialCard> SpecialCards { get; private set; }
		public Dictionary<int, Character> Characters { get; private set; }

		public Task CardCollectionImport { get; }

		private List<Card> drawableCards = new List<Card>();

		public CardStorage(Sprite defaultSprite, bool loadRemoteCollectionFirst) {
			this.defaultSprite = defaultSprite;
			this.loadRemoteCollectionFirst = loadRemoteCollectionFirst;
			CardCollectionImport = PopulateCollection();
		}

		public Card Random() {
			return drawableCards[UnityEngine.Random.Range(0, drawableCards.Count)];
		}

	public Card Random(System.Func<Card, bool> filter) {
		List<Card> filteredCards = drawableCards
				.Where(card => filter == null || filter(card))
				.ToList();
		if (filteredCards.Count == 0) {
			return null;
		}
		return filteredCards[UnityEngine.Random.Range(0, filteredCards.Count)];
	}

	public Card Random(DisasterType disasterType) {
		return Random(disasterType, null);
	}

	public Card Random(DisasterType disasterType, System.Func<Card, bool> filter) {
		List<Card> disasterSpecificCards = drawableCards
				.Where(card => card.HasExplicitDisasterTypes && card.IsApplicableToDisaster(disasterType)
					&& (filter == null || filter(card)))
				.ToList();
		List<Card> generalCards = drawableCards
				.Where(card => !card.HasExplicitDisasterTypes && (filter == null || filter(card)))
				.ToList();

		if (disasterType == DisasterType.None || disasterSpecificCards.Count == 0) {
			if (generalCards.Count > 0) {
				return generalCards[UnityEngine.Random.Range(0, generalCards.Count)];
			}
			return Random(filter);
		}

			List<(Card card, int weight)> weightedCandidates = new List<(Card card, int weight)>();
			weightedCandidates.AddRange(disasterSpecificCards.Select(card => (card, 5)));
			weightedCandidates.AddRange(generalCards.Select(card => (card, 1)));
			return WeightedRandom(weightedCandidates);
		}

		private Card WeightedRandom(List<(Card card, int weight)> candidates) {
			int totalWeight = 0;
			for (int i = 0; i < candidates.Count; i++) {
				totalWeight += candidates[i].weight;
			}

			int selectedValue = UnityEngine.Random.Range(0, totalWeight);
			int current = 0;
			for (int i = 0; i < candidates.Count; i++) {
				current += candidates[i].weight;
				if (selectedValue < current) {
					return candidates[i].card;
				}
			}

			return candidates[candidates.Count - 1].card;
		}

		public Card ForId(int id) {
			Card card;
			Cards.TryGetValue(id, out card);
			return card;
		}

		public SpecialCard SpecialCard(string id) {
			SpecialCard card;
			SpecialCards.TryGetValue(id, out card);
			return card;
		}

		public Character Character(int id) {
			Character character;
			if (Characters != null && Characters.TryGetValue(id, out character)) {
				return character;
			}
			return null;
		}

		public void ResolvePrerequisites() {
			drawableCards.Clear();
			foreach (Card card in Cards.Values) {
				card.ResolvePrerequisites(this);
				if (card.PrerequisitesSatisfied()) {
					AddDrawableCard(card);
				}
			}
		}

		public void AddDrawableCard(Card card) {
			drawableCards.Add(card);
		}

		private async Task PopulateCollection() {
			ImportedCards importedCards =
					await new CollectionImporter(defaultSprite, loadRemoteCollectionFirst).Import();
			Cards = importedCards.cards;
			SpecialCards = importedCards.specialCards;
			Characters = importedCards.characters;
			if (Cards == null || Cards.Count == 0) {
				PopulateFallback();
			}
			VerifySpecialCards();
		}

		private void PopulateFallback() {
			Cards = new Dictionary<int, Card>();
			Character placeholderPerson = new Character("Placeholder Person", defaultSprite);
			Cards.Add(0, new Card("Placeholder card 1",
					"A",
					"B",
					placeholderPerson,
					new ActionOutcome(-2, 4, -2, 2),
					new ActionOutcome(2, 0, 2, -2),
					new List<ICardPrerequisite>()));
			Cards.Add(1, new Card("Placeholder card 2",
					"A",
					"B",
					placeholderPerson,
					new ActionOutcome(-1, -1, -1, -1),
					new ActionOutcome(2, 2, 2, 2),
					new List<ICardPrerequisite>()));
			Cards.Add(2, new Card("Placeholder card 3",
					"A",
					"B",
					placeholderPerson,
					new ActionOutcome(0, 1, 1, -2),
					new ActionOutcome(-2, 2, 2, -4),
					new List<ICardPrerequisite>()));
		}

		private void VerifySpecialCards() {
			if (SpecialCards == null) {
				SpecialCards = new Dictionary<string, SpecialCard>();
			}

			/*
				if (!SpecialCards.ContainsKey("gameover_safety")) {
					SpecialCards.Add("gameover_safety", new SpecialCard("The city runs out of coal to run the generator, and freezes over.", "", "",
							_defaultGameOverCharacter,
							new GameOverOutcome(),
							new GameOverOutcome()));
				}
				if (!SpecialCards.ContainsKey("gameover_supplies")) {
					SpecialCards.Add("gameover_supplies", new SpecialCard("Hunger consumes the city, as food reserves deplete.", "", "",
							_defaultGameOverCharacter,
							new GameOverOutcome(),
							new GameOverOutcome()));
				}
				if (!SpecialCards.ContainsKey("gameover_health")) {
					SpecialCards.Add("gameover_health", new SpecialCard("The city's population succumbs to wounds and spreading diseases.", "", "",
							_defaultGameOverCharacter,
							new GameOverOutcome(),
							new GameOverOutcome()));
				}
				if (!SpecialCards.ContainsKey("gameover_community")) {
					SpecialCards.Add("gameover_community", new SpecialCard("All hope among the people is lost.", "", "",
							_defaultGameOverCharacter,
							new GameOverOutcome(),
							new GameOverOutcome()));
				}
			*/

			if (!SpecialCards.ContainsKey("disaster_storm")) {
				SpecialCards.Add("disaster_storm", new SpecialCard("A roaring storm slams into the region, tearing roofs from buildings and flooding streets.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
			if (!SpecialCards.ContainsKey("disaster_flood")) {
				SpecialCards.Add("disaster_flood", new SpecialCard("Rising waters sweep through the city, plunging homes and supplies beneath the flood.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
			if (!SpecialCards.ContainsKey("disaster_drought")) {
				SpecialCards.Add("disaster_drought", new SpecialCard("The dry season persists without relief, and reservoirs crack under relentless heat.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
			if (!SpecialCards.ContainsKey("disaster_wildfire")) {
				SpecialCards.Add("disaster_wildfire", new SpecialCard("Wildfires race across the land, choking the air and consuming everything in their path.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
		}

	}

}
