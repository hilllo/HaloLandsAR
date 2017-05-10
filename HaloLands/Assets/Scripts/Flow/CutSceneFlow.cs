using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CutSceneFlow : Flow {

    protected static float _AGENT_STARE_TIME = 3f;

    protected IEnumerator PlayerWatchObject(EyeDetector obj, float stareTime)
    {
        float t = 0;
        while (t < stareTime)
        {
            if (obj.stareState == EyeDetector.StareState.STARED)
            {
                t += Time.deltaTime;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    protected IEnumerator AgentTalk(AudioClip clip)
    {
        if (clip == null)
            yield break;

        AgentBehaviour.Instance.Talk(clip);
        yield return new WaitForSeconds(clip.length);
    }

}
