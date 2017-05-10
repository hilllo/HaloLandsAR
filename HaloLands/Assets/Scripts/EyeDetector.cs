using System.Collections;
using System.Collections.Generic;
using UnityEngine;


abstract public class EyeDetector : MonoBehaviour {

    public enum StareState
    {
        NONE,
        STARED,
        VISITED
    }
    public StareState stareState = StareState.NONE;
    private bool _isVisited = false;

    protected virtual void Awake()
    {
        EyeTrigger trigger = GetComponent<EyeTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EyeTrigger>();
        }
        trigger.onViewEnter += OnViewEnter;
        trigger.onViewExit += OnViewExit;
        trigger.onStared += OnStared;
        trigger.onNotStared += OnNotStared;
    }

    protected virtual void OnViewEnter()
    {
        _isVisited = true;
        stareState = StareState.STARED;
    }

    protected virtual void OnViewExit() {
        stareState = _isVisited ? StareState.VISITED : StareState.NONE;
    }

    protected virtual void OnStared() {
        stareState = StareState.STARED;
    }
    protected virtual void OnNotStared() {
        stareState = _isVisited ? StareState.VISITED : StareState.NONE;
    }
}
