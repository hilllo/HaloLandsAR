using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using HoloToolkit.Unity;
using Game.Event;


public enum GameMode
{
    Init,
    SetupRoomMode,
    SetupGameMode,
    TutorialMode,
    GameFlowMode,
    EndMode
}

public class GameManager : Game.Singleton<GameManager> {

    private GameMode _gameMode = GameMode.Init;

    [SerializeField]
    private ManagerEvent setupRoomEvent;

    [SerializeField]
    private ManagerEvent setupGameEvent;

    public GameMode gameMode
    {
        get
        {
            return this._gameMode;
        }
        set
        {
            if (this._gameMode == value)
                return;

            Debug.Log(string.Format("GameMode: {0}", value.ToString()));
            this._gameMode = value;
        }
    }


    // Use this for initialization
    void Start () {

#if UNITY_WSA && !UNITY_EDITOR
        GameObject.Find("EditorDebug").SetActive(false);
#endif

        StartCoroutine(GameFlow());
	}
    

    public void SetupMode()
    {
        if(gameMode == GameMode.Init)
        {
            //Debug.Log("Setup Mode");
            gameMode = GameMode.SetupRoomMode;

            
        }
    }

    public void SetupRoomFinished()
    {
        if (gameMode == GameMode.SetupRoomMode)
        {

            //Start Game
            this.setupRoomEvent.EndEvent();
        }
    }

    public void SetupGameFinished()
    {
        //Start Game
        this.setupGameEvent.EndEvent();
    }

    IEnumerator GameFlow()
    {
        //Logo

        SpriteRenderer logoSprite = GameObject.Find("Logo").GetComponentInChildren<SpriteRenderer>();
        float percent = 0;
        while (percent <= 1)
        {
            percent += Time.deltaTime * 1f;
            logoSprite.color = new Color(percent, percent, percent, 1);
            yield return null;
        }
        yield return new WaitForSeconds(3f);
        
        while (percent >= 0)
        {
            percent -= Time.deltaTime * 1f;
            logoSprite.color = new Color(percent, percent, percent, 1);
            yield return null;
        }
        Destroy(logoSprite.transform.parent.gameObject);

        //Go into game or setup
        SetupManager.Instance.setupRoomCompleteCallback += SetupRoomFinished;
        SetupManager.Instance.setupGameCompleteCallback += SetupGameFinished;


        //Save new anchor
        WorldAnchorManager.Instance.AttachAnchor(SpatialUnderstanding.Instance.gameObject, "HalolandsWorld");

        if (gameMode == GameMode.SetupRoomMode)
        {
            //Start Setup Room
            this.setupRoomEvent.StartEvent();
        }
        else
        {

            //Start Setup Game
            this.setupGameEvent.StartEvent();
        }

        yield return null;
    }
}
