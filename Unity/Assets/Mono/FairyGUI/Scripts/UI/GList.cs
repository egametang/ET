using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// Callback function when an item is needed to update its look.
    /// </summary>
    /// <param name="index">Item index.</param>
    /// <param name="item">Item object.</param>
    public delegate void ListItemRenderer(int index, GObject item);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public delegate string ListItemProvider(int index);

    /// <summary>
    /// GList class.
    /// </summary>
    public class GList : GComponent
    {
        /// <summary>
        /// 如果true，当item不可见时自动折叠，否则依然占位
        /// </summary>
        public bool foldInvisibleItems = false;

        /// <summary>
        /// List selection mode
        /// </summary>
        /// <seealso cref="ListSelectionMode"/>
        public ListSelectionMode selectionMode;

        /// <summary>
        /// Callback function when an item is needed to update its look.
        /// </summary>
        public ListItemRenderer itemRenderer;

        /// <summary>
        /// Callback funtion to return item resource url.
        /// </summary>
        public ListItemProvider itemProvider;

        /// <summary>
        /// 
        /// </summary>
        public bool scrollItemToViewOnClick;

        string _defaultItem;
        ListLayoutType _layout;
        int _lineCount;
        int _columnCount;
        int _lineGap;
        int _columnGap;
        AlignType _align;
        VertAlignType _verticalAlign;
        bool _autoResizeItem;
        Controller _selectionController;

        GObjectPool _pool;
        int _lastSelectedIndex;

        EventListener _onClickItem;
        EventListener _onRightClickItem;

        //Virtual List support
        bool _virtual;
        bool _loop;
        int _numItems;
        int _realNumItems;
        int _firstIndex; //the top left index
        int _curLineItemCount; //item count in one line
        int _curLineItemCount2; //只用在页面模式，表示垂直方向的项目数
        Vector2 _itemSize;
        int _virtualListChanged; //1-content changed, 2-size changed
        uint itemInfoVer; //用来标志item是否在本次处理中已经被重用了

        int _miscFlags; //1-event locked, 2-focus events registered

        class ItemInfo
        {
            public Vector2 size;
            public GObject obj;
            public uint updateFlag;
            public bool selected;
        }
        List<ItemInfo> _virtualItems;

        EventCallback1 _itemClickDelegate;

        public GList()
            : base()
        {
            _trackBounds = true;
            this.opaque = true;
            scrollItemToViewOnClick = true;

            container = new Container();
            rootContainer.AddChild(container);
            rootContainer.gameObject.name = "GList";

            _pool = new GObjectPool(container.cachedTransform);

            _itemClickDelegate = __clickItem;
        }

        public override void Dispose()
        {
            _pool.Clear();
            if (_virtualListChanged != 0)
                Timers.inst.Remove(this.RefreshVirtualList);

            _selectionController = null;
            scrollItemToViewOnClick = false;
            itemRenderer = null;
            itemProvider = null;

            base.Dispose();
        }

        /// <summary>
        /// Dispatched when a list item being clicked.
        /// </summary>
        public EventListener onClickItem
        {
            get { return _onClickItem ?? (_onClickItem = new EventListener(this, "onClickItem")); }
        }

        /// <summary>
        /// Dispatched when a list item being clicked with right button.
        /// </summary>
        public EventListener onRightClickItem
        {
            get { return _onRightClickItem ?? (_onRightClickItem = new EventListener(this, "onRightClickItem")); }
        }

        /// <summary>
        /// Resource url of the default item.
        /// </summary>
        public string defaultItem
        {
            get { return _defaultItem; }
            set
            {
                _defaultItem = UIPackage.NormalizeURL(value);
            }
        }

        /// <summary>
        /// List layout type.
        /// </summary>
        public ListLayoutType layout
        {
            get { return _layout; }
            set
            {
                if (_layout != value)
                {
                    _layout = value;
                    SetBoundsChangedFlag();
                    if (_virtual)
                        SetVirtualListChangedFlag(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int lineCount
        {
            get { return _lineCount; }
            set
            {
                if (_lineCount != value)
                {
                    _lineCount = value;
                    if (_layout == ListLayoutType.FlowVertical || _layout == ListLayoutType.Pagination)
                    {
                        SetBoundsChangedFlag();
                        if (_virtual)
                            SetVirtualListChangedFlag(true);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int columnCount
        {
            get { return _columnCount; }
            set
            {
                if (_columnCount != value)
                {
                    _columnCount = value;
                    if (_layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.Pagination)
                    {
                        SetBoundsChangedFlag();
                        if (_virtual)
                            SetVirtualListChangedFlag(true);
                    }
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
                    SetBoundsChangedFlag();
                    if (_virtual)
                        SetVirtualListChangedFlag(true);
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
                    SetBoundsChangedFlag();
                    if (_virtual)
                        SetVirtualListChangedFlag(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public AlignType align
        {
            get { return _align; }
            set
            {
                if (_align != value)
                {
                    _align = value;
                    SetBoundsChangedFlag();
                    if (_virtual)
                        SetVirtualListChangedFlag(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public VertAlignType verticalAlign
        {
            get { return _verticalAlign; }
            set
            {
                if (_verticalAlign != value)
                {
                    _verticalAlign = value;
                    SetBoundsChangedFlag();
                    if (_virtual)
                        SetVirtualListChangedFlag(true);
                }
            }
        }
        /// <summary>
        /// If the item will resize itself to fit the list width/height.
        /// </summary>
        public bool autoResizeItem
        {
            get { return _autoResizeItem; }
            set
            {
                if (_autoResizeItem != value)
                {
                    _autoResizeItem = value;
                    SetBoundsChangedFlag();
                    if (_virtual)
                        SetVirtualListChangedFlag(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Vector2 defaultItemSize
        {
            get { return _itemSize; }
            set
            {
                _itemSize = value;
                if (_virtual)
                {
                    if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                        this.scrollPane.scrollStep = _itemSize.y;
                    else
                        this.scrollPane.scrollStep = _itemSize.x;
                    SetVirtualListChangedFlag(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public GObjectPool itemPool
        {
            get { return _pool; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public GObject GetFromPool(string url)
        {
            if (string.IsNullOrEmpty(url))
                url = _defaultItem;

            GObject ret = _pool.GetObject(url);
            if (ret != null)
                ret.visible = true;
            return ret;
        }

        void ReturnToPool(GObject obj)
        {
            _pool.ReturnObject(obj);
        }

        /// <summary>
        /// Add a item to list, same as GetFromPool+AddChild
        /// </summary>
        /// <returns>Item object</returns>
        public GObject AddItemFromPool()
        {
            GObject obj = GetFromPool(null);

            return AddChild(obj);
        }

        /// <summary>
        /// Add a item to list, same as GetFromPool+AddChild
        /// </summary>
        /// <param name="url">Item resource url</param>
        /// <returns>Item object</returns>
        public GObject AddItemFromPool(string url)
        {
            GObject obj = GetFromPool(url);

            return AddChild(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        override public GObject AddChildAt(GObject child, int index)
        {
            base.AddChildAt(child, index);
            if (child is GButton)
            {
                GButton button = (GButton)child;
                button.selected = false;
                button.changeStateOnClick = false;
            }

            child.onClick.Add(_itemClickDelegate);
            child.onRightClick.Add(_itemClickDelegate);

            return child;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dispose"></param>
        /// <returns></returns>
        override public GObject RemoveChildAt(int index, bool dispose)
        {
            GObject child = base.RemoveChildAt(index, dispose);
            child.onClick.Remove(_itemClickDelegate);
            child.onRightClick.Remove(_itemClickDelegate);

            return child;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveChildToPoolAt(int index)
        {
            GObject child = base.RemoveChildAt(index);
            ReturnToPool(child);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChildToPool(GObject child)
        {
            base.RemoveChild(child);
            ReturnToPool(child);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveChildrenToPool()
        {
            RemoveChildrenToPool(0, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        public void RemoveChildrenToPool(int beginIndex, int endIndex)
        {
            if (endIndex < 0 || endIndex >= _children.Count)
                endIndex = _children.Count - 1;

            for (int i = beginIndex; i <= endIndex; ++i)
                RemoveChildToPoolAt(beginIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        public int selectedIndex
        {
            get
            {
                if (_virtual)
                {
                    int cnt = _realNumItems;
                    for (int i = 0; i < cnt; i++)
                    {
                        ItemInfo ii = _virtualItems[i];
                        if ((ii.obj is GButton) && ((GButton)ii.obj).selected
                            || ii.obj == null && ii.selected)
                        {
                            if (_loop)
                                return i % _numItems;
                            else
                                return i;
                        }
                    }
                }
                else
                {
                    int cnt = _children.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        GButton obj = _children[i].asButton;
                        if (obj != null && obj.selected)
                            return i;
                    }
                }
                return -1;
            }

            set
            {
                if (value >= 0 && value < this.numItems)
                {
                    if (selectionMode != ListSelectionMode.Single)
                        ClearSelection();
                    AddSelection(value, false);
                }
                else
                    ClearSelection();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Controller selectionController
        {
            get { return _selectionController; }
            set { _selectionController = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<int> GetSelection()
        {
            return GetSelection(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<int> GetSelection(List<int> result)
        {
            if (result == null)
                result = new List<int>();
            if (_virtual)
            {
                int cnt = _realNumItems;
                for (int i = 0; i < cnt; i++)
                {
                    ItemInfo ii = _virtualItems[i];
                    if ((ii.obj is GButton) && ((GButton)ii.obj).selected
                        || ii.obj == null && ii.selected)
                    {
                        int j = i;
                        if (_loop)
                        {
                            j = i % _numItems;
                            if (result.Contains(j))
                                continue;
                        }
                        result.Add(j);
                    }
                }
            }
            else
            {
                int cnt = _children.Count;
                for (int i = 0; i < cnt; i++)
                {
                    GButton obj = _children[i].asButton;
                    if (obj != null && obj.selected)
                        result.Add(i);
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="scrollItToView"></param>
        public void AddSelection(int index, bool scrollItToView)
        {
            if (selectionMode == ListSelectionMode.None)
                return;

            CheckVirtualList();

            if (selectionMode == ListSelectionMode.Single)
                ClearSelection();

            if (scrollItToView)
                ScrollToView(index);

            _lastSelectedIndex = index;
            GButton obj = null;
            if (_virtual)
            {
                ItemInfo ii = _virtualItems[index];
                if (ii.obj != null)
                    obj = ii.obj.asButton;
                ii.selected = true;
            }
            else
                obj = GetChildAt(index).asButton;

            if (obj != null && !obj.selected)
            {
                obj.selected = true;
                UpdateSelectionController(index);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveSelection(int index)
        {
            if (selectionMode == ListSelectionMode.None)
                return;

            GButton obj = null;
            if (_virtual)
            {
                ItemInfo ii = _virtualItems[index];
                if (ii.obj != null)
                    obj = ii.obj.asButton;
                ii.selected = false;
            }
            else
                obj = GetChildAt(index).asButton;

            if (obj != null)
                obj.selected = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearSelection()
        {
            if (_virtual)
            {
                int cnt = _realNumItems;
                for (int i = 0; i < cnt; i++)
                {
                    ItemInfo ii = _virtualItems[i];
                    if ((ii.obj is GButton))
                        ((GButton)ii.obj).selected = false;
                    ii.selected = false;
                }
            }
            else
            {
                int cnt = _children.Count;
                for (int i = 0; i < cnt; i++)
                {
                    GButton obj = _children[i].asButton;
                    if (obj != null)
                        obj.selected = false;
                }
            }
        }

        void ClearSelectionExcept(GObject g)
        {
            if (_virtual)
            {
                int cnt = _realNumItems;
                for (int i = 0; i < cnt; i++)
                {
                    ItemInfo ii = _virtualItems[i];
                    if (ii.obj != g)
                    {
                        if ((ii.obj is GButton))
                            ((GButton)ii.obj).selected = false;
                        ii.selected = false;
                    }
                }
            }
            else
            {
                int cnt = _children.Count;
                for (int i = 0; i < cnt; i++)
                {
                    GButton obj = _children[i].asButton;
                    if (obj != null && obj != g)
                        obj.selected = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SelectAll()
        {
            CheckVirtualList();

            int last = -1;
            if (_virtual)
            {
                int cnt = _realNumItems;
                for (int i = 0; i < cnt; i++)
                {
                    ItemInfo ii = _virtualItems[i];
                    if ((ii.obj is GButton) && !((GButton)ii.obj).selected)
                    {
                        ((GButton)ii.obj).selected = true;
                        last = i;
                    }
                    ii.selected = true;
                }
            }
            else
            {
                int cnt = _children.Count;
                for (int i = 0; i < cnt; i++)
                {
                    GButton obj = _children[i].asButton;
                    if (obj != null && !obj.selected)
                    {
                        obj.selected = true;
                        last = i;
                    }
                }
            }

            if (last != -1)
                UpdateSelectionController(last);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SelectNone()
        {
            ClearSelection();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SelectReverse()
        {
            CheckVirtualList();

            int last = -1;
            if (_virtual)
            {
                int cnt = _realNumItems;
                for (int i = 0; i < cnt; i++)
                {
                    ItemInfo ii = _virtualItems[i];
                    if ((ii.obj is GButton))
                    {
                        ((GButton)ii.obj).selected = !((GButton)ii.obj).selected;
                        if (((GButton)ii.obj).selected)
                            last = i;
                    }
                    ii.selected = !ii.selected;
                }
            }
            else
            {
                int cnt = _children.Count;
                for (int i = 0; i < cnt; i++)
                {
                    GButton obj = _children[i].asButton;
                    if (obj != null)
                    {
                        obj.selected = !obj.selected;
                        if (obj.selected)
                            last = i;
                    }
                }
            }

            if (last != -1)
                UpdateSelectionController(last);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        public void EnableSelectionFocusEvents(bool enabled)
        {
            if (((_miscFlags & 2) != 0) == enabled)
                return;

            if (enabled)
            {
                _miscFlags |= 2;
                this.tabStopChildren = true;
                onFocusIn.Add(NotifySelection);
                onFocusOut.Add(NotifySelection);
            }
            else
            {
                _miscFlags &= 0xFD;
                onFocusIn.Remove(NotifySelection);
                onFocusOut.Remove(NotifySelection);
            }
        }

        void NotifySelection(EventContext context)
        {
            string eventType = context.type == "onFocusIn" ? "onListFocusIn" : "onListFocusOut";
            int cnt = _children.Count;
            for (int i = 0; i < cnt; i++)
            {
                GButton obj = _children[i].asButton;
                if (obj != null && obj.selected)
                    obj.DispatchEvent(eventType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void EnableArrowKeyNavigation(bool enabled)
        {
            if (enabled)
            {
                this.tabStopChildren = true;
                onKeyDown.Add(__keydown);
            }
            else
            {
                this.tabStopChildren = false;
                onKeyDown.Remove(__keydown);
            }
        }

        void __keydown(EventContext context)
        {
            int index = -1;
            switch (context.inputEvent.keyCode)
            {
                case KeyCode.LeftArrow:
                    index = HandleArrowKey(7);
                    break;

                case KeyCode.RightArrow:
                    index = HandleArrowKey(3);
                    break;

                case KeyCode.UpArrow:
                    index = HandleArrowKey(1);
                    break;

                case KeyCode.DownArrow:
                    index = HandleArrowKey(5);
                    break;
            }

            if (index != -1)
            {
                index = ItemIndexToChildIndex(index);
                if (index != -1)
                    DispatchItemEvent(GetChildAt(index), context);

                context.StopPropagation();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        public int HandleArrowKey(int dir)
        {
            int curIndex = this.selectedIndex;
            if (curIndex == -1)
                return -1;

            int index = curIndex;
            switch (dir)
            {
                case 1://up
                    if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowVertical)
                    {
                        index--;
                    }
                    else if (_layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.Pagination)
                    {
                        if (_virtual)
                        {
                            index -= _curLineItemCount;
                        }
                        else
                        {
                            GObject current = _children[index];
                            int k = 0;
                            int i;
                            for (i = index - 1; i >= 0; i--)
                            {
                                GObject obj = _children[i];
                                if (obj.y != current.y)
                                {
                                    current = obj;
                                    break;
                                }
                                k++;
                            }
                            for (; i >= 0; i--)
                            {
                                GObject obj = _children[i];
                                if (obj.y != current.y)
                                {
                                    index = i + k + 1;
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case 3://right
                    if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.Pagination)
                    {
                        index++;
                    }
                    else if (_layout == ListLayoutType.FlowVertical)
                    {
                        if (_virtual)
                        {
                            index += _curLineItemCount;
                        }
                        else
                        {
                            GObject current = _children[index];
                            int k = 0;
                            int cnt = _children.Count;
                            int i;
                            for (i = index + 1; i < cnt; i++)
                            {
                                GObject obj = _children[i];
                                if (obj.x != current.x)
                                {
                                    current = obj;
                                    break;
                                }
                                k++;
                            }
                            for (; i < cnt; i++)
                            {
                                GObject obj = _children[i];
                                if (obj.x != current.x)
                                {
                                    index = i - k - 1;
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case 5://down
                    if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowVertical)
                    {
                        index++;
                    }
                    else if (_layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.Pagination)
                    {
                        if (_virtual)
                        {
                            index += _curLineItemCount;
                        }
                        else
                        {
                            GObject current = _children[index];
                            int k = 0;
                            int cnt = _children.Count;
                            int i;
                            for (i = index + 1; i < cnt; i++)
                            {
                                GObject obj = _children[i];
                                if (obj.y != current.y)
                                {
                                    current = obj;
                                    break;
                                }
                                k++;
                            }
                            for (; i < cnt; i++)
                            {
                                GObject obj = _children[i];
                                if (obj.y != current.y)
                                {
                                    index = i - k - 1;
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case 7://left
                    if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.Pagination)
                    {
                        index--;
                    }
                    else if (_layout == ListLayoutType.FlowVertical)
                    {
                        if (_virtual)
                        {
                            index -= _curLineItemCount;
                        }
                        else
                        {
                            GObject current = _children[index];
                            int k = 0;
                            int i;
                            for (i = index - 1; i >= 0; i--)
                            {
                                GObject obj = _children[i];
                                if (obj.x != current.x)
                                {
                                    current = obj;
                                    break;
                                }
                                k++;
                            }
                            for (; i >= 0; i--)
                            {
                                GObject obj = _children[i];
                                if (obj.x != current.x)
                                {
                                    index = i + k + 1;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            if (index != curIndex && index >= 0 && index < this.numItems)
            {
                ClearSelection();
                AddSelection(index, true);
                return index;
            }
            else
                return -1;
        }

        void __clickItem(EventContext context)
        {
            GObject item = context.sender as GObject;
            if ((item is GButton) && selectionMode != ListSelectionMode.None)
                SetSelectionOnEvent(item, context.inputEvent);

            if (scrollPane != null && scrollItemToViewOnClick)
                scrollPane.ScrollToView(item, true);

            DispatchItemEvent(item, context);
        }

        virtual protected void DispatchItemEvent(GObject item, EventContext context)
        {
            if (context.type == item.onRightClick.type)
                DispatchEvent("onRightClickItem", item);
            else
                DispatchEvent("onClickItem", item);
        }

        void SetSelectionOnEvent(GObject item, InputEvent evt)
        {
            bool dontChangeLastIndex = false;
            GButton button = (GButton)item;
            int index = ChildIndexToItemIndex(GetChildIndex(item));

            if (selectionMode == ListSelectionMode.Single)
            {
                if (!button.selected)
                {
                    ClearSelectionExcept(button);
                    button.selected = true;
                }
            }
            else
            {
                if (evt.shift)
                {
                    if (!button.selected)
                    {
                        if (_lastSelectedIndex != -1)
                        {
                            int min = Math.Min(_lastSelectedIndex, index);
                            int max = Math.Max(_lastSelectedIndex, index);
                            max = Math.Min(max, this.numItems - 1);
                            if (_virtual)
                            {
                                for (int i = min; i <= max; i++)
                                {
                                    ItemInfo ii = _virtualItems[i];
                                    if (ii.obj is GButton)
                                        ((GButton)ii.obj).selected = true;
                                    ii.selected = true;
                                }
                            }
                            else
                            {
                                for (int i = min; i <= max; i++)
                                {
                                    GButton obj = GetChildAt(i).asButton;
                                    if (obj != null && !obj.selected)
                                        obj.selected = true;
                                }
                            }

                            dontChangeLastIndex = true;
                        }
                        else
                        {
                            button.selected = true;
                        }
                    }
                }
                else if (evt.ctrlOrCmd || selectionMode == ListSelectionMode.Multiple_SingleClick)
                {
                    button.selected = !button.selected;
                }
                else
                {
                    if (!button.selected)
                    {
                        ClearSelectionExcept(button);
                        button.selected = true;
                    }
                    else if (evt.button == 0)
                        ClearSelectionExcept(button);
                }
            }

            if (!dontChangeLastIndex)
                _lastSelectedIndex = index;

            if (button.selected)
                UpdateSelectionController(index);
        }

        /// <summary>
        /// Resize to list size to fit specified item count. 
        /// If list layout is single column or flow horizontally, the height will change to fit. 
        /// If list layout is single row or flow vertically, the width will change to fit.
        /// </summary>
        public void ResizeToFit()
        {
            ResizeToFit(int.MaxValue, 0);
        }

        /// <summary>
        /// Resize to list size to fit specified item count. 
        /// If list layout is single column or flow horizontally, the height will change to fit. 
        /// If list layout is single row or flow vertically, the width will change to fit.
        /// </summary>
        /// <param name="itemCount">Item count</param>
        public void ResizeToFit(int itemCount)
        {
            ResizeToFit(itemCount, 0);
        }

        /// <summary>
        /// Resize to list size to fit specified item count. 
        /// If list layout is single column or flow horizontally, the height will change to fit. 
        /// If list layout is single row or flow vertically, the width will change to fit.
        /// </summary>
        /// <param name="itemCount">>Item count</param>
        /// <param name="minSize">If the result size if smaller than minSize, then use minSize.</param>
        public void ResizeToFit(int itemCount, int minSize)
        {
            EnsureBoundsCorrect();

            int curCount = this.numItems;
            if (itemCount > curCount)
                itemCount = curCount;

            if (_virtual)
            {
                int lineCount = Mathf.CeilToInt((float)itemCount / _curLineItemCount);
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                    this.viewHeight = lineCount * _itemSize.y + Math.Max(0, lineCount - 1) * _lineGap;
                else
                    this.viewWidth = lineCount * _itemSize.x + Math.Max(0, lineCount - 1) * _columnGap;
            }
            else if (itemCount == 0)
            {
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                    this.viewHeight = minSize;
                else
                    this.viewWidth = minSize;
            }
            else
            {
                int i = itemCount - 1;
                GObject obj = null;
                while (i >= 0)
                {
                    obj = this.GetChildAt(i);
                    if (!foldInvisibleItems || obj.visible)
                        break;
                    i--;
                }
                if (i < 0)
                {
                    if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                        this.viewHeight = minSize;
                    else
                        this.viewWidth = minSize;
                }
                else
                {
                    float size;
                    if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                    {
                        size = obj.y + obj.height;
                        if (size < minSize)
                            size = minSize;
                        this.viewHeight = size;
                    }
                    else
                    {
                        size = obj.x + obj.width;
                        if (size < minSize)
                            size = minSize;
                        this.viewWidth = size;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        override protected void HandleSizeChanged()
        {
            base.HandleSizeChanged();

            SetBoundsChangedFlag();
            if (_virtual)
                SetVirtualListChangedFlag(true);
        }

        override public void HandleControllerChanged(Controller c)
        {
            base.HandleControllerChanged(c);

            if (_selectionController == c)
                this.selectedIndex = c.selectedIndex;
        }

        void UpdateSelectionController(int index)
        {
            if (_selectionController != null && !_selectionController.changing
                && index < _selectionController.pageCount)
            {
                Controller c = _selectionController;
                _selectionController = null;
                c.selectedIndex = index;
                _selectionController = c;
            }
        }

        /// <summary>
        /// Scroll the list to make an item with certain index visible.
        /// </summary>
        /// <param name="index">Item index</param>
        public void ScrollToView(int index)
        {
            ScrollToView(index, false);
        }

        /// <summary>
        ///  Scroll the list to make an item with certain index visible.
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="ani">True to scroll smoothly, othewise immdediately.</param>
        public void ScrollToView(int index, bool ani)
        {
            ScrollToView(index, ani, false);
        }

        /// <summary>
        ///  Scroll the list to make an item with certain index visible.
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="ani">True to scroll smoothly, othewise immdediately.</param>
        /// <param name="setFirst">If true, scroll to make the target on the top/left; If false, scroll to make the target any position in view.</param>
        public void ScrollToView(int index, bool ani, bool setFirst)
        {
            if (_virtual)
            {
                if (_numItems == 0)
                    return;

                CheckVirtualList();

                if (index >= _virtualItems.Count)
                    throw new Exception("Invalid child index: " + index + ">" + _virtualItems.Count);

                if (_loop)
                    index = Mathf.FloorToInt((float)_firstIndex / _numItems) * _numItems + index;

                Rect rect;
                ItemInfo ii = _virtualItems[index];
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                {
                    float pos = 0;
                    for (int i = _curLineItemCount - 1; i < index; i += _curLineItemCount)
                        pos += _virtualItems[i].size.y + _lineGap;
                    rect = new Rect(0, pos, _itemSize.x, ii.size.y);
                }
                else if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowVertical)
                {
                    float pos = 0;
                    for (int i = _curLineItemCount - 1; i < index; i += _curLineItemCount)
                        pos += _virtualItems[i].size.x + _columnGap;
                    rect = new Rect(pos, 0, ii.size.x, _itemSize.y);
                }
                else
                {
                    int page = index / (_curLineItemCount * _curLineItemCount2);
                    rect = new Rect(page * viewWidth + (index % _curLineItemCount) * (ii.size.x + _columnGap),
                        (index / _curLineItemCount) % _curLineItemCount2 * (ii.size.y + _lineGap),
                        ii.size.x, ii.size.y);
                }

                if (this.scrollPane != null)
                    scrollPane.ScrollToView(rect, ani, setFirst);
                else if (parent != null && parent.scrollPane != null)
                    parent.scrollPane.ScrollToView(this.TransformRect(rect, parent), ani, setFirst);
            }
            else
            {
                GObject obj = GetChildAt(index);
                if (this.scrollPane != null)
                    scrollPane.ScrollToView(obj, ani, setFirst);
                else if (parent != null && parent.scrollPane != null)
                    parent.scrollPane.ScrollToView(obj, ani, setFirst);
            }
        }

        /// <summary>
        /// 获取当前点击哪个item
        /// </summary>
        public GObject touchItem
        {
            get
            {
                //find out which item is under finger
                //逐层往上知道查到点击了那个item
                GObject obj = GRoot.inst.touchTarget;
                GObject p = obj.parent;
                while (p != null)
                {
                    if (p == this)
                        return obj;

                    obj = p;
                    p = p.parent;
                }

                return null;
            }
        }

        /// <summary>
        /// Get first child in view.
        /// </summary>
        /// <returns></returns>
        public override int GetFirstChildInView()
        {
            return ChildIndexToItemIndex(base.GetFirstChildInView());
        }

        public int ChildIndexToItemIndex(int index)
        {
            if (!_virtual)
                return index;

            if (_layout == ListLayoutType.Pagination)
            {
                for (int i = _firstIndex; i < _realNumItems; i++)
                {
                    if (_virtualItems[i].obj != null)
                    {
                        index--;
                        if (index < 0)
                            return i;
                    }
                }

                return index;
            }
            else
            {
                index += _firstIndex;
                if (_loop && _numItems > 0)
                    index = index % _numItems;

                return index;
            }
        }

        public int ItemIndexToChildIndex(int index)
        {
            if (!_virtual)
                return index;

            if (_layout == ListLayoutType.Pagination)
            {
                return GetChildIndex(_virtualItems[index].obj);
            }
            else
            {
                if (_loop && _numItems > 0)
                {
                    int j = _firstIndex % _numItems;
                    if (index >= j)
                        index = index - j;
                    else
                        index = _numItems - j + index;
                }
                else
                    index -= _firstIndex;

                return index;
            }
        }


        /// <summary>
        /// Set the list to be virtual list.
        /// 设置列表为虚拟列表模式。在虚拟列表模式下，列表不会为每一条列表数据创建一个实体对象，而是根据视口大小创建最小量的显示对象，然后通过itemRenderer指定的回调函数设置列表数据。
        /// 在虚拟模式下，你不能通过AddChild、RemoveChild等方式管理列表，只能通过设置numItems设置列表数据的长度。
        /// 如果要刷新列表，可以通过重新设置numItems，或者调用RefreshVirtualList完成。
        /// ‘单行’或者‘单列’的列表布局可支持不等高的列表项目。
        /// 除了‘页面’的列表布局，其他布局均支持使用不同资源构建列表项目，你可以在itemProvider里返回。如果不提供，默认使用defaultItem。
        /// </summary>
        public void SetVirtual()
        {
            SetVirtual(false);
        }

        public bool isVirtual
        {
            get { return _virtual; }
        }

        /// <summary>
        /// Set the list to be virtual list, and has loop behavior.
        /// </summary>
        public void SetVirtualAndLoop()
        {
            SetVirtual(true);
        }

        void SetVirtual(bool loop)
        {
            if (!_virtual)
            {
                if (this.scrollPane == null)
                    Debug.LogError("FairyGUI: Virtual list must be scrollable!");

                if (loop)
                {
                    if (_layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.FlowVertical)
                        Debug.LogError("FairyGUI: Loop list is not supported for FlowHorizontal or FlowVertical layout!");

                    this.scrollPane.bouncebackEffect = false;
                }

                _virtual = true;
                _loop = loop;
                _virtualItems = new List<ItemInfo>();
                RemoveChildrenToPool();

                if (_itemSize.x == 0 || _itemSize.y == 0)
                {
                    GObject obj = GetFromPool(null);
                    if (obj == null)
                    {
                        Debug.LogError("FairyGUI: Virtual List must have a default list item resource.");
                        _itemSize = new Vector2(100, 100);
                    }
                    else
                    {
                        _itemSize = obj.size;
                        _itemSize.x = Mathf.CeilToInt(_itemSize.x);
                        _itemSize.y = Mathf.CeilToInt(_itemSize.y);
                        ReturnToPool(obj);
                    }
                }

                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                {
                    this.scrollPane.scrollStep = _itemSize.y;
                    if (_loop)
                        this.scrollPane._loop = 2;
                }
                else
                {
                    this.scrollPane.scrollStep = _itemSize.x;
                    if (_loop)
                        this.scrollPane._loop = 1;
                }

                this.scrollPane.onScroll.AddCapture(__scrolled);
                SetVirtualListChangedFlag(true);
            }
        }

        /// <summary>
        /// Set the list item count. 
        /// If the list is not virtual, specified number of items will be created. 
        /// If the list is virtual, only items in view will be created.
        /// </summary>
        public int numItems
        {
            get
            {
                if (_virtual)
                    return _numItems;
                else
                    return _children.Count;
            }
            set
            {
                if (_virtual)
                {
                    if (itemRenderer == null)
                        throw new Exception("FairyGUI: Set itemRenderer first!");

                    _numItems = value;
                    if (_loop)
                        _realNumItems = _numItems * 6;//设置6倍数量，用于循环滚动
                    else
                        _realNumItems = _numItems;

                    //_virtualItems的设计是只增不减的
                    int oldCount = _virtualItems.Count;
                    if (_realNumItems > oldCount)
                    {
                        for (int i = oldCount; i < _realNumItems; i++)
                        {
                            ItemInfo ii = new ItemInfo();
                            ii.size = _itemSize;

                            _virtualItems.Add(ii);
                        }
                    }
                    else
                    {
                        for (int i = _realNumItems; i < oldCount; i++)
                            _virtualItems[i].selected = false;
                    }

                    if (_virtualListChanged != 0)
                        Timers.inst.Remove(this.RefreshVirtualList);
                    //立即刷新
                    this.RefreshVirtualList(null);
                }
                else
                {
                    int cnt = _children.Count;
                    if (value > cnt)
                    {
                        for (int i = cnt; i < value; i++)
                        {
                            if (itemProvider == null)
                                AddItemFromPool();
                            else
                                AddItemFromPool(itemProvider(i));
                        }
                    }
                    else
                    {
                        RemoveChildrenToPool(value, cnt);
                    }

                    if (itemRenderer != null)
                    {
                        for (int i = 0; i < value; i++)
                            itemRenderer(i, GetChildAt(i));
                    }
                }
            }
        }

        public void RefreshVirtualList()
        {
            if (!_virtual)
                throw new Exception("FairyGUI: not virtual list");

            SetVirtualListChangedFlag(false);
        }

        void CheckVirtualList()
        {
            if (_virtualListChanged != 0)
            {
                this.RefreshVirtualList(null);
                Timers.inst.Remove(this.RefreshVirtualList);
            }
        }

        void SetVirtualListChangedFlag(bool layoutChanged)
        {
            if (layoutChanged)
                _virtualListChanged = 2;
            else if (_virtualListChanged == 0)
                _virtualListChanged = 1;

            Timers.inst.CallLater(RefreshVirtualList);
        }

        void RefreshVirtualList(object param)
        {
            bool layoutChanged = _virtualListChanged == 2;
            _virtualListChanged = 0;
            _miscFlags |= 1;

            if (layoutChanged)
            {
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.SingleRow)
                    _curLineItemCount = 1;
                else if (_layout == ListLayoutType.FlowHorizontal)
                {
                    if (_columnCount > 0)
                        _curLineItemCount = _columnCount;
                    else
                    {
                        _curLineItemCount = Mathf.FloorToInt((this.scrollPane.viewWidth + _columnGap) / (_itemSize.x + _columnGap));
                        if (_curLineItemCount <= 0)
                            _curLineItemCount = 1;
                    }
                }
                else if (_layout == ListLayoutType.FlowVertical)
                {
                    if (_lineCount > 0)
                        _curLineItemCount = _lineCount;
                    else
                    {
                        _curLineItemCount = Mathf.FloorToInt((this.scrollPane.viewHeight + _lineGap) / (_itemSize.y + _lineGap));
                        if (_curLineItemCount <= 0)
                            _curLineItemCount = 1;
                    }
                }
                else //pagination
                {
                    if (_columnCount > 0)
                        _curLineItemCount = _columnCount;
                    else
                    {
                        _curLineItemCount = Mathf.FloorToInt((this.scrollPane.viewWidth + _columnGap) / (_itemSize.x + _columnGap));
                        if (_curLineItemCount <= 0)
                            _curLineItemCount = 1;
                    }

                    if (_lineCount > 0)
                        _curLineItemCount2 = _lineCount;
                    else
                    {
                        _curLineItemCount2 = Mathf.FloorToInt((this.scrollPane.viewHeight + _lineGap) / (_itemSize.y + _lineGap));
                        if (_curLineItemCount2 <= 0)
                            _curLineItemCount2 = 1;
                    }
                }
            }

            float ch = 0, cw = 0;
            if (_realNumItems > 0)
            {
                int len = Mathf.CeilToInt((float)_realNumItems / _curLineItemCount) * _curLineItemCount;
                int len2 = Math.Min(_curLineItemCount, _realNumItems);
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                {
                    for (int i = 0; i < len; i += _curLineItemCount)
                        ch += _virtualItems[i].size.y + _lineGap;
                    if (ch > 0)
                        ch -= _lineGap;

                    if (_autoResizeItem)
                        cw = scrollPane.viewWidth;
                    else
                    {
                        for (int i = 0; i < len2; i++)
                            cw += _virtualItems[i].size.x + _columnGap;
                        if (cw > 0)
                            cw -= _columnGap;
                    }
                }
                else if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowVertical)
                {
                    for (int i = 0; i < len; i += _curLineItemCount)
                        cw += _virtualItems[i].size.x + _columnGap;
                    if (cw > 0)
                        cw -= _columnGap;

                    if (_autoResizeItem)
                        ch = this.scrollPane.viewHeight;
                    else
                    {
                        for (int i = 0; i < len2; i++)
                            ch += _virtualItems[i].size.y + _lineGap;
                        if (ch > 0)
                            ch -= _lineGap;
                    }
                }
                else
                {
                    int pageCount = Mathf.CeilToInt((float)len / (_curLineItemCount * _curLineItemCount2));
                    cw = pageCount * viewWidth;
                    ch = viewHeight;
                }
            }

            HandleAlign(cw, ch);
            this.scrollPane.SetContentSize(cw, ch);

            _miscFlags &= 0xFE;

            HandleScroll(true);
        }

        void __scrolled(EventContext context)
        {
            HandleScroll(false);
        }

        int GetIndexOnPos1(ref float pos, bool forceUpdate)
        {
            if (_realNumItems < _curLineItemCount)
            {
                pos = 0;
                return 0;
            }

            if (numChildren > 0 && !forceUpdate)
            {
                float pos2 = this.GetChildAt(0).y;
                if (pos2 + (_lineGap > 0 ? 0 : -_lineGap) > pos)
                {
                    for (int i = _firstIndex - _curLineItemCount; i >= 0; i -= _curLineItemCount)
                    {
                        pos2 -= (_virtualItems[i].size.y + _lineGap);
                        if (pos2 <= pos)
                        {
                            pos = pos2;
                            return i;
                        }
                    }

                    pos = 0;
                    return 0;
                }
                else
                {
                    float testGap = _lineGap > 0 ? _lineGap : 0;
                    for (int i = _firstIndex; i < _realNumItems; i += _curLineItemCount)
                    {
                        float pos3 = pos2 + _virtualItems[i].size.y;
                        if (pos3 + testGap > pos)
                        {
                            pos = pos2;
                            return i;
                        }
                        pos2 = pos3 + _lineGap;
                    }

                    pos = pos2;
                    return _realNumItems - _curLineItemCount;
                }
            }
            else
            {
                float pos2 = 0;
                float testGap = _lineGap > 0 ? _lineGap : 0;
                for (int i = 0; i < _realNumItems; i += _curLineItemCount)
                {
                    float pos3 = pos2 + _virtualItems[i].size.y;
                    if (pos3 + testGap > pos)
                    {
                        pos = pos2;
                        return i;
                    }
                    pos2 = pos3 + _lineGap;
                }

                pos = pos2;
                return _realNumItems - _curLineItemCount;
            }
        }

        int GetIndexOnPos2(ref float pos, bool forceUpdate)
        {
            if (_realNumItems < _curLineItemCount)
            {
                pos = 0;
                return 0;
            }

            if (numChildren > 0 && !forceUpdate)
            {
                float pos2 = this.GetChildAt(0).x;
                if (pos2 + (_columnGap > 0 ? 0 : -_columnGap) > pos)
                {
                    for (int i = _firstIndex - _curLineItemCount; i >= 0; i -= _curLineItemCount)
                    {
                        pos2 -= (_virtualItems[i].size.x + _columnGap);
                        if (pos2 <= pos)
                        {
                            pos = pos2;
                            return i;
                        }
                    }

                    pos = 0;
                    return 0;
                }
                else
                {
                    float testGap = _columnGap > 0 ? _columnGap : 0;
                    for (int i = _firstIndex; i < _realNumItems; i += _curLineItemCount)
                    {
                        float pos3 = pos2 + _virtualItems[i].size.x;
                        if (pos3 + testGap > pos)
                        {
                            pos = pos2;
                            return i;
                        }
                        pos2 = pos3 + _columnGap;
                    }

                    pos = pos2;
                    return _realNumItems - _curLineItemCount;
                }
            }
            else
            {
                float pos2 = 0;
                float testGap = _columnGap > 0 ? _columnGap : 0;
                for (int i = 0; i < _realNumItems; i += _curLineItemCount)
                {
                    float pos3 = pos2 + _virtualItems[i].size.x;
                    if (pos3 + testGap > pos)
                    {
                        pos = pos2;
                        return i;
                    }
                    pos2 = pos3 + _columnGap;
                }

                pos = pos2;
                return _realNumItems - _curLineItemCount;
            }
        }

        int GetIndexOnPos3(ref float pos, bool forceUpdate)
        {
            if (_realNumItems < _curLineItemCount)
            {
                pos = 0;
                return 0;
            }

            float viewWidth = this.viewWidth;
            int page = Mathf.FloorToInt(pos / viewWidth);
            int startIndex = page * (_curLineItemCount * _curLineItemCount2);
            float pos2 = page * viewWidth;
            float testGap = _columnGap > 0 ? _columnGap : 0;
            for (int i = 0; i < _curLineItemCount; i++)
            {
                float pos3 = pos2 + _virtualItems[startIndex + i].size.x;
                if (pos3 + testGap > pos)
                {
                    pos = pos2;
                    return startIndex + i;
                }
                pos2 = pos3 + _columnGap;
            }

            pos = pos2;
            return startIndex + _curLineItemCount - 1;
        }

        void HandleScroll(bool forceUpdate)
        {
            if ((_miscFlags & 1) != 0)
                return;

            if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
            {
                int enterCounter = 0;
                while (HandleScroll1(forceUpdate))
                {
                    //可能会因为ITEM资源改变导致ITEM大小发生改变，所有出现最后一页填不满的情况，这时要反复尝试填满。
                    enterCounter++;
                    forceUpdate = false;
                    if (enterCounter > 20)
                    {
                        Debug.Log("FairyGUI: list will never be filled as the item renderer function always returns a different size.");
                        break;
                    }
                }

                HandleArchOrder1();
            }
            else if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowVertical)
            {
                int enterCounter = 0;
                while (HandleScroll2(forceUpdate))
                {
                    enterCounter++;
                    forceUpdate = false;
                    if (enterCounter > 20)
                    {
                        Debug.Log("FairyGUI: list will never be filled as the item renderer function always returns a different size.");
                        break;
                    }
                }

                HandleArchOrder2();
            }
            else
            {
                HandleScroll3(forceUpdate);
            }

            _boundsChanged = false;
        }

        bool HandleScroll1(bool forceUpdate)
        {
            float pos = scrollPane.scrollingPosY;
            float max = pos + scrollPane.viewHeight;
            bool end = max == scrollPane.contentHeight;//这个标志表示当前需要滚动到最末，无论内容变化大小

            //寻找当前位置的第一条项目
            int newFirstIndex = GetIndexOnPos1(ref pos, forceUpdate);
            if (newFirstIndex == _firstIndex && !forceUpdate)
                return false;

            int oldFirstIndex = _firstIndex;
            _firstIndex = newFirstIndex;
            int curIndex = newFirstIndex;
            bool forward = oldFirstIndex > newFirstIndex;
            int childCount = this.numChildren;
            int lastIndex = oldFirstIndex + childCount - 1;
            int reuseIndex = forward ? lastIndex : oldFirstIndex;
            float curX = 0, curY = pos;
            bool needRender;
            float deltaSize = 0;
            float firstItemDeltaSize = 0;
            string url = _defaultItem;
            int partSize = (int)((scrollPane.viewWidth - _columnGap * (_curLineItemCount - 1)) / _curLineItemCount);

            itemInfoVer++;
            while (curIndex < _realNumItems && (end || curY < max))
            {
                ItemInfo ii = _virtualItems[curIndex];

                if (ii.obj == null || forceUpdate)
                {
                    if (itemProvider != null)
                    {
                        url = itemProvider(curIndex % _numItems);
                        if (url == null)
                            url = _defaultItem;
                        url = UIPackage.NormalizeURL(url);
                    }

                    if (ii.obj != null && ii.obj.resourceURL != url)
                    {
                        if (ii.obj is GButton)
                            ii.selected = ((GButton)ii.obj).selected;
                        RemoveChildToPool(ii.obj);
                        ii.obj = null;
                    }
                }

                if (ii.obj == null)
                {
                    //搜索最适合的重用item，保证每次刷新需要新建或者重新render的item最少
                    if (forward)
                    {
                        for (int j = reuseIndex; j >= oldFirstIndex; j--)
                        {
                            ItemInfo ii2 = _virtualItems[j];
                            if (ii2.obj != null && ii2.updateFlag != itemInfoVer && ii2.obj.resourceURL == url)
                            {
                                if (ii2.obj is GButton)
                                    ii2.selected = ((GButton)ii2.obj).selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex--;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int j = reuseIndex; j <= lastIndex; j++)
                        {
                            ItemInfo ii2 = _virtualItems[j];
                            if (ii2.obj != null && ii2.updateFlag != itemInfoVer && ii2.obj.resourceURL == url)
                            {
                                if (ii2.obj is GButton)
                                    ii2.selected = ((GButton)ii2.obj).selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex++;
                                break;
                            }
                        }
                    }

                    if (ii.obj != null)
                    {
                        SetChildIndex(ii.obj, forward ? curIndex - newFirstIndex : numChildren);
                    }
                    else
                    {
                        ii.obj = _pool.GetObject(url);
                        if (forward)
                            this.AddChildAt(ii.obj, curIndex - newFirstIndex);
                        else
                            this.AddChild(ii.obj);
                    }
                    if (ii.obj is GButton)
                        ((GButton)ii.obj).selected = ii.selected;

                    needRender = true;
                }
                else
                    needRender = forceUpdate;

                if (needRender)
                {
                    if (_autoResizeItem && (_layout == ListLayoutType.SingleColumn || _columnCount > 0))
                        ii.obj.SetSize(partSize, ii.obj.height, true);

                    itemRenderer(curIndex % _numItems, ii.obj);
                    if (curIndex % _curLineItemCount == 0)
                    {
                        deltaSize += Mathf.CeilToInt(ii.obj.size.y) - ii.size.y;
                        if (curIndex == newFirstIndex && oldFirstIndex > newFirstIndex)
                        {
                            //当内容向下滚动时，如果新出现的项目大小发生变化，需要做一个位置补偿，才不会导致滚动跳动
                            firstItemDeltaSize = Mathf.CeilToInt(ii.obj.size.y) - ii.size.y;
                        }
                    }
                    ii.size.x = Mathf.CeilToInt(ii.obj.size.x);
                    ii.size.y = Mathf.CeilToInt(ii.obj.size.y);
                }

                ii.updateFlag = itemInfoVer;
                ii.obj.SetXY(curX, curY);
                if (curIndex == newFirstIndex) //要显示多一条才不会穿帮
                    max += ii.size.y;

                curX += ii.size.x + _columnGap;

                if (curIndex % _curLineItemCount == _curLineItemCount - 1)
                {
                    curX = 0;
                    curY += ii.size.y + _lineGap;
                }
                curIndex++;
            }

            for (int i = 0; i < childCount; i++)
            {
                ItemInfo ii = _virtualItems[oldFirstIndex + i];
                if (ii.updateFlag != itemInfoVer && ii.obj != null)
                {
                    if (ii.obj is GButton)
                        ii.selected = ((GButton)ii.obj).selected;
                    RemoveChildToPool(ii.obj);
                    ii.obj = null;
                }
            }

            childCount = _children.Count;
            for (int i = 0; i < childCount; i++)
            {
                GObject obj = _virtualItems[newFirstIndex + i].obj;
                if (_children[i] != obj)
                    SetChildIndex(obj, i);
            }

            if (deltaSize != 0 || firstItemDeltaSize != 0)
                this.scrollPane.ChangeContentSizeOnScrolling(0, deltaSize, 0, firstItemDeltaSize);

            if (curIndex > 0 && this.numChildren > 0 && this.container.y <= 0 && GetChildAt(0).y > -this.container.y)//最后一页没填满！
                return true;
            else
                return false;
        }

        bool HandleScroll2(bool forceUpdate)
        {
            float pos = scrollPane.scrollingPosX;
            float max = pos + scrollPane.viewWidth;
            bool end = pos == scrollPane.contentWidth;//这个标志表示当前需要滚动到最末，无论内容变化大小

            //寻找当前位置的第一条项目
            int newFirstIndex = GetIndexOnPos2(ref pos, forceUpdate);
            if (newFirstIndex == _firstIndex && !forceUpdate)
                return false;

            int oldFirstIndex = _firstIndex;
            _firstIndex = newFirstIndex;
            int curIndex = newFirstIndex;
            bool forward = oldFirstIndex > newFirstIndex;
            int childCount = this.numChildren;
            int lastIndex = oldFirstIndex + childCount - 1;
            int reuseIndex = forward ? lastIndex : oldFirstIndex;
            float curX = pos, curY = 0;
            bool needRender;
            float deltaSize = 0;
            float firstItemDeltaSize = 0;
            string url = _defaultItem;
            int partSize = (int)((scrollPane.viewHeight - _lineGap * (_curLineItemCount - 1)) / _curLineItemCount);

            itemInfoVer++;
            while (curIndex < _realNumItems && (end || curX < max))
            {
                ItemInfo ii = _virtualItems[curIndex];

                if (ii.obj == null || forceUpdate)
                {
                    if (itemProvider != null)
                    {
                        url = itemProvider(curIndex % _numItems);
                        if (url == null)
                            url = _defaultItem;
                        url = UIPackage.NormalizeURL(url);
                    }

                    if (ii.obj != null && ii.obj.resourceURL != url)
                    {
                        if (ii.obj is GButton)
                            ii.selected = ((GButton)ii.obj).selected;
                        RemoveChildToPool(ii.obj);
                        ii.obj = null;
                    }
                }

                if (ii.obj == null)
                {
                    if (forward)
                    {
                        for (int j = reuseIndex; j >= oldFirstIndex; j--)
                        {
                            ItemInfo ii2 = _virtualItems[j];
                            if (ii2.obj != null && ii2.updateFlag != itemInfoVer && ii2.obj.resourceURL == url)
                            {
                                if (ii2.obj is GButton)
                                    ii2.selected = ((GButton)ii2.obj).selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex--;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int j = reuseIndex; j <= lastIndex; j++)
                        {
                            ItemInfo ii2 = _virtualItems[j];
                            if (ii2.obj != null && ii2.updateFlag != itemInfoVer && ii2.obj.resourceURL == url)
                            {
                                if (ii2.obj is GButton)
                                    ii2.selected = ((GButton)ii2.obj).selected;
                                ii.obj = ii2.obj;
                                ii2.obj = null;
                                if (j == reuseIndex)
                                    reuseIndex++;
                                break;
                            }
                        }
                    }

                    if (ii.obj != null)
                    {
                        SetChildIndex(ii.obj, forward ? curIndex - newFirstIndex : numChildren);
                    }
                    else
                    {
                        ii.obj = _pool.GetObject(url);
                        if (forward)
                            this.AddChildAt(ii.obj, curIndex - newFirstIndex);
                        else
                            this.AddChild(ii.obj);
                    }
                    if (ii.obj is GButton)
                        ((GButton)ii.obj).selected = ii.selected;

                    needRender = true;
                }
                else
                    needRender = forceUpdate;

                if (needRender)
                {
                    if (_autoResizeItem && (_layout == ListLayoutType.SingleRow || _lineCount > 0))
                        ii.obj.SetSize(ii.obj.width, partSize, true);

                    itemRenderer(curIndex % _numItems, ii.obj);
                    if (curIndex % _curLineItemCount == 0)
                    {
                        deltaSize += Mathf.CeilToInt(ii.obj.size.x) - ii.size.x;
                        if (curIndex == newFirstIndex && oldFirstIndex > newFirstIndex)
                        {
                            //当内容向下滚动时，如果新出现的一个项目大小发生变化，需要做一个位置补偿，才不会导致滚动跳动
                            firstItemDeltaSize = Mathf.CeilToInt(ii.obj.size.x) - ii.size.x;
                        }
                    }
                    ii.size.x = Mathf.CeilToInt(ii.obj.size.x);
                    ii.size.y = Mathf.CeilToInt(ii.obj.size.y);
                }

                ii.updateFlag = itemInfoVer;
                ii.obj.SetXY(curX, curY);
                if (curIndex == newFirstIndex) //要显示多一条才不会穿帮
                    max += ii.size.x;

                curY += ii.size.y + _lineGap;

                if (curIndex % _curLineItemCount == _curLineItemCount - 1)
                {
                    curY = 0;
                    curX += ii.size.x + _columnGap;
                }
                curIndex++;
            }

            for (int i = 0; i < childCount; i++)
            {
                ItemInfo ii = _virtualItems[oldFirstIndex + i];
                if (ii.updateFlag != itemInfoVer && ii.obj != null)
                {
                    if (ii.obj is GButton)
                        ii.selected = ((GButton)ii.obj).selected;
                    RemoveChildToPool(ii.obj);
                    ii.obj = null;
                }
            }

            childCount = _children.Count;
            for (int i = 0; i < childCount; i++)
            {
                GObject obj = _virtualItems[newFirstIndex + i].obj;
                if (_children[i] != obj)
                    SetChildIndex(obj, i);
            }

            if (deltaSize != 0 || firstItemDeltaSize != 0)
                this.scrollPane.ChangeContentSizeOnScrolling(deltaSize, 0, firstItemDeltaSize, 0);

            if (curIndex > 0 && this.numChildren > 0 && this.container.x <= 0 && GetChildAt(0).x > -this.container.x)//最后一页没填满！
                return true;
            else
                return false;
        }

        void HandleScroll3(bool forceUpdate)
        {
            float pos = scrollPane.scrollingPosX;

            //寻找当前位置的第一条项目
            int newFirstIndex = GetIndexOnPos3(ref pos, forceUpdate);
            if (newFirstIndex == _firstIndex && !forceUpdate)
                return;

            int oldFirstIndex = _firstIndex;
            _firstIndex = newFirstIndex;

            //分页模式不支持不等高，所以渲染满一页就好了

            int reuseIndex = oldFirstIndex;
            int virtualItemCount = _virtualItems.Count;
            int pageSize = _curLineItemCount * _curLineItemCount2;
            int startCol = newFirstIndex % _curLineItemCount;
            float viewWidth = this.viewWidth;
            int page = (int)(newFirstIndex / pageSize);
            int startIndex = page * pageSize;
            int lastIndex = startIndex + pageSize * 2; //测试两页
            bool needRender;
            string url = _defaultItem;
            int partWidth = (int)((scrollPane.viewWidth - _columnGap * (_curLineItemCount - 1)) / _curLineItemCount);
            int partHeight = (int)((scrollPane.viewHeight - _lineGap * (_curLineItemCount2 - 1)) / _curLineItemCount2);
            itemInfoVer++;

            //先标记这次要用到的项目
            for (int i = startIndex; i < lastIndex; i++)
            {
                if (i >= _realNumItems)
                    continue;

                int col = i % _curLineItemCount;
                if (i - startIndex < pageSize)
                {
                    if (col < startCol)
                        continue;
                }
                else
                {
                    if (col > startCol)
                        continue;
                }

                ItemInfo ii = _virtualItems[i];
                ii.updateFlag = itemInfoVer;
            }

            GObject lastObj = null;
            int insertIndex = 0;
            for (int i = startIndex; i < lastIndex; i++)
            {
                if (i >= _realNumItems)
                    continue;

                ItemInfo ii = _virtualItems[i];
                if (ii.updateFlag != itemInfoVer)
                    continue;

                if (ii.obj == null)
                {
                    //寻找看有没有可重用的
                    while (reuseIndex < virtualItemCount)
                    {
                        ItemInfo ii2 = _virtualItems[reuseIndex];
                        if (ii2.obj != null && ii2.updateFlag != itemInfoVer)
                        {
                            if (ii2.obj is GButton)
                                ii2.selected = ((GButton)ii2.obj).selected;
                            ii.obj = ii2.obj;
                            ii2.obj = null;
                            break;
                        }
                        reuseIndex++;
                    }

                    if (insertIndex == -1)
                        insertIndex = GetChildIndex(lastObj) + 1;

                    if (ii.obj == null)
                    {
                        if (itemProvider != null)
                        {
                            url = itemProvider(i % _numItems);
                            if (url == null)
                                url = _defaultItem;
                            url = UIPackage.NormalizeURL(url);
                        }

                        ii.obj = _pool.GetObject(url);
                        this.AddChildAt(ii.obj, insertIndex);
                    }
                    else
                    {
                        insertIndex = SetChildIndexBefore(ii.obj, insertIndex);
                    }
                    insertIndex++;

                    if (ii.obj is GButton)
                        ((GButton)ii.obj).selected = ii.selected;

                    needRender = true;
                }
                else
                {
                    needRender = forceUpdate;
                    insertIndex = -1;
                    lastObj = ii.obj;
                }

                if (needRender)
                {
                    if (_autoResizeItem)
                    {
                        if (_curLineItemCount == _columnCount && _curLineItemCount2 == _lineCount)
                            ii.obj.SetSize(partWidth, partHeight, true);
                        else if (_curLineItemCount == _columnCount)
                            ii.obj.SetSize(partWidth, ii.obj.height, true);
                        else if (_curLineItemCount2 == _lineCount)
                            ii.obj.SetSize(ii.obj.width, partHeight, true);
                    }

                    itemRenderer(i % _numItems, ii.obj);
                    ii.size.x = Mathf.CeilToInt(ii.obj.size.x);
                    ii.size.y = Mathf.CeilToInt(ii.obj.size.y);
                }
            }

            //排列item
            float borderX = (startIndex / pageSize) * viewWidth;
            float xx = borderX;
            float yy = 0;
            float lineHeight = 0;
            for (int i = startIndex; i < lastIndex; i++)
            {
                if (i >= _realNumItems)
                    continue;

                ItemInfo ii = _virtualItems[i];
                if (ii.updateFlag == itemInfoVer)
                    ii.obj.SetXY(xx, yy);

                if (ii.size.y > lineHeight)
                    lineHeight = ii.size.y;
                if (i % _curLineItemCount == _curLineItemCount - 1)
                {
                    xx = borderX;
                    yy += lineHeight + _lineGap;
                    lineHeight = 0;

                    if (i == startIndex + pageSize - 1)
                    {
                        borderX += viewWidth;
                        xx = borderX;
                        yy = 0;
                    }
                }
                else
                    xx += ii.size.x + _columnGap;
            }

            //释放未使用的
            for (int i = reuseIndex; i < virtualItemCount; i++)
            {
                ItemInfo ii = _virtualItems[i];
                if (ii.updateFlag != itemInfoVer && ii.obj != null)
                {
                    if (ii.obj is GButton)
                        ii.selected = ((GButton)ii.obj).selected;
                    RemoveChildToPool(ii.obj);
                    ii.obj = null;
                }
            }
        }

        void HandleArchOrder1()
        {
            if (this.childrenRenderOrder == ChildrenRenderOrder.Arch)
            {
                float mid = this.scrollPane.posY + this.viewHeight / 2;
                float minDist = int.MaxValue, dist;
                int apexIndex = 0;
                int cnt = this.numChildren;
                for (int i = 0; i < cnt; i++)
                {
                    GObject obj = GetChildAt(i);
                    if (!foldInvisibleItems || obj.visible)
                    {
                        dist = Mathf.Abs(mid - obj.y - obj.height / 2);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            apexIndex = i;
                        }
                    }
                }
                this.apexIndex = apexIndex;
            }
        }

        void HandleArchOrder2()
        {
            if (this.childrenRenderOrder == ChildrenRenderOrder.Arch)
            {
                float mid = this.scrollPane.posX + this.viewWidth / 2;
                float minDist = int.MaxValue, dist;
                int apexIndex = 0;
                int cnt = this.numChildren;
                for (int i = 0; i < cnt; i++)
                {
                    GObject obj = GetChildAt(i);
                    if (!foldInvisibleItems || obj.visible)
                    {
                        dist = Mathf.Abs(mid - obj.x - obj.width / 2);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            apexIndex = i;
                        }
                    }
                }
                this.apexIndex = apexIndex;
            }
        }

        override public void GetSnappingPositionWithDir(ref float xValue, ref float yValue, float xDir, float yDir)
        {
            if (_virtual)
            {
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                {
                    float saved = yValue;
                    int index = GetIndexOnPos1(ref yValue, false);
                    if (index < _virtualItems.Count && index < _realNumItems)
                    {
                        float size = _virtualItems[index].size.y;
                        if (ShouldSnapToNext(yDir, saved - yValue, size))
                            yValue += size + _lineGap;
                    }
                }
                else if (_layout == ListLayoutType.SingleRow || _layout == ListLayoutType.FlowVertical)
                {
                    float saved = xValue;
                    int index = GetIndexOnPos2(ref xValue, false);
                    if (index < _virtualItems.Count && index < _realNumItems)
                    {
                        float size = _virtualItems[index].size.x;
                        if (ShouldSnapToNext(xDir, saved - xValue, size))
                            xValue += size + _columnGap;
                    }
                }
                else
                {
                    float saved = xValue;
                    int index = GetIndexOnPos3(ref xValue, false);
                    if (index < _virtualItems.Count && index < _realNumItems)
                    {
                        float size = _virtualItems[index].size.x;
                        if (ShouldSnapToNext(xDir, saved - xValue, size))
                            xValue += size + _columnGap;
                    }
                }
            }
            else
                base.GetSnappingPositionWithDir(ref xValue, ref yValue, xDir, yDir);
        }

        private void HandleAlign(float contentWidth, float contentHeight)
        {
            Vector2 newOffset = Vector2.zero;

            if (contentHeight < viewHeight)
            {
                if (_verticalAlign == VertAlignType.Middle)
                    newOffset.y = (int)((viewHeight - contentHeight) / 2);
                else if (_verticalAlign == VertAlignType.Bottom)
                    newOffset.y = viewHeight - contentHeight;
            }

            if (contentWidth < this.viewWidth)
            {
                if (_align == AlignType.Center)
                    newOffset.x = (int)((viewWidth - contentWidth) / 2);
                else if (_align == AlignType.Right)
                    newOffset.x = viewWidth - contentWidth;
            }

            if (newOffset != _alignOffset)
            {
                _alignOffset = newOffset;
                if (scrollPane != null)
                    scrollPane.AdjustMaskContainer();
                else
                    container.SetXY(_margin.left + _alignOffset.x, _margin.top + _alignOffset.y);
            }
        }

        override protected void UpdateBounds()
        {
            if (_virtual)
                return;

            int cnt = _children.Count;
            int i;
            int j = 0;
            GObject child;
            float curX = 0;
            float curY = 0;
            float cw, ch;
            float maxWidth = 0;
            float maxHeight = 0;
            float viewWidth = this.viewWidth;
            float viewHeight = this.viewHeight;

            if (_layout == ListLayoutType.SingleColumn)
            {
                for (i = 0; i < cnt; i++)
                {
                    child = GetChildAt(i);
                    if (foldInvisibleItems && !child.visible)
                        continue;

                    if (curY != 0)
                        curY += _lineGap;
                    child.y = curY;
                    if (_autoResizeItem)
                        child.SetSize(viewWidth, child.height, true);
                    curY += Mathf.CeilToInt(child.height);
                    if (child.width > maxWidth)
                        maxWidth = child.width;
                }

                ch = curY;
                if (ch <= viewHeight && _autoResizeItem && scrollPane != null && scrollPane._displayInDemand && scrollPane.vtScrollBar != null)
                {
                    viewWidth += scrollPane.vtScrollBar.width;
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        child.SetSize(viewWidth, child.height, true);
                        if (child.width > maxWidth)
                            maxWidth = child.width;
                    }
                }
                cw = Mathf.CeilToInt(maxWidth);
            }
            else if (_layout == ListLayoutType.SingleRow)
            {
                for (i = 0; i < cnt; i++)
                {
                    child = GetChildAt(i);
                    if (foldInvisibleItems && !child.visible)
                        continue;

                    if (curX != 0)
                        curX += _columnGap;
                    child.x = curX;
                    if (_autoResizeItem)
                        child.SetSize(child.width, viewHeight, true);
                    curX += Mathf.CeilToInt(child.width);
                    if (child.height > maxHeight)
                        maxHeight = child.height;
                }

                cw = curX;
                if (cw <= viewWidth && _autoResizeItem && scrollPane != null && scrollPane._displayInDemand && scrollPane.hzScrollBar != null)
                {
                    viewHeight += scrollPane.hzScrollBar.height;
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        child.SetSize(child.width, viewHeight, true);
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                    }
                }
                ch = Mathf.CeilToInt(maxHeight);
            }
            else if (_layout == ListLayoutType.FlowHorizontal)
            {
                if (_autoResizeItem && _columnCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        lineSize += child.sourceWidth;
                        j++;
                        if (j == _columnCount || i == cnt - 1)
                        {
                            remainSize = viewWidth - (j - 1) * _columnGap;
                            remainPercent = 1;
                            curX = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                child.SetXY(curX, curY);
                                float perc = child.sourceWidth / lineSize;
                                child.SetSize(Mathf.Round(perc / remainPercent * remainSize), child.height, true);
                                remainSize -= child.width;
                                remainPercent -= perc;
                                curX += child.width + _columnGap;

                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Mathf.CeilToInt(maxHeight) + _lineGap;
                            maxHeight = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    ch = curY + Mathf.CeilToInt(maxHeight);
                    cw = viewWidth;
                }
                else
                {
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curX != 0)
                            curX += _columnGap;

                        if (_columnCount != 0 && j >= _columnCount
                            || _columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0)
                        {
                            //new line
                            curX = 0;
                            curY += Mathf.CeilToInt(maxHeight) + _lineGap;
                            maxHeight = 0;
                            j = 0;
                        }
                        child.SetXY(curX, curY);
                        curX += Mathf.CeilToInt(child.width);
                        if (curX > maxWidth)
                            maxWidth = curX;
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                        j++;
                    }
                    ch = curY + Mathf.CeilToInt(maxHeight);
                    cw = Mathf.CeilToInt(maxWidth);
                }
            }
            else if (_layout == ListLayoutType.FlowVertical)
            {
                if (_autoResizeItem && _lineCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        lineSize += child.sourceHeight;
                        j++;
                        if (j == _lineCount || i == cnt - 1)
                        {
                            remainSize = viewHeight - (j - 1) * _lineGap;
                            remainPercent = 1;
                            curY = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                child.SetXY(curX, curY);
                                float perc = child.sourceHeight / lineSize;
                                child.SetSize(child.width, Mathf.Round(perc / remainPercent * remainSize), true);
                                remainSize -= child.height;
                                remainPercent -= perc;
                                curY += child.height + _lineGap;

                                if (child.width > maxWidth)
                                    maxWidth = child.width;
                            }
                            //new line
                            curX += Mathf.CeilToInt(maxWidth) + _columnGap;
                            maxWidth = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    cw = curX + Mathf.CeilToInt(maxWidth);
                    ch = viewHeight;
                }
                else
                {
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curY != 0)
                            curY += _lineGap;

                        if (_lineCount != 0 && j >= _lineCount
                            || _lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0)
                        {
                            curY = 0;
                            curX += Mathf.CeilToInt(maxWidth) + _columnGap;
                            maxWidth = 0;
                            j = 0;
                        }
                        child.SetXY(curX, curY);
                        curY += child.height;
                        if (curY > maxHeight)
                            maxHeight = curY;
                        if (child.width > maxWidth)
                            maxWidth = child.width;
                        j++;
                    }
                    cw = curX + Mathf.CeilToInt(maxWidth);
                    ch = Mathf.CeilToInt(maxHeight);
                }
            }
            else //pagination
            {
                int page = 0;
                int k = 0;
                float eachHeight = 0;
                if (_autoResizeItem && _lineCount > 0)
                    eachHeight = Mathf.Floor((viewHeight - (_lineCount - 1) * _lineGap) / _lineCount);

                if (_autoResizeItem && _columnCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (j == 0 && (_lineCount != 0 && k >= _lineCount
                            || _lineCount == 0 && curY + (_lineCount > 0 ? eachHeight : child.height) > viewHeight))
                        {
                            //new page
                            page++;
                            curY = 0;
                            k = 0;
                        }

                        lineSize += child.sourceWidth;
                        j++;
                        if (j == _columnCount || i == cnt - 1)
                        {
                            remainSize = viewWidth - (j - 1) * _columnGap;
                            remainPercent = 1;
                            curX = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                child.SetXY(page * viewWidth + curX, curY);
                                float perc = child.sourceWidth / lineSize;
                                child.SetSize(Mathf.Round(perc / remainPercent * remainSize), _lineCount > 0 ? eachHeight : child.height, true);
                                remainSize -= child.width;
                                remainPercent -= perc;
                                curX += child.width + _columnGap;

                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Mathf.CeilToInt(maxHeight) + _lineGap;
                            maxHeight = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;

                            k++;
                        }
                    }
                }
                else
                {
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curX != 0)
                            curX += _columnGap;

                        if (_autoResizeItem && _lineCount > 0)
                            child.SetSize(child.width, eachHeight, true);

                        if (_columnCount != 0 && j >= _columnCount
                            || _columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0)
                        {
                            curX = 0;
                            curY += maxHeight + _lineGap;
                            maxHeight = 0;
                            j = 0;
                            k++;

                            if (_lineCount != 0 && k >= _lineCount
                                || _lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0)//new page
                            {
                                page++;
                                curY = 0;
                                k = 0;
                            }
                        }
                        child.SetXY(page * viewWidth + curX, curY);
                        curX += Mathf.CeilToInt(child.width);
                        if (curX > maxWidth)
                            maxWidth = curX;
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                        j++;
                    }
                }
                ch = page > 0 ? viewHeight : (curY + Mathf.CeilToInt(maxHeight));
                cw = (page + 1) * viewWidth;
            }

            HandleAlign(cw, ch);
            SetBounds(0, 0, cw, ch);

            InvalidateBatchingState(true);
        }

        override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 5);

            _layout = (ListLayoutType)buffer.ReadByte();
            selectionMode = (ListSelectionMode)buffer.ReadByte();
            _align = (AlignType)buffer.ReadByte();
            _verticalAlign = (VertAlignType)buffer.ReadByte();
            _lineGap = buffer.ReadShort();
            _columnGap = buffer.ReadShort();
            _lineCount = buffer.ReadShort();
            _columnCount = buffer.ReadShort();
            _autoResizeItem = buffer.ReadBool();
            _childrenRenderOrder = (ChildrenRenderOrder)buffer.ReadByte();
            _apexIndex = buffer.ReadShort();

            if (buffer.ReadBool())
            {
                _margin.top = buffer.ReadInt();
                _margin.bottom = buffer.ReadInt();
                _margin.left = buffer.ReadInt();
                _margin.right = buffer.ReadInt();
            }

            OverflowType overflow = (OverflowType)buffer.ReadByte();
            if (overflow == OverflowType.Scroll)
            {
                int savedPos = buffer.position;
                buffer.Seek(beginPos, 7);
                SetupScroll(buffer);
                buffer.position = savedPos;
            }
            else
                SetupOverflow(overflow);

            if (buffer.ReadBool())
            {
                int i1 = buffer.ReadInt();
                int i2 = buffer.ReadInt();
                this.clipSoftness = new Vector2(i1, i2);
            }

            if (buffer.version >= 2)
            {
                scrollItemToViewOnClick = buffer.ReadBool();
                foldInvisibleItems = buffer.ReadBool();
            }

            buffer.Seek(beginPos, 8);

            _defaultItem = buffer.ReadS();
            ReadItems(buffer);
        }

        virtual protected void ReadItems(ByteBuffer buffer)
        {
            int itemCount = buffer.ReadShort();
            for (int i = 0; i < itemCount; i++)
            {
                int nextPos = buffer.ReadUshort();
                nextPos += buffer.position;

                string str = buffer.ReadS();
                if (str == null)
                {
                    str = _defaultItem;
                    if (string.IsNullOrEmpty(str))
                    {
                        buffer.position = nextPos;
                        continue;
                    }
                }

                GObject obj = GetFromPool(str);
                if (obj != null)
                {
                    AddChild(obj);
                    SetupItem(buffer, obj);
                }

                buffer.position = nextPos;
            }
        }

        protected void SetupItem(ByteBuffer buffer, GObject obj)
        {
            string str;
            str = buffer.ReadS();
            if (str != null)
                obj.text = str;
            str = buffer.ReadS();
            if (str != null && (obj is GButton))
                (obj as GButton).selectedTitle = str;
            str = buffer.ReadS();
            if (str != null)
                obj.icon = str;
            str = buffer.ReadS();
            if (str != null && (obj is GButton))
                (obj as GButton).selectedIcon = str;
            str = buffer.ReadS();
            if (str != null)
                obj.name = str;

            if (obj is GComponent)
            {
                int cnt = buffer.ReadShort();
                for (int i = 0; i < cnt; i++)
                {
                    Controller cc = ((GComponent)obj).GetController(buffer.ReadS());
                    str = buffer.ReadS();
                    if (cc != null)
                        cc.selectedPageId = str;
                }

                if (buffer.version >= 2)
                {
                    cnt = buffer.ReadShort();
                    for (int i = 0; i < cnt; i++)
                    {
                        string target = buffer.ReadS();
                        int propertyId = buffer.ReadShort();
                        string value = buffer.ReadS();
                        GObject obj2 = ((GComponent)obj).GetChildByPath(target);
                        if (obj2 != null)
                        {
                            if (propertyId == 0)
                                obj2.text = value;
                            else if (propertyId == 1)
                                obj2.icon = value;
                        }
                    }
                }
            }
        }

        override public void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_AfterAdd(buffer, beginPos);

            buffer.Seek(beginPos, 6);

            int i = buffer.ReadShort();
            if (i != -1)
                _selectionController = parent.GetControllerAt(i);
        }
    }
}
