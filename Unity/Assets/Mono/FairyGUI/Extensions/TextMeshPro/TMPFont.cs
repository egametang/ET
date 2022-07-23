#if FAIRYGUI_TMPRO

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using TMPro;

namespace FairyGUI
{
    /// <summary>
    /// TextMeshPro text adapter for FairyGUI. Most of codes were taken from TextMeshPro! 
    /// Note that not all of TextMeshPro features are supported.
    /// </summary>
    public class TMPFont : BaseFont
    {
        protected TMP_FontAsset _fontAsset;

        FontStyles _style;
        float _scale;
        float _padding;
        float _stylePadding;
        float _ascent;
        float _lineHeight;
        float _boldMultiplier;
        FontWeight _defaultFontWeight;
        FontWeight _fontWeight;
        TextFormat _format;
        TMP_Character _char;
        TMP_Character _lineChar;
        Material _material;
        MaterialManager _manager;

        public TMPFont()
        {
            this.canTint = true;
            this.shader = "FairyGUI/TextMeshPro/Distance Field";
            this.keepCrisp = true;

            _defaultFontWeight = FontWeight.Medium;
        }

        override public void Dispose()
        {
            Release();
        }

        public TMP_FontAsset fontAsset
        {
            get { return _fontAsset; }
            set
            {
                _fontAsset = value;
                Init();
            }
        }

        public FontWeight fontWeight
        {
            get { return _defaultFontWeight; }
            set { _defaultFontWeight = value; }
        }

        void Release()
        {
            if (_manager != null)
            {
                _manager.onCreateNewMaterial -= OnCreateNewMaterial;
                _manager = null;
            }

            if (mainTexture != null)
            {
                mainTexture.Dispose();
                mainTexture = null;
            }

            if (_material != null)
            {
                Material.DestroyImmediate(_material);
                _material = null;
            }
        }

        void Init()
        {
            Release();

            mainTexture = new NTexture(_fontAsset.atlasTexture);
            mainTexture.destroyMethod = DestroyMethod.None;

            _manager = mainTexture.GetMaterialManager(this.shader);
            _manager.onCreateNewMaterial += OnCreateNewMaterial;

            _material = new Material(_fontAsset.material); //copy
            _material.SetFloat(ShaderUtilities.ID_TextureWidth, mainTexture.width);
            _material.SetFloat(ShaderUtilities.ID_TextureHeight, mainTexture.height);
            _material.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding + 1);
            _material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            _material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

            // _ascent = _fontAsset.faceInfo.ascentLine;
            // _lineHeight = _fontAsset.faceInfo.lineHeight;
            _ascent = _fontAsset.faceInfo.pointSize;
            _lineHeight = _fontAsset.faceInfo.pointSize * 1.25f;

            _lineChar = GetCharacterFromFontAsset('_', FontStyles.Normal);
        }

        void OnCreateNewMaterial(Material mat)
        {
            mat.SetFloat(ShaderUtilities.ID_TextureWidth, mainTexture.width);
            mat.SetFloat(ShaderUtilities.ID_TextureHeight, mainTexture.height);
            mat.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding + 1);
            mat.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            mat.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
        }

        override public void UpdateGraphics(NGraphics graphics)
        {
            MaterialPropertyBlock block = graphics.materialPropertyBlock;
            if (_format.outline > 0)
            {
                graphics.ToggleKeyword("OUTLINE_ON", true);

                block.SetFloat(ShaderUtilities.ID_OutlineWidth, _format.outline);
                block.SetColor(ShaderUtilities.ID_OutlineColor, _format.outlineColor);
            }
            else
            {
                graphics.ToggleKeyword("OUTLINE_ON", false);

                block.SetFloat(ShaderUtilities.ID_OutlineWidth, 0);
            }

            if (_format.shadowOffset.x != 0 || _format.shadowOffset.y != 0)
            {
                graphics.ToggleKeyword("UNDERLAY_ON", true);

                block.SetColor(ShaderUtilities.ID_UnderlayColor, _format.shadowColor);
                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, _format.shadowOffset.x);
                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -_format.shadowOffset.y);
                block.SetFloat(ShaderUtilities.ID_UnderlaySoftness, _format.underlaySoftness);
            }
            else
            {
                graphics.ToggleKeyword("UNDERLAY_ON", false);

                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0);
                block.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, 0);
                block.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0);
            }

            block.SetFloat(ShaderUtilities.ID_FaceDilate, _format.faceDilate);
            block.SetFloat(ShaderUtilities.ID_OutlineSoftness, _format.outlineSoftness);

            if (_material.HasProperty(ShaderUtilities.ID_ScaleRatio_A))
            {
                //ShaderUtilities.GetPadding does not support handle materialproperyblock, we have to use a temp material
                _material.SetFloat(ShaderUtilities.ID_OutlineWidth, block.GetFloat(ShaderUtilities.ID_OutlineWidth));
                _material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, block.GetFloat(ShaderUtilities.ID_UnderlayOffsetX));
                _material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, block.GetFloat(ShaderUtilities.ID_UnderlayOffsetY));
                _material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, block.GetFloat(ShaderUtilities.ID_UnderlaySoftness));
                _material.SetFloat(ShaderUtilities.ID_FaceDilate, block.GetFloat(ShaderUtilities.ID_FaceDilate));
                _material.SetFloat(ShaderUtilities.ID_OutlineSoftness, block.GetFloat(ShaderUtilities.ID_OutlineSoftness));

                _padding = ShaderUtilities.GetPadding(_material, false, false);

                //and then set back the properteis
                block.SetFloat(ShaderUtilities.ID_ScaleRatio_A, _material.GetFloat(ShaderUtilities.ID_ScaleRatio_A));
                block.SetFloat(ShaderUtilities.ID_ScaleRatio_B, _material.GetFloat(ShaderUtilities.ID_ScaleRatio_B));
                block.SetFloat(ShaderUtilities.ID_ScaleRatio_C, _material.GetFloat(ShaderUtilities.ID_ScaleRatio_C));
            }

            // Set Padding based on selected font style
            #region Handle Style Padding
            if (((_style & FontStyles.Bold) == FontStyles.Bold)) // Checks for any combination of Bold Style.
            {
                if (_material.HasProperty(ShaderUtilities.ID_GradientScale))
                {
                    float gradientScale = _material.GetFloat(ShaderUtilities.ID_GradientScale);
                    _stylePadding = _fontAsset.boldStyle / 4.0f * gradientScale * _material.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                    // Clamp overall padding to Gradient Scale size.
                    if (_stylePadding + _padding > gradientScale)
                        _padding = gradientScale - _stylePadding;
                }
                else
                    _stylePadding = 0;
            }
            else
            {
                if (_material.HasProperty(ShaderUtilities.ID_GradientScale))
                {
                    float gradientScale = _material.GetFloat(ShaderUtilities.ID_GradientScale);
                    _stylePadding = _fontAsset.normalStyle / 4.0f * gradientScale * _material.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                    // Clamp overall padding to Gradient Scale size.
                    if (_stylePadding + _padding > gradientScale)
                        _padding = gradientScale - _stylePadding;
                }
                else
                    _stylePadding = 0;
            }
            #endregion Handle Style Padding
        }

        override public void SetFormat(TextFormat format, float fontSizeScale)
        {
            _format = format;

            float size = format.size * fontSizeScale;
            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript || _format.specialStyle == TextFormat.SpecialStyle.Superscript)
                size *= SupScale;

            _scale = size / _fontAsset.faceInfo.pointSize * _fontAsset.faceInfo.scale;
            _style = FontStyles.Normal;
            if (format.bold)
            {
                _style |= FontStyles.Bold;
                _fontWeight = FontWeight.Bold;
                _boldMultiplier = 1 + _fontAsset.boldSpacing * 0.01f;
            }
            else
            {
                _fontWeight = _defaultFontWeight;
                _boldMultiplier = 1.0f;
            }
            if (format.italic)
                _style |= FontStyles.Italic;

            format.FillVertexColors(vertexColors);
        }

        override public bool GetGlyph(char ch, out float width, out float height, out float baseline)
        {
            _char = GetCharacterFromFontAsset(ch, _style);
            if (_char != null)
            {
                width = _char.glyph.metrics.horizontalAdvance * _boldMultiplier * _scale;
                height = _lineHeight * _scale;
                baseline = _ascent * _scale;

                if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
                {
                    height /= SupScale;
                    baseline /= SupScale;
                }
                else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
                {
                    height = height / SupScale + baseline * SupOffset;
                    baseline *= (SupOffset + 1 / SupScale);
                }

                height = Mathf.RoundToInt(height);
                baseline = Mathf.RoundToInt(baseline);

                return true;
            }
            else
            {
                width = 0;
                height = 0;
                baseline = 0;
                return false;
            }
        }

        TMP_Character GetCharacterFromFontAsset(uint unicode, FontStyles fontStyle)
        {
            bool isAlternativeTypeface;
#pragma warning disable
            TMP_FontAsset actualAsset;
#pragma warning restore
            return TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, _fontAsset, true, fontStyle, _fontWeight, 
                out isAlternativeTypeface
                //,out actualAsset //old TMP version need this line
            );
        }

        static Vector3 bottomLeft;
        static Vector3 topLeft;
        static Vector3 topRight;
        static Vector3 bottomRight;

        static Vector4 uvBottomLeft;
        static Vector4 uvTopLeft;
        static Vector4 uvTopRight;
        static Vector4 uvBottomRight;

        static Vector4 uv2BottomLeft;
        static Vector4 uv2TopLeft;
        static Vector4 uv2TopRight;
        static Vector4 uv2BottomRight;

        static Color32[] vertexColors = new Color32[4];

        override public int DrawGlyph(float x, float y,
            List<Vector3> vertList, List<Vector2> uvList, List<Vector2> uv2List, List<Color32> colList)
        {
            GlyphMetrics metrics = _char.glyph.metrics;
            GlyphRect rect = _char.glyph.glyphRect;

            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
                y = y - Mathf.RoundToInt(_ascent * _scale * SupOffset);
            else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
                y = y + Mathf.RoundToInt(_ascent * _scale * (1 / SupScale - 1 + SupOffset));

            topLeft.x = x + (metrics.horizontalBearingX - _padding - _stylePadding) * _scale;
            topLeft.y = y + (metrics.horizontalBearingY + _padding) * _scale;
            bottomRight.x = topLeft.x + (metrics.width + _padding * 2 + _stylePadding * 2) * _scale;
            bottomRight.y = topLeft.y - (metrics.height + _padding * 2) * _scale;

            topRight.x = bottomRight.x;
            topRight.y = topLeft.y;
            bottomLeft.x = topLeft.x;
            bottomLeft.y = bottomRight.y;

            #region Handle Italic & Shearing
            if (((_style & FontStyles.Italic) == FontStyles.Italic))
            {
                // Shift Top vertices forward by half (Shear Value * height of character) and Bottom vertices back by same amount. 
                float shear_value = _fontAsset.italicStyle * 0.01f;
                Vector3 topShear = new Vector3(shear_value * ((metrics.horizontalBearingY + _padding + _stylePadding) * _scale), 0, 0);
                Vector3 bottomShear = new Vector3(shear_value * (((metrics.horizontalBearingY - metrics.height - _padding - _stylePadding)) * _scale), 0, 0);

                topLeft += topShear;
                bottomLeft += bottomShear;
                topRight += topShear;
                bottomRight += bottomShear;
            }
            #endregion Handle Italics & Shearing

            vertList.Add(bottomLeft);
            vertList.Add(topLeft);
            vertList.Add(topRight);
            vertList.Add(bottomRight);

            float u = (rect.x - _padding - _stylePadding) / _fontAsset.atlasWidth;
            float v = (rect.y - _padding - _stylePadding) / _fontAsset.atlasHeight;
            float uw = (rect.width + _padding * 2 + _stylePadding * 2) / _fontAsset.atlasWidth;
            float vw = (rect.height + _padding * 2 + _stylePadding * 2) / _fontAsset.atlasHeight;

            uvBottomLeft = new Vector2(u, v);
            uvTopLeft = new Vector2(u, v + vw);
            uvTopRight = new Vector2(u + uw, v + vw);
            uvBottomRight = new Vector2(u + uw, v);

            float xScale = _scale * 0.01f;
            if (_format.bold)
                xScale *= -1;
            uv2BottomLeft = new Vector2(0, xScale);
            uv2TopLeft = new Vector2(511, xScale);
            uv2TopRight = new Vector2(2093567, xScale);
            uv2BottomRight = new Vector2(2093056, xScale);

            uvList.Add(uvBottomLeft);
            uvList.Add(uvTopLeft);
            uvList.Add(uvTopRight);
            uvList.Add(uvBottomRight);

            uv2List.Add(uv2BottomLeft);
            uv2List.Add(uv2TopLeft);
            uv2List.Add(uv2TopRight);
            uv2List.Add(uv2BottomRight);

            colList.Add(vertexColors[0]);
            colList.Add(vertexColors[1]);
            colList.Add(vertexColors[2]);
            colList.Add(vertexColors[3]);

            return 4;
        }

        override public int DrawLine(float x, float y, float width, int fontSize, int type,
             List<Vector3> vertList, List<Vector2> uvList, List<Vector2> uv2List, List<Color32> colList)
        {
            if (_lineChar == null)
                return 0;

            float thickness;
            float offset;

            if (type == 0)
            {
                thickness = _fontAsset.faceInfo.underlineThickness;
                offset = _fontAsset.faceInfo.underlineOffset;
            }
            else
            {
                thickness = _fontAsset.faceInfo.strikethroughThickness;
                offset = _fontAsset.faceInfo.strikethroughOffset;
            }

            float scale = (float)fontSize / _fontAsset.faceInfo.pointSize * _fontAsset.faceInfo.scale;
            float segmentWidth = _lineChar.glyph.metrics.width / 2 * scale;
            if (width < _lineChar.glyph.metrics.width * scale)
                segmentWidth = width / 2f;

            // UNDERLINE VERTICES FOR (3) LINE SEGMENTS
            #region UNDERLINE VERTICES

            thickness = thickness * scale;
            if (thickness < 1)
                thickness = 1;
            offset = Mathf.RoundToInt(offset * scale);

            // Front Part of the Underline
            y += offset;
            topLeft.x = x;
            topLeft.y = y + _padding * scale;
            bottomRight.x = x + segmentWidth;
            bottomRight.y = y - thickness - _padding * scale;

            topRight.x = bottomRight.x;
            topRight.y = topLeft.y;
            bottomLeft.x = topLeft.x;
            bottomLeft.y = bottomRight.y;

            vertList.Add(bottomLeft);
            vertList.Add(topLeft);
            vertList.Add(topRight);
            vertList.Add(bottomRight);

            // Middle Part of the Underline
            topLeft = topRight;
            bottomLeft = bottomRight;

            topRight.x = x + width - segmentWidth;
            bottomRight.x = topRight.x;

            vertList.Add(bottomLeft);
            vertList.Add(topLeft);
            vertList.Add(topRight);
            vertList.Add(bottomRight);

            // End Part of the Underline
            topLeft = topRight;
            bottomLeft = bottomRight;

            topRight.x = x + width;
            bottomRight.x = topRight.x;

            vertList.Add(bottomLeft);
            vertList.Add(topLeft);
            vertList.Add(topRight);
            vertList.Add(bottomRight);

            #endregion

            // UNDERLINE UV0
            #region HANDLE UV0

            // Calculate UV required to setup the 3 Quads for the Underline.
            Vector2 uv0 = new Vector2((_lineChar.glyph.glyphRect.x - _padding) / _fontAsset.atlasWidth, (_lineChar.glyph.glyphRect.y - _padding) / _fontAsset.atlasHeight);  // bottom left
            Vector2 uv1 = new Vector2(uv0.x, (_lineChar.glyph.glyphRect.y + _lineChar.glyph.glyphRect.height + _padding) / _fontAsset.atlasHeight);  // top left
            Vector2 uv2 = new Vector2((_lineChar.glyph.glyphRect.x - _padding + (float)_lineChar.glyph.glyphRect.width / 2) / _fontAsset.atlasWidth, uv1.y); // Mid Top Left
            Vector2 uv3 = new Vector2(uv2.x, uv0.y); // Mid Bottom Left
            Vector2 uv4 = new Vector2((_lineChar.glyph.glyphRect.x + _padding + (float)_lineChar.glyph.glyphRect.width / 2) / _fontAsset.atlasWidth, uv1.y); // Mid Top Right
            Vector2 uv5 = new Vector2(uv4.x, uv0.y); // Mid Bottom right
            Vector2 uv6 = new Vector2((_lineChar.glyph.glyphRect.x + _padding + _lineChar.glyph.glyphRect.width) / _fontAsset.atlasWidth, uv1.y); // End Part - Bottom Right
            Vector2 uv7 = new Vector2(uv6.x, uv0.y); // End Part - Top Right

            uvList.Add(uv0);
            uvList.Add(uv1);
            uvList.Add(uv2);
            uvList.Add(uv3);

            // Middle Part of the Underline
            uvList.Add(new Vector2(uv2.x - uv2.x * 0.001f, uv0.y));
            uvList.Add(new Vector2(uv2.x - uv2.x * 0.001f, uv1.y));
            uvList.Add(new Vector2(uv2.x + uv2.x * 0.001f, uv1.y));
            uvList.Add(new Vector2(uv2.x + uv2.x * 0.001f, uv0.y));

            // Right Part of the Underline

            uvList.Add(uv5);
            uvList.Add(uv4);
            uvList.Add(uv6);
            uvList.Add(uv7);

            #endregion

            // UNDERLINE UV2
            #region HANDLE UV2 - SDF SCALE
            // UV1 contains Face / Border UV layout.
            float segUv1 = segmentWidth / width;
            float segUv2 = 1 - segUv1;

            //Calculate the xScale or how much the UV's are getting stretched on the X axis for the middle section of the underline.
            float xScale = scale * 0.01f;

            uv2List.Add(PackUV(0, 0, xScale));
            uv2List.Add(PackUV(0, 1, xScale));
            uv2List.Add(PackUV(segUv1, 1, xScale));
            uv2List.Add(PackUV(segUv1, 0, xScale));

            uv2List.Add(PackUV(segUv1, 0, xScale));
            uv2List.Add(PackUV(segUv1, 1, xScale));
            uv2List.Add(PackUV(segUv2, 1, xScale));
            uv2List.Add(PackUV(segUv2, 0, xScale));

            uv2List.Add(PackUV(segUv2, 0, xScale));
            uv2List.Add(PackUV(segUv2, 1, xScale));
            uv2List.Add(PackUV(1, 1, xScale));
            uv2List.Add(PackUV(1, 0, xScale));

            #endregion

            // UNDERLINE VERTEX COLORS
            #region
            // Alpha is the lower of the vertex color or tag color alpha used.

            for (int i = 0; i < 12; i++)
                colList.Add(vertexColors[0]);

            #endregion

            return 12;
        }

        Vector2 PackUV(float x, float y, float xScale)
        {
            double x0 = (int)(x * 511);
            double y0 = (int)(y * 511);

            return new Vector2((float)((x0 * 4096) + y0), xScale);
        }

        override public bool HasCharacter(char ch)
        {
            return _fontAsset.HasCharacter(ch);
        }

        override public int GetLineHeight(int size)
        {
            return Mathf.RoundToInt(_lineHeight * ((float)size / _fontAsset.faceInfo.pointSize * _fontAsset.faceInfo.scale));
        }
    }
}

#endif