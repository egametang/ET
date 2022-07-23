using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;
using Object = UnityEngine.Object;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class NGraphics : IMeshFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MeshFilter meshFilter { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MeshRenderer meshRenderer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Mesh mesh { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public BlendMode blendMode;

        /// <summary>
        /// 不参与剪裁
        /// </summary>
        public bool dontClip;

        /// <summary>
        /// 当Mesh更新时触发
        /// </summary>
        public event Action meshModifier;

        NTexture _texture;
        string _shader;
        Material _material;
        int _customMatarial; //0-none, 1-common, 2-support internal mask, 128-owns material
        MaterialManager _manager;
        string[] _shaderKeywords;
        int _materialFlags;
        IMeshFactory _meshFactory;

        float _alpha;
        Color _color;
        bool _meshDirty;
        Rect _contentRect;
        FlipType _flip;

        public class VertexMatrix
        {
            public Vector3 cameraPos;
            public Matrix4x4 matrix;
        }
        VertexMatrix _vertexMatrix;

        bool hasAlphaBackup;
        List<byte> _alphaBackup; //透明度改变需要通过修改顶点颜色实现，但顶点颜色本身可能就带有透明度，所以这里要有一个备份

        internal int _maskFlag;
        StencilEraser _stencilEraser;

#if !UNITY_5_6_OR_NEWER
        Color32[] _colors;
#endif

        MaterialPropertyBlock _propertyBlock;
        bool _blockUpdated;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        public NGraphics(GameObject gameObject)
        {
            this.gameObject = gameObject;

            _alpha = 1f;
            _shader = ShaderConfig.imageShader;
            _color = Color.white;
            _meshFactory = this;

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.receiveShadows = false;

            mesh = new Mesh();
            mesh.name = gameObject.name;
            mesh.MarkDynamic();

            meshFilter.mesh = mesh;

            meshFilter.hideFlags = DisplayObject.hideFlags;
            meshRenderer.hideFlags = DisplayObject.hideFlags;
            mesh.hideFlags = DisplayObject.hideFlags;

            Stats.LatestGraphicsCreation++;
        }

        /// <summary>
        /// 
        /// </summary>
        public IMeshFactory meshFactory
        {
            get { return _meshFactory; }
            set
            {
                if (_meshFactory != value)
                {
                    _meshFactory = value;
                    _meshDirty = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetMeshFactory<T>() where T : IMeshFactory, new()
        {
            if (!(_meshFactory is T))
            {
                _meshFactory = new T();
                _meshDirty = true;
            }
            return (T)_meshFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        public Rect contentRect
        {
            get { return _contentRect; }
            set
            {
                _contentRect = value;
                _meshDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FlipType flip
        {
            get { return _flip; }
            set
            {
                if (_flip != value)
                {
                    _flip = value;
                    _meshDirty = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NTexture texture
        {
            get { return _texture; }
            set
            {
                if (_texture != value)
                {
                    if (value != null)
                        value.AddRef();
                    if (_texture != null)
                        _texture.ReleaseRef();

                    _texture = value;
                    if (_customMatarial != 0 && _material != null)
                        _material.mainTexture = _texture != null ? _texture.nativeTexture : null;
                    _meshDirty = true;
                    UpdateManager();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string shader
        {
            get { return _shader; }
            set
            {
                _shader = value;
                UpdateManager();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="texture"></param>
        public void SetShaderAndTexture(string shader, NTexture texture)
        {
            _shader = shader;
            if (_texture != texture)
                this.texture = texture;
            else
                UpdateManager();
        }

        /// <summary>
        /// 
        /// </summary>
        public Material material
        {
            get
            {
                if (_customMatarial == 0 && _material == null && _manager != null)
                    _material = _manager.GetMaterial(_materialFlags, blendMode, 0);
                return _material;
            }
            set
            {
                if ((_customMatarial & 128) != 0 && _material != null)
                    Object.DestroyImmediate(_material);

                _material = value;
                if (_material != null)
                {
                    _customMatarial = 1;
                    if (_material.HasProperty(ShaderConfig.ID_Stencil) || _material.HasProperty(ShaderConfig.ID_ClipBox))
                        _customMatarial |= 2;

                    meshRenderer.sharedMaterial = _material;
                    if (_texture != null)
                        _material.mainTexture = _texture.nativeTexture;
                }
                else
                {
                    _customMatarial = 0;
                    meshRenderer.sharedMaterial = null;
                }
            }
        }

        /// <summary>
        /// Same as material property except that ownership is transferred to this object.
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterial(Material material)
        {
            this.material = material;
            _customMatarial |= 128;
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] materialKeywords
        {
            get { return _shaderKeywords; }
            set
            {
                _shaderKeywords = value;
                UpdateMaterialFlags();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="enabled"></param>
        public void ToggleKeyword(string keyword, bool enabled)
        {
            if (enabled)
            {
                if (_shaderKeywords == null)
                {
                    _shaderKeywords = new string[] { keyword };
                    UpdateMaterialFlags();
                }
                else if (Array.IndexOf(_shaderKeywords, keyword) == -1)
                {
                    Array.Resize(ref _shaderKeywords, _shaderKeywords.Length + 1);
                    _shaderKeywords[_shaderKeywords.Length - 1] = keyword;
                    UpdateMaterialFlags();
                }
            }
            else
            {
                if (_shaderKeywords != null)
                {
                    int i = Array.IndexOf(_shaderKeywords, keyword);
                    if (i != -1)
                    {
                        _shaderKeywords[i] = null;
                        UpdateMaterialFlags();
                    }
                }
            }
        }

        void UpdateManager()
        {
            if (_texture != null)
                _manager = _texture.GetMaterialManager(_shader);
            else
                _manager = null;
            UpdateMaterialFlags();
        }

        void UpdateMaterialFlags()
        {
            if (_customMatarial != 0)
            {
                if (material != null)
                    material.shaderKeywords = _shaderKeywords;
            }
            else if (_shaderKeywords != null && _manager != null)
                _materialFlags = _manager.GetFlagsByKeywords(_shaderKeywords);
            else
                _materialFlags = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool enabled
        {
            get { return meshRenderer.enabled; }
            set { meshRenderer.enabled = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int sortingOrder
        {
            get { return meshRenderer.sortingOrder; }
            set { meshRenderer.sortingOrder = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        internal void _SetStencilEraserOrder(int value)
        {
            _stencilEraser.meshRenderer.sortingOrder = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public Color color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Tint()
        {
            if (_meshDirty)
                return;

            int vertCount = mesh.vertexCount;
            if (vertCount == 0)
                return;

#if !UNITY_5_6_OR_NEWER
            Color32[] colors = _colors;
            if (colors == null)
                colors = mesh.colors32;
#else
            VertexBuffer vb = VertexBuffer.Begin();
            mesh.GetColors(vb.colors);
            List<Color32> colors = vb.colors;
#endif
            for (int i = 0; i < vertCount; i++)
            {
                Color32 col = _color;
                col.a = (byte)(_alpha * (hasAlphaBackup ? _alphaBackup[i] : (byte)255));
                colors[i] = col;
            }

#if !UNITY_5_6_OR_NEWER
            mesh.colors32 = colors;
#else
            mesh.SetColors(vb.colors);
            vb.End();
#endif
        }

        void ChangeAlpha(float value)
        {
            _alpha = value;

            int vertCount = mesh.vertexCount;
            if (vertCount == 0)
                return;

#if !UNITY_5_6_OR_NEWER
            Color32[] colors = _colors;
            if (colors == null)
                colors = mesh.colors32;
#else
            VertexBuffer vb = VertexBuffer.Begin();
            mesh.GetColors(vb.colors);
            List<Color32> colors = vb.colors;
#endif
            for (int i = 0; i < vertCount; i++)
            {
                Color32 col = colors[i];
                col.a = (byte)(_alpha * (hasAlphaBackup ? _alphaBackup[i] : (byte)255));
                colors[i] = col;
            }

#if !UNITY_5_6_OR_NEWER
            mesh.colors32 = colors;
#else
            mesh.SetColors(vb.colors);
            vb.End();
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public VertexMatrix vertexMatrix
        {
            get { return _vertexMatrix; }
            set
            {
                _vertexMatrix = value;
                _meshDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MaterialPropertyBlock materialPropertyBlock
        {
            get
            {
                if (_propertyBlock == null)
                    _propertyBlock = new MaterialPropertyBlock();

                _blockUpdated = true;
                return _propertyBlock;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetMeshDirty()
        {
            _meshDirty = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool UpdateMesh()
        {
            if (_meshDirty)
            {
                UpdateMeshNow();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (mesh != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(mesh);
                else
                    Object.DestroyImmediate(mesh);
                mesh = null;
            }
            if ((_customMatarial & 128) != 0 && _material != null)
                Object.DestroyImmediate(_material);

            if (_texture != null)
            {
                _texture.ReleaseRef();
                _texture = null;
            }

            _manager = null;
            _material = null;
            meshRenderer = null;
            meshFilter = null;
            _stencilEraser = null;
            meshModifier = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="alpha"></param>
        /// <param name="grayed"></param>
        public void Update(UpdateContext context, float alpha, bool grayed)
        {
            Stats.GraphicsCount++;

            if (_meshDirty)
            {
                _alpha = alpha;
                UpdateMeshNow();
            }
            else if (_alpha != alpha)
                ChangeAlpha(alpha);

            if (_propertyBlock != null && _blockUpdated)
            {
                meshRenderer.SetPropertyBlock(_propertyBlock);
                _blockUpdated = false;
            }

            if (_customMatarial != 0)
            {
                if ((_customMatarial & 2) != 0 && _material != null)
                    context.ApplyClippingProperties(_material, false);
            }
            else
            {
                if (_manager != null)
                {
                    if (_maskFlag == 1)
                    {
                        _material = _manager.GetMaterial((int)MaterialFlags.AlphaMask | _materialFlags, BlendMode.Normal, context.clipInfo.clipId);
                        context.ApplyAlphaMaskProperties(_material, false);
                    }
                    else
                    {
                        int matFlags = _materialFlags;
                        if (grayed)
                            matFlags |= (int)MaterialFlags.Grayed;

                        if (context.clipped)
                        {
                            if (context.stencilReferenceValue > 0)
                                matFlags |= (int)MaterialFlags.StencilTest;
                            if (context.rectMaskDepth > 0)
                            {
                                if (context.clipInfo.soft)
                                    matFlags |= (int)MaterialFlags.SoftClipped;
                                else
                                    matFlags |= (int)MaterialFlags.Clipped;
                            }

                            _material = _manager.GetMaterial(matFlags, blendMode, context.clipInfo.clipId);
                            if (_manager.firstMaterialInFrame)
                                context.ApplyClippingProperties(_material, true);
                        }
                        else
                            _material = _manager.GetMaterial(matFlags, blendMode, 0);
                    }
                }
                else
                    _material = null;

                if (!Material.ReferenceEquals(_material, meshRenderer.sharedMaterial))
                    meshRenderer.sharedMaterial = _material;
            }

            if (_maskFlag != 0)
            {
                if (_maskFlag == 1)
                    _maskFlag = 2;
                else
                {
                    if (_stencilEraser != null)
                        _stencilEraser.enabled = false;

                    _maskFlag = 0;
                }
            }
        }

        internal void _PreUpdateMask(UpdateContext context, uint maskId)
        {
            //_maskFlag: 0-new mask, 1-active mask, 2-mask complete
            if (_maskFlag == 0)
            {
                if (_stencilEraser == null)
                {
                    _stencilEraser = new StencilEraser(gameObject.transform);
                    _stencilEraser.meshFilter.mesh = mesh;
                }
                else
                    _stencilEraser.enabled = true;
            }

            _maskFlag = 1;

            if (_manager != null)
            {
                //这里使用maskId而不是clipInfo.clipId，是因为遮罩有两个用途，一个是写入遮罩，一个是擦除，两个不能用同一个材质
                Material mat = _manager.GetMaterial((int)MaterialFlags.AlphaMask | _materialFlags, BlendMode.Normal, maskId);
                if (!Material.ReferenceEquals(mat, _stencilEraser.meshRenderer.sharedMaterial))
                    _stencilEraser.meshRenderer.sharedMaterial = mat;

                context.ApplyAlphaMaskProperties(mat, true);
            }
        }

        void UpdateMeshNow()
        {
            _meshDirty = false;

            if (_texture == null || _meshFactory == null)
            {
                if (mesh.vertexCount > 0)
                {
                    mesh.Clear();

                    if (meshModifier != null)
                        meshModifier();
                }
                return;
            }

            VertexBuffer vb = VertexBuffer.Begin();
            vb.contentRect = _contentRect;
            vb.uvRect = _texture.uvRect;
            if (_texture != null)
                vb.textureSize = new Vector2(_texture.width, _texture.height);
            else
                vb.textureSize = new Vector2(0, 0);
            if (_flip != FlipType.None)
            {
                if (_flip == FlipType.Horizontal || _flip == FlipType.Both)
                {
                    float tmp = vb.uvRect.xMin;
                    vb.uvRect.xMin = vb.uvRect.xMax;
                    vb.uvRect.xMax = tmp;
                }
                if (_flip == FlipType.Vertical || _flip == FlipType.Both)
                {
                    float tmp = vb.uvRect.yMin;
                    vb.uvRect.yMin = vb.uvRect.yMax;
                    vb.uvRect.yMax = tmp;
                }
            }
            vb.vertexColor = _color;
            _meshFactory.OnPopulateMesh(vb);

            int vertCount = vb.currentVertCount;
            if (vertCount == 0)
            {
                if (mesh.vertexCount > 0)
                {
                    mesh.Clear();

                    if (meshModifier != null)
                        meshModifier();
                }
                vb.End();
                return;
            }

            if (_texture.rotated)
            {
                float xMin = _texture.uvRect.xMin;
                float yMin = _texture.uvRect.yMin;
                float yMax = _texture.uvRect.yMax;
                float tmp;
                for (int i = 0; i < vertCount; i++)
                {
                    Vector2 vec = vb.uvs[i];
                    tmp = vec.y;
                    vec.y = yMin + vec.x - xMin;
                    vec.x = xMin + yMax - tmp;
                    vb.uvs[i] = vec;
                }
            }

            hasAlphaBackup = vb._alphaInVertexColor;
            if (hasAlphaBackup)
            {
                if (_alphaBackup == null)
                    _alphaBackup = new List<byte>();
                else
                    _alphaBackup.Clear();
                for (int i = 0; i < vertCount; i++)
                {
                    Color32 col = vb.colors[i];
                    _alphaBackup.Add(col.a);

                    col.a = (byte)(col.a * _alpha);
                    vb.colors[i] = col;
                }
            }
            else if (_alpha != 1)
            {
                for (int i = 0; i < vertCount; i++)
                {
                    Color32 col = vb.colors[i];
                    col.a = (byte)(col.a * _alpha);
                    vb.colors[i] = col;
                }
            }

            if (_vertexMatrix != null)
            {
                Vector3 camPos = _vertexMatrix.cameraPos;
                Vector3 center = new Vector3(camPos.x, camPos.y, 0);
                center -= _vertexMatrix.matrix.MultiplyPoint(center);
                for (int i = 0; i < vertCount; i++)
                {
                    Vector3 pt = vb.vertices[i];
                    pt = _vertexMatrix.matrix.MultiplyPoint(pt);
                    pt += center;
                    Vector3 vec = pt - camPos;
                    float lambda = -camPos.z / vec.z;
                    pt.x = camPos.x + lambda * vec.x;
                    pt.y = camPos.y + lambda * vec.y;
                    pt.z = 0;

                    vb.vertices[i] = pt;
                }
            }

            mesh.Clear();

#if UNITY_5_2 || UNITY_5_3_OR_NEWER
            mesh.SetVertices(vb.vertices);
            if (vb._isArbitraryQuad)
                mesh.SetUVs(0, vb.FixUVForArbitraryQuad());
            else
                mesh.SetUVs(0, vb.uvs);
            mesh.SetColors(vb.colors);
            mesh.SetTriangles(vb.triangles, 0);
            if (vb.uvs2.Count == vb.uvs.Count)
                mesh.SetUVs(1, vb.uvs2);

#if !UNITY_5_6_OR_NEWER
            _colors = null;
#endif
#else
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uv = new Vector2[vertCount];
            _colors = new Color32[vertCount];
            int[] triangles = new int[vb.triangles.Count];

            vb.vertices.CopyTo(vertices);
            vb.uvs.CopyTo(uv);
            vb.colors.CopyTo(_colors);
            vb.triangles.CopyTo(triangles);

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.colors32 = _colors;

            if(vb.uvs2.Count==uv.Length)
            {
                uv = new Vector2[vertCount];
                vb.uvs2.CopyTo(uv);
                mesh.uv2 = uv;
            }
#endif
            vb.End();

            if (meshModifier != null)
                meshModifier();
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            Rect rect = texture.GetDrawRect(vb.contentRect);

            vb.AddQuad(rect, vb.vertexColor, vb.uvRect);
            vb.AddTriangles();
            vb._isArbitraryQuad = _vertexMatrix != null;
        }

        class StencilEraser
        {
            public GameObject gameObject;
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;

            public StencilEraser(Transform parent)
            {
                gameObject = new GameObject("StencilEraser");
                gameObject.transform.SetParent(parent, false);

                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                meshRenderer.receiveShadows = false;

                gameObject.layer = parent.gameObject.layer;
                gameObject.hideFlags = parent.gameObject.hideFlags;
                meshFilter.hideFlags = parent.gameObject.hideFlags;
                meshRenderer.hideFlags = parent.gameObject.hideFlags;
            }

            public bool enabled
            {
                get { return meshRenderer.enabled; }
                set { meshRenderer.enabled = value; }
            }
        }
    }
}
