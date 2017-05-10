using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ReactionType
{
    None                    = 0,
    PlayerGotAtk            = 21,
    GhostKillHint           = 20,
    GhostCloseHint          = 19,
    PlayerGotSpiderAtk      = 18,
    GhostActiveHint         = 10,
    SpellKillNothingHint    = 6,
    SpellToBatSpiderHint    = 7,
    SpellOutdatedHint       = 8
}

[DisallowMultipleComponent]
public class AgentReaction : MonoBehaviour {

    [SerializeField]
    private ReactionType _reactionType;
    [SerializeField]
    private int _priority;
    [SerializeField]
    private float _lifeTime;
    [SerializeField]
    private AudioClip _audio;
    [SerializeField]
    private float _waitBeforeFinish;

    private float _timer;
    private bool _hasFinished = false;

    public ReactionType reactionType { get { return this._reactionType; } }
    public int priority { get { return this._priority; } }

    #region Constructor

    public AgentReaction(ReactionType reactionType,
                         int priority,
                         float lifetime,
                         AudioClip audio,
                         float waitBeforeFinish)
    {
        this._reactionType = reactionType;

        if (priority < 0)
            throw new System.ArgumentOutOfRangeException("Expected priority >= 0");
        else
            this._priority = priority;

        if (lifetime < 0)
            throw new System.ArgumentOutOfRangeException("Expected lifetime > 0.0f");
        else
            this._lifeTime = lifetime;

        this._audio = audio;
        this._waitBeforeFinish = waitBeforeFinish;

        StartCoroutine(this.Timing());
    }

    #endregion Constructor


    protected IEnumerator Timing()
    {
        this._timer = 0.0f;
        while (this._timer < this._lifeTime)
        {
            this._timer += Time.deltaTime;
            yield return null;
        }
        this.FinishReaction();
    }

    public void StartReaction()
    {
        StopCoroutine(this.Timing());

        if (this._timer >= this._lifeTime)
        {
            this.FinishReaction();
            return;
        }

        StartCoroutine(this.CoExecuteReaction());
    }

    public void FinishReaction()
    {
        if (this._hasFinished)
            return;

        this._hasFinished = true;
        StopAllCoroutines();
        Destroy(this);
    }

    private IEnumerator CoExecuteReaction()
    {
        yield return StartCoroutine(this.CoExecuteReactionMain());

        yield return new WaitForSeconds(this._waitBeforeFinish);
        this.FinishReaction();
    }

    protected virtual IEnumerator CoExecuteReactionMain()
    {
        float length = AgentBehaviour.Instance.Talk(this._audio);
        yield return new WaitForSeconds(length);     
    }

    public virtual IEnumerator CoBreakReaction()
    {
        // TODO
        yield return null;
        this.FinishReaction();        
    }
}
