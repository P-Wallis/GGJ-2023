using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SFX
{
    GATHERING,
    GROWING,
    BREAK,
    SUCCESS
}

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class Effects
    {
        public AudioClip gathering;
        public AudioClip growing;
        public AudioClip breaking;
        public AudioClip success;
        public AudioMixerGroup effectsMixer;
        public string effectsMixerVolumeParameter;
    }

    [System.Serializable]
    public class Music
    {
        public AudioClip background;
        public AudioMixerGroup backgroundMixer;
        public string backgroundMixerVolumeParameter;
        public AudioClip danger;
        public AudioMixerGroup dangerMixer;
        public string dangerMixerVolumeParameter;
        public AudioClip growing;
        public AudioMixerGroup growingMixer;
        public string growingMixerVolumeParameter;
    }

    [System.Serializable]
    public class Ambience
    {
        public AudioClip nature;
        public AudioMixerGroup ambienceMixer;
        public string ambienceMixerVolumeParameter;
    }

    public Effects soundEffects;
    public Music music;
    public Ambience ambience;
    public AudioMixer mixer;

    private Dictionary<SFX, AudioSource> sfxDict = new Dictionary<SFX, AudioSource>();

    private void Start()
    {
        // Music
        MakeSound(music.background, music.backgroundMixer, music.backgroundMixerVolumeParameter, 0, true, true);
        MakeSound(music.danger, music.dangerMixer, music.dangerMixerVolumeParameter, -80, true, true);
        MakeSound(music.growing, music.growingMixer, music.growingMixerVolumeParameter, -80, true, true);

        // Ambience
        MakeSound(ambience.nature, ambience.ambienceMixer, ambience.ambienceMixerVolumeParameter, 0, true, true);

        // SFX
        sfxDict.Add(SFX.GATHERING, MakeSound(soundEffects.gathering, soundEffects.effectsMixer));
        sfxDict.Add(SFX.GROWING, MakeSound(soundEffects.growing, soundEffects.effectsMixer));
        sfxDict.Add(SFX.BREAK, MakeSound(soundEffects.breaking, soundEffects.effectsMixer));
        sfxDict.Add(SFX.SUCCESS, MakeSound(soundEffects.success, soundEffects.effectsMixer));

    }

    public void PlaySFX(SFX fx)
    {
        if (sfxDict.ContainsKey(fx))
        {
            sfxDict[fx].Play();
        }
    }

    AudioSource MakeSound(AudioClip clip, AudioMixerGroup mixerGroup, string volumeParameter = null, float mixerVolume = 0, bool play = false, bool loop = false)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = clip;
        source.dopplerLevel = 0;
        source.loop = loop;
        source.spatialBlend = 0;
        source.outputAudioMixerGroup = mixerGroup;
        if (!string.IsNullOrEmpty(volumeParameter))
        {
            mixer.SetFloat(volumeParameter, mixerVolume);
        }
        if(play)
        {
            source.Play();
        }

        return source;
    }
}
