using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class SelectionShape : DisplayObject
	{
		List<Rect> _rects;
		Color _color;

		public SelectionShape()
		{
			CreateGameObject("SelectionShape");
			graphics = new NGraphics(gameObject);
			graphics.texture = NTexture.Empty;
			_color = Color.white;
		}

		/// <summary>
		/// 
		/// </summary>
		public List<Rect> rects
		{
			get { return _rects; }
			set
			{
				_rects = value;

				if (_rects != null)
				{
					int count = _rects.Count;
					if (count > 0)
					{
						_contentRect = _rects[0];
						Rect tmp;
						for (int i = 1; i < count; i++)
						{
							tmp = _rects[i];
							_contentRect = ToolSet.Union(ref _contentRect, ref tmp);
						}
					}
					else
						_contentRect.Set(0, 0, 0, 0);
				}
				else
					_contentRect.Set(0, 0, 0, 0);
				OnSizeChanged(true, true);
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _color; }
			set
			{
				if (_color != value)
				{
					_color = value;
					graphics.Tint(_color);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			if (_rects != null && _rects.Count > 0)
			{
				_rects.Clear();
				_contentRect.Set(0, 0, 0, 0);
				OnSizeChanged(true, true);
				_requireUpdateMesh = true;
			}
		}

		public override void Update(UpdateContext context)
		{
			if (_requireUpdateMesh)
			{
				_requireUpdateMesh = false;
				if (_rects != null && _rects.Count > 0)
				{
					int count = _rects.Count * 4;
					graphics.Alloc(count);
					Rect uvRect = new Rect(0, 0, 1, 1);
					for (int i = 0; i < count; i += 4)
					{
						graphics.FillVerts(i, _rects[i / 4]);
						graphics.FillUV(i, uvRect);
					}
					graphics.FillColors(_color);
					graphics.FillTriangles();
					graphics.UpdateMesh();
				}
				else
					graphics.ClearMesh();
			}

			base.Update(context);
		}

		protected override DisplayObject HitTest()
		{
			if (_rects == null)
				return null;

			int count = _rects.Count;
			if (count == 0)
				return null;

			Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);
			if (!_contentRect.Contains(localPoint))
				return null;

			for (int i = 0; i < count; i++)
			{
				if (_rects[i].Contains(localPoint))
					return this;
			}

			return null;
		}
	}
}
