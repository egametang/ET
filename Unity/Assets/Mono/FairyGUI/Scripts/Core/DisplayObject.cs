using System;
using System.Text;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class DisplayObject : EventDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        public string name;

        /// <summary>
        /// 
        /// </summary>
        public Container parent { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public GameObject gameObject { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Transform cachedTransform { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public NGraphics graphics { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public NGraphics paintingGraphics { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public event Action onPaint;

        /// <summary>
        /// 
        /// </summary>
        public GObject gOwner;

        /// <summary>
        /// 
        /// </summary>
        public uint id;

        bool _visible;
        bool _touchable;
        Vector2 _pivot;
        Vector3 _pivotOffset;
        Vector3 _rotation; //由于万向锁，单独旋转一个轴是会影响到其他轴的，所以这里需要单独保存
        Vector2 _skew;
        int _renderingOrder;
        float _alpha;
        bool _grayed;
        BlendMode _blendMode;
        IFilter _filter;
        Transform _home;
        string _cursor;
        bool _perspective;
        int _focalLength;
        Vector3 _pixelPerfectAdjustment;
        int _checkPixelPerfect;

        EventListener _onClick;
        EventListener _onRightClick;
        EventListener _onTouchBegin;
        EventListener _onTouchMove;
        EventListener _onTouchEnd;
        EventListener _onRollOver;
        EventListener _onRollOut;
        EventListener _onMouseWheel;
        EventListener _onAddedToStage;
        EventListener _onRemovedFromStage;
        EventListener _onKeyDown;
        EventListener _onClickLink;
        EventListener _onFocusIn;
        EventListener _onFocusOut;

        protected internal int _paintingMode; //1-滤镜，2-blendMode，4-transformMatrix, 8-cacheAsBitmap
        protected internal PaintingInfo _paintingInfo;
        protected Rect _contentRect;
        protected NGraphics.VertexMatrix _vertexMatrix;
        protected internal Flags _flags;
        protected internal float[] _batchingBounds;

        internal static uint _gInstanceCounter;

        internal static HideFlags hideFlags = HideFlags.None;

        public DisplayObject()
        {
            id = _gInstanceCounter++;

            _alpha = 1;
            _visible = true;
            _touchable = true;
            _blendMode = BlendMode.Normal;
            _focalLength = 2000;
            _flags |= Flags.OutlineChanged;
            if (UIConfig.makePixelPerfect)
                _flags |= Flags.PixelPerfect;
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onClick
        {
            get { return _onClick ?? (_onClick = new EventListener(this, "onClick")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onRightClick
        {
            get { return _onRightClick ?? (_onRightClick = new EventListener(this, "onRightClick")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onTouchBegin
        {
            get { return _onTouchBegin ?? (_onTouchBegin = new EventListener(this, "onTouchBegin")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onTouchMove
        {
            get { return _onTouchMove ?? (_onTouchMove = new EventListener(this, "onTouchMove")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onTouchEnd
        {
            get { return _onTouchEnd ?? (_onTouchEnd = new EventListener(this, "onTouchEnd")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onRollOver
        {
            get { return _onRollOver ?? (_onRollOver = new EventListener(this, "onRollOver")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onRollOut
        {
            get { return _onRollOut ?? (_onRollOut = new EventListener(this, "onRollOut")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onMouseWheel
        {
            get { return _onMouseWheel ?? (_onMouseWheel = new EventListener(this, "onMouseWheel")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onAddedToStage
        {
            get { return _onAddedToStage ?? (_onAddedToStage = new EventListener(this, "onAddedToStage")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onRemovedFromStage
        {
            get { return _onRemovedFromStage ?? (_onRemovedFromStage = new EventListener(this, "onRemovedFromStage")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onKeyDown
        {
            get { return _onKeyDown ?? (_onKeyDown = new EventListener(this, "onKeyDown")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onClickLink
        {
            get { return _onClickLink ?? (_onClickLink = new EventListener(this, "onClickLink")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onFocusIn
        {
            get { return _onFocusIn ?? (_onFocusIn = new EventListener(this, "onFocusIn")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onFocusOut
        {
            get { return _onFocusOut ?? (_onFocusOut = new EventListener(this, "onFocusOut")); }

        }

        protected void CreateGameObject(string gameObjectName)
        {
            gameObject = new GameObject(gameObjectName);
            cachedTransform = gameObject.transform;
            if (Application.isPlaying)
            {
                UnityEngine.Object.DontDestroyOnLoad(gameObject);

                DisplayObjectInfo info = gameObject.AddComponent<DisplayObjectInfo>();
                info.displayObject = this;
            }
            gameObject.hideFlags = DisplayObject.hideFlags;
            gameObject.SetActive(false);
        }

        protected void SetGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.cachedTransform = gameObject.transform;
            _rotation = cachedTransform.localEulerAngles;

            _flags |= Flags.UserGameObject;
        }

        protected void DestroyGameObject()
        {
            if ((_flags & Flags.UserGameObject) == 0 && gameObject != null)
            {
                if (Application.isPlaying)
                    GameObject.Destroy(gameObject);
                else
                    GameObject.DestroyImmediate(gameObject);
                gameObject = null;
                cachedTransform = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool grayed
        {
            get { return _grayed; }
            set { _grayed = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    _flags |= Flags.OutlineChanged;
                    if (parent != null && _visible)
                    {
                        gameObject.SetActive(true);
                        InvalidateBatchingState();
                        if (this is Container)
                            ((Container)this).InvalidateBatchingState(true);
                    }
                    else
                        gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float x
        {
            get { return cachedTransform.localPosition.x; }
            set
            {
                SetPosition(value, -cachedTransform.localPosition.y, cachedTransform.localPosition.z);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float y
        {
            get { return -cachedTransform.localPosition.y; }
            set
            {
                SetPosition(cachedTransform.localPosition.x, value, cachedTransform.localPosition.z);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float z
        {
            get { return cachedTransform.localPosition.z; }
            set
            {
                SetPosition(cachedTransform.localPosition.x, -cachedTransform.localPosition.y, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 xy
        {
            get { return new Vector2(this.x, this.y); }
            set { SetPosition(value.x, value.y, cachedTransform.localPosition.z); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 position
        {
            get { return new Vector3(this.x, this.y, this.z); }
            set { SetPosition(value.x, value.y, value.z); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xv"></param>
        /// <param name="yv"></param>
        public void SetXY(float xv, float yv)
        {
            SetPosition(xv, yv, cachedTransform.localPosition.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xv"></param>
        /// <param name="yv"></param>
        /// <param name="zv"></param>
        public void SetPosition(float xv, float yv, float zv)
        {
            Vector3 v = new Vector3();
            v.x = xv;
            v.y = -yv;
            v.z = zv;
            if (v != cachedTransform.localPosition)
            {
                cachedTransform.localPosition = v;
                _flags |= Flags.OutlineChanged;
                if ((_flags & Flags.PixelPerfect) != 0)
                {
                    //总在下一帧再完成PixelPerfect，这样当物体在连续运动时，不会因为PixelPerfect而发生抖动。
                    _checkPixelPerfect = Time.frameCount;
                    _pixelPerfectAdjustment = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// If the object position is align by pixel
        /// </summary>
        public bool pixelPerfect
        {
            get { return (_flags & Flags.PixelPerfect) != 0; }
            set
            {
                if (value)
                    _flags |= Flags.PixelPerfect;
                else
                    _flags &= ~Flags.PixelPerfect;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float width
        {
            get
            {
                EnsureSizeCorrect();
                return _contentRect.width;
            }
            set
            {
                if (!Mathf.Approximately(value, _contentRect.width))
                {
                    _contentRect.width = value;
                    _flags |= Flags.WidthChanged;
                    _flags &= ~Flags.HeightChanged;
                    OnSizeChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float height
        {
            get
            {
                EnsureSizeCorrect();
                return _contentRect.height;
            }
            set
            {
                if (!Mathf.Approximately(value, _contentRect.height))
                {
                    _contentRect.height = value;
                    _flags &= ~Flags.WidthChanged;
                    _flags |= Flags.HeightChanged;
                    OnSizeChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 size
        {
            get
            {
                EnsureSizeCorrect();
                return _contentRect.size;
            }
            set
            {
                SetSize(value.x, value.y);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wv"></param>
        /// <param name="hv"></param>
        public void SetSize(float wv, float hv)
        {
            if (!Mathf.Approximately(wv, _contentRect.width))
                _flags |= Flags.WidthChanged;
            else
                _flags &= ~Flags.WidthChanged;
            if (!Mathf.Approximately(hv, _contentRect.height))
                _flags |= Flags.HeightChanged;
            else
                _flags &= ~Flags.HeightChanged;

            if ((_flags & Flags.WidthChanged) != 0 || (_flags & Flags.HeightChanged) != 0)
            {
                _contentRect.width = wv;
                _contentRect.height = hv;
                OnSizeChanged();
            }
        }

        virtual public void EnsureSizeCorrect()
        {
        }

        virtual protected void OnSizeChanged()
        {
            ApplyPivot();

            if (_paintingInfo != null)
                _paintingInfo.flag = 1;
            if (graphics != null)
                graphics.contentRect = _contentRect;
            _flags |= Flags.OutlineChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        public float scaleX
        {
            get { return cachedTransform.localScale.x; }
            set
            {
                Vector3 v = cachedTransform.localScale;
                v.x = v.z = ValidateScale(value);
                cachedTransform.localScale = v;
                _flags |= Flags.OutlineChanged;
                ApplyPivot();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float scaleY
        {
            get { return cachedTransform.localScale.y; }
            set
            {
                Vector3 v = cachedTransform.localScale;
                v.y = ValidateScale(value);
                cachedTransform.localScale = v;
                _flags |= Flags.OutlineChanged;
                ApplyPivot();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xv"></param>
        /// <param name="yv"></param>
        public void SetScale(float xv, float yv)
        {
            Vector3 v = new Vector3();
            v.x = v.z = ValidateScale(xv);
            v.y = ValidateScale(yv);
            cachedTransform.localScale = v;
            _flags |= Flags.OutlineChanged;
            ApplyPivot();
        }

        /// <summary>
        /// 在scale过小情况（极端情况=0），当使用Transform的坐标变换时，变换到世界，再从世界变换到本地，会由于精度问题造成结果错误。
        /// 这种错误会导致Batching错误，因为Batching会使用缓存的outline。
        /// 这里限制一下scale的最小值作为当前解决方案。
        /// 这个方案并不完美，因为限制了本地scale值并不能保证对世界scale不会过小。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private float ValidateScale(float value)
        {
            if (value >= 0 && value < 0.001f)
                value = 0.001f;
            else if (value < 0 && value > -0.001f)
                value = -0.001f;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 scale
        {
            get { return cachedTransform.localScale; }
            set
            {
                SetScale(value.x, value.y);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float rotation
        {
            get
            {
                //和Unity默认的旋转方向相反
                return -_rotation.z;
            }
            set
            {
                _rotation.z = -value;
                _flags |= Flags.OutlineChanged;
                if (_perspective)
                    UpdateTransformMatrix();
                else
                {
                    cachedTransform.localEulerAngles = _rotation;
                    ApplyPivot();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float rotationX
        {
            get
            {
                return _rotation.x;
            }
            set
            {
                _rotation.x = value;
                _flags |= Flags.OutlineChanged;
                if (_perspective)
                    UpdateTransformMatrix();
                else
                {
                    cachedTransform.localEulerAngles = _rotation;
                    ApplyPivot();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float rotationY
        {
            get
            {
                return _rotation.y;
            }
            set
            {
                _rotation.y = value;
                _flags |= Flags.OutlineChanged;
                if (_perspective)
                    UpdateTransformMatrix();
                else
                {
                    cachedTransform.localEulerAngles = _rotation;
                    ApplyPivot();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 skew
        {
            get { return _skew; }
            set
            {
                _skew = value;
                _flags |= Flags.OutlineChanged;

                if (!Application.isPlaying) //编辑期间不支持！！
                    return;

                UpdateTransformMatrix();
            }
        }

        /// <summary>
        /// 当对象处于ScreenSpace，也就是使用正交相机渲染时，对象虽然可以绕X轴或者Y轴旋转，但没有透视效果。设置perspective，可以模拟出透视效果。
        /// </summary>
        public bool perspective
        {
            get
            {
                return _perspective;
            }
            set
            {
                if (_perspective != value)
                {
                    _perspective = value;
                    if (_perspective)//屏蔽Unity自身的旋转变换
                        cachedTransform.localEulerAngles = Vector3.zero;
                    else
                        cachedTransform.localEulerAngles = _rotation;

                    ApplyPivot();
                    UpdateTransformMatrix();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int focalLength
        {
            get { return _focalLength; }
            set
            {
                if (value <= 0)
                    value = 1;

                _focalLength = value;
                if (_vertexMatrix != null)
                    UpdateTransformMatrix();
            }
        }

        void UpdateTransformMatrix()
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            if (_skew.x != 0 || _skew.y != 0)
                ToolSet.SkewMatrix(ref matrix, _skew.x, _skew.y);
            if (_perspective)
                matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(_rotation), Vector3.one);
            if (matrix.isIdentity)
                _vertexMatrix = null;
            else if (_vertexMatrix == null)
                _vertexMatrix = new NGraphics.VertexMatrix();

            //组件的transformMatrix是通过paintingMode实现的，因为全部通过矩阵变换的话，和unity自身的变换混杂在一起，无力理清。
            if (_vertexMatrix != null)
            {
                _vertexMatrix.matrix = matrix;
                _vertexMatrix.cameraPos = new Vector3(_pivot.x * _contentRect.width, -_pivot.y * _contentRect.height, _focalLength);

                if (graphics == null)
                    EnterPaintingMode(4, null);
            }
            else
            {
                if (graphics == null)
                    LeavePaintingMode(4);
            }

            if (_paintingMode > 0)
            {
                paintingGraphics.vertexMatrix = _vertexMatrix;
                _paintingInfo.flag = 1;
            }
            else if (graphics != null)
                graphics.vertexMatrix = _vertexMatrix;

            _flags |= Flags.OutlineChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 pivot
        {
            get { return _pivot; }
            set
            {
                Vector3 deltaPivot = new Vector2((value.x - _pivot.x) * _contentRect.width, (_pivot.y - value.y) * _contentRect.height);
                Vector3 oldOffset = _pivotOffset;

                _pivot = value;
                UpdatePivotOffset();
                Vector3 v = cachedTransform.localPosition;
                v += oldOffset - _pivotOffset + deltaPivot;
                cachedTransform.localPosition = v;
                _flags |= Flags.OutlineChanged;
            }
        }

        void UpdatePivotOffset()
        {
            float px = _pivot.x * _contentRect.width;
            float py = _pivot.y * _contentRect.height;

            //注意这里不用处理skew，因为在顶点变换里有对pivot的处理
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, cachedTransform.localRotation, cachedTransform.localScale);
            _pivotOffset = matrix.MultiplyPoint(new Vector3(px, -py, 0));

            if (_vertexMatrix != null)
                _vertexMatrix.cameraPos = new Vector3(_pivot.x * _contentRect.width, -_pivot.y * _contentRect.height, _focalLength);
        }

        void ApplyPivot()
        {
            if (_pivot.x != 0 || _pivot.y != 0)
            {
                Vector3 oldOffset = _pivotOffset;

                UpdatePivotOffset();
                Vector3 v = cachedTransform.localPosition;

                if ((_flags & Flags.PixelPerfect) != 0)
                {
                    v -= _pixelPerfectAdjustment;
                    _checkPixelPerfect = Time.frameCount;
                    _pixelPerfectAdjustment = Vector3.zero;
                }

                v += oldOffset - _pivotOffset;
                cachedTransform.localPosition = v;
                _flags |= Flags.OutlineChanged;
            }
        }

        /// <summary>
        /// This is the pivot position
        /// </summary>
        public Vector3 location
        {
            get
            {
                Vector3 pos = this.position;
                pos.x += _pivotOffset.x;
                pos.y -= _pivotOffset.y;
                pos.z += _pivotOffset.z;
                return pos;
            }

            set
            {
                this.SetPosition(value.x - _pivotOffset.x, value.y + _pivotOffset.y, value.z - _pivotOffset.z);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        virtual public Material material
        {
            get
            {
                if (graphics != null)
                    return graphics.material;
                else
                    return null;
            }
            set
            {
                if (graphics != null)
                    graphics.material = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        virtual public string shader
        {
            get
            {
                if (graphics != null)
                    return graphics.shader;
                else
                    return null;
            }
            set
            {
                if (graphics != null)
                    graphics.shader = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        virtual public int renderingOrder
        {
            get
            {
                return _renderingOrder;
            }
            set
            {
                if ((_flags & Flags.GameObjectDisposed) != 0)
                {
                    DisplayDisposedWarning();
                    return;
                }

                _renderingOrder = value;
                if (graphics != null)
                    graphics.sortingOrder = value;
                if (_paintingMode > 0)
                    paintingGraphics.sortingOrder = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int layer
        {
            get
            {
                if (_paintingMode > 0)
                    return paintingGraphics.gameObject.layer;
                else
                    return gameObject.layer;
            }
            set
            {
                SetLayer(value, false);
            }
        }

        /// <summary>
        /// If the object can be focused?
        /// </summary>
        public bool focusable
        {
            get { return (_flags & Flags.NotFocusable) == 0; }
            set
            {
                if (value)
                    _flags &= ~Flags.NotFocusable;
                else
                    _flags |= Flags.NotFocusable;
            }
        }

        /// <summary>
        /// If the object can be navigated by TAB?
        /// </summary>
        public bool tabStop
        {
            get { return (_flags & Flags.TabStop) != 0; }
            set
            {
                if (value)
                    _flags |= Flags.TabStop;
                else
                    _flags &= ~Flags.TabStop;
            }
        }

        /// <summary>
        /// If the object focused?
        /// </summary>
        public bool focused
        {
            get
            {
                return Stage.inst.focus == this || (this is Container) && ((Container)this).IsAncestorOf(Stage.inst.focus);
            }
        }

        internal bool _AcceptTab()
        {
            if (_touchable && _visible
                && ((_flags & Flags.TabStop) != 0 || (_flags & Flags.TabStopChildren) != 0)
                && (_flags & Flags.NotFocusable) == 0)
            {
                Stage.inst.SetFocus(this, true);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string cursor
        {
            get { return _cursor; }
            set
            {
                _cursor = value;
                if (Application.isPlaying
                    && (this == Stage.inst.touchTarget || (this is Container) && ((Container)this).IsAncestorOf(Stage.inst.touchTarget)))
                {
                    Stage.inst._ChangeCursor(_cursor);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool isDisposed
        {
            get { return (_flags & Flags.Disposed) != 0 || gameObject == null; }
        }

        internal void InternalSetParent(Container value)
        {
            if (parent != value)
            {
                if (value == null && (parent._flags & Flags.Disposed) != 0)
                    parent = value;
                else
                {
                    parent = value;
                    UpdateHierarchy();
                }
                _flags |= Flags.OutlineChanged;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Container topmost
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject.parent != null)
                    currentObject = currentObject.parent;
                return currentObject as Container;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Stage stage
        {
            get
            {
                return topmost as Stage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Container worldSpaceContainer
        {
            get
            {
                Container wsc = null;
                DisplayObject currentObject = this;
                while (currentObject.parent != null)
                {
                    if ((currentObject is Container) && ((Container)currentObject).renderMode == RenderMode.WorldSpace)
                    {
                        wsc = (Container)currentObject;
                        break;
                    }
                    currentObject = currentObject.parent;
                }

                return wsc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool touchable
        {
            get { return _touchable; }
            set
            {
                if (_touchable != value)
                {
                    _touchable = value;
                    if (this is Container)
                    {
                        ColliderHitTest hitArea = ((Container)this).hitArea as ColliderHitTest;
                        if (hitArea != null)
                            hitArea.collider.enabled = value;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool touchDisabled
        {
            get { return (_flags & Flags.TouchDisabled) != 0; }
        }

        /// <summary>
        /// 进入绘画模式，整个对象将画到一张RenderTexture上，然后这种贴图将代替原有的显示内容。
        /// 可以在onPaint回调里对这张纹理进行进一步操作，实现特殊效果。
        /// </summary>
        public void EnterPaintingMode()
        {
            EnterPaintingMode(16384, null, 1);
        }

        /// <summary>
        /// 进入绘画模式，整个对象将画到一张RenderTexture上，然后这种贴图将代替原有的显示内容。
        /// 可以在onPaint回调里对这张纹理进行进一步操作，实现特殊效果。
        /// 可能有多个地方要求进入绘画模式，这里用requestorId加以区别，取值是1、2、4、8、16以此类推。1024内内部保留。用户自定义的id从1024开始。
        /// </summary>
        /// <param name="requestId">请求者id</param>
        /// <param name="extend">纹理四周的留空。如果特殊处理后的内容大于原内容，那么这里的设置可以使纹理扩大。</param>
        public void EnterPaintingMode(int requestorId, Margin? extend)
        {
            EnterPaintingMode(requestorId, extend, 1);
        }

        /// <summary>
        /// 进入绘画模式，整个对象将画到一张RenderTexture上，然后这种贴图将代替原有的显示内容。
        /// 可以在onPaint回调里对这张纹理进行进一步操作，实现特殊效果。
        /// 可能有多个地方要求进入绘画模式，这里用requestorId加以区别，取值是1、2、4、8、16以此类推。1024内内部保留。用户自定义的id从1024开始。
        /// </summary>
        /// <param name="requestorId">请求者id</param>
        /// <param name="extend">扩展纹理。如果特殊处理后的内容大于原内容，那么这里的设置可以使纹理扩大。</param>
        /// <param name="scale">附加一个缩放系数</param>
        public void EnterPaintingMode(int requestorId, Margin? extend, float scale)
        {
            bool first = _paintingMode == 0;
            _paintingMode |= requestorId;
            if (first)
            {
                if (_paintingInfo == null)
                {
                    _paintingInfo = new PaintingInfo()
                    {
                        captureDelegate = Capture,
                        scale = 1
                    };
                }

                if (paintingGraphics == null)
                {
                    if (graphics == null)
                        paintingGraphics = new NGraphics(this.gameObject);
                    else
                    {
                        GameObject go = new GameObject(this.gameObject.name + " (Painter)");
                        go.layer = this.gameObject.layer;
                        go.transform.SetParent(cachedTransform, false);
                        go.hideFlags = DisplayObject.hideFlags;
                        paintingGraphics = new NGraphics(go);
                    }
                }
                else
                    paintingGraphics.enabled = true;
                paintingGraphics.vertexMatrix = null;

                if (this is Container)
                {
                    ((Container)this).SetChildrenLayer(CaptureCamera.hiddenLayer);
                    ((Container)this).UpdateBatchingFlags();
                }
                else
                    this.InvalidateBatchingState();

                if (graphics != null)
                    this.gameObject.layer = CaptureCamera.hiddenLayer;
            }
            if (extend != null)
                _paintingInfo.extend = (Margin)extend;
            _paintingInfo.scale = scale;
            _paintingInfo.flag = 1;
        }

        /// <summary>
        /// 离开绘画模式
        /// </summary>
        /// <param name="requestId"></param>
        public void LeavePaintingMode(int requestorId)
        {
            if (_paintingMode == 0 || (_flags & Flags.Disposed) != 0)
                return;

            _paintingMode ^= requestorId;
            if (_paintingMode == 0)
            {
                paintingGraphics.enabled = false;

                if (this is Container)
                {
                    ((Container)this).SetChildrenLayer(this.layer);
                    ((Container)this).UpdateBatchingFlags();
                }
                else
                    this.InvalidateBatchingState();

                if (graphics != null)
                    this.gameObject.layer = paintingGraphics.gameObject.layer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool paintingMode
        {
            get { return _paintingMode > 0; }
        }

        /// <summary>
        /// 将整个显示对象（如果是容器，则容器包含的整个显示列表）静态化，所有内容被缓冲到一张纹理上。
        /// DC将保持为1。CPU消耗将降到最低。但对象的任何变化不会更新。
        /// 当cacheAsBitmap已经为true时，再次调用cacheAsBitmap=true将会刷新对象一次。
        /// </summary>
        public bool cacheAsBitmap
        {
            get { return (_flags & Flags.CacheAsBitmap) != 0; }
            set
            {
                if (value)
                {
                    _flags |= Flags.CacheAsBitmap;
                    EnterPaintingMode(8, null, UIContentScaler.scaleFactor);
                }
                else
                {
                    _flags &= ~Flags.CacheAsBitmap;
                    LeavePaintingMode(8);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extend"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public Texture2D GetScreenShot(Margin? extend, float scale)
        {
            EnterPaintingMode(8, null, scale);
            UpdatePainting();
            Capture();

            Texture2D output;
            if (paintingGraphics.texture == null)
                output = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            else
            {
                RenderTexture rt = (RenderTexture)paintingGraphics.texture.nativeTexture;
                output = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
                RenderTexture old = RenderTexture.active;
                RenderTexture.active = rt;
                output.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                output.Apply();
                RenderTexture.active = old;
            }

            LeavePaintingMode(8);

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        public IFilter filter
        {
            get
            {
                return _filter;
            }

            set
            {
                if (!Application.isPlaying) //编辑期间不支持！！
                    return;

                if (value == _filter)
                    return;

                if (_filter != null)
                    _filter.Dispose();

                if (value != null && value.target != null)
                    value.target.filter = null;

                _filter = value;
                if (_filter != null)
                    _filter.target = this;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public BlendMode blendMode
        {
            get { return _blendMode; }
            set
            {
                _blendMode = value;
                InvalidateBatchingState();

                if (graphics == null)
                {
                    if (_blendMode != BlendMode.Normal)
                    {
                        if (!Application.isPlaying) //Not supported in edit mode！
                            return;

                        EnterPaintingMode(2, null);
                        paintingGraphics.blendMode = _blendMode;
                    }
                    else
                        LeavePaintingMode(2);
                }
                else
                    graphics.blendMode = _blendMode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetSpace"></param>
        /// <returns></returns>
        virtual public Rect GetBounds(DisplayObject targetSpace)
        {
            EnsureSizeCorrect();

            if (targetSpace == this) // optimization
            {
                return _contentRect;
            }
            else if (targetSpace == parent && _rotation.z == 0)
            {
                return new Rect(cachedTransform.localPosition.x, -cachedTransform.localPosition.y,
                    _contentRect.width * cachedTransform.localScale.x, _contentRect.height * cachedTransform.localScale.y);
            }
            else
                return TransformRect(_contentRect, targetSpace);
        }

        internal DisplayObject InternalHitTest()
        {
            if (_visible && (!HitTestContext.forTouch || _touchable))
                return HitTest();
            else
                return null;
        }

        internal DisplayObject InternalHitTestMask()
        {
            if (_visible)
                return HitTest();
            else
                return null;
        }

        virtual protected DisplayObject HitTest()
        {
            Rect rect = GetBounds(this);
            if (rect.width == 0 || rect.height == 0)
                return null;

            Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);
            if (rect.Contains(localPoint))
                return this;
            else
                return null;
        }

        /// <summary>
        /// 将舞台坐标转换为本地坐标
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector2 GlobalToLocal(Vector2 point)
        {
            Container wsc = this.worldSpaceContainer;

            if (wsc != null)//I am in a world space
            {
                Camera cam = wsc.GetRenderCamera();
                Vector3 worldPoint;
                Vector3 direction;
                Vector3 screenPoint = new Vector3();
                screenPoint.x = point.x;
                screenPoint.y = Screen.height - point.y;

                if (wsc.hitArea is MeshColliderHitTest)
                {
                    Ray ray = cam.ScreenPointToRay(screenPoint);
                    RaycastHit hit;
                    if (((MeshColliderHitTest)wsc.hitArea).collider.Raycast(ray, out hit, 100))
                    {
                        point = new Vector2(hit.textureCoord.x * _contentRect.width, (1 - hit.textureCoord.y) * _contentRect.height);
                        worldPoint = Stage.inst.cachedTransform.TransformPoint(point.x, -point.y, 0);
                        direction = Vector3.back;
                    }
                    else //当射线没有击中模型时，无法确定本地坐标
                        return new Vector2(float.NaN, float.NaN);
                }
                else
                {
                    screenPoint.z = cam.WorldToScreenPoint(this.cachedTransform.position).z;
                    worldPoint = cam.ScreenToWorldPoint(screenPoint);
                    Ray ray = cam.ScreenPointToRay(screenPoint);
                    direction = Vector3.zero - ray.direction;
                }

                return this.WorldToLocal(worldPoint, direction);
            }
            else //I am in stage space
            {
                Vector3 worldPoint = Stage.inst.cachedTransform.TransformPoint(point.x, -point.y, 0);
                return this.WorldToLocal(worldPoint, Vector3.back);
            }
        }

        /// <summary>
        /// 将本地坐标转换为舞台坐标
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector2 LocalToGlobal(Vector2 point)
        {
            Container wsc = this.worldSpaceContainer;

            Vector3 worldPoint = this.cachedTransform.TransformPoint(point.x, -point.y, 0);
            if (wsc != null)
            {
                if (wsc.hitArea is MeshColliderHitTest) //Not supported for UIPainter, use TransfromPoint instead.
                    return new Vector2(float.NaN, float.NaN);

                Vector3 screePoint = wsc.GetRenderCamera().WorldToScreenPoint(worldPoint);
                return new Vector2(screePoint.x, Stage.inst.size.y - screePoint.y);
            }
            else
            {
                point = Stage.inst.cachedTransform.InverseTransformPoint(worldPoint);
                point.y = -point.y;
                return point;
            }
        }

        /// <summary>
        /// 转换世界坐标点到等效的本地xy平面的点。等效的意思是他们在屏幕方向看到的位置一样。
        /// 返回的点是在对象的本地坐标空间，且z=0
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 WorldToLocal(Vector3 worldPoint, Vector3 direction)
        {
            Vector3 localPoint = this.cachedTransform.InverseTransformPoint(worldPoint);
            if (localPoint.z != 0) //如果对象绕x轴或y轴旋转过，或者对象是在透视相机，那么z值可能不为0，
            {
                //将世界坐标的摄影机方向在本地空间上投射，求出与xy平面的交点
                direction = this.cachedTransform.InverseTransformDirection(direction);
                float distOnLine = Vector3.Dot(Vector3.zero - localPoint, Vector3.forward) / Vector3.Dot(direction, Vector3.forward);
                if (float.IsInfinity(distOnLine))
                    return Vector2.zero;

                localPoint = localPoint + direction * distOnLine;
            }
            else if (_vertexMatrix != null)
            {
                Vector3 center = _vertexMatrix.cameraPos;
                center.z = 0;
                center -= _vertexMatrix.matrix.MultiplyPoint(center);

                Matrix4x4 mm = _vertexMatrix.matrix.inverse;

                localPoint -= center;
                localPoint = mm.MultiplyPoint(localPoint);

                Vector3 camPos = mm.MultiplyPoint(_vertexMatrix.cameraPos);
                Vector3 vec = localPoint - camPos;
                float lambda = -camPos.z / vec.z;
                localPoint = camPos + lambda * vec;
                localPoint.z = 0;
            }
            localPoint.y = -localPoint.y;

            return localPoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public Vector3 LocalToWorld(Vector3 localPoint)
        {
            localPoint.y = -localPoint.y;
            if (_vertexMatrix != null)
            {
                Vector3 center = _vertexMatrix.cameraPos;
                center.z = 0;
                center -= _vertexMatrix.matrix.MultiplyPoint(center);

                localPoint = _vertexMatrix.matrix.MultiplyPoint(localPoint);
                localPoint += center;

                Vector3 camPos = _vertexMatrix.cameraPos;
                Vector3 vec = localPoint - camPos;
                float lambda = -camPos.z / vec.z;
                localPoint = camPos + lambda * vec;
                localPoint.z = 0;
            }

            return this.cachedTransform.TransformPoint(localPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="targetSpace">null if to world space</param>
        /// <returns></returns>
        public Vector2 TransformPoint(Vector2 point, DisplayObject targetSpace)
        {
            if (targetSpace == this)
                return point;

            point = LocalToWorld(point);
            if (targetSpace != null)
                point = targetSpace.WorldToLocal(point, Vector3.back);

            return point;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="targetSpace">null if to world space</param>
        /// <returns></returns>
        public Rect TransformRect(Rect rect, DisplayObject targetSpace)
        {
            if (targetSpace == this)
                return rect;

            if (targetSpace == parent && _rotation.z == 0) // optimization
            {
                Vector3 vec = cachedTransform.localScale;
                return new Rect((this.x + rect.x) * vec.x, (this.y + rect.y) * vec.y,
                    rect.width * vec.x, rect.height * vec.y);
            }
            else
            {
                Vector4 vec4 = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

                TransformRectPoint(rect.xMin, rect.yMin, targetSpace, ref vec4);
                TransformRectPoint(rect.xMax, rect.yMin, targetSpace, ref vec4);
                TransformRectPoint(rect.xMin, rect.yMax, targetSpace, ref vec4);
                TransformRectPoint(rect.xMax, rect.yMax, targetSpace, ref vec4);

                return Rect.MinMaxRect(vec4.x, vec4.y, vec4.z, vec4.w);
            }
        }

        protected void TransformRectPoint(float px, float py, DisplayObject targetSpace, ref Vector4 vec4)
        {
            Vector2 v = TransformPoint(new Vector2(px, py), targetSpace);

            if (vec4.x > v.x) vec4.x = v.x;
            if (vec4.z < v.x) vec4.z = v.x;
            if (vec4.y > v.y) vec4.y = v.y;
            if (vec4.w < v.y) vec4.w = v.y;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveFromParent()
        {
            if (parent != null)
                parent.RemoveChild(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InvalidateBatchingState()
        {
            if (parent != null)
                parent.InvalidateBatchingState(true);
        }

        virtual public void Update(UpdateContext context)
        {
            if (_checkPixelPerfect != 0)
            {
                if (_rotation == Vector3.zero)
                {
                    Vector3 v = cachedTransform.localPosition;
                    v.x = Mathf.Round(v.x);
                    v.y = Mathf.Round(v.y);
                    _pixelPerfectAdjustment = v - cachedTransform.localPosition;
                    if (_pixelPerfectAdjustment != Vector3.zero)
                        cachedTransform.localPosition = v;
                }
                _checkPixelPerfect = 0;
            }

            if (graphics != null)
                graphics.Update(context, context.alpha * _alpha, context.grayed | _grayed);

            if (_paintingMode != 0)
            {
                UpdatePainting();

                //如果是容器，Capture要等到Container.Update的最后执行，因为容器中可能也有需要Capture的内容，要等他们完成后再进行容器的Capture。
                if (!(this is Container))
                {
                    if ((_flags & Flags.CacheAsBitmap) == 0 || _paintingInfo.flag != 2)
                        UpdateContext.OnEnd += _paintingInfo.captureDelegate;
                }

                paintingGraphics.Update(context, 1, false);
            }

            if (_filter != null)
                _filter.Update();

            Stats.ObjectCount++;
        }

        void UpdatePainting()
        {
            NTexture paintingTexture = paintingGraphics.texture;
            if (paintingTexture != null && paintingTexture.disposed) //Texture可能已被Stage.MonitorTexture销毁
            {
                paintingTexture = null;
                _paintingInfo.flag = 1;
            }

            if (_paintingInfo.flag == 1)
            {
                _paintingInfo.flag = 0;

                //从优化考虑，决定使用绘画模式的容器都需要明确指定大小，而不是自动计算包围。这在UI使用上并没有问题，因为组件总是有固定大小的
                Margin extend = _paintingInfo.extend;
                paintingGraphics.contentRect = new Rect(-extend.left, -extend.top, _contentRect.width + extend.left + extend.right, _contentRect.height + extend.top + extend.bottom);
                int textureWidth = Mathf.RoundToInt(paintingGraphics.contentRect.width * _paintingInfo.scale);
                int textureHeight = Mathf.RoundToInt(paintingGraphics.contentRect.height * _paintingInfo.scale);
                if (paintingTexture == null || paintingTexture.width != textureWidth || paintingTexture.height != textureHeight)
                {
                    if (paintingTexture != null)
                        paintingTexture.Dispose();
                    if (textureWidth > 0 && textureHeight > 0)
                    {
                        paintingTexture = new NTexture(CaptureCamera.CreateRenderTexture(textureWidth, textureHeight, UIConfig.depthSupportForPaintingMode));
                        Stage.inst.MonitorTexture(paintingTexture);
                    }
                    else
                        paintingTexture = null;
                    paintingGraphics.texture = paintingTexture;
                }
            }

            if (paintingTexture != null)
                paintingTexture.lastActive = Time.time;
        }

        void Capture()
        {
            if (paintingGraphics.texture == null)
                return;

            Vector2 offset = new Vector2(_paintingInfo.extend.left, _paintingInfo.extend.top);
            CaptureCamera.Capture(this, (RenderTexture)paintingGraphics.texture.nativeTexture, paintingGraphics.contentRect.height, offset);

            _paintingInfo.flag = 2; //2表示已完成一次Capture
            if (onPaint != null)
                onPaint();
        }

        /// <summary>
        /// 为对象设置一个默认的父Transform。当对象不在显示列表里时，它的GameObject挂到哪里。
        /// </summary>
        public Transform home
        {
            get { return _home; }
            set
            {
                _home = value;
                if (value != null && cachedTransform.parent == null)
                    cachedTransform.SetParent(value, false);
            }
        }

        void UpdateHierarchy()
        {
            if ((_flags & Flags.GameObjectDisposed) != 0)
                return;

            if ((_flags & Flags.UserGameObject) != 0)
            {
                //we dont change transform parent of this object
                if (gameObject != null)
                {
                    if (parent != null && visible)
                        gameObject.SetActive(true);
                    else
                        gameObject.SetActive(false);
                }
            }
            else if (parent != null)
            {
                cachedTransform.SetParent(parent.cachedTransform, false);

                if (_visible)
                    gameObject.SetActive(true);

                int layerValue = parent.gameObject.layer;
                if (parent._paintingMode != 0)
                    layerValue = CaptureCamera.hiddenLayer;
                SetLayer(layerValue, true);
            }
            else if ((_flags & Flags.Disposed) == 0 && this.gameObject != null && !StageEngine.beingQuit)
            {
                if (Application.isPlaying)
                {
                    if (gOwner == null || gOwner.parent == null)//如果gOwner还有parent的话，说明只是暂时的隐藏
                    {
                        cachedTransform.SetParent(_home, false);
                        if (_home == null)
                            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
                    }
                }

                gameObject.SetActive(false);
            }
        }

        virtual protected bool SetLayer(int value, bool fromParent)
        {
            if ((_flags & Flags.LayerSet) != 0) //setted
            {
                if (fromParent)
                    return false;
            }
            else if ((_flags & Flags.LayerFromParent) != 0) //inherit from parent
            {
                if (!fromParent)
                    _flags |= Flags.LayerSet;
            }
            else
            {
                if (fromParent)
                    _flags |= Flags.LayerFromParent;
                else
                    _flags |= Flags.LayerSet;
            }

            if (_paintingMode > 0)
                paintingGraphics.gameObject.layer = value;
            else if (gameObject.layer != value)
            {
                gameObject.layer = value;
                if ((this is Container))
                {
                    int cnt = ((Container)this).numChildren;
                    for (int i = 0; i < cnt; i++)
                    {
                        DisplayObject child = ((Container)this).GetChildAt(i);
                        child.SetLayer(value, true);
                    }
                }
            }

            return true;
        }

        internal void _SetLayerDirect(int value)
        {
            if (_paintingMode > 0)
                paintingGraphics.gameObject.layer = value;
            else
                gameObject.layer = value;
        }

        virtual public void Dispose()
        {
            if ((_flags & Flags.Disposed) != 0)
                return;

            _flags |= Flags.Disposed;
            RemoveFromParent();
            RemoveEventListeners();
            if (graphics != null)
                graphics.Dispose();
            if (_filter != null)
                _filter.Dispose();
            if (paintingGraphics != null)
            {
                if (paintingGraphics.texture != null)
                    paintingGraphics.texture.Dispose();

                paintingGraphics.Dispose();
                if (paintingGraphics.gameObject != this.gameObject)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(paintingGraphics.gameObject);
                    else
                        UnityEngine.Object.DestroyImmediate(paintingGraphics.gameObject);
                }
            }
            DestroyGameObject();
        }

        internal void DisplayDisposedWarning()
        {
            if ((_flags & Flags.DisposedWarning) == 0)
            {
                _flags |= Flags.DisposedWarning;

                StringBuilder sb = new StringBuilder();
                sb.Append("DisplayObject is still in use but GameObject was disposed. (");
                if (gOwner != null)
                {
                    sb.Append("type=").Append(gOwner.GetType().Name).Append(", x=").Append(gOwner.x).Append(", y=").Append(gOwner.y).Append(", name=").Append(gOwner.name);
                    if (gOwner.packageItem != null)
                        sb.Append(", res=" + gOwner.packageItem.name);
                }
                else
                {
                    sb.Append("id=").Append(id).Append(", type=").Append(this.GetType().Name).Append(", name=").Append(name);
                }
                sb.Append(")");
                Debug.LogError(sb.ToString());
            }
        }

        protected internal class PaintingInfo
        {
            public Action captureDelegate; //缓存这个delegate，可以防止Capture状态下每帧104B的GC
            public Margin extend;
            public float scale;
            public int flag;
        }

        [Flags]
        protected internal enum Flags
        {
            Disposed = 1,
            UserGameObject = 2,
            TouchDisabled = 4,
            OutlineChanged = 8,
            UpdatingSize = 0x10,
            WidthChanged = 0x20,
            HeightChanged = 0x40,
            PixelPerfect = 0x80,
            LayerSet = 0x100,
            LayerFromParent = 0x200,
            NotFocusable = 0x400,
            TabStop = 0x800,
            TabStopChildren = 0x1000,
            FairyBatching = 0x2000,
            BatchingRequested = 0x4000,
            BatchingRoot = 0x8000,
            SkipBatching = 0x10000,
            CacheAsBitmap = 0x20000,
            GameObjectDisposed = 0x40000,
            DisposedWarning = 0x80000
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DisplayObjectInfo : MonoBehaviour
    {
        /// <summary>
        /// 
        /// /// </summary>
        [System.NonSerialized]
        public DisplayObject displayObject;

        private void OnDestroy()
        {
            if (displayObject != null)
                displayObject._flags |= DisplayObject.Flags.GameObjectDisposed;
        }
    }
}
