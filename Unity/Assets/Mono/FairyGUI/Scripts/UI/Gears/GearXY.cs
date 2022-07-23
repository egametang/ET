using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    class GearXYValue
    {
        public float x;
        public float y;
        public float px;
        public float py;

        public GearXYValue(float x = 0, float y = 0, float px = 0, float py = 0)
        {
            this.x = x;
            this.y = y;
            this.px = px;
            this.py = py;
        }
    }

    /// <summary>
    /// Gear is a connection between object and controller.
    /// </summary>
    public class GearXY : GearBase, ITweenListener
    {
        public bool positionsInPercent;

        Dictionary<string, GearXYValue> _storage;
        GearXYValue _default;

        public GearXY(GObject owner)
            : base(owner)
        {
        }

        protected override void Init()
        {
            _default = new GearXYValue(_owner.x, _owner.y, _owner.x / _owner.parent.width, _owner.y / _owner.parent.height);
            _storage = new Dictionary<string, GearXYValue>();
        }

        override protected void AddStatus(string pageId, ByteBuffer buffer)
        {
            GearXYValue gv;
            if (pageId == null)
                gv = _default;
            else
            {
                gv = new GearXYValue();
                _storage[pageId] = gv;
            }

            gv.x = buffer.ReadInt();
            gv.y = buffer.ReadInt();
        }

        public void AddExtStatus(string pageId, ByteBuffer buffer)
        {
            GearXYValue gv;
            if (pageId == null)
                gv = _default;
            else
                gv = _storage[pageId];
            gv.px = buffer.ReadFloat();
            gv.py = buffer.ReadFloat();
        }

        override public void Apply()
        {
            GearXYValue gv;
            if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
                gv = _default;

            Vector2 endPos = new Vector2();

            if (positionsInPercent && _owner.parent != null)
            {
                endPos.x = gv.px * _owner.parent.width;
                endPos.y = gv.py * _owner.parent.height;
            }
            else
            {
                endPos.x = gv.x;
                endPos.y = gv.y;
            }

            if (_tweenConfig != null && _tweenConfig.tween && UIPackage._constructing == 0 && !disableAllTweenEffect)
            {
                if (_tweenConfig._tweener != null)
                {
                    if (_tweenConfig._tweener.endValue.x != endPos.x || _tweenConfig._tweener.endValue.y != endPos.y)
                    {
                        _tweenConfig._tweener.Kill(true);
                        _tweenConfig._tweener = null;
                    }
                    else
                        return;
                }
                Vector2 origin = _owner.xy;

                if (endPos != origin)
                {
                    if (_owner.CheckGearController(0, _controller))
                        _tweenConfig._displayLockToken = _owner.AddDisplayLock();

                    _tweenConfig._tweener = GTween.To(origin, endPos, _tweenConfig.duration)
                        .SetDelay(_tweenConfig.delay)
                        .SetEase(_tweenConfig.easeType, _tweenConfig.customEase)
                        .SetTarget(this)
                        .SetListener(this);
                }
            }
            else
            {
                _owner._gearLocked = true;
                _owner.SetXY(endPos.x, endPos.y);
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
            _owner.DispatchEvent("onGearStop", this);
        }

        override public void UpdateState()
        {
            GearXYValue gv;
            if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
                _storage[_controller.selectedPageId] = gv = new GearXYValue();

            gv.x = _owner.x;
            gv.y = _owner.y;

            gv.px = _owner.x / _owner.parent.width;
            gv.py = _owner.y / _owner.parent.height;

        }

        override public void UpdateFromRelations(float dx, float dy)
        {
            if (_controller != null && _storage != null && !positionsInPercent)
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
