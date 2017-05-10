using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class PlayerHealth : Singleton<PlayerHealth> {

    [SerializeField]
    GameObject prefabVFX;

    static int MAX_HARM = 4;
    static float TIME = 5f;
    static float EFFECT = 10f;
    
    AudioSource hurtAudioSource;

    GameObject objVFX = null;
    ParticleSystem _vfx = null;
    ParticleSystem vfx
    {
        get
        {
            if (objVFX == null)
            {
                objVFX = (GameObject)Instantiate(prefabVFX);
                objVFX.transform.parent = Camera.main.transform;
                objVFX.transform.localPosition = new Vector3(0f, -0.18f, 1.97f);
                objVFX.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            }
            if (_vfx == null)
            {
                _vfx = objVFX.GetComponent<ParticleSystem>();
            }
            return _vfx;
        }
    }

    // min 0, max 4
    int _harm = 0;
    int harm
    {
        get { return _harm; }
        set
        {
            _harm = value;
            ParticleSystem.EmissionModule emission = vfx.emission;
            emission.rateOverTime = _harm * EFFECT;

            if (hurtAudioSource == null)
            {
                hurtAudioSource = Camera.main.GetComponent<AudioSource>();
                if (!hurtAudioSource.isPlaying)
                {
                    hurtAudioSource.Play();
                }
            }
            hurtAudioSource.volume = 0.4f * _harm / 4f;
            if(_harm == 0)
            {
                hurtAudioSource.Stop();
            }
        }
    }

    float timer = 0;

    private List<AudioClip> _playerGotAtkVOClips;
	
    void Start()
    {
        this._playerGotAtkVOClips = AudioClipFactory.GetVOClipsWithReactionType(ReactionType.PlayerGotAtk);
        Debug.Log("Voice: " + this._playerGotAtkVOClips.Count);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.H))
        {
            GetHarm();
        }

		if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0 && harm > 0)
            {
                harm -= 1;
                if (harm > 0)
                    timer = TIME;
            }
        }
	}

    public void GetHarm()
    {
        if (harm < MAX_HARM)
        {
            harm += 1;
            timer = TIME;
            ReactionManager.Instance.Register(new List<Reaction> { new TalkReaction(this._playerGotAtkVOClips[Mathf.Clamp(harm - 1, 0, this._playerGotAtkVOClips.Count)]) }, ReactionType.PlayerGotAtk);
        }
    }

    void PlayHurtSound()
    {
        if(hurtAudioSource == null)
        {
            hurtAudioSource = Camera.main.GetComponent<AudioSource>();
            if (!hurtAudioSource.isPlaying)
            {
                hurtAudioSource.Play();
            }
        }
    }
}
