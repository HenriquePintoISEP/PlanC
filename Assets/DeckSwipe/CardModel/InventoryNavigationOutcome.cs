using DeckSwipe;

namespace DeckSwipe.CardModel {

    public class InventoryNavigationOutcome : IActionOutcome {

        private readonly int direction;

        public InventoryNavigationOutcome(int direction) {
            this.direction = direction;
        }

        public StatsModification StatsModification => null;

        public void Perform(Game controller) {
            if (controller == null) {
                return;
            }

            controller.MoveInventoryItem(direction);
        }

    }

}
