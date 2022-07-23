using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class Container : DisplayObject
    {
        /// <summary>
        /// 
        /// </summary>
        public RenderMode renderMode;

        /// <summary>
        /// 
        /// </summary>
        public Camera renderCamera;

        /// <summary>
        /// 
        /// </summary>
        public bool opaque;

        /// <summary>
        /// 
        /// </summary>
        public Vector4? clipSoftness;

        /// <summary>
        /// 
        /// </summary>
        public IHitTest hitArea;

        /// <summary>
        /// 
        /// </summary>
        public bool touchChildren;

        /// <summary>
        /// 
        /// </summary>
        public event Action onUpdate;

        /// <summary>
        /// 
        /// </summary>
        public bool reversedMask;

        List<DisplayObject> _children;
        DisplayObject _mask;
        Rect? _clipRect;
        List<DisplayObject> _descendants;

        internal int _panelOrder;
        internal DisplayObject _lastFocus;

        /// <summary>
        /// 
        /// </summary>
        public Container()
            : base()
        {
            CreateGameObject("Container");
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObjectName"></param>
        public Container(string gameObjectName)
            : base()
        {
            CreateGameObject(gameObjectName);
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attachTarget"></param>
        public Container(GameObject attachTarget)
            : base()
        {
            SetGameObject(attachTarget);
            Init();
        }

        void Init()
        {
            _children = new List<DisplayObject>();
            touchChildren = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public int numChildren
        {
            get { return _children.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public DisplayObject AddChild(DisplayObject child)
        {
            AddChildAt(child, _children.Count);
            return child;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public DisplayObject AddChildAt(DisplayObject child, int index)
        {
            int count = _children.Count;
            if (index >= 0 && index <= count)
            {
                if (child.parent == this)
                {
                    SetChildIndex(child, index);
                }
                else
                {
                    child.RemoveFromParent();
                    if (index == count)
                        _children.Add(child);
                    else
                        _children.Insert(index, child);
                    child.InternalSetParent(this);

                    if (stage != null)
                    {
                        if (child is Container)
                            child.BroadcastEvent("onAddedToStage", null);
                        else
                            child.DispatchEvent("onAddedToStage", null);
                    }

                    InvalidateBatchingState(true);
                }
                return child;
            }
            else
            {
                throw new Exception("Invalid child index");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public bool Contains(DisplayObject child)
        {
            return _children.Contains(child);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DisplayObject GetChildAt(int index)
        {
            return _children[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DisplayObject GetChild(string name)
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                if (_children[i].name == name)
                    return _children[i];
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DisplayObject[] GetChildren()
        {
            return _children.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public int GetChildIndex(DisplayObject child)
        {
            return _children.IndexOf(child);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public DisplayObject RemoveChild(DisplayObject child)
        {
            return RemoveChild(child, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="dispose"></param>
        /// <returns></returns>
        public DisplayObject RemoveChild(DisplayObject child, bool dispose)
        {
            if (child.parent != this)
                throw new Exception("obj is not a child");

            int i = _children.IndexOf(child);
            if (i >= 0)
                return RemoveChildAt(i, dispose);
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DisplayObject RemoveChildAt(int index)
        {
            return RemoveChildAt(index, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dispose"></param>
        /// <returns></returns>
        public DisplayObject RemoveChildAt(int index, bool dispose)
        {
            if (index >= 0 && index < _children.Count)
            {
                DisplayObject child = _children[index];

                if (stage != null && (child._flags & Flags.Disposed) == 0)
                {
                    if (child is Container)
                    {
                        child.BroadcastEvent("onRemovedFromStage", null);
                        if (child == Stage.inst.focus || ((Container)child).IsAncestorOf(Stage.inst.focus))
                            Stage.inst._OnFocusRemoving(this);
                    }
                    else
                    {
                        child.DispatchEvent("onRemovedFromStage", null);
                        if (child == Stage.inst.focus)
                            Stage.inst._OnFocusRemoving(this);
                    }
                }
                _children.Remove(child);
                InvalidateBatchingState(true);
                if (!dispose)
                    child.InternalSetParent(null);
                else
                    child.Dispose();

                return child;
            }
            else
                throw new Exception("Invalid child index");
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveChildren()
        {
            RemoveChildren(0, int.MaxValue, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="dispose"></param>
        public void RemoveChildren(int beginIndex, int endIndex, bool dispose)
        {
            if (endIndex < 0 || endIndex >= numChildren)
                endIndex = numChildren - 1;

            for (int i = beginIndex; i <= endIndex; ++i)
                RemoveChildAt(beginIndex, dispose);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="index"></param>
        public void SetChildIndex(DisplayObject child, int index)
        {
            int oldIndex = _children.IndexOf(child);
            if (oldIndex == index) return;
            if (oldIndex == -1) throw new ArgumentException("Not a child of this container");
            _children.RemoveAt(oldIndex);
            if (index >= _children.Count)
                _children.Add(child);
            else
                _children.Insert(index, child);
            InvalidateBatchingState(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child1"></param>
        /// <param name="child2"></param>
        public void SwapChildren(DisplayObject child1, DisplayObject child2)
        {
            int index1 = _children.IndexOf(child1);
            int index2 = _children.IndexOf(child2);
            if (index1 == -1 || index2 == -1)
                throw new Exception("Not a child of this container");
            SwapChildrenAt(index1, index2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        public void SwapChildrenAt(int index1, int index2)
        {
            DisplayObject obj1 = _children[index1];
            DisplayObject obj2 = _children[index2];
            _children[index1] = obj2;
            _children[index2] = obj1;
            InvalidateBatchingState(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="objs"></param>
        public void ChangeChildrenOrder(IList<int> indice, IList<DisplayObject> objs)
        {
            int cnt = objs.Count;
            for (int i = 0; i < cnt; i++)
            {
                DisplayObject obj = objs[i];
                if (obj.parent != this)
                    throw new Exception("Not a child of this container");

                _children[indice[i]] = obj;
            }
            InvalidateBatchingState(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<DisplayObject> GetDescendants(bool backward)
        {
            return new DescendantsEnumerator(this, backward);
        }

        /// <summary>
        /// 
        /// </summary>
        public Rect? clipRect
        {
            get { return _clipRect; }
            set
            {
                if (_clipRect != value)
                {
                    _clipRect = value;
                    UpdateBatchingFlags();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DisplayObject mask
        {
            get { return _mask; }
            set
            {
                if (_mask != value)
                {
                    _mask = value;
                    UpdateBatchingFlags();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateGraphics()
        {
            if (graphics == null)
            {
                graphics = new NGraphics(this.gameObject);
                graphics.texture = NTexture.Empty;
            }
        }

        public override Rect GetBounds(DisplayObject targetSpace)
        {
            if (_clipRect != null)
                return TransformRect((Rect)_clipRect, targetSpace);

            int count = _children.Count;

            Rect rect;
            if (count == 0)
            {
                Vector2 v = TransformPoint(Vector2.zero, targetSpace);
                rect = Rect.MinMaxRect(v.x, v.y, 0, 0);
            }
            else if (count == 1)
            {
                rect = _children[0].GetBounds(targetSpace);
            }
            else
            {
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;

                for (int i = 0; i < count; ++i)
                {
                    rect = _children[i].GetBounds(targetSpace);
                    minX = minX < rect.xMin ? minX : rect.xMin;
                    maxX = maxX > rect.xMax ? maxX : rect.xMax;
                    minY = minY < rect.yMin ? minY : rect.yMin;
                    maxY = maxY > rect.yMax ? maxY : rect.yMax;
                }

                rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            }

            return rect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Camera GetRenderCamera()
        {
            if (renderMode == RenderMode.ScreenSpaceOverlay)
                return StageCamera.main;
            else
            {
                Camera cam = this.renderCamera;
                if (cam == null)
                {
                    if (HitTestContext.cachedMainCamera != null)
                        cam = HitTestContext.cachedMainCamera;
                    else
                    {
                        cam = Camera.main;
                        if (cam == null)
                            cam = StageCamera.main;
                    }
                }
                return cam;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stagePoint"></param>
        /// <param name="forTouch"></param>
        /// <param name="displayIndex"></param>
        /// <returns></returns>
        public DisplayObject HitTest(Vector2 stagePoint, bool forTouch)
        {
            if (StageCamera.main == null)
            {
                if (this is Stage)
                    return this;
                else
                    return null;
            }

            HitTestContext.screenPoint = new Vector3(stagePoint.x, Screen.height - stagePoint.y, 0);
            if (Display.displays.Length > 1)
            {
                Vector3 p = Display.RelativeMouseAt(HitTestContext.screenPoint);
                if (p != Vector3.zero)
                    HitTestContext.screenPoint = p;
            }
            HitTestContext.worldPoint = StageCamera.main.ScreenToWorldPoint(HitTestContext.screenPoint);
            HitTestContext.direction = Vector3.back;
            HitTestContext.forTouch = forTouch;
            HitTestContext.camera = StageCamera.main;

            DisplayObject ret = HitTest();
            if (ret != null)
                return ret;
            else if (this is Stage)
                return this;
            else
                return null;
        }

        override protected DisplayObject HitTest()
        {
            if ((_flags & Flags.UserGameObject) != 0 && !gameObject.activeInHierarchy)
                return null;

            if (this.cachedTransform.localScale.x == 0 || this.cachedTransform.localScale.y == 0)
                return null;

            Camera savedCamera = HitTestContext.camera;
            Vector3 savedWorldPoint = HitTestContext.worldPoint;
            Vector3 savedDirection = HitTestContext.direction;
            DisplayObject target;

            if (renderMode != RenderMode.ScreenSpaceOverlay || (_flags & Flags.UserGameObject) != 0)
            {
                Camera cam = GetRenderCamera();
                if (cam.targetDisplay != HitTestContext.screenPoint.z)
                    return null;

                HitTestContext.camera = cam;
                if (renderMode == RenderMode.WorldSpace)
                {
                    Vector3 screenPoint = HitTestContext.camera.WorldToScreenPoint(this.cachedTransform.position); //only for query z value
                    screenPoint.x = HitTestContext.screenPoint.x;
                    screenPoint.y = HitTestContext.screenPoint.y;

                    //获得本地z轴在世界坐标的方向
                    HitTestContext.worldPoint = HitTestContext.camera.ScreenToWorldPoint(screenPoint);
                    Ray ray = HitTestContext.camera.ScreenPointToRay(screenPoint);
                    HitTestContext.direction = Vector3.zero - ray.direction;
                }
                else if (renderMode == RenderMode.ScreenSpaceCamera)
                {
                    HitTestContext.worldPoint = HitTestContext.camera.ScreenToWorldPoint(HitTestContext.screenPoint);
                }
            }
            else
            {
                if (HitTestContext.camera.targetDisplay != HitTestContext.screenPoint.z && !(this is Stage))
                    return null;
            }

            target = HitTest_Container();

            HitTestContext.camera = savedCamera;
            HitTestContext.worldPoint = savedWorldPoint;
            HitTestContext.direction = savedDirection;

            return target;
        }

        DisplayObject HitTest_Container()
        {
            Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);
            if (_vertexMatrix != null)
                HitTestContext.worldPoint = this.cachedTransform.TransformPoint(new Vector2(localPoint.x, -localPoint.y));

            if (hitArea != null)
            {
                if (!hitArea.HitTest(_contentRect, localPoint))
                    return null;

                if (hitArea is MeshColliderHitTest)
                    localPoint = ((MeshColliderHitTest)hitArea).lastHit;
            }
            else
            {
                if (_clipRect != null && !((Rect)_clipRect).Contains(localPoint))
                    return null;
            }

            if (_mask != null)
            {
                DisplayObject tmp = _mask.InternalHitTestMask();
                if (!reversedMask && tmp == null || reversedMask && tmp != null)
                    return null;
            }

            DisplayObject target = null;
            if (touchChildren)
            {
                int count = _children.Count;
                for (int i = count - 1; i >= 0; --i) // front to back!
                {
                    DisplayObject child = _children[i];
                    if ((child._flags & Flags.GameObjectDisposed) != 0)
                    {
                        child.DisplayDisposedWarning();
                        continue;
                    }

                    if (child == _mask || (child._flags & Flags.TouchDisabled) != 0)
                        continue;

                    target = child.InternalHitTest();
                    if (target != null)
                        break;
                }
            }

            if (target == null && opaque && (hitArea != null || _contentRect.Contains(localPoint)))
                target = this;

            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsAncestorOf(DisplayObject obj)
        {
            if (obj == null)
                return false;

            Container p = obj.parent;
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
        public bool fairyBatching
        {
            get { return (_flags & Flags.FairyBatching) != 0; }
            set
            {
                bool oldValue = (_flags & Flags.FairyBatching) != 0;
                if (oldValue != value)
                {
                    if (value)
                        _flags |= Flags.FairyBatching;
                    else
                        _flags &= ~Flags.FairyBatching;
                    UpdateBatchingFlags();
                }
            }
        }

        internal void UpdateBatchingFlags()
        {
            bool oldValue = (_flags & Flags.BatchingRoot) != 0;
            bool newValue = (_flags & Flags.FairyBatching) != 0 || _clipRect != null || _mask != null || _paintingMode > 0;
            if (newValue)
                _flags |= Flags.BatchingRoot;
            else
                _flags &= ~Flags.BatchingRoot;
            if (oldValue != newValue)
            {
                if (newValue)
                    _flags |= Flags.BatchingRequested;
                else if (_descendants != null)
                    _descendants.Clear();

                InvalidateBatchingState();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childrenChanged"></param>
        public void InvalidateBatchingState(bool childrenChanged)
        {
            if (childrenChanged && (_flags & Flags.BatchingRoot) != 0)
                _flags |= Flags.BatchingRequested;
            else
            {
                Container p = this.parent;
                while (p != null)
                {
                    if ((p._flags & Flags.BatchingRoot) != 0)
                    {
                        p._flags |= Flags.BatchingRequested;
                        break;
                    }

                    p = p.parent;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetChildrenLayer(int value)
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; i++)
            {
                DisplayObject child = _children[i];
                child._SetLayerDirect(value);
                if ((child is Container) && child._paintingMode == 0)
                    ((Container)child).SetChildrenLayer(value);
            }
        }

        override public void Update(UpdateContext context)
        {
            if ((_flags & Flags.UserGameObject) != 0 && !gameObject.activeInHierarchy)
                return;

            base.Update(context);

            if (_paintingMode != 0)
            {
                if ((_flags & Flags.CacheAsBitmap) != 0 && _paintingInfo.flag == 2)
                {
                    if (onUpdate != null)
                        onUpdate();
                    return;
                }

                context.EnterPaintingMode();
            }

            if (_mask != null)
            {
                context.EnterClipping(this.id, reversedMask);
                if (_mask.graphics != null)
                    _mask.graphics._PreUpdateMask(context, _mask.id);
            }
            else if (_clipRect != null)
                context.EnterClipping(this.id, this.TransformRect((Rect)_clipRect, null), clipSoftness);

            float savedAlpha = context.alpha;
            context.alpha *= this.alpha;
            bool savedGrayed = context.grayed;
            context.grayed = context.grayed || this.grayed;

            if ((_flags & Flags.FairyBatching) != 0)
                context.batchingDepth++;

            if (context.batchingDepth > 0)
            {
                int cnt = _children.Count;
                for (int i = 0; i < cnt; i++)
                {
                    DisplayObject child = _children[i];
                    if ((child._flags & Flags.GameObjectDisposed) != 0)
                    {
                        child.DisplayDisposedWarning();
                        continue;
                    }

                    if (child.visible)
                        child.Update(context);
                }
            }
            else
            {
                if (_mask != null)
                    _mask.renderingOrder = context.renderingOrder++;

                int cnt = _children.Count;
                for (int i = 0; i < cnt; i++)
                {
                    DisplayObject child = _children[i];
                    if ((child._flags & Flags.GameObjectDisposed) != 0)
                    {
                        child.DisplayDisposedWarning();
                        continue;
                    }

                    if (child.visible)
                    {
                        if (!(child.graphics != null && child.graphics._maskFlag == 1)) //if not a mask
                            child.renderingOrder = context.renderingOrder++;

                        child.Update(context);
                    }
                }

                if (_mask != null)
                {
                    if (_mask.graphics != null)
                        _mask.graphics._SetStencilEraserOrder(context.renderingOrder++);
                }
            }

            if ((_flags & Flags.FairyBatching) != 0)
            {
                if (context.batchingDepth == 1)
                    SetRenderingOrder(context);
                context.batchingDepth--;
            }

            context.alpha = savedAlpha;
            context.grayed = savedGrayed;

            if (_clipRect != null || _mask != null)
                context.LeaveClipping();

            if (_paintingMode != 0)
            {
                context.LeavePaintingMode();
                UpdateContext.OnEnd += _paintingInfo.captureDelegate;
            }

            if (onUpdate != null)
                onUpdate();
        }

        private void SetRenderingOrder(UpdateContext context)
        {
            if ((_flags & Flags.BatchingRequested) != 0)
                DoFairyBatching();

            if (_mask != null)
                _mask.renderingOrder = context.renderingOrder++;

            int cnt = _descendants.Count;
            for (int i = 0; i < cnt; i++)
            {
                DisplayObject child = _descendants[i];
                if (!(child.graphics != null && child.graphics._maskFlag == 1))
                    child.renderingOrder = context.renderingOrder++;

                if ((child._flags & Flags.BatchingRoot) != 0)
                    ((Container)child).SetRenderingOrder(context);
            }

            if (_mask != null)
            {
                if (_mask.graphics != null)
                    _mask.graphics._SetStencilEraserOrder(context.renderingOrder++);
            }
        }

        private void DoFairyBatching()
        {
            _flags &= ~Flags.BatchingRequested;

            if (_descendants == null)
                _descendants = new List<DisplayObject>();
            else
                _descendants.Clear();
            CollectChildren(this, false);

            int cnt = _descendants.Count;

            int i, j, k, m;
            object curMat, testMat, lastMat;
            DisplayObject current, test;
            float[] bound;
            for (i = 0; i < cnt; i++)
            {
                current = _descendants[i];
                bound = current._batchingBounds;
                curMat = current.material;
                if (curMat == null || (current._flags & Flags.SkipBatching) != 0)
                    continue;

                k = -1;
                lastMat = null;
                m = i;
                for (j = i - 1; j >= 0; j--)
                {
                    test = _descendants[j];
                    if ((test._flags & Flags.SkipBatching) != 0)
                        break;

                    testMat = test.material;
                    if (testMat != null)
                    {
                        if (lastMat != testMat)
                        {
                            lastMat = testMat;
                            m = j + 1;
                        }

                        if (curMat == testMat)
                            k = m;
                    }

                    if ((bound[0] > test._batchingBounds[0] ? bound[0] : test._batchingBounds[0])
                        <= (bound[2] < test._batchingBounds[2] ? bound[2] : test._batchingBounds[2])
                        && (bound[1] > test._batchingBounds[1] ? bound[1] : test._batchingBounds[1])
                        <= (bound[3] < test._batchingBounds[3] ? bound[3] : test._batchingBounds[3]))
                    {
                        if (k == -1)
                            k = m;
                        break;
                    }
                }
                if (k != -1 && i != k)
                {
                    _descendants.RemoveAt(i);
                    _descendants.Insert(k, current);
                }
            }

            //Debug.Log("DoFairyBatching " + cnt + "," + this.cachedTransform.GetInstanceID());
        }

        private void CollectChildren(Container initiator, bool outlineChanged)
        {
            int count = _children.Count;
            for (int i = 0; i < count; i++)
            {
                DisplayObject child = _children[i];
                if (!child.visible)
                    continue;

                if (child._batchingBounds == null)
                    child._batchingBounds = new float[4];

                if (child is Container)
                {
                    Container container = (Container)child;
                    if ((container._flags & Flags.BatchingRoot) != 0)
                    {
                        initiator._descendants.Add(container);
                        if (outlineChanged || (container._flags & Flags.OutlineChanged) != 0)
                        {
                            Rect rect = container.GetBounds(initiator);
                            container._batchingBounds[0] = rect.xMin;
                            container._batchingBounds[1] = rect.yMin;
                            container._batchingBounds[2] = rect.xMax;
                            container._batchingBounds[3] = rect.yMax;
                        }
                        if ((container._flags & Flags.BatchingRequested) != 0)
                            container.DoFairyBatching();
                    }
                    else
                        container.CollectChildren(initiator, outlineChanged || (container._flags & Flags.OutlineChanged) != 0);
                }
                else if (child != initiator._mask)
                {
                    if (outlineChanged || (child._flags & Flags.OutlineChanged) != 0)
                    {
                        Rect rect = child.GetBounds(initiator);
                        child._batchingBounds[0] = rect.xMin;
                        child._batchingBounds[1] = rect.yMin;
                        child._batchingBounds[2] = rect.xMax;
                        child._batchingBounds[3] = rect.yMax;
                    }
                    initiator._descendants.Add(child);
                }

                child._flags &= ~Flags.OutlineChanged;
            }
        }

        public override void Dispose()
        {
            if ((_flags & Flags.Disposed) != 0)
                return;

            base.Dispose(); //Destroy GameObject tree first, avoid destroying each seperately;

            int numChildren = _children.Count;
            for (int i = numChildren - 1; i >= 0; --i)
            {
                DisplayObject obj = _children[i];
                obj.InternalSetParent(null); //Avoid RemoveParent call
                obj.Dispose();
            }
        }

        /// <summary>
        /// If true, when the container is focused, tab navigation is lock inside it.
        /// </summary>
        public bool tabStopChildren
        {
            get { return (_flags & Flags.TabStopChildren) != 0; }
            set
            {
                if (value)
                    _flags |= Flags.TabStopChildren;
                else
                    _flags &= ~Flags.TabStopChildren;
            }
        }

        struct DescendantsEnumerator : IEnumerator<DisplayObject>
        {
            Container _root;
            Container _com;
            DisplayObject _current;
            int _index;
            bool _forward;

            public DescendantsEnumerator(Container root, bool backward)
            {
                _root = root;
                _com = _root;
                _current = null;
                _forward = !backward;
                if (_forward)
                    _index = 0;
                else
                    _index = _com._children.Count - 1;
            }

            public DisplayObject Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_forward)
                {
                    if (_index >= _com._children.Count)
                    {
                        if (_com == _root)
                        {
                            _current = null;
                            return false;
                        }

                        _current = _com;
                        _com = _com.parent;
                        _index = _com.GetChildIndex(_current) + 1;
                        return true;
                    }
                    else
                    {
                        DisplayObject obj = _com._children[_index];
                        if (obj is Container)
                        {
                            _com = (Container)obj;
                            _index = 0;
                            return MoveNext();
                        }
                        _index++;
                        _current = obj;
                        return true;
                    }
                }
                else
                {
                    if (_index < 0)
                    {
                        if (_com == _root)
                        {
                            _current = null;
                            return false;
                        }

                        _current = _com;
                        _com = _com.parent;
                        _index = _com.GetChildIndex(_current) - 1;
                        return true;
                    }
                    else
                    {
                        DisplayObject obj = _com._children[_index];
                        if (obj is Container)
                        {
                            _com = (Container)obj;
                            _index = _com._children.Count - 1;
                            return MoveNext();
                        }
                        _index--;
                        _current = obj;
                        return true;
                    }
                }
            }

            public void Reset()
            {
                _com = _root;
                _current = null;
                _index = 0;
            }

            public void Dispose()
            {
            }
        }
    }
}
