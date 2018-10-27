using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	class RelationDef
	{
		public bool percent;
		public RelationType type;
		public int axis;

		public void copyFrom(RelationDef source)
		{
			this.percent = source.percent;
			this.type = source.type;
			this.axis = source.axis;
		}
	}

	class RelationItem
	{
		GObject _owner;
		GObject _target;
		List<RelationDef> _defs;
		Vector4 _targetData;

		public RelationItem(GObject owner)
		{
			_owner = owner;
			_defs = new List<RelationDef>();
		}

		public GObject target
		{
			get { return _target; }
			set
			{
				if (_target != value)
				{
					if (_target != null)
						ReleaseRefTarget(_target);
					_target = value;
					if (_target != null)
						AddRefTarget(_target);
				}
			}
		}

		public void Add(RelationType relationType, bool usePercent)
		{
			if (relationType == RelationType.Size)
			{
				Add(RelationType.Width, usePercent);
				Add(RelationType.Height, usePercent);
				return;
			}

			int dc = _defs.Count;
			for (int k = 0; k < dc; k++)
			{
				if (_defs[k].type == relationType)
					return;
			}

			InternalAdd(relationType, usePercent);
		}

		public void InternalAdd(RelationType relationType, bool usePercent)
		{
			if (relationType == RelationType.Size)
			{
				InternalAdd(RelationType.Width, usePercent);
				InternalAdd(RelationType.Height, usePercent);
				return;
			}

			RelationDef info = new RelationDef();
			info.percent = usePercent;
			info.type = relationType;
			info.axis = (relationType <= RelationType.Right_Right || relationType == RelationType.Width || relationType >= RelationType.LeftExt_Left && relationType <= RelationType.RightExt_Right) ? 0 : 1;
			_defs.Add(info);

			//当使用中线关联时，因为需要除以2，很容易因为奇数宽度/高度造成小数点坐标；当使用百分比时，也会造成小数坐标；
			//所以设置了这类关联的对象，自动启用pixelSnapping
			if (usePercent || relationType == RelationType.Left_Center || relationType == RelationType.Center_Center || relationType == RelationType.Right_Center
					|| relationType == RelationType.Top_Middle || relationType == RelationType.Middle_Middle || relationType == RelationType.Bottom_Middle)
				_owner.pixelSnapping = true;
		}

		public void Remove(RelationType relationType)
		{
			if (relationType == RelationType.Size)
			{
				Remove(RelationType.Width);
				Remove(RelationType.Height);
				return;
			}

			int dc = _defs.Count;
			for (int k = 0; k < dc; k++)
			{
				if (_defs[k].type == relationType)
				{
					_defs.RemoveAt(k);
					break;
				}
			}
		}

		public void CopyFrom(RelationItem source)
		{
			this.target = source.target;

			_defs.Clear();
			foreach (RelationDef info in source._defs)
			{
				RelationDef info2 = new RelationDef();
				info2.copyFrom(info);
				_defs.Add(info2);
			}
		}

		public void Dispose()
		{
			if (_target != null)
			{
				ReleaseRefTarget(_target);
				_target = null;
			}
		}

		public bool isEmpty
		{
			get { return _defs.Count == 0; }
		}

		public void ApplyOnSelfSizeChanged(float dWidth, float dHeight, bool applyPivot)
		{
			int cnt = _defs.Count;
			if (cnt == 0)
				return;

			float ox = _owner.x;
			float oy = _owner.y;

			for (int i = 0; i < cnt; i++)
			{
				RelationDef info = _defs[i];
				switch (info.type)
				{
					case RelationType.Center_Center:
						_owner.x -= (0.5f - (applyPivot ? _owner.pivotX : 0)) * dWidth;
						break;

					case RelationType.Right_Center:
					case RelationType.Right_Left:
					case RelationType.Right_Right:
						_owner.x -= (1 - (applyPivot ? _owner.pivotX : 0)) * dWidth;
						break;

					case RelationType.Middle_Middle:
						_owner.y -= (0.5f - (applyPivot ? _owner.pivotY : 0)) * dHeight;
						break;

					case RelationType.Bottom_Middle:
					case RelationType.Bottom_Top:
					case RelationType.Bottom_Bottom:
						_owner.y -= (1 - (applyPivot ? _owner.pivotY : 0)) * dHeight;
						break;
				}
			}

			if (!Mathf.Approximately(ox, _owner.x) || !Mathf.Approximately(oy, _owner.y))
			{
				ox = _owner.x - ox;
				oy = _owner.y - oy;

				_owner.UpdateGearFromRelations(1, ox, oy);

				if (_owner.parent != null)
				{
					int transCount = _owner.parent._transitions.Count;
					for (int i = 0; i < transCount; i++)
						_owner.parent._transitions[i].UpdateFromRelations(_owner.id, ox, oy);
				}
			}
		}

		void ApplyOnXYChanged(RelationDef info, float dx, float dy)
		{
			float tmp;
			switch (info.type)
			{
				case RelationType.Left_Left:
				case RelationType.Left_Center:
				case RelationType.Left_Right:
				case RelationType.Center_Center:
				case RelationType.Right_Left:
				case RelationType.Right_Center:
				case RelationType.Right_Right:
					_owner.x += dx;
					break;

				case RelationType.Top_Top:
				case RelationType.Top_Middle:
				case RelationType.Top_Bottom:
				case RelationType.Middle_Middle:
				case RelationType.Bottom_Top:
				case RelationType.Bottom_Middle:
				case RelationType.Bottom_Bottom:
					_owner.y += dy;
					break;

				case RelationType.Width:
				case RelationType.Height:
					break;

				case RelationType.LeftExt_Left:
				case RelationType.LeftExt_Right:
					tmp = _owner.xMin;
					_owner.width = _owner._rawWidth - dx;
					_owner.xMin = tmp + dx;
					break;

				case RelationType.RightExt_Left:
				case RelationType.RightExt_Right:
					tmp = _owner.xMin;
					_owner.width = _owner._rawWidth + dx;
					_owner.xMin = tmp;
					break;

				case RelationType.TopExt_Top:
				case RelationType.TopExt_Bottom:
					tmp = _owner.yMin;
					_owner.height = _owner._rawHeight - dy;
					_owner.yMin = tmp + dy;
					break;

				case RelationType.BottomExt_Top:
				case RelationType.BottomExt_Bottom:
					tmp = _owner.yMin;
					_owner.height = _owner._rawHeight + dy;
					_owner.yMin = tmp;
					break;
			}
		}

		void ApplyOnSizeChanged(RelationDef info)
		{
			float pos = 0, pivot = 0, delta = 0;
			if (info.axis == 0)
			{
				if (_target != _owner.parent)
				{
					pos = _target.x;
					if (_target.pivotAsAnchor)
						pivot = _target.pivotX;
				}

				if (info.percent)
				{
					if (_targetData.z != 0)
						delta = _target._width / _targetData.z;
				}
				else
					delta = _target._width - _targetData.z;
			}
			else
			{
				if (_target != _owner.parent)
				{
					pos = _target.y;
					if (_target.pivotAsAnchor)
						pivot = _target.pivotY;
				}

				if (info.percent)
				{
					if (_targetData.w != 0)
						delta = _target._height / _targetData.w;
				}
				else
					delta = _target._height - _targetData.w;
			}

			float v, tmp;

			switch (info.type)
			{
				case RelationType.Left_Left:
					if (info.percent)
						_owner.xMin = pos + (_owner.xMin - pos) * delta;
					else if (pivot != 0)
						_owner.x += delta * (-pivot);
					break;
				case RelationType.Left_Center:
					if (info.percent)
						_owner.xMin = pos + (_owner.xMin - pos) * delta;
					else
						_owner.x += delta * (0.5f - pivot);
					break;
				case RelationType.Left_Right:
					if (info.percent)
						_owner.xMin = pos + (_owner.xMin - pos) * delta;
					else
						_owner.x += delta * (1 - pivot);
					break;
				case RelationType.Center_Center:
					if (info.percent)
						_owner.xMin = pos + (_owner.xMin + _owner._rawWidth * 0.5f - pos) * delta - _owner._rawWidth * 0.5f;
					else
						_owner.x += delta * (0.5f - pivot);
					break;
				case RelationType.Right_Left:
					if (info.percent)
						_owner.xMin = pos + (_owner.xMin + _owner._rawWidth - pos) * delta - _owner._rawWidth;
					else if (pivot != 0)
						_owner.x += delta * (-pivot);
					break;
				case RelationType.Right_Center:
					if (info.percent)
						_owner.xMin = pos + (_owner.xMin + _owner._rawWidth - pos) * delta - _owner._rawWidth;
					else
						_owner.x += delta * (0.5f - pivot);
					break;
				case RelationType.Right_Right:
					if (info.percent)
						_owner.xMin = pos + (_owner.xMin + _owner._rawWidth - pos) * delta - _owner._rawWidth;
					else
						_owner.x += delta * (1 - pivot);
					break;

				case RelationType.Top_Top:
					if (info.percent)
						_owner.yMin = pos + (_owner.yMin - pos) * delta;
					else if (pivot != 0)
						_owner.y += delta * (-pivot);
					break;
				case RelationType.Top_Middle:
					if (info.percent)
						_owner.yMin = pos + (_owner.yMin - pos) * delta;
					else
						_owner.y += delta * (0.5f - pivot);
					break;
				case RelationType.Top_Bottom:
					if (info.percent)
						_owner.yMin = pos + (_owner.yMin - pos) * delta;
					else
						_owner.y += delta * (1 - pivot);
					break;
				case RelationType.Middle_Middle:
					if (info.percent)
						_owner.yMin = pos + (_owner.yMin + _owner._rawHeight * 0.5f - pos) * delta - _owner._rawHeight * 0.5f;
					else
						_owner.y += delta * (0.5f - pivot);
					break;
				case RelationType.Bottom_Top:
					if (info.percent)
						_owner.yMin = pos + (_owner.yMin + _owner._rawHeight - pos) * delta - _owner._rawHeight;
					else if (pivot != 0)
						_owner.y += delta * (-pivot);
					break;
				case RelationType.Bottom_Middle:
					if (info.percent)
						_owner.yMin = pos + (_owner.yMin + _owner._rawHeight - pos) * delta - _owner._rawHeight;
					else
						_owner.y += delta * (0.5f - pivot);
					break;
				case RelationType.Bottom_Bottom:
					if (info.percent)
						_owner.yMin = pos + (_owner.yMin + _owner._rawHeight - pos) * delta - _owner._rawHeight;
					else
						_owner.y += delta * (1 - pivot);
					break;

				case RelationType.Width:
					if (_owner.underConstruct && _owner == _target.parent)
						v = _owner.sourceWidth - _target.initWidth;
					else
						v = _owner._rawWidth - _targetData.z;
					if (info.percent)
						v = v * delta;
					if (_target == _owner.parent)
					{
						if (_owner.pivotAsAnchor)
						{
							tmp = _owner.xMin;
							_owner.SetSize(_target._width + v, _owner._rawHeight, true);
							_owner.xMin = tmp;
						}
						else
							_owner.SetSize(_target._width + v, _owner._rawHeight, true);
					}
					else
						_owner.width = _target._width + v;
					break;
				case RelationType.Height:
					if (_owner.underConstruct && _owner == _target.parent)
						v = _owner.sourceHeight - _target.initHeight;
					else
						v = _owner._rawHeight - _targetData.w;
					if (info.percent)
						v = v * delta;
					if (_target == _owner.parent)
					{
						if (_owner.pivotAsAnchor)
						{
							tmp = _owner.yMin;
							_owner.SetSize(_owner._rawWidth, _target._height + v, true);
							_owner.yMin = tmp;
						}
						else
							_owner.SetSize(_owner._rawWidth, _target._height + v, true);
					}
					else
						_owner.height = _target._height + v;
					break;

				case RelationType.LeftExt_Left:
					tmp = _owner.xMin;
					if (info.percent)
						v = pos + (tmp - pos) * delta - tmp;
					else
						v = delta * (-pivot);
					_owner.width = _owner._rawWidth - v;
					_owner.xMin = tmp + v;
					break;
				case RelationType.LeftExt_Right:
					tmp = _owner.xMin;
					if (info.percent)
						v = pos + (tmp - pos) * delta - tmp;
					else
						v = delta * (1 - pivot);
					_owner.width = _owner._rawWidth - v;
					_owner.xMin = tmp + v;
					break;
				case RelationType.RightExt_Left:
					tmp = _owner.xMin;
					if (info.percent)
						v = pos + (tmp + _owner._rawWidth - pos) * delta - (tmp + _owner._rawWidth);
					else
						v = delta * (-pivot);
					_owner.width = _owner._rawWidth + v;
					_owner.xMin = tmp;
					break;
				case RelationType.RightExt_Right:
					tmp = _owner.xMin;
					if (info.percent)
					{
						if (_owner == _target.parent)
						{
							if (_owner.underConstruct)
								_owner.width = pos + _target._width - _target._width * pivot +
									(_owner.sourceWidth - pos - _target.initWidth + _target.initWidth * pivot) * delta;
							else
								_owner.width = pos + (_owner._rawWidth - pos) * delta;
						}
						else
						{
							v = pos + (tmp + _owner._rawWidth - pos) * delta - (tmp + _owner._rawWidth);
							_owner.width = _owner._rawWidth + v;
							_owner.xMin = tmp;
						}
					}
					else
					{
						if (_owner == _target.parent)
						{
							if (_owner.underConstruct)
								_owner.width = _owner.sourceWidth + (_target._width - _target.initWidth) * (1 - pivot);
							else
								_owner.width = _owner._rawWidth + delta * (1 - pivot);
						}
						else
						{
							v = delta * (1 - pivot);
							_owner.width = _owner._rawWidth + v;
							_owner.xMin = tmp;
						}
					}
					break;
				case RelationType.TopExt_Top:
					tmp = _owner.yMin;
					if (info.percent)
						v = pos + (tmp - pos) * delta - tmp;
					else
						v = delta * (-pivot);
					_owner.height = _owner._rawHeight - v;
					_owner.yMin = tmp + v;
					break;
				case RelationType.TopExt_Bottom:
					tmp = _owner.yMin;
					if (info.percent)
						v = pos + (tmp - pos) * delta - tmp;
					else
						v = delta * (1 - pivot);
					_owner.height = _owner._rawHeight - v;
					_owner.yMin = tmp + v;
					break;
				case RelationType.BottomExt_Top:
					tmp = _owner.yMin;
					if (info.percent)
						v = pos + (tmp + _owner._rawHeight - pos) * delta - (tmp + _owner._rawHeight);
					else
						v = delta * (-pivot);
					_owner.height = _owner._rawHeight + v;
					_owner.yMin = tmp;
					break;
				case RelationType.BottomExt_Bottom:
					tmp = _owner.yMin;
					if (info.percent)
					{
						if (_owner == _target.parent)
						{
							if (_owner.underConstruct)
								_owner.height = pos + _target._height - _target._height * pivot +
									(_owner.sourceHeight - pos - _target.initHeight + _target.initHeight * pivot) * delta;
							else
								_owner.height = pos + (_owner._rawHeight - pos) * delta;
						}
						else
						{
							v = pos + (tmp + _owner._rawHeight - pos) * delta - (tmp + _owner._rawHeight);
							_owner.height = _owner._rawHeight + v;
							_owner.yMin = tmp;
						}
					}
					else
					{
						if (_owner == _target.parent)
						{
							if (_owner.underConstruct)
								_owner.height = _owner.sourceHeight + (_target._height - _target.initHeight) * (1 - pivot);
							else
								_owner.height = _owner._rawHeight + delta * (1 - pivot);
						}
						else
						{
							v = delta * (1 - pivot);
							_owner.height = _owner._rawHeight + v;
							_owner.yMin = tmp;
						}
					}
					break;
			}
		}

		void AddRefTarget(GObject target)
		{
			if (target != _owner.parent)
				target.onPositionChanged.Add(__targetXYChanged);
			target.onSizeChanged.Add(__targetSizeChanged);
			_targetData.x = _target.x;
			_targetData.y = _target.y;
			_targetData.z = _target._width;
			_targetData.w = _target._height;
		}

		void ReleaseRefTarget(GObject target)
		{
			target.onPositionChanged.Remove(__targetXYChanged);
			target.onSizeChanged.Remove(__targetSizeChanged);
		}

		void __targetXYChanged(EventContext context)
		{
			if (_owner.relations.handling != null
				|| _owner.group != null && _owner.group._updating != 0)
			{
				_targetData.x = _target.x;
				_targetData.y = _target.y;
				return;
			}

			_owner.relations.handling = (GObject)context.sender;

			float ox = _owner.x;
			float oy = _owner.y;
			float dx = _target.x - _targetData.x;
			float dy = _target.y - _targetData.y;

			int cnt = _defs.Count;
			for (int i = 0; i < cnt; i++)
				ApplyOnXYChanged(_defs[i], dx, dy);

			_targetData.x = _target.x;
			_targetData.y = _target.y;

			if (!Mathf.Approximately(ox, _owner.x) || !Mathf.Approximately(oy, _owner.y))
			{
				ox = _owner.x - ox;
				oy = _owner.y - oy;

				_owner.UpdateGearFromRelations(1, ox, oy);

				if (_owner.parent != null)
				{
					int transCount = _owner.parent._transitions.Count;
					for (int i = 0; i < transCount; i++)
						_owner.parent._transitions[i].UpdateFromRelations(_owner.id, ox, oy);
				}
			}

			_owner.relations.handling = null;
		}

		void __targetSizeChanged(EventContext context)
		{
			if (_owner.relations.handling != null
				|| _owner.group != null && _owner.group._updating != 0)
			{
				_targetData.z = _target._width;
				_targetData.w = _target._height;
				return;
			}

			_owner.relations.handling = (GObject)context.sender;

			float ox = _owner.x;
			float oy = _owner.y;
			float ow = _owner._rawWidth;
			float oh = _owner._rawHeight;

			int cnt = _defs.Count;
			for (int i = 0; i < cnt; i++)
				ApplyOnSizeChanged(_defs[i]);

			_targetData.z = _target._width;
			_targetData.w = _target._height;

			if (!Mathf.Approximately(ox, _owner.x) || !Mathf.Approximately(oy, _owner.y))
			{
				ox = _owner.x - ox;
				oy = _owner.y - oy;

				_owner.UpdateGearFromRelations(1, ox, oy);

				if (_owner.parent != null)
				{
					int transCount = _owner.parent._transitions.Count;
					for (int i = 0; i < transCount; i++)
						_owner.parent._transitions[i].UpdateFromRelations(_owner.id, ox, oy);
				}
			}

			if (!Mathf.Approximately(ow, _owner._rawWidth) || !Mathf.Approximately(oh, _owner._rawHeight))
			{
				ow = _owner._rawWidth - ow;
				oh = _owner._rawHeight - oh;

				_owner.UpdateGearFromRelations(2, ow, oh);
			}

			_owner.relations.handling = null;
		}
	}
}
