using System;
﻿using DeckSwipe.Gamestate;

namespace DeckSwipe.CardModel {

	[Serializable]
	public class StatsModification {

		public int health;
		public int supplies;
		public int safety;
		public int community;

		public StatsModification(int health, int supplies, int safety, int community) {
			this.health = health;
			this.supplies = supplies;
			this.safety = safety;
			this.community = community;
		}

		public void Perform() {
			// TODO Pass through status effects
			Stats.ApplyModification(this);
		}

	}

}
