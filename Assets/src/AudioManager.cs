using UnityEngine;
using System.Collections.Generic;

// AudioManager handles all game audio
// Uses the Singleton pattern to ensure only one instance exists
public class AudioManager : MonoBehaviour
{
    // Serializable class to define sound effect properties in the Unity Inspector
    [System.Serializable]
    public class SoundEffect
    {
        public string name;                    // Identifier for the sound effect
        public AudioClip clip;                 // Actual audio file
        [Range(0f, 1f)]
        public float volume = 1f;              // Volume control for this sound
        [Range(0.1f, 3f)]
        public float pitch = 1f;               // Pitch control for this sound
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;    // Source for background music
    [SerializeField] private AudioSource sfxSource;      // Source for sound effects

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;  // Background music track
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;   // Volume control for music

    [Header("Sound Effects")]
    [SerializeField] private SoundEffect[] soundEffects; // Array of all available sound effects

    // Dictionary for lookup of sound effects by name
    private Dictionary<string, SoundEffect> soundEffectDictionary;

    // Singleton instance
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // If no instance exists, set this as the instance and persist between scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        // If instance already exists, destroy this duplicate
        else
        {
            Destroy(gameObject);
        }
    }

    // Initialize the audio system
    private void InitializeAudioManager()
    {
        Debug.Log("Initializing Audio Manager");

        // Build dictionary from sound effect array for fast lookups
        soundEffectDictionary = new Dictionary<string, SoundEffect>();
        foreach (var sfx in soundEffects)
        {
            Debug.Log($"Adding sound effect: {sfx.name}");
            soundEffectDictionary.Add(sfx.name, sfx);
        }

        // Set up and start background music if available
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;           // Ensure music loops continuously
            musicSource.Play();
        }
    }

    // Play a sound effect by name with default settings
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

    // Play sound effect with randomized pitch and volume variations (Used for footsteps)
    public void PlaySoundEffectWithVariation(string soundName, float pitchVariation, float volumeVariation = 1f)
    {
        if (soundEffectDictionary.TryGetValue(soundName, out SoundEffect sound))
        {
            // Store original pitch to restore later
            float originalPitch = sfxSource.pitch;

            // Apply the pitch variation
            sfxSource.pitch = sound.pitch * pitchVariation;

            // Play sound with volume variation
            sfxSource.PlayOneShot(sound.clip, sound.volume * volumeVariation);

            // Reset pitch to original value
            sfxSource.pitch = originalPitch;
        }
        else
        {
            Debug.LogWarning($"Sound effect {soundName} not found in dictionary!");
        }
    }

    // Set volume of background music
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);   // Ensure volume stays between 0 and 1
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    // Set volume for all sound effects
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
    }

    // Enable/disable background music
    public void ToggleMusic(bool enabled)
    {
        if (musicSource != null)
            musicSource.mute = !enabled;
    }

    // Enable/disable all sound effects
    public void ToggleSFX(bool enabled)
    {
        if (sfxSource != null)
            sfxSource.mute = !enabled;
    }
}