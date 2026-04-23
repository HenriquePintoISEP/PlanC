using System.Collections.Generic;
using DeckSwipe;

namespace DeckSwipe.CardModel {

    public static class ItemLibrary {

        public static Item CreateItem(ItemType type) {
            switch (type) {
                case ItemType.Generator:
                    return new Item(type,
                            "Generator",
                            "A backup generator that keeps the lights on when everything else fails.",
                            "You managed to take the generator home.",
                            new List<DisasterType>());
                case ItemType.Flashlight:
                    return new Item(type,
                            "Flashlight",
                            "A reliable beam of light to guide you through blackouts.",
                            "You snagged the flashlight before the sun went down.",
                            new List<DisasterType>());
                case ItemType.Television:
                    return new Item(type,
                            "Television",
                            "A small television that helps you stay connected to broadcasting news.",
                            "You carried the television back to your home.",
                            new List<DisasterType>());
                case ItemType.Radio:
                    return new Item(type,
                            "Radio",
                            "A battery-powered radio for receiving emergency alerts.",
                            "You took the radio with you.",
                            new List<DisasterType>());
                case ItemType.Compass:
                    return new Item(type,
                            "Compass",
                            "A compass for navigating through confusing, damaged streets.",
                            "The compass is now in your pack.",
                            new List<DisasterType>());
                case ItemType.Rope:
                    return new Item(type,
                            "Rope",
                            "A strong rope for climbing, towing, or securing supplies.",
                            "You coiled the rope and added it to your kit.",
                            new List<DisasterType> { DisasterType.Storm, DisasterType.Flood, DisasterType.Wildfire });
                case ItemType.Medkit:
                    return new Item(type,
                            "Medkit",
                            "A first-aid kit stocked with bandages and pain relief.",
                            "You grabbed the medkit and tucked it away.",
                            new List<DisasterType>());
                case ItemType.Newspaper:
                    return new Item(type,
                            "Newspaper",
                            "A stack of newspapers with maps, headlines, and local updates.",
                            "You brought the newspapers back with you.",
                            new List<DisasterType>());
                case ItemType.SurvivalBook:
                    return new Item(type,
                            "Survival Book",
                            "A guidebook with survival tips and emergency know-how.",
                            "You clipped the survival book into your bag.",
                            new List<DisasterType>());
                case ItemType.SwissKnife:
                    return new Item(type,
                            "Swiss Army Knife",
                            "A multipurpose tool useful in many emergency situations.",
                            "You added the Swiss Army Knife to your gear.",
                            new List<DisasterType>());
                case ItemType.Barricade:
                    return new Item(type,
                            "Barricade",
                            "Wooden boards and nails to strengthen your house against strong winds.",
                            "You hauled the barricade materials home.",
                            new List<DisasterType> { DisasterType.Wildfire, DisasterType.Storm });
                case ItemType.BoardedUpWindow:
                    return new Item(type,
                            "Boarded Up Window",
                            "Boards that reinforce your house windows before a storm hits.",
                            "You secured the house windows with boards.",
                            new List<DisasterType> { DisasterType.Storm });
                case ItemType.LifeVest:
                    return new Item(type,
                            "Life Vest",
                            "A life vest designed for fast water escapes.",
                            "You slipped the life vest into your pack.",
                            new List<DisasterType> { DisasterType.Flood });
                case ItemType.Sandbags:
                    return new Item(type,
                            "Sandbags",
                            "Sandbags to keep rising water out of your home.",
                            "You dragged a stack of sandbags along.",
                            new List<DisasterType> { DisasterType.Flood });
                case ItemType.WaterBottles:
                    return new Item(type,
                            "Water Bottles",
                            "Fresh water to keep you hydrated through dry spells.",
                            "You packed the water bottles.",
                            new List<DisasterType> { DisasterType.Drought });
                default:
                    return new Item(type,
                            type.ToString(),
                            "A useful item.",
                            "You collected the item.",
                            new List<DisasterType>());
            }
        }

    }

}
