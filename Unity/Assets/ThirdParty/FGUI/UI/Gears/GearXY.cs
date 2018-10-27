using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	class GearXYValue
	{
		public float x;
		public float y;

		public GearXYValue(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearXY : GearBase, ITweenListener
	{
		Dictionary<string, GearXYValue> _storage;
		GearXYValue _default;

		public GearXY(GObject owner)
			: base(owner)
		{
		}

		protected override void Init()
		{
			_default = new GearXYValue(_owner.x, _owner.y);
			_storage = new Dictionary<string, GearXYValue>();
		}

		override protected void AddStatus(string pageId, ByteBuffer buffer)
		{
			GearXYValue gv;
			if (pageId == null)
				gv = _default;
			else
			{
				gv = new GearXYValue(0, 0);
				_storage[pageId] = gv;
			}

			gv.x = buffer.ReadInt();
			gv.y = buffer.ReadInt();
		}

		override public void Apply()
		{
			GearXYValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				gv = _default;

			if (_tweenConfig != null && _tweenConfig.tween && UIPackage._constructing == 0 && !disableAllTweenEffect)
			{
				if (_tweenConfig._tweener != null)
				{
					if (_tweenConfig._tweener.endValue.x != gv.x || _tweenConfig._tweener.endValue.y != gv.y)
					{
						_tweenConfig._tweener.Kill(true);
						_tweenConfig._tweener = null;
					}
					else
						return;
				}

				if (_owner.x != gv.x || _owner.y != gv.y)
				{
					if (_owner.CheckGearController(0, _controller))
						_tweenConfig._displayLockToken = _owner.AddDisplayLock();

					_tweenConfig._tweener = GTween.To(_owner.xy, new Vector2(gv.x, gv.y), _tweenConfig.duration)
						.SetDelay(_tweenConfig.delay)
						.SetEase(_tweenConfig.easeType)
						.SetTarget(this)
						.SetListener(this);
				}
			}
			else
			{
				_owner._gearLocked = true;
				_owner.SetXY(gv.x, gv.y);
				_owner._gearLocked = false;
			}
		}

		public void OnTweenStart(GTweener tweener)
		{//nothing
		}

		public void OnTweenUpdate(GTweener tweener)
		{
			_owner._gearLocked = true;
			_owner.SetXY(tweener.value.x, tweener.value.y);
			_owner._gearLocked = false;

			_owner.InvalidateBatchingState();
		}

		public void OnTweenComplete(GTweener tweener)
		{
			_tweenConfig._tweener = null;
			if (_tweenConfig._displayLockToken != 0)
			{
				_owner.ReleaseDisplayLock(_tweenConfig._displayLockToken);
				_tweenConfig._displayLockToken = 0;
			}
			_owner.OnGearStop.Call(this);
		}

		override public void UpdateState()
		{
			GearXYValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				_storage[_controller.selectedPageId] = new GearXYValue(_owner.x, _owner.y);
			else
			{
				gv.x = _owner.x;
				gv.y = _owner.y;
			}
		}

		override public void UpdateFromRelations(float dx, float dy)
		{
			if (_controller != null && _storage != null)
			{
				foreach (GearXYValue gv in _storage.Values)
				{
					gv.x += dx;
					gv.y += dy;
				}
				_default.x += dx;
				_default.y += dy;

				UpdateState();
			}
		}
	}
}
