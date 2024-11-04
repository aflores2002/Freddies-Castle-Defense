using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffect[] soundEffects;

    // Dictionary to store sound effects for easy lookup
    private Dictionary<string, SoundEffect> soundEffectDictionary;

    private static AudioManager instance;
    public static AudioManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // Singleton pattern to ensure only one AudioManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioManager()
    {
        Debug.Log("Initializing Audio Manager");

        // Create dictionary for sound effects
        soundEffectDictionary = new Dictionary<string, SoundEffect>();
        foreach (var sfx in soundEffects)
        {
            Debug.Log($"Adding sound effect: {sfx.name}");
            soundEffectDictionary.Add(sfx.name, sfx);
        }

        // Initialize music
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySoundEffect(string soundName)
    {
        if (soundEffectDictionary.TryGetValue(soundName, out SoundEffect sound))
        {
            sfxSource.pitch = sound.pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
        else
        {
            Debug.LogWarning($"Sound effect {soundName} not found in dictionary!");
        }
    }

    // New method for playing sound with variation
    public void PlaySoundEffectWithVariation(string soundName, float pitchVariation, float volumeVariation = 1f)
    {
        if (soundEffectDictionary.TryGetValue(soundName, out SoundEffect sound))
        {
            // Store original pitch
            float originalPitch = sfxSource.pitch;

            // Apply variation
            sfxSource.pitch = sound.pitch * pitchVariation;

            // Play with volume variation
            sfxSource.PlayOneShot(sound.clip, sound.volume * volumeVariation);

            // Restore original pitch
            sfxSource.pitch = originalPitch;
        }
        else
        {
            Debug.LogWarning($"Sound effect {soundName} not found in dictionary!");
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void ToggleMusic(bool enabled)
    {
        if (musicSource != null)
            musicSource.mute = !enabled;
    }

    public void ToggleSFX(bool enabled)
    {
        if (sfxSource != null)
            sfxSource.mute = !enabled;
    }
}