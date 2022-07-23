using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;
#if FAIRYGUI_TOLUA
using LuaInterface;
#endif

namespace FairyGUI
{
    /// <summary>
    /// Component
    /// </summary>
    public class GComponent : GObject
    {
        /// <summary>
        /// Root container.
        /// </summary>
        public Container rootContainer { get; private set; }
        /// <summary>
        /// Content container. If the component is not clipped, then container==rootContainer.
        /// </summary>
        public Container container { get; protected set; }
        /// <summary>
        /// ScrollPane of the component. If the component is not scrollable, the value is null.
        /// </summary>
        public ScrollPane scrollPane { get; private set; }

        internal List<GObject> _children;
        internal List<Controller> _controllers;
        internal List<Transition> _transitions;
        internal bool _buildingDisplayList;

        protected Margin _margin;
        protected bool _trackBounds;
        protected bool _boundsChanged;
        protected ChildrenRenderOrder _childrenRenderOrder;
        protected int _apexIndex;
        internal Vector2 _alignOffset;

        Vector2 _clipSoftness;
        int _sortingChildCount;
        Action _buildDelegate;
        Controller _applyingController;

        EventListener _onDrop;

        public GComponent()
        {
            _children = new List<GObject>();
            _controllers = new List<Controller>();
            _transitions = new List<Transition>();
            _margin = new Margin();
            _buildDelegate = BuildNativeDisplayList;
        }

        override protected void CreateDisplayObject()
        {
            rootContainer = new Container("GComponent");
            rootContainer.gOwner = this;
            rootContainer.onUpdate += OnUpdate;
            container = rootContainer;

            displayObject = rootContainer;
        }

        override public void Dispose()
        {
            int cnt = _transitions.Count;
            for (int i = 0; i < cnt; ++i)
            {
                Transition trans = _transitions[i];
                trans.Dispose();
            }

            cnt = _controllers.Count;
            for (int i = 0; i < cnt; ++i)
            {
                Controller c = _controllers[i];
                c.Dispose();
            }

            if (scrollPane != null)
                scrollPane.Dispose();

            base.Dispose(); //Dispose native tree first, avoid DisplayObject.RemoveFromParent call

            cnt = _children.Count;
            for (int i = cnt - 1; i >= 0; --i)
            {
                GObject obj = _children[i];
                obj.InternalSetParent(null); //Avoid GObject.RemoveParent call
                obj.Dispose();
            }

#if FAIRYGUI_TOLUA
            if (_peerTable != null)
            {
                _peerTable.Dispose();
                _peerTable = null;
            }
#endif

#if FAIRYGUI_PUERTS
            if (__onDispose != null)
                __onDispose();
            __onConstruct = null;
            __onDispose = null;
#endif
        }

        /// <summary>
        /// Dispatched when an object was dragged and dropped to this component.
        /// </summary>
        public EventListener onDrop
        {
            get { return _onDrop ?? (_onDrop = new EventListener(this, "onDrop")); }
        }

        /// <summary>
        /// Draw call optimization switch.
        /// </summary>
        public bool fairyBatching
        {
            get { return rootContainer.fairyBatching; }
            set { rootContainer.fairyBatching = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childChanged"></param>
        public void InvalidateBatchingState(bool childChanged)
        {
            if (childChanged)
                container.InvalidateBatchingState(childChanged);
            else
                rootContainer.InvalidateBatchingState();
        }

        /// <summary>
        /// If true, mouse/touch events cannot pass through the empty area of the component. Default is true.
        /// </summary>
        public bool opaque
        {
            get { return rootContainer.opaque; }
            set { rootContainer.opaque = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Margin margin
        {
            get { return _margin; }
            set
            {
                _margin = value;
                if (rootContainer.clipRect != null && scrollPane == null) //如果scrollPane不为空，则HandleSizeChanged里面的处理会促使ScrollPane处理
                    container.SetXY(_margin.left + _alignOffset.x, _margin.top + _alignOffset.y);
                HandleSizeChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ChildrenRenderOrder childrenRenderOrder
        {
            get { return _childrenRenderOrder; }
            set
            {
                if (_childrenRenderOrder != value)
                {
                    _childrenRenderOrder = value;
                    BuildNativeDisplayList();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int apexIndex
        {
            get { return _apexIndex; }
            set
            {
                if (_apexIndex != value)
                {
                    _apexIndex = value;

                    if (_childrenRenderOrder == ChildrenRenderOrder.Arch)
                        BuildNativeDisplayList();
                }
            }
        }

        /// <summary>
        /// If true, children can be navigated by TAB from first to last, and repeat
        /// </summary>
        public bool tabStopChildren
        {
            get { return rootContainer.tabStopChildren; }
            set { rootContainer.tabStopChildren = value; }
        }

        /// <summary>
        /// Add a child to the component. It will be at the frontmost position.
        /// </summary>
        /// <param name="child">A child object</param>
        /// <returns>GObject</returns>
        public GObject AddChild(GObject child)
        {
            AddChildAt(child, _children.Count);
            return child;
        }

        /// <summary>
        /// Adds a child to the component at a certain index.
        /// </summary>
        /// <param name="child">A child object</param>
        /// <param name="index">Index</param>
        /// <returns>GObject</returns>
        virtual public GObject AddChildAt(GObject child, int index)
        {
            if (index >= 0 && index <= _children.Count)
            {
                if (child.parent == this)
                {
                    SetChildIndex(child, index);
                }
                else
                {
                    child.RemoveFromParent();
                    child.InternalSetParent(this);

                    int cnt = _children.Count;
                    if (child.sortingOrder != 0)
                    {
                        _sortingChildCount++;
                        index = GetInsertPosForSortingChild(child);
                    }
                    else if (_sortingChildCount > 0)
                    {
                        if (index > (cnt - _sortingChildCount))
                            index = cnt - _sortingChildCount;
                    }

                    if (index == cnt)
                        _children.Add(child);
                    else
                        _children.Insert(index, child);

                    ChildStateChanged(child);
                    SetBoundsChangedFlag();
                }
                return child;
            }
            else
            {
                throw new Exception("Invalid child index: " + index + ">" + _children.Count);
            }
        }

        int GetInsertPosForSortingChild(GObject target)
        {
            int cnt = _children.Count;
            int i;
            for (i = 0; i < cnt; i++)
            {
                GObject child = _children[i];
                if (child == target)
                    continue;

                if (target.sortingOrder < child.sortingOrder)
                    break;
            }
            return i;
        }

        /// <summary>
        /// Removes a child from the component. If the object is not a child, nothing happens. 
        /// </summary>
        /// <param name="child">A child object</param>
        /// <returns>GObject</returns>
        public GObject RemoveChild(GObject child)
        {
            return RemoveChild(child, false);
        }

        /// <summary>
        /// Removes a child from the component. If the object is not a child, nothing happens. 
        /// </summary>
        /// <param name="child">A child object</param>
        /// <param name="dispose">If true, the child will be disposed right away.</param>
        /// <returns>GObject</returns>
        public GObject RemoveChild(GObject child, bool dispose)
        {
            int childIndex = _children.IndexOf(child);
            if (childIndex != -1)
            {
                RemoveChildAt(childIndex, dispose);
            }
            return child;
        }

        /// <summary>
        /// Removes a child at a certain index. Children above the child will move down.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>GObject</returns>
        public GObject RemoveChildAt(int index)
        {
            return RemoveChildAt(index, false);
        }

        /// <summary>
        /// Removes a child at a certain index. Children above the child will move down.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="dispose">If true, the child will be disposed right away.</param>
        /// <returns>GObject</returns>
        virtual public GObject RemoveChildAt(int index, bool dispose)
        {
            if (index >= 0 && index < numChildren)
            {
                GObject child = _children[index];

                child.InternalSetParent(null);

                if (child.sortingOrder != 0)
                    _sortingChildCount--;

                _children.RemoveAt(index);
                child.group = null;
                if (child.inContainer)
                {
                    container.RemoveChild(child.displayObject);
                    if (_childrenRenderOrder == ChildrenRenderOrder.Arch)
                    {
                        UpdateContext.OnBegin -= _buildDelegate;
                        UpdateContext.OnBegin += _buildDelegate;
                    }
                }

                if (dispose)
                    child.Dispose();

                SetBoundsChangedFlag();
                return child;
            }
            else
                throw new Exception("Invalid child index: " + index + ">" + numChildren);
        }

        /// <summary>
        /// Remove all children.
        /// </summary>
        public void RemoveChildren()
        {
            RemoveChildren(0, -1, false);
        }

        /// <summary>
        /// Removes a range of children from the container (endIndex included). 
        /// </summary>
        /// <param name="beginIndex">Begin index.</param>
        /// <param name="endIndex">End index.(Included).</param>
        /// <param name="dispose">If true, the child will be disposed right away.</param>
        public void RemoveChildren(int beginIndex, int endIndex, bool dispose)
        {
            if (endIndex < 0 || endIndex >= numChildren)
                endIndex = numChildren - 1;

            for (int i = beginIndex; i <= endIndex; ++i)
                RemoveChildAt(beginIndex, dispose);
        }

        /// <summary>
        /// Returns a child object at a certain index. If index out of bounds, exception raised.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>A child object.</returns>
        public GObject GetChildAt(int index)
        {
            if (index >= 0 && index < numChildren)
                return _children[index];
            else
                throw new Exception("Invalid child index: " + index + ">" + numChildren);
        }

        /// <summary>
        /// Returns a child object with a certain name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>A child object. Null if not found.</returns>
        public GObject GetChild(string name)
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                if (_children[i].name == name)
                    return _children[i];
            }

            return null;
        }

        public GObject GetChildByPath(string path)
        {
            string[] arr = path.Split('.');
            int cnt = arr.Length;
            GComponent gcom = this;
            GObject obj = null;
            for (int i = 0; i < cnt; ++i)
            {
                obj = gcom.GetChild(arr[i]);
                if (obj == null)
                    break;

                if (i != cnt - 1)
                {
                    if (!(obj is GComponent))
                    {
                        obj = null;
                        break;
                    }
                    else
                        gcom = (GComponent)obj;
                }
            }

            return obj;
        }

        /// <summary>
        /// Returns a visible child object with a certain name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>A child object. Null if not found.</returns>
        public GObject GetVisibleChild(string name)
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                GObject child = _children[i];
                if (child.internalVisible && child.internalVisible2 && child.name == name)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// Returns a child object belong to a group with a certain name.
        /// </summary>
        /// <param name="group">A group object</param>
        /// <param name="name">Name</param>
        /// <returns>A child object. Null if not found.</returns>
        public GObject GetChildInGroup(GGroup group, string name)
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                GObject child = _children[i];
                if (child.group == group && child.name == name)
                    return child;
            }

            return null;
        }

        internal GObject GetChildById(string id)
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                if (_children[i].id == id)
                    return _children[i];
            }

            return null;
        }

        /// <summary>
        /// Returns a copy of all children with an array.
        /// </summary>
        /// <returns>An array contains all children</returns>
        public GObject[] GetChildren()
        {
            return _children.ToArray();
        }

        /// <summary>
        /// Returns the index of a child within the container, or "-1" if it is not found.
        /// </summary>
        /// <param name="child">A child object</param>
        /// <returns>Index of the child. -1 If not found.</returns>
        public int GetChildIndex(GObject child)
        {
            return _children.IndexOf(child);
        }

        /// <summary>
        /// Moves a child to a certain index. Children at and after the replaced position move up.
        /// </summary>
        /// <param name="child">A Child</param>
        /// <param name="index">Index</param>
        public void SetChildIndex(GObject child, int index)
        {
            int oldIndex = _children.IndexOf(child);
            if (oldIndex == -1)
                throw new ArgumentException("Not a child of this container");

            if (child.sortingOrder != 0) //no effect
                return;

            if (_sortingChildCount > 0)
            {
                int cnt = _children.Count;
                if (index > (cnt - _sortingChildCount - 1))
                    index = cnt - _sortingChildCount - 1;
            }

            _SetChildIndex(child, oldIndex, index);
        }

        /// <summary>
        /// Moves a child to a certain position which is in front of the child previously at given index.
        /// 与SetChildIndex不同的是，如果child原来在index的前面，那么child插入的位置是index-1，即保证排在原来占据index的对象的前面。
        /// </summary>
        /// <param name="child"></param>
        /// <param name="index"></param>
        public int SetChildIndexBefore(GObject child, int index)
        {
            int oldIndex = _children.IndexOf(child);
            if (oldIndex == -1)
                throw new ArgumentException("Not a child of this container");

            if (child.sortingOrder != 0) //no effect
                return oldIndex;

            int cnt = _children.Count;
            if (_sortingChildCount > 0)
            {
                if (index > (cnt - _sortingChildCount - 1))
                    index = cnt - _sortingChildCount - 1;
            }

            if (oldIndex < index)
                return _SetChildIndex(child, oldIndex, index - 1);
            else
                return _SetChildIndex(child, oldIndex, index);
        }

        int _SetChildIndex(GObject child, int oldIndex, int index)
        {
            int cnt = _children.Count;
            if (index > cnt)
                index = cnt;

            if (oldIndex == index)
                return oldIndex;

            _children.RemoveAt(oldIndex);
            if (index >= cnt)
                _children.Add(child);
            else
                _children.Insert(index, child);

            if (child.inContainer)
            {
                int displayIndex = 0;
                if (_childrenRenderOrder == ChildrenRenderOrder.Ascent)
                {
                    for (int i = 0; i < index; i++)
                    {
                        GObject g = _children[i];
                        if (g.inContainer)
                            displayIndex++;
                    }
                    container.SetChildIndex(child.displayObject, displayIndex);
                }
                else if (_childrenRenderOrder == ChildrenRenderOrder.Descent)
                {
                    for (int i = cnt - 1; i > index; i--)
                    {
                        GObject g = _children[i];
                        if (g.inContainer)
                            displayIndex++;
                    }
                    container.SetChildIndex(child.displayObject, displayIndex);
                }
                else
                {
                    UpdateContext.OnBegin -= _buildDelegate;
                    UpdateContext.OnBegin += _buildDelegate;
                }

                SetBoundsChangedFlag();
            }

            return index;
        }

        /// <summary>
        /// Swaps the indexes of two children. 
        /// </summary>
        /// <param name="child1">A child object</param>
        /// <param name="child2">A child object</param>
        public void SwapChildren(GObject child1, GObject child2)
        {
            int index1 = _children.IndexOf(child1);
            int index2 = _children.IndexOf(child2);
            if (index1 == -1 || index2 == -1)
                throw new Exception("Not a child of this container");
            SwapChildrenAt(index1, index2);
        }

        /// <summary>
        ///  Swaps the indexes of two children.
        /// </summary>
        /// <param name="index1">index of first child</param>
        /// <param name="index2">index of second child</param>
        public void SwapChildrenAt(int index1, int index2)
        {
            GObject child1 = _children[index1];
            GObject child2 = _children[index2];

            SetChildIndex(child1, index2);
            SetChildIndex(child2, index1);
        }

        /// <summary>
        /// The number of children of this component.
        /// </summary>
        public int numChildren
        {
            get { return _children.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsAncestorOf(GObject obj)
        {
            if (obj == null)
                return false;

            GComponent p = obj.parent;
            while (p != null)
            {
                if (p == this)
                    return true;

                p = p.parent;
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objs"></param>
        public void ChangeChildrenOrder(IList<GObject> objs)
        {
            int cnt = objs.Count;
            for (int i = 0; i < cnt; i++)
            {
                GObject obj = objs[i];
                if (obj.parent != this)
                    throw new Exception("Not a child of this container");

                _children[i] = obj;
            }
            BuildNativeDisplayList();
            SetBoundsChangedFlag();
        }

        /// <summary>
        /// Adds a controller to the container.
        /// </summary>
        /// <param name="controller">Controller object</param>
        public void AddController(Controller controller)
        {
            _controllers.Add(controller);
            controller.parent = this;
            ApplyController(controller);
        }

        /// <summary>
        /// Returns a controller object  at a certain index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Controller object.</returns>
        public Controller GetControllerAt(int index)
        {
            return _controllers[index];
        }

        /// <summary>
        /// Returns a controller object with a certain name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Controller object. Null if not found.</returns>
        public Controller GetController(string name)
        {
            int cnt = _controllers.Count;
            for (int i = 0; i < cnt; ++i)
            {
                Controller c = _controllers[i];
                if (c.name == name)
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Removes a controller from the container. 
        /// </summary>
        /// <param name="c">Controller object.</param>
        public void RemoveController(Controller c)
        {
            int index = _controllers.IndexOf(c);
            if (index == -1)
                throw new Exception("controller not exists: " + c.name);

            c.parent = null;
            _controllers.RemoveAt(index);

            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                GObject child = _children[i];
                child.HandleControllerChanged(c);
            }
        }

        /// <summary>
        /// Returns controller list.
        /// </summary>
        /// <returns>Controller list</returns>
        public List<Controller> Controllers
        {
            get { return _controllers; }
        }

        /// <summary>
        /// Returns a transition object  at a certain index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>transition object.</returns>
        public Transition GetTransitionAt(int index)
        {
            return _transitions[index];
        }

        /// <summary>
        /// Returns a transition object at a certain name. 
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Transition Object</returns>
        public Transition GetTransition(string name)
        {
            int cnt = _transitions.Count;
            for (int i = 0; i < cnt; ++i)
            {
                Transition trans = _transitions[i];
                if (trans.name == name)
                    return trans;
            }

            return null;
        }

        /// <summary>
        /// Returns transition list.
        /// </summary>
        /// <returns>Transition list</returns>
        public List<Transition> Transitions
        {
            get { return _transitions; }
        }

        internal void ChildStateChanged(GObject child)
        {
            if (_buildingDisplayList)
                return;

            int cnt = _children.Count;

            if (child is GGroup)
            {
                for (int i = 0; i < cnt; ++i)
                {
                    GObject g = _children[i];
                    if (g.group == child)
                        ChildStateChanged(g);
                }
                return;
            }

            if (child.displayObject == null)
                return;

            if (child.internalVisible)
            {
                if (child.displayObject.parent == null)
                {
                    if (_childrenRenderOrder == ChildrenRenderOrder.Ascent)
                    {
                        int index = 0;
                        for (int i = 0; i < cnt; i++)
                        {
                            GObject g = _children[i];
                            if (g == child)
                                break;

                            if (g.displayObject != null && g.displayObject.parent != null)
                                index++;
                        }
                        container.AddChildAt(child.displayObject, index);
                    }
                    else if (_childrenRenderOrder == ChildrenRenderOrder.Descent)
                    {
                        int index = 0;
                        for (int i = cnt - 1; i >= 0; i--)
                        {
                            GObject g = _children[i];
                            if (g == child)
                                break;

                            if (g.displayObject != null && g.displayObject.parent != null)
                                index++;
                        }
                        container.AddChildAt(child.displayObject, index);
                    }
                    else
                    {
                        container.AddChild(child.displayObject);

                        UpdateContext.OnBegin -= _buildDelegate;
                        UpdateContext.OnBegin += _buildDelegate;
                    }
                }
            }
            else
            {
                if (child.displayObject.parent != null)
                {
                    container.RemoveChild(child.displayObject);
                    if (_childrenRenderOrder == ChildrenRenderOrder.Arch)
                    {
                        UpdateContext.OnBegin -= _buildDelegate;
                        UpdateContext.OnBegin += _buildDelegate;
                    }
                }
            }
        }

        void BuildNativeDisplayList()
        {
            if (displayObject == null || displayObject.isDisposed)
                return;

            int cnt = _children.Count;
            if (cnt == 0)
                return;

            switch (_childrenRenderOrder)
            {
                case ChildrenRenderOrder.Ascent:
                    {
                        for (int i = 0; i < cnt; i++)
                        {
                            GObject child = _children[i];
                            if (child.displayObject != null && child.internalVisible)
                                container.AddChild(child.displayObject);
                        }
                    }
                    break;
                case ChildrenRenderOrder.Descent:
                    {
                        for (int i = cnt - 1; i >= 0; i--)
                        {
                            GObject child = _children[i];
                            if (child.displayObject != null && child.internalVisible)
                                container.AddChild(child.displayObject);
                        }
                    }
                    break;

                case ChildrenRenderOrder.Arch:
                    {
                        int apex = Mathf.Clamp(_apexIndex, 0, cnt);
                        for (int i = 0; i < apex; i++)
                        {
                            GObject child = _children[i];
                            if (child.displayObject != null && child.internalVisible)
                                container.AddChild(child.displayObject);
                        }
                        for (int i = cnt - 1; i >= apex; i--)
                        {
                            GObject child = _children[i];
                            if (child.displayObject != null && child.internalVisible)
                                container.AddChild(child.displayObject);
                        }
                    }
                    break;
            }
        }

        internal void ApplyController(Controller c)
        {
            _applyingController = c;
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                GObject child = _children[i];
                child.HandleControllerChanged(c);
            }
            _applyingController = null;

            c.RunActions();
        }

        void ApplyAllControllers()
        {
            int cnt = _controllers.Count;
            for (int i = 0; i < cnt; ++i)
            {
                Controller controller = _controllers[i];
                ApplyController(controller);
            }
        }

        internal void AdjustRadioGroupDepth(GObject obj, Controller c)
        {
            int cnt = _children.Count;
            int i;
            GObject child;
            int myIndex = -1, maxIndex = -1;
            for (i = 0; i < cnt; i++)
            {
                child = _children[i];
                if (child == obj)
                {
                    myIndex = i;
                }
                else if ((child is GButton)
                    && ((GButton)child).relatedController == c)
                {
                    if (i > maxIndex)
                        maxIndex = i;
                }
            }
            if (myIndex < maxIndex)
            {
                if (_applyingController != null)
                    _children[maxIndex].HandleControllerChanged(_applyingController);
                this.SwapChildrenAt(myIndex, maxIndex);
            }
        }

        /// <summary>
        /// If clipping softness is set, clipped containers will have soft border effect.
        /// </summary>
        public Vector2 clipSoftness
        {
            get { return _clipSoftness; }
            set
            {
                _clipSoftness = value;
                if (scrollPane != null)
                    scrollPane.UpdateClipSoft();
                else if (_clipSoftness.x > 0 || _clipSoftness.y > 0)
                    rootContainer.clipSoftness = new Vector4(value.x, value.y, value.x, value.y);
                else
                    rootContainer.clipSoftness = null;
            }
        }

        /// <summary>
        /// The mask of the component.
        /// </summary>
        public DisplayObject mask
        {
            get { return container.mask; }
            set
            {
                container.mask = value;
                if (value != null && value.parent != container)
                    container.AddChild(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool reversedMask
        {
            get { return container.reversedMask; }
            set { container.reversedMask = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string baseUserData
        {
            get
            {
                ByteBuffer buffer = packageItem.rawData;
                buffer.Seek(0, 4);
                return buffer.ReadS();
            }
        }

        /// <summary>
        /// Test if a child is in view.
        /// </summary>
        /// <param name="child">A child object</param>
        /// <returns>True if in view</returns>
        public bool IsChildInView(GObject child)
        {
            if (scrollPane != null)
            {
                return scrollPane.IsChildInView(child);
            }
            else if (rootContainer.clipRect != null)
            {
                return child.x + child.width >= 0 && child.x <= this.width
                    && child.y + child.height >= 0 && child.y <= this.height;
            }
            else
                return true;
        }

        virtual public int GetFirstChildInView()
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                GObject child = _children[i];
                if (IsChildInView(child))
                    return i;
            }
            return -1;
        }

        protected void SetupScroll(ByteBuffer buffer)
        {
            if (rootContainer == container)
            {
                container = new Container();
                rootContainer.AddChild(container);
            }

            scrollPane = new ScrollPane(this);
            scrollPane.Setup(buffer);
        }

        protected void SetupOverflow(OverflowType overflow)
        {
            if (overflow == OverflowType.Hidden)
            {
                if (rootContainer == container)
                {
                    container = new Container();
                    rootContainer.AddChild(container);
                }

                UpdateClipRect();
                container.SetXY(_margin.left, _margin.top);
            }
            else if (_margin.left != 0 || _margin.top != 0)
            {
                if (rootContainer == container)
                {
                    container = new Container();
                    rootContainer.AddChild(container);
                }

                container.SetXY(_margin.left, _margin.top);
            }
        }

        void UpdateClipRect()
        {
            if (scrollPane == null)
            {
                float w = this.width - (_margin.left + _margin.right);
                float h = this.height - (_margin.top + _margin.bottom);
                rootContainer.clipRect = new Rect(_margin.left, _margin.top, w, h);
            }
            else
                rootContainer.clipRect = new Rect(0, 0, this.width, this.height);
        }

        override protected void HandleSizeChanged()
        {
            base.HandleSizeChanged();

            if (scrollPane != null)
                scrollPane.OnOwnerSizeChanged();
            if (rootContainer.clipRect != null)
                UpdateClipRect();
        }

        override protected void HandleGrayedChanged()
        {
            Controller cc = GetController("grayed");
            if (cc != null)
                cc.selectedIndex = this.grayed ? 1 : 0;
            else
                base.HandleGrayedChanged();
        }

        override public void HandleControllerChanged(Controller c)
        {
            base.HandleControllerChanged(c);

            if (scrollPane != null)
                scrollPane.HandleControllerChanged(c);
        }

        /// <summary>
        /// Notify the component the bounds should recaculate.
        /// </summary>
        public void SetBoundsChangedFlag()
        {
            if (scrollPane == null && !_trackBounds)
                return;

            _boundsChanged = true;
        }

        /// <summary>
        /// Make sure the bounds of the component is correct. 
        /// Bounds of the component is not updated on every changed. For example, you add a new child to the list, children in the list will be rearranged in next frame.
        /// If you want to access the correct child position immediatelly, call this function first.
        /// </summary>
        public void EnsureBoundsCorrect()
        {
            if (_boundsChanged)
                UpdateBounds();
        }

        virtual protected void UpdateBounds()
        {
            float ax, ay, aw, ah;
            if (_children.Count > 0)
            {
                ax = int.MaxValue;
                ay = int.MaxValue;
                float ar = int.MinValue, ab = int.MinValue;
                float tmp;

                int cnt = _children.Count;
                for (int i = 0; i < cnt; ++i)
                {
                    GObject child = _children[i];
                    tmp = child.x;
                    if (tmp < ax)
                        ax = tmp;
                    tmp = child.y;
                    if (tmp < ay)
                        ay = tmp;
                    tmp = child.x + child.actualWidth;
                    if (tmp > ar)
                        ar = tmp;
                    tmp = child.y + child.actualHeight;
                    if (tmp > ab)
                        ab = tmp;
                }
                aw = ar - ax;
                ah = ab - ay;
            }
            else
            {
                ax = 0;
                ay = 0;
                aw = 0;
                ah = 0;
            }

            SetBounds(ax, ay, aw, ah);
        }

        protected void SetBounds(float ax, float ay, float aw, float ah)
        {
            _boundsChanged = false;
            if (scrollPane != null)
                scrollPane.SetContentSize(Mathf.RoundToInt(ax + aw), Mathf.RoundToInt(ay + ah));
        }

        /// <summary>
        /// Viwe port width of the container.
        /// </summary>
        public float viewWidth
        {
            get
            {
                if (scrollPane != null)
                    return scrollPane.viewWidth;
                else
                    return this.width - _margin.left - _margin.right;
            }

            set
            {
                if (scrollPane != null)
                    scrollPane.viewWidth = value;
                else
                    this.width = value + _margin.left + _margin.right;
            }
        }

        /// <summary>
        /// View port height of the container.
        /// </summary>
        public float viewHeight
        {
            get
            {
                if (scrollPane != null)
                    return scrollPane.viewHeight;
                else
                    return this.height - _margin.top - _margin.bottom;
            }

            set
            {
                if (scrollPane != null)
                    scrollPane.viewHeight = value;
                else
                    this.height = value + _margin.top + _margin.bottom;
            }
        }

        public void GetSnappingPosition(ref float xValue, ref float yValue)
        {
            GetSnappingPositionWithDir(ref xValue, ref yValue, 0, 0);
        }

        protected bool ShouldSnapToNext(float dir, float delta, float size)
        {
            return dir < 0 && delta > UIConfig.defaultScrollSnappingThreshold * size
                || dir > 0 && delta > (1 - UIConfig.defaultScrollSnappingThreshold) * size
                || dir == 0 && delta > size / 2;
        }

        /**
        * dir正数表示右移或者下移，负数表示左移或者上移
        */
        virtual public void GetSnappingPositionWithDir(ref float xValue, ref float yValue, float xDir, float yDir)
        {
            int cnt = _children.Count;
            if (cnt == 0)
                return;

            EnsureBoundsCorrect();

            GObject obj = null;

            int i = 0;
            if (yValue != 0)
            {
                for (; i < cnt; i++)
                {
                    obj = _children[i];
                    if (yValue < obj.y)
                    {
                        if (i == 0)
                        {
                            yValue = 0;
                            break;
                        }
                        else
                        {
                            GObject prev = _children[i - 1];
                            if (ShouldSnapToNext(yDir, yValue - prev.y, prev.height))
                                yValue = obj.y;
                            else
                                yValue = prev.y;
                            break;
                        }
                    }
                }

                if (i == cnt)
                    yValue = obj.y;
            }

            if (xValue != 0)
            {
                if (i > 0)
                    i--;
                for (; i < cnt; i++)
                {
                    obj = _children[i];
                    if (xValue < obj.x)
                    {
                        if (i == 0)
                        {
                            xValue = 0;
                            break;
                        }
                        else
                        {
                            GObject prev = _children[i - 1];
                            if (ShouldSnapToNext(xDir, xValue - prev.x, prev.width))
                                xValue = obj.x;
                            else
                                xValue = prev.x;
                            break;
                        }
                    }
                }
                if (i == cnt)
                    xValue = obj.x;
            }
        }

        internal void ChildSortingOrderChanged(GObject child, int oldValue, int newValue)
        {
            if (newValue == 0)
            {
                _sortingChildCount--;
                SetChildIndex(child, _children.Count);
            }
            else
            {
                if (oldValue == 0)
                    _sortingChildCount++;

                int oldIndex = _children.IndexOf(child);
                int index = GetInsertPosForSortingChild(child);
                if (oldIndex < index)
                    _SetChildIndex(child, oldIndex, index - 1);
                else
                    _SetChildIndex(child, oldIndex, index);
            }
        }

        /// <summary>
        /// 每帧调用的一个回调。如果你要override，请记住以下两点：
        /// 1、记得调用base.onUpdate;
        /// 2、不要在方法里进行任何会更改显示列表的操作，例如AddChild、RemoveChild、visible等。
        /// </summary>
        virtual protected void OnUpdate()
        {
            if (_boundsChanged)
                UpdateBounds();
        }

        override public void ConstructFromResource()
        {
            ConstructFromResource(null, 0);
        }

        internal void ConstructFromResource(List<GObject> objectPool, int poolIndex)
        {
            this.gameObjectName = packageItem.name;

            PackageItem contentItem = packageItem.getBranch();

            if (!contentItem.translated)
            {
                contentItem.translated = true;
                TranslationHelper.TranslateComponent(contentItem);
            }

            ByteBuffer buffer = contentItem.rawData;
            buffer.Seek(0, 0);

            underConstruct = true;

            sourceWidth = buffer.ReadInt();
            sourceHeight = buffer.ReadInt();
            initWidth = sourceWidth;
            initHeight = sourceHeight;

            SetSize(sourceWidth, sourceHeight);

            if (buffer.ReadBool())
            {
                minWidth = buffer.ReadInt();
                maxWidth = buffer.ReadInt();
                minHeight = buffer.ReadInt();
                maxHeight = buffer.ReadInt();
            }

            if (buffer.ReadBool())
            {
                float f1 = buffer.ReadFloat();
                float f2 = buffer.ReadFloat();
                SetPivot(f1, f2, buffer.ReadBool());
            }

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
                buffer.Seek(0, 7);
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

            _buildingDisplayList = true;

            buffer.Seek(0, 1);

            int controllerCount = buffer.ReadShort();
            for (int i = 0; i < controllerCount; i++)
            {
                int nextPos = buffer.ReadUshort();
                nextPos += buffer.position;

                Controller controller = new Controller();
                _controllers.Add(controller);
                controller.parent = this;
                controller.Setup(buffer);

                buffer.position = nextPos;
            }

            buffer.Seek(0, 2);

            GObject child;
            int childCount = buffer.ReadShort();
            for (int i = 0; i < childCount; i++)
            {
                int dataLen = buffer.ReadShort();
                int curPos = buffer.position;

                if (objectPool != null)
                    child = objectPool[poolIndex + i];
                else
                {
                    buffer.Seek(curPos, 0);

                    ObjectType type = (ObjectType)buffer.ReadByte();
                    string src = buffer.ReadS();
                    string pkgId = buffer.ReadS();

                    PackageItem pi = null;
                    if (src != null)
                    {
                        UIPackage pkg;
                        if (pkgId != null)
                            pkg = UIPackage.GetById(pkgId);
                        else
                            pkg = contentItem.owner;

                        pi = pkg != null ? pkg.GetItem(src) : null;
                    }

                    if (pi != null)
                    {
                        child = UIObjectFactory.NewObject(pi);
                        child.ConstructFromResource();
                    }
                    else
                        child = UIObjectFactory.NewObject(type);
                }

                child.underConstruct = true;
                child.Setup_BeforeAdd(buffer, curPos);
                child.InternalSetParent(this);
                _children.Add(child);

                buffer.position = curPos + dataLen;
            }

            buffer.Seek(0, 3);
            this.relations.Setup(buffer, true);

            buffer.Seek(0, 2);
            buffer.Skip(2);

            for (int i = 0; i < childCount; i++)
            {
                int nextPos = buffer.ReadUshort();
                nextPos += buffer.position;

                buffer.Seek(buffer.position, 3);
                _children[i].relations.Setup(buffer, false);

                buffer.position = nextPos;
            }

            buffer.Seek(0, 2);
            buffer.Skip(2);

            for (int i = 0; i < childCount; i++)
            {
                int nextPos = buffer.ReadUshort();
                nextPos += buffer.position;

                child = _children[i];
                child.Setup_AfterAdd(buffer, buffer.position);
                child.underConstruct = false;
                if (child.displayObject != null)
                    child.displayObject.cachedTransform.SetParent(this.displayObject.cachedTransform, false);

                buffer.position = nextPos;
            }

            buffer.Seek(0, 4);

            buffer.Skip(2); //customData
            this.opaque = buffer.ReadBool();
            int maskId = buffer.ReadShort();
            if (maskId != -1)
            {
                container.mask = GetChildAt(maskId).displayObject;
                if (buffer.ReadBool())
                    container.reversedMask = true;
            }

            {
                string hitTestId = buffer.ReadS();
                int i1 = buffer.ReadInt();
                int i2 = buffer.ReadInt();
                if (hitTestId != null)
                {
                    PackageItem pi = contentItem.owner.GetItem(hitTestId);
                    if (pi != null && pi.pixelHitTestData != null)
                        rootContainer.hitArea = new PixelHitTest(pi.pixelHitTestData, i1, i2, sourceWidth, sourceHeight);
                }
                else if (i1 != 0 && i2 != -1)
                {
                    rootContainer.hitArea = new ShapeHitTest(this.GetChildAt(i2).displayObject);
                }
            }

            if (buffer.version >= 5)
            {
                string str = buffer.ReadS();
                if (!string.IsNullOrEmpty(str))
                    this.onAddedToStage.Add(() => __playSound(str, 1));

                string str2 = buffer.ReadS();
                if (!string.IsNullOrEmpty(str2))
                    this.onRemovedFromStage.Add(() => __playSound(str2, 1));
            }

            buffer.Seek(0, 5);

            int transitionCount = buffer.ReadShort();
            for (int i = 0; i < transitionCount; i++)
            {
                int nextPos = buffer.ReadUshort();
                nextPos += buffer.position;

                Transition trans = new Transition(this);
                trans.Setup(buffer);
                _transitions.Add(trans);

                buffer.position = nextPos;
            }

            if (_transitions.Count > 0)
            {
                this.onAddedToStage.Add(__addedToStage);
                this.onRemovedFromStage.Add(__removedFromStage);
            }

            ApplyAllControllers();

            _buildingDisplayList = false;
            underConstruct = false;

            BuildNativeDisplayList();
            SetBoundsChangedFlag();

            if (contentItem.objectType != ObjectType.Component)
                ConstructExtension(buffer);

            ConstructFromXML(null);

#if FAIRYGUI_TOLUA
            CallLua("ctor");
#endif
#if FAIRYGUI_PUERTS
            if (__onConstruct != null)
                __onConstruct();
#endif
        }

        virtual protected void ConstructExtension(ByteBuffer buffer)
        {
        }

        /// <summary>
        /// Method for extensions to override
        /// </summary>
        /// <param name="xml"></param>
        virtual public void ConstructFromXML(XML xml)
        {
        }

        public override void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_AfterAdd(buffer, beginPos);

            buffer.Seek(beginPos, 4);

            int pageController = buffer.ReadShort();
            if (pageController != -1 && scrollPane != null && scrollPane.pageMode)
                scrollPane.pageController = parent.GetControllerAt(pageController);

            int cnt = buffer.ReadShort();
            for (int i = 0; i < cnt; i++)
            {
                Controller cc = GetController(buffer.ReadS());
                string pageId = buffer.ReadS();
                if (cc != null)
                    cc.selectedPageId = pageId;
            }

            if (buffer.version >= 2)
            {
                cnt = buffer.ReadShort();
                for (int i = 0; i < cnt; i++)
                {
                    string target = buffer.ReadS();
                    int propertyId = buffer.ReadShort();
                    string value = buffer.ReadS();
                    GObject obj = this.GetChildByPath(target);
                    if (obj != null)
                    {
                        if (propertyId == 0)
                            obj.text = value;
                        else if (propertyId == 1)
                            obj.icon = value;
                    }
                }
            }
        }

        void __playSound(string soundRes, float volumeScale)
        {
            NAudioClip sound = UIPackage.GetItemAssetByURL(soundRes) as NAudioClip;
            if (sound != null && sound.nativeClip != null)
                Stage.inst.PlayOneShotSound(sound.nativeClip, volumeScale);
        }

        void __addedToStage()
        {
            int cnt = _transitions.Count;
            for (int i = 0; i < cnt; ++i)
                _transitions[i].OnOwnerAddedToStage();
        }

        void __removedFromStage()
        {
            int cnt = _transitions.Count;
            for (int i = 0; i < cnt; ++i)
                _transitions[i].OnOwnerRemovedFromStage();
        }

#if FAIRYGUI_TOLUA
        internal LuaTable _peerTable;

        public void SetLuaPeer(LuaTable peerTable)
        {
            _peerTable = peerTable;
        }

        [NoToLua]
        public bool CallLua(string funcName)
        {
            if (_peerTable != null)
            {
                LuaFunction ctor = _peerTable.GetLuaFunction(funcName);
                if (ctor != null)
                {
                    try
                    {
                        ctor.Call(this);
                    }
                    catch (Exception err)
                    {
                        Debug.LogError(err);
                    }
                    
                    ctor.Dispose();
                    return true;
                }
            }

            return false;
        }
#endif

#if FAIRYGUI_PUERTS
        public Action __onConstruct;
        public Action __onDispose;
#endif
    }
}
