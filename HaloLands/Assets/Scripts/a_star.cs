using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
//using Wintellect.PowerCollections;
namespace PATH
{
    //to move the object, call Move(), to stop, call Stop(); the default would be Move at start;
    public class a_star : MonoBehaviour
    {
        static float SPEED = 0.3f;
        float currentSpeed;
        PathMap pathmap;
        GameObject targetObj;
        static float[] speedLevel = { 0.3f, 0.5f, 0.8f, 1f};
        //public float Speed = 1f;
        float MovingDelta = 0.008f;
        Vector3 startpos, targetpos;
        float journey;
        //float ypos;
        Vector3 orgPos;
        static int speedLevelCnt = 0;
        //Default would be move
        bool movable = false;
        void Start()
        {
            SPEED = speedLevel[speedLevelCnt];
            pathmap =  NavMapManager.Instance.currentPathMap;
            targetObj = Camera.main.gameObject;
           
            orgPos= transform.position;
            startpos = orgPos;
            targetpos = orgPos;
            //ypos = transform.position.y;
            journey = 1f;
            currentSpeed = SPEED;
        }
        public static void SpeedUp()
        {
            if(speedLevelCnt< speedLevel.Length)
            {
                SPEED = speedLevel[++speedLevelCnt];
            }
        }
        public static void SetSpeed(float s)
        {
            SPEED = s;
        }
        public void Reset()
        {
            orgPos = transform.position;
            startpos = orgPos;
            targetpos = orgPos;
            //speedLevelCnt = 0;
            //SPEED = speedLevel[speedLevelCnt];
            journey = 1f;
        }
        //Move the Object
        public void Move()
        {
            //Debug.Log("move");
            movable = true;
        }
        //Stop the Object
        public void Stop()
        {
            //Debug.Log("stop");
            movable = false;
        }
        void Update()
        {
            if (pathmap == null || pathmap.map.Count == 0)
            {
                pathmap = NavMapManager.Instance.currentPathMap;
                if (movable)
                {
                    Vector3 dir = (Camera.main.transform.position - transform.position).normalized;
                    transform.position += dir * currentSpeed * Time.deltaTime;
                }
                return;
            }
            if (movable)
            {
                currentSpeed = SPEED;
            }
            else
            {
                if (currentSpeed > SPEED/4)
                    currentSpeed = Mathf.Lerp(currentSpeed, 0f, 0.999f * Time.deltaTime);
                else
                    currentSpeed = 0;
            }
            //Debug.Log(currentSpeed);
            if (journey >= 1f )
            {
                journey = 0;
                UpdateTarget();
                MovingDelta = ( Time.deltaTime) / Vector3.Distance(startpos, targetpos);
            }
            transform.position = Vector3.Lerp(startpos, targetpos, journey);
            journey += currentSpeed * MovingDelta;
        }
        void UpdateTarget()
        {
            //Debug.Log("TARGET");
            startpos = targetpos;
            targetpos = A_NextPosition(targetpos, targetObj.transform.position);
           // Debug.Log("New Target!"+targetpos);

            targetpos.y = orgPos.y;
            //Debug.Log("end " + targetpos);
            //yield return new WaitForSeconds(MovingSpeed);


        }
        public Vector3 NextPosition(Vector3 n, Vector3 g)
        {
            PathPoint now = new PathPoint(n.x, n.y, n.z);
            PathPoint goal = new PathPoint(g.x, g.y, g.z);
            List<PathData> data = null;
            float minDist = CalculateDistance(now, goal);
            PathPoint next = now;
            if (pathmap.map.TryGetValue(now, out data) == false)
            {

                foreach (PathPoint p in pathmap.map.Keys)
                {
                    float tmp = CalculateDistance(p, now);
                    if (tmp < minDist)
                    {
                        minDist = tmp;
                        next = p;
                    }
                }

            }
            else
            {
                foreach (PathData d in data)
                {
                    float tmp = CalculateDistance(d.point, goal);
                    if (tmp < minDist)
                    {
                        minDist = tmp;
                        next = d.point;
                    }
                }
            }
            if (next.Equals(now))
            {
                next = goal;
            }
            return next.ToVector3();
        }
        public Vector3 A_NextPosition(Vector3 n, Vector3 g)
        {
            PathPoint now = new PathPoint(n.x, n.y, n.z);
            PathPoint goal = new PathPoint(g.x, g.y, g.z);
            List<PathData> data = null;
            float minDist = CalculateDistance(now, goal);
            PathPoint next = now;
            if (pathmap.map.ContainsKey(now)==false)
            {
                //Debug.Log("to nearst point" + now.ToVector3());
                foreach (PathPoint p in pathmap.map.Keys)
                {
                    float tmp = CalculateDistance(p, now);
                    if (tmp < minDist)
                    {
                        minDist = tmp;
                        next = p;
                    }
                }

            }
            else
            {
                //Debug.Log("");
                minDist = CalculateDistance(now, goal);
                PathPoint goalNearestPoint = now;
                foreach (PathPoint p in pathmap.map.Keys)
                {
                    float tmp = CalculateDistance(p, goal);
                    if (tmp < minDist)
                    {
                        minDist = tmp;
                        goalNearestPoint = p;
                    }
                }
                if (goalNearestPoint.Equals(now))
                {
                    //Debug.Log("on nearst point, to target");
                    next = goal;
                }
                else
                {
                    //Debug.Log("A* "+ now.ToVector3() +" "+ goalNearestPoint.ToVector3());

                    next = aStar(now, goalNearestPoint);
                }

            }
            
            return next.ToVector3();
        }
        float CalculateDistance(PathPoint a, PathPoint b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z);
        }

        public PathPoint aStar(PathPoint origin, PathPoint goal)
        {
            //Dictionary<PathPoint, STATE> pointState=new Dictionary<PathPoint, STATE>(new pointMapComparer());
            SimplePriorityQueue<pathNode> OPEN = new SimplePriorityQueue<pathNode>();
            //Wintellect.PowerCollections.OrderedMultiDictionary<float, pathNode> QUEUE = new OrderedMultiDictionary<float, pathNode>(true);
            //Wintellect.PowerCollections.OrderedDictionary<pathNode, float> OPEN = new OrderedDictionary<pathNode, float>();
            HashSet<pathNode> CLOSE_LIST = new HashSet<pathNode>();
            Dictionary<pathNode,float> OPEN_LIST = new Dictionary<pathNode, float>();
            
            pathNode goalNode = new pathNode(goal, null);
            //QUEUE.Add(0, goalNode);
            OPEN.Enqueue(goalNode, goalNode.getfx());
            OPEN_LIST.Add(goalNode, goalNode.getfx());
            while (OPEN.Count > 0)
            {
                pathNode node = OPEN.Dequeue();
                OPEN_LIST.Remove(node);
                if (node.point.Equals(origin))
                {
                    return node.parent;
                }
                //QUEUE.Remove(node.g + node.h, node);
                //OPEN.Remove(node);
                CLOSE_LIST.Add(node);
                List<PathData> list = null;
                pathmap.map.TryGetValue(node.point, out list);
                foreach (PathData p in list)
                {
                    pathNode tmp = new pathNode(p.point, node.point);
                    tmp.g = node.g + p.distance;
                    //tmp.f = (node.point.x - tmp.point.x) * (node.point.x - tmp.point.x) + (node.point.y - tmp.point.y) * (node.point.y - tmp.point.y);
                    if (CLOSE_LIST.Contains(tmp)) continue;
                    if (OPEN.Contains(tmp))
                    {
                        
                        tmp.h = node.point.distance(tmp.point);
                        float originalF = float.MaxValue;
                        OPEN_LIST.TryGetValue(tmp, out originalF);
                        if (tmp.h + tmp.g >= originalF) continue;

                        OPEN_LIST.Remove(tmp);
                        OPEN.Remove(tmp);
                    }
                    OPEN.Enqueue(tmp, tmp.getfx());
                    OPEN_LIST.Add(tmp,tmp.getfx());

                }

            }

            return null;
        }


    }
    class pathNode : IEquatable<pathNode>
    {
        public PathPoint point;
        public PathPoint parent;
        public float g;
        public float h;

        public pathNode() { }
        public pathNode(PathPoint a, PathPoint b)
        {
            g = 0;
            h = 0;

            point = a;
            parent = b;
        }
        public float getfx()
        {
            return g + h;
        }
        public override int GetHashCode()
        {
            return point.x.GetHashCode() * 1000 + point.z.GetHashCode();
        }

        public bool Equals(pathNode other)
        {
            return other.point.Equals(point);
        }
    }
}