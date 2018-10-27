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
		public EventCallback0 onPaint;

		/// <summary>
		/// 
		/// </summary>
		public GObject gOwner;

		/// <summary>
		/// 
		/// </summary>
		public uint id;

		/// <summary>
		/// 
		/// </summary>
		public EventListener onClick { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRightClick { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchBegin { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchMove { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchEnd { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRollOver { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRollOut { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onMouseWheel { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onAddedToStage { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRemovedFromStage { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onKeyDown { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onClickLink { get; private set; }

		bool _visible;
		bool _touchable;
		Vector2 _pivot;
		Vector3 _pivotOffset;
		Vector2 _skew;
		int _renderingOrder;
		float _alpha;
		bool _grayed;
		BlendMode _blendMode;
		IFilter _filter;
		Transform _home;

		bool _perspective;
		int _focalLength;
		Vector3 _rotation; //由于万向锁，单独旋转一个轴是会影响到其他轴的，所以这里需要单独保存

		protected EventCallback0 _captureDelegate; //缓存这个delegate，可以防止Capture状态下每帧104B的GC
		protected int _paintingMode; //1-滤镜，2-blendMode，4-transformMatrix, 8-cacheAsBitmap
		protected Margin _paintingMargin;
		protected int _paintingFlag;
		protected Material _paintingMaterial;
		protected bool _cacheAsBitmap;

		protected Rect _contentRect;
		protected bool _requireUpdateMesh;
		protected Matrix4x4? _transformMatrix;
		protected bool _ownsGameObject;

		internal bool _disposed;
		internal protected bool _touchDisabled;
		internal float[] _internal_bounds;
		internal protected bool _skipInFairyBatching;
		internal bool _outlineChanged;

		internal static uint _gInstanceCounter;

		public DisplayObject()
		{
			_alpha = 1;
			_visible = true;
			_touchable = true;
			id = _gInstanceCounter++;
			_blendMode = BlendMode.Normal;
			_focalLength = 2000;
			_captureDelegate = Capture;
			_outlineChanged = true;
			_internal_bounds = new float[4];

			onClick = new EventListener(this, "onClick");
			onRightClick = new EventListener(this, "onRightClick");
			onTouchBegin = new EventListener(this, "onTouchBegin");
			onTouchMove = new EventListener(this, "onTouchMove");
			onTouchEnd = new EventListener(this, "onTouchEnd");
			onRollOver = new EventListener(this, "onRollOver");
			onRollOut = new EventListener(this, "onRollOut");
			onMouseWheel = new EventListener(this, "onMouseWheel");
			onAddedToStage = new EventListener(this, "onAddedToStage");
			onRemovedFromStage = new EventListener(this, "onRemovedFromStage");
			onKeyDown = new EventListener(this, "onKeyDown");
			onClickLink = new EventListener(this, "onClickLink");
		}

		protected void CreateGameObject(string gameObjectName)
		{
			gameObject = new GameObject(gameObjectName);
			cachedTransform = gameObject.transform;
			if (Application.isPlaying)
				Object.DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = DisplayOptions.hideFlags;
			gameObject.SetActive(false);
			_ownsGameObject = true;
		}

		protected void SetGameObject(GameObject gameObject)
		{
			this.gameObject = gameObject;
			this.cachedTransform = gameObject.transform;
			_rotation = cachedTransform.localEulerAngles;
			_ownsGameObject = false;
		}

		protected void DestroyGameObject()
		{
			if (_ownsGameObject && gameObject != null)
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
					_outlineChanged = true;
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
				Vector3 v = cachedTransform.localPosition;
				v.x = value;
				cachedTransform.localPosition = v;
				_outlineChanged = true;
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
				Vector3 v = cachedTransform.localPosition;
				v.y = -value;
				cachedTransform.localPosition = v;
				_outlineChanged = true;
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
				Vector3 v = cachedTransform.localPosition;
				v.z = value;
				cachedTransform.localPosition = v;
				_outlineChanged = true;
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
			Vector3 v = cachedTransform.localPosition;
			v.x = xv;
			v.y = -yv;
			v.z = zv;
			cachedTransform.localPosition = v;
			_outlineChanged = true;
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
					OnSizeChanged(true, false);
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
					OnSizeChanged(false, true);
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
			bool wc = !Mathf.Approximately(wv, _contentRect.width);
			bool hc = !Mathf.Approximately(hv, _contentRect.height);

			if (wc || hc)
			{
				_contentRect.width = wv;
				_contentRect.height = hv;
				OnSizeChanged(wc, hc);
			}
		}

		virtual public void EnsureSizeCorrect()
		{
		}

		virtual protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			ApplyPivot();
			_paintingFlag = 1;
			if (graphics != null)
				_requireUpdateMesh = true;
			_outlineChanged = true;
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
				_outlineChanged = true;
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
				_outlineChanged = true;
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
			Vector3 v = cachedTransform.localScale;
			v.x = v.z = ValidateScale(xv);
			v.y = ValidateScale(yv);
			cachedTransform.localScale = v;
			_outlineChanged = true;
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
				_outlineChanged = true;
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
				_outlineChanged = true;
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
				_outlineChanged = true;
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
				_outlineChanged = true;

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
				if (_transformMatrix != null)
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
			Vector3 camPos = Vector3.zero;
			if (matrix.isIdentity)
				_transformMatrix = null;
			else
			{
				_transformMatrix = matrix;
				camPos = new Vector3(_pivot.x * _contentRect.width, -_pivot.y * _contentRect.height, _focalLength);
			}

			//组件的transformMatrix是通过paintingMode实现的，因为全部通过矩阵变换的话，和unity自身的变换混杂在一起，无力理清。
			if (_transformMatrix != null)
			{
				if (this is Container)
					this.EnterPaintingMode(4, null);
			}
			else
			{
				if (this is Container)
					this.LeavePaintingMode(4);
			}

			if (this._paintingMode > 0)
			{
				this.paintingGraphics.cameraPosition = camPos;
				this.paintingGraphics.vertexMatrix = _transformMatrix;
				this._paintingFlag = 1;
			}
			else if (this.graphics != null)
			{
				this.graphics.cameraPosition = camPos;
				this.graphics.vertexMatrix = _transformMatrix;
				_requireUpdateMesh = true;
			}

			_outlineChanged = true;
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
				_outlineChanged = true;

				if (_transformMatrix != null)
					UpdateTransformMatrix();
			}
		}

		void UpdatePivotOffset()
		{
			float px = _pivot.x * _contentRect.width;
			float py = _pivot.y * _contentRect.height;

			//注意这里不用处理skew，因为在顶点变换里有对pivot的处理
			Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, cachedTransform.localRotation, cachedTransform.localScale);
			_pivotOffset = matrix.MultiplyPoint(new Vector3(px, -py, 0));
		}

		void ApplyPivot()
		{
			if (_pivot.x != 0 || _pivot.y != 0)
			{
				Vector3 oldOffset = _pivotOffset;

				UpdatePivotOffset();
				Vector3 v = cachedTransform.localPosition;
				v += oldOffset - _pivotOffset;
				cachedTransform.localPosition = v;
				_outlineChanged = true;
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
		virtual public int layer
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
				if (_paintingMode > 0)
					paintingGraphics.gameObject.layer = value;
				else
					gameObject.layer = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDisposed
		{
			get { return _disposed || gameObject == null; }
		}

		internal void InternalSetParent(Container value)
		{
			if (parent != value)
			{
				if (value == null && parent._disposed)
					parent = value;
				else
				{
					parent = value;
					UpdateHierarchy();
				}
				_outlineChanged = true;
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
		virtual public bool touchable
		{
			get { return _touchable; }
			set { _touchable = value; }
		}

		/// <summary>
		/// 进入绘画模式，整个对象将画到一张RenderTexture上，然后这种贴图将代替原有的显示内容。
		/// 可以在onPaint回调里对这张纹理进行进一步操作，实现特殊效果。
		/// 可能有多个地方要求进入绘画模式，这里用requestorId加以区别，取值是1、2、4、8、16以此类推。1024内内部保留。用户自定义的id从1024开始。
		/// </summary>
		/// <param name="requestId">请求者id</param>
		/// <param name="margin">纹理四周的留空。如果特殊处理后的内容大于原内容，那么这里的设置可以使纹理扩大。</param>
		public void EnterPaintingMode(int requestorId, Margin? margin)
		{
			bool first = _paintingMode == 0;
			_paintingMode |= requestorId;
			if (first)
			{
				if (paintingGraphics == null)
				{
					if (graphics == null)
						paintingGraphics = new NGraphics(this.gameObject);
					else
					{
						GameObject go = new GameObject(this.gameObject.name + " (Painter)");
						go.layer = this.gameObject.layer;
						ToolSet.SetParent(go.transform, cachedTransform);
						go.hideFlags = DisplayOptions.hideFlags;
						paintingGraphics = new NGraphics(go);
					}
				}
				else
					paintingGraphics.enabled = true;
				paintingGraphics.vertexMatrix = null;

				if (_paintingMaterial == null)
				{
					_paintingMaterial = new Material(ShaderConfig.GetShader(ShaderConfig.imageShader));
					_paintingMaterial.hideFlags = DisplayOptions.hideFlags;
				}
				paintingGraphics.material = _paintingMaterial;

				if (this is Container)
				{
					((Container)this).SetChildrenLayer(CaptureCamera.hiddenLayer);
					((Container)this).UpdateBatchingFlags();
				}
				else
					this.InvalidateBatchingState();

				if (graphics != null)
					this.gameObject.layer = CaptureCamera.hiddenLayer;

				_paintingMargin = new Margin();
			}
			if (margin != null)
				_paintingMargin = (Margin)margin;
			_paintingFlag = 1;
		}

		/// <summary>
		/// 离开绘画模式
		/// </summary>
		/// <param name="requestId"></param>
		public void LeavePaintingMode(int requestorId)
		{
			if (_paintingMode == 0 || _disposed)
				return;

			_paintingMode ^= requestorId;
			if (_paintingMode == 0)
			{
				paintingGraphics.ClearMesh();
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
		/// 当cacheAsBitmap已经为true时，再次调用cacheAsBitmap=true将会刷新一次。
		/// </summary>
		public bool cacheAsBitmap
		{
			get { return _cacheAsBitmap; }
			set
			{
				_cacheAsBitmap = value;
				if (value)
					EnterPaintingMode(8, null);
				else
					LeavePaintingMode(8);
			}
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

				if (this is Container)
				{
					if (_blendMode != BlendMode.Normal)
					{
						if (!Application.isPlaying) //编辑期间不支持！！
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

			if (targetSpace == this || _contentRect.width == 0 || _contentRect.height == 0) // optimization
			{
				return _contentRect;
			}
			else if (targetSpace == parent && _rotation.z == 0)
			{
				float sx = this.scaleX;
				float sy = this.scaleY;
				return new Rect(this.x, this.y, _contentRect.width * sx, _contentRect.height * sy);
			}
			else
				return TransformRect(_contentRect, targetSpace);
		}

		protected internal DisplayObject InternalHitTest()
		{
			if (!_visible || (HitTestContext.forTouch && (!_touchable || _touchDisabled)))
				return null;

			return HitTest();
		}

		protected internal DisplayObject InternalHitTestMask()
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
					if (((MeshColliderHitTest)wsc.hitArea).ScreenToLocal(cam, screenPoint, ref point))
					{
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
				return new Vector2(screePoint.x, Stage.inst.stageHeight - screePoint.y);
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
					return new Vector2(0, 0);

				localPoint = localPoint + direction * distOnLine;
			}
			else if (_transformMatrix != null)
			{
				Matrix4x4 mm = (Matrix4x4)_transformMatrix;
				Vector3 camPos = new Vector3(_pivot.x * _contentRect.width, -_pivot.y * _contentRect.height, _focalLength);
				Vector3 center = new Vector3(camPos.x, camPos.y, 0);
				center -= mm.MultiplyPoint(center);
				mm = mm.inverse;
				//相机位置需要变换！
				camPos = mm.MultiplyPoint(camPos);
				//消除轴心影响
				localPoint -= center;
				localPoint = mm.MultiplyPoint(localPoint);
				//获得与平面交点
				Vector3 vec = localPoint - camPos;
				float lambda = -camPos.z / vec.z;
				localPoint.x = camPos.x + lambda * vec.x;
				localPoint.y = camPos.y + lambda * vec.y;
				localPoint.z = 0;

				//在这写可能不大合适，但要转回世界坐标，才能保证孩子的点击检测正确进行
				HitTestContext.worldPoint = this.cachedTransform.TransformPoint(localPoint);
			}
			localPoint.y = -localPoint.y;

			return localPoint;
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

			point.y = -point.y;
			Vector3 v = this.cachedTransform.TransformPoint(point);
			if (targetSpace != null)
			{
				v = targetSpace.cachedTransform.InverseTransformPoint(v);
				v.y = -v.y;
			}
			return v;
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
				Rect result = Rect.MinMaxRect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

				TransformRectPoint(rect.xMin, rect.yMin, targetSpace, ref result);
				TransformRectPoint(rect.xMax, rect.yMin, targetSpace, ref result);
				TransformRectPoint(rect.xMin, rect.yMax, targetSpace, ref result);
				TransformRectPoint(rect.xMax, rect.yMax, targetSpace, ref result);

				return result;
			}
		}

		protected void TransformRectPoint(float px, float py, DisplayObject targetSpace, ref Rect rect)
		{
			Vector2 v = this.cachedTransform.TransformPoint(px, -py, 0);
			if (targetSpace != null)
			{
				v = targetSpace.cachedTransform.InverseTransformPoint(v);
				v.y = -v.y;
			}
			if (rect.xMin > v.x) rect.xMin = v.x;
			if (rect.xMax < v.x) rect.xMax = v.x;
			if (rect.yMin > v.y) rect.yMin = v.y;
			if (rect.yMax < v.y) rect.yMax = v.y;
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
			if (graphics != null)
			{
				graphics.alpha = context.alpha * _alpha;
				graphics.grayed = context.grayed | _grayed;
				graphics.UpdateMaterial(context);
			}

			if (_paintingMode != 0)
			{
				NTexture paintingTexture = paintingGraphics.texture;
				if (paintingTexture != null && paintingTexture.disposed) //Texture可能已被Stage.MonitorTexture销毁
				{
					paintingTexture = null;
					_paintingFlag = 1;
				}
				if (_paintingFlag == 1)
				{
					_paintingFlag = 0;

					//从优化考虑，决定使用绘画模式的容器都需要明确指定大小，而不是自动计算包围。这在UI使用上并没有问题，因为组件总是有固定大小的
					int textureWidth = Mathf.RoundToInt(_contentRect.width + _paintingMargin.left + _paintingMargin.right);
					int textureHeight = Mathf.RoundToInt(_contentRect.height + _paintingMargin.top + _paintingMargin.bottom);
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

					if (paintingTexture != null)
					{
						paintingGraphics.DrawRect(
							new Rect(-_paintingMargin.left, -_paintingMargin.top, paintingTexture.width, paintingTexture.height),
							new Rect(0, 0, 1, 1), Color.white);
						paintingGraphics.UpdateMesh();
					}
					else
						paintingGraphics.ClearMesh();
				}

				if (paintingTexture != null)
				{
					paintingTexture.lastActive = Time.time;

					if (!(this is Container) //如果是容器，这句移到Container.Update的最后执行，因为容器中可能也有需要Capture的内容，要等他们完成后再进行容器的Capture。
						&& (_paintingFlag != 2 || !_cacheAsBitmap))
						UpdateContext.OnEnd += _captureDelegate;
				}

				paintingGraphics.UpdateMaterial(context);
			}

			if (_filter != null)
				_filter.Update();

			Stats.ObjectCount++;
		}

		void Capture()
		{
			Vector2 offset = new Vector2(_paintingMargin.left, _paintingMargin.top);
			CaptureCamera.Capture(this, (RenderTexture)paintingGraphics.texture.nativeTexture, offset);

			_paintingFlag = 2; //2表示已完成一次Capture
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
				if ((object)value != null && (object)cachedTransform.parent == null)
					ToolSet.SetParent(cachedTransform, value);
			}
		}

		void UpdateHierarchy()
		{
			if (!_ownsGameObject)
			{
				if (gameObject != null)
				{
					//we dont change transform parent of this object
					if (parent != null && visible)
						gameObject.SetActive(true);
					else
						gameObject.SetActive(false);
				}
			}
			else if (parent != null)
			{
				ToolSet.SetParent(cachedTransform, parent.cachedTransform);

				if (_visible)
					gameObject.SetActive(true);

				int layerValue = parent.gameObject.layer;
				if (parent._paintingMode != 0)
					layerValue = CaptureCamera.hiddenLayer;

				if ((this is Container) && this.gameObject.layer != layerValue && this._paintingMode == 0)
					((Container)this).SetChildrenLayer(layerValue);

				this.layer = layerValue;
			}
			else if (!_disposed && this.gameObject != null && !StageEngine.beingQuit)
			{
				if (Application.isPlaying)
				{
					if (gOwner == null || gOwner.parent == null)//如果gOwner还有parent的话，说明只是暂时的隐藏
					{
						ToolSet.SetParent(cachedTransform, _home);
						if (_home == null)
							Object.DontDestroyOnLoad(this.gameObject);
					}
				}

				gameObject.SetActive(false);
			}
		}

		virtual public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
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
				if (_paintingMaterial != null)
					Object.Destroy(_paintingMaterial);

				paintingGraphics.Dispose();
				if (paintingGraphics.gameObject != this.gameObject)
				{
					if (Application.isPlaying)
						Object.Destroy(paintingGraphics.gameObject);
					else
						Object.DestroyImmediate(paintingGraphics.gameObject);
				}
			}
			DestroyGameObject();
		}
	}
}
