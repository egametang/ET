using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class Shape : DisplayObject
    {
        /// <summary>
        /// 
        /// </summary>
        public Shape()
        {
            CreateGameObject("Shape");
            graphics = new NGraphics(gameObject);
            graphics.texture = NTexture.Empty;
            graphics.meshFactory = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Color color
        {
            get
            {
                return graphics.color;
            }
            set
            {
                graphics.color = value;
                graphics.SetMeshDirty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineSize"></param>
        /// <param name="lineColor"></param>
        /// <param name="fillColor"></param>
        public void DrawRect(float lineSize, Color lineColor, Color fillColor)
        {
            RectMesh mesh = graphics.GetMeshFactory<RectMesh>();
            mesh.lineWidth = lineSize;
            mesh.lineColor = lineColor;
            mesh.fillColor = null;
            mesh.colors = null;

            graphics.color = fillColor;
            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineSize"></param>
        /// <param name="colors"></param>
        public void DrawRect(float lineSize, Color32[] colors)
        {
            RectMesh mesh = graphics.GetMeshFactory<RectMesh>();
            mesh.lineWidth = lineSize;
            mesh.colors = colors;

            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineSize"></param>
        /// <param name="lineColor"></param>
        /// <param name="fillColor"></param>
        /// <param name="topLeftRadius"></param>
        /// <param name="topRightRadius"></param>
        /// <param name="bottomLeftRadius"></param>
        /// <param name="bottomRightRadius"></param>
        public void DrawRoundRect(float lineSize, Color lineColor, Color fillColor,
            float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius)
        {
            RoundedRectMesh mesh = graphics.GetMeshFactory<RoundedRectMesh>();
            mesh.lineWidth = lineSize;
            mesh.lineColor = lineColor;
            mesh.fillColor = null;
            mesh.topLeftRadius = topLeftRadius;
            mesh.topRightRadius = topRightRadius;
            mesh.bottomLeftRadius = bottomLeftRadius;
            mesh.bottomRightRadius = bottomRightRadius;

            graphics.color = fillColor;
            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillColor"></param>
        public void DrawEllipse(Color fillColor)
        {
            EllipseMesh mesh = graphics.GetMeshFactory<EllipseMesh>();
            mesh.lineWidth = 0;
            mesh.startDegree = 0;
            mesh.endDegreee = 360;
            mesh.fillColor = null;
            mesh.centerColor = null;

            graphics.color = fillColor;
            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineSize"></param>
        /// <param name="centerColor"></param>
        /// <param name="lineColor"></param>
        /// <param name="fillColor"></param>
        /// <param name="startDegree"></param>
        /// <param name="endDegree"></param>
        public void DrawEllipse(float lineSize, Color centerColor, Color lineColor, Color fillColor, float startDegree, float endDegree)
        {
            EllipseMesh mesh = graphics.GetMeshFactory<EllipseMesh>();
            mesh.lineWidth = lineSize;
            if (centerColor.Equals(fillColor))
                mesh.centerColor = null;
            else
                mesh.centerColor = centerColor;
            mesh.lineColor = lineColor;
            mesh.fillColor = null;
            mesh.startDegree = startDegree;
            mesh.endDegreee = endDegree;

            graphics.color = fillColor;
            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="fillColor"></param>
        public void DrawPolygon(IList<Vector2> points, Color fillColor)
        {
            PolygonMesh mesh = graphics.GetMeshFactory<PolygonMesh>();
            mesh.points.Clear();
            mesh.points.AddRange(points);
            mesh.fillColor = null;
            mesh.colors = null;

            graphics.color = fillColor;
            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="colors"></param>
        public void DrawPolygon(IList<Vector2> points, Color32[] colors)
        {
            PolygonMesh mesh = graphics.GetMeshFactory<PolygonMesh>();
            mesh.points.Clear();
            mesh.points.AddRange(points);
            mesh.fillColor = null;
            mesh.colors = colors;

            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="fillColor"></param>
        /// <param name="lineSize"></param>
        /// <param name="lineColor"></param>
        public void DrawPolygon(IList<Vector2> points, Color fillColor, float lineSize, Color lineColor)
        {
            PolygonMesh mesh = graphics.GetMeshFactory<PolygonMesh>();
            mesh.points.Clear();
            mesh.points.AddRange(points);
            mesh.fillColor = null;
            mesh.lineWidth = lineSize;
            mesh.lineColor = lineColor;
            mesh.colors = null;

            graphics.color = fillColor;
            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sides"></param>
        /// <param name="lineSize"></param>
        /// <param name="centerColor"></param>
        /// <param name="lineColor"></param>
        /// <param name="fillColor"></param>
        /// <param name="rotation"></param>
        /// <param name="distances"></param>
        public void DrawRegularPolygon(int sides, float lineSize, Color centerColor, Color lineColor, Color fillColor, float rotation, float[] distances)
        {
            RegularPolygonMesh mesh = graphics.GetMeshFactory<RegularPolygonMesh>();
            mesh.sides = sides;
            mesh.lineWidth = lineSize;
            mesh.centerColor = centerColor;
            mesh.lineColor = lineColor;
            mesh.fillColor = null;
            mesh.rotation = rotation;
            mesh.distances = distances;

            graphics.color = fillColor;
            graphics.SetMeshDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            graphics.meshFactory = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool isEmpty
        {
            get { return graphics.meshFactory == null; }
        }

        protected override DisplayObject HitTest()
        {
            if (graphics.meshFactory == null)
                return null;

            Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);

            IHitTest ht = graphics.meshFactory as IHitTest;
            if (ht != null)
                return ht.HitTest(_contentRect, localPoint) ? this : null;
            else if (_contentRect.Contains(localPoint))
                return this;
            else
                return null;
        }
    }
}
