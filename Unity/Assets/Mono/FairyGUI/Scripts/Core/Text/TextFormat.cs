using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TextFormat
    {
        public enum SpecialStyle
        {
            None,
            Superscript,
            Subscript
        }

        /// <summary>
        /// 
        /// </summary>
        public int size;

        /// <summary>
        /// 
        /// </summary>
        public string font;

        /// <summary>
        /// 
        /// </summary>
        public Color color;

        /// <summary>
        /// 
        /// </summary>
        public int lineSpacing;

        /// <summary>
        /// 
        /// </summary>
        public int letterSpacing;

        /// <summary>
        /// 
        /// </summary>
        public bool bold;

        /// <summary>
        /// 
        /// </summary>
        public bool underline;

        /// <summary>
        /// 
        /// </summary>
        public bool italic;

        /// <summary>
        /// 
        /// </summary>
        public bool strikethrough;

        /// <summary>
        /// 
        /// </summary>
        public Color32[] gradientColor;

        /// <summary>
        /// 
        /// </summary>
        public AlignType align;

        /// <summary>
        /// 
        /// </summary>
        public SpecialStyle specialStyle;

        /// <summary>
        /// 
        /// </summary>
        public float outline;

        /// <summary>
        /// 
        /// </summary>
        public Color outlineColor;

        /// <summary>
        /// 
        /// </summary>
        public Vector2 shadowOffset;

        /// <summary>
        /// 
        /// </summary>
        public Color shadowColor;

        public TextFormat()
        {
            color = Color.black;
            size = 12;
            lineSpacing = 3;
            outlineColor = shadowColor = Color.black;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetColor(uint value)
        {
            uint rr = (value >> 16) & 0x0000ff;
            uint gg = (value >> 8) & 0x0000ff;
            uint bb = value & 0x0000ff;
            float r = rr / 255.0f;
            float g = gg / 255.0f;
            float b = bb / 255.0f;
            color = new Color(r, g, b, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aFormat"></param>
        /// <returns></returns>
        public bool EqualStyle(TextFormat aFormat)
        {
            return size == aFormat.size && color == aFormat.color
                && bold == aFormat.bold && underline == aFormat.underline
                && italic == aFormat.italic
                && strikethrough == aFormat.strikethrough
                && gradientColor == aFormat.gradientColor
                && align == aFormat.align
                && specialStyle == aFormat.specialStyle;
        }

        /// <summary>
        /// Only base NOT all formats will be copied
        /// </summary>
        /// <param name="source"></param>
        public void CopyFrom(TextFormat source)
        {
            this.size = source.size;
            this.font = source.font;
            this.color = source.color;
            this.lineSpacing = source.lineSpacing;
            this.letterSpacing = source.letterSpacing;
            this.bold = source.bold;
            this.underline = source.underline;
            this.italic = source.italic;
            this.strikethrough = source.strikethrough;
            if (source.gradientColor != null)
            {
                this.gradientColor = new Color32[4];
                source.gradientColor.CopyTo(this.gradientColor, 0);
            }
            else
                this.gradientColor = null;
            this.align = source.align;
            this.specialStyle = source.specialStyle;
        }

        public void FillVertexColors(Color32[] vertexColors)
        {
            if (gradientColor == null)
                vertexColors[0] = vertexColors[1] = vertexColors[2] = vertexColors[3] = color;
            else
            {
                vertexColors[0] = gradientColor[1];
                vertexColors[1] = gradientColor[0];
                vertexColors[2] = gradientColor[2];
                vertexColors[3] = gradientColor[3];
            }
        }
    }
}
