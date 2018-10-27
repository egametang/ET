using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class HtmlInput : IHtmlObject
	{
		public GTextInput textInput { get; private set; }

		RichTextField _owner;
		HtmlElement _element;
		bool _hidden;

		Shape _border;
		int _borderSize;

		public static int defaultBorderSize = 2;
		public static Color defaultBorderColor = ToolSet.ColorFromRGB(0xA9A9A9);

		public HtmlInput()
		{
			textInput = (GTextInput)UIObjectFactory.NewObject(ObjectType.InputText);
			textInput.gameObjectName = "HtmlInput";
			textInput.verticalAlign = VertAlignType.Middle;

			_border = new Shape();
			_border.graphics.dontClip = true;
			((InputTextField)textInput.displayObject).AddChildAt(_border, 0);
		}

		public DisplayObject displayObject
		{
			get { return textInput.displayObject; }
		}

		public HtmlElement element
		{
			get { return _element; }
		}

		public float width
		{
			get { return _hidden ? 0 : _border.width; }
		}

		public float height
		{
			get { return _hidden ? 0 : _border.height; }
		}

		public void Create(RichTextField owner, HtmlElement element)
		{
			_owner = owner;
			_element = element;

			string type = element.GetString("type");
			if (type != null)
				type = type.ToLower();

			_hidden = type == "hidden";
			if (!_hidden)
			{
				int width = element.GetInt("width", 0);
				int height = element.GetInt("height", 0);
				_borderSize = element.GetInt("border", defaultBorderSize);
				Color borderColor = element.GetColor("border-color", defaultBorderColor);

				if (width == 0)
				{
					width = element.space;
					if (width > _owner.width / 2 || width < 100)
						width = (int)_owner.width / 2;
				}
				if (height == 0)
					height = element.format.size + 10 + _borderSize * 2;

				textInput.textFormat = element.format;
				textInput.displayAsPassword = type == "password";
				textInput.SetSize(width - _borderSize * 2, height - _borderSize * 2);
				textInput.maxLength = element.GetInt("maxlength", int.MaxValue);

				_border.SetXY(-_borderSize, -_borderSize);
				_border.SetSize(width, height);
				_border.DrawRect(_borderSize, borderColor, new Color(0, 0, 0, 0));
			}
			textInput.text = element.GetString("value");
		}

		public void SetPosition(float x, float y)
		{
			if (!_hidden)
				textInput.SetXY(x + _borderSize, y + _borderSize);
		}

		public void Add()
		{
			if (!_hidden)
				_owner.AddChild(textInput.displayObject);
		}

		public void Remove()
		{
			if (!_hidden && textInput.displayObject.parent != null)
				_owner.RemoveChild(textInput.displayObject);
		}

		public void Release()
		{
			textInput.RemoveEventListeners();
			textInput.text = null;

			_owner = null;
			_element = null;
		}

		public void Dispose()
		{
			textInput.Dispose();
		}
	}
}
