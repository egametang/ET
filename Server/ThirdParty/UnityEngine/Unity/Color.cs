#if SERVER
using System;

namespace UnityEngine
{	
	/// <summary>
	///   <para>Representation of RGBA colors.</para>
	/// </summary>
	public struct Color
	{
	    /// <summary>
	    ///   <para>Red component of the color.</para>
	    /// </summary>
	    public float r;
	
	    /// <summary>
	    ///   <para>Green component of the color.</para>
	    /// </summary>
	    public float g;
	
	    /// <summary>
	    ///   <para>Blue component of the color.</para>
	    /// </summary>
	    public float b;
	
	    /// <summary>
	    ///   <para>Alpha component of the color (0 is transparent, 1 is opaque).</para>
	    /// </summary>
	    public float a;
	
	    /// <summary>
	    ///   <para>Solid red. RGBA is (1, 0, 0, 1).</para>
	    /// </summary>
	    public static Color red => new Color(1f, 0f, 0f, 1f);
	
	    /// <summary>
	    ///   <para>Solid green. RGBA is (0, 1, 0, 1).</para>
	    /// </summary>
	    public static Color green => new Color(0f, 1f, 0f, 1f);
	
	    /// <summary>
	    ///   <para>Solid blue. RGBA is (0, 0, 1, 1).</para>
	    /// </summary>
	    public static Color blue => new Color(0f, 0f, 1f, 1f);
	
	    /// <summary>
	    ///   <para>Solid white. RGBA is (1, 1, 1, 1).</para>
	    /// </summary>
	    public static Color white => new Color(1f, 1f, 1f, 1f);
	
	    /// <summary>
	    ///   <para>Solid black. RGBA is (0, 0, 0, 1).</para>
	    /// </summary>
	    public static Color black => new Color(0f, 0f, 0f, 1f);
	
	    /// <summary>
	    ///   <para>Yellow. RGBA is (1, 0.92, 0.016, 1), but the color is nice to look at!</para>
	    /// </summary>
	    public static Color yellow => new Color(1f, 47f / 51f, 4f / 255f, 1f);
	
	    /// <summary>
	    ///   <para>Cyan. RGBA is (0, 1, 1, 1).</para>
	    /// </summary>
	    public static Color cyan => new Color(0f, 1f, 1f, 1f);
	
	    /// <summary>
	    ///   <para>Magenta. RGBA is (1, 0, 1, 1).</para>
	    /// </summary>
	    public static Color magenta => new Color(1f, 0f, 1f, 1f);
	
	    /// <summary>
	    ///   <para>Gray. RGBA is (0.5, 0.5, 0.5, 1).</para>
	    /// </summary>
	    public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);
	
	    /// <summary>
	    ///   <para>English spelling for gray. RGBA is the same (0.5, 0.5, 0.5, 1).</para>
	    /// </summary>
	    public static Color grey => new Color(0.5f, 0.5f, 0.5f, 1f);
	
	    /// <summary>
	    ///   <para>Completely transparent. RGBA is (0, 0, 0, 0).</para>
	    /// </summary>
	    public static Color clear => new Color(0f, 0f, 0f, 0f);
	
	    /// <summary>
	    ///   <para>The grayscale value of the color. (Read Only)</para>
	    /// </summary>
	    public float grayscale => 0.299f * r + 0.587f * g + 0.114f * b;
	
	    /// <summary>
	    ///   <para>A linear value of an sRGB color.</para>
	    /// </summary>
	    public Color linear => new Color(Mathf.GammaToLinearSpace(r), Mathf.GammaToLinearSpace(g), Mathf.GammaToLinearSpace(b), a);
	
	    /// <summary>
	    ///   <para>A version of the color that has had the gamma curve applied.</para>
	    /// </summary>
	    public Color gamma => new Color(Mathf.LinearToGammaSpace(r), Mathf.LinearToGammaSpace(g), Mathf.LinearToGammaSpace(b), a);
	
	    /// <summary>
	    ///   <para>Returns the maximum color component value: Max(r,g,b).</para>
	    /// </summary>
	    public float maxColorComponent => Mathf.Max(Mathf.Max(r, g), b);
	
	    public float this[int index]
	    {
	        get
	        {
	            switch (index)
	            {
	                case 0:
	                    return r;
	                case 1:
	                    return g;
	                case 2:
	                    return b;
	                case 3:
	                    return a;
	                default:
	                    throw new IndexOutOfRangeException("Invalid Vector3 index!");
	            }
	        }
	        set
	        {
	            switch (index)
	            {
	                case 0:
	                    r = value;
	                    break;
	                case 1:
	                    g = value;
	                    break;
	                case 2:
	                    b = value;
	                    break;
	                case 3:
	                    a = value;
	                    break;
	                default:
	                    throw new IndexOutOfRangeException("Invalid Vector3 index!");
	            }
	        }
	    }
	
	    /// <summary>
	    ///   <para>Constructs a new Color with given r,g,b,a components.</para>
	    /// </summary>
	    /// <param name="r">Red component.</param>
	    /// <param name="g">Green component.</param>
	    /// <param name="b">Blue component.</param>
	    /// <param name="a">Alpha component.</param>
	    public Color(float r, float g, float b, float a)
	    {
	        this.r = r;
	        this.g = g;
	        this.b = b;
	        this.a = a;
	    }
	
	    /// <summary>
	    ///   <para>Constructs a new Color with given r,g,b components and sets a to 1.</para>
	    /// </summary>
	    /// <param name="r">Red component.</param>
	    /// <param name="g">Green component.</param>
	    /// <param name="b">Blue component.</param>
	    public Color(float r, float g, float b)
	    {
	        this.r = r;
	        this.g = g;
	        this.b = b;
	        a = 1f;
	    }
	
	    /// <summary>
	    ///   <para>Returns a nicely formatted string of this color.</para>
	    /// </summary>
	    /// <param name="format"></param>
	    public override string ToString()
	    {
	        return string.Format("RGBA({0:F3}, {1:F3}, {2:F3}, {3:F3})", r, g, b, a);
	    }
	
	    /// <summary>
	    ///   <para>Returns a nicely formatted string of this color.</para>
	    /// </summary>
	    /// <param name="format"></param>
	    public string ToString(string format)
	    {
	        return string.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format), g.ToString(format), b.ToString(format), a.ToString(format));
	    }
	
	    public override int GetHashCode()
	    {
	        return ((Vector4)this).GetHashCode();
	    }
	
	    public override bool Equals(object other)
	    {
	        if (!(other is Color))
	        {
	            return false;
	        }
	        Color color = (Color)other;
	        return r.Equals(color.r) && g.Equals(color.g) && b.Equals(color.b) && a.Equals(color.a);
	    }
	
	    public static Color operator +(Color a, Color b)
	    {
	        return new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
	    }
	
	    public static Color operator -(Color a, Color b)
	    {
	        return new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
	    }
	
	    public static Color operator *(Color a, Color b)
	    {
	        return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
	    }
	
	    public static Color operator *(Color a, float b)
	    {
	        return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
	    }
	
	    public static Color operator *(float b, Color a)
	    {
	        return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
	    }
	
	    public static Color operator /(Color a, float b)
	    {
	        return new Color(a.r / b, a.g / b, a.b / b, a.a / b);
	    }
	
	    public static bool operator ==(Color lhs, Color rhs)
	    {
	        return (Vector4)lhs == (Vector4)rhs;
	    }
	
	    public static bool operator !=(Color lhs, Color rhs)
	    {
	        return !(lhs == rhs);
	    }
	
	    /// <summary>
	    ///   <para>Linearly interpolates between colors a and b by t.</para>
	    /// </summary>
	    /// <param name="a">Color a</param>
	    /// <param name="b">Color b</param>
	    /// <param name="t">Float for combining a and b</param>
	    public static Color Lerp(Color a, Color b, float t)
	    {
	        t = Mathf.Clamp01(t);
	        return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
	    }
	
	    /// <summary>
	    ///   <para>Linearly interpolates between colors a and b by t.</para>
	    /// </summary>
	    /// <param name="a"></param>
	    /// <param name="b"></param>
	    /// <param name="t"></param>
	    public static Color LerpUnclamped(Color a, Color b, float t)
	    {
	        return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
	    }
	
	    internal Color RGBMultiplied(float multiplier)
	    {
	        return new Color(r * multiplier, g * multiplier, b * multiplier, a);
	    }
	
	    internal Color AlphaMultiplied(float multiplier)
	    {
	        return new Color(r, g, b, a * multiplier);
	    }
	
	    internal Color RGBMultiplied(Color multiplier)
	    {
	        return new Color(r * multiplier.r, g * multiplier.g, b * multiplier.b, a);
	    }
	
	    public static implicit operator Vector4(Color c)
	    {
	        return new Vector4(c.r, c.g, c.b, c.a);
	    }
	
	    public static implicit operator Color(Vector4 v)
	    {
	        return new Color(v.x, v.y, v.z, v.w);
	    }
	
	    public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
	    {
	        if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
	        {
	            RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
	        }
	        else if (rgbColor.g > rgbColor.r)
	        {
	            RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
	        }
	        else
	        {
	            RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
	        }
	    }
	
	    private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
	    {
	        V = dominantcolor;
	        if (V != 0f)
	        {
	            float num = 0f;
	            num = ((!(colorone > colortwo)) ? colorone : colortwo);
	            float num2 = V - num;
	            if (num2 != 0f)
	            {
	                S = num2 / V;
	                H = offset + (colorone - colortwo) / num2;
	            }
	            else
	            {
	                S = 0f;
	                H = offset + (colorone - colortwo);
	            }
	            H /= 6f;
	            if (H < 0f)
	            {
	                H += 1f;
	            }
	        }
	        else
	        {
	            S = 0f;
	            H = 0f;
	        }
	    }
	
	    /// <summary>
	    ///   <para>Creates an RGB colour from HSV input.</para>
	    /// </summary>
	    /// <param name="H">Hue [0..1].</param>
	    /// <param name="S">Saturation [0..1].</param>
	    /// <param name="V">Value [0..1].</param>
	    /// <param name="hdr">Output HDR colours. If true, the returned colour will not be clamped to [0..1].</param>
	    /// <returns>
	    ///   <para>An opaque colour with HSV matching the input.</para>
	    /// </returns>
	    public static Color HSVToRGB(float H, float S, float V)
	    {
	        return HSVToRGB(H, S, V, hdr: true);
	    }
	
	    /// <summary>
	    ///   <para>Creates an RGB colour from HSV input.</para>
	    /// </summary>
	    /// <param name="H">Hue [0..1].</param>
	    /// <param name="S">Saturation [0..1].</param>
	    /// <param name="V">Value [0..1].</param>
	    /// <param name="hdr">Output HDR colours. If true, the returned colour will not be clamped to [0..1].</param>
	    /// <returns>
	    ///   <para>An opaque colour with HSV matching the input.</para>
	    /// </returns>
	    public static Color HSVToRGB(float H, float S, float V, bool hdr)
	    {
	        Color white = Color.white;
	        if (S == 0f)
	        {
	            white.r = V;
	            white.g = V;
	            white.b = V;
	        }
	        else if (V == 0f)
	        {
	            white.r = 0f;
	            white.g = 0f;
	            white.b = 0f;
	        }
	        else
	        {
	            white.r = 0f;
	            white.g = 0f;
	            white.b = 0f;
	            float num = H * 6f;
	            int num2 = (int)Mathf.Floor(num);
	            float num3 = num - (float)num2;
	            float num4 = V * (1f - S);
	            float num5 = V * (1f - S * num3);
	            float num6 = V * (1f - S * (1f - num3));
	            switch (num2)
	            {
	                case 0:
	                    white.r = V;
	                    white.g = num6;
	                    white.b = num4;
	                    break;
	                case 1:
	                    white.r = num5;
	                    white.g = V;
	                    white.b = num4;
	                    break;
	                case 2:
	                    white.r = num4;
	                    white.g = V;
	                    white.b = num6;
	                    break;
	                case 3:
	                    white.r = num4;
	                    white.g = num5;
	                    white.b = V;
	                    break;
	                case 4:
	                    white.r = num6;
	                    white.g = num4;
	                    white.b = V;
	                    break;
	                case 5:
	                    white.r = V;
	                    white.g = num4;
	                    white.b = num5;
	                    break;
	                case 6:
	                    white.r = V;
	                    white.g = num6;
	                    white.b = num4;
	                    break;
	                case -1:
	                    white.r = V;
	                    white.g = num4;
	                    white.b = num5;
	                    break;
	            }
	            if (!hdr)
	            {
	                white.r = Mathf.Clamp(white.r, 0f, 1f);
	                white.g = Mathf.Clamp(white.g, 0f, 1f);
	                white.b = Mathf.Clamp(white.b, 0f, 1f);
	            }
	        }
	        return white;
	    }
	}
}
#endif