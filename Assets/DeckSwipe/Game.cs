using System.Collections.Generic;
using DeckSwipe;
using DeckSwipe.CardModel;
using DeckSwipe.CardModel.DrawQueue;
using DeckSwipe.Gamestate;
using DeckSwipe.Gamestate.Persistence;
using DeckSwipe.World;
using Outfrost;
using System.Text;
using UnityEngine;

namespace DeckSwipe {

	public enum DisasterType { None, Storm, Flood, Drought, Wildfire }
	public enum ResourceType { Health, Supplies, Safety, Community }

	public class Game : MonoBehaviour {

		private const int _saveInterval = 8;

		public InputDispatcher inputDispatcher;
		public CardBehaviour cardPrefab;
		public Vector3 spawnPosition;
		public Sprite defaultCharacterSprite;
		public bool loadRemoteCollectionFirst;

		[Header("Time Progression")]
		public int maxEnergy = 3;
		public int maxDays = 7;
		public string disasterTitle = "Disaster Struck";
		private int currentDay = 1;
		private int currentEnergy = 1;
		private DisasterType selectedDisaster = DisasterType.None;
		private bool disasterCardDisplayed;

		[System.Serializable]
		public class DayInfo {
			public string title;
		}

		public DayInfo[] dayInfos = new[] {
			new DayInfo { title = "The Calm Before the Storm" },
			new DayInfo { title = "Warning Signs" },
			new DayInfo { title = "Rising Tension" },
			new DayInfo { title = "Last Chance" },
			new DayInfo { title = "Breaking Point" },
			new DayInfo { title = "Aftermath" },
			new DayInfo { title = "Final Stand" }
		};

		[Header("Game Over Conditions")]
		[Tooltip("The first resource hitting 0 in this list will trigger its game over card.")]
		public List<ResourceType> gameOverConditions = new List<ResourceType> {
			ResourceType.Health,
			ResourceType.Supplies,
			ResourceType.Safety,
			ResourceType.Community
	};

	public CardStorage CardStorage {
		get { return cardStorage; }
	}

	private CardStorage cardStorage;
	private ProgressStorage progressStorage;
	private float daysPassedPreviously;
	private float daysLastRun;
	private int saveIntervalCounter;
	private CardDrawQueue cardDrawQueue = new CardDrawQueue();
	private PreparednessRunTracker preparednessTracker = new PreparednessRunTracker();

		private void Awake() {
			// Listen for Escape key ('Back' on Android) that suspends the game on Android
			// or ends it on any other platform
			#if UNITY_ANDROID
			inputDispatcher.AddKeyUpHandler(KeyCode.Escape,
					keyCode => {
						AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
							.GetStatic<AndroidJavaObject>("currentActivity");
						activity.Call<bool>("moveTaskToBack", true);
					});
			#else
			inputDispatcher.AddKeyDownHandler(KeyCode.Escape,
					keyCode => Application.Quit());
			#endif

			cardStorage = new CardStorage(defaultCharacterSprite, loadRemoteCollectionFirst);
			progressStorage = new ProgressStorage(cardStorage);

			GameStartOverlay.FadeOutCallback = StartGameplayLoop;
		}

		private void Start() {
			CallbackWhenDoneLoading(StartGame);
		}

		private void StartGame() {
			daysPassedPreviously = progressStorage.Progress.daysPassed;
			GameStartOverlay.StartSequence(progressStorage.Progress.daysPassed, daysLastRun);
		}

		public void RestartGame() {
			progressStorage.Save();
			daysLastRun = progressStorage.Progress.daysPassed - daysPassedPreviously;
			cardDrawQueue.Clear();
			StartGame();
		}

		private void StartGameplayLoop() {
			Stats.ResetStats();
			preparednessTracker.Reset();
			currentDay = 1;
			currentEnergy = 1;
			if (dayInfos != null && dayInfos.Length > 0) {
				maxDays = dayInfos.Length;
			}
			selectedDisaster = ChooseDisasterType();
			Debug.Log("[Game] Selected disaster: " + selectedDisaster);
			ProgressDisplay.SetCurrentDayName(GetDayName(currentDay));
			ProgressDisplay.ShowTimeProgress(true);
			ProgressDisplay.UpdateTimeProgress(currentDay, currentEnergy, maxEnergy);
			DrawNextCard();
		}

		private string GetDayName(int day) {
			if (dayInfos == null || dayInfos.Length == 0 || day < 1 || day > dayInfos.Length) {
				return string.Empty;
			}
			return dayInfos[day - 1].title;
		}

		private DisasterType ChooseDisasterType() {
			DisasterType[] disasterTypes = new[] {
				DisasterType.Storm,
				DisasterType.Flood,
				DisasterType.Drought,
				DisasterType.Wildfire
			};
			return disasterTypes[UnityEngine.Random.Range(0, disasterTypes.Length)];
		}

		public void DrawNextCard() {
			if (currentDay > maxDays) {
				TriggerDisasterEnd();
				return;
			}
			
			bool gameOverTriggered = false;
			foreach (ResourceType condition in gameOverConditions) {
				if (GetStatValue(condition) == 0) {
					SpawnCard(cardStorage.SpecialCard($"gameover_{condition.ToString().ToLower()}"));
					gameOverTriggered = true;
					break;
				}
			}

			if (!gameOverTriggered) {
				IFollowup followup = cardDrawQueue.Next();
				ICard card = followup?.Fetch(cardStorage) ?? cardStorage.Random(selectedDisaster);
				SpawnCard(card);
			}

			saveIntervalCounter = (saveIntervalCounter - 1) % _saveInterval;
			if (saveIntervalCounter == 0) {
				progressStorage.Save();
			}
		}

		private int GetStatValue(ResourceType resource) {
			switch (resource) {
				case ResourceType.Health: return Stats.Health;
				case ResourceType.Supplies: return Stats.Supplies;
				case ResourceType.Safety: return Stats.Safety;
				case ResourceType.Community: return Stats.Community;
				default: return -1;
			}
		}

		public void CardActionPerformed() {
			// Updated the internal tracker for legacy data/statistics so longest run tracking still works
			progressStorage.Progress.AddDays(1.0f / maxEnergy, daysPassedPreviously);
			
			// Handle Energy / Day logic
			currentEnergy++;
			if (currentEnergy > maxEnergy) {
				currentEnergy = 1;
				currentDay++;
				ProgressDisplay.SetCurrentDayName(GetDayName(currentDay));
			}
			
			ProgressDisplay.UpdateTimeProgress(currentDay, currentEnergy, maxEnergy);
			
			if (disasterCardDisplayed) {
				disasterCardDisplayed = false;
				CompleteDisasterRun();
				return;
			}

			DrawNextCard();
		}

		public void PerformDecision(ICard card, bool chooseLeft) {
			preparednessTracker.RecordDecision(card, chooseLeft);

			if (chooseLeft) {
				card.PerformLeftDecision(this);
			}
			else {
				card.PerformRightDecision(this);
			}
		}

		public void AddFollowupCard(IFollowup followup) {
			cardDrawQueue.Insert(followup);
		}

		private async void CallbackWhenDoneLoading(Callback callback) {
			await progressStorage.ProgressStorageInit;
			callback();
		}

		private void SpawnCard(ICard card) {
			CardBehaviour cardInstance = Instantiate(cardPrefab, spawnPosition,
					Quaternion.Euler(0.0f, -180.0f, 0.0f));
			cardInstance.Card = card;
			cardInstance.snapPosition.y = spawnPosition.y;
			cardInstance.Controller = this;
		}

		private void CompleteDisasterRun() {
			Debug.Log("[Game] Disaster card resolved and run completed.");
			cardDrawQueue.Clear();
			LogPreparednessReport();
		}

		private void LogPreparednessReport() {
			PreparednessState initialState = new PreparednessState(16, 16, 16, 16);
			PreparednessState actualFinalState = new PreparednessState(Stats.Health, Stats.Supplies, Stats.Safety, Stats.Community);

			PreparednessReport report = preparednessTracker.BuildReport(
				initialState,
				actualFinalState,
				0,
				32);

			Debug.Log($"DISASTER HITS! Preparedness: {report.ActualScore}/100 (best possible: {report.BestPossibleScore}/100, gap: {report.ScoreGap}).");
			Debug.Log($"Actual final stats => H:{report.ActualFinalState.Health} S:{report.ActualFinalState.Supplies} Sa:{report.ActualFinalState.Safety} C:{report.ActualFinalState.Community}");
			Debug.Log($"Best possible stats => H:{report.BestPossibleFinalState.Health} S:{report.BestPossibleFinalState.Supplies} Sa:{report.BestPossibleFinalState.Safety} C:{report.BestPossibleFinalState.Community}");

			if (report.BestDecisionPath != null && report.BestDecisionPath.Count > 0) {
				StringBuilder pathBuilder = new StringBuilder();
				for (int i = 0; i < report.BestDecisionPath.Count; i++) {
					if (i > 0) {
						pathBuilder.Append(", ");
					}
					pathBuilder.Append(report.BestDecisionPath[i] ? "L" : "R");
				}
				Debug.Log($"Best scenario path by turn: {pathBuilder}");
			}
		}

		private void TriggerDisasterEnd() {
			SpecialCard disasterCard = cardStorage.SpecialCard($"disaster_{selectedDisaster.ToString().ToLower()}");
			if (disasterCard != null) {
				disasterCardDisplayed = true;
				ProgressDisplay.SetDayLabelText(disasterTitle);
				ProgressDisplay.ShowTimeProgress(false);
				SpawnCard(disasterCard);
				return;
			}

			PreparednessState initialState = new PreparednessState(16, 16, 16, 16);
			PreparednessState actualFinalState = new PreparednessState(Stats.Health, Stats.Supplies, Stats.Safety, Stats.Community);

			PreparednessReport report = preparednessTracker.BuildReport(
				initialState,
				actualFinalState,
				0,
				32);

			Debug.Log($"DISASTER HITS! Preparedness: {report.ActualScore}/100 (best possible: {report.BestPossibleScore}/100, gap: {report.ScoreGap}).");
			Debug.Log($"Actual final stats => H:{report.ActualFinalState.Health} S:{report.ActualFinalState.Supplies} Sa:{report.ActualFinalState.Safety} C:{report.ActualFinalState.Community}");
			Debug.Log($"Best possible stats => H:{report.BestPossibleFinalState.Health} S:{report.BestPossibleFinalState.Supplies} Sa:{report.BestPossibleFinalState.Safety} C:{report.BestPossibleFinalState.Community}");

			if (report.BestDecisionPath != null && report.BestDecisionPath.Count > 0) {
				StringBuilder pathBuilder = new StringBuilder();
				for (int i = 0; i < report.BestDecisionPath.Count; i++) {
					if (i > 0) {
						pathBuilder.Append(", ");
					}
					pathBuilder.Append(report.BestDecisionPath[i] ? "L" : "R");
				}
				Debug.Log($"Best scenario path by turn: {pathBuilder}");
			}
		}

	}

}
