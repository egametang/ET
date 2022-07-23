using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FairyGUI
{
    /// <summary>
    /// GoWrapper is class for wrapping common gameobject into UI display list.
    /// </summary>
    public class GoWrapper : DisplayObject
    {
        [Obsolete("No need to manually set this flag anymore, coz it will be handled automatically.")]
        public bool supportStencil;

        public event Action<UpdateContext> onUpdate;
        public Action<Dictionary<Material, Material>> customCloneMaterials;
        public Action customRecoverMaterials;

        protected GameObject _wrapTarget;
        protected List<RendererInfo> _renderers;
        protected Dictionary<Material, Material> _materialsBackup;
        protected Canvas _canvas;
        protected bool _cloneMaterial;
        protected bool _shouldCloneMaterial;

        protected struct RendererInfo
        {
            public Renderer renderer;
            public Material[] materials;
            public int sortingOrder;
        }

        protected static List<Transform> helperTransformList = new List<Transform>();

        /// <summary>
        /// 
        /// </summary>
        public GoWrapper()
        {
            // _flags |= Flags.SkipBatching;

            _renderers = new List<RendererInfo>();
            _materialsBackup = new Dictionary<Material, Material>();

            CreateGameObject("GoWrapper");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go">包装对象。</param>
        public GoWrapper(GameObject go) : this()
        {
            SetWrapTarget(go, false);
        }

        /// <summary>
        /// 设置包装对象。注意如果原来有包装对象，设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。
        /// 对象包含的所有材质不会被复制，如果材质已经是公用的，这可能影响到其他对象。如果希望自动复制，改为使用SetWrapTarget(target, true)设置。
        /// </summary>
        public GameObject wrapTarget
        {
            get { return _wrapTarget; }
            set { SetWrapTarget(value, false); }
        }

        [Obsolete("setWrapTarget is deprecated. Use SetWrapTarget instead.")]
        public void setWrapTarget(GameObject target, bool cloneMaterial)
        {
            SetWrapTarget(target, cloneMaterial);
        }

        /// <summary>
        ///  设置包装对象。注意如果原来有包装对象，设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cloneMaterial">如果true，则复制材质，否则直接使用sharedMaterial。</param>
        public void SetWrapTarget(GameObject target, bool cloneMaterial)
        {
            // set Flags.SkipBatching only target not null
            if (target == null) _flags &= ~Flags.SkipBatching;
            else _flags |= Flags.SkipBatching;
            InvalidateBatchingState();

            RecoverMaterials();

            _cloneMaterial = cloneMaterial;
            if (_wrapTarget != null)
                _wrapTarget.transform.SetParent(null, false);

            _canvas = null;
            _wrapTarget = target;
            _shouldCloneMaterial = false;
            _renderers.Clear();

            if (_wrapTarget != null)
            {
                _wrapTarget.transform.SetParent(this.cachedTransform, false);
                _canvas = _wrapTarget.GetComponent<Canvas>();
                if (_canvas != null)
                {
                    _canvas.renderMode = RenderMode.WorldSpace;
                    _canvas.worldCamera = StageCamera.main;
                    _canvas.overrideSorting = true;

                    RectTransform rt = _canvas.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 1);
                    rt.position = new Vector3(0, 0, 0);
                    this.SetSize(rt.rect.width, rt.rect.height);
                }
                else
                {
                    CacheRenderers();
                    this.SetSize(0, 0);
                }

                SetGoLayers(this.layer);
            }
        }

        /// <summary>
        /// GoWrapper will cache all renderers of your gameobject on constructor. 
        /// If your gameobject change laterly, call this function to update the cache.
        /// GoWrapper会在构造函数里查询你的gameobject所有的Renderer并保存。如果你的gameobject
        /// 后续发生了改变，调用这个函数通知GoWrapper重新查询和保存。
        /// </summary>
        public void CacheRenderers()
        {
            if (_canvas != null)
                return;

            RecoverMaterials();
            _renderers.Clear();

            Renderer[] items = _wrapTarget.GetComponentsInChildren<Renderer>(true);

            int cnt = items.Length;
            _renderers.Capacity = cnt;
            for (int i = 0; i < cnt; i++)
            {
                Renderer r = items[i];
                Material[] mats = r.sharedMaterials;
                RendererInfo ri = new RendererInfo()
                {
                    renderer = r,
                    materials = mats,
                    sortingOrder = r.sortingOrder
                };
                _renderers.Add(ri);

                if (!_cloneMaterial && mats != null
                    && ((r is SkinnedMeshRenderer) || (r is MeshRenderer)))
                {
                    int mcnt = mats.Length;
                    for (int j = 0; j < mcnt; j++)
                    {
                        Material mat = mats[j];
                        if (mat != null && mat.renderQueue != 3000) //Set the object rendering in Transparent Queue as UI objects
                            mat.renderQueue = 3000;
                    }
                }
            }
            _renderers.Sort((RendererInfo c1, RendererInfo c2) =>
            {
                return c1.sortingOrder - c2.sortingOrder;
            });

            _shouldCloneMaterial = _cloneMaterial;
        }

        void CloneMaterials()
        {
            _shouldCloneMaterial = false;

            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++)
            {
                RendererInfo ri = _renderers[i];
                Material[] mats = ri.materials;
                if (mats == null)
                    continue;

                bool shouldSetRQ = (ri.renderer is SkinnedMeshRenderer) || (ri.renderer is MeshRenderer);

                int mcnt = mats.Length;
                for (int j = 0; j < mcnt; j++)
                {
                    Material mat = mats[j];
                    if (mat == null)
                        continue;

                    //确保相同的材质不会复制两次
                    Material newMat;
                    if (!_materialsBackup.TryGetValue(mat, out newMat))
                    {
                        newMat = new Material(mat);
                        _materialsBackup[mat] = newMat;
                    }
                    mats[j] = newMat;

                    if (shouldSetRQ && mat.renderQueue != 3000) //Set the object rendering in Transparent Queue as UI objects
                        newMat.renderQueue = 3000;
                }

                if (customCloneMaterials != null)
                    customCloneMaterials.Invoke(_materialsBackup);
                else if (ri.renderer != null)
                    ri.renderer.sharedMaterials = mats;
            }
        }

        void RecoverMaterials()
        {
            if (_materialsBackup.Count == 0)
                return;

            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++)
            {
                RendererInfo ri = _renderers[i];
                if (ri.renderer == null)
                    continue;

                Material[] mats = ri.materials;
                if (mats == null)
                    continue;

                int mcnt = mats.Length;
                for (int j = 0; j < mcnt; j++)
                {
                    Material mat = mats[j];

                    foreach (KeyValuePair<Material, Material> kv in _materialsBackup)
                    {
                        if (kv.Value == mat)
                            mats[j] = kv.Key;
                    }
                }

                if (customRecoverMaterials != null)
                    customRecoverMaterials.Invoke();
                else
                    ri.renderer.sharedMaterials = mats;
            }

            foreach (KeyValuePair<Material, Material> kv in _materialsBackup)
                Material.DestroyImmediate(kv.Value);

            _materialsBackup.Clear();
        }

        public override int renderingOrder
        {
            get
            {
                return base.renderingOrder;
            }
            set
            {
                base.renderingOrder = value;

                if (_canvas != null)
                    _canvas.sortingOrder = value;
                else
                {
                    int cnt = _renderers.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        RendererInfo ri = _renderers[i];
                        if (ri.renderer != null)
                        {
                            if (i != 0 && _renderers[i].sortingOrder != _renderers[i - 1].sortingOrder)
                                value = UpdateContext.current.renderingOrder++;
                            ri.renderer.sortingOrder = value;
                        }
                    }
                }
            }
        }

        override protected bool SetLayer(int value, bool fromParent)
        {
            if (base.SetLayer(value, fromParent))
            {
                SetGoLayers(value);
                return true;
            }
            else
                return false;
        }

        protected void SetGoLayers(int layer)
        {
            if (_wrapTarget == null)
                return;

            _wrapTarget.GetComponentsInChildren<Transform>(true, helperTransformList);
            int cnt = helperTransformList.Count;
            for (int i = 0; i < cnt; i++)
                helperTransformList[i].gameObject.layer = layer;
            helperTransformList.Clear();
        }

        override public void Update(UpdateContext context)
        {
            if (onUpdate != null)
                onUpdate(context);

            if (_shouldCloneMaterial)
                CloneMaterials();

            ApplyClipping(context);

            base.Update(context);
        }

        private List<Material> helperMaterials = new List<Material>();
        virtual protected void ApplyClipping(UpdateContext context)
        {
#if UNITY_2018_2_OR_NEWER
            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++)
            {
                Renderer renderer = _renderers[i].renderer;
                if (renderer == null)
                    continue;

                if (customCloneMaterials != null)
                    helperMaterials.AddRange(_materialsBackup.Values);
                else
                    renderer.GetSharedMaterials(helperMaterials);

                int cnt2 = helperMaterials.Count;
                for (int j = 0; j < cnt2; j++)
                {
                    Material mat = helperMaterials[j];
                    if (mat != null)
                        context.ApplyClippingProperties(mat, false);
                }

                helperMaterials.Clear();
            }
#else
            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++)
            {
                Material[] mats = _renderers[i].materials;
                if (mats == null)
                    continue;
                
                int cnt2 = mats.Length;
                for (int j = 0; j < cnt2; j++)
                {
                    Material mat = mats[j];
                    if (mat != null)
                        context.ApplyClippingProperties(mat, false);
                }
            }
#endif
        }

        public override void Dispose()
        {
            if ((_flags & Flags.Disposed) != 0)
                return;

            if (_wrapTarget != null)
            {
                UnityEngine.Object.Destroy(_wrapTarget);
                _wrapTarget = null;

                if (_materialsBackup.Count > 0)
                { //如果有备份，说明材质是复制出来的，应该删除
                    foreach (KeyValuePair<Material, Material> kv in _materialsBackup)
                        Material.DestroyImmediate(kv.Value);
                }
            }

            _renderers = null;
            _materialsBackup = null;
            _canvas = null;
            customCloneMaterials = null;
            customRecoverMaterials = null;

            base.Dispose();
        }
    }
}