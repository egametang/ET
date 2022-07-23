using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FairyGUI
{
    [Flags]
    public enum MaterialFlags
    {
        Clipped = 1,
        SoftClipped = 2,
        StencilTest = 4,
        AlphaMask = 8,
        Grayed = 16,
        ColorFilter = 32
    }

    /// <summary>
    /// Every texture-shader combination has a MaterialManager.
    /// </summary>
    public class MaterialManager
    {
        public event Action<Material> onCreateNewMaterial;

        public bool firstMaterialInFrame;

        NTexture _texture;
        Shader _shader;
        List<string> _addKeywords;
        Dictionary<int, List<MaterialRef>> _materials;
        bool _combineTexture;

        class MaterialRef
        {
            public Material material;
            public int frame;
            public BlendMode blendMode;
            public uint group;
        }

        const int internalKeywordsCount = 6;
        static string[] internalKeywords = new[] { "CLIPPED", "SOFT_CLIPPED", null, "ALPHA_MASK", "GRAYED", "COLOR_FILTER" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="shader"></param>
        internal MaterialManager(NTexture texture, Shader shader)
        {
            _texture = texture;
            _shader = shader;
            _materials = new Dictionary<int, List<MaterialRef>>();
            _combineTexture = texture.alphaTexture != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public int GetFlagsByKeywords(IList<string> keywords)
        {
            if (_addKeywords == null)
                _addKeywords = new List<string>();

            int flags = 0;
            for (int i = 0; i < keywords.Count; i++)
            {
                string s = keywords[i];
                if (string.IsNullOrEmpty(s))
                    continue;
                int j = _addKeywords.IndexOf(s);
                if (j == -1)
                {
                    j = _addKeywords.Count;
                    _addKeywords.Add(s);
                }
                flags += (1 << (j + internalKeywordsCount));
            }

            return flags;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="blendMode"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public Material GetMaterial(int flags, BlendMode blendMode, uint group)
        {
            if (blendMode != BlendMode.Normal && BlendModeUtils.Factors[(int)blendMode].pma)
                flags |= (int)MaterialFlags.ColorFilter;

            List<MaterialRef> items;
            if (!_materials.TryGetValue(flags, out items))
            {
                items = new List<MaterialRef>();
                _materials[flags] = items;
            }

            int frameId = Time.frameCount;
            int cnt = items.Count;
            MaterialRef result = null;
            for (int i = 0; i < cnt; i++)
            {
                MaterialRef item = items[i];

                if (item.group == group && item.blendMode == blendMode)
                {
                    if (item.frame != frameId)
                    {
                        firstMaterialInFrame = true;
                        item.frame = frameId;
                    }
                    else
                        firstMaterialInFrame = false;

                    if (_combineTexture)
                        item.material.SetTexture(ShaderConfig.ID_AlphaTex, _texture.alphaTexture);

                    return item.material;
                }
                else if (result == null && (item.frame > frameId || item.frame < frameId - 1)) //collect materials if it is unused in last frame
                    result = item;
            }

            if (result == null)
            {
                result = new MaterialRef() { material = CreateMaterial(flags) };
                items.Add(result);
            }
            else if (_combineTexture)
                result.material.SetTexture(ShaderConfig.ID_AlphaTex, _texture.alphaTexture);

            if (result.blendMode != blendMode)
            {
                BlendModeUtils.Apply(result.material, blendMode);
                result.blendMode = blendMode;
            }

            result.group = group;
            result.frame = frameId;
            firstMaterialInFrame = true;
            return result.material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Material CreateMaterial(int flags)
        {
            Material mat = new Material(_shader);

            mat.mainTexture = _texture.nativeTexture;
            if (_texture.alphaTexture != null)
            {
                mat.EnableKeyword("COMBINED");
                mat.SetTexture(ShaderConfig.ID_AlphaTex, _texture.alphaTexture);
            }

            for (int i = 0; i < internalKeywordsCount; i++)
            {
                if ((flags & (1 << i)) != 0)
                {
                    string s = internalKeywords[i];
                    if (s != null)
                        mat.EnableKeyword(s);
                }
            }
            if (_addKeywords != null)
            {
                int keywordCnt = _addKeywords.Count;
                for (int i = 0; i < keywordCnt; i++)
                {
                    if ((flags & (1 << (i + internalKeywordsCount))) != 0)
                        mat.EnableKeyword(_addKeywords[i]);
                }
            }

            mat.hideFlags = DisplayObject.hideFlags;
            if (onCreateNewMaterial != null)
                onCreateNewMaterial(mat);

            return mat;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DestroyMaterials()
        {
            var iter = _materials.GetEnumerator();
            while (iter.MoveNext())
            {
                List<MaterialRef> items = iter.Current.Value;
                if (Application.isPlaying)
                {
                    int cnt = items.Count;
                    for (int j = 0; j < cnt; j++)
                        Object.Destroy(items[j].material);
                }
                else
                {
                    int cnt = items.Count;
                    for (int j = 0; j < cnt; j++)
                        Object.DestroyImmediate(items[j].material);
                }
                items.Clear();
            }
            iter.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshMaterials()
        {
            _combineTexture = _texture.alphaTexture != null;
            var iter = _materials.GetEnumerator();
            while (iter.MoveNext())
            {
                List<MaterialRef> items = iter.Current.Value;
                int cnt = items.Count;
                for (int j = 0; j < cnt; j++)
                {
                    Material mat = items[j].material;
                    mat.mainTexture = _texture.nativeTexture;
                    if (_combineTexture)
                    {
                        mat.EnableKeyword("COMBINED");
                        mat.SetTexture(ShaderConfig.ID_AlphaTex, _texture.alphaTexture);
                    }
                }
            }
            iter.Dispose();
        }
    }
}
