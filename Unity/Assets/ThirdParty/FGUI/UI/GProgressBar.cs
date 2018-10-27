using System;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GProgressBar class.
	/// </summary>
	public class GProgressBar : GComponent
	{
		double _max;
		double _value;
		ProgressTitleType _titleType;
		bool _reverse;

		GTextField _titleObject;
		GMovieClip _aniObject;
		GObject _barObjectH;
		GObject _barObjectV;
		float _barMaxWidth;
		float _barMaxHeight;
		float _barMaxWidthDelta;
		float _barMaxHeightDelta;
		float _barStartX;
		float _barStartY;
		bool _tweening;

		public GProgressBar()
		{
			_value = 50;
			_max = 100;
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
					Update(_value);
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
					Update(_value);
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
				if (_tweening)
				{
					GTween.Kill(this, TweenPropType.Progress, true);
					_tweening = false;
				}

				if (_value != value)
				{
					_value = value;
					Update(_value);
				}
			}
		}

		public bool reverse
		{
			get { return _reverse; }
			set { _reverse = value; }
		}

		/// <summary>
		/// 动态改变进度值。
		/// </summary>
		/// <param name="value"></param>
		/// <param name="duration"></param>
		public GTweener TweenValue(double value, float duration)
		{
			double oldValule = _value;
			_value = value;

			if (_tweening)
				GTween.Kill(this, TweenPropType.Progress, false);
			_tweening = true;
			return GTween.ToDouble(oldValule, _value, duration)
				.SetEase(EaseType.Linear)
				.SetTarget(this, TweenPropType.Progress)
				.OnComplete(() => { _tweening = false; });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newValue"></param>
		public void Update(double newValue)
		{
			float percent = _max != 0 ? (float)Math.Min(newValue / _max, 1) : 0;
			if (_titleObject != null)
			{
				switch (_titleType)
				{
					case ProgressTitleType.Percent:
						_titleObject.text = Mathf.RoundToInt(percent * 100) + "%";
						break;

					case ProgressTitleType.ValueAndMax:
						_titleObject.text = Math.Round(newValue) + "/" + Math.Round(max);
						break;

					case ProgressTitleType.Value:
						_titleObject.text = "" + Math.Round(newValue);
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
			if (_aniObject != null)
				_aniObject.frame = Mathf.RoundToInt(percent * 100);

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
			_aniObject = GetChild("ani") as GMovieClip;

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
		}

		override public void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_AfterAdd(buffer, beginPos);

			if (!buffer.Seek(beginPos, 6))
			{
				Update(_value);
				return;
			}

			if ((ObjectType)buffer.ReadByte() != packageItem.objectType)
			{
				Update(_value);
				return;
			}

			_value = buffer.ReadInt();
			_max = buffer.ReadInt();

			Update(_value);
		}

		override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();

			if (_barObjectH != null)
				_barMaxWidth = this.width - _barMaxWidthDelta;
			if (_barObjectV != null)
				_barMaxHeight = this.height - _barMaxHeightDelta;

			if (!this.underConstruct)
				Update(_value);
		}

		public override void Dispose()
		{
			if (_tweening)
				GTween.Kill(this);
			base.Dispose();
		}
	}
}
