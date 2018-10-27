using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 长按手势。当按下一定时间后(duration)，派发onAction，如果once为false，则间隔duration时间持续派发onAction，直到手指释放。
	/// </summary>
	public class LongPressGesture : EventDispatcher
	{
		/// <summary>
		/// 
		/// </summary>
		public GObject host { get; private set; }

		/// <summary>
		/// 当手指按下时派发该事件。
		/// </summary>
		public EventListener onBegin { get; private set; }
		/// <summary>
		/// 手指离开屏幕时派发该事件。
		/// </summary>
		public EventListener onEnd { get; private set; }
		/// <summary>
		/// 当手指按下后一段时间后派发该事件。并且在手指离开前按一定周期派发该事件。
		/// </summary>
		public EventListener onAction { get; private set; }

		/// <summary>
		/// 第一次派发事件的触发时间。单位秒
		/// </summary>
		public float trigger;

		/// <summary>
		/// 派发onAction事件的时间间隔。单位秒。
		/// </summary>
		public float interval;

		/// <summary>
		/// 如果为真，则onAction再一次按下释放过程只派发一次。如果为假，则每隔duration时间派发一次。
		/// </summary>
		public bool once;

		/// <summary>
		/// 手指按住后，移动超出此半径范围则手势停止。
		/// </summary>
		public int holdRangeRadius;

		Vector2 _startPoint;
		bool _started;

		public static float TRIGGER = 1.5f;
		public static float INTERVAL = 1f;

		public LongPressGesture(GObject host)
		{
			this.host = host;
			trigger = TRIGGER;
			interval = INTERVAL;
			holdRangeRadius = 50;
			Enable(true);

			onBegin = new EventListener(this, "onLongPressBegin");
			onEnd = new EventListener(this, "onLongPressEnd");
			onAction = new EventListener(this, "onLongPressAction");
		}

		public void Dispose()
		{
			Enable(false);
			host = null;
		}

		public void Enable(bool value)
		{
			if (value)
			{
				if (host == GRoot.inst)
				{
					Stage.inst.onTouchBegin.Add(__touchBegin);
					Stage.inst.onTouchEnd.Add(__touchEnd);
				}
				else
				{
					host.onTouchBegin.Add(__touchBegin);
					host.onTouchEnd.Add(__touchEnd);
				}
			}
			else
			{
				if (host == GRoot.inst)
				{
					Stage.inst.onTouchBegin.Remove(__touchBegin);
					Stage.inst.onTouchEnd.Remove(__touchEnd);
				}
				else
				{
					host.onTouchBegin.Remove(__touchBegin);
					host.onTouchEnd.Remove(__touchEnd);
				}
				Timers.inst.Remove(__timer);
			}
		}

		public void Cancel()
		{
			Timers.inst.Remove(__timer);
			_started = false;
		}

		void __touchBegin(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			_startPoint = host.GlobalToLocal(new Vector2(evt.x, evt.y));
			_started = false;

			Timers.inst.Add(trigger, 1, __timer);
			context.CaptureTouch();
		}

		void __timer(object param)
		{
			Vector2 pt = host.GlobalToLocal(Stage.inst.touchPosition);
			if (Mathf.Pow(pt.x - _startPoint.x, 2) + Mathf.Pow(pt.y - _startPoint.y, 2) > Mathf.Pow(holdRangeRadius, 2))
			{
				Timers.inst.Remove(__timer);
				return;
			}
			if (!_started)
			{
				_started = true;
				onBegin.Call();

				if (!once)
					Timers.inst.Add(interval, 0, __timer);
			}

			onAction.Call();
		}

		void __touchEnd(EventContext context)
		{
			Timers.inst.Remove(__timer);

			if (_started)
			{
				_started = false;
				onEnd.Call();
			}
		}
	}
}
