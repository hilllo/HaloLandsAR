using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using System;

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;


public class SetupManager : Game.Singleton<SetupManager>, IInputClickHandler
{

    public GameObject StatusUIPrefab;
    public Material OcculusionMaterial,ScanningMaterial;
    private Material currentMaterial;

    GameObject statusUI;

    public delegate void SetupRoomCompleteDelegate();
    public SetupRoomCompleteDelegate setupRoomCompleteCallback;

    public delegate void SetupGameCompleteDelegate();
    public SetupGameCompleteDelegate setupGameCompleteCallback;

    public enum SetupState
    {
        Init,
        PrepareScan,
        Scanning,
        PrepareObjectsPlacing,
        ObjectsPlacing,
        PrepareNavMap,
        NavMapping,
        Finished,
    }

    public SetupState setupState;

    public void SetupRoom()
    {
        if(setupState == SetupState.Init)
        {
            ShowSetupUI();
            GotoNextSetupState();
        }
    }

    //Say 'Begin'
    public void Begin()
    {
        switch (setupState)
        {
            case SetupState.Init:
                break;
            case SetupState.Scanning:
                break;
            case SetupState.ObjectsPlacing:
                break;
            case SetupState.NavMapping:
                break;
            default:
                GotoNextSetupState();
                break;
        }
    }

    //Say 'Finish'
    public void Finish()
    {
        switch (setupState)
        {
            case SetupState.PrepareScan:
                GotoNextSetupState(true);
                break;
            case SetupState.Scanning:
                FinishScanning();
                break;
            case SetupState.PrepareObjectsPlacing:
                GotoNextSetupState(true);
                break;
            case SetupState.ObjectsPlacing:
                FinishObjectsPlacing();
                break;
            case SetupState.PrepareNavMap:
                GotoNextSetupState(true);
                break;
            case SetupState.NavMapping:
                FinishNavMapping();
                break;
            default:
                break;
        }
    }
    
    //Setup Game
    public void SetupGame()
    {
        LoadRoomMesh();
    }


    /**************************Setup Room****************************/
    //State
    void GotoNextSetupState(bool skipOne = false)
    {
        switch (setupState)
        {
            case SetupState.Init:
                //Pre scan
                UpdateStatus("Finished");
                setupState = SetupState.PrepareScan;
                ShowPreviousScanning();
                UpdateStatus("Showing previous scan");
                break;
            case SetupState.PrepareScan:
                if (skipOne)
                {
                    //Prepare Object placing
                    UpdateStatus("Finished");
                    setupState = SetupState.PrepareObjectsPlacing;
                    UpdateStatus("Showing previous objects placing");
                    ShowPreviousObjectsPlacing();
                }
                else
                {
                    //Scan
                    UpdateStatus("Scanning");
                    setupState = SetupState.Scanning;
                    HidePreviousScanning();
                    StartScanning();
                }
                break;
            case SetupState.Scanning:
                //Prepare Objects Placing
                UpdateStatus("Finished");
                setupState = SetupState.PrepareObjectsPlacing;
                ShowPreviousObjectsPlacing();
                UpdateStatus("Showing previous objects placing");
                break;
            case SetupState.PrepareObjectsPlacing:
                if (skipOne)
                {
                    //Prepare Nav mapping
                    UpdateStatus("Finished");
                    setupState = SetupState.PrepareNavMap;
                    ShowPreviousNavMapping();
                    UpdateStatus("Showing previous nav mapping");
                }
                else
                {
                    //Object placing
                    UpdateStatus("Placing");
                    setupState = SetupState.ObjectsPlacing;
                    StartObjectsPlacing();
                }
                break;
            case SetupState.ObjectsPlacing:
                //Prepare Nav Mapping
                UpdateStatus("Finished");
                setupState = SetupState.PrepareNavMap;
                ShowPreviousNavMapping();
                UpdateStatus("Showing previous nav mapping");
                break;
            case SetupState.PrepareNavMap:
                if (skipOne)
                {
                    //Finish
                    UpdateStatus("Finished");
                    setupState = SetupState.Finished;
                    FinishSetupMode();
                    UpdateStatus("Finished");
                }
                else
                {
                    //Nav Mapping
                    UpdateStatus("Mapping");
                    setupState = SetupState.NavMapping;
                    HidePreviousNavMapping();
                    StartNavMapping();
                }
                break;
            case SetupState.NavMapping:
                //Finish
                UpdateStatus("Finished");
                setupState = SetupState.Finished;
                FinishSetupMode();
                UpdateStatus("Finished");
                break;
            case SetupState.Finished:
                break;
        }
    }

    //UI
    void ShowSetupUI()
    {
        if (!Camera.main.transform.FindChild("SetupStatusUI(Clone)"))
        {
            statusUI = GameObject.Instantiate(StatusUIPrefab);
            statusUI.transform.parent = Camera.main.transform;
            statusUI.transform.localPosition = new Vector3(0, 0, 1.2f);
        }
        else
        {
            statusUI = Camera.main.transform.FindChild("SetupStatusUI(Clone)").gameObject;
        }
    }

    void HideSetupUI()
    {
        if (statusUI)
        {
            Destroy(statusUI);
        }
    }
    

    //Scanning
    void ShowPreviousScanning()
    {
        currentMaterial = ScanningMaterial;
        
        StartCoroutine(LoadRoomMeshCoroutine());
    }

    void HidePreviousScanning()
    {
        if (MeshLoadSave.Instance.loadMeshCallback != null)
        {
            MeshLoadSave.Instance.loadMeshCallback -= LoadMeshFinished;
        }

        DeleteScanningMesh();
    }

    void DeleteScanningMesh()
    {
        SpatialUnderstanding.Instance.UnderstandingCustomMesh.ClearSurfaceObjects();
        if (SpatialUnderstanding.Instance.transform.GetComponentsInChildren<MeshRenderer>()!=null)
        {
            foreach (MeshRenderer mr in SpatialUnderstanding.Instance.transform.GetComponentsInChildren<MeshRenderer>())
            {
                DestroyImmediate(mr.transform.gameObject);
            }
        }
    }

    void StartScanning()
    {
        WorldAnchorManager.Instance.RemoveAnchor(SpatialUnderstanding.Instance.gameObject);
        WorldAnchorManager.Instance.DeleteAnchorfromAnchorStore("HalolandsWorld");
        SpatialUnderstanding.Instance.gameObject.transform.position = Vector3.zero;
        SpatialUnderstanding.Instance.gameObject.transform.rotation = Quaternion.identity;
        SpatialUnderstanding.Instance.RequestBeginScanning();
        Invoke("DelayAttachHalolandsAnchor", 0.5f);
    }

    void DelayAttachHalolandsAnchor()
    {
        WorldAnchorManager.Instance.AttachAnchor(SpatialUnderstanding.Instance.gameObject, "HalolandsWorld");
    }

    void FinishScanning()
    {
        UpdateStatus("Finishing Scan");
        SpatialUnderstanding.Instance.RequestFinishScan();
        SaveScanning();
    }

    void SaveScanning()
    {
        StartCoroutine(SaveScanningCoroutine());
    }

    IEnumerator SaveScanningCoroutine()
    {

        //Get Mesh Data
        while (true)
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done && SpatialUnderstanding.Instance.UnderstandingCustomMesh.IsImportActive == false)
            {
                break;
            }
            yield return null;
        }

        SpatialUnderstandingDll dll = SpatialUnderstanding.Instance.UnderstandingDLL;

        Vector3[] meshVertices = null;
        Vector3[] meshNormals = null;
        Int32[] meshIndices = null;

        // Pull the mesh - first get the size, then allocate and pull the data
        int vertCount;
        int idxCount;

        if ((SpatialUnderstandingDll.Imports.GeneratePlayspace_ExtractMesh_Setup(out vertCount, out idxCount) > 0) &&
            (vertCount > 0) &&
            (idxCount > 0))
        {
            meshVertices = new Vector3[vertCount];
            IntPtr vertPos = dll.PinObject(meshVertices);
            meshNormals = new Vector3[vertCount];
            IntPtr vertNorm = dll.PinObject(meshNormals);
            meshIndices = new Int32[idxCount];
            IntPtr indices = dll.PinObject(meshIndices);

            SpatialUnderstandingDll.Imports.GeneratePlayspace_ExtractMesh_Extract(vertCount, vertPos, vertNorm, idxCount, indices);
        }

        // Wait a frame
        yield return null;

        if(MeshLoadSave.Instance.saveMeshCallback != null)
        {
            MeshLoadSave.Instance.saveMeshCallback -= SaveMeshFinished;
        }
        MeshLoadSave.Instance.saveMeshCallback += SaveMeshFinished;

        //Save Mesh Data
        MeshLoadSave.Instance.SaveRoomMesh(meshVertices, meshNormals, meshIndices);


        yield return null;
    }


    void SaveMeshFinished(bool finished)
    {
        if (finished)
        {
            //Scanning complete
            GotoNextSetupState();
        }
    }

    //ObjectsPlacing
    void ShowPreviousObjectsPlacing()
    {
        ObjectsManager.Instance.InstantiateSetupRoomObjects();
    }

    void StartObjectsPlacing()
    {
        ObjectsManager.Instance.DeleteSetupRoomObjectsAnchors();
    }

    void FinishObjectsPlacing()
    {
        ObjectsManager.Instance.FinishedPositioningRoomObjects();
        GotoNextSetupState();
    }


    //NavMapping
    void ShowPreviousNavMapping()
    {
        LoadNavMap(true);
    }

    void HidePreviousNavMapping()
    {
        NavMapManager.Instance.DeleteNavMap();
    }

    void StartNavMapping()
    {
        if (NavMapManager.Instance.navLaodFinishedDelegate != null)
        {
            NavMapManager.Instance.navLaodFinishedDelegate -= LoadNavMapFinished;
        }
        NavMapManager.Instance.navSetupFinishedDelegate += NavMapSetupFinished;
        NavMapManager.Instance.StartMakingNavMap();
        InputManager.Instance.PushModalInputHandler(gameObject);
    }

    void UpdateNavPoint(Vector3 cursorPosition)
    {
        NavMapManager.Instance.UpdatePathPoint(cursorPosition);
    }

    void FinishNavMapping()
    {
        UpdateStatus("Saving");
        InputManager.Instance.PopModalInputHandler();
        NavMapManager.Instance.FinishPathMap();
    }

    public void NavMapSetupFinished()
    {
        GotoNextSetupState();
    }

    //Finish

    void FinishSetupMode()
    {
        ObjectsManager.Instance.FinishedAllSetup();

        statusUI.SetActive(false);

        DeleteScanningMesh();

        if (setupRoomCompleteCallback != null)
        {
            setupRoomCompleteCallback();
        }

    }

    //UI
    void UpdateStatus(string status)
    {
        switch (setupState)
        {
            case SetupState.Init:
                TextMesh setupUI = statusUI.transform.FindChild("SetupStatus").GetComponentInChildren<TextMesh>();
                setupUI.text = "Init - " + status;
                break;
            case SetupState.PrepareScan:
                TextMesh prescanUI = statusUI.transform.FindChild("ScanStatus").GetComponentInChildren<TextMesh>();
                prescanUI.text = "Scan:" + status;
                break;
            case SetupState.Scanning:
                TextMesh scanUI = statusUI.transform.FindChild("ScanStatus").GetComponentInChildren<TextMesh>();
                scanUI.text = "Scan:" + status;
                break;
            case SetupState.PrepareObjectsPlacing:
                TextMesh preObjectsUI = statusUI.transform.FindChild("ObjectsStatus").GetComponentInChildren<TextMesh>();
                preObjectsUI.text = "Objects: " + status;
                break;
            case SetupState.ObjectsPlacing:
                TextMesh objectsUI = statusUI.transform.FindChild("ObjectsStatus").GetComponentInChildren<TextMesh>();
                objectsUI.text = "Objects: " + status;
                break;
            case SetupState.PrepareNavMap:
                TextMesh preNavUI = statusUI.transform.FindChild("NavMappingStatus").GetComponentInChildren<TextMesh>();
                preNavUI.text = "Map:" + status;
                break;
            case SetupState.NavMapping:
                TextMesh navUI = statusUI.transform.FindChild("NavMappingStatus").GetComponentInChildren<TextMesh>();
                navUI.text = "Map:" + status;
                break;
            case SetupState.Finished:
                TextMesh finishUI = statusUI.transform.FindChild("SetupStatus").GetComponentInChildren<TextMesh>();
                finishUI.text = "Setup: " + status;
                break;
        }
    }


    //Interaction
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (setupState == SetupState.NavMapping)
        {
            Vector3 hitPosition = GazeManager.Instance.HitPosition;

            hitPosition -= SpatialUnderstanding.Instance.gameObject.transform.position;
            UpdateNavPoint(hitPosition);
        }
    }


    /***************************Setup Game******************************/
    //Loading Room Mesh

    void LoadRoomMesh()
    {
#if UNITY_WSA && !UNITY_EDITOR
        currentMaterial = OcculusionMaterial;
        StartCoroutine(LoadRoomMeshCoroutine());
#else
        LoadGameObjects();
#endif
    }

    IEnumerator LoadRoomMeshCoroutine()
    {
        
        if (MeshLoadSave.Instance.loadMeshCallback != null)
        {
            MeshLoadSave.Instance.loadMeshCallback -= LoadMeshFinished;//Check double adding
        }
        MeshLoadSave.Instance.loadMeshCallback += LoadMeshFinished;

        MeshLoadSave.Instance.LoadRoomMesh();

        yield return null;
    }

    void LoadMeshFinished(bool success, Vector3[] meshVertices, Vector3[] meshNormals, Int32[] meshIndices)
    {
        if (success)
        {
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.CreateRoomMesh(meshVertices, meshNormals, meshIndices, currentMaterial);

            if(setupState == SetupState.Init || setupState == SetupState.Finished)
            {
                //Setup Game
                LoadGameObjects();
            }
        }
    }

    //Loading Game Objects
    void LoadGameObjects()
    {

        ObjectsManager.Instance.instantiateGameObjectsFinishedCallback += LoadGameObjectsFinished;
        ObjectsManager.Instance.InstantiateSetupGameObjects();
    }

    void LoadGameObjectsFinished()
    {
        //Set Object refereces To Game
        foreach(GameObject gameRoomObject in ObjectsManager.Instance.GameObjectsInstances)
        {
            if (gameRoomObject.name.Contains("Spell"))
            {
                if (SpellManager.Instance.spawnPointGameObj == null)
                {
                    SpellManager.Instance.spawnPointGameObj = new List<GameObject>();
                }
                SpellManager.Instance.spawnPointGameObj.Add(gameRoomObject);
            }
            else if(gameRoomObject.name.Contains("GhostSpawn"))
            {
                if(GhostManager.Instance.whiteGhostPositions == null)
                {
                    GhostManager.Instance.whiteGhostPositions = new List<GameObject>();
                }
                GhostManager.Instance.whiteGhostPositions.Add(gameRoomObject);
            }
            else if (gameRoomObject.name.Contains("Bat"))
            {
                if (GhostManager.Instance.batPositions == null)
                {
                    GhostManager.Instance.batPositions = new List<GameObject>();
                }
                GhostManager.Instance.batPositions.Add(gameRoomObject);
            }
            else if (gameRoomObject.name.Contains("Spider"))
            {
                if (GhostManager.Instance.spiderPositions == null)
                {
                    GhostManager.Instance.spiderPositions = new List<GameObject>();
                }
                GhostManager.Instance.spiderPositions.Add(gameRoomObject);
            }
            else if (gameRoomObject.name.Contains("CatStart"))
            {
                if (GameObject.Find("TutorialFlow") != null)
                {
                    GameObject.Find("TutorialFlow").GetComponent<TutorialFlow>().startRangeAnchor = gameRoomObject;
                }
            }
        }

        LoadNavMap();
    }



    //Loading Nav Map
    void LoadNavMap(bool show = false)
    {
        //Load Nav Map
        if(NavMapManager.Instance.navLaodFinishedDelegate != null)
        {
            NavMapManager.Instance.navLaodFinishedDelegate -= LoadNavMapFinished;
        }
        NavMapManager.Instance.navLaodFinishedDelegate += LoadNavMapFinished;
        NavMapManager.Instance.LoadNavMap(show);
    }

    void LoadNavMapFinished()
    {
        if(setupState == SetupState.Init || setupState == SetupState.Finished)
        {
            Invoke("SetupGameComplete", 2);
        }
    }
    
    //Finishing
    void SetupGameComplete()
    {
        if (setupGameCompleteCallback != null)
        {
            setupGameCompleteCallback();
        }
    }

}
