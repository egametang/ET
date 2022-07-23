using System;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("FairyGUI/UI Painter")]
    [RequireComponent(typeof(MeshCollider), typeof(MeshRenderer))]
    public class UIPainter : MonoBehaviour, EMRenderTarget
    {
        /// <summary>
        /// 
        /// </summary>
        public Container container { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string packageName;

        /// <summary>
        /// 
        /// </summary>
        public string componentName;

        /// <summary>
        /// 
        /// </summary>
        public int sortingOrder;

        [SerializeField]
        string packagePath;
        [SerializeField]
        Camera renderCamera = null;
        [SerializeField]
        bool fairyBatching = false;
        [SerializeField]
        bool touchDisabled = false;

        GComponent _ui;
        [NonSerialized]
        bool _created;
        [NonSerialized]
        bool _captured;
        [NonSerialized]
        Renderer _renderer;

        [NonSerialized]
        RenderTexture _texture;

        Action _captureDelegate;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                if (this.container == null)
                {
                    CreateContainer();

                    if (!string.IsNullOrEmpty(packagePath) && UIPackage.GetByName(packageName) == null)
                        UIPackage.AddPackage(packagePath);
                }
            }
            else
            {
                EMRenderSupport.Add(this);
            }
        }

        void OnDisable()
        {
            if (!Application.isPlaying)
                EMRenderSupport.Remove(this);
        }

        void OnGUI()
        {
            if (!Application.isPlaying)
                EM_BeforeUpdate();
        }

        void Start()
        {
            useGUILayout = false;

            if (!_created && Application.isPlaying)
                CreateUI();
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (_ui != null)
                {
                    _ui.Dispose();
                    _ui = null;
                }

                container.Dispose();
                container = null;
            }
            else
            {
                EMRenderSupport.Remove(this);
            }

            DestroyTexture();
        }

        void CreateContainer()
        {
            this.container = new Container("UIPainter");
            this.container.renderMode = RenderMode.WorldSpace;
            this.container.renderCamera = renderCamera;
            this.container.touchable = !touchDisabled;
            this.container.fairyBatching = fairyBatching;
            this.container._panelOrder = sortingOrder;
            this.container.hitArea = new MeshColliderHitTest(this.gameObject.GetComponent<MeshCollider>());
            SetSortingOrder(this.sortingOrder, true);
            this.container.layer = CaptureCamera.hiddenLayer;
        }

        /// <summary>
        /// Change the sorting order of the panel in runtime.
        /// </summary>
        /// <param name="value">sorting order value</param>
        /// <param name="apply">false if you dont want the default sorting behavior.</param>
        public void SetSortingOrder(int value, bool apply)
        {
            this.sortingOrder = value;
            container._panelOrder = value;

            if (apply)
                Stage.inst.ApplyPanelOrder(container);
        }

        /// <summary>
        /// 
        /// </summary>
        public GComponent ui
        {
            get
            {
                if (!_created && Application.isPlaying)
                    CreateUI();

                return _ui;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateUI()
        {
            if (_ui != null)
            {
                _ui.Dispose();
                _ui = null;
                DestroyTexture();
            }

            _created = true;

            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(componentName))
                return;

            _ui = (GComponent)UIPackage.CreateObject(packageName, componentName);
            if (_ui != null)
            {
                this.container.AddChild(_ui.displayObject);
                this.container.size = _ui.size;
                _texture = CaptureCamera.CreateRenderTexture(Mathf.RoundToInt(_ui.width), Mathf.RoundToInt(_ui.height), UIConfig.depthSupportForPaintingMode);
                _renderer = this.GetComponent<Renderer>();
                if (_renderer != null)
                {
                    _renderer.sharedMaterial.mainTexture = _texture;
                    _captureDelegate = Capture;
                    if (_renderer.sharedMaterial.renderQueue == 3000) //Set in transpare queue only
                    {
                        this.container.onUpdate += () =>
                        {
                            UpdateContext.OnEnd += _captureDelegate;
                        };
                    }
                }
            }
            else
                Debug.LogError("Create " + componentName + "@" + packageName + " failed!");
        }

        void Capture()
        {
            CaptureCamera.Capture(this.container, _texture, this.container.size.y, Vector2.zero);
            if (_renderer != null)
                _renderer.sortingOrder = container.renderingOrder;
        }

        void DestroyTexture()
        {
            if (_texture != null)
            {
                if (Application.isPlaying)
                    RenderTexture.Destroy(_texture);
                else
                    RenderTexture.DestroyImmediate(_texture);
                _texture = null;

                if (_renderer != null)
                    _renderer.sharedMaterial.mainTexture = null;
            }
        }

        #region edit mode functions

        void CaptureInEditMode()
        {
            if (!EMRenderSupport.packageListReady || UIPackage.GetByName(packageName) == null)
                return;

            _captured = true;

            DisplayObject.hideFlags = HideFlags.DontSaveInEditor;
            GComponent view = (GComponent)UIPackage.CreateObject(packageName, componentName);

            if (view != null)
            {
                DestroyTexture();

                _texture = CaptureCamera.CreateRenderTexture(Mathf.RoundToInt(view.width), Mathf.RoundToInt(view.height), false);

                Container root = (Container)view.displayObject;
                root.layer = CaptureCamera.layer;
                root.gameObject.hideFlags = HideFlags.None;
                root.gameObject.SetActive(true);

                GameObject cameraObject = new GameObject("Temp Capture Camera");
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.depth = 0;
                camera.cullingMask = 1 << CaptureCamera.layer;
                camera.clearFlags = CameraClearFlags.Depth;
                camera.orthographic = true;
                camera.nearClipPlane = -30;
                camera.farClipPlane = 30;
                camera.enabled = false;
                camera.targetTexture = _texture;

                float halfHeight = (float)_texture.height / 2;
                camera.orthographicSize = halfHeight;
                cameraObject.transform.localPosition = root.cachedTransform.TransformPoint(halfHeight * camera.aspect, -halfHeight, 0);

                UpdateContext context = new UpdateContext();
                //run two times
                context.Begin();
                view.displayObject.Update(context);
                context.End();

                context.Begin();
                view.displayObject.Update(context);
                context.End();

                RenderTexture old = RenderTexture.active;
                RenderTexture.active = _texture;
                GL.Clear(true, true, Color.clear);
                camera.Render();
                RenderTexture.active = old;

                camera.targetTexture = null;
                view.Dispose();
                GameObject.DestroyImmediate(cameraObject);

                if (_renderer != null)
                    _renderer.sharedMaterial.mainTexture = _texture;
            }
        }

        public void ApplyModifiedProperties(bool sortingOrderChanged)
        {
            if (sortingOrderChanged)
            {
                if (Application.isPlaying)
                    SetSortingOrder(sortingOrder, true);
                else
                    EMRenderSupport.orderChanged = true;
            }
        }

        public void OnUpdateSource(object[] data)
        {
            if (Application.isPlaying)
                return;

            this.packageName = (string)data[0];
            this.packagePath = (string)data[1];
            this.componentName = (string)data[2];

            if ((bool)data[3])
                _captured = false;
        }

        public int EM_sortingOrder
        {
            get { return sortingOrder; }
        }

        public void EM_BeforeUpdate()
        {
            if (_renderer == null)
                _renderer = this.GetComponent<Renderer>();
            if (_renderer != null && _renderer.sharedMaterial.mainTexture != _texture)
                _renderer.sharedMaterial.mainTexture = _texture;

            if (packageName != null && componentName != null && !_captured)
                CaptureInEditMode();
        }

        public void EM_Update(UpdateContext context)
        {
            if (_renderer != null)
                _renderer.sortingOrder = context.renderingOrder++;
        }

        public void EM_Reload()
        {
            _captured = false;
        }

        #endregion
    }
}
