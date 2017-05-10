using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Reaction {
    void Start();
    bool Run();
    void Break();
}

public class TalkReaction : Reaction
{
    AudioClip clip;

    public TalkReaction(AudioClip clip)
    {
        this.clip = clip;
    }

    public void Start()
    {
        AgentBehaviour.Instance.Talk(clip);
    }

    public bool Run()
    {
        return AgentBehaviour.Instance.isTalking(clip);
    }

    public void Break()
    {
        AgentBehaviour.Instance.StopTalk();
    }
}

public class MoveReaction : Reaction
{
    GameObject target = null;
    Vector3 targetPos;

    public MoveReaction(Vector3 targetPos)
    {
        this.targetPos = targetPos;
    }

    public MoveReaction(GameObject target)
    {
        this.target = target;
    }

    public void Start()
    {
        if (target != null)
            AgentBehaviour.Instance.SetTarget(target);
        else 
            target = AgentBehaviour.Instance.SetTarget(targetPos);
    }

    public bool Run()
    {
        GameObject curTarget = AgentBehaviour.Instance.GetTarget();
        return curTarget != null && target == curTarget;
    }

    public void Break()
    {

    }
}

public class WaitForLookReaction : Reaction
{
    public void Start() { }

    public bool Run()
    {
        // TODO: audio hint to look at cat
        return AgentBehaviour.Instance.stareState != EyeDetector.StareState.STARED;
    }

    public void Break() { }
}

public class BreakReaction : Reaction
{
    public void Start() { }

    public bool Run() {
        return false;
    }

    public void Break() { }
}