using DeckSwipe.CardModel;
using DeckSwipe.Gamestate;
using Outfrost;
using TMPro;
using UnityEngine;

namespace DeckSwipe.World {

	public class CardBehaviour : MonoBehaviour {

		private enum AnimationState {

			Idle,
			Converging,
			FlyingAway,
			Revealing

		}

		private const float _animationDuration = 0.4f;

		[Header("Swipe Dynamics")]
		public float swipeThreshold = 1.0f;
		public float dragTiltMultiplier = -5.0f;
		public bool lockVerticalMovement = true;

		[Header("Card Settings")]
		public Vector3 snapPosition;
		public Vector3 snapRotationAngles;

		public Vector2 cardImageSpriteTargetSize;

		[Tooltip("If true, ignores target size and uses the absolute 1x1 sprite scale without automatically fitting it in a bounding box.")]
		public bool forceNativeImageScale = false;

		[Tooltip("How far the card moves horizontally when the first arrow press shows the left or right decision without confirming it.")]
		public float keyboardTiltDistance = 0.75f;
		
		public TextMeshPro leftActionText;
		public TextMeshPro rightActionText;
		public SpriteRenderer cardBackSpriteRenderer;
		public SpriteRenderer cardFrontSpriteRenderer;
		public SpriteRenderer cardImageSpriteRenderer;

		private ICard card;
		private Vector3 dragStartPosition;
		private Vector3 dragStartPointerPosition;
		private Vector3 animationStartPosition;
		private Vector3 animationStartRotationAngles;
		private float animationStartTime;
		private AnimationState animationState = AnimationState.Idle;
		private bool animationSuspended;
		private bool isDragging;
		private int keyboardSelectionDirection;
		private bool keyboardSelectionPending;

		public ICard Card {
			get { return card; }
			set {
				card = value;
				leftActionText.text = card.LeftSwipeText;
				rightActionText.text = card.RightSwipeText;
				if (card.CardSprite != null) {
					if (forceNativeImageScale) {
						cardImageSpriteRenderer.transform.localScale = Vector3.one;
					}
					else {
						Vector2 targetSizeRatio = cardImageSpriteTargetSize / card.CardSprite.bounds.size;
						float scaleFactor = Mathf.Min(targetSizeRatio.x, targetSizeRatio.y);

						Vector3 scale = cardImageSpriteRenderer.transform.localScale;
						scale.x = scaleFactor;
						scale.y = scaleFactor;
						cardImageSpriteRenderer.transform.localScale = scale;
					}

					cardImageSpriteRenderer.sprite = card.CardSprite;
				}
			}
		}

		public Game Controller { private get; set; }

		private void Awake() {
			ShowVisibleSide();

			Util.SetTextAlpha(leftActionText, 0.0f);
			Util.SetTextAlpha(rightActionText, 0.0f);
		}

		private void Start() {
			// Rotate clockwise on reveal instead of anticlockwise
			snapRotationAngles.y += 360.0f;

			animationStartPosition = transform.position;
			animationStartRotationAngles = transform.eulerAngles;
			animationStartTime = Time.time;
			animationState = AnimationState.Revealing;

			card.CardShown(Controller);
		}

		private void Update() {
			// Animate card by interpolating translation and rotation, destroy swiped cards
			if (animationState != AnimationState.Idle && !animationSuspended) {
				float animationProgress = (Time.time - animationStartTime) / _animationDuration;
				float scaledProgress = ScaleProgress(animationProgress);
				if (scaledProgress > 1.0f || animationProgress > 1.0f) {
					transform.position = snapPosition;
					transform.eulerAngles = snapRotationAngles;

					if (animationState == AnimationState.Revealing) {
						CardDescriptionDisplay.SetDescription(card.CardText, card.CharacterName);
						snapRotationAngles.y -= 360.0f;
					}

					if (animationState == AnimationState.FlyingAway) {
						Destroy(gameObject);
					}
					else {
						animationState = AnimationState.Idle;
					}
				}
				else {
					transform.position = Vector3.Lerp(animationStartPosition, snapPosition, scaledProgress);
					transform.eulerAngles = Vector3.Lerp(animationStartRotationAngles, snapRotationAngles, scaledProgress);

					ShowVisibleSide();
				}
				if (animationState != AnimationState.Revealing && animationState != AnimationState.FlyingAway) {
					UpdateDecisionVisuals(transform.position.x - snapPosition.x);
				}
				else if (animationState == AnimationState.FlyingAway) {
					Stats.ShowIndicators(null, 0f);
				}
			}

			if (animationState == AnimationState.Idle && !isDragging) {
				HandleKeyboardInput();
			}
		}

		private void UpdateDecisionVisuals(float horizontalDisplacement) {
			float alphaCoord = horizontalDisplacement / (swipeThreshold / 2.0f);
			
			Util.SetTextAlpha(leftActionText, Mathf.Clamp01(-alphaCoord));
			Util.SetTextAlpha(rightActionText, Mathf.Clamp01(alphaCoord));

			if (card != null) {
				if (alphaCoord < -0.01f) {
					Stats.ShowIndicators(card.LeftSwipeOutcome?.StatsModification, Mathf.Clamp01(-alphaCoord));
				}
				else if (alphaCoord > 0.01f) {
					Stats.ShowIndicators(card.RightSwipeOutcome?.StatsModification, Mathf.Clamp01(alphaCoord));
				}
				else {
					Stats.ShowIndicators(null, 0f);
				}
			}
		}

		public void BeginDrag() {
			if (animationState != AnimationState.Idle) {
				return;
			}
			ResetKeyboardSelection();
			isDragging = true;
			animationSuspended = true;
			dragStartPosition = transform.position;
			dragStartPointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}

		public void Drag() {
			if (!isDragging) {
				return;
			}
			Vector3 displacement = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragStartPointerPosition;
			
			// Lock the Z and Y coordinates so the card can only move perfectly horizontally
			displacement.z = 0.0f;
			if (lockVerticalMovement) {
				displacement.y = 0.0f;
			}
			
			transform.position = dragStartPosition + displacement;

			// Add rotation (arc) based on how far the card is dragged horizontally from the snap position
			float horizontalDisplacement = transform.position.x - snapPosition.x;
			float rotationAngle = horizontalDisplacement * dragTiltMultiplier;
			transform.localEulerAngles = new Vector3(snapRotationAngles.x, snapRotationAngles.y, rotationAngle);

			UpdateDecisionVisuals(horizontalDisplacement);
		}

		private void HandleKeyboardInput() {
			if (animationState != AnimationState.Idle || isDragging) {
				return;
			}

			if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				if (keyboardSelectionPending && keyboardSelectionDirection == -1) {
					ConfirmKeyboardSelection(true);
				}
				else {
					SetKeyboardSelection(-1);
				}
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow)) {
				if (keyboardSelectionPending && keyboardSelectionDirection == 1) {
					ConfirmKeyboardSelection(false);
				}
				else {
					SetKeyboardSelection(1);
				}
			}
		}

		private void SetKeyboardSelection(int direction) {
			keyboardSelectionDirection = direction;
			keyboardSelectionPending = true;

			float horizontalDisplacement = direction * keyboardTiltDistance;
			transform.position = new Vector3(snapPosition.x + horizontalDisplacement, snapPosition.y, transform.position.z);
			float rotationAngle = horizontalDisplacement * dragTiltMultiplier;
			transform.localEulerAngles = new Vector3(snapRotationAngles.x, snapRotationAngles.y, rotationAngle);
			UpdateDecisionVisuals(horizontalDisplacement);
		}

		private void ConfirmKeyboardSelection(bool chooseLeft) {
			keyboardSelectionPending = false;
			animationStartPosition = transform.position;
			animationStartRotationAngles = transform.eulerAngles;
			animationStartTime = Time.time;

			Controller.PerformDecision(card, chooseLeft);

			Vector3 displacement = animationStartPosition - snapPosition;
			snapPosition += displacement.normalized
			                * Util.OrthoCameraWorldDiagonalSize(Camera.main)
			                * 2.0f;
			snapRotationAngles = animationStartRotationAngles;
			animationState = AnimationState.FlyingAway;
			CardDescriptionDisplay.ResetDescription();
		}

		private void ResetKeyboardSelection() {
			if (!keyboardSelectionPending) {
				return;
			}
			keyboardSelectionPending = false;
			keyboardSelectionDirection = 0;
			transform.position = snapPosition;
			transform.localEulerAngles = new Vector3(snapRotationAngles.x, snapRotationAngles.y, snapRotationAngles.z);
			UpdateDecisionVisuals(0f);
		}

		public void EndDrag() {
			if (!isDragging) {
				return;
			}
			isDragging = false;
			animationStartPosition = transform.position;
			animationStartRotationAngles = transform.eulerAngles;
			
			// Fix the spinning issue when returning to the snap position by finding the shortest rotation path
			animationStartRotationAngles.z = snapRotationAngles.z + Mathf.DeltaAngle(snapRotationAngles.z, animationStartRotationAngles.z);
			
			animationStartTime = Time.time;
			if (animationState != AnimationState.FlyingAway) {
				if (transform.position.x < snapPosition.x - swipeThreshold) {
					Controller.PerformDecision(card, true);
					Vector3 displacement = animationStartPosition - snapPosition;
					snapPosition += displacement.normalized
					                * Util.OrthoCameraWorldDiagonalSize(Camera.main)
					                * 2.0f;
					snapRotationAngles = animationStartRotationAngles;
					animationState = AnimationState.FlyingAway;
					CardDescriptionDisplay.ResetDescription();
				}
				else if (transform.position.x > snapPosition.x + swipeThreshold) {
					Controller.PerformDecision(card, false);
					Vector3 displacement = animationStartPosition - snapPosition;
					snapPosition += displacement.normalized
					                * Util.OrthoCameraWorldDiagonalSize(Camera.main)
					                * 2.0f;
					snapRotationAngles = animationStartRotationAngles;
					animationState = AnimationState.FlyingAway;
					CardDescriptionDisplay.ResetDescription();
				}
				else if (animationState == AnimationState.Idle) {
					animationState = AnimationState.Converging;
				}
			}
			animationSuspended = false;
		}

		private void ShowVisibleSide() {
			// Display correct card elements based on whether it's facing the main camera
			bool isFacingCamera = Util.IsFacingCamera(gameObject);
			cardBackSpriteRenderer.enabled = !isFacingCamera;
			cardFrontSpriteRenderer.enabled = isFacingCamera;
			cardImageSpriteRenderer.enabled = isFacingCamera;
			leftActionText.enabled = isFacingCamera;
			rightActionText.enabled = isFacingCamera;
		}

		private float ScaleProgress(float animationProgress) {
			switch (animationState) {
				case AnimationState.Converging:
					return 0.15f * Mathf.Pow(animationProgress, 3.0f)
					       - 1.5f * Mathf.Pow(animationProgress, 2.0f)
					       + 2.38f * animationProgress;
				case AnimationState.FlyingAway:
					return 1.5f * Mathf.Pow(animationProgress, 3.0f)
					       + 0.55f * animationProgress;
				default:
					return animationProgress;
			}
		}

	}

}
