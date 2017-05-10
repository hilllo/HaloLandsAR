#define CAT_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
public class AgentBehaviour : EyeDetector {

    [SerializeField]
    private GameObject objectLand, objectFloat, modelFloat;

    [SerializeField]
    private ParticleSystem particleTail, particleGlow;

    private enum MoveState
    {
        LAND,
        NONE,
        DISAPPEAR,
        MOVE,
        APPEAR,
        END
    }
    private MoveState moveState = MoveState.LAND;

    private Vector3 _landPos;
    public Vector3 landPos
    {
        get
        {
            if (this._landPos == null)
                throw new System.ArgumentNullException("landPos");
            return this._landPos;
        }
        set
        {
            if (this._landPos == value)
                return;

            this._landPos = value;
        }
    }

    #region Parameters
    private static float FLOAT_TIME = 0.5f;
    private static float FLOAT_COOLDOWN = 0.5f;
    private static float FLOAT_THRESHOLD = 0.5f;
    private static float MIN_DISTANCE = 0.15f;
    private static float ALPHA_MIN = 1f;
    private static float ALPHA_MAX = 0.7f;
    private static float ALPHA_SPEED = 5f;
    #endregion
    
    #region Target
    private GameObject target = null;
    public GameObject GetTarget()
    {
        return target;
    }
    private bool isTargetTemp = false;
    private void ResetTarget()
    {
        if (target != null && isTargetTemp)
        {
            Destroy(target);
        }
        target = null;
    }
    public void SetTarget(GameObject obj)
    {
        ResetTarget();
        target = obj;
        isTargetTemp = false;
    }
    public GameObject SetTarget(Vector3 pos)
    {
        ResetTarget();
        GameObject obj = new GameObject("agent_target");
        obj.transform.position = pos;
        SetTarget(obj);
        isTargetTemp = true;
        return obj;
    }
    #endregion

    #region Material
    private Material mat;
    private float _alphaIndex = 0;
    private float alphaIndex
    {
        get
        {
            return _alphaIndex;
        }
        set
        {
            _alphaIndex = value;
            float t = ALPHA_MIN + (ALPHA_MAX - ALPHA_MIN) * _alphaIndex;
            mat.SetFloat("_Cutoff", t);
        }
    }
    #endregion

    private static AgentBehaviour _instance = null;
    public static AgentBehaviour Instance
    {
        get
        {
            return _instance;
        }
    }

    private GameObject _anchor;
    public GameObject anchor
    {
        get
        {
            if (_anchor == null)
            {
                _anchor = new GameObject("Cat Anchor");
                _anchor.transform.parent = Camera.main.transform;
                _anchor.transform.localPosition = new Vector3(0.263f, -0.075f, 1.838f);
            }
            return _anchor;
        }
    }

    private bool isLanding = true;

    protected override void Awake()
    {
        base.Awake();
        if (_instance == null)
            _instance = this;
    }

    // Use this for initialization
    void Start()
    {
        mat = modelFloat.GetComponent<Renderer>().material;
        mat.SetFloat("_Cutoff", 1);
        prevPos = transform.position;
    }

    Vector3 prevPos;

    // Update is called once per frame
    void Update()
    {
        if (moveState == MoveState.END)
            return;

#if CAT_DEBUG

        if (Input.GetKeyDown(KeyCode.M))
        {
            FloatUp();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            LandTo(landPos);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            DisappearTo(landPos);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            pos.z = 2f;
            pos = Camera.main.ScreenToWorldPoint(pos);
            SetTarget(pos);
        }

#endif

        /*
        if (isFloating)
        {
            transform.LookAt(Camera.main.transform);
            if (!isMoving)
                floatTimer += Time.deltaTime;
            if (floatTimer > FLOAT_COOLDOWN && Vector3.Distance(transform.position, anchorPos) > FLOAT_THRESHOLD)
            {
                FloatTo(anchorPos, false);
                floatTimer = 0;
            }
        }
        */

        if (currentTalkClip != null && talkTimer > 0)
        {
            talkTimer -= Time.deltaTime;
            if (talkTimer <= 0)
            {
                talkTimer = 0;
                currentTalkClip = null;
            }
        }

        if (moveState == MoveState.NONE)
        {
            transform.LookAt(Camera.main.transform);
        }

        switch(moveState)
        {
            case MoveState.LAND:
                if (!isLanding && target != null)
                    ChangeState(MoveState.MOVE);
                break;
            case MoveState.NONE:
                if (target != null)
                    ChangeState(MoveState.APPEAR);
                break;
            case MoveState.DISAPPEAR:
                if (target == null)
                    ChangeState(MoveState.APPEAR);
                else if (alphaIndex > 0)
                    alphaIndex -= ALPHA_SPEED;
                else
                    ChangeState(MoveState.MOVE);
                break;
            case MoveState.MOVE:
                if (target == null)
                    ChangeState(MoveState.APPEAR);
                else if (Vector3.Distance(transform.position, target.transform.position) > MIN_DISTANCE)
                    transform.position = Vector3.Slerp(transform.position, target.transform.position, 0.4f);
                else
                {
                    if (isEnding)
                    {
                        ChangeState(MoveState.END);
                    }
                    ResetTarget();
                    ChangeState(isLanding ? MoveState.LAND : MoveState.APPEAR);
                }
                break;
            case MoveState.APPEAR:
                if (target != null)
                    ChangeState(MoveState.DISAPPEAR);
                else if (alphaIndex < 1)
                    alphaIndex += ALPHA_SPEED;
                else
                    ChangeState(MoveState.NONE);
                break;
        }
    }

    void ChangeState(MoveState state)
    {
        ParticleSystem.EmissionModule glow = particleGlow.emission;
        ParticleSystem.EmissionModule tail = particleTail.emission;
        switch (state)
        {
            case MoveState.LAND:
                alphaIndex = 0;
                glow.enabled = false;
                tail.enabled = false;
                objectLand.SetActive(true);
                #if UNITY_WSA && !UNITY_EDITOR
                WorldAnchorManager.Instance.AttachAnchor(gameObject, gameObject.name);
                #endif
                break;
            case MoveState.NONE:
                alphaIndex = 1;
                glow.enabled = true;
                tail.enabled = false;
                objectLand.SetActive(false);
                break;
            case MoveState.APPEAR:
                glow.enabled = true;
                tail.enabled = false;
                objectLand.SetActive(false);
                break;
            case MoveState.MOVE:
                alphaIndex = 0;
                glow.enabled = false;
                tail.enabled = true;
                objectLand.SetActive(false);
                break;
            case MoveState.DISAPPEAR:
                glow.enabled = false;
                tail.enabled = true;
                objectLand.SetActive(false);
                break;
            case MoveState.END:
                glow.enabled = false;
                tail.enabled = false;
                objectLand.SetActive(false);
                objectFloat.SetActive(false);
                break;
        }
        moveState = state;
    }

    public void Land()
    {
        this.LandTo(this.landPos);
    }

    public void LandTo(Vector3 pos)
    {
        if (isLanding)
            return;
        isLanding = true;
        SetTarget(prevPos);
    }

    public void FloatUp()
    {
        if (!isLanding)
            return;
        isLanding = false;
        SetTarget(anchor);
        
        #if UNITY_WSA && !UNITY_EDITOR
        WorldAnchorManager.Instance.RemoveAnchor(gameObject);
        #endif
    }

    bool isEnd = false;
    bool isEnding = false;
    public void DisappearTo(Vector3 pos)
    {
        isEnding = true;
        FloatUp();
        FloatTo(pos);
    }

    public void FloatTo(Vector3 pos)
    {
        if (isLanding)
            return;

        SetTarget(pos);
    }

    #region Talk
    private float talkTimer = 0f;
    private AudioClip currentTalkClip = null;

    public bool isTalking(AudioClip clip)
    {
        return currentTalkClip == clip;
    }

    /// <param name="audio">Dialogue audio clip</param>
    /// <returns>Audio length</returns>
    public float Talk(AudioClip clip)
    {
        StopTalk();
        currentTalkClip = clip;
        talkTimer = clip.length;
        AudioManager.Instance.VOPlay(clip, 1f);
        return clip.length;
    }

    public float StopTalk()
    {
        currentTalkClip = null;
        talkTimer = 0;
        return AudioManager.Instance.VOStop();
    }

    #endregion
    public void LookAt(GameObject obj)
    {
        this.LookAt(obj.transform.position);
    }

    public void LookAt(Vector3 pos)
    {
        pos.y = this.transform.position.y;
        this.transform.LookAt(pos);
    }
}