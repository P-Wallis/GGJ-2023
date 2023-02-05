using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SFX
{
    GATHERING,
    GROWING,
    BREAK,
    SUCCESS,
    ERROR,
    HOVER,
    CLICK,
    SUNRISE
}

public enum MusicTrack
{
    BACKGROUND,
    DANGER,
    GROWING
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
        public AudioClip error;
        public AudioClip hover;
        public AudioClip click;
        public AudioClip sunrise;
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
    private MusicTrack track = MusicTrack.BACKGROUND;
    private MusicTrack oldTrack = MusicTrack.BACKGROUND;


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
        sfxDict.Add(SFX.ERROR, MakeSound(soundEffects.error, soundEffects.effectsMixer));
        sfxDict.Add(SFX.HOVER, MakeSound(soundEffects.hover, soundEffects.effectsMixer));
        sfxDict.Add(SFX.CLICK, MakeSound(soundEffects.click, soundEffects.effectsMixer));
        sfxDict.Add(SFX.SUNRISE, MakeSound(soundEffects.sunrise, soundEffects.effectsMixer));

    }

    public void PlaySFX(SFX fx)
    {
        if (sfxDict.ContainsKey(fx))
        {
            sfxDict[fx].Play();
        }
    }

    Coroutine musicTransitionAnimation;
    public void TransitionMusicTo(MusicTrack newMusic)
    {
        if (track == newMusic)
        {
            return;
        }

        if (musicTransitionAnimation != null)
        {
            StopCoroutine(musicTransitionAnimation);
            mixer.SetFloat(GetMusicParameter(oldTrack), -80);
        }
        musicTransitionAnimation = StartCoroutine(CoTransitionMusic(track, newMusic));

    }

    string GetMusicParameter(MusicTrack musicTrack)
    {
        switch (musicTrack)
        {
            case MusicTrack.BACKGROUND:
                return music.backgroundMixerVolumeParameter;

            case MusicTrack.DANGER:
                return music.dangerMixerVolumeParameter;

            case MusicTrack.GROWING:
                return music.growingMixerVolumeParameter;
        }

        return "";
    }

    IEnumerator CoTransitionMusic(MusicTrack from, MusicTrack to)
    {
        oldTrack = from;
        track = to;

        string fromParam = GetMusicParameter(from), toParam = GetMusicParameter(to);
        AnimationCurve smoothCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        float percent = 0;
        while (percent < 1)
        {
            mixer.SetFloat(fromParam, Mathf.Lerp(0, -80, smoothCurve.Evaluate(percent)));
            mixer.SetFloat(toParam, Mathf.Lerp(-80, 0, smoothCurve.Evaluate(percent)));

            percent += Time.deltaTime;
            yield return null;
        }

        mixer.SetFloat(fromParam, -80);
        mixer.SetFloat(toParam, 0);
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
