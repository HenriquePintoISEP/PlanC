using System.Collections;
using UnityEngine;

namespace DeckSwipe.World {

	public class PlanCSoundEffects : MonoBehaviour {

		private static PlanCSoundEffects instance;

		[Header("SFX Audio Source")]
		public AudioSource audioSource;

		[Header("Background Music")]
		public AudioSource musicSource;
		public AudioClip backgroundMusicClip;
		public bool playMusicOnStart = true;
		public bool fadeMusicIn = true;
		public float musicFadeInDuration = 1.25f;

		[Header("Volume")]
		[Range(0.0f, 1.0f)] public float masterVolume = 0.65f;
		[Range(0.0f, 1.0f)] public float musicVolume = 0.35f;
		[Range(0.0f, 1.0f)] public float sfxVolume = 1.0f;

		[Range(0.0f, 1.0f)] public float swipeVolume = 0.55f;
		[Range(0.0f, 1.0f)] public float cardVolume = 0.35f;
		[Range(0.0f, 1.0f)] public float itemVolume = 0.65f;
		[Range(0.0f, 1.0f)] public float warningVolume = 0.75f;
		[Range(0.0f, 1.0f)] public float buttonVolume = 0.45f;

		[Header("Optional Custom SFX Clips")]
		public AudioClip swipeLeftClip;
		public AudioClip swipeRightClip;
		public AudioClip cardAppearClip;
		public AudioClip itemCollectedClip;
		public AudioClip warningClip;
		public AudioClip buttonClickClip;

		private AudioClip generatedSwipeLeft;
		private AudioClip generatedSwipeRight;
		private AudioClip generatedCardAppear;
		private AudioClip generatedItemCollected;
		private AudioClip generatedWarning;
		private AudioClip generatedButtonClick;

		private Coroutine musicFadeCoroutine;

		private const int SampleRate = 44100;

		private void Awake() {
			if (instance != null && instance != this) {
				Destroy(gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);

			SetupSfxSource();
			SetupMusicSource();

			BuildGeneratedClips();
		}

		private void Start() {
			if (playMusicOnStart) {
				PlayBackgroundMusic();
			}
		}

		private void SetupSfxSource() {
			if (audioSource == null) {
				audioSource = GetComponent<AudioSource>();
			}

			if (audioSource == null) {
				audioSource = gameObject.AddComponent<AudioSource>();
			}

			audioSource.playOnAwake = false;
			audioSource.loop = false;
			audioSource.spatialBlend = 0.0f;
		}

		private void SetupMusicSource() {
			if (musicSource == null) {
				GameObject musicObject = new GameObject("PlanC Background Music Source");
				musicObject.transform.SetParent(transform, false);
				musicSource = musicObject.AddComponent<AudioSource>();
			}

			musicSource.playOnAwake = false;
			musicSource.loop = true;
			musicSource.spatialBlend = 0.0f;
			musicSource.volume = 0.0f;
		}

		private void BuildGeneratedClips() {
			generatedSwipeLeft = CreateSweepClip(
				"Generated Swipe Left",
				0.10f,
				520.0f,
				260.0f,
				0.18f
			);

			generatedSwipeRight = CreateSweepClip(
				"Generated Swipe Right",
				0.10f,
				300.0f,
				620.0f,
				0.18f
			);

			generatedCardAppear = CreatePopClip(
				"Generated Card Appear",
				0.08f,
				420.0f,
				0.10f
			);

			generatedItemCollected = CreateDoubleToneClip(
				"Generated Item Collected",
				0.18f,
				520.0f,
				820.0f,
				0.16f
			);

			generatedWarning = CreateDoubleToneClip(
				"Generated Warning",
				0.26f,
				220.0f,
				180.0f,
				0.22f
			);

			generatedButtonClick = CreateClickClip(
				"Generated Button Click",
				0.045f,
				720.0f,
				0.13f
			);
		}

		public static float GetMasterVolume() {
			return instance != null ? instance.masterVolume : 1.0f;
		}

		public static float GetMusicVolume() {
			return instance != null ? instance.musicVolume : 1.0f;
		}

		public static float GetSfxVolume() {
			return instance != null ? instance.sfxVolume : 1.0f;
		}

		public static void SetMasterVolume(float volume) {
			if (instance == null) {
				return;
			}

			instance.masterVolume = Mathf.Clamp01(volume);
			instance.RefreshMusicVolume();
		}

		public static void SetMusicVolume(float volume) {
			if (instance == null) {
				return;
			}

			instance.musicVolume = Mathf.Clamp01(volume);
			instance.RefreshMusicVolume();
		}

		public static void SetSfxVolume(float volume) {
			if (instance == null) {
				return;
			}

			instance.sfxVolume = Mathf.Clamp01(volume);
		}

		public static void PlayMusic() {
			if (instance == null) {
				return;
			}

			instance.PlayBackgroundMusic();
		}

		public static void StopMusic() {
			if (instance == null) {
				return;
			}

			instance.StopBackgroundMusic();
		}

		private void PlayBackgroundMusic() {
			if (musicSource == null || backgroundMusicClip == null) {
				return;
			}

			if (musicSource.isPlaying && musicSource.clip == backgroundMusicClip) {
				return;
			}

			if (musicFadeCoroutine != null) {
				StopCoroutine(musicFadeCoroutine);
				musicFadeCoroutine = null;
			}

			musicSource.clip = backgroundMusicClip;
			musicSource.loop = true;

			if (fadeMusicIn) {
				musicSource.volume = 0.0f;
				musicSource.Play();

				musicFadeCoroutine = StartCoroutine(FadeMusicVolume(
					0.0f,
					musicVolume * masterVolume,
					musicFadeInDuration,
					false
				));
			}
			else {
				musicSource.volume = musicVolume * masterVolume;
				musicSource.Play();
			}
		}

		private void StopBackgroundMusic() {
			if (musicSource == null || !musicSource.isPlaying) {
				return;
			}

			if (musicFadeCoroutine != null) {
				StopCoroutine(musicFadeCoroutine);
				musicFadeCoroutine = null;
			}

			musicFadeCoroutine = StartCoroutine(FadeMusicVolume(
				musicSource.volume,
				0.0f,
				0.4f,
				true
			));
		}

		private void RefreshMusicVolume() {
			if (musicSource == null) {
				return;
			}

			musicSource.volume = Mathf.Clamp01(musicVolume * masterVolume);
		}

		private IEnumerator FadeMusicVolume(float from, float to, float duration, bool stopWhenDone) {
			if (musicSource == null) {
				yield break;
			}

			float startTime = Time.time;

			while (Time.time - startTime < duration) {
				float progress = Mathf.Clamp01((Time.time - startTime) / duration);
				float eased = EaseOutCubic(progress);
				musicSource.volume = Mathf.Lerp(from, to, eased);
				yield return null;
			}

			musicSource.volume = to;

			if (stopWhenDone) {
				musicSource.Stop();
			}

			musicFadeCoroutine = null;
		}

		public static void PlaySwipe(bool leftSwipe) {
			if (instance == null) {
				return;
			}

			AudioClip clip = leftSwipe
				? instance.swipeLeftClip ?? instance.generatedSwipeLeft
				: instance.swipeRightClip ?? instance.generatedSwipeRight;

			instance.PlayClip(clip, instance.swipeVolume);
		}

		public static void PlayCardAppear() {
			if (instance == null) {
				return;
			}

			AudioClip clip = instance.cardAppearClip ?? instance.generatedCardAppear;
			instance.PlayClip(clip, instance.cardVolume);
		}

		public static void PlayItemCollected() {
			if (instance == null) {
				return;
			}

			AudioClip clip = instance.itemCollectedClip ?? instance.generatedItemCollected;
			instance.PlayClip(clip, instance.itemVolume);
		}

		public static void PlayWarning() {
			if (instance == null) {
				return;
			}

			AudioClip clip = instance.warningClip ?? instance.generatedWarning;
			instance.PlayClip(clip, instance.warningVolume);
		}

		public static void PlayButtonClick() {
			if (instance == null) {
				return;
			}

			AudioClip clip = instance.buttonClickClip ?? instance.generatedButtonClick;
			instance.PlayClip(clip, instance.buttonVolume);
		}

		private void PlayClip(AudioClip clip, float volume) {
			if (audioSource == null || clip == null) {
				return;
			}

			audioSource.pitch = Random.Range(0.96f, 1.04f);
			audioSource.PlayOneShot(clip, Mathf.Clamp01(volume * sfxVolume * masterVolume));
		}

		private static AudioClip CreateSweepClip(
			string clipName,
			float duration,
			float startFrequency,
			float endFrequency,
			float amplitude
		) {
			int sampleCount = Mathf.CeilToInt(SampleRate * duration);
			float[] samples = new float[sampleCount];

			float phase = 0.0f;

			for (int i = 0; i < sampleCount; i++) {
				float t = (float)i / sampleCount;
				float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
				float envelope = Mathf.Sin(t * Mathf.PI);
				float noise = Random.Range(-0.015f, 0.015f);

				phase += frequency * 2.0f * Mathf.PI / SampleRate;
				samples[i] = (Mathf.Sin(phase) * envelope * amplitude) + noise;
			}

			AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
			clip.SetData(samples, 0);
			return clip;
		}

		private static AudioClip CreatePopClip(
			string clipName,
			float duration,
			float frequency,
			float amplitude
		) {
			int sampleCount = Mathf.CeilToInt(SampleRate * duration);
			float[] samples = new float[sampleCount];

			float phase = 0.0f;

			for (int i = 0; i < sampleCount; i++) {
				float t = (float)i / sampleCount;
				float envelope = Mathf.Exp(-t * 8.0f);

				phase += frequency * 2.0f * Mathf.PI / SampleRate;
				samples[i] = Mathf.Sin(phase) * envelope * amplitude;
			}

			AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
			clip.SetData(samples, 0);
			return clip;
		}

		private static AudioClip CreateDoubleToneClip(
			string clipName,
			float duration,
			float firstFrequency,
			float secondFrequency,
			float amplitude
		) {
			int sampleCount = Mathf.CeilToInt(SampleRate * duration);
			float[] samples = new float[sampleCount];

			float phase = 0.0f;

			for (int i = 0; i < sampleCount; i++) {
				float t = (float)i / sampleCount;
				float frequency = t < 0.5f ? firstFrequency : secondFrequency;
				float envelope = Mathf.Sin(t * Mathf.PI);

				phase += frequency * 2.0f * Mathf.PI / SampleRate;
				samples[i] = Mathf.Sin(phase) * envelope * amplitude;
			}

			AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
			clip.SetData(samples, 0);
			return clip;
		}

		private static AudioClip CreateClickClip(
			string clipName,
			float duration,
			float frequency,
			float amplitude
		) {
			int sampleCount = Mathf.CeilToInt(SampleRate * duration);
			float[] samples = new float[sampleCount];

			float phase = 0.0f;

			for (int i = 0; i < sampleCount; i++) {
				float t = (float)i / sampleCount;
				float envelope = 1.0f - t;

				phase += frequency * 2.0f * Mathf.PI / SampleRate;
				samples[i] = Mathf.Sin(phase) * envelope * amplitude;
			}

			AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
			clip.SetData(samples, 0);
			return clip;
		}

		private static float EaseOutCubic(float value) {
			value = Mathf.Clamp01(value);
			return 1.0f - Mathf.Pow(1.0f - value, 3.0f);
		}
	}
}