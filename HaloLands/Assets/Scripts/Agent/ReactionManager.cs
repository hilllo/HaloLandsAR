using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionManager : Game.Singleton<ReactionManager>
{
    Queue<Reaction> queue = new Queue<Reaction>();

    Reaction currentReaction = null;
    ReactionType currentType;

    // if reaction list contains any MoveReaction
    // lock random move
    bool isMoveLocked = false; 

    public bool Register(List<Reaction> list, ReactionType type)
    {
        if (currentReaction != null || queue.Count > 0)
        {
            if (type > currentType)
                Break();
            else
                return false;
        }
        currentType = type;
        if (list.Count == 0)
            return true;
        if (list[0] is TalkReaction)
        {
            Add(new MoveReaction(AgentBehaviour.Instance.anchor));
        }
        foreach (Reaction reaction in list)
        {
            Add(reaction);
            if (reaction is MoveReaction)
                isMoveLocked = true;
        }
        return true;
    }

    void Add(Reaction reaction)
    {
        queue.Enqueue(reaction);
    }

    private void Break()
    {
        queue.Clear();
        currentReaction.Break();
        if (currentReaction is TalkReaction)
        {
            // TODO: Add(new TalkReaction(connect_clip));
        }
        currentReaction = null;
    }

    void Update()
    {
        // UseCaseSample();

        if (currentReaction != null)
        {
            if (!currentReaction.Run())
            {
                currentReaction = null;
            }
        }
        if (currentReaction == null && queue.Count > 0)
        {
            currentReaction = queue.Dequeue();
            currentReaction.Start();
        }
        if (currentReaction == null && queue.Count == 0 && isMoveLocked)
            isMoveLocked = false;
    }

    void UseCaseSample()
    {
        ReactionManager reactionManager = this;
        AudioClip clip1 = AudioManager.Instance.clipVOItsThere;
        AudioClip clip2 = AudioManager.Instance.clipVOBehindYou;

        // Single reaction (priority 1):
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("Reaction 1");
            reactionManager.Register(new List<Reaction>{
                new TalkReaction(clip1)
            }, ReactionType.GhostActiveHint);
        }

        // Multiple reaction (priority 8):
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("Reaction 8");
            reactionManager.Register(new List<Reaction>{
                new MoveReaction(Vector3.zero),
                new TalkReaction(clip2),
                new MoveReaction(new Vector3(3f, 0f, 0f))
            }, ReactionType.SpellToBatSpiderHint);
        }

        // Single reaction (priority 16):
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log("Reaction 16");
            reactionManager.Register(new List<Reaction>{
                new TalkReaction(clip2)
            }, ReactionType.SpellOutdatedHint);
        }
    }
}
