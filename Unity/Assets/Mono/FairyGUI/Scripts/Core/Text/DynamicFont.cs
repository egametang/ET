using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicFont : BaseFont
    {
        Font _font;
        int _size;
        float _ascent;
        float _lineHeight;
        float _scale;
        TextFormat _format;
        FontStyle _style;
        bool _boldVertice;
        CharacterInfo _char;
        CharacterInfo _lineChar;
        bool _gotLineChar;

        public DynamicFont()
        {
            this.canTint = true;
            this.keepCrisp = true;
            this.customOutline = true;
            this.shader = ShaderConfig.textShader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public DynamicFont(string name, Font font) : this()
        {
            this.name = name;
            this.nativeFont = font;
        }

        override public void Dispose()
        {
            Font.textureRebuilt -= textureRebuildCallback;
        }

        public Font nativeFont
        {
            get { return _font; }
            set
            {
                if (_font != null)
                    Font.textureRebuilt -= textureRebuildCallback;

                _font = value;
                Font.textureRebuilt += textureRebuildCallback;
                _font.hideFlags = DisplayObject.hideFlags;
                _font.material.hideFlags = DisplayObject.hideFlags;
                _font.material.mainTexture.hideFlags = DisplayObject.hideFlags;

                if (mainTexture != null)
                    mainTexture.Dispose();
                mainTexture = new NTexture(_font.material.mainTexture);
                mainTexture.destroyMethod = DestroyMethod.None;

                // _ascent = _font.ascent;
                // _lineHeight = _font.lineHeight;
                _ascent = _font.fontSize;
                _lineHeight = _font.fontSize * 1.25f;
            }
        }

        override public void SetFormat(TextFormat format, float fontSizeScale)
        {
            _format = format;
            float size = format.size * fontSizeScale;
            if (keepCrisp)
                size *= UIContentScaler.scaleFactor;
            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript || _format.specialStyle == TextFormat.SpecialStyle.Superscript)
                size *= SupScale;
            _size = Mathf.FloorToInt(size);
            if (_size == 0)
                _size = 1;
            _scale = (float)_size / _font.fontSize;

            if (format.bold && !customBold)
            {
                if (format.italic)
                {
                    if (customBoldAndItalic)
                        _style = FontStyle.Italic;
                    else
                        _style = FontStyle.BoldAndItalic;
                }
                else
                    _style = FontStyle.Bold;
            }
            else
            {
                if (format.italic)
                    _style = FontStyle.Italic;
                else
                    _style = FontStyle.Normal;
            }

            _boldVertice = format.bold && (customBold || (format.italic && customBoldAndItalic));
            format.FillVertexColors(vertexColors);
        }

        override public void PrepareCharacters(string text)
        {
            _font.RequestCharactersInTexture(text, _size, _style);
        }

        override public bool GetGlyph(char ch, out float width, out float height, out float baseline)
        {
            if (!_font.GetCharacterInfo(ch, out _char, _size, _style))
            {
                if (ch == ' ')
                {
                    //space may not be prepared, try again
                    _font.RequestCharactersInTexture(" ", _size, _style);
                    _font.GetCharacterInfo(ch, out _char, _size, _style);
                }
                else
                {
                    width = height = baseline = 0;
                    return false;
                }
            }

            width = _char.advance;
            height = _lineHeight * _scale;
            baseline = _ascent * _scale;
            if (_boldVertice)
                width++;

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

            if (keepCrisp)
            {
                width /= UIContentScaler.scaleFactor;
                height /= UIContentScaler.scaleFactor;
                baseline /= UIContentScaler.scaleFactor;
            }

            return true;
        }

        static Vector3 bottomLeft;
        static Vector3 topLeft;
        static Vector3 topRight;
        static Vector3 bottomRight;

        static Vector2 uvBottomLeft;
        static Vector2 uvTopLeft;
        static Vector2 uvTopRight;
        static Vector2 uvBottomRight;

        static Color32[] vertexColors = new Color32[4];

        static Vector3[] BOLD_OFFSET = new Vector3[]
        {
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0f, -0.5f, 0f),
            new Vector3(0f, 0.5f, 0f)
        };

        override public int DrawGlyph(float x, float y,
            List<Vector3> vertList, List<Vector2> uvList, List<Vector2> uv2List, List<Color32> colList)
        {
            topLeft.x = _char.minX;
            topLeft.y = _char.maxY;
            bottomRight.x = _char.maxX;
            if (_char.glyphWidth == 0) //zero width, space etc
                bottomRight.x = topLeft.x + _size / 2;
            bottomRight.y = _char.minY;

            if (keepCrisp)
            {
                topLeft /= UIContentScaler.scaleFactor;
                bottomRight /= UIContentScaler.scaleFactor;
            }

            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
                y = y - Mathf.RoundToInt(_ascent * _scale * SupOffset);
            else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
                y = y + Mathf.RoundToInt(_ascent * _scale * (1 / SupScale - 1 + SupOffset));

            topLeft.x += x;
            topLeft.y += y;
            bottomRight.x += x;
            bottomRight.y += y;

            topRight.x = bottomRight.x;
            topRight.y = topLeft.y;
            bottomLeft.x = topLeft.x;
            bottomLeft.y = bottomRight.y;

            vertList.Add(bottomLeft);
            vertList.Add(topLeft);
            vertList.Add(topRight);
            vertList.Add(bottomRight);

            uvBottomLeft = _char.uvBottomLeft;
            uvTopLeft = _char.uvTopLeft;
            uvTopRight = _char.uvTopRight;
            uvBottomRight = _char.uvBottomRight;

            uvList.Add(uvBottomLeft);
            uvList.Add(uvTopLeft);
            uvList.Add(uvTopRight);
            uvList.Add(uvBottomRight);

            colList.Add(vertexColors[0]);
            colList.Add(vertexColors[1]);
            colList.Add(vertexColors[2]);
            colList.Add(vertexColors[3]);

            if (_boldVertice)
            {
                for (int b = 0; b < 4; b++)
                {
                    Vector3 boldOffset = BOLD_OFFSET[b];

                    vertList.Add(bottomLeft + boldOffset);
                    vertList.Add(topLeft + boldOffset);
                    vertList.Add(topRight + boldOffset);
                    vertList.Add(bottomRight + boldOffset);

                    uvList.Add(uvBottomLeft);
                    uvList.Add(uvTopLeft);
                    uvList.Add(uvTopRight);
                    uvList.Add(uvBottomRight);

                    colList.Add(vertexColors[0]);
                    colList.Add(vertexColors[1]);
                    colList.Add(vertexColors[2]);
                    colList.Add(vertexColors[3]);
                }

                return 20;
            }
            else
                return 4;
        }

        override public int DrawLine(float x, float y, float width, int fontSize, int type,
            List<Vector3> vertList, List<Vector2> uvList, List<Vector2> uv2List, List<Color32> colList)
        {
            if (!_gotLineChar)
            {
                _gotLineChar = true;
                _font.RequestCharactersInTexture("_", 50, FontStyle.Normal);
                _font.GetCharacterInfo('_', out _lineChar, 50, FontStyle.Normal);
            }

            float thickness;
            float offset;

            thickness = Mathf.Max(1, fontSize / 16f); //guest underline size
            if (type == 0)
                offset = Mathf.RoundToInt(_lineChar.minY * (float)fontSize / 50 + thickness);
            else
                offset = Mathf.RoundToInt(_ascent * 0.4f * fontSize / _font.fontSize);
            if (thickness < 1)
                thickness = 1;

            topLeft.x = x;
            topLeft.y = y + offset;
            bottomRight.x = x + width;
            bottomRight.y = topLeft.y - thickness;

            topRight.x = bottomRight.x;
            topRight.y = topLeft.y;
            bottomLeft.x = topLeft.x;
            bottomLeft.y = bottomRight.y;

            vertList.Add(bottomLeft);
            vertList.Add(topLeft);
            vertList.Add(topRight);
            vertList.Add(bottomRight);

            uvBottomLeft = _lineChar.uvBottomLeft;
            uvTopLeft = _lineChar.uvTopLeft;
            uvTopRight = _lineChar.uvTopRight;
            uvBottomRight = _lineChar.uvBottomRight;

            //取中点的UV
            Vector2 u0;
            if (_lineChar.uvBottomLeft.x != _lineChar.uvBottomRight.x)
                u0.x = (_lineChar.uvBottomLeft.x + _lineChar.uvBottomRight.x) * 0.5f;
            else
                u0.x = (_lineChar.uvBottomLeft.x + _lineChar.uvTopLeft.x) * 0.5f;

            if (_lineChar.uvBottomLeft.y != _lineChar.uvTopLeft.y)
                u0.y = (_lineChar.uvBottomLeft.y + _lineChar.uvTopLeft.y) * 0.5f;
            else
                u0.y = (_lineChar.uvBottomLeft.y + _lineChar.uvBottomRight.y) * 0.5f;

            uvList.Add(u0);
            uvList.Add(u0);
            uvList.Add(u0);
            uvList.Add(u0);

            colList.Add(vertexColors[0]);
            colList.Add(vertexColors[1]);
            colList.Add(vertexColors[2]);
            colList.Add(vertexColors[3]);

            if (_boldVertice)
            {
                for (int b = 0; b < 4; b++)
                {
                    Vector3 boldOffset = BOLD_OFFSET[b];

                    vertList.Add(bottomLeft + boldOffset);
                    vertList.Add(topLeft + boldOffset);
                    vertList.Add(topRight + boldOffset);
                    vertList.Add(bottomRight + boldOffset);

                    uvList.Add(u0);
                    uvList.Add(u0);
                    uvList.Add(u0);
                    uvList.Add(u0);

                    colList.Add(vertexColors[0]);
                    colList.Add(vertexColors[1]);
                    colList.Add(vertexColors[2]);
                    colList.Add(vertexColors[3]);
                }

                return 20;
            }
            else
                return 4;
        }

        override public bool HasCharacter(char ch)
        {
            return _font.HasCharacter(ch);
        }

        override public int GetLineHeight(int size)
        {
            return Mathf.RoundToInt(_lineHeight * size / _font.fontSize);
        }

        void textureRebuildCallback(Font targetFont)
        {
            if (_font != targetFont)
                return;

            if (mainTexture == null || !Application.isPlaying)
            {
                mainTexture = new NTexture(_font.material.mainTexture);
                mainTexture.destroyMethod = DestroyMethod.None;
            }
            else
                mainTexture.Reload(_font.material.mainTexture, null);

            _gotLineChar = false;

            textRebuildFlag = true;
            version++;

            //Debug.Log("Font texture rebuild: " + name + "," + mainTexture.width + "," + mainTexture.height);
        }
    }
}
