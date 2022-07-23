using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class FillMesh : IMeshFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public FillMethod method;

        /// <summary>
        /// 
        /// </summary>
        public int origin;

        /// <summary>
        /// 
        /// </summary>
        public float amount;

        /// <summary>
        /// 
        /// </summary>
        public bool clockwise;

        public FillMesh()
        {
            clockwise = true;
            amount = 1;
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            float amount = Mathf.Clamp01(this.amount);
            switch (method)
            {
                case FillMethod.Horizontal:
                    FillHorizontal(vb, vb.contentRect, origin, amount);
                    break;

                case FillMethod.Vertical:
                    FillVertical(vb, vb.contentRect, origin, amount);
                    break;

                case FillMethod.Radial90:
                    FillRadial90(vb, vb.contentRect, (Origin90)origin, amount, clockwise);
                    break;

                case FillMethod.Radial180:
                    FillRadial180(vb, vb.contentRect, (Origin180)origin, amount, clockwise);
                    break;

                case FillMethod.Radial360:
                    FillRadial360(vb, vb.contentRect, (Origin360)origin, amount, clockwise);
                    break;
            }
        }

        static void FillHorizontal(VertexBuffer vb, Rect vertRect, int origin, float amount)
        {
            float a = vertRect.width * amount;
            if ((OriginHorizontal)origin == OriginHorizontal.Right || (OriginVertical)origin == OriginVertical.Bottom)
                vertRect.x += (vertRect.width - a);
            vertRect.width = a;

            vb.AddQuad(vertRect);
            vb.AddTriangles();
        }

        static void FillVertical(VertexBuffer vb, Rect vertRect, int origin, float amount)
        {
            float a = vertRect.height * amount;
            if ((OriginHorizontal)origin == OriginHorizontal.Right || (OriginVertical)origin == OriginVertical.Bottom)
                vertRect.y += (vertRect.height - a);
            vertRect.height = a;

            vb.AddQuad(vertRect);
            vb.AddTriangles();
        }

        //4 vertex
        static void FillRadial90(VertexBuffer vb, Rect vertRect, Origin90 origin, float amount, bool clockwise)
        {
            bool flipX = origin == Origin90.TopRight || origin == Origin90.BottomRight;
            bool flipY = origin == Origin90.BottomLeft || origin == Origin90.BottomRight;
            if (flipX != flipY)
                clockwise = !clockwise;

            float ratio = clockwise ? amount : (1 - amount);
            float tan = Mathf.Tan(Mathf.PI * 0.5f * ratio);
            bool thresold = false;
            if (ratio != 1)
                thresold = (vertRect.height / vertRect.width - tan) > 0;
            if (!clockwise)
                thresold = !thresold;
            float x = vertRect.x + (ratio == 0 ? float.MaxValue : (vertRect.height / tan));
            float y = vertRect.y + (ratio == 1 ? float.MaxValue : (vertRect.width * tan));
            float x2 = x;
            float y2 = y;
            if (flipX)
                x2 = vertRect.width - x;
            if (flipY)
                y2 = vertRect.height - y;
            float xMin = flipX ? (vertRect.width - vertRect.x) : vertRect.xMin;
            float yMin = flipY ? (vertRect.height - vertRect.y) : vertRect.yMin;
            float xMax = flipX ? -vertRect.xMin : vertRect.xMax;
            float yMax = flipY ? -vertRect.yMin : vertRect.yMax;

            vb.AddVert(new Vector3(xMin, yMin, 0));

            if (clockwise)
                vb.AddVert(new Vector3(xMax, yMin, 0));

            if (y > vertRect.yMax)
            {
                if (thresold)
                    vb.AddVert(new Vector3(x2, yMax, 0));
                else
                    vb.AddVert(new Vector3(xMax, yMax, 0));
            }
            else
                vb.AddVert(new Vector3(xMax, y2, 0));

            if (x > vertRect.xMax)
            {
                if (thresold)
                    vb.AddVert(new Vector3(xMax, y2, 0));
                else
                    vb.AddVert(new Vector3(xMax, yMax, 0));
            }
            else
                vb.AddVert(new Vector3(x2, yMax, 0));

            if (!clockwise)
                vb.AddVert(new Vector3(xMin, yMax, 0));

            if (flipX == flipY)
            {
                vb.AddTriangle(0, 1, 2);
                vb.AddTriangle(0, 2, 3);
            }
            else
            {
                vb.AddTriangle(2, 1, 0);
                vb.AddTriangle(3, 2, 0);
            }
        }

        //8 vertex
        static void FillRadial180(VertexBuffer vb, Rect vertRect, Origin180 origin, float amount, bool clockwise)
        {
            switch (origin)
            {
                case Origin180.Top:
                    if (amount <= 0.5f)
                    {
                        vertRect.width /= 2;
                        if (clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.TopLeft : Origin90.TopRight, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-4);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.width /= 2;
                        if (!clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.TopRight : Origin90.TopLeft, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.x += vertRect.width;
                        else
                            vertRect.x -= vertRect.width;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }
                    break;

                case Origin180.Bottom:
                    if (amount <= 0.5f)
                    {
                        vertRect.width /= 2;
                        if (!clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.BottomRight : Origin90.BottomLeft, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-4);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.width /= 2;
                        if (clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.BottomLeft : Origin90.BottomRight, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.x -= vertRect.width;
                        else
                            vertRect.x += vertRect.width;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }
                    break;

                case Origin180.Left:
                    if (amount <= 0.5f)
                    {
                        vertRect.height /= 2;
                        if (!clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.BottomLeft : Origin90.TopLeft, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-4);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.height /= 2;
                        if (clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.TopLeft : Origin90.BottomLeft, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.y -= vertRect.height;
                        else
                            vertRect.y += vertRect.height;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }
                    break;

                case Origin180.Right:
                    if (amount <= 0.5f)
                    {
                        vertRect.height /= 2;
                        if (clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.TopRight : Origin90.BottomRight, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-4);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.height /= 2;
                        if (!clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial90(vb, vertRect, clockwise ? Origin90.BottomRight : Origin90.TopRight, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.y += vertRect.height;
                        else
                            vertRect.y -= vertRect.height;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }
                    break;
            }
        }

        //12 vertex
        static void FillRadial360(VertexBuffer vb, Rect vertRect, Origin360 origin, float amount, bool clockwise)
        {
            switch (origin)
            {
                case Origin360.Top:
                    if (amount < 0.5f)
                    {
                        vertRect.width /= 2;
                        if (clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Left : Origin180.Right, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-8);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.width /= 2;
                        if (!clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Right : Origin180.Left, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.x += vertRect.width;
                        else
                            vertRect.x -= vertRect.width;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }

                    break;

                case Origin360.Bottom:
                    if (amount < 0.5f)
                    {
                        vertRect.width /= 2;
                        if (!clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Right : Origin180.Left, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-8);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.width /= 2;
                        if (clockwise)
                            vertRect.x += vertRect.width;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Left : Origin180.Right, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.x -= vertRect.width;
                        else
                            vertRect.x += vertRect.width;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }
                    break;

                case Origin360.Left:
                    if (amount < 0.5f)
                    {
                        vertRect.height /= 2;
                        if (!clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Bottom : Origin180.Top, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-8);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.height /= 2;
                        if (clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Top : Origin180.Bottom, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.y -= vertRect.height;
                        else
                            vertRect.y += vertRect.height;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }
                    break;

                case Origin360.Right:
                    if (amount < 0.5f)
                    {
                        vertRect.height /= 2;
                        if (clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Top : Origin180.Bottom, amount / 0.5f, clockwise);
                        Vector3 vec = vb.GetPosition(-8);
                        vb.AddQuad(new Rect(vec.x, vec.y, 0, 0));
                        vb.AddTriangles(-4);
                    }
                    else
                    {
                        vertRect.height /= 2;
                        if (!clockwise)
                            vertRect.y += vertRect.height;

                        FillRadial180(vb, vertRect, clockwise ? Origin180.Bottom : Origin180.Top, (amount - 0.5f) / 0.5f, clockwise);

                        if (clockwise)
                            vertRect.y += vertRect.height;
                        else
                            vertRect.y -= vertRect.height;
                        vb.AddQuad(vertRect);
                        vb.AddTriangles(-4);
                    }
                    break;
            }
        }
    }
}