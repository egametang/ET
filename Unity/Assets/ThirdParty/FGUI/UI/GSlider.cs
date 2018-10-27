using System;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GSlider : GComponent
	{
		double _max;
		double _value;
		ProgressTitleType _titleType;
		bool _reverse;

		GTextField _titleObject;
		GObject _barObjectH;
		GObject _barObjectV;
		float _barMaxWidth;
		float _barMaxHeight;
		float _barMaxWidthDelta;
		float _barMaxHeightDelta;
		GObject _gripObject;
		Vector2 _clickPos;
		float _clickPercent;
		float _barStartX;
		float _barStartY;

		public bool changeOnClick;
		public bool canDrag;

		/// <summary>
		/// 
		/// </summary>
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onGripTouchEnd { get; private set; }

		public GSlider()
		{
			_value = 50;
			_max = 100;
			changeOnClick = true;
			canDrag = true;

			onChanged = new EventListener(this, "onChanged");
			onGripTouchEnd = new EventListener(this, "onGripTouchEnd");
		}

		/// <summary>
		/// 
		/// </summary>
		public ProgressTitleType titleType
		{
			get
			{
				return _titleType;
			}
			set
			{
				if (_titleType != value)
				{
					_titleType = value;
					Update();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double max
		{
			get
			{
				return _max;
			}
			set
			{
				if (_max != value)
				{
					_max = value;
					Update();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public double value
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					_value = value;
					Update();
				}
			}
		}

		private void Update()
		{
			float percent = (float)Math.Min(_value / _max, 1);
			UpdateWidthPercent(percent);
		}

		private void UpdateWidthPercent(float percent)
		{
			if (_titleObject != null)
			{
				switch (_titleType)
				{
					case ProgressTitleType.Percent:
						_titleObject.text = Mathf.RoundToInt(percent * 100) + "%";
						break;

					case ProgressTitleType.ValueAndMax:
						_titleObject.text = Math.Round(_value) + "/" + Math.Round(max);
						break;

					case ProgressTitleType.Value:
						_titleObject.text = "" + Math.Round(_value);
						break;

					case ProgressTitleType.Max:
						_titleObject.text = "" + Math.Round(_max);
						break;
				}
			}

			float fullWidth = this.width - _barMaxWidthDelta;
			float fullHeight = this.height - _barMaxHeightDelta;
			if (!_reverse)
			{
				if (_barObjectH != null)
				{
					if ((_barObjectH is GImage) && ((GImage)_barObjectH).fillMethod != FillMethod.None)
						((GImage)_barObjectH).fillAmount = percent;
					else if ((_barObjectH is GLoader) && ((GLoader)_barObjectH).fillMethod != FillMethod.None)
						((GLoader)_barObjectH).fillAmount = percent;
					else
						_barObjectH.width = Mathf.RoundToInt(fullWidth * percent);
				}
				if (_barObjectV != null)
				{
					if ((_barObjectV is GImage) && ((GImage)_barObjectV).fillMethod != FillMethod.None)
						((GImage)_barObjectV).fillAmount = percent;
					else if ((_barObjectV is GLoader) && ((GLoader)_barObjectV).fillMethod != FillMethod.None)
						((GLoader)_barObjectV).fillAmount = percent;
					else
						_barObjectV.height = Mathf.RoundToInt(fullHeight * percent);
				}
			}
			else
			{
				if (_barObjectH != null)
				{
					if ((_barObjectH is GImage) && ((GImage)_barObjectH).fillMethod != FillMethod.None)
						((GImage)_barObjectH).fillAmount = 1 - percent;
					else if ((_barObjectH is GLoader) && ((GLoader)_barObjectH).fillMethod != FillMethod.None)
						((GLoader)_barObjectH).fillAmount = 1 - percent;
					else
					{
						_barObjectH.width = Mathf.RoundToInt(fullWidth * percent);
						_barObjectH.x = _barStartX + (fullWidth - _barObjectH.width);
					}
				}
				if (_barObjectV != null)
				{
					if ((_barObjectV is GImage) && ((GImage)_barObjectV).fillMethod != FillMethod.None)
						((GImage)_barObjectV).fillAmount = 1 - percent;
					else if ((_barObjectV is GLoader) && ((GLoader)_barObjectV).fillMethod != FillMethod.None)
						((GLoader)_barObjectV).fillAmount = 1 - percent;
					else
					{
						_barObjectV.height = Mathf.RoundToInt(fullHeight * percent);
						_barObjectV.y = _barStartY + (fullHeight - _barObjectV.height);
					}
				}
			}

			InvalidateBatchingState(true);
		}

		override protected void ConstructExtension(ByteBuffer buffer)
		{
			buffer.Seek(0, 6);

			_titleType = (ProgressTitleType)buffer.ReadByte();
			_reverse = buffer.ReadBool();

			_titleObject = GetChild("title") as GTextField;
			_barObjectH = GetChild("bar");
			_barObjectV = GetChild("bar_v");
			_gripObject = GetChild("grip");

			if (_barObjectH != null)
			{
				_barMaxWidth = _barObjectH.width;
				_barMaxWidthDelta = this.width - _barMaxWidth;
				_barStartX = _barObjectH.x;
			}
			if (_barObjectV != null)
			{
				_barMaxHeight = _barObjectV.height;
				_barMaxHeightDelta = this.height - _barMaxHeight;
				_barStartY = _barObjectV.y;
			}

			if (_gripObject != null)
			{
				_gripObject.onTouchBegin.Add(__gripTouchBegin);
				_gripObject.onTouchMove.Add(__gripTouchMove);
				_gripObject.onTouchEnd.Add(__gripTouchEnd);
			}

			onTouchBegin.Add(__barTouchBegin);
		}

		override public void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_AfterAdd(buffer, beginPos);

			if (!buffer.Seek(beginPos, 6))
			{
				Update();
				return;
			}

			if ((ObjectType)buffer.ReadByte() != packageItem.objectType)
			{
				Update();
				return;
			}

			_value = buffer.ReadInt();
			_max = buffer.ReadInt();

			Update();
		}

		override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if (_barObjectH != null)
				_barMaxWidth = this.width - _barMaxWidthDelta;
			if (_barObjectV != null)
				_barMaxHeight = this.height - _barMaxHeightDelta;

			if (!this.underConstruct)
				Update();
		}

		private void __gripTouchBegin(EventContext context)
		{
			this.canDrag = true;

			context.StopPropagation();

			InputEvent evt = context.inputEvent;
			if (evt.button != 0)
				return;

			context.CaptureTouch();

			_clickPos = this.GlobalToLocal(new Vector2(evt.x, evt.y));
			_clickPercent = (float)(_value / _max);
		}

		private void __gripTouchMove(EventContext context)
		{
			if (!this.canDrag)
				return;

			InputEvent evt = context.inputEvent;
			Vector2 pt = this.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (float.IsNaN(pt.x))
				return;

			float deltaX = pt.x - _clickPos.x;
			float deltaY = pt.y - _clickPos.y;
			if (_reverse)
			{
				deltaX = -deltaX;
				deltaY = -deltaY;
			}

			float percent;
			if (_barObjectH != null)
				percent = _clickPercent + deltaX / _barMaxWidth;
			else
				percent = _clickPercent + deltaY / _barMaxHeight;
			if (percent > 1)
				percent = 1;
			else if (percent < 0)
				percent = 0;

			double newValue = percent * _max;
			if (newValue != _value)
			{
				_value = newValue;
				if (onChanged.Call())
					return;
			}
			UpdateWidthPercent(percent);
		}

		private void __gripTouchEnd(EventContext context)
		{
			onGripTouchEnd.Call();
		}

		private void __barTouchBegin(EventContext context)
		{
			if (!changeOnClick)
				return;

			InputEvent evt = context.inputEvent;
			Vector2 pt = _gripObject.GlobalToLocal(new Vector2(evt.x, evt.y));
			float percent = (float)(_value / _max);
			float delta = 0;
			if (_barObjectH != null)
				delta = (pt.x - _gripObject.width / 2) / _barMaxWidth;
			if (_barObjectV != null)
				delta = (pt.y - _gripObject.height / 2) / _barMaxHeight;
			if (_reverse)
				percent -= delta;
			else
				percent += delta;
			if (percent > 1)
				percent = 1;
			else if (percent < 0)
				percent = 0;
			double newValue = percent * _max;
			if (newValue != _value)
			{
				_value = newValue;
				onChanged.Call();
			}
			UpdateWidthPercent(percent);
		}
	}
}
