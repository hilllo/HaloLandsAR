using UnityEngine;
using System.Collections;

public class AudioManager : Game.Singleton<AudioManager> {

    [SerializeField]
    private AudioSource _agentAudioSource;

    #region SFX
    [Header("SFX Ghost")]
    public AudioClip clipSFXGhostInit;
    public AudioClip clipSFXGhostAppear;
    public AudioClip[] clipSFXGhostAttack;
    public AudioClip clipSFXGhostDie;
    public AudioClip clipSFXGhostFly;   

    [Header("SFX Spell")]
    public AudioClip clipSFXSpellReveal;
    public AudioClip clipSFXSpellBurning;
    public AudioClip clipSFXSpellHide;

    [Header("SFX Bat")]
    public AudioClip clipSFXBatsFlying;

    [Header("SFX Hurt")]
    public AudioClip clipSFXPlayerHurt;

    #endregion SFX

    #region Tutorial VO
    [Header("Tutorial Intro")]
    public AudioClip[] clipVOMeow;
    public AudioClip[] clipVOGreeting;
    public AudioClip[] clipVOComeCloser;
    public AudioClip[] clipVOSpeech;
    public AudioClip clipVOItsThere;
    public AudioClip clipVOBehindYou;
    public AudioClip clipVOOnYourLeft;
    public AudioClip clipVOOnYourRight;

    [Header("Tutorial Battle")]
    public AudioClip clipVOStareAtIt;
    public AudioClip clipVOSeeYouCanStopIt;
    public AudioClip clipVOSpellCastTheSpell;
    public AudioClip clipVOOnTheWall;
    public AudioClip clipVOShoutItToTheGhost;
    public AudioClip clipVOBeBrave;
    public AudioClip clipVORelax;
    public AudioClip clipVOOhNoMoreOfThem;
    #endregion

    #region SFX Method
    public AudioSource Play(AudioClip clip)
    {
        return Play(clip, this.gameObject, 1, clip.length, false);
    }
    public AudioSource Play(AudioClip clip, GameObject emitter, bool space)
    {
        return Play(clip, emitter, 0.8f, clip.length, space);
    }

   
    public AudioSource Play(AudioClip clip, GameObject emitter, float volume, float length, bool space)
    {
        //Create an empty game object
        GameObject go = new GameObject("Audio: " + clip.name);
        go.transform.parent = emitter.transform;
        go.transform.localPosition = Vector3.zero;

        //Create source
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();

        // TODO: 3D audio setting
        if (space)
        {
            source.spatialBlend = 1;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.maxDistance = 10;
        }

        Destroy(go, length);
        return source;
    }
    #endregion

    #region CutScene VO

    [Header("CutScene VO")]
    public AudioClip clipVOWave12CutScene;
    public AudioClip clipVOWave23CutScene;
    public AudioClip[] clipsVOEndCutScene;

    #endregion CutScene VO

    #region Reaction VO

    [Header("Reaction VO")]
    public AudioClip clipVOGhostActiveHint;
    public AudioClip clipVOGhostCloseLeftHint;
    public AudioClip clipVOGhostCloseRightHint;
    public AudioClip clipVOGhostCloseBackHint;
    public AudioClip clipVOSpellKillNothingHint;
    public AudioClip clipVOSpellToBatSpiderHint;
    public AudioClip clipVOSpellOutdatedHint;

    #endregion Reaction VO

    #region VO Method

    public float VOPlay(AudioClip clip, float volume)
    {
        if (this._agentAudioSource == null)
            this._agentAudioSource = AgentBehaviour.Instance.gameObject.GetComponent<AudioSource>();
        //Create source
        this._agentAudioSource.clip = clip;
        this._agentAudioSource.volume = volume;
        this._agentAudioSource.Play();

        // TODO: 3D audio setting (directly in AudioSource on Agent)
        // Always 3D here

        return clip.length;
    }

    public float VOStop()
    {
        if (this._agentAudioSource == null)
            this._agentAudioSource = GameObject.Find("Agent0").GetComponent<AudioSource>();

        this._agentAudioSource.Stop();
        // TODO: add transition
        return 0f; // TODO: return transition clip length

    }

    #endregion VO Method



    #region Volume Fade Method
    /*
    private void VolumeFadeOn(AudioSource audioSource)
    {
        StartCoroutine(VolumeFade(audioSource, 1f));
    }

    private void VolumeFadeOff(AudioSource audioSource)
    {
        StartCoroutine(VolumeFade(audioSource, 0f));
    }

    private void VolumeFadeTo(AudioSource audioSource, float volume)
    {
        StartCoroutine(VolumeFade(audioSource, volume));
    }
    */

    public void FadeTo(AudioSource audioSource, float targetVolume, float time)
    {
        StartCoroutine(VolumeFade(audioSource, targetVolume, time));
    }

    private IEnumerator VolumeFade(AudioSource audioSource, float targetVolume, float time)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        bool isIncrease = audioSource.volume < targetVolume;
        float increase = (targetVolume - audioSource.volume) / time;
        while (audioSource != null)
        {
            time -= Time.deltaTime;
            audioSource.volume = Mathf.Clamp01(audioSource.volume + increase * Time.deltaTime);
            if(audioSource.volume >= targetVolume && isIncrease)
            {
                break;
            }else if(audioSource.volume <= targetVolume && !isIncrease)
            {
                break;
            }
            yield return null;
        }
        if (targetVolume == 0)
        {
            audioSource.Stop();
        }
    }
    #endregion
}