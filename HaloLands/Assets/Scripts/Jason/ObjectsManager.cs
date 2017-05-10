using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;

public class ObjectsManager : Game.Singleton<ObjectsManager> {

    [System.Serializable]
    public struct RoomObject
    {
        public string name;
        public int count;
        public GameObject SetupPrefab;
        public GameObject GamePrefab;
    }

    [System.Serializable]
    public struct RoomObjectSet
    {
        public string name;
        public RoomObject[] set;
    }
    
    
    public List<GameObject> SetupObjectsInstances;
    
    public List<GameObject> GameObjectsInstances;

    public RoomObjectSet[] RoomObjectSets;

    public delegate void InstantiateGameObjectsFinished();
    public InstantiateGameObjectsFinished instantiateGameObjectsFinishedCallback;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    

    public void InstantiateSetupRoomObjects()
    {
        StartCoroutine(InstantiateRoomObjectsCoroutine());
    }

    public void InstantiateSetupGameObjects()
    {
        StartCoroutine(InstantiateGameObjectsCoroutine());
    }

    public void DeleteSetupRoomObjectsAnchors()
    {
        foreach(GameObject roomObject in SetupObjectsInstances)
        {
            WorldAnchorManager.Instance.RemoveAnchor(roomObject);
            WorldAnchorManager.Instance.DeleteAnchorfromAnchorStore(roomObject.name);
        }
    }

    IEnumerator InstantiateRoomObjectsCoroutine()
    {
        SetupObjectsInstances = new List<GameObject>();
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward;
        float startTime = Time.time;
        for (int i=0; i< RoomObjectSets.Length; i++)
        {
            for(int j=0; j<RoomObjectSets[i].set.Length; j++)
            {
                for(int n=0; n<RoomObjectSets[i].set[j].count; n++)
                {
                    GameObject instance = Instantiate(RoomObjectSets[i].set[j].SetupPrefab, pos, Quaternion.identity);
                    instance.name = RoomObjectSets[i].set[j].name + n.ToString();
                    WorldAnchorManager.Instance.AttachAnchor(instance, instance.name);
                    SetupObjectsInstances.Add(instance);
                    if(Time.time - startTime > 0.01f)
                    {
                        startTime = Time.time;
                        yield return null;
                    }
                }
            }
        }
        yield return null;
    }

    IEnumerator InstantiateGameObjectsCoroutine()
    {
        GameObjectsInstances = new List<GameObject>();
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward;
        float startTime = Time.time;
        for (int i = 0; i < RoomObjectSets.Length; i++)
        {
            for (int j = 0; j < RoomObjectSets[i].set.Length; j++)
            {
                for (int n = 0; n < RoomObjectSets[i].set[j].count; n++)
                {
                    GameObject instance = Instantiate(RoomObjectSets[i].set[j].GamePrefab, pos, Quaternion.identity);
                    instance.name = RoomObjectSets[i].set[j].name + n.ToString();
                    WorldAnchorManager.Instance.AttachAnchor(instance, instance.name);
                    GameObjectsInstances.Add(instance);
                    if (Time.time - startTime > 0.01f)
                    {
                        startTime = Time.time;
                        yield return null;
                    }
                }
            }
        }
        if(instantiateGameObjectsFinishedCallback != null)
        {
            instantiateGameObjectsFinishedCallback();
        }
        yield return null;
        
    }

    public void FinishedPositioningRoomObjects()
    {
        for (int i = 0; i < SetupObjectsInstances.Count; i++)
        {
            WorldAnchorManager.Instance.DeleteAnchorfromAnchorStore(SetupObjectsInstances[i].name);
            WorldAnchorManager.Instance.AttachAnchor(SetupObjectsInstances[i], SetupObjectsInstances[i].name);
            SetupObjectsInstances[i].GetComponent<HandDraggable>().IsDraggingEnabled = false;
        }
    }

    public void FinishedAllSetup()
    {
        for (int i = 0; i < SetupObjectsInstances.Count; i++)
        {
            WorldAnchorManager.Instance.RemoveAnchor(SetupObjectsInstances[i]);
            Destroy(SetupObjectsInstances[i]);
        }
    }
}
