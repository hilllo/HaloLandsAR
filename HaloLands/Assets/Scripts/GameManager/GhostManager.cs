using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using Game.Event;

public enum GhostType
{
    White = 0,
    Spider = 1,
    Bat = 2
}

public class GhostManager : Game.Singleton<GhostManager> {

    #region Fields

    public GameObject whiteGhostPrefab;
    public GameObject spiderPrefab;
    public GameObject batPrefab;

    public List<GameObject> whiteGhostPositions;
    public List<GameObject> batPositions;
    public List<GameObject> spiderPositions;

    public List<WhiteGhost> whiteGhostList = new List<WhiteGhost>();
    List<Ghost> otherGhostList = new List<Ghost>();

    private Game.Event.WaveEvent _currentWaveEvent;

    [SerializeField]
    private GameObject BlastWaveVFX;

    private const float findGhostThreshold = 3.0f;
    [SerializeField]
    private float findGhostTimer = 0.0f;
    private const float activateGhostThreshold = 8.0f;
    [SerializeField]
    private float activateGhostTimer = 0.0f;

    #endregion Fields

    #region Properties

    public Game.Event.WaveEvent currentWaveEvent
    {
        get
        {
            return this._currentWaveEvent;
        }
        set
        {
            if (this._currentWaveEvent == value)
                return;

            if (this.whiteGhostList.Count > 0)
                throw new System.ArgumentException("Expected this.ghostList.Count == 0 before changing wave");
            this._currentWaveEvent = value;
        }
    }

    #endregion Properties

    #region Vase
    private int _vaseIndex = -1;
    // GhostManager.Instance.vaseIndex = i;
    public int vaseIndex
    {
        set
        {
            _vaseIndex = value;
        }
    }
    #endregion
    #region lamp
    bool lampFirstTime = true;
    #endregion

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateAGhost(whiteGhostPositions.Count);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            DispellGhosts();
        }

        this.CheckFindGhost();
    }

    public void GenerateAGhost(int count)
    {
        int len = whiteGhostPositions.Count;
        if (count > len)
        {
            throw new System.ArgumentException("Ghost count out of range");
        }
        WhiteGhost[] ghosts = new WhiteGhost[count];
        bool[] isVisited = new bool[len];
        for (int i = 0; i < count; i++)
        {
            // get an available anchor
            int index = Random.Range(0, len - i);
            int j = 0;
            while (isVisited[j]) j++;
            while (index-- > 0)
            {
                j++;
                while (isVisited[j]) j++;
            }
            if (j >= count) break;
            isVisited[j] = true;
            ghosts[i] = GenerateWhiteGhost(whiteGhostPositions[j]);

            if (lampFirstTime == true)
            {
                //TurnOffLamp                
                foreach(GameObject lamp in GameObject.FindGameObjectsWithTag("Lamp"))
                {
                    lamp.GetComponent<LampController>().TurnOffLight();
                }
                lampFirstTime = false;
            }
            if (j == _vaseIndex)
            {
                _vaseIndex = -1;
                // TODO: vase broke
                VaseControl.Instance.Break();
            }
        }
    }

    List<SpiderGhost> allSpiderList = new List<SpiderGhost>();

    public void GenerateAllSpiders()
    {
        foreach (GameObject anchor in spiderPositions)
        {
            SpiderGhost sg = (SpiderGhost)GenerateSpider(anchor);
            allSpiderList.Add(sg);
            otherGhostList.Add(sg);
        }
    }

    public void ActivateAllSpiders()
    {
        foreach (SpiderGhost spider in allSpiderList)
        {
            spider.Activate();
        }
    }

    public void GenerateSpiders(int count)
    {
        ActivateAllSpiders();
    }

    public void GenerateBats(int count)
    {
        int len = whiteGhostPositions.Count;
        if (count > len)
        {
            throw new System.ArgumentException("Ghost count out of range");
        }
        for (int i = 0; i < count; i++)
        {
            GenerateBat(batPositions[i]);
        }
    }

    WhiteGhost GenerateWhiteGhost(GameObject anchor)
    {
        GameObject obj = Instantiate(whiteGhostPrefab);
        obj.transform.parent = anchor.transform;
        obj.transform.localPosition = Vector3.zero;
        WhiteGhost ghost = obj.GetComponentInChildren<WhiteGhost>();
        whiteGhostList.Add(ghost);
        return ghost;
    }

    Ghost GenerateSpider(GameObject anchor)
    {
        GameObject obj = Instantiate(spiderPrefab);
        obj.transform.parent = anchor.transform;
        obj.transform.localPosition = Vector3.zero;
        Ghost ghost = obj.GetComponentInChildren<Ghost>();
        // otherGhostList.Add(ghost);
        return ghost;
    }

    Ghost GenerateBat(GameObject anchor)
    {
        GameObject obj = Instantiate(batPrefab);
        obj.transform.parent = anchor.transform;
        obj.transform.localPosition = Vector3.zero;
        Ghost ghost = obj.GetComponentInChildren<Ghost>();
        otherGhostList.Add(ghost);
        return ghost;
    }

    public int DispellGhosts(int currentSpellIndex)
    {
        Camera.main.transform.FindChild("CastSpell").GetComponent<AudioSource>().Play();
        return this.DispellGhosts();
    }

    /// <summary>
    /// Called when a spell is casted
    /// </summary>
    /// <returns>Number of ghost killed</returns>
    public int DispellGhosts()
    {
        int count = 0;
        if (whiteGhostList.Count == 0)
        {
            return 0;
        }
        foreach (WhiteGhost ghost in whiteGhostList)
        {
            if (ghost != null && ghost.Kill())
            {
                count += 1;
            }
        }
        // check if all ghosts are killed
        bool isAllKilled = true;
        foreach (WhiteGhost ghost in whiteGhostList)
        {
            if (ghost != null && ghost.ghostState != GhostState.KILLED)
            {
                isAllKilled = false;
                break;
            }
        }
        if (isAllKilled)
        {
            ClearGhost();
        }
        Debug.Log("Killed ghosts: " + count);
        if (count == 0)
        {
            foreach (Ghost ghost in otherGhostList)
            {
                EyeDetector e = (EyeDetector)ghost;
                if (e.stareState == EyeDetector.StareState.STARED)
                    count = -1;
            }
        }

        // blast wave VFX
        GameObject objVFX = (GameObject)Instantiate(BlastWaveVFX);
        BlastWave vfx = objVFX.GetComponent<BlastWave>();
        vfx.Setup(count);
        return count;
    }

    void ClearGhost()
    {
        foreach (Ghost other in otherGhostList)
        {
            if (other != null)
            {
                other.Kill();
            }
        }
        otherGhostList.Clear();
        whiteGhostList.Clear();
        if (GameManager.Instance.gameMode == GameMode.GameFlowMode)
        {
            currentWaveEvent.EndEvent();
        }
    }

    private void CheckFindGhost()
    {
        if (whiteGhostList.Count <= 0)
        {
            this.findGhostTimer = 0.0f;
            this.activateGhostTimer = 0.0f;
            return;
        }

        bool noGhost = true;
        foreach (WhiteGhost ghost in whiteGhostList)
        {
            if (ghost.ghostState == GhostState.APPEAR)
            {
                this.activateGhostTimer = 0.0f;
                noGhost = false;
                break;
            }
        }
        if (noGhost)
        {
            this.findGhostTimer = 0.0f;
            this.activateGhostTimer += Time.deltaTime;
            if (this.activateGhostTimer > GhostManager.activateGhostThreshold)
            {
                Transform playerTrans = Camera.main.transform;
                foreach (WhiteGhost ghost in whiteGhostList)
                {                    
                    if (ghost.ghostState == GhostState.HIDE)
                    {
                        Vector3 ghostPos = ghost.transform.position;
                        int angleDir = AngleDir(playerTrans.forward, ghostPos - playerTrans.position, playerTrans.up);
                        AudioClip audioClip = null;

                        if (angleDir == -1) // left
                            audioClip = AudioManager.Instance.clipVOGhostCloseLeftHint;
                        else if (angleDir == 1) // right
                            audioClip = AudioManager.Instance.clipVOGhostCloseRightHint;
                        else if (angleDir == 0) // back
                            audioClip = AudioManager.Instance.clipVOGhostCloseBackHint;

                        if (audioClip != null)
                            ReactionManager.Instance.Register(new List<Reaction> { new TalkReaction(audioClip) }, ReactionType.GhostActiveHint);

                        this.activateGhostTimer = 0.0f;
                        return;
                    }
                }
                this.activateGhostTimer = 0.0f;
            }
            return;
        }


        foreach (WhiteGhost ghost in whiteGhostList)
        {
            if (ghost.stareState == EyeDetector.StareState.STARED)
            {
                this.findGhostTimer = 0.0f;
                return;
            }
        }

        // TODO: Optimized this
        this.findGhostTimer += Time.deltaTime;
        if (this.findGhostTimer > GhostManager.findGhostThreshold)
        {
            float alertDistanceSqr = WhiteGhost.NEAR_ALERT_DISTANCE_SQR;
            float minDistanceSqr = WhiteGhost.MIN_DISTANCE * WhiteGhost.MIN_DISTANCE;
            Transform playerTrans = Camera.main.transform;

            foreach (WhiteGhost ghost in whiteGhostList)
            {
                Vector3 ghostPos = ghost.transform.position;
                float sqrDistance = Vector3.SqrMagnitude(playerTrans.position - ghostPos);
                if (sqrDistance < alertDistanceSqr && sqrDistance > minDistanceSqr)
                {
                    int angleDir = AngleDir(playerTrans.forward, ghostPos - playerTrans.position, playerTrans.up);
                    AudioClip audioClip = null;

                    if (angleDir == -1) // left
                        audioClip = AudioManager.Instance.clipVOGhostCloseLeftHint;
                    else if (angleDir == 1) // right
                        audioClip = AudioManager.Instance.clipVOGhostCloseRightHint;
                    else if (angleDir == 0) // back
                        audioClip = AudioManager.Instance.clipVOGhostCloseBackHint;

                    if (audioClip != null)
                        ReactionManager.Instance.Register(new List<Reaction> { new TalkReaction(audioClip) }, ReactionType.GhostCloseHint);

                    this.findGhostTimer = 0.0f;
                    return;
                }
                this.findGhostTimer = 0.0f;
            }
        }
        
    }

    /// <summary>
    /// returns -1 when to the left, 1 to the right, and 0 for forward/backward
    /// </summary>
    /// <param name="fwd"></param>
    /// <param name="targetDir"></param>
    /// <param name="up"></param>
    /// <returns></returns>
    private static int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
        {
            return 1;
        }
        else if (dir < 0.0f)
        {
            return -1;
        }
        else {
            return 0;
        }
    }
}