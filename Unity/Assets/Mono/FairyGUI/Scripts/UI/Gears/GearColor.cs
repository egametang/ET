using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    class GearColorValue
    {
        public Color color;
        public Color strokeColor;

        public GearColorValue()
        {
        }

        public GearColorValue(Color color, Color strokeColor)
        {
            this.color = color;
            this.strokeColor = strokeColor;
        }
    }

    /// <summary>
    /// Gear is a connection between object and controller.
    /// </summary>
    public class GearColor : GearBase, ITweenListener
    {
        Dictionary<string, GearColorValue> _storage;
        GearColorValue _default;

        public GearColor(GObject owner)
            : base(owner)
        {
        }

        protected override void Init()
        {
            _default = new GearColorValue();
            _default.color = ((IColorGear)_owner).color;
            if (_owner is ITextColorGear)
                _default.strokeColor = ((ITextColorGear)_owner).strokeColor;
            _storage = new Dictionary<string, GearColorValue>();
        }

        override protected void AddStatus(string pageId, ByteBuffer buffer)
        {
            GearColorValue gv;
            if (pageId == null)
                gv = _default;
            else
            {
                gv = new GearColorValue(Color.black, Color.black);
                _storage[pageId] = gv;
            }

            gv.color = buffer.ReadColor();
            gv.strokeColor = buffer.ReadColor();
        }

        override public void Apply()
        {
            GearColorValue gv;
            if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
                gv = _default;

            if (_tweenConfig != null && _tweenConfig.tween && UIPackage._constructing == 0 && !disableAllTweenEffect)
            {
                if ((_owner is ITextColorGear) && gv.strokeColor.a > 0)
                {
                    _owner._gearLocked = true;
                    ((ITextColorGear)_owner).strokeColor = gv.strokeColor;
                    _owner._gearLocked = false;
                }

                if (_tweenConfig._tweener != null)
                {
                    if (_tweenConfig._tweener.endValue.color != gv.color)
                    {
                        _tweenConfig._tweener.Kill(true);
                        _tweenConfig._tweener = null;
                    }
                    else
                        return;
                }

                if (((IColorGear)_owner).color != gv.color)
                {
                    if (_owner.CheckGearController(0, _controller))
                        _tweenConfig._displayLockToken = _owner.AddDisplayLock();

                    _tweenConfig._tweener = GTween.To(((IColorGear)_owner).color, gv.color, _tweenConfig.duration)
                        .SetDelay(_tweenConfig.delay)
                        .SetEase(_tweenConfig.easeType, _tweenConfig.customEase)
                        .SetTarget(this)
                        .SetListener(this);
                }
            }
            else
            {
                _owner._gearLocked = true;
                ((IColorGear)_owner).color = gv.color;
                if ((_owner is ITextColorGear) && gv.strokeColor.a > 0)
                    ((ITextColorGear)_owner).strokeColor = gv.strokeColor;
                _owner._gearLocked = false;
            }
        }

        public void OnTweenStart(GTweener tweener)
        {
        }

        public void OnTweenUpdate(GTweener tweener)
        {
            _owner._gearLocked = true;
            ((IColorGear)_owner).color = tweener.value.color;
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
            GearColorValue gv;
            if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
                _storage[_controller.selectedPageId] = gv = new GearColorValue();
            gv.color = ((IColorGear)_owner).color;
            if (_owner is ITextColorGear)
                gv.strokeColor = ((ITextColorGear)_owner).strokeColor;
        }
    }
}
