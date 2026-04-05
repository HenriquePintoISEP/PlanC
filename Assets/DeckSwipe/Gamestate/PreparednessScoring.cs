using System;
using System.Collections.Generic;
using DeckSwipe.CardModel;

namespace DeckSwipe.Gamestate {

	// Snapshot of four resource stats at any point during a run.
	// Used to evaluate preparedness before and after decisions.
	public struct PreparednessState {

		public int Health;
		public int Supplies;
		public int Safety;
		public int Community;

		public PreparednessState(int health, int supplies, int safety, int community) {
			Health = health;
			Supplies = supplies;
			Safety = safety;
			Community = community;
		}

		public int Total => Health + Supplies + Safety + Community;

		// Returns a new state with stats modified and clamped to [minValue, maxValue].
		public PreparednessState Apply(StatsModification modification, int minValue, int maxValue) {
			if (modification == null) {
				return this;
			}

			return new PreparednessState(
				Math.Clamp(Health + modification.health, minValue, maxValue),
				Math.Clamp(Supplies + modification.supplies, minValue, maxValue),
				Math.Clamp(Safety + modification.safety, minValue, maxValue),
				Math.Clamp(Community + modification.community, minValue, maxValue));
		}

	}

	// Represents a single decision shown to the player with its two possible outcomes.
	// Records which choice (left/right) the player actually made.
	public struct PreparednessDecision {

		public string CardText;
		public StatsModification LeftDelta;
		public StatsModification RightDelta;
		public bool ChoseLeft;

		public PreparednessDecision(string cardText, StatsModification leftDelta, StatsModification rightDelta, bool choseLeft) {
			CardText = cardText;
			LeftDelta = leftDelta;
			RightDelta = rightDelta;
			ChoseLeft = choseLeft;
		}

	}

	// Final results comparing player's actual run against the theoretically best possible run.
	// Includes scores, final stats, and the optimal decision sequence.
	public sealed class PreparednessReport {

		public int DecisionCount { get; }
		public int ActualScore { get; }
		public int BestPossibleScore { get; }
		public int ScoreGap => BestPossibleScore - ActualScore;

		public PreparednessState ActualFinalState { get; }
		public PreparednessState BestPossibleFinalState { get; }
		public IReadOnlyList<bool> BestDecisionPath { get; }

		public PreparednessReport(
				int decisionCount,
				int actualScore,
				int bestPossibleScore,
				PreparednessState actualFinalState,
				PreparednessState bestPossibleFinalState,
				IReadOnlyList<bool> bestDecisionPath) {
			DecisionCount = decisionCount;
			ActualScore = actualScore;
			BestPossibleScore = bestPossibleScore;
			ActualFinalState = actualFinalState;
			BestPossibleFinalState = bestPossibleFinalState;
			BestDecisionPath = bestDecisionPath;
		}

	}

	// Collects all decisions made during a playthrough and generates a preparedness report.
	// Called every time the player swipes left or right.
	public sealed class PreparednessRunTracker {

		private readonly List<PreparednessDecision> decisions = new List<PreparednessDecision>();

		// Clears the decision history at the start of a new run.
		public void Reset() {
			decisions.Clear();
		}

		// Records a decision made by the player (which choice was selected).
		public void RecordDecision(ICard card, bool choseLeft) {
			decisions.Add(new PreparednessDecision(
				card?.CardText ?? string.Empty,
				card?.LeftSwipeOutcome?.StatsModification,
				card?.RightSwipeOutcome?.StatsModification,
				choseLeft));
		}

		// Computes best possible state and score, then builds a comparison report.
		public PreparednessReport BuildReport(PreparednessState initialState, PreparednessState actualFinalState, int minValue, int maxValue) {
			PreparednessState bestPossibleState = PreparednessScoring.FindBestPossibleFinalState(decisions, initialState, minValue, maxValue);
			IReadOnlyList<bool> bestDecisionPath = PreparednessScoring.FindBestDecisionPath(decisions, initialState, minValue, maxValue);
			int actualScore = PreparednessScoring.CalculateScore(actualFinalState, maxValue);
			int bestScore = PreparednessScoring.CalculateScore(bestPossibleState, maxValue);

			return new PreparednessReport(decisions.Count, actualScore, bestScore, actualFinalState, bestPossibleState, bestDecisionPath);
		}

	}

	// Core scoring system using dynamic programming to find the best possible final state
	// from the same sequence of choices presented to the player.
	public static class PreparednessScoring {

		// Memoization key: (which decision index, current state).
		// Allows caching to avoid recalculating the same decision point.
		private readonly struct StateKey : IEquatable<StateKey> {

			public readonly int DecisionIndex;
			public readonly PreparednessState State;

			public StateKey(int decisionIndex, PreparednessState state) {
				DecisionIndex = decisionIndex;
				State = state;
			}

			public bool Equals(StateKey other) {
				return DecisionIndex == other.DecisionIndex
				       && State.Health == other.State.Health
				       && State.Supplies == other.State.Supplies
				       && State.Safety == other.State.Safety
				       && State.Community == other.State.Community;
			}

			public override bool Equals(object obj) {
				return obj is StateKey other && Equals(other);
			}

			public override int GetHashCode() {
				HashCode hashCode = new HashCode();
				hashCode.Add(DecisionIndex);
				hashCode.Add(State.Health);
				hashCode.Add(State.Supplies);
				hashCode.Add(State.Safety);
				hashCode.Add(State.Community);
				return hashCode.ToHashCode();
			}

		}

		// Uses dynamic programming to find the best final state reachable from initial state.
		public static PreparednessState FindBestPossibleFinalState(
				IReadOnlyList<PreparednessDecision> decisions,
				PreparednessState initialState,
				int minValue,
				int maxValue) {
			Dictionary<StateKey, PreparednessState> memo = new Dictionary<StateKey, PreparednessState>();
			return Solve(0, initialState, decisions, minValue, maxValue, memo);
		}

		// Reconstructs the optimal left/right sequence that leads to the best final state.
		public static IReadOnlyList<bool> FindBestDecisionPath(
				IReadOnlyList<PreparednessDecision> decisions,
				PreparednessState initialState,
				int minValue,
				int maxValue) {
			Dictionary<StateKey, PreparednessState> memo = new Dictionary<StateKey, PreparednessState>();
			List<bool> path = new List<bool>(decisions.Count);
			PreparednessState current = initialState;

			for (int i = 0; i < decisions.Count; i++) {
				PreparednessDecision decision = decisions[i];
				PreparednessState leftState = current.Apply(decision.LeftDelta, minValue, maxValue);
				PreparednessState leftFinal = Solve(i + 1, leftState, decisions, minValue, maxValue, memo);

				PreparednessState rightState = current.Apply(decision.RightDelta, minValue, maxValue);
				PreparednessState rightFinal = Solve(i + 1, rightState, decisions, minValue, maxValue, memo);

				bool chooseLeft = Compare(leftFinal, rightFinal, maxValue) >= 0;
				path.Add(chooseLeft);
				current = chooseLeft ? leftState : rightState;
			}

			return path;
		}

		// Converts final state total resources into a 0-100 preparedness percentage.
		public static int CalculateScore(PreparednessState state, int maxValue) {
			float ratio = (float) state.Total / (4 * maxValue);
			return (int) Math.Round(ratio * 100.0f);
		}

		// Recursive DP solver: explores all possible decision outcomes from current state onwards,
		// returning the best final state reachable. Results are memoized.
		private static PreparednessState Solve(
				int index,
				PreparednessState currentState,
				IReadOnlyList<PreparednessDecision> decisions,
				int minValue,
				int maxValue,
				Dictionary<StateKey, PreparednessState> memo) {
			if (index >= decisions.Count) {
				return currentState;
			}

			StateKey key = new StateKey(index, currentState);
			if (memo.TryGetValue(key, out PreparednessState cached)) {
				return cached;
			}

			PreparednessDecision decision = decisions[index];

			PreparednessState leftState = currentState.Apply(decision.LeftDelta, minValue, maxValue);
			PreparednessState leftFinal = Solve(index + 1, leftState, decisions, minValue, maxValue, memo);

			PreparednessState rightState = currentState.Apply(decision.RightDelta, minValue, maxValue);
			PreparednessState rightFinal = Solve(index + 1, rightState, decisions, minValue, maxValue, memo);

			PreparednessState best = Compare(leftFinal, rightFinal, maxValue) >= 0 ? leftFinal : rightFinal;
			memo[key] = best;
			return best;
		}

		// Ranks two states: first by total score, then by weakest resource (tiebreaker).
		// Returns >0 if a is better, <0 if b is better.
		private static int Compare(PreparednessState a, PreparednessState b, int maxValue) {
			int scoreDelta = CalculateScore(a, maxValue) - CalculateScore(b, maxValue);
			if (scoreDelta != 0) {
				return scoreDelta;
			}

			int minA = Math.Min(Math.Min(a.Health, a.Supplies), Math.Min(a.Safety, a.Community));
			int minB = Math.Min(Math.Min(b.Health, b.Supplies), Math.Min(b.Safety, b.Community));
			return minA - minB;
		}

	}

}