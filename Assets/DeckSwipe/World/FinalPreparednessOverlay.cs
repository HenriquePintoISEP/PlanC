using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {

	public class FinalPreparednessOverlay : MonoBehaviour {

		private static readonly List<FinalPreparednessOverlay> _listeners = new List<FinalPreparednessOverlay>();

		[Header("Final Screen Timing")]
		public float fadeInDuration = 0.75f;
		public float scoreCountDuration = 1.1f;
		public float typingSpeed = 0.016f;
		public float delayBetweenBlocks = 0.22f;
		public float buttonsFadeDuration = 0.35f;

		[Header("Final Screen Text")]
		public string headerText = "Preparedness Report";

		private Canvas canvas;
		private CanvasGroup overlayGroup;
		private RectTransform overlayRoot;

		private TextMeshProUGUI headerLabel;
		private TextMeshProUGUI scoreLabel;
		private TextMeshProUGUI titleLabel;
		private TextMeshProUGUI bodyLabel;
		private TextMeshProUGUI improvementLabel;

		private CanvasGroup buttonsGroup;
		private Button restartButton;
		private Button quitButton;

		private Coroutine overlayCoroutine;

		private void Awake() {
			if (!_listeners.Contains(this)) {
				_listeners.Add(this);
			}

			BuildOverlay();
		}

		private void Start() {
			SetOverlayVisible(false);
		}

		private void OnDestroy() {
			_listeners.Remove(this);

			if (overlayCoroutine != null) {
				StopCoroutine(overlayCoroutine);
			}
		}

		public static void ShowResult(
			int score,
			string resultTitle,
			string resultBody,
			string improvementText
		) {
			for (int i = _listeners.Count - 1; i >= 0; i--) {
				if (_listeners[i] == null) {
					_listeners.RemoveAt(i);
				}
				else {
					_listeners[i].PlayResult(score, resultTitle, resultBody, improvementText);
				}
			}
		}

		private void BuildOverlay() {
			canvas = GetComponent<Canvas>();

			if (canvas == null) {
				canvas = gameObject.AddComponent<Canvas>();
			}

			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 10000;

			CanvasScaler scaler = GetComponent<CanvasScaler>();

			if (scaler == null) {
				scaler = gameObject.AddComponent<CanvasScaler>();
			}

			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(402.0f, 874.0f);
			scaler.matchWidthOrHeight = 0.5f;

			if (GetComponent<GraphicRaycaster>() == null) {
				gameObject.AddComponent<GraphicRaycaster>();
			}

			GameObject rootObject = new GameObject("Runtime Final Preparedness Screen");
			rootObject.transform.SetParent(transform, false);
			rootObject.transform.SetAsLastSibling();

			overlayRoot = rootObject.AddComponent<RectTransform>();
			overlayRoot.anchorMin = Vector2.zero;
			overlayRoot.anchorMax = Vector2.one;
			overlayRoot.offsetMin = Vector2.zero;
			overlayRoot.offsetMax = Vector2.zero;

			overlayGroup = rootObject.AddComponent<CanvasGroup>();
			overlayGroup.alpha = 0.0f;
			overlayGroup.interactable = true;
			overlayGroup.blocksRaycasts = true;

			CreateBackground(overlayRoot);

			headerLabel = CreateText(
				"Header",
				headerText,
				18,
				new Vector2(0.0f, 285.0f),
				new Vector2(360.0f, 40.0f),
				new Color(0.88f, 0.88f, 0.88f, 1.0f)
			);

			scoreLabel = CreateText(
				"Score",
				"0%",
				70,
				new Vector2(0.0f, 190.0f),
				new Vector2(360.0f, 95.0f),
				Color.white
			);

			titleLabel = CreateText(
				"Result Title",
				"",
				22,
				new Vector2(0.0f, 98.0f),
				new Vector2(360.0f, 58.0f),
				Color.white
			);

			bodyLabel = CreateText(
				"Result Body",
				"",
				17,
				new Vector2(0.0f, 2.0f),
				new Vector2(330.0f, 92.0f),
				new Color(0.86f, 0.86f, 0.86f, 1.0f)
			);

			improvementLabel = CreateText(
				"Improvement Text",
				"",
				17,
				new Vector2(0.0f, -125.0f),
				new Vector2(330.0f, 120.0f),
				new Color(0.86f, 0.86f, 0.86f, 1.0f)
			);

			bodyLabel.lineSpacing = 4.0f;
			improvementLabel.lineSpacing = 4.0f;

			CreateButtons();

			SetOverlayVisible(false);
		}

		private void CreateBackground(RectTransform parent) {
			GameObject backgroundObject = new GameObject("Black Background");
			backgroundObject.transform.SetParent(parent, false);

			RectTransform rect = backgroundObject.AddComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			Image image = backgroundObject.AddComponent<Image>();
			image.color = Color.black;
			image.raycastTarget = true;
		}

		private void CreateButtons() {
			GameObject buttonsObject = new GameObject("Buttons Group");
			buttonsObject.transform.SetParent(overlayRoot, false);
			buttonsObject.transform.SetAsLastSibling();

			RectTransform buttonsRect = buttonsObject.AddComponent<RectTransform>();
			buttonsRect.anchorMin = new Vector2(0.5f, 0.5f);
			buttonsRect.anchorMax = new Vector2(0.5f, 0.5f);
			buttonsRect.pivot = new Vector2(0.5f, 0.5f);
			buttonsRect.anchoredPosition = new Vector2(0.0f, -295.0f);
			buttonsRect.sizeDelta = new Vector2(342.0f, 60.0f);

			buttonsGroup = buttonsObject.AddComponent<CanvasGroup>();
			buttonsGroup.alpha = 0.0f;
			buttonsGroup.interactable = false;
			buttonsGroup.blocksRaycasts = false;

			restartButton = CreateButton(
				"Restart Button",
				"Restart",
				new Vector2(-88.0f, 0.0f),
				new Vector2(158.0f, 50.0f)
			);

			quitButton = CreateButton(
				"Quit Button",
				"Quit",
				new Vector2(88.0f, 0.0f),
				new Vector2(158.0f, 50.0f)
			);

			restartButton.onClick.AddListener(RestartGame);
			quitButton.onClick.AddListener(QuitGame);
		}

		private Button CreateButton(
			string objectName,
			string label,
			Vector2 anchoredPosition,
			Vector2 size
		) {
			GameObject buttonObject = new GameObject(objectName);
			buttonObject.transform.SetParent(buttonsGroup.transform, false);
			buttonObject.transform.SetAsLastSibling();

			RectTransform rect = buttonObject.AddComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			Image image = buttonObject.AddComponent<Image>();
			image.color = new Color(1.0f, 1.0f, 1.0f, 0.12f);
			image.raycastTarget = true;

			Button button = buttonObject.AddComponent<Button>();

			ColorBlock colors = button.colors;
			colors.normalColor = new Color(1.0f, 1.0f, 1.0f, 0.12f);
			colors.highlightedColor = new Color(1.0f, 1.0f, 1.0f, 0.22f);
			colors.pressedColor = new Color(1.0f, 1.0f, 1.0f, 0.32f);
			colors.selectedColor = new Color(1.0f, 1.0f, 1.0f, 0.18f);
			colors.disabledColor = new Color(1.0f, 1.0f, 1.0f, 0.05f);
			button.colors = colors;

			CreateButtonText(
				label + " Text",
				label,
				buttonObject.transform
			);

			return button;
		}

		private TextMeshProUGUI CreateButtonText(
			string objectName,
			string text,
			Transform parent
		) {
			GameObject textObject = new GameObject(objectName);
			textObject.transform.SetParent(parent, false);
			textObject.transform.SetAsLastSibling();

			RectTransform rect = textObject.AddComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
			textComponent.text = text;
			textComponent.fontSize = 18;
			textComponent.fontStyle = FontStyles.Bold;
			textComponent.alignment = TextAlignmentOptions.Center;
			textComponent.color = Color.white;
			textComponent.raycastTarget = false;
			textComponent.enableAutoSizing = false;
			textComponent.textWrappingMode = TextWrappingModes.NoWrap;
			textComponent.overflowMode = TextOverflowModes.Overflow;

			return textComponent;
		}

		private TextMeshProUGUI CreateText(
			string objectName,
			string text,
			float fontSize,
			Vector2 anchoredPosition,
			Vector2 size,
			Color color
		) {
			GameObject textObject = new GameObject(objectName);
			textObject.transform.SetParent(overlayRoot, false);
			textObject.transform.SetAsLastSibling();

			RectTransform rect = textObject.AddComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
			textComponent.text = text;
			textComponent.fontSize = fontSize;
			textComponent.fontStyle = FontStyles.Bold;
			textComponent.alignment = TextAlignmentOptions.Center;
			textComponent.color = color;
			textComponent.raycastTarget = false;
			textComponent.enableAutoSizing = false;
			textComponent.textWrappingMode = TextWrappingModes.Normal;
			textComponent.overflowMode = TextOverflowModes.Overflow;

			return textComponent;
		}

		private void PlayResult(
			int score,
			string resultTitle,
			string resultBody,
			string improvementText
		) {
			if (overlayCoroutine != null) {
				StopCoroutine(overlayCoroutine);
			}

			overlayCoroutine = StartCoroutine(PlayResultCoroutine(
				Mathf.Clamp(score, 0, 100),
				resultTitle,
				resultBody,
				improvementText
			));
		}

		private IEnumerator PlayResultCoroutine(
			int score,
			string resultTitle,
			string resultBody,
			string improvementText
		) {
			SetOverlayVisible(true);
			overlayRoot.SetAsLastSibling();

			overlayGroup.alpha = 0.0f;

			scoreLabel.text = "0%";
			titleLabel.text = "";
			bodyLabel.text = "";
			improvementLabel.text = "";

			bodyLabel.maxVisibleCharacters = int.MaxValue;
			improvementLabel.maxVisibleCharacters = int.MaxValue;

			buttonsGroup.alpha = 0.0f;
			buttonsGroup.interactable = false;
			buttonsGroup.blocksRaycasts = false;

			SetTextAlpha(headerLabel, 0.0f);
			SetTextAlpha(scoreLabel, 0.0f);
			SetTextAlpha(titleLabel, 0.0f);
			SetTextAlpha(bodyLabel, 1.0f);
			SetTextAlpha(improvementLabel, 1.0f);

			yield return FadeOverlayIn();

			SetTextAlpha(headerLabel, 1.0f);
			SetTextAlpha(scoreLabel, 1.0f);

			yield return CountScore(score);

			yield return new WaitForSeconds(delayBetweenBlocks);

			SetTextAlpha(titleLabel, 1.0f);
			yield return TypeTextSmooth(titleLabel, resultTitle);

			yield return new WaitForSeconds(delayBetweenBlocks);

			yield return TypeTextSmooth(bodyLabel, resultBody);

			yield return new WaitForSeconds(delayBetweenBlocks);

			yield return TypeTextSmooth(improvementLabel, improvementText);

			yield return new WaitForSeconds(0.18f);

			yield return FadeButtonsIn();

			overlayCoroutine = null;
		}

		private IEnumerator FadeOverlayIn() {
			float startTime = Time.time;

			while (Time.time - startTime < fadeInDuration) {
				float progress = Mathf.Clamp01((Time.time - startTime) / fadeInDuration);
				overlayGroup.alpha = Mathf.Lerp(0.0f, 1.0f, EaseInOutSine(progress));
				yield return null;
			}

			overlayGroup.alpha = 1.0f;
		}

		private IEnumerator FadeButtonsIn() {
			float startTime = Time.time;

			while (Time.time - startTime < buttonsFadeDuration) {
				float progress = Mathf.Clamp01((Time.time - startTime) / buttonsFadeDuration);
				buttonsGroup.alpha = Mathf.Lerp(0.0f, 1.0f, EaseInOutSine(progress));
				yield return null;
			}

			buttonsGroup.alpha = 1.0f;
			buttonsGroup.interactable = true;
			buttonsGroup.blocksRaycasts = true;
		}

		private IEnumerator CountScore(int targetScore) {
			float startTime = Time.time;

			while (Time.time - startTime < scoreCountDuration) {
				float progress = Mathf.Clamp01((Time.time - startTime) / scoreCountDuration);
				float eased = EaseOutCubic(progress);
				int displayedScore = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, eased));

				scoreLabel.text = displayedScore + "%";

				yield return null;
			}

			scoreLabel.text = targetScore + "%";
		}

		private IEnumerator TypeTextSmooth(TextMeshProUGUI targetText, string fullText) {
			if (targetText == null) {
				yield break;
			}

			if (string.IsNullOrEmpty(fullText)) {
				targetText.text = "";
				yield break;
			}

			targetText.text = fullText;
			targetText.maxVisibleCharacters = 0;
			targetText.ForceMeshUpdate();

			int visibleCharacterCount = targetText.textInfo.characterCount;

			for (int i = 0; i <= visibleCharacterCount; i++) {
				targetText.maxVisibleCharacters = i;
				yield return new WaitForSeconds(typingSpeed);
			}
		}

		private void RestartGame() {
			SetOverlayVisible(false);

			DeckSwipe.Game game = FindFirstObjectByType<DeckSwipe.Game>();

			if (game != null) {
				game.RestartGame();
			}
			else {
				Debug.LogWarning("[FinalPreparednessOverlay] Could not find Game instance to restart.");
			}
		}

		private void QuitGame() {
			Application.Quit();

			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#endif
		}

		private void SetOverlayVisible(bool visible) {
			if (overlayRoot != null) {
				overlayRoot.gameObject.SetActive(visible);
			}
		}

		private static void SetTextAlpha(TextMeshProUGUI text, float alpha) {
			if (text == null) {
				return;
			}

			Color color = text.color;
			color.a = alpha;
			text.color = color;
		}

		private static float EaseOutCubic(float value) {
			value = Mathf.Clamp01(value);
			return 1.0f - Mathf.Pow(1.0f - value, 3.0f);
		}

		private static float EaseInOutSine(float value) {
			value = Mathf.Clamp01(value);
			return -(Mathf.Cos(Mathf.PI * value) - 1.0f) / 2.0f;
		}
	}
}