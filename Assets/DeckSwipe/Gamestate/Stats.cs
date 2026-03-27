using System.Collections.Generic;
using DeckSwipe.CardModel;
using DeckSwipe.World;
using UnityEngine;

namespace DeckSwipe.Gamestate {
	
	public static class Stats {
		
		private const int _maxStatValue = 32;
		private const int _startingHealth = 16;
		private const int _startingSupplies = 16;
		private const int _startingSafety = 16;
		private const int _startingCommunity = 16;
		
		private static readonly List<StatsDisplay> _changeListeners = new List<StatsDisplay>();
		
		public static int Health { get; private set; }
		public static int Supplies { get; private set; }
		public static int Safety { get; private set; }
		public static int Community { get; private set; }
		
		public static float HealthPercentage => (float) Health / _maxStatValue;
		public static float SuppliesPercentage => (float) Supplies / _maxStatValue;
		public static float SafetyPercentage => (float) Safety / _maxStatValue;
		public static float CommunityPercentage => (float) Community / _maxStatValue;
		
		public static void ApplyModification(StatsModification mod) {
			Health = ClampValue(Health + mod.health);
			Supplies = ClampValue(Supplies + mod.supplies);
			Safety = ClampValue(Safety + mod.safety);
			Community = ClampValue(Community + mod.community);
			TriggerAllListeners();
		}
		
		public static void ResetStats() {
			ApplyStartingValues();
			TriggerAllListeners();
		}
		
		private static void ApplyStartingValues() {
			Health = ClampValue(_startingHealth);
			Supplies = ClampValue(_startingSupplies);
			Safety = ClampValue(_startingSafety);
			Community = ClampValue(_startingCommunity);
		}
		
		private static void TriggerAllListeners() {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].TriggerUpdate();
				}
			}
		}

		public static void ShowIndicators(StatsModification mod, float opacity) {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].SetIndicators(mod, opacity);
				}
			}
		}
		
		public static void AddChangeListener(StatsDisplay listener) {
			_changeListeners.Add(listener);
		}
		
		private static int ClampValue(int value) {
			return Mathf.Clamp(value, 0, _maxStatValue);
		}
		
	}
	
}
