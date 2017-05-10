using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity;

[DisallowMultipleComponent]
public class SpellManager : Game.Manager.Manager<SpellManager>, IInputClickHandler
{

    #region Fields

    public List<GameObject> spawnPointGameObj;

    [SerializeField]
    private KeywordManager _keywordManager;
    
    [SerializeField]
    private Spell Spell;

    [SerializeField]
    private float _secBetweenSpells;

    [SerializeField]
    private int _killNothingThreshold;

    [SerializeField]
    private int _spellCount;

    [SerializeField]
    private int _spellStartIndex;

    [SerializeField]
    private List<string> _keywords;

    private int _currentIndex = -1;

    private int _lastIndex = -1;

    private bool _canGenerate = false;

    private bool _isGenerating = false;

    private int _killNothingCount = 0;

    private int _stage = 0;

    private List<AudioClip> _spellOutdatedHintVOClips;
    private int _spellOutdatedHintVOIndex = 0;

    private List<AudioClip> _spellToBatSpiderHintVOClips;
    private int _spellToBatSpiderHintVOIndex = 0;

    private List<AudioClip> _spellKillNothingHintVOClips;
    private int _spellKillNothingHintVOIndex = 0;

    #endregion Fields

    #region Properties
    private bool firstTime = true;

    public bool canGenerate
    {
        get
        {
            return this._canGenerate;
        }
        set
        {
            if (this._canGenerate == value)
                return;

            Debug.Log("can generate: " + value);
            this._canGenerate = value;

            if (value && !this.Spell.hasBeenActivated)
            {
                if (firstTime)
                {
                    InputManager.Instance.PushModalInputHandler(gameObject);
                    //Debug.Log("Pushedin stack");
                    firstTime = false;
                }
                this._lastIndex = -1;                
                this.UpdateSpell();
            }
            else
                this.Spell.Activate(false);
        }
    }

    public int stage
    {
        get
        {
            return this._stage;
        }
        set
        {
            if (this._stage == value)
                return;

            this._stage = value;

            this._spellOutdatedHintVOIndex = 0;
            this._spellToBatSpiderHintVOIndex = 0;
            this._spellKillNothingHintVOIndex = 0;            
        }
    }

    #endregion Properties

    protected override void Start()
    {
        base.Start();
        this._keywords = new List<string>(this._spellCount);
        for(int i = 0; i < this._spellCount; i++)
        {
            int index = i + this._spellStartIndex;
            if (index >= this._keywordManager.KeywordsAndResponses.Length)
                break;
            this._keywords.Add(this._keywordManager.KeywordsAndResponses[index].Keyword);
        }

        this._spellOutdatedHintVOClips = AudioClipFactory.GetVOClipsWithReactionType(ReactionType.SpellOutdatedHint);
        this._spellToBatSpiderHintVOClips = AudioClipFactory.GetVOClipsWithReactionType(ReactionType.SpellToBatSpiderHint);
        this._spellKillNothingHintVOClips = AudioClipFactory.GetVOClipsWithReactionType(ReactionType.SpellKillNothingHint);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        this.UpdateSpell(this._currentIndex);
    }

    public void UpdateSpell(int index)
    {
        if(index == 0)
        {
            StartCoroutine(this.CoUpdateSpell(true));
            return;
        }

        if (this._stage != 0 && this._currentIndex != index && this._lastIndex == index)
        {
            int index01 = UnityEngine.Random.Range(0,2);
            if (ReactionManager.Instance.Register(new List<Reaction> { new TalkReaction(this._spellOutdatedHintVOClips[index01]) }, ReactionType.SpellOutdatedHint))
            {
                //if (this._spellOutdatedHintVOIndex < this._spellOutdatedHintVOClips.Count - 1)
                //    this._spellOutdatedHintVOIndex++;
                //else
                //    this._spellOutdatedHintVOIndex = 0;
            }
            return;
        }
        else
            this._spellOutdatedHintVOIndex = 0;

        if (!this.canGenerate || this._currentIndex != index || this._isGenerating)
            return;

        int returnValue = GhostManager.Instance.DispellGhosts(this._currentIndex);

        if (returnValue <= 0)
        {
            this._killNothingCount++;
            if(returnValue < 0)
            {
                if(ReactionManager.Instance.Register(new List<Reaction> { new TalkReaction(this._spellToBatSpiderHintVOClips[this._spellToBatSpiderHintVOIndex]) }, ReactionType.SpellToBatSpiderHint))
                {
                    if (this._spellToBatSpiderHintVOIndex < this._spellToBatSpiderHintVOClips.Count - 1)
                        this._spellToBatSpiderHintVOIndex++;
                    else
                        this._spellToBatSpiderHintVOIndex = 0;
                }
            }
            else if (this._killNothingCount > this._killNothingThreshold)
            {
                if(ReactionManager.Instance.Register(new List<Reaction> { new TalkReaction(this._spellKillNothingHintVOClips[this._spellKillNothingHintVOIndex]) }, ReactionType.SpellKillNothingHint))
                {
                    if (this._spellKillNothingHintVOIndex < this._spellKillNothingHintVOClips.Count - 1)
                        this._spellKillNothingHintVOIndex++;
                    else
                        this._spellKillNothingHintVOIndex = 0;
                }
            }
        }
        else
        {
            this._killNothingCount = 0;
            this._spellToBatSpiderHintVOIndex = 0;
            this._spellKillNothingHintVOIndex = 0;
        }

        if (this._stage != 0)
            StartCoroutine(this.CoUpdateSpell());
    }

    private void UpdateSpell()
    {
        if (!this.canGenerate || this._isGenerating)
            return;

        StartCoroutine(this.CoUpdateSpell());
    }

    private IEnumerator CoUpdateSpell(bool hack = false)
    {
        this._isGenerating = true;
        if (this.Spell.hasBeenActivated)
        {
            this.Spell.Activate(false);
            yield return new WaitForSeconds(this._secBetweenSpells);
        }

        if (!this.canGenerate)
        {
            this._isGenerating = false;
            yield break;
        }

        // Generate new spell
        int startIndex = 1;
        int endIndex = 1;
        switch (this._stage)
        {
            case 0:
                startIndex = 1;
                endIndex = 2;
                break;
            case 1:
                startIndex = 1;
                endIndex = 4;
                break;
            case 2:
                startIndex = 4;
                endIndex = 7;
                break;
            case 3:
                startIndex = 7;
                endIndex = 10;
                break;
            default:
                startIndex = 1;
                endIndex = this._spellCount;
                break;
        }
        this._lastIndex = this._currentIndex;
        while (this._currentIndex == this._lastIndex)
        {
            this._currentIndex = UnityEngine.Random.Range(startIndex, endIndex);
        }
        this.Spell.text = this._keywords[this._currentIndex];

        // Find spawn point
        if (this.spawnPointGameObj == null)
            throw new System.ArgumentNullException("Expected spawnPointGameObj to be set.");
        if(this._killNothingCount <= 0 || !hack)
        {
            int index = UnityEngine.Random.Range(0, this.spawnPointGameObj.Count);
            this.Spell.transform.SetParent(this.spawnPointGameObj[index].transform);
            this.Spell.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            this.Spell.transform.localPosition = new Vector3(0f, 0f, 0f);
        }

        if (this.canGenerate)
            this.Spell.Activate(true);

        this._isGenerating = false;
    }

    #region Instance

    protected override void ReleaseInstance()
    {
        SpellManager.Instance = null;
    }

    protected override void SetInstance()
    {
        SpellManager.Instance = this;
    }

    #endregion Instance
}
