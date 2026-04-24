using System.Collections.Generic;
using System.Linq;
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
		public PowerOutageDisplay powerOutageDisplay;

		[Header("Time Progression")]
		public int maxEnergy = 3;
		public int maxDays = 7;
		public string disasterTitle = "Disaster Struck";

		private int currentDay = 1;
		private int currentEnergy = 1;
		private DisasterType selectedDisaster = DisasterType.None;
		private bool disasterCardDisplayed;
		private bool powerOutageTriggered;
		private bool powerOutageActiveForDay;
		private int powerOutageDay;
		private bool clueDeliveredForCurrentDay;
		private ClueCollection clueCollection;

		[Tooltip("Set the imported character ID for Television broadcasts.")]
		public int televisionCharacterId = 121;

		[Tooltip("Set the imported character ID for Radio broadcasts.")]
		public int radioCharacterId = 113;

		private readonly List<DeckSwipe.CardModel.Item> heldItems = new List<DeckSwipe.CardModel.Item>();

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

		private Card lastRandomCard;

		public IReadOnlyList<DeckSwipe.CardModel.Item> HeldItems {
			get { return heldItems.AsReadOnly(); }
		}

		public bool HasItem(DeckSwipe.CardModel.ItemType itemType) {
			return heldItems.Any(item => item.Type == itemType);
		}

		public bool HasPreparednessItemFor(DisasterType disasterType) {
			return heldItems.Any(item => item.PreparednessFor != null && item.PreparednessFor.Contains(disasterType));
		}

		public void AddItem(DeckSwipe.CardModel.Item item) {
			if (item == null) {
				return;
			}

			if (HasItem(item.Type)) {
				Debug.Log("[Game] Item already held: " + item.Type);
				return;
			}

			heldItems.Add(item);
			Debug.Log("[Game] Added item: " + item.Type);
		}

		public DeckSwipe.CardModel.Item GetItem(DeckSwipe.CardModel.ItemType itemType) {
			return heldItems.FirstOrDefault(item => item.Type == itemType);
		}

		public bool TryGetItem(DeckSwipe.CardModel.ItemType itemType, out DeckSwipe.CardModel.Item item) {
			item = GetItem(itemType);
			return item != null;
		}

		private void Awake() {
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
			clueCollection = ClueLoader.Load();

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
			heldItems.Clear();
			powerOutageTriggered = false;
			powerOutageActiveForDay = false;
			preparednessTracker.Reset();

			progressStorage.Progress.ResetCardProgress();
			cardStorage.ResolvePrerequisites();

			lastRandomCard = null;

			currentDay = 1;
			currentEnergy = 1;

			if (dayInfos != null && dayInfos.Length > 0) {
				maxDays = dayInfos.Length;
			}

			int earliestOutageDay = Mathf.Clamp(Mathf.FloorToInt(maxDays / 3.0f) + 1, 1, maxDays);
			powerOutageDay = UnityEngine.Random.Range(earliestOutageDay, maxDays + 1);

			selectedDisaster = ChooseDisasterType();

			AddItem(ItemLibrary.CreateItem(ItemType.Television));

			clueDeliveredForCurrentDay = false;

			Debug.Log("[Game] Selected disaster: " + selectedDisaster);
			Debug.Log("[Game] Power outage scheduled for day: " + powerOutageDay);

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
				if (TrySpawnPowerOutageCard()) {
					return;
				}

				if (TrySpawnClueCard()) {
					return;
				}

				IFollowup followup = cardDrawQueue.Next();

				if (followup != null) {
					SpawnCard(followup.Fetch(cardStorage));
					return;
				}

				if (TrySpawnClueCard()) {
					return;
				}

				ICard card = SelectNextCard(selectedDisaster);
				SpawnCard(card);
			}

			saveIntervalCounter = (saveIntervalCounter - 1) % _saveInterval;

			if (saveIntervalCounter == 0) {
				progressStorage.Save();
			}
		}

		private ICard SelectNextCard(DisasterType disasterType) {
			Card validCard = cardStorage.Random(disasterType, candidate => {
				if (candidate == lastRandomCard) {
					return false;
				}

				if (!candidate.ItemType.HasValue) {
					return true;
				}

				return !HasItem(candidate.ItemType.Value)
					&& (candidate.Progress.Status & CardStatus.CardShown) != CardStatus.CardShown;
			});

			if (validCard != null) {
				lastRandomCard = validCard;
				return validCard;
			}

			validCard = cardStorage.Random(disasterType, candidate => {
				if (!candidate.ItemType.HasValue) {
					return true;
				}

				return !HasItem(candidate.ItemType.Value)
					&& (candidate.Progress.Status & CardStatus.CardShown) != CardStatus.CardShown;
			});

			if (validCard != null) {
				lastRandomCard = validCard;
				return validCard;
			}

			validCard = cardStorage.Random(disasterType, candidate =>
				!candidate.ItemType.HasValue
				&& candidate != lastRandomCard);

			if (validCard != null) {
				lastRandomCard = validCard;
				return validCard;
			}

			validCard = cardStorage.Random(disasterType, candidate =>
				!candidate.ItemType.HasValue);

			if (validCard != null) {
				lastRandomCard = validCard;
			}

			return validCard;
		}

		private int GetStatValue(ResourceType resource) {
			switch (resource) {
				case ResourceType.Health:
					return Stats.Health;

				case ResourceType.Supplies:
					return Stats.Supplies;

				case ResourceType.Safety:
					return Stats.Safety;

				case ResourceType.Community:
					return Stats.Community;

				default:
					return -1;
			}
		}

		private enum BroadcastDevice {
			Television,
			Radio
		}

		private enum ClueSpecificity {
			Generic,
			Specific,
			MostSpecific
		}

		private bool TrySpawnClueCard() {
			if (clueDeliveredForCurrentDay || clueCollection == null) {
				return false;
			}

			if (!powerOutageTriggered && currentDay == powerOutageDay && currentEnergy == 1) {
				return false;
			}

			if (!TryGetBroadcastDevice(out BroadcastDevice device)) {
				return false;
			}

			string clueText = GetClueTextForCurrentDay();

			if (string.IsNullOrWhiteSpace(clueText)) {
				return false;
			}

			Character broadcaster = GetBroadcastCharacter(device);

			ProgressDisplay.ShowTimeProgress(false);

			SpecialCard clueCard = new SpecialCard(
				clueText,
				"What?",
				"Okay.",
				broadcaster,
				new NoActionOutcome(),
				new NoActionOutcome());

			SpawnCard(clueCard);
			clueDeliveredForCurrentDay = true;

			return true;
		}

		private bool TrySpawnPowerOutageCard() {
			if (powerOutageTriggered || currentEnergy != 1 || currentDay != powerOutageDay) {
				return false;
			}

			SpecialCard outageCard = cardStorage.SpecialCard("power_outage");

			if (outageCard == null) {
				Debug.LogWarning("[Game] Power outage card not found: power_outage");
				return false;
			}

			powerOutageTriggered = true;
			powerOutageActiveForDay = true;

			if (!HasItem(ItemType.Flashlight) && !HasItem(ItemType.Generator)) {
				StatsDisplay.ShowResourceIndicators(false);
				PowerOutageDisplay.SetDimmed(true);
			}

			ProgressDisplay.ShowTimeProgress(false);

			SpecialCard outageDisplayCard = new SpecialCard(
				outageCard.CardText,
				outageCard.LeftSwipeText,
				outageCard.RightSwipeText,
				outageCard.character,
				new NoActionOutcome(),
				new NoActionOutcome());

			SpawnCard(outageDisplayCard);
			return true;
		}

		private bool TryGetBroadcastDevice(out BroadcastDevice device) {
			if (powerOutageActiveForDay) {
				if (HasItem(ItemType.Generator)) {
					device = BroadcastDevice.Television;
					return true;
				}

				if (HasItem(ItemType.Radio)) {
					device = BroadcastDevice.Radio;
					return true;
				}

				device = default;
				return false;
			}

			if (HasItem(ItemType.Television)) {
				device = BroadcastDevice.Television;
				return true;
			}

			device = default;
			return false;
		}

		private ClueSpecificity GetCurrentClueSpecificity() {
			if (powerOutageActiveForDay) {
				return ClueSpecificity.MostSpecific;
			}

			float ratio = maxDays <= 0 ? 1.0f : (float)currentDay / maxDays;

			if (ratio <= 0.35f) {
				return ClueSpecificity.Generic;
			}

			if (ratio <= 0.65f) {
				return ClueSpecificity.Specific;
			}

			return ClueSpecificity.MostSpecific;
		}

		private Character GetBroadcastCharacter(BroadcastDevice device) {
			int characterId = device == BroadcastDevice.Television
				? televisionCharacterId
				: radioCharacterId;

			Character character = null;

			if (characterId >= 0 && cardStorage != null) {
				character = cardStorage.Character(characterId);
			}

			if (character != null) {
				return character;
			}

			return new Character(
				device == BroadcastDevice.Television ? "Television" : "Radio",
				defaultCharacterSprite);
		}

		private string GetClueTextForCurrentDay() {
			ClueSpecificity specificity = GetCurrentClueSpecificity();
			string clueText = null;

			switch (specificity) {
				case ClueSpecificity.Generic:
					clueText = clueCollection.GetRandomGeneric();
					break;

				case ClueSpecificity.Specific:
					clueText = clueCollection.GetRandomSpecific(selectedDisaster);
					break;

				case ClueSpecificity.MostSpecific:
					clueText = clueCollection.GetRandomMostSpecific(selectedDisaster);
					break;
			}

			if (string.IsNullOrWhiteSpace(clueText)
				&& specificity == ClueSpecificity.MostSpecific) {
				clueText = clueCollection.GetRandomSpecific(selectedDisaster);
			}

			if (string.IsNullOrWhiteSpace(clueText)
				&& specificity != ClueSpecificity.Generic) {
				clueText = clueCollection.GetRandomGeneric();
			}

			return clueText;
		}

		public void CardActionPerformed() {
			progressStorage.Progress.AddDays(1.0f / maxEnergy, daysPassedPreviously);

			currentEnergy++;

			if (currentEnergy > maxEnergy) {
				currentEnergy = 1;
				currentDay++;
				powerOutageActiveForDay = false;
				clueDeliveredForCurrentDay = false;
				StatsDisplay.ShowResourceIndicators(true);
				PowerOutageDisplay.SetDimmed(false);
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
			if (card == null) {
				Debug.LogWarning("[Game] Tried to spawn a null card.");
				return;
			}

			CardBehaviour cardInstance = Instantiate(
				cardPrefab,
				spawnPosition,
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

		private PreparednessReport BuildCurrentPreparednessReport() {
			PreparednessState initialState = new PreparednessState(16, 16, 16, 16);

			PreparednessState actualFinalState = new PreparednessState(
				Stats.Health,
				Stats.Supplies,
				Stats.Safety,
				Stats.Community);

			return preparednessTracker.BuildReport(
				initialState,
				actualFinalState,
				0,
				32,
				HeldItems.Select(item => item.Type).Distinct().ToList(),
				selectedDisaster);
		}

		private void LogPreparednessReport() {
			PreparednessReport report = BuildCurrentPreparednessReport();

			Debug.Log($"DISASTER HITS! Preparedness: {report.ActualScore}/100 (best possible: {report.BestPossibleScore}/100, gap: {report.ScoreGap}).");
			Debug.Log($"Decision score: {report.DecisionScore}/100, item score: {report.ItemScore}/100, best item score: {report.BestPossibleItemScore}/100");
			Debug.Log($"Actual final stats => H:{report.ActualFinalState.Health} S:{report.ActualFinalState.Supplies} Sa:{report.ActualFinalState.Safety} C:{report.ActualFinalState.Community}");
			Debug.Log($"Best possible stats => H:{report.BestPossibleFinalState.Health} S:{report.BestPossibleFinalState.Supplies} Sa:{report.BestPossibleFinalState.Safety} C:{report.BestPossibleFinalState.Community}");
			Debug.Log($"User held items: {string.Join(", ", report.ActualItems)}");

			List<ItemType> expectedDisasterItems = report.OfferedItems
				.Where(item => PreparednessScoring.IsRelevantItem(item, selectedDisaster))
				.ToList();

			List<ItemType> ignoredOffered = report.OfferedItems
				.Where(item => PreparednessScoring.IsIgnoredItem(item))
				.ToList();

			List<ItemType> heldRelevantDisasterItems = report.ActualItems
				.Where(item => PreparednessScoring.IsRelevantItem(item, selectedDisaster))
				.ToList();

			List<ItemType> heldIrrelevantDisasterItems = report.ActualItems
				.Where(item => PreparednessScoring.IsSpecificPreparednessItem(item) && !PreparednessScoring.IsRelevantItem(item, selectedDisaster))
				.ToList();

			List<ItemType> heldIgnoredItems = report.ActualItems
				.Where(item => PreparednessScoring.IsIgnoredItem(item))
				.ToList();

			Debug.Log($"Expected disaster-specific offered items: {string.Join(", ", expectedDisasterItems)}");
			Debug.Log($"Ignored offered items: {string.Join(", ", ignoredOffered)}");
			Debug.Log($"Held relevant disaster-specific items: {string.Join(", ", heldRelevantDisasterItems)}");
			Debug.Log($"Held irrelevant disaster-specific items: {string.Join(", ", heldIrrelevantDisasterItems)}");
			Debug.Log($"Held ignored/generic items: {string.Join(", ", heldIgnoredItems)}");

			LogDecisionPaths(report);

			ShowFinalPreparednessScreen(report);
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

			PreparednessReport report = BuildCurrentPreparednessReport();

			Debug.Log($"DISASTER HITS! Preparedness: {report.ActualScore}/100 (best possible: {report.BestPossibleScore}/100, gap: {report.ScoreGap}).");
			Debug.Log($"Decision score: {report.DecisionScore}/100, item score: {report.ItemScore}/100, best item score: {report.BestPossibleItemScore}/100");
			Debug.Log($"Actual final stats => H:{report.ActualFinalState.Health} S:{report.ActualFinalState.Supplies} Sa:{report.ActualFinalState.Safety} C:{report.ActualFinalState.Community}");
			Debug.Log($"Best possible stats => H:{report.BestPossibleFinalState.Health} S:{report.BestPossibleFinalState.Supplies} Sa:{report.BestPossibleFinalState.Safety} C:{report.BestPossibleFinalState.Community}");

			List<ItemType> relevantOffered = report.OfferedItems
				.Where(item => PreparednessScoring.IsRelevantItem(item, selectedDisaster))
				.ToList();

			List<ItemType> ignoredOffered = report.OfferedItems
				.Where(item => PreparednessScoring.IsIgnoredItem(item))
				.ToList();

			List<ItemType> relevantHeld = report.ActualItems
				.Where(item => PreparednessScoring.IsRelevantItem(item, selectedDisaster))
				.ToList();

			List<ItemType> irrelevantHeld = report.ActualItems
				.Where(item => !PreparednessScoring.IsRelevantItem(item, selectedDisaster) && !PreparednessScoring.IsIgnoredItem(item))
				.ToList();

			List<ItemType> ignoredHeld = report.ActualItems
				.Where(item => PreparednessScoring.IsIgnoredItem(item))
				.ToList();

			Debug.Log($"Offered relevant items: {string.Join(", ", relevantOffered)}");
			Debug.Log($"Ignored offered items: {string.Join(", ", ignoredOffered)}");
			Debug.Log($"Held relevant items: {string.Join(", ", relevantHeld)}");
			Debug.Log($"Held irrelevant disaster-specific items: {string.Join(", ", irrelevantHeld)}");
			Debug.Log($"Held ignored/generic items: {string.Join(", ", ignoredHeld)}");

			LogDecisionPaths(report);

			ShowFinalPreparednessScreen(report);
		}

		private void LogDecisionPaths(PreparednessReport report) {
			if (report.ActualDecisionPath != null && report.ActualDecisionPath.Count > 0) {
				StringBuilder actualPathBuilder = new StringBuilder();

				for (int i = 0; i < report.ActualDecisionPath.Count; i++) {
					if (i > 0) {
						actualPathBuilder.Append(", ");
					}

					actualPathBuilder.Append(report.ActualDecisionPath[i] ? "L" : "R");
				}

				Debug.Log($"Actual player path by turn: {actualPathBuilder}");
			}

			if (report.BestDecisionPath != null && report.BestDecisionPath.Count > 0) {
				StringBuilder bestPathBuilder = new StringBuilder();

				for (int i = 0; i < report.BestDecisionPath.Count; i++) {
					if (i > 0) {
						bestPathBuilder.Append(", ");
					}

					bestPathBuilder.Append(report.BestDecisionPath[i] ? "L" : "R");
				}

				Debug.Log($"Best scenario path by turn: {bestPathBuilder}");
			}
		}

		private void ShowFinalPreparednessScreen(PreparednessReport report) {
			if (report == null) {
				return;
			}

			string resultTitle = GetPreparednessResultTitle(report.ActualScore);
			string resultBody = GetPreparednessResultBody(report.ActualScore);
			string improvementText = BuildPreparednessImprovementText(report);

			FinalPreparednessOverlay.ShowResult(
				report.ActualScore,
				resultTitle,
				resultBody,
				improvementText
			);
		}

		private string GetPreparednessResultTitle(int score) {
			if (score >= 85) {
				return "Strong preparation.";
			}

			if (score >= 65) {
				return "Solid preparation.";
			}

			if (score >= 45) {
				return "Partial preparation.";
			}

			if (score >= 25) {
				return "Weak preparation.";
			}

			return "Not prepared.";
		}

		private string GetPreparednessResultBody(int score) {
			if (score >= 85) {
				return "You built a flexible plan by protecting several resources instead of relying on one solution.";
			}

			if (score >= 65) {
				return "You made mostly solid choices, but a few risks were left open when pressure increased.";
			}

			if (score >= 45) {
				return "Your preparation helped in some moments, but it was not balanced enough to stay reliable.";
			}

			if (score >= 25) {
				return "You reacted to some warning signs, but your plan lacked enough backup options.";
			}

			return "You entered the crisis with too many weak points and not enough useful preparation.";
		}

		private string BuildPreparednessImprovementText(PreparednessReport report) {
			int scoreGap = Mathf.Max(0, report.ScoreGap);

			if (scoreGap <= 0) {
				return "Next time, use the same approach: keep resources balanced and choose items that cover different kinds of risk.";
			}

			return GetGapAdviceText(scoreGap);
		}

		private string GetGapAdviceText(int scoreGap) {
			if (scoreGap <= 8) {
				return "Next time, focus on small optimizations: avoid unnecessary losses and keep your strongest resources protected.";
			}

			if (scoreGap <= 18) {
				return "Next time, choose fewer risky shortcuts and prioritize decisions that protect more than one resource.";
			}

			if (scoreGap <= 32) {
				return "Next time, watch for repeated resource losses and use items or choices that cover your weakest areas.";
			}

			if (scoreGap <= 50) {
				return "Next time, build a broader safety net early instead of waiting until the final days to recover.";
			}

			return "Next time, treat every early choice as preparation: protect core resources before the disaster becomes clear.";
		}
	}
}