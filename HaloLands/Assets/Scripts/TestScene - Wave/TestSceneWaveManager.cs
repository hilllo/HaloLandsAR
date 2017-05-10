using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSceneWaveManager : Game.Manager.Manager<TestSceneWaveManager> {

    protected override void ReleaseInstance()
    {
        TestSceneWaveManager.Instance = null;
    }

    protected override void SetInstance()
    {
        TestSceneWaveManager.Instance = this;
    }

}
