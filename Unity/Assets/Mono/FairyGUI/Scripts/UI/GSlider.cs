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
        double _min;
        double _max;
        double _value;
        ProgressTitleType _titleType;
        bool _reverse;
        bool _wholeNumbers;

        GObject _titleObject;
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

        EventListener _onChanged;
        EventListener _onGripTouchEnd;

        public bool changeOnClick;
        public bool canDrag;

        public GSlider()
        {
            _value = 50;
            _max = 100;
            changeOnClick = true;
            canDrag = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onChanged
        {
            get { return _onChanged ?? (_onChanged = new EventListener(this, "onChanged")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onGripTouchEnd
        {
            get { return _onGripTouchEnd ?? (_onGripTouchEnd = new EventListener(this, "onGripTouchEnd")); }
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
        public double min
        {
            get
            {
                return _min;
            }
            set
            {
                if (_min != value)
                {
                    _min = value;
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

        /// <summary>
        /// 
        /// </summary>
        public bool wholeNumbers
        {
            get
            {
                return _wholeNumbers;
            }
            set
            {
                if (_wholeNumbers != value)
                {
                    _wholeNumbers = value;
                    Update();
                }
            }
        }

        private void Update()
        {
            UpdateWithPercent((float)((_value - _min) / (_max - _min)), false);
        }

        private void UpdateWithPercent(float percent, bool manual)
        {
            percent = Mathf.Clamp01(percent);
            if (manual)
            {
                double newValue = _min + (_max - _min) * percent;
                if (newValue < _min)
                    newValue = _min;
                if (newValue > _max)
                    newValue = _max;
                if (_wholeNumbers)
                {
                    newValue = Math.Round(newValue);
                    percent = Mathf.Clamp01((float)((newValue - _min) / (_max - _min)));
                }

                if (newValue != _value)
                {
                    _value = newValue;
                    if (DispatchEvent("onChanged", null))
                        return;
                }
            }

            if (_titleObject != null)
            {
                switch (_titleType)
                {
                    case ProgressTitleType.Percent:
                        _titleObject.text = Mathf.FloorToInt(percent * 100) + "%";
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
                    if (!SetFillAmount(_barObjectH, percent))
                        _barObjectH.width = Mathf.RoundToInt(fullWidth * percent);
                }
                if (_barObjectV != null)
                {
                    if (!SetFillAmount(_barObjectV, percent))
                        _barObjectV.height = Mathf.RoundToInt(fullHeight * percent);
                }
            }
            else
            {
                if (_barObjectH != null)
                {
                    if (!SetFillAmount(_barObjectH, 1 - percent))
                    {
                        _barObjectH.width = Mathf.RoundToInt(fullWidth * percent);
                        _barObjectH.x = _barStartX + (fullWidth - _barObjectH.width);
                    }
                }
                if (_barObjectV != null)
                {
                    if (!SetFillAmount(_barObjectV, 1 - percent))
                    {
                        _barObjectV.height = Mathf.RoundToInt(fullHeight * percent);
                        _barObjectV.y = _barStartY + (fullHeight - _barObjectV.height);
                    }
                }
            }

            InvalidateBatchingState(true);
        }

        bool SetFillAmount(GObject bar, float amount)
        {
            if ((bar is GImage) && ((GImage)bar).fillMethod != FillMethod.None)
                ((GImage)bar).fillAmount = amount;
            else if ((bar is GLoader) && ((GLoader)bar).fillMethod != FillMethod.None)
                ((GLoader)bar).fillAmount = amount;
            else
                return false;

            return true;
        }

        override protected void ConstructExtension(ByteBuffer buffer)
        {
            buffer.Seek(0, 6);

            _titleType = (ProgressTitleType)buffer.ReadByte();
            _reverse = buffer.ReadBool();
            if (buffer.version >= 2)
            {
                _wholeNumbers = buffer.ReadBool();
                this.changeOnClick = buffer.ReadBool();
            }

            _titleObject = GetChild("title");
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
            if (buffer.version >= 2)
                _min = buffer.ReadInt();


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
            _clickPercent = Mathf.Clamp01((float)((_value - _min) / (_max - _min)));
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

            UpdateWithPercent(percent, true);
        }

        private void __gripTouchEnd(EventContext context)
        {
            DispatchEvent("onGripTouchEnd", null);
        }

        private void __barTouchBegin(EventContext context)
        {
            if (!changeOnClick)
                return;

            InputEvent evt = context.inputEvent;
            Vector2 pt = _gripObject.GlobalToLocal(new Vector2(evt.x, evt.y));
            float percent = Mathf.Clamp01((float)((_value - _min) / (_max - _min)));
            float delta = 0;
            if (_barObjectH != null)
                delta = (pt.x - _gripObject.width / 2) / _barMaxWidth;
            if (_barObjectV != null)
                delta = (pt.y - _gripObject.height / 2) / _barMaxHeight;
            if (_reverse)
                percent -= delta;
            else
                percent += delta;

            UpdateWithPercent(percent, true);
        }
    }
}
