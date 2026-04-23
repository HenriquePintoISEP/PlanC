using DeckSwipe.CardModel.Prerequisite;

using System.Collections.Generic;

namespace DeckSwipe.CardModel {

    public static class ItemCardFactory {

        public static Card CreateItemAcquisitionCard(
                Item item,
                Character character,
                string cardText,
                string leftSwipeText,
                string rightSwipeText,
                bool itemOnLeft = true) {
            IActionOutcome leftOutcome = itemOnLeft ? new ItemOutcome(item) : new ActionOutcome();
            IActionOutcome rightOutcome = itemOnLeft ? new ActionOutcome() : new ItemOutcome(item);

            return new Card(
                    cardText,
                    leftSwipeText,
                    rightSwipeText,
                    character,
                    leftOutcome,
                    rightOutcome,
                    new List<ICardPrerequisite>());
        }

    }

}
