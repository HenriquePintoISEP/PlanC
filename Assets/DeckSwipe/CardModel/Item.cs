using System.Collections.Generic;
using DeckSwipe;

namespace DeckSwipe.CardModel {

    public enum ItemType {
        Generator,
        Flashlight,
        Television,
        Radio,
        Compass,
        Rope,
        Medkit,
        Newspaper,
        SurvivalBook,
        SwissKnife,
        Barricade,
        BoardedUpWindow,
        LifeVest,
        Sandbags,
        WaterBottles
    }

    public class Item {

        public ItemType Type { get; }
        public string Title { get; }
        public string EffectDescription { get; }
        public string AcquisitionText { get; }
        public IReadOnlyList<DisasterType> PreparednessFor { get; }
        public bool Used { get; private set; }

        public Item(
                ItemType type,
                string title,
                string effectDescription,
                string acquisitionText = "",
                List<DisasterType> preparednessFor = null) {
            this.Type = type;
            this.Title = title;
            this.EffectDescription = effectDescription;
            this.AcquisitionText = acquisitionText;
            this.PreparednessFor = (preparednessFor ?? new List<DisasterType>()).AsReadOnly();
            this.Used = false;
        }

        public bool IsPreparednessItem {
            get { return PreparednessFor != null && PreparednessFor.Count > 0; }
        }

        public void MarkUsed() {
            Used = true;
        }

    }

}
