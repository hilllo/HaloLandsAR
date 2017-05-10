using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GhostState
{
    APPEAR,
    KILLED,
    HIDE
}

[RequireComponent(typeof(PATH.a_star))]
public class WhiteGhost : EyeDetector, Ghost
{    
    public GhostState ghostState = GhostState.HIDE;

    public AudioClip[] ghostIdleClips;

    [SerializeField]
    private GameObject ghostModel;
    [SerializeField]
    private ParticleSystem ghostHideVFX;
    [SerializeField]
    private GameObject ghostSpawnVFX;
    [SerializeField]
    private GameObject ghostDieVFX;

    private static float SPEED = 0.2f;
    private static float APPEAR_COUNT = 1f;
    public static float MIN_DISTANCE = 0.8f;
    private static bool IS_AUTO_MOVE = false;
    public static float NEAR_ALERT_DISTANCE_SQR = 9f;
    public static GameObject PLAYER;

    private AudioSource audioSource;
    private PATH.a_star navigator;

    private List<AudioClip> _ghostKillHintVOClips;

    // Use this for initialization
    void Start()
    {
        if (PLAYER == null)
        {
            PLAYER = Camera.main.gameObject;
        }
        navigator = GetComponent<PATH.a_star>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = ghostIdleClips[Random.Range(0, ghostIdleClips.Length - 1)];
        audioSource.Play();
        AudioManager.Instance.Play(AudioManager.Instance.clipSFXGhostInit, gameObject, true);

        this._ghostKillHintVOClips = AudioClipFactory.GetVOClipsWithReactionType(ReactionType.GhostKillHint);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            Kill();

        Vector3 ghostPos = transform.position;
        Vector3 playerPos = Camera.main.transform.position;
        ghostPos.y = 0;
        playerPos.y = 0;

        if (Vector3.Distance(ghostPos, playerPos) < MIN_DISTANCE)
        {
            // TODO: attack player
            PlayerHealth.Instance.GetHarm();
            AudioManager.Instance.Play(AudioManager.Instance.clipSFXGhostAttack[Random.Range(0, AudioManager.Instance.clipSFXGhostAttack.Length-1)], Camera.main.gameObject, true);
            Hide();
        }
        transform.LookAt(PLAYER.transform.position);
    }

    public bool Kill()
    {
        if (ghostState != GhostState.APPEAR || stareState != StareState.STARED)
        {
            return false;
        }
        ghostState = GhostState.KILLED;

        int voIndex = Random.Range(0, this._ghostKillHintVOClips.Count);
        ReactionManager.Instance.Register(new List<Reaction> { new TalkReaction(this._ghostKillHintVOClips[voIndex]) }, ReactionType.GhostKillHint);

        StartCoroutine(KillProcess());
        return true;
    }

    IEnumerator KillProcess()
    {
        float distance = Vector3.Distance(gameObject.transform.position, PLAYER.transform.position);
        yield return new WaitForSeconds(distance / BlastWave.speed);
        ghostModel.SetActive(false);
        Instantiate(ghostDieVFX, transform.position, Quaternion.identity);
        float length = AudioManager.Instance.Play(AudioManager.Instance.clipSFXGhostDie, gameObject, true).clip.length;
        yield return new WaitForSeconds(length);
        Destroy(gameObject);
        yield return null;
    }

    protected override void OnViewExit()
    {
        base.OnViewExit();
        if (audioSource != null)
            audioSource.volume = 1f;

        if (ghostState == GhostState.APPEAR)
            navigator.Move();
    }

    protected override void OnViewEnter()
    {
        base.OnViewEnter();

        if (ghostState == GhostState.HIDE)
        {
            StartCoroutine(CountAppear());
        }

        if (audioSource != null)
            audioSource.volume = 0.6f;
        navigator.Stop();
    }

    protected override void OnNotStared()
    {
        base.OnNotStared();
        if ((!IS_AUTO_MOVE && stareState == StareState.NONE) || ghostState != GhostState.APPEAR)
        {
            return;
        }
    }

    IEnumerator CountAppear()
    {
        float t = APPEAR_COUNT;
        while (t > 0)
        {
            if (stareState != StareState.STARED)
                yield break;
            t -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Appear();
        yield return null;
    }

    void Appear()
    {
        ghostState = GhostState.APPEAR;
        ghostModel.SetActive(true);
        ghostHideVFX.loop = false;
        transform.localPosition = Vector3.zero;
        Instantiate(ghostSpawnVFX, transform.position, Quaternion.identity);
        AudioManager.Instance.Play(AudioManager.Instance.clipSFXGhostAppear, gameObject, true);
        
    }

    void Hide()
    {
        ghostState = GhostState.HIDE;
        ghostModel.SetActive(false);
        transform.localPosition = Vector3.zero;
        ghostHideVFX.Play();
        ghostHideVFX.loop = true;

        navigator.Stop();
        navigator.Reset();

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}