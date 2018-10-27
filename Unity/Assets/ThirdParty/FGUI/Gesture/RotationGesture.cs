using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 手指反向操作的手势。
	/// </summary>
	public class RotationGesture : EventDispatcher
	{
		/// <summary>
		/// 
		/// </summary>
		public GObject host { get; private set; }

		/// <summary>
		/// 当两个手指开始呈反向操作时派发该事件。
		/// </summary>
		public EventListener onBegin { get; private set; }
		/// <summary>
		/// 当其中一个手指离开屏幕时派发该事件。
		/// </summary>
		public EventListener onEnd { get; private set; }
		/// <summary>
		/// 当手势动作时派发该事件。
		/// </summary>
		public EventListener onAction { get; private set; }

		/// <summary>
		/// 总共旋转的角度。
		/// </summary>
		public float rotation;

		/// <summary>
		/// 从上次通知后的改变量。
		/// </summary>
		public float delta;

		/// <summary>
		/// 是否把变化量强制为整数。默认true。
		/// </summary>
		public bool snapping;

		Vector2 _startVector;
		float _lastRotation;
		int[] _touches;
		bool _started;
		bool _touchBegan;

		public RotationGesture(GObject host)
		{
			this.host = host;
			Enable(true);

			_touches = new int[2];
			snapping = true;

			onBegin = new EventListener(this, "onRotationBegin");
			onEnd = new EventListener(this, "onRotationEnd");
			onAction = new EventListener(this, "onRotationAction");
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
					Stage.inst.onTouchMove.Add(__touchMove);
					Stage.inst.onTouchEnd.Add(__touchEnd);
				}
				else
				{
					host.onTouchBegin.Add(__touchBegin);
					host.onTouchMove.Add(__touchMove);
					host.onTouchEnd.Add(__touchEnd);
				}
			}
			else
			{
				_started = false;
				_touchBegan = false;
				if (host == GRoot.inst)
				{
					Stage.inst.onTouchBegin.Remove(__touchBegin);
					Stage.inst.onTouchMove.Remove(__touchMove);
					Stage.inst.onTouchEnd.Remove(__touchEnd);
				}
				else
				{
					host.onTouchBegin.Remove(__touchBegin);
					host.onTouchMove.Remove(__touchMove);
					host.onTouchEnd.Remove(__touchEnd);
				}
			}
		}

		void __touchBegin(EventContext context)
		{
			if (Stage.inst.touchCount == 2)
			{
				if (!_started && !_touchBegan)
				{
					_touchBegan = true;
					Stage.inst.GetAllTouch(_touches);
					Vector2 pt1 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[0]));
					Vector2 pt2 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[1]));
					_startVector = pt1 - pt2;

					context.CaptureTouch();
				}
			}
		}

		void __touchMove(EventContext context)
		{
			if (!_touchBegan || Stage.inst.touchCount != 2)
				return;

			InputEvent evt = context.inputEvent;
			Vector2 pt1 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[0]));
			Vector2 pt2 = host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[1]));
			Vector2 vec = pt1 - pt2;

			float rot = Mathf.Rad2Deg * ((Mathf.Atan2(vec.y, vec.x) - Mathf.Atan2(_startVector.y, _startVector.x)));
			if (snapping)
			{
				rot = Mathf.Round(rot);
				if (rot == 0)
					return;
			}

			if (!_started && rot > 5)
			{
				_started = true;
				rotation = 0;
				_lastRotation = 0;

				onBegin.Call(evt);
			}

			if (_started)
			{
				delta = rot - _lastRotation;
				_lastRotation = rot;
				this.rotation += delta;
				onAction.Call(evt);
			}
		}

		void __touchEnd(EventContext context)
		{
			_touchBegan = false;
			if (_started)
			{
				_started = false;
				onEnd.Call(context.inputEvent);
			}
		}
	}
}
