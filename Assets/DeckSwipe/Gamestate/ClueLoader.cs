using System;
using System.Collections.Generic;
using UnityEngine;
using DeckSwipe;

namespace DeckSwipe.Gamestate {

    [Serializable]
    public class ClueCollection {

        public List<string> generic;
        public List<DisasterClue> specific;
        public List<DisasterClue> mostSpecific;

        public bool HasAnyClues() {
            return (generic != null && generic.Count > 0)
                || (specific != null && specific.Count > 0)
                || (mostSpecific != null && mostSpecific.Count > 0);
        }

        public string GetRandomGeneric() {
            if (generic == null || generic.Count == 0) {
                return null;
            }
            return generic[UnityEngine.Random.Range(0, generic.Count)];
        }

        public string GetRandomSpecific(DisasterType disasterType) {
            return GetRandomClueForList(specific, disasterType);
        }

        public string GetRandomMostSpecific(DisasterType disasterType) {
            return GetRandomClueForList(mostSpecific, disasterType);
        }

        private string GetRandomClueForList(List<DisasterClue> list, DisasterType disasterType) {
            if (list == null || list.Count == 0) {
                return null;
            }

            DisasterClue disasterClue = list.Find(entry => entry.Matches(disasterType));
            if (disasterClue == null || disasterClue.clues == null || disasterClue.clues.Count == 0) {
                return null;
            }

            return disasterClue.clues[UnityEngine.Random.Range(0, disasterClue.clues.Count)];
        }

    }

    [Serializable]
    public class DisasterClue {
        public string disasterType;
        public List<string> clues;

        public bool Matches(DisasterType disasterTypeValue) {
            return !string.IsNullOrWhiteSpace(disasterType)
                && string.Equals(disasterType.Trim(), disasterTypeValue.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class ClueLoader {

        private const string ResourcePath = "Collection/Clues/clues";

        public static ClueCollection Load() {
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null) {
                Debug.LogWarning("[ClueLoader] Clue file not found at Resources/" + ResourcePath);
                return null;
            }

            try {
                ClueCollection collection = JsonUtility.FromJson<ClueCollection>(asset.text);
                if (collection == null || !collection.HasAnyClues()) {
                    Debug.LogWarning("[ClueLoader] Clue file loaded but contained no entries");
                    return null;
                }
                return collection;
            }
            catch (Exception e) {
                Debug.LogError("[ClueLoader] Failed to parse clue data: " + e.Message);
                return null;
            }
        }

    }

}
