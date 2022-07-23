using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public enum EaseType
    {
        Linear,
        SineIn,
        SineOut,
        SineInOut,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        CircIn,
        CircOut,
        CircInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BackIn,
        BackOut,
        BackInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        Custom
    }

    /// <summary>
    /// 
    /// </summary>
    public class CustomEase
    {
        Vector2[] _points;
        int _pointDensity;

        static GPath helperPath = new GPath();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointDensity"></param>
        public CustomEase(int pointDensity = 200)
        {
            _points = new Vector2[pointDensity + 1];
            _pointDensity = pointDensity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathPoints"></param>
        public void Create(IEnumerable<GPathPoint> pathPoints)
        {
            helperPath.Create(pathPoints);
            for (int i = 0; i <= _pointDensity; i++)
            {
                Vector3 pt = helperPath.GetPointAt(i / (float)_pointDensity);
                _points[i] = pt;
            }
            _points[0] = Vector2.zero;
            _points[_pointDensity] = Vector2.one;

            Array.Sort(_points, (Vector2 p1, Vector2 p2) =>
            {
                return p1.x.CompareTo(p2.x);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float Evaluate(float time)
        {
            if (time <= 0)
                return 0;
            else if (time >= 1)
                return 1;

            int low = 0;
            int high = _pointDensity;
            int cur = 0;
            while (low != high)
            {
                cur = low + (int)((high - low) / 2f);
                float x = _points[cur].x;
                if (time == x)
                    break;
                else if (time > x)
                {
                    if (low == cur)
                    {
                        cur = high;
                        break;
                    }
                    low = cur;
                }
                else
                {
                    if (high == cur)
                    {
                        cur = low;
                        break;
                    }
                    high = cur;
                }
            }

            Vector2 v0 = _points[cur];
            Vector2 v1;
            if (cur == _pointDensity)
                v1 = Vector2.one;
            else
                v1 = _points[cur + 1];
            float k = (v1.y - v0.y) / (v1.x - v0.x);
            if (float.IsNaN(k))
                k = 0;

            return v0.y + (time - v0.x) * k;
        }
    }
}
