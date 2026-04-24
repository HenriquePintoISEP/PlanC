using System.Collections.Generic;
using System.Linq;
using DeckSwipe;
using DeckSwipe.CardModel.Prerequisite;
using DeckSwipe.Gamestate;
using UnityEngine;

namespace DeckSwipe.CardModel {

	public class Card : ICard {

		public string CardText { get; }
		public string LeftSwipeText { get; }
		public string RightSwipeText { get; }

		public string CharacterName {
			get { return character != null ? character.name : ""; }
		}

		public Sprite CardSprite {
			get { return character?.sprite; }
		}

		public ICardProgress Progress {
			get { return progress; }
		}

		public Character character;
		public CardProgress progress;

		public IActionOutcome LeftSwipeOutcome => leftSwipeOutcome;
		public IActionOutcome RightSwipeOutcome => rightSwipeOutcome;

		public IReadOnlyList<DisasterType> DisasterTypes { get; }
		public bool HasExplicitDisasterTypes => DisasterTypes.Count > 0;
	public ItemType? ItemType { get; }
	private readonly List<ICardPrerequisite> prerequisites;
		private readonly IActionOutcome leftSwipeOutcome;
		private readonly IActionOutcome rightSwipeOutcome;

		private Dictionary<ICard, ICardPrerequisite> unsatisfiedPrerequisites;
		private List<Card> dependentCards = new List<Card>();

		public Card(
				string cardText,
				string leftSwipeText,
				string rightSwipeText,
				Character character,
				IActionOutcome leftOutcome,
				IActionOutcome rightOutcome,
				List<ICardPrerequisite> prerequisites,
				List<DisasterType> disasterTypes = null,
				ItemType? itemType = null) {
			this.CardText = cardText;
			this.LeftSwipeText = leftSwipeText;
			this.RightSwipeText = rightSwipeText;
			this.character = character;
			leftSwipeOutcome = leftOutcome;
			rightSwipeOutcome = rightOutcome;
			this.prerequisites = prerequisites;
			DisasterTypes = disasterTypes ?? new List<DisasterType>();
			ItemType = itemType;
		}

		public void CardShown(Game controller) {
			progress.Status |= CardStatus.CardShown;
			foreach (Card card in dependentCards) {
				card.CheckPrerequisite(this, controller.CardStorage);
			}
		}

		public void PerformLeftDecision(Game controller) {
			progress.Status |= CardStatus.LeftActionTaken;
			foreach (Card card in dependentCards) {
				card.CheckPrerequisite(this, controller.CardStorage);
			}
			leftSwipeOutcome.Perform(controller);
		}

		public void PerformRightDecision(Game controller) {
			progress.Status |= CardStatus.RightActionTaken;
			foreach (Card card in dependentCards) {
				card.CheckPrerequisite(this, controller.CardStorage);
			}
			rightSwipeOutcome.Perform(controller);
		}

		public bool IsApplicableToDisaster(DisasterType disasterType) {
			return disasterType == DisasterType.None || !HasExplicitDisasterTypes || DisasterTypes.Contains(disasterType);
		}

		public void CheckPrerequisite(ICard dependency, CardStorage cardStorage) {
			if (PrerequisitesSatisfied()
					|| !unsatisfiedPrerequisites.ContainsKey(dependency)) {
				dependency.RemoveDependentCard(this);
				return;
			}

			ICardPrerequisite prerequisite = unsatisfiedPrerequisites[dependency];
			if ((dependency.Progress.Status & prerequisite?.Status) == prerequisite?.Status) {
				unsatisfiedPrerequisites.Remove(dependency);
				dependency.RemoveDependentCard(this);
			}

			if (PrerequisitesSatisfied()) {
				// Duplicate-proof because we've verified that this card's
				// prerequisites were not satisfied before
				cardStorage.AddDrawableCard(this);
			}
		}

		public void ResolvePrerequisites(CardStorage cardStorage) {
			unsatisfiedPrerequisites = new Dictionary<ICard, ICardPrerequisite>();
			dependentCards.Clear();
			foreach (ICardPrerequisite prerequisite in prerequisites) {
				ICard card = prerequisite.GetCard(cardStorage);
				if (card != null
						&& (card.Progress.Status & prerequisite.Status) != prerequisite.Status
						&& !unsatisfiedPrerequisites.ContainsKey(card)) {
					unsatisfiedPrerequisites.Add(card, prerequisite);
					card.AddDependentCard(this);
				}
			}
		}

		public void AddDependentCard(Card card) {
			dependentCards.Add(card);
		}

		public void RemoveDependentCard(Card card) {
			dependentCards.Remove(card);
		}

		public bool PrerequisitesSatisfied() {
			return unsatisfiedPrerequisites.Count == 0;
		}

	}

}
