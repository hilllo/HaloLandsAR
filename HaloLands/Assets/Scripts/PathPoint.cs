using System.Collections;
using System.Collections.Generic;
using System;
namespace PATH
{
    [System.Serializable]
    public class PathPoint : IEquatable<PathPoint>
    {

        public float x;
        public float y;
        public float z;
        public UnityEngine.Vector3 vector
        {
            get
            {
                return new UnityEngine.Vector3(x, y, z);
            }
        }

        public PathPoint()
        {
            x = 0; y = 0; z = 0;
        }
        public PathPoint(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public UnityEngine.Vector3 ToVector3()
        {
            return new UnityEngine.Vector3(x, y, z);
            
        }
        public float distance(PathPoint other)
        {
            return (x - other.x) * (x - other.x) + (z - other.z) * (z - other.z);
        }

        public override int GetHashCode()
        {
            //return base.GetHashCode();

            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        }
        public bool Equals(PathPoint other)
        {
            if (other.x == x && other.z == z) return true;
            return false;
        }

        public override bool Equals(object o)
        {
            return this.Equals(o as PathPoint);
        }

        public void setPathPoint(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

    }
    public class PathData
    {
        public PathPoint point;
        public float distance;
        public PathData()
        {
            distance = 0;
        }
        public PathData(PathPoint origin, PathPoint target)
        {
            setData(origin, target);
        }
        public void setData(PathPoint origin, PathPoint target)
        {
            point = target;
            distance = (origin.x - target.x) * (origin.x - target.x) +
                        (origin.z - target.z) * (origin.z - target.z);
        }
    }

    /*public class pointMapComparer : IEqualityComparer<PathPoint>
    {
        public bool Equals(PathPoint a, PathPoint b)
        {
            return a.x == b.x && a.z == b.z;
        }

        public int GetHashCode(PathPoint a)
        {
            return a.x.GetHashCode() * 1000 + a.z.GetHashCode();
        }


    }*/
}

