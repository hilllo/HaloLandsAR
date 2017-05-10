using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using Game.Event;

public class TutorialFlow : CutSceneFlow
{
    public enum Stage
    {
        NONE,
        START,
        BATTLE,
        FINISH
    }
    private Stage _stage = Stage.NONE;
    public Stage stage { get { return _stage; } }

    private static float GHOST_STARE_TIME = 0.5f;

    private GameObject _startRangeAnchor = null;
    public GameObject startRangeAnchor
    {
        set { _startRangeAnchor = value; }
        get {
            if (_startRangeAnchor == null || Vector3.Distance(_startRangeAnchor.transform.position, AgentBehaviour.Instance.transform.position) < 1.5f)
            {
                GameObject anchor = new GameObject("rangeAnchor");
                anchor.transform.position = AgentBehaviour.Instance.transform.position + Vector3.forward * 1.5f;
                _startRangeAnchor = anchor;
            }
            return _startRangeAnchor;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartFlow();
        }
    }
    
    public override void StartFlow()
    {
        GameObject defaultCursor = GameObject.Find("DefaultCursor");
        if (defaultCursor != null)
            defaultCursor.SetActive(false);

        base.StartFlow();
    }

    protected override IEnumerator CoMainFlow()
    {
        Debug.Log("Start Tutorial");

        AgentBehaviour.Instance.landPos = AgentBehaviour.Instance.transform.position;
        yield return StartCoroutine(TutorialIntro());
        AgentBehaviour.Instance.FloatUp();
        yield return StartCoroutine(TutorialBattle());
        AgentBehaviour.Instance.LandTo(AgentBehaviour.Instance.landPos);
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(TutorialEnding());
        AgentBehaviour.Instance.FloatUp();
        yield return new WaitForSeconds(2f);
        _stage = Stage.NONE;

        //this.tutorialManagerEvent.EndEvent();
        //yield return null;
    }

    IEnumerator TutorialIntro()
    {
        _stage = Stage.START;

        // TODO: cat meows
        yield return StartCoroutine(StagePlayerWatchCat());        
        yield return StartCoroutine(StagePlayerWalk());

        GameObject.Find("Radio0").GetComponent<RadioController>().TurnOff();
        AudioManager.Instance.FadeTo(Camera.main.transform.FindChild("Ambient").GetComponent<AudioSource>(), 0.3f, 10f);

        AgentBehaviour.Instance.LookAt(Camera.main.gameObject);
        for (int i = 0; i < AudioManager.Instance.clipVOSpeech.Length; i++)
        {
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOSpeech[i]));
        }
        yield return null;
    }

    IEnumerator StagePlayerWatchCat()
    {
        float stareTime = _AGENT_STARE_TIME;
        float t = 0f;
        float t1 = 0f;
        float meowTime = Random.value * 8f + 2f;
        int meowLen = AudioManager.Instance.clipVOMeow.Length;
        EyeDetector obj = AgentBehaviour.Instance;
        while (t < stareTime)
        {
            if (t1 < meowTime)
            {
                t1 += Time.deltaTime;
                if (t1 >= meowTime)
                {
                    int index = (int)(Random.value * meowLen);
                    Debug.Log("index = " + index);
                    t1 = 0f;
                    meowTime = Random.value * 8f + 2f;
                    meowTime += AudioManager.Instance.clipVOMeow[index].length;
                    Debug.Log("time = " + meowTime);
                    AgentBehaviour.Instance.Talk(AudioManager.Instance.clipVOMeow[index]);   
                }
            }
            if (obj.stareState == EyeDetector.StareState.STARED)
            {
                t += Time.deltaTime;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    IEnumerator StagePlayerWalk()
    {
        // greeting
        yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOGreeting[0]));
        yield return new WaitForSeconds(1f);
        AgentBehaviour.Instance.LookAt(Camera.main.gameObject);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOGreeting[1]));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOGreeting[2]));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOGreeting[3]));

        // wait for player to come closer
        float gap = Random.value * 5f + 5f;
        float t = 0;
        int lenCloser = AudioManager.Instance.clipVOComeCloser.Length;
        int indexCloser = 0;
        while (Vector3.Distance(Camera.main.transform.position, AgentBehaviour.Instance.transform.position)
            > Vector3.Distance(startRangeAnchor.transform.position, AgentBehaviour.Instance.transform.position))
        {
            if (t < gap)
            {
                t += Time.deltaTime;
            }
            if (t >= gap)
            {
                gap = Random.value * 5f + 3f;
                t = 0;
                // Talk
                AgentBehaviour.Instance.LookAt(Camera.main.gameObject);
                AgentBehaviour.Instance.Talk(AudioManager.Instance.clipVOComeCloser[indexCloser]);
                gap += AudioManager.Instance.clipVOComeCloser[indexCloser].length;
                indexCloser++;
                if (indexCloser == lenCloser) {
                    indexCloser -= 2;
                }
            }
            yield return null;
        }
        yield return null;
    }

    private WhiteGhost _ghost;

    IEnumerator TutorialBattle()
    {
        _stage = Stage.BATTLE;

        GhostManager.Instance.GenerateAGhost(1);
        _ghost = GhostManager.Instance.whiteGhostList[0];
        yield return StartCoroutine(StageFindGhost());

        SpellManager.Instance.stage = 0;
        SpellManager.Instance.canGenerate = true;

        yield return StartCoroutine(StageFindSpell());
        yield return StartCoroutine(StageKillGhost());
        _stage = Stage.FINISH;

        SpellManager.Instance.canGenerate = false;

        yield return null;
    }

    IEnumerator TutorialEnding()
    {
        yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOOhNoMoreOfThem));
        yield return null;
    }
    

    IEnumerator StageFindGhost()
    {
        float time = 0f;
        bool loopFinished = false;
        while (true)
        {
            // wait for ghost appear
            while (_ghost.ghostState == GhostState.HIDE)
            {
                if (time > 3f && _ghost.stareState == EyeDetector.StareState.STARED)
                {
                    AgentBehaviour.Instance.Talk(AudioManager.Instance.clipVOItsThere);
                    time = 0;
                }
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            if (loopFinished)
                break;
            // wait for player look away
            while (_ghost.stareState == EyeDetector.StareState.STARED)
            {
                yield return new WaitForEndOfFrame();
            }
            AgentBehaviour.Instance.Talk(AudioManager.Instance.clipVOStareAtIt);
            float t = 0;
            while (t < GHOST_STARE_TIME)
            {
                if (_ghost.ghostState == GhostState.HIDE)
                {
                    loopFinished = true;
                    break;
                }
                if (_ghost.stareState == EyeDetector.StareState.STARED)
                    t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            if (_ghost.ghostState == GhostState.HIDE)
            {
                loopFinished = true;
                continue;
            }
            else break;
        }
        yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOSeeYouCanStopIt));
        yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVOSpellCastTheSpell));
        yield return null;
    }

    IEnumerator StageFindSpell()
    {
        float time = 0f;

        // TODO: player saw the spell

        AgentBehaviour.Instance.Talk(AudioManager.Instance.clipVOShoutItToTheGhost);
        yield return null;
    }

    IEnumerator StageKillGhost()
    {
        while (_ghost != null && _ghost.ghostState != GhostState.KILLED)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    IEnumerator DefeatGhost()
    {
        /*
        while (_ghost != null && _ghost.ghostState != GhostState.KILLED)
        {
            // player failed
            if (_ghost.ghostState == GhostState.HIDE)
            {
                yield return StartCoroutine(AgentTalk(AudioManager.Instance.clipVO_lost));
            }
            yield return new WaitForEndOfFrame();
        }
        */
        yield return null;
    }
}
