using System;
﻿using DeckSwipe.Gamestate;

namespace DeckSwipe.CardModel {

	[Serializable]
	public class StatsModification {

		public int health;
		public int food;
		public int coal;
		public int hope;

		public StatsModification(int health, int food, int coal, int hope) {
			this.health = health;
			this.food = food;
			this.coal = coal;
			this.hope = hope;
		}

		public void Perform() {
			// TODO Pass through status effects
			Stats.ApplyModification(this);
		}

	}

}
