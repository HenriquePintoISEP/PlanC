using System.Collections.Generic;
using DeckSwipe.CardModel;
using DeckSwipe.World;
using UnityEngine;

namespace DeckSwipe.Gamestate {
	
	public static class Stats {
		
		private const int _maxStatValue = 32;
		private const int _startingHealth = 16;
		private const int _startingFood = 16;
		private const int _startingCoal = 16;
		private const int _startingHope = 16;
		
		private static readonly List<StatsDisplay> _changeListeners = new List<StatsDisplay>();
		
		public static int Health { get; private set; }
		public static int Food { get; private set; }
		public static int Coal { get; private set; }
		public static int Hope { get; private set; }
		
		public static float HealthPercentage => (float) Health / _maxStatValue;
		public static float FoodPercentage => (float) Food / _maxStatValue;
		public static float CoalPercentage => (float) Coal / _maxStatValue;
		public static float HopePercentage => (float) Hope / _maxStatValue;
		
		public static void ApplyModification(StatsModification mod) {
			Health = ClampValue(Health + mod.health);
			Food = ClampValue(Food + mod.food);
			Coal = ClampValue(Coal + mod.coal);
			Hope = ClampValue(Hope + mod.hope);
			TriggerAllListeners();
		}
		
		public static void ResetStats() {
			ApplyStartingValues();
			TriggerAllListeners();
		}
		
		private static void ApplyStartingValues() {
			Health = ClampValue(_startingHealth);
			Food = ClampValue(_startingFood);
			Coal = ClampValue(_startingCoal);
			Hope = ClampValue(_startingHope);
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
		
		public static void AddChangeListener(StatsDisplay listener) {
			_changeListeners.Add(listener);
		}
		
		private static int ClampValue(int value) {
			return Mathf.Clamp(value, 0, _maxStatValue);
		}
		
	}
	
}
