using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// GGraph class.
    /// 对应编辑器里的图形对象。图形有两个用途，一是用来显示简单的图形，例如矩形等；二是作为一个占位的用途，
    /// 可以将本对象替换为其他对象，或者在它的前后添加其他对象，相当于一个位置和深度的占位；还可以直接将内容设置
    /// 为原生对象。
    /// </summary>
    public class GGraph : GObject, IColorGear
    {
        Shape _shape;

        public GGraph()
        {
        }

        override protected void CreateDisplayObject()
        {
            _shape = new Shape();
            _shape.gOwner = this;
            displayObject = _shape;
        }

        /// <summary>
        /// Replace this object to another object in the display list.
        /// 在显示列表中，将指定对象取代这个图形对象。这个图形对象相当于一个占位的用途。
        /// </summary>
        /// <param name="target">Target object.</param>
        public void ReplaceMe(GObject target)
        {
            if (parent == null)
                throw new Exception("parent not set");

            target.name = this.name;
            target.alpha = this.alpha;
            target.rotation = this.rotation;
            target.visible = this.visible;
            target.touchable = this.touchable;
            target.grayed = this.grayed;
            target.SetXY(this.x, this.y);
            target.SetSize(this.width, this.height);

            int index = parent.GetChildIndex(this);
            parent.AddChildAt(target, index);
            target.relations.CopyFrom(this.relations);

            parent.RemoveChild(this, true);
        }

        /// <summary>
        /// Add another object before this object.
        /// 在显示列表中，将另一个对象插入到这个对象的前面。
        /// </summary>
        /// <param name="target">Target object.</param>
        public void AddBeforeMe(GObject target)
        {
            if (parent == null)
                throw new Exception("parent not set");

            int index = parent.GetChildIndex(this);
            parent.AddChildAt(target, index);
        }

        /// <summary>
        /// Add another object after this object.
        /// 在显示列表中，将另一个对象插入到这个对象的后面。
        /// </summary>
        /// <param name="target">Target object.</param>
        public void AddAfterMe(GObject target)
        {
            if (parent == null)
                throw new Exception("parent not set");

            int index = parent.GetChildIndex(this);
            index++;
            parent.AddChildAt(target, index);
        }

        /// <summary>
        /// 设置内容为一个原生对象。这个图形对象相当于一个占位的用途。
        /// </summary>
        /// <param name="obj">原生对象</param>
        public void SetNativeObject(DisplayObject obj)
        {
            if (displayObject == obj)
                return;

            if (_shape != null)
            {
                if (_shape.parent != null)
                    _shape.parent.RemoveChild(displayObject, true);
                else
                    _shape.Dispose();
                _shape.gOwner = null;
                _shape = null;
            }

            displayObject = obj;

            if (displayObject != null)
            {
                displayObject.alpha = this.alpha;
                displayObject.rotation = this.rotation;
                displayObject.visible = this.visible;
                displayObject.touchable = this.touchable;
                displayObject.gOwner = this;
            }

            if (parent != null)
                parent.ChildStateChanged(this);
            HandlePositionChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        public Color color
        {
            get
            {
                if (_shape != null)
                    return _shape.color;
                else
                    return Color.clear;
            }
            set
            {
                if (_shape != null && _shape.color != value)
                {
                    _shape.color = value;
                    UpdateGear(4);
                }
            }
        }

        /// <summary>
        /// Get the shape object. It can be used for drawing.
        /// 获取图形的原生对象，可用于绘制图形。
        /// </summary>
        public Shape shape
        {
            get { return _shape; }
        }

        /// <summary>
        /// Draw a rectangle.
        /// 画矩形。
        /// </summary>
        /// <param name="aWidth">Width</param>
        /// <param name="aHeight">Height</param>
        /// <param name="lineSize">Line size</param>
        /// <param name="lineColor">Line color</param>
        /// <param name="fillColor">Fill color</param>
        public void DrawRect(float aWidth, float aHeight, int lineSize, Color lineColor, Color fillColor)
        {
            this.SetSize(aWidth, aHeight);
            _shape.DrawRect(lineSize, lineColor, fillColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        /// <param name="fillColor"></param>
        /// <param name="corner"></param>
        public void DrawRoundRect(float aWidth, float aHeight, Color fillColor, float[] corner)
        {
            this.SetSize(aWidth, aHeight);
            this.shape.DrawRoundRect(0, Color.white, fillColor, corner[0], corner[1], corner[2], corner[3]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        /// <param name="fillColor"></param>
        public void DrawEllipse(float aWidth, float aHeight, Color fillColor)
        {
            this.SetSize(aWidth, aHeight);
            _shape.DrawEllipse(fillColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        /// <param name="points"></param>
        /// <param name="fillColor"></param>
        public void DrawPolygon(float aWidth, float aHeight, IList<Vector2> points, Color fillColor)
        {
            this.SetSize(aWidth, aHeight);
            _shape.DrawPolygon(points, fillColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        /// <param name="points"></param>
        /// <param name="fillColor"></param>
        /// <param name="lineSize"></param>
        /// <param name="lineColor"></param>
        public void DrawPolygon(float aWidth, float aHeight, IList<Vector2> points, Color fillColor, float lineSize, Color lineColor)
        {
            this.SetSize(aWidth, aHeight);
            _shape.DrawPolygon(points, fillColor, lineSize, lineColor);
        }

        override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 5);

            int type = buffer.ReadByte();
            if (type != 0)
            {
                int lineSize = buffer.ReadInt();
                Color lineColor = buffer.ReadColor();
                Color fillColor = buffer.ReadColor();
                bool roundedRect = buffer.ReadBool();
                Vector4 cornerRadius = new Vector4();
                if (roundedRect)
                {
                    for (int i = 0; i < 4; i++)
                        cornerRadius[i] = buffer.ReadFloat();
                }

                if (type == 1)
                {
                    if (roundedRect)
                        _shape.DrawRoundRect(lineSize, lineColor, fillColor, cornerRadius.x, cornerRadius.y, cornerRadius.z, cornerRadius.w);
                    else
                        _shape.DrawRect(lineSize, lineColor, fillColor);
                }
                else if (type == 2)
                    _shape.DrawEllipse(lineSize, fillColor, lineColor, fillColor, 0, 360);
                else if (type == 3)
                {
                    int cnt = buffer.ReadShort() / 2;
                    Vector2[] points = new Vector2[cnt];
                    for (int i = 0; i < cnt; i++)
                        points[i].Set(buffer.ReadFloat(), buffer.ReadFloat());

                    _shape.DrawPolygon(points, fillColor, lineSize, lineColor);
                }
                else if (type == 4)
                {
                    int sides = buffer.ReadShort();
                    float startAngle = buffer.ReadFloat();
                    int cnt = buffer.ReadShort();
                    float[] distances = null;
                    if (cnt > 0)
                    {
                        distances = new float[cnt];
                        for (int i = 0; i < cnt; i++)
                            distances[i] = buffer.ReadFloat();
                    }

                    _shape.DrawRegularPolygon(sides, lineSize, fillColor, lineColor, fillColor, startAngle, distances);
                }
            }
        }
    }
}
