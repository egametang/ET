using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class HtmlLink : IHtmlObject
	{
		RichTextField _owner;
		HtmlElement _element;
		SelectionShape _shape;

		EventCallback1 _clickHandler;
		EventCallback1 _rolloverHandler;
		EventCallback0 _rolloutHandler;

		public HtmlLink()
		{
			_shape = new SelectionShape();
			_shape.gameObject.name = "HtmlLink";

			_clickHandler = (EventContext context) =>
			{
				_owner.onClickLink.BubbleCall(_element.GetString("href"));
			};
			_rolloverHandler = (EventContext context) =>
			{
				if (_owner.htmlParseOptions.linkHoverBgColor.a > 0)
					_shape.color = _owner.htmlParseOptions.linkHoverBgColor;
			};
			_rolloutHandler = () =>
			{
				if (_owner.htmlParseOptions.linkHoverBgColor.a > 0)
					_shape.color = _owner.htmlParseOptions.linkBgColor;
			};
		}

		public DisplayObject displayObject
		{
			get { return _shape; }
		}

		public HtmlElement element
		{
			get { return _element; }
		}

		public float width
		{
			get { return 0; }
		}

		public float height
		{
			get { return 0; }
		}

		public void Create(RichTextField owner, HtmlElement element)
		{
			_owner = owner;
			_element = element;
			_shape.onClick.Add(_clickHandler);
			_shape.onRollOver.Add(_rolloverHandler);
			_shape.onRollOut.Add(_rolloutHandler);
			_shape.color = _owner.htmlParseOptions.linkBgColor;
		}

		public void SetArea(int startLine, float startCharX, int endLine, float endCharX)
		{
			List<Rect> rects = _shape.rects;
			if (rects == null)
				rects = new List<Rect>(2);
			else
				rects.Clear();
			if (startLine == endLine && startCharX > endCharX)
			{
				float tmp = startCharX;
				startCharX = endCharX;
				endCharX = tmp;
			}
			_owner.textField.GetLinesShape(startLine, startCharX, endLine, endCharX, true, rects);
			_shape.rects = rects;
		}

		public void SetPosition(float x, float y)
		{
			_shape.SetXY(x, y);
		}

		public void Add()
		{
			//add below _shape
			_owner.AddChildAt(_shape, 0);
		}

		public void Remove()
		{
			if (_shape.parent != null)
				_owner.RemoveChild(_shape);
		}

		public void Release()
		{
			_shape.RemoveEventListeners();

			_owner = null;
			_element = null;
		}

		public void Dispose()
		{
			_shape.Dispose();
			_shape = null;
		}
	}
}
