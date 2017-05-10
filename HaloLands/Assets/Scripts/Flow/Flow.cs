using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Flow : MonoBehaviour
{
    /// <summary>
    /// Call EndEvent() for these Events when completing this flow.
    /// </summary>
    [SerializeField]
    protected List<Game.Event.Event> EndEvents = new List<Game.Event.Event>();

    public virtual void StartFlow()
    {
        StartCoroutine(this.CoFlow());
    }

    protected IEnumerator CoFlow()
    {
        yield return StartCoroutine(this.CoMainFlow());

        for(int i = 0; i < this.EndEvents.Count; i++)
        {
            this.EndEvents[i].EndEvent();
        }
    }

    protected abstract IEnumerator CoMainFlow();
}


