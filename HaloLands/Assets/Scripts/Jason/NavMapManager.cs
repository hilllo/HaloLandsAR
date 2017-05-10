using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using PATH;
using System.IO;

using System.Text;

public class NavMapManager : Game.Singleton<NavMapManager> {

    static float MAX_CONNECT_DISTANCE = 0.5f;

    public PathMap currentPathMap;

    List<GameObject> visualPoints;
    List<GameObject> visualLines;
    public Material VisualLineMaterial;

    public GameObject visualPointPrefab;
    

    private string PathMapFilePath;

    public delegate void NavMapSetupFinished();
    public NavMapSetupFinished navSetupFinishedDelegate;

    public delegate void NavMapLoadFinished();
    public NavMapLoadFinished navLaodFinishedDelegate;

    private bool showMapAfterLoad = false;
    
	// Use this for initialization
	void Start () {
        PathMapFilePath = Application.persistentDataPath + "/NavMap.txt";

        currentPathMap = new PathMap();
        visualPoints = new List<GameObject>();
        visualLines = new List<GameObject>();
    }

    public void DeleteNavMap()
    {
        foreach(GameObject visual in visualLines)
        {
            Destroy(visual);
        }

        foreach(GameObject visual in visualPoints)
        {
            Destroy(visual);
        }
        currentPathMap = new PathMap();
    }

    //Editor test
    public void TestMap()
    {
        this.navSetupFinishedDelegate += TestShowNavMap;

        for (int i=0; i<15; i++)
        {
            for(int j=0; j<15; j++)
            {
                Vector3 point = new Vector3(-2.5f + 5f * i / 15f, -2.5f, -2.5f + 5f * j / 15f);
                UpdatePathPoint(point);
            }
        }

        FinishPathMap();
    }

    void TestShowNavMap()
    {
        ShowNavMap();
        this.navLaodFinishedDelegate();
    }

    void ShowNavMap()
    {
        foreach (KeyValuePair<PathPoint, List<PathData>> pair in currentPathMap.map)
        {
            foreach (PathData path in pair.Value)
            {
                DrawLine(pair.Key.vector, path.point.vector);
            }
        }
    }

	// Update is called once per frame
	void Update () {
		
	}

    //Make Nav Map

    public void StartMakingNavMap()
    {
        
        
    }

    public void UpdatePathPoint(Vector3 point)
    {

        //Make New Point
        PathPoint pathPoint = new PathPoint();
        pathPoint.x = point.x;
        pathPoint.y = point.y;
        pathPoint.z = point.z;
        currentPathMap.pathPoints.Add(pathPoint);
        currentPathMap.pointNum += 1;
        currentPathMap.map[pathPoint] = new List<PathData>();

        //Calculate Path
        UpdatePathMapDistance();

        //Make Visual
        GameObject visualPoint = Instantiate(visualPointPrefab, HoloToolkit.Unity.SpatialUnderstanding.Instance.transform);
        visualPoint.transform.localPosition = point;
        visualPoints.Add(visualPoint);
        List<PathData> paths = currentPathMap.map[pathPoint];
        if(paths != null && paths.Count > 0)
        {
            foreach (PathData path in paths)
            {
                DrawLine(point, path.point.vector);
            }
        }
    }

    public void FinishPathMap()
    {

        //Destroy Visuals
         foreach(GameObject visualPoint in visualPoints)
        {
            Destroy(visualPoint);
        }
        foreach (GameObject visualLine in visualLines)
        {
            Destroy(visualLine);
        }
        visualPoints.Clear();
        visualLines.Clear();

        //Save Points
        SaveNavMap();
    }

    void UpdatePathMapDistance()
    {
        //Update last node distance
        if(currentPathMap.pathPoints.Count <= 1)
        {
            return;
        }
        for(int i=0; i<currentPathMap.pathPoints.Count - 1; i++)
        {
            
            //Calculate Distance
            PathPoint point = currentPathMap.pathPoints[i];
            PathPoint lastPoint = currentPathMap.pathPoints[currentPathMap.pointNum - 1];
            float distance = Vector3.Distance(point.vector, lastPoint.vector);
            if (distance < MAX_CONNECT_DISTANCE)
            {
                //Connect
                List<PathData> pointPaths = currentPathMap.map[point];
                List<PathData> lastPointPaths = currentPathMap.map[lastPoint];

                PathData pathToLastPoint = new PathData();
                pathToLastPoint.point = lastPoint;
                pathToLastPoint.distance = distance;

                PathData pathToPoint = new PathData();
                pathToPoint.point = point;
                pathToPoint.distance = distance;

                pointPaths.Add(pathToLastPoint);
                lastPointPaths.Add(pathToPoint);

                currentPathMap.map[point] = pointPaths;
                currentPathMap.map[lastPoint] = lastPointPaths;
            }
        }
    }


    void DrawLine( Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject();
        visualLines.Add(lineObject);
        lineObject.AddComponent<LineRenderer>();
        LineRenderer lr = lineObject.GetComponent<LineRenderer>();
        

        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.useWorldSpace = true;
        lr.material = VisualLineMaterial;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        Vector3 pos = HoloToolkit.Unity.SpatialUnderstanding.Instance.transform.position;
        lr.SetPosition(0, start + pos);
        lr.SetPosition(1, end + pos);


    }

    //Save Load Nav Map

    void SaveNavMap()
    {
        ConvertToString();
    }

    public void LoadNavMap(bool show = false)
    {
        showMapAfterLoad = show;
#if UNITY_WSA && !UNITY_EDITOR
        if (File.Exists(Application.persistentDataPath + "/NavMap.txt"))
        {
            string fileContent = File.ReadAllText(PathMapFilePath);

            ConvertToMap(fileContent);
        }
        else
        {
            Debug.Log("Nav map doesn't exist");
        }
#else
        TestMap();
#endif
    }

    //Callback
    void ConvertToStringDone(string resultString)
    {
        //Save to file
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(resultString);

        File.WriteAllBytes(PathMapFilePath, bytes);

        if(this.navSetupFinishedDelegate != null)
        {
            this.navSetupFinishedDelegate();
        }
    }

    void ConvertToMapDone()
    {
        if (this.navLaodFinishedDelegate != null)
        {
            this.navLaodFinishedDelegate();
        }
    }


    //To String

    void ConvertToString()
    {
        StartCoroutine(ConvertCoroutine());
    }

    IEnumerator ConvertCoroutine()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");

        float startTime = Time.time;
        foreach(KeyValuePair<PathPoint, List<PathData>> pair in currentPathMap.map)
        {
            ConvertPairToString(sb, pair);
            if(Time.time - startTime > 0.01f)
            {
                startTime = Time.time;
                yield return null;
            }
        }
        RemoveLastCharacter(sb).Append("}");

        ConvertToStringDone(sb.ToString());

        yield return null;
    }

    StringBuilder RemoveLastCharacter(StringBuilder sb)
    {
        return sb.Remove(sb.Length - 1, 1);
    }

    StringBuilder ConvertVector3ToString(StringBuilder sb, PathPoint point)
    {
        return sb.Append("(").Append(point.x).Append(" ").Append(point.y).Append(" ").Append(point.z).Append(")");
    }

    StringBuilder ConvertVector3ListToString(StringBuilder sb, List<PathData> pointsList)
    {
        sb.Append("[");
        foreach(PathData point in pointsList)
        {
            ConvertVector3ToString(sb, point.point).Append("|");
        }
        //Remove coma
        return RemoveLastCharacter(sb).Append("]");
    }

    StringBuilder ConvertPairToString(StringBuilder sb, KeyValuePair<PathPoint, List<PathData>> pair)
    {
        ConvertVector3ToString(sb, pair.Key).Append(",");
        return ConvertVector3ListToString(sb, pair.Value).Append(";");
    }

    //String To Map
    void ConvertToMap(string data)
    {
        currentPathMap = new PathMap();
        StartCoroutine(ConvertToMapCoroutine(data));
    }

    IEnumerator ConvertToMapCoroutine(string data)
    {


        //{
        data = removeQuote(data);

        //Pair
        string[] pairs = data.Split(';');


        float startTime = Time.time;
        foreach (string pair in pairs)
        {
            //Key Value
            string[] keyValue = pair.Split(',');

            //Key
            string key = keyValue[0];

            string value = keyValue[1];

            //Current Point
            PathPoint keyPoint = ConvertStringToPoint(key);

            currentPathMap.pathPoints.Add(keyPoint);
            currentPathMap.pointNum += 1;

            //Paths
            List<PathData> pathsList = new List<PathData>();

            if (value.Length > 2)
            {
                value = removeQuote(value);
                string[] connectedPoints = value.Split('|');
                if(connectedPoints.Length > 0)
                {
                    foreach (string connectPoint in connectedPoints)
                    {
                        PathPoint cPoint = ConvertStringToPoint(connectPoint);
                        PathData path = new PathData(keyPoint, cPoint);
                        path.distance = Vector3.Distance(keyPoint.vector, cPoint.vector);
                        pathsList.Add(path);
                    }
                }
            }

            currentPathMap.map[keyPoint] = pathsList;

            if (Time.time - startTime > 0.01f)
            {
                startTime = Time.time;
                yield return null;
            }
        }


        ConvertToMapDone();

        if (showMapAfterLoad)
        {
            ShowNavMap();
        }
        yield return null;
    }

    string removeQuote(string data)
    {
        return data.Substring(1, data.Length - 2);
    }

    PathPoint ConvertStringToPoint(string data)
    {
        string[] values = removeQuote(data).Split(' ');
        return new PathPoint(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
    }


    


}
