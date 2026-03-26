using DeckSwipe.CardModel;
using DeckSwipe.CardModel.DrawQueue;
using DeckSwipe.Gamestate;
using DeckSwipe.Gamestate.Persistence;
using DeckSwipe.World;
using Outfrost;
using UnityEngine;

namespace DeckSwipe {

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
		private int currentDay = 1;
		private int currentEnergy = 1;

		public CardStorage CardStorage {
			get { return cardStorage; }
		}

		private CardStorage cardStorage;
		private ProgressStorage progressStorage;
		private float daysPassedPreviously;
		private float daysLastRun;
		private int saveIntervalCounter;
		private CardDrawQueue cardDrawQueue = new CardDrawQueue();

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
			currentDay = 1;
			currentEnergy = 1;
			ProgressDisplay.UpdateTimeProgress(currentDay, currentEnergy, maxEnergy);
			DrawNextCard();
		}

		public void DrawNextCard() {
			if (currentDay > maxDays) {
				// DISASTER EVENT HOOK: the run is over (reached max days), replace this with calling endgame/disaster results logic later
				Debug.Log("DISASTER HITS! Calculate Preparedness Score.");
				return;
			}
			
			if (Stats.Health == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_health"));
			}
			else if (Stats.Food == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_food"));
			}
			else if (Stats.Coal == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_coal"));
			}
			else if (Stats.Hope == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_hope"));
			}
			else {
				IFollowup followup = cardDrawQueue.Next();
				ICard card = followup?.Fetch(cardStorage) ?? cardStorage.Random();
				SpawnCard(card);
			}
			saveIntervalCounter = (saveIntervalCounter - 1) % _saveInterval;
			if (saveIntervalCounter == 0) {
				progressStorage.Save();
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
			}
			
			ProgressDisplay.UpdateTimeProgress(currentDay, currentEnergy, maxEnergy);
			
			DrawNextCard();
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

	}

}
