using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneFlow : CutSceneFlow
{
    protected override IEnumerator CoMainFlow()
    {
        yield return new WaitForSeconds(1f);
        AgentBehaviour.Instance.Land();
        //yield return StartCoroutine(PlayerWatchObject(AgentBehaviour.Instance, _AGENT_STARE_TIME));

        AudioClip[] audioClips = AudioManager.Instance.clipsVOEndCutScene;
        for(int i = 0; i < audioClips.Length; i++)
        {
            yield return StartCoroutine(this.AgentTalk(audioClips[i]));
            
        }
        yield return new WaitForSeconds(0.5f);
        AgentBehaviour.Instance.DisappearTo(GameObject.Find("LampHanging0").transform.position);
        AudioManager.Instance.FadeTo(Camera.main.transform.FindChild("Ambient").GetComponent<AudioSource>(), 0f, 5f);
    }
}
