using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {

	public class PlanCSettingsOverlay : MonoBehaviour {

		private static PlanCSettingsOverlay instance;

		[Header("Layout")]
		public Vector2 referenceResolution = new Vector2(402.0f, 874.0f);

		private Canvas canvas;
		private CanvasGroup canvasGroup;
		private RectTransform root;

		private Slider masterSlider;
		private Slider musicSlider;
		private Slider sfxSlider;

		private TextMeshProUGUI masterValueText;
		private TextMeshProUGUI musicValueText;
		private TextMeshProUGUI sfxValueText;

		private Sprite roundedPanelSprite;
		private Sprite roundedButtonSprite;
		private Sprite roundedTrackSprite;
		private Sprite circleHandleSprite;

		private void Awake() {
			if (instance != null && instance != this) {
				Destroy(gameObject);
				return;
			}

			instance = this;

			CreateSprites();
			BuildOverlay();
			HideInternal();
		}

		public static void Show() {
			if (instance == null) {
				Debug.LogWarning("[PlanCSettingsOverlay] No PlanCSettingsOverlay found in scene.");
				return;
			}

			instance.ShowInternal();
		}

		public static void Hide() {
			if (instance == null) {
				return;
			}

			instance.HideInternal();
		}

		public void OpenSettings() {
			ShowInternal();
		}

		public void CloseSettings() {
			HideInternal();
		}

		private void ShowInternal() {
			if (root == null || canvasGroup == null) {
				return;
			}

			RefreshValues();

			root.gameObject.SetActive(true);
			canvasGroup.alpha = 1.0f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;

			transform.SetAsLastSibling();
		}

		private void HideInternal() {
			if (root == null || canvasGroup == null) {
				return;
			}

			canvasGroup.alpha = 0.0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			root.gameObject.SetActive(false);
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
			scaler.referenceResolution = referenceResolution;
			scaler.matchWidthOrHeight = 0.5f;

			if (GetComponent<GraphicRaycaster>() == null) {
				gameObject.AddComponent<GraphicRaycaster>();
			}

			GameObject rootObject = new GameObject("Settings Overlay");
			rootObject.transform.SetParent(transform, false);

			root = rootObject.AddComponent<RectTransform>();
			root.anchorMin = Vector2.zero;
			root.anchorMax = Vector2.one;
			root.offsetMin = Vector2.zero;
			root.offsetMax = Vector2.zero;

			canvasGroup = rootObject.AddComponent<CanvasGroup>();

			CreateDimBackground(root);
			CreatePanel(root);
		}

		private void CreateDimBackground(RectTransform parent) {
			GameObject backgroundObject = new GameObject("Dim Background");
			backgroundObject.transform.SetParent(parent, false);

			RectTransform rect = backgroundObject.AddComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			Image image = backgroundObject.AddComponent<Image>();
			image.color = new Color(0.0f, 0.0f, 0.0f, 0.68f);
			image.raycastTarget = true;
		}

		private void CreatePanel(RectTransform parent) {
			GameObject panelObject = new GameObject("Settings Panel");
			panelObject.transform.SetParent(parent, false);

			RectTransform panelRect = panelObject.AddComponent<RectTransform>();
			panelRect.anchorMin = new Vector2(0.5f, 0.5f);
			panelRect.anchorMax = new Vector2(0.5f, 0.5f);
			panelRect.pivot = new Vector2(0.5f, 0.5f);
			panelRect.anchoredPosition = Vector2.zero;
			panelRect.sizeDelta = new Vector2(355.0f, 410.0f);

			Image panelImage = panelObject.AddComponent<Image>();
			panelImage.sprite = roundedPanelSprite;
			panelImage.type = Image.Type.Sliced;
			panelImage.color = new Color(0.12f, 0.095f, 0.065f, 0.97f);
			panelImage.raycastTarget = true;

			CreateText(
				panelRect,
				"Title",
				"Settings",
				30,
				new Vector2(0.0f, 155.0f),
				new Vector2(290.0f, 44.0f),
				Color.white
			);

			masterSlider = CreateVolumeRow(
				panelRect,
				"Master Volume",
				new Vector2(0.0f, 78.0f),
				out masterValueText
			);

			musicSlider = CreateVolumeRow(
				panelRect,
				"Music Volume",
				new Vector2(0.0f, 8.0f),
				out musicValueText
			);

			sfxSlider = CreateVolumeRow(
				panelRect,
				"SFX Volume",
				new Vector2(0.0f, -62.0f),
				out sfxValueText
			);

			CreateCloseButton(panelRect, new Vector2(0.0f, -150.0f));

			masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
			musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
			sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
		}

		private Slider CreateVolumeRow(
			RectTransform parent,
			string label,
			Vector2 anchoredPosition,
			out TextMeshProUGUI valueText
		) {
			GameObject rowObject = new GameObject(label + " Row");
			rowObject.transform.SetParent(parent, false);

			RectTransform rowRect = rowObject.AddComponent<RectTransform>();
			rowRect.anchorMin = new Vector2(0.5f, 0.5f);
			rowRect.anchorMax = new Vector2(0.5f, 0.5f);
			rowRect.pivot = new Vector2(0.5f, 0.5f);
			rowRect.anchoredPosition = anchoredPosition;
			rowRect.sizeDelta = new Vector2(292.0f, 58.0f);

			CreateText(
				rowRect,
				label + " Label",
				label,
				15,
				new Vector2(-62.0f, 17.0f),
				new Vector2(180.0f, 24.0f),
				Color.white,
				TextAlignmentOptions.Left
			);

			valueText = CreateText(
				rowRect,
				label + " Value",
				"100%",
				14,
				new Vector2(108.0f, 17.0f),
				new Vector2(72.0f, 24.0f),
				new Color(0.86f, 0.84f, 0.78f, 1.0f),
				TextAlignmentOptions.Right
			);

			return CreateSlider(rowRect, new Vector2(0.0f, -13.0f));
		}

		private Slider CreateSlider(RectTransform parent, Vector2 anchoredPosition) {
			GameObject sliderObject = new GameObject("Slider");
			sliderObject.transform.SetParent(parent, false);

			RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
			sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
			sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
			sliderRect.pivot = new Vector2(0.5f, 0.5f);
			sliderRect.anchoredPosition = anchoredPosition;
			sliderRect.sizeDelta = new Vector2(270.0f, 28.0f);

			Slider slider = sliderObject.AddComponent<Slider>();
			slider.minValue = 0.0f;
			slider.maxValue = 1.0f;
			slider.wholeNumbers = false;
			slider.direction = Slider.Direction.LeftToRight;

			GameObject backgroundObject = new GameObject("Background");
			backgroundObject.transform.SetParent(sliderObject.transform, false);

			RectTransform backgroundRect = backgroundObject.AddComponent<RectTransform>();
			backgroundRect.anchorMin = new Vector2(0.0f, 0.5f);
			backgroundRect.anchorMax = new Vector2(1.0f, 0.5f);
			backgroundRect.pivot = new Vector2(0.5f, 0.5f);
			backgroundRect.anchoredPosition = Vector2.zero;
			backgroundRect.sizeDelta = new Vector2(0.0f, 6.0f);

			Image backgroundImage = backgroundObject.AddComponent<Image>();
			backgroundImage.sprite = roundedTrackSprite;
			backgroundImage.type = Image.Type.Sliced;
			backgroundImage.color = new Color(0.28f, 0.255f, 0.215f, 1.0f);
			backgroundImage.raycastTarget = false;

			GameObject fillAreaObject = new GameObject("Fill Area");
			fillAreaObject.transform.SetParent(sliderObject.transform, false);

			RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
			fillAreaRect.anchorMin = new Vector2(0.0f, 0.5f);
			fillAreaRect.anchorMax = new Vector2(1.0f, 0.5f);
			fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
			fillAreaRect.anchoredPosition = Vector2.zero;
			fillAreaRect.sizeDelta = new Vector2(-18.0f, 6.0f);

			GameObject fillObject = new GameObject("Fill");
			fillObject.transform.SetParent(fillAreaObject.transform, false);

			RectTransform fillRect = fillObject.AddComponent<RectTransform>();
			fillRect.anchorMin = Vector2.zero;
			fillRect.anchorMax = Vector2.one;
			fillRect.offsetMin = Vector2.zero;
			fillRect.offsetMax = Vector2.zero;

			Image fillImage = fillObject.AddComponent<Image>();
			fillImage.sprite = roundedTrackSprite;
			fillImage.type = Image.Type.Sliced;
			fillImage.color = new Color(0.96f, 0.76f, 0.34f, 1.0f);
			fillImage.raycastTarget = false;

			GameObject handleAreaObject = new GameObject("Handle Slide Area");
			handleAreaObject.transform.SetParent(sliderObject.transform, false);

			RectTransform handleAreaRect = handleAreaObject.AddComponent<RectTransform>();
			handleAreaRect.anchorMin = Vector2.zero;
			handleAreaRect.anchorMax = Vector2.one;
			handleAreaRect.offsetMin = new Vector2(9.0f, 0.0f);
			handleAreaRect.offsetMax = new Vector2(-9.0f, 0.0f);

			GameObject handleObject = new GameObject("Handle");
			handleObject.transform.SetParent(handleAreaObject.transform, false);

			RectTransform handleRect = handleObject.AddComponent<RectTransform>();

			// IMPORTANT:
			// This forces the slider controller to stay as a perfect circle.
			handleRect.anchorMin = new Vector2(0.5f, 0.5f);
			handleRect.anchorMax = new Vector2(0.5f, 0.5f);
			handleRect.pivot = new Vector2(0.5f, 0.5f);
			handleRect.anchoredPosition = Vector2.zero;
			handleRect.sizeDelta = new Vector2(18.0f, 18.0f);

			Image handleImage = handleObject.AddComponent<Image>();
			handleImage.sprite = circleHandleSprite;
			handleImage.type = Image.Type.Simple;
			handleImage.preserveAspect = true;
			handleImage.color = new Color(1.0f, 0.96f, 0.86f, 1.0f);
			handleImage.raycastTarget = true;

			slider.targetGraphic = handleImage;
			slider.fillRect = fillRect;
			slider.handleRect = handleRect;

			return slider;
		}

		private void CreateCloseButton(RectTransform parent, Vector2 anchoredPosition) {
			GameObject buttonObject = new GameObject("Close Button");
			buttonObject.transform.SetParent(parent, false);

			RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
			buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
			buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
			buttonRect.pivot = new Vector2(0.5f, 0.5f);
			buttonRect.anchoredPosition = anchoredPosition;
			buttonRect.sizeDelta = new Vector2(190.0f, 46.0f);

			Image image = buttonObject.AddComponent<Image>();
			image.sprite = roundedButtonSprite;
			image.type = Image.Type.Sliced;
			image.color = new Color(0.88f, 0.78f, 0.62f, 1.0f);

			Button button = buttonObject.AddComponent<Button>();
			button.targetGraphic = image;
			button.onClick.AddListener(() => {
				PlanCSoundEffects.PlayButtonClick();
				CloseSettings();
			});

			CreateText(
				buttonRect,
				"Close Text",
				"Close",
				18,
				Vector2.zero,
				new Vector2(190.0f, 46.0f),
				new Color(0.11f, 0.09f, 0.06f, 1.0f)
			);
		}

		private TextMeshProUGUI CreateText(
			RectTransform parent,
			string objectName,
			string text,
			float fontSize,
			Vector2 anchoredPosition,
			Vector2 size,
			Color color,
			TextAlignmentOptions alignment = TextAlignmentOptions.Center
		) {
			GameObject textObject = new GameObject(objectName);
			textObject.transform.SetParent(parent, false);

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
			textComponent.alignment = alignment;
			textComponent.color = color;
			textComponent.raycastTarget = false;
			textComponent.enableAutoSizing = false;

			return textComponent;
		}

		private void RefreshValues() {
			if (masterSlider == null || musicSlider == null || sfxSlider == null) {
				return;
			}

			masterSlider.SetValueWithoutNotify(PlanCSoundEffects.GetMasterVolume());
			musicSlider.SetValueWithoutNotify(PlanCSoundEffects.GetMusicVolume());
			sfxSlider.SetValueWithoutNotify(PlanCSoundEffects.GetSfxVolume());

			RefreshValueLabels();
		}

		private void OnMasterVolumeChanged(float value) {
			PlanCSoundEffects.SetMasterVolume(value);
			RefreshValueLabels();
		}

		private void OnMusicVolumeChanged(float value) {
			PlanCSoundEffects.SetMusicVolume(value);
			RefreshValueLabels();
		}

		private void OnSfxVolumeChanged(float value) {
			PlanCSoundEffects.SetSfxVolume(value);
			RefreshValueLabels();
		}

		private void RefreshValueLabels() {
			if (masterValueText != null && masterSlider != null) {
				masterValueText.text = Mathf.RoundToInt(masterSlider.value * 100.0f) + "%";
			}

			if (musicValueText != null && musicSlider != null) {
				musicValueText.text = Mathf.RoundToInt(musicSlider.value * 100.0f) + "%";
			}

			if (sfxValueText != null && sfxSlider != null) {
				sfxValueText.text = Mathf.RoundToInt(sfxSlider.value * 100.0f) + "%";
			}
		}

		private void CreateSprites() {
			roundedPanelSprite = CreateRoundedSprite(96, 96, 18);
			roundedButtonSprite = CreateRoundedSprite(96, 96, 24);
			roundedTrackSprite = CreateRoundedSprite(64, 64, 16);

			// Real circular sprite for the slider controller.
			circleHandleSprite = CreateCircleSprite(128);
		}

		private Sprite CreateCircleSprite(int size) {
			Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
			texture.filterMode = FilterMode.Bilinear;
			texture.wrapMode = TextureWrapMode.Clamp;

			float center = (size - 1) * 0.5f;
			float radius = (size * 0.5f) - 2.0f;

			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					float dx = x - center;
					float dy = y - center;
					float distance = Mathf.Sqrt(dx * dx + dy * dy);

					float alpha = Mathf.Clamp01(radius - distance + 1.0f);

					texture.SetPixel(
						x,
						y,
						new Color(1.0f, 1.0f, 1.0f, alpha)
					);
				}
			}

			texture.Apply();

			return Sprite.Create(
				texture,
				new Rect(0.0f, 0.0f, size, size),
				new Vector2(0.5f, 0.5f),
				100.0f
			);
		}

		private Sprite CreateRoundedSprite(int width, int height, int radius) {
			Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			texture.filterMode = FilterMode.Bilinear;
			texture.wrapMode = TextureWrapMode.Clamp;

			Color clear = new Color(1.0f, 1.0f, 1.0f, 0.0f);
			Color solid = Color.white;

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					bool inside = IsInsideRoundedRect(x, y, width, height, radius);
					texture.SetPixel(x, y, inside ? solid : clear);
				}
			}

			texture.Apply();

			Vector4 border = new Vector4(radius, radius, radius, radius);

			return Sprite.Create(
				texture,
				new Rect(0.0f, 0.0f, width, height),
				new Vector2(0.5f, 0.5f),
				100.0f,
				0,
				SpriteMeshType.FullRect,
				border
			);
		}

		private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius) {
			int left = radius;
			int right = width - radius - 1;
			int bottom = radius;
			int top = height - radius - 1;

			if (x >= left && x <= right) {
				return true;
			}

			if (y >= bottom && y <= top) {
				return true;
			}

			int cornerX = x < left ? left : right;
			int cornerY = y < bottom ? bottom : top;

			int dx = x - cornerX;
			int dy = y - cornerY;

			return dx * dx + dy * dy <= radius * radius;
		}
	}
}