using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;


public class SpecViewManager : Game.Singleton<SpecViewManager> {

    public enum SceneStage
    {
        Starting = 0,
        WaitingForAnchor,
        WaitingForStageTransform,
        Ready,
    }

    public SceneStage currentStage = SceneStage.Starting;

	// Use this for initialization
	void Start () {
        currentStage = SceneStage.WaitingForAnchor;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
