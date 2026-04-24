using DeckSwipe.CardModel.DrawQueue;

namespace DeckSwipe.CardModel {

    public class ItemOutcome : IActionOutcome {

        public StatsModification StatsModification => statsModification ?? new StatsModification(0, 0, 0, 0);
        public Item Item => item;

        private readonly Item item;
        private readonly StatsModification statsModification;
        private readonly IFollowup followup;

        public ItemOutcome(Item item, StatsModification statsModification = null, IFollowup followup = null) {
            this.item = item;
            this.statsModification = statsModification;
            this.followup = followup;
        }

        public void Perform(Game controller) {
            if (statsModification != null) {
                statsModification.Perform();
            }
            if (item != null) {
                controller.AddItem(item);
            }
            if (followup != null) {
                controller.AddFollowupCard(followup);
            }
            controller.CardActionPerformed();
        }

    }

}
