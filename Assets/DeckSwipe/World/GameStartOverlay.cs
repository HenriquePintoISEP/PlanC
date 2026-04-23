using System.Collections;
using System.Collections.Generic;
using Outfrost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {

	public class GameStartOverlay : MonoBehaviour {

		private static readonly List<GameStartOverlay> _controlListeners = new List<GameStartOverlay>();

		public static Callback FadeOutCallback { private get; set; }

		[Header("Splash Timing")]
		public float titleIntroDuration = 0.75f;
		public float dividerDuration = 0.45f;
		public float holdBeforeTyping = 0.25f;
		public float typingSpeed = 0.022f;
		public float holdAfterTyping = 1.45f;
		public float fadeOutDuration = 0.7f;

		[Header("Splash Text")]
		public string title = "Plan C";
		public string description =
			"A game about disaster preparedness.\nBecause plans A and B failed.";

		private Canvas canvas;
		private CanvasGroup splashGroup;
		private RectTransform splashRoot;

		private TextMeshProUGUI titleText;
		private TextMeshProUGUI descriptionText;
		private Image dividerLine;

		private Coroutine splashCoroutine;
		private Coroutine titleFloatCoroutine;

		private void Awake() {
			if (!Util.IsPrefab(gameObject)) {
				_controlListeners.Add(this);
			}

			BuildSplashScreen();
		}

		private void OnDestroy() {
			_controlListeners.Remove(this);

			if (splashCoroutine != null) {
				StopCoroutine(splashCoroutine);
			}

			if (titleFloatCoroutine != null) {
				StopCoroutine(titleFloatCoroutine);
			}
		}

		private void Start() {
			SetSplashVisible(false);
		}

		public static void StartSequence(float daysPassed, float daysLastRun) {
			for (int i = _controlListeners.Count - 1; i >= 0; i--) {
				if (_controlListeners[i] == null) {
					_controlListeners.RemoveAt(i);
				}
				else {
					_controlListeners[i].PlaySplash();
				}
			}
		}

		private void BuildSplashScreen() {
			canvas = GetComponent<Canvas>();

			if (canvas == null) {
				canvas = gameObject.AddComponent<Canvas>();
			}

			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 9999;

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

			GameObject rootObject = new GameObject("Runtime Splash Screen");
			rootObject.transform.SetParent(transform, false);
			rootObject.transform.SetAsLastSibling();

			splashRoot = rootObject.AddComponent<RectTransform>();
			splashRoot.anchorMin = Vector2.zero;
			splashRoot.anchorMax = Vector2.one;
			splashRoot.offsetMin = Vector2.zero;
			splashRoot.offsetMax = Vector2.zero;

			splashGroup = rootObject.AddComponent<CanvasGroup>();
			splashGroup.alpha = 0.0f;
			splashGroup.interactable = false;
			splashGroup.blocksRaycasts = false;

			CreateBackground(splashRoot);

			titleText = CreateText(
				"Plan C Title",
				title,
				64,
				new Vector2(0.0f, 92.0f),
				new Vector2(360.0f, 82.0f),
				Color.white
			);

			dividerLine = CreateDividerLine(
				"Divider Line",
				new Vector2(0.0f, 38.0f),
				new Vector2(0.0f, 3.0f),
				new Color(0.95f, 0.95f, 0.95f, 1.0f)
			);

			descriptionText = CreateText(
				"Description Text",
				"",
				21,
				new Vector2(0.0f, -48.0f),
				new Vector2(360.0f, 115.0f),
				new Color(0.86f, 0.86f, 0.86f, 1.0f)
			);

			descriptionText.lineSpacing = 18.0f;
			descriptionText.enableAutoSizing = false;
			descriptionText.overflowMode = TextOverflowModes.Overflow;

			SetSplashVisible(false);
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
			image.raycastTarget = false;
		}

		private Image CreateDividerLine(
			string objectName,
			Vector2 anchoredPosition,
			Vector2 size,
			Color color
		) {
			GameObject lineObject = new GameObject(objectName);
			lineObject.transform.SetParent(splashRoot, false);
			lineObject.transform.SetAsLastSibling();

			RectTransform rect = lineObject.AddComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			Image image = lineObject.AddComponent<Image>();
			image.color = color;
			image.raycastTarget = false;

			return image;
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
			textObject.transform.SetParent(splashRoot, false);
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
			textComponent.overflowMode = TextOverflowModes.Overflow;

			return textComponent;
		}

		private void PlaySplash() {
			if (splashCoroutine != null) {
				StopCoroutine(splashCoroutine);
			}

			if (titleFloatCoroutine != null) {
				StopCoroutine(titleFloatCoroutine);
				titleFloatCoroutine = null;
			}

			splashCoroutine = StartCoroutine(PlaySplashCoroutine());
		}

		private IEnumerator PlaySplashCoroutine() {
			SetSplashVisible(true);
			splashRoot.SetAsLastSibling();

			splashGroup.alpha = 1.0f;

			titleText.text = title;
			descriptionText.text = "";

			SetTextAlpha(titleText, 0.0f);
			SetTextAlpha(descriptionText, 1.0f);
			SetImageAlpha(dividerLine, 1.0f);

			titleText.transform.localScale = Vector3.one * 0.72f;
			dividerLine.rectTransform.sizeDelta = new Vector2(0.0f, 3.0f);

			yield return AnimateTitleIntro();
			yield return AnimateDividerIn();

			titleFloatCoroutine = StartCoroutine(AnimateTitleFloat());

			yield return new WaitForSeconds(holdBeforeTyping);

			yield return TypeTextSmooth(descriptionText, description);

			yield return new WaitForSeconds(holdAfterTyping);

			yield return FadeSplashOut();

			if (titleFloatCoroutine != null) {
				StopCoroutine(titleFloatCoroutine);
				titleFloatCoroutine = null;
			}

			SetSplashVisible(false);
			FadeOutCallback?.Invoke();

			splashCoroutine = null;
		}

		private IEnumerator AnimateTitleIntro() {
			float startTime = Time.time;

			while (Time.time - startTime < titleIntroDuration) {
				float progress = Mathf.Clamp01((Time.time - startTime) / titleIntroDuration);
				float easedAlpha = EaseOutCubic(progress);
				float easedScale = EaseOutBackSoft(progress);

				SetTextAlpha(titleText, easedAlpha);
				titleText.transform.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.0f, easedScale);

				yield return null;
			}

			SetTextAlpha(titleText, 1.0f);
			titleText.transform.localScale = Vector3.one;
		}

		private IEnumerator AnimateDividerIn() {
			float startTime = Time.time;

			while (Time.time - startTime < dividerDuration) {
				float progress = Mathf.Clamp01((Time.time - startTime) / dividerDuration);
				float eased = EaseInOutSine(progress);

				dividerLine.rectTransform.sizeDelta = new Vector2(
					Mathf.Lerp(0.0f, 120.0f, eased),
					3.0f
				);

				yield return null;
			}

			dividerLine.rectTransform.sizeDelta = new Vector2(120.0f, 3.0f);
		}

		private IEnumerator AnimateTitleFloat() {
			Vector3 baseScale = Vector3.one;

			while (true) {
				float t = Time.time;
				float scale = 1.0f + Mathf.Sin(t * 2.2f) * 0.012f;
				titleText.transform.localScale = baseScale * scale;
				yield return null;
			}
		}

		private IEnumerator TypeTextSmooth(TextMeshProUGUI targetText, string fullText) {
			if (targetText == null) {
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

		private IEnumerator FadeSplashOut() {
			float startTime = Time.time;

			while (Time.time - startTime < fadeOutDuration) {
				float progress = Mathf.Clamp01((Time.time - startTime) / fadeOutDuration);
				splashGroup.alpha = Mathf.Lerp(1.0f, 0.0f, EaseInOutSine(progress));
				yield return null;
			}

			splashGroup.alpha = 0.0f;
		}

		private void SetSplashVisible(bool visible) {
			if (splashRoot != null) {
				splashRoot.gameObject.SetActive(visible);
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

		private static void SetImageAlpha(Image image, float alpha) {
			if (image == null) {
				return;
			}

			Color color = image.color;
			color.a = alpha;
			image.color = color;
		}

		private static float EaseOutCubic(float value) {
			value = Mathf.Clamp01(value);
			return 1.0f - Mathf.Pow(1.0f - value, 3.0f);
		}

		private static float EaseOutBackSoft(float value) {
			value = Mathf.Clamp01(value);

			const float c1 = 1.05f;
			const float c3 = c1 + 1.0f;

			return 1.0f + c3 * Mathf.Pow(value - 1.0f, 3.0f)
				+ c1 * Mathf.Pow(value - 1.0f, 2.0f);
		}

		private static float EaseInOutSine(float value) {
			value = Mathf.Clamp01(value);
			return -(Mathf.Cos(Mathf.PI * value) - 1.0f) / 2.0f;
		}
	}
}