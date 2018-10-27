using System;
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

			if (displayObject != null)
			{
				if (displayObject.parent != null)
					displayObject.parent.RemoveChild(displayObject, true);
				else
					displayObject.Dispose();
				_shape = null;
				displayObject.gOwner = null;
				displayObject = null;
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
			get
			{
				if (_shape != null)
					return _shape;

				if (displayObject != null)
					displayObject.Dispose();

				_shape = new Shape();
				_shape.gOwner = this;
				displayObject = _shape;
				if (parent != null)
					parent.ChildStateChanged(this);
				HandleSizeChanged();
				HandleScaleChanged();
				HandlePositionChanged();
				_shape.alpha = this.alpha;
				_shape.rotation = this.rotation;
				_shape.visible = this.visible;

				return _shape;
			}
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
			this.shape.DrawRect(lineSize, lineColor, fillColor);
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
			this.shape.DrawRoundRect(fillColor, corner);
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
			this.shape.DrawEllipse(fillColor);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="fillColor"></param>
		public void DrawPolygon(float aWidth, float aHeight, Vector2[] points, Color fillColor)
		{
			this.SetSize(aWidth, aHeight);
			this.shape.DrawPolygon(points, fillColor);
		}

		override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
		{
			buffer.Seek(beginPos, 5);

			int type = buffer.ReadByte();
			int lineSize = 0;
			Color lineColor = new Color();
			Color fillColor = new Color();
			float[] cornerRadius = null;

			if (type != 0)
			{
				_shape = new Shape();
				_shape.gOwner = this;
				displayObject = _shape;

				lineSize = buffer.ReadInt();
				lineColor = buffer.ReadColor();
				fillColor = buffer.ReadColor();
				cornerRadius = null;
				if (buffer.ReadBool())
				{
					cornerRadius = new float[4];
					for (int i = 0; i < 4; i++)
						cornerRadius[i] = buffer.ReadFloat();
				}
			}
			base.Setup_BeforeAdd(buffer, beginPos);

			if (_shape != null)
			{
				if (type == 1)
				{
					if (cornerRadius != null)
						DrawRoundRect(this.width, this.height, fillColor, cornerRadius);
					else
						DrawRect(this.width, this.height, lineSize, lineColor, fillColor);
				}
				else
					DrawEllipse(this.width, this.height, fillColor);
			}
		}
	}
}
