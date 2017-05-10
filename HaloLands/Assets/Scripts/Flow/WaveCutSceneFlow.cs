using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCutSceneFlow : CutSceneFlow {

    private enum WaveCutSceneType
    {
        Wave_1_2,
        Wave_2_3
    }

    [SerializeField]
    private WaveCutSceneType waveCutSceneType;

    private AudioClip cutSceneAudioClip
    {
        get
        {
            if (this.waveCutSceneType == WaveCutSceneType.Wave_1_2)
                return AudioManager.Instance.clipVOWave12CutScene;

            if (this.waveCutSceneType == WaveCutSceneType.Wave_2_3)
                return AudioManager.Instance.clipVOWave23CutScene;

            return null;
        }
    }

    protected override IEnumerator CoMainFlow()
    {
        yield return new WaitForSeconds(1f);
        AgentBehaviour.Instance.Land();
        //yield return StartCoroutine(PlayerWatchObject(AgentBehaviour.Instance, _AGENT_STARE_TIME));
        yield return StartCoroutine(this.AgentTalk(this.cutSceneAudioClip));
        AgentBehaviour.Instance.FloatUp();
        yield return new WaitForSeconds(0.5f);
    }

}
