using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Clips")]
    [SerializeField] private AudioClip[] audioClips;
    
    private float currentMusicVolume;
    private AudioClip sfxClip;

    [Header("Volume")]
    [SerializeField] private float masterVolume = 100f;
    [SerializeField] private float musicVolume = 100f;
    [SerializeField] private float soundVolume = 100f;

    [SerializeField] private TMP_Text masterCounter, masterCounterPause;
    [SerializeField] private TMP_Text musicCounter, musicCounterPause;
    [SerializeField] private TMP_Text soundCounter, soundCounterPause;

    [SerializeField] private Slider masterSlider, masterSliderPause;
    [SerializeField] private Slider musicSlider, musicSliderPause;
    [SerializeField] private Slider soundSlider, soundSliderPause;

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaySFX(int id, float volume = 1f, float pitch = 1f)
    {
        if (!(id < audioClips.Length && id >= 0)) return;

        if (sfxClip == audioClips[id])
            sfxSource.Stop();
        else 
            sfxClip = audioClips[id];

        float totalVolume = (masterVolume / 100) * (soundVolume / 100) * volume;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(audioClips[id], totalVolume);
        sfxSource.pitch = 1f;
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (musicSource.clip == clip) return;

        float totalVolume = (masterVolume / 100) * (musicVolume / 100) * volume;

        musicSource.clip = clip;
        musicSource.volume = totalVolume;
        currentMusicVolume = volume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void ChangeMasterVolume(Slider slider)
    {
        masterVolume = slider.value;
        masterCounter.text = masterVolume.ToString();
        masterCounterPause.text = masterVolume.ToString();
        masterSlider.value = masterVolume;
        masterSliderPause.value = masterVolume;

        musicSource.volume = (masterVolume / 100) * (musicVolume / 100) * currentMusicVolume;
    }

    public void ChangeMusicVolume(Slider slider)
    {
        musicVolume = slider.value;
        musicCounter.text = musicVolume.ToString();
        musicCounterPause.text = musicVolume.ToString();
        musicSlider.value = musicVolume;
        musicSliderPause.value = musicVolume;

        musicSource.volume = (masterVolume / 100) * (musicVolume / 100) * currentMusicVolume;
    }

    public void ChangeSoundVolume(Slider slider)
    {
        soundVolume = slider.value;
        soundCounter.text = soundVolume.ToString();
        soundCounterPause.text = soundVolume.ToString();
        soundSlider.value = soundVolume;
        soundSliderPause.value = soundVolume;
    }
}