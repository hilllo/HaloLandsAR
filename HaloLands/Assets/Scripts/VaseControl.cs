using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaseControl : Game.Singleton<VaseControl> {
    public GameObject original, broken;
    // Use this for initialization
    int  nearestSpawnPointIndex=-1;
    void Start () {
        
        float minDistance = float.MaxValue;
        foreach(GameObject spoint in GhostManager.Instance.whiteGhostPositions)
        {
            float tmpdis = Vector3.Distance(transform.position, spoint.transform.position);
            if ( tmpdis< minDistance)
            {
                minDistance = tmpdis;
                nearestSpawnPointIndex= GhostManager.Instance.whiteGhostPositions.IndexOf(spoint);
            }
        }
        GhostManager.Instance.vaseIndex = nearestSpawnPointIndex;
	}
    public void Break()
    {
        original.SetActive(false);
        broken.SetActive(true);
    }
    // Update is called once per frame
    void Update () {
		
	}
}
