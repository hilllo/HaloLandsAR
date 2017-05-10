using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PATH {
    public class PathMap
    {
        public List<PathPoint> pathPoints;
        public int pointNum;
        public Dictionary<PathPoint,List<PathData>> map;
        
        public delegate void ConvertToStringFinished(string result);
        public ConvertToStringFinished convertFinishedDelegate;

        public PathMap()
        { 
            pathPoints = new List<PathPoint>();
            pointNum = 0;
            map = new Dictionary<PathPoint, List<PathData>>(new PathMapComparer());
            //GenerateTestData();
        }
        public void GenerateTestData()
        {
            PathPoint p0 = new PathPoint(0, 0, 0);
            PathPoint p1 = new PathPoint(10, 0, 10);
            PathPoint p2 = new PathPoint(20, 0, 20);
            PathPoint p3 = new PathPoint(30, 0, 30);
            PathPoint p4 = new PathPoint(40, 0, 40);
            List<PathData> d0 = new List<PathData>();
            List<PathData> d1 = new List<PathData>();
            List<PathData> d2 = new List<PathData>();
            List<PathData> d3 = new List<PathData>();
            List<PathData> d4 = new List<PathData>();
            d0.Add(new PathData(p0, p1));
            d0.Add(new PathData(p0, p1));

            d1.Add(new PathData(p1, p0));
            d1.Add(new PathData(p1, p2));
            d1.Add(new PathData(p1, p3));

            d2.Add(new PathData(p2, p0));
            d2.Add(new PathData(p2, p1));

            d3.Add(new PathData(p3, p1));
            d3.Add(new PathData(p3, p4));

            d4.Add(new PathData(p4, p3));
            map.Add(p0, d0);
            map.Add(p1, d1);
            map.Add(p2, d2);
            map.Add(p3, d3);
            map.Add(p4, d4);
            
        }


    }
    public class PathMapComparer : IEqualityComparer<PathPoint>
    {
        public bool Equals(PathPoint x, PathPoint y)
        {
            return (x.x == y.x && x.z == y.z);
            //return x.A == y.A && x.B == y.B;
        }

        public int GetHashCode(PathPoint x)
        {
            return (x.x.GetHashCode() * 1000 + x.z.GetHashCode());
            //return x.A.GetHashCode() + x.B.GetHashCode();
        }


    }



}
