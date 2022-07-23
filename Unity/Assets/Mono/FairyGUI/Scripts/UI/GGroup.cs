using System;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// GGroup class.
    /// 组对象，对应编辑器里的高级组。
    /// </summary>
    public class GGroup : GObject
    {
        GroupLayoutType _layout;
        int _lineGap;
        int _columnGap;

        bool _excludeInvisibles;
        bool _autoSizeDisabled;
        int _mainGridIndex;
        int _mainGridMinSize;

        bool _percentReady;
        bool _boundsChanged;
        int _mainChildIndex;
        float _totalSize;
        int _numChildren;
        internal int _updating;

        Action _refreshDelegate;

        public GGroup()
        {
            _mainGridIndex = -1;
            _mainChildIndex = -1;
            _mainGridMinSize = 50;
            _refreshDelegate = EnsureBoundsCorrect;
        }

        /// <summary>
        /// Group layout type.
        /// </summary>
        public GroupLayoutType layout
        {
            get { return _layout; }
            set
            {
                if (_layout != value)
                {
                    _layout = value;
                    SetBoundsChangedFlag();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int lineGap
        {
            get { return _lineGap; }
            set
            {
                if (_lineGap != value)
                {
                    _lineGap = value;
                    SetBoundsChangedFlag(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int columnGap
        {
            get { return _columnGap; }
            set
            {
                if (_columnGap != value)
                {
                    _columnGap = value;
                    SetBoundsChangedFlag(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool excludeInvisibles
        {
            get { return _excludeInvisibles; }
            set
            {
                if (_excludeInvisibles != value)
                {
                    _excludeInvisibles = value;
                    SetBoundsChangedFlag();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool autoSizeDisabled
        {
            get { return _autoSizeDisabled; }
            set
            {
                if (_autoSizeDisabled != value)
                {
                    _autoSizeDisabled = value;
                    SetBoundsChangedFlag();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int mainGridMinSize
        {
            get { return _mainGridMinSize; }
            set
            {
                if (_mainGridMinSize != value)
                {
                    _mainGridMinSize = value;
                    SetBoundsChangedFlag();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int mainGridIndex
        {
            get { return _mainGridIndex; }
            set
            {
                if (_mainGridIndex != value)
                {
                    _mainGridIndex = value;
                    SetBoundsChangedFlag();
                }
            }
        }

        /// <summary>
        /// Update group bounds.
        /// 更新组的包围.
        /// </summary>
        public void SetBoundsChangedFlag(bool positionChangedOnly = false)
        {
            if (_updating == 0 && parent != null)
            {
                if (!positionChangedOnly)
                    _percentReady = false;

                if (!_boundsChanged)
                {
                    _boundsChanged = true;

                    if (_layout != GroupLayoutType.None)
                    {
                        UpdateContext.OnBegin -= _refreshDelegate;
                        UpdateContext.OnBegin += _refreshDelegate;
                    }
                }
            }
        }

        public void EnsureBoundsCorrect()
        {
            if (parent == null || !_boundsChanged)
                return;

            UpdateContext.OnBegin -= _refreshDelegate;
            _boundsChanged = false;

            if (_autoSizeDisabled)
                ResizeChildren(0, 0);
            else
            {
                HandleLayout();
                UpdateBounds();
            }
        }

        void UpdateBounds()
        {
            int cnt = parent.numChildren;
            int i;
            GObject child;
            float ax = int.MaxValue, ay = int.MaxValue;
            float ar = int.MinValue, ab = int.MinValue;
            float tmp;
            bool empty = true;
            bool skipInvisibles = _layout != GroupLayoutType.None && _excludeInvisibles;

            for (i = 0; i < cnt; i++)
            {
                child = parent.GetChildAt(i);
                if (child.group != this)
                    continue;

                if (skipInvisibles && !child.internalVisible3)
                    continue;

                tmp = child.xMin;
                if (tmp < ax)
                    ax = tmp;
                tmp = child.yMin;
                if (tmp < ay)
                    ay = tmp;
                tmp = child.xMin + child.width;
                if (tmp > ar)
                    ar = tmp;
                tmp = child.yMin + child.height;
                if (tmp > ab)
                    ab = tmp;

                empty = false;
            }

            float w;
            float h;
            if (!empty)
            {
                _updating |= 1;
                SetXY(ax, ay);
                _updating &= 2;

                w = ar - ax;
                h = ab - ay;
            }
            else
                w = h = 0;

            if ((_updating & 2) == 0)
            {
                _updating |= 2;
                SetSize(w, h);
                _updating &= 1;
            }
            else
            {
                _updating &= 1;
                ResizeChildren(_width - w, _height - h);
            }
        }

        void HandleLayout()
        {
            _updating |= 1;

            if (_layout == GroupLayoutType.Horizontal)
            {
                float curX = this.x;
                int cnt = parent.numChildren;
                for (int i = 0; i < cnt; i++)
                {
                    GObject child = parent.GetChildAt(i);
                    if (child.group != this)
                        continue;
                    if (_excludeInvisibles && !child.internalVisible3)
                        continue;

                    child.xMin = curX;
                    if (child.width != 0)
                        curX += child.width + _columnGap;
                }
            }
            else if (_layout == GroupLayoutType.Vertical)
            {
                float curY = this.y;
                int cnt = parent.numChildren;
                for (int i = 0; i < cnt; i++)
                {
                    GObject child = parent.GetChildAt(i);
                    if (child.group != this)
                        continue;
                    if (_excludeInvisibles && !child.internalVisible3)
                        continue;

                    child.yMin = curY;
                    if (child.height != 0)
                        curY += child.height + _lineGap;
                }
            }

            _updating &= 2;
        }

        internal void MoveChildren(float dx, float dy)
        {
            if ((_updating & 1) != 0 || parent == null)
                return;

            _updating |= 1;

            int cnt = parent.numChildren;
            int i;
            GObject child;
            for (i = 0; i < cnt; i++)
            {
                child = parent.GetChildAt(i);
                if (child.group == this)
                {
                    child.SetXY(child.x + dx, child.y + dy);
                }
            }

            _updating &= 2;
        }

        internal void ResizeChildren(float dw, float dh)
        {
            if (_layout == GroupLayoutType.None || (_updating & 2) != 0 || parent == null)
                return;

            _updating |= 2;

            if (_boundsChanged)
            {
                _boundsChanged = false;
                if (!_autoSizeDisabled)
                {
                    UpdateBounds();
                    return;
                }
            }

            int cnt = parent.numChildren;

            if (!_percentReady)
            {
                _percentReady = true;
                _numChildren = 0;
                _totalSize = 0;
                _mainChildIndex = -1;

                int j = 0;
                for (int i = 0; i < cnt; i++)
                {
                    GObject child = parent.GetChildAt(i);
                    if (child.group != this)
                        continue;

                    if (!_excludeInvisibles || child.internalVisible3)
                    {
                        if (j == _mainGridIndex)
                            _mainChildIndex = i;

                        _numChildren++;

                        if (_layout == GroupLayoutType.Horizontal)
                            _totalSize += child.width;
                        else
                            _totalSize += child.height;
                    }

                    j++;
                }

                if (_mainChildIndex != -1)
                {
                    if (_layout == GroupLayoutType.Horizontal)
                    {
                        GObject child = parent.GetChildAt(_mainChildIndex);
                        _totalSize += _mainGridMinSize - child.width;
                        child._sizePercentInGroup = _mainGridMinSize / _totalSize;
                    }
                    else
                    {
                        GObject child = parent.GetChildAt(_mainChildIndex);
                        _totalSize += _mainGridMinSize - child.height;
                        child._sizePercentInGroup = _mainGridMinSize / _totalSize;
                    }
                }

                for (int i = 0; i < cnt; i++)
                {
                    GObject child = parent.GetChildAt(i);
                    if (child.group != this)
                        continue;

                    if (i == _mainChildIndex)
                        continue;

                    if (_totalSize > 0)
                        child._sizePercentInGroup = (_layout == GroupLayoutType.Horizontal ? child.width : child.height) / _totalSize;
                    else
                        child._sizePercentInGroup = 0;
                }
            }

            float remainSize = 0;
            float remainPercent = 1;
            bool priorHandled = false;

            if (_layout == GroupLayoutType.Horizontal)
            {
                remainSize = this.width - (_numChildren - 1) * _columnGap;
                if (_mainChildIndex != -1 && remainSize >= _totalSize)
                {
                    GObject child = parent.GetChildAt(_mainChildIndex);
                    child.SetSize(remainSize - (_totalSize - _mainGridMinSize), child._rawHeight + dh, true);
                    remainSize -= child.width;
                    remainPercent -= child._sizePercentInGroup;
                    priorHandled = true;
                }

                float curX = this.x;
                for (int i = 0; i < cnt; i++)
                {
                    GObject child = parent.GetChildAt(i);
                    if (child.group != this)
                        continue;

                    if (_excludeInvisibles && !child.internalVisible3)
                    {
                        child.SetSize(child._rawWidth, child._rawHeight + dh, true);
                        continue;
                    }

                    if (!priorHandled || i != _mainChildIndex)
                    {
                        child.SetSize(Mathf.Round(child._sizePercentInGroup / remainPercent * remainSize), child._rawHeight + dh, true);
                        remainPercent -= child._sizePercentInGroup;
                        remainSize -= child.width;
                    }

                    child.xMin = curX;
                    if (child.width != 0)
                        curX += child.width + _columnGap;
                }
            }
            else
            {
                remainSize = this.height - (_numChildren - 1) * _lineGap;
                if (_mainChildIndex != -1 && remainSize >= _totalSize)
                {
                    GObject child = parent.GetChildAt(_mainChildIndex);
                    child.SetSize(child._rawWidth + dw, remainSize - (_totalSize - _mainGridMinSize), true);
                    remainSize -= child.height;
                    remainPercent -= child._sizePercentInGroup;
                    priorHandled = true;
                }

                float curY = this.y;
                for (int i = 0; i < cnt; i++)
                {
                    GObject child = parent.GetChildAt(i);
                    if (child.group != this)
                        continue;

                    if (_excludeInvisibles && !child.internalVisible3)
                    {
                        child.SetSize(child._rawWidth + dw, child._rawHeight, true);
                        continue;
                    }

                    if (!priorHandled || i != _mainChildIndex)
                    {
                        child.SetSize(child._rawWidth + dw, Mathf.Round(child._sizePercentInGroup / remainPercent * remainSize), true);
                        remainPercent -= child._sizePercentInGroup;
                        remainSize -= child.height;
                    }

                    child.yMin = curY;
                    if (child.height != 0)
                        curY += child.height + _lineGap;
                }
            }

            _updating &= 1;
        }

        override protected void HandleAlphaChanged()
        {
            base.HandleAlphaChanged();

            if (this.underConstruct || parent == null)
                return;

            int cnt = parent.numChildren;
            float a = this.alpha;
            for (int i = 0; i < cnt; i++)
            {
                GObject child = parent.GetChildAt(i);
                if (child.group == this)
                    child.alpha = a;
            }
        }

        override internal protected void HandleVisibleChanged()
        {
            if (parent == null)
                return;

            int cnt = parent.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                GObject child = parent.GetChildAt(i);
                if (child.group == this)
                    child.HandleVisibleChanged();
            }
        }

        override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 5);

            _layout = (GroupLayoutType)buffer.ReadByte();
            _lineGap = buffer.ReadInt();
            _columnGap = buffer.ReadInt();
            if (buffer.version >= 2)
            {
                _excludeInvisibles = buffer.ReadBool();
                _autoSizeDisabled = buffer.ReadBool();
                _mainGridIndex = buffer.ReadShort();
            }
        }

        override public void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_AfterAdd(buffer, beginPos);

            if (!this.visible)
                HandleVisibleChanged();
        }
    }
}
