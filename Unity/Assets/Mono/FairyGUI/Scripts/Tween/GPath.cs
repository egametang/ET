using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public struct GPathPoint
    {
        /// <summary>
        /// 
        /// </summary>
        public Vector3 pos;

        /// <summary>
        /// 
        /// </summary>
        public Vector3 control1;

        /// <summary>
        /// 
        /// </summary>
        public Vector3 control2;

        /// <summary>
        /// 
        /// </summary>
        public CurveType curveType;

        /// <summary>
        /// 
        /// </summary>
        public bool smooth;

        /// <summary>
        /// 
        /// </summary>
        public enum CurveType
        {
            CRSpline,
            Bezier,
            CubicBezier,
            Straight
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        public GPathPoint(Vector3 pos)
        {
            this.pos = pos;
            this.control1 = Vector3.zero;
            this.control2 = Vector3.zero;
            this.curveType = CurveType.CRSpline;
            this.smooth = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="control"></param>
        public GPathPoint(Vector3 pos, Vector3 control)
        {
            this.pos = pos;
            this.control1 = control;
            this.control2 = Vector3.zero;
            this.curveType = CurveType.Bezier;
            this.smooth = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="control1"></param>
        /// <param name="control2"></param>
        public GPathPoint(Vector3 pos, Vector3 control1, Vector3 control2)
        {
            this.pos = pos;
            this.control1 = control1;
            this.control2 = control2;
            this.curveType = CurveType.CubicBezier;
            this.smooth = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="curveType"></param>
        public GPathPoint(Vector3 pos, CurveType curveType)
        {
            this.pos = pos;
            this.control1 = Vector3.zero;
            this.control2 = Vector3.zero;
            this.curveType = curveType;
            this.smooth = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GPath
    {
        protected struct Segment
        {
            public GPathPoint.CurveType type;
            public float length;
            public int ptStart;
            public int ptCount;
        }

        protected List<Segment> _segments;
        protected List<Vector3> _points;
        protected float _fullLength;

        static List<GPathPoint> helperList = new List<GPathPoint>();
        static List<Vector3> splinePoints = new List<Vector3>();

        public GPath()
        {
            _segments = new List<Segment>();
            _points = new List<Vector3>();
        }

        /// <summary>
        /// 
        /// </summary>
        public float length
        {
            get { return _fullLength; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        public void Create(GPathPoint pt1, GPathPoint pt2)
        {
            helperList.Clear();
            helperList.Add(pt1);
            helperList.Add(pt2);
            Create(helperList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="pt3"></param>
        public void Create(GPathPoint pt1, GPathPoint pt2, GPathPoint pt3)
        {
            helperList.Clear();
            helperList.Add(pt1);
            helperList.Add(pt2);
            helperList.Add(pt3);
            Create(helperList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="pt3"></param>
        /// <param name="pt4"></param>
        public void Create(GPathPoint pt1, GPathPoint pt2, GPathPoint pt3, GPathPoint pt4)
        {
            helperList.Clear();
            helperList.Add(pt1);
            helperList.Add(pt2);
            helperList.Add(pt3);
            helperList.Add(pt4);
            Create(helperList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        public void Create(IEnumerable<GPathPoint> points)
        {
            _segments.Clear();
            _points.Clear();
            splinePoints.Clear();
            _fullLength = 0;

            var et = points.GetEnumerator();
            if (!et.MoveNext())
                return;

            GPathPoint prev = et.Current;
            if (prev.curveType == GPathPoint.CurveType.CRSpline)
                splinePoints.Add(prev.pos);

            while (et.MoveNext())
            {
                GPathPoint current = et.Current;

                if (prev.curveType != GPathPoint.CurveType.CRSpline)
                {
                    Segment seg = new Segment();
                    seg.type = prev.curveType;
                    seg.ptStart = _points.Count;
                    if (prev.curveType == GPathPoint.CurveType.Straight)
                    {
                        seg.ptCount = 2;
                        _points.Add(prev.pos);
                        _points.Add(current.pos);
                    }
                    else if (prev.curveType == GPathPoint.CurveType.Bezier)
                    {
                        seg.ptCount = 3;
                        _points.Add(prev.pos);
                        _points.Add(current.pos);
                        _points.Add(prev.control1);
                    }
                    else if (prev.curveType == GPathPoint.CurveType.CubicBezier)
                    {
                        seg.ptCount = 4;
                        _points.Add(prev.pos);
                        _points.Add(current.pos);
                        _points.Add(prev.control1);
                        _points.Add(prev.control2);
                    }
                    seg.length = Vector3.Distance(prev.pos, current.pos);
                    _fullLength += seg.length;
                    _segments.Add(seg);
                }

                if (current.curveType != GPathPoint.CurveType.CRSpline)
                {
                    if (splinePoints.Count > 0)
                    {
                        splinePoints.Add(current.pos);
                        CreateSplineSegment();
                    }
                }
                else
                    splinePoints.Add(current.pos);

                prev = current;
            }

            if (splinePoints.Count > 1)
                CreateSplineSegment();
        }

        void CreateSplineSegment()
        {
            int cnt = splinePoints.Count;
            splinePoints.Insert(0, splinePoints[0]);
            splinePoints.Add(splinePoints[cnt]);
            splinePoints.Add(splinePoints[cnt]);
            cnt += 3;

            Segment seg = new Segment();
            seg.type = GPathPoint.CurveType.CRSpline;
            seg.ptStart = _points.Count;
            seg.ptCount = cnt;
            _points.AddRange(splinePoints);

            seg.length = 0;
            for (int i = 1; i < cnt; i++)
                seg.length += Vector3.Distance(splinePoints[i - 1], splinePoints[i]);
            _fullLength += seg.length;
            _segments.Add(seg);
            splinePoints.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _segments.Clear();
            _points.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetPointAt(float t)
        {
            t = Mathf.Clamp01(t);
            int cnt = _segments.Count;
            if (cnt == 0)
                return Vector3.zero;

            Segment seg;
            if (t == 1)
            {
                seg = _segments[cnt - 1];

                if (seg.type == GPathPoint.CurveType.Straight)
                    return Vector3.Lerp(_points[seg.ptStart], _points[seg.ptStart + 1], t);
                else if (seg.type == GPathPoint.CurveType.Bezier || seg.type == GPathPoint.CurveType.CubicBezier)
                    return onBezierCurve(seg.ptStart, seg.ptCount, t);
                else
                    return onCRSplineCurve(seg.ptStart, seg.ptCount, t);
            }

            float len = t * _fullLength;
            Vector3 pt = new Vector3();
            for (int i = 0; i < cnt; i++)
            {
                seg = _segments[i];

                len -= seg.length;
                if (len < 0)
                {
                    t = 1 + len / seg.length;

                    if (seg.type == GPathPoint.CurveType.Straight)
                        pt = Vector3.Lerp(_points[seg.ptStart], _points[seg.ptStart + 1], t);
                    else if (seg.type == GPathPoint.CurveType.Bezier || seg.type == GPathPoint.CurveType.CubicBezier)
                        pt = onBezierCurve(seg.ptStart, seg.ptCount, t);
                    else
                        pt = onCRSplineCurve(seg.ptStart, seg.ptCount, t);

                    break;
                }
            }

            return pt;
        }

        /// <summary>
        /// 
        /// </summary>
        public int segmentCount
        {
            get { return _segments.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentIndex"></param>
        /// <returns></returns>
        public float GetSegmentLength(int segmentIndex)
        {
            return _segments[segmentIndex].length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentIndex"></param>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <param name="points"></param>
        /// <param name="ts"></param>
        public void GetPointsInSegment(int segmentIndex, float t0, float t1, List<Vector3> points, List<float> ts = null, float pointDensity = 0.1f)
        {
            if (points == null)
                points = new List<Vector3>();

            if (ts != null)
                ts.Add(t0);
            Segment seg = _segments[segmentIndex];
            if (seg.type == GPathPoint.CurveType.Straight)
            {
                points.Add(Vector3.Lerp(_points[seg.ptStart], _points[seg.ptStart + 1], t0));
                points.Add(Vector3.Lerp(_points[seg.ptStart], _points[seg.ptStart + 1], t1));
            }
            else if (seg.type == GPathPoint.CurveType.Bezier || seg.type == GPathPoint.CurveType.CubicBezier)
            {
                points.Add(onBezierCurve(seg.ptStart, seg.ptCount, t0));
                int SmoothAmount = (int)Mathf.Min(seg.length * pointDensity, 50);
                for (int j = 0; j <= SmoothAmount; j++)
                {
                    float t = (float)j / SmoothAmount;
                    if (t > t0 && t < t1)
                    {
                        points.Add(onBezierCurve(seg.ptStart, seg.ptCount, t));
                        if (ts != null)
                            ts.Add(t);
                    }
                }
                points.Add(onBezierCurve(seg.ptStart, seg.ptCount, t1));
            }
            else
            {
                points.Add(onCRSplineCurve(seg.ptStart, seg.ptCount, t0));
                int SmoothAmount = (int)Mathf.Min(seg.length * pointDensity, 50);
                for (int j = 0; j <= SmoothAmount; j++)
                {
                    float t = (float)j / SmoothAmount;
                    if (t > t0 && t < t1)
                    {
                        points.Add(onCRSplineCurve(seg.ptStart, seg.ptCount, t));
                        if (ts != null)
                            ts.Add(t);
                    }
                }
                points.Add(onCRSplineCurve(seg.ptStart, seg.ptCount, t1));
            }

            if (ts != null)
                ts.Add(t1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        public void GetAllPoints(List<Vector3> points, float pointDensity = 0.1f)
        {
            int cnt = _segments.Count;
            for (int i = 0; i < cnt; i++)
                GetPointsInSegment(i, 0, 1, points, null, pointDensity);
        }

        /// <summary>
        /// Catmull rom spline implementation
        /// by Stéphane Drouot, laei - http://games.laei.org
        /// 
        /// Actual translation of math gebrish to C# credit is due to 
        /// Boon Cotter - http://www.booncotter.com/waypoints-catmull-rom-splines/
        /// 
        /// This takes a list of vector3 (or an array) and gives a function called .onCurve(t)
        /// returning a value on a Catmull-Rom spline for 0 <= t <= 1
        /// </summary>
        Vector3 onCRSplineCurve(int ptStart, int ptCount, float t)
        {
            int adjustedIndex = Mathf.FloorToInt(t * (ptCount - 4)) + ptStart; //Since the equation works with 4 points, we adjust the starting point depending on t to return a point on the specific segment

            Vector3 result = new Vector3();

            Vector3 p0 = _points[adjustedIndex];
            Vector3 p1 = _points[adjustedIndex + 1];
            Vector3 p2 = _points[adjustedIndex + 2];
            Vector3 p3 = _points[adjustedIndex + 3];

            float adjustedT = (t == 1f) ? 1f : Mathf.Repeat(t * (ptCount - 4), 1f); // Then we adjust t to be that value on that new piece of segment... for t == 1f don't use repeat (that would return 0f);

            float t0 = ((-adjustedT + 2f) * adjustedT - 1f) * adjustedT * 0.5f;
            float t1 = (((3f * adjustedT - 5f) * adjustedT) * adjustedT + 2f) * 0.5f;
            float t2 = ((-3f * adjustedT + 4f) * adjustedT + 1f) * adjustedT * 0.5f;
            float t3 = ((adjustedT - 1f) * adjustedT * adjustedT) * 0.5f;

            result.x = p0.x * t0 + p1.x * t1 + p2.x * t2 + p3.x * t3;
            result.y = p0.y * t0 + p1.y * t1 + p2.y * t2 + p3.y * t3;
            result.z = p0.z * t0 + p1.z * t1 + p2.z * t2 + p3.z * t3;

            return result;
        }

        Vector3 onBezierCurve(int ptStart, int ptCount, float t)
        {
            float t2 = 1f - t;
            Vector3 p0 = _points[ptStart];
            Vector3 p1 = _points[ptStart + 1];
            Vector3 cp0 = _points[ptStart + 2];

            if (ptCount == 4)
            {
                Vector3 cp1 = _points[ptStart + 3];
                return t2 * t2 * t2 * p0 + 3f * t2 * t2 * t * cp0 + 3f * t2 * t * t * cp1 + t * t * t * p1;
            }
            else
                return t2 * t2 * p0 + 2f * t2 * t * cp0 + t * t * p1;
        }
    }
}