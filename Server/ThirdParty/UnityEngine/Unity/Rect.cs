#if SERVER
using System;

namespace UnityEngine
{
	
	/// <summary>
	///   <para>A 2D Rectangle defined by X and Y position, width and height.</para>
	/// </summary>
	public struct Rect
	{
	    private float m_XMin;
	
	    private float m_YMin;
	
	    private float m_Width;
	
	    private float m_Height;
	
	    /// <summary>
	    ///   <para>Shorthand for writing new Rect(0,0,0,0).</para>
	    /// </summary>
	    public static Rect zero => new Rect(0f, 0f, 0f, 0f);
	
	    /// <summary>
	    ///   <para>The X coordinate of the rectangle.</para>
	    /// </summary>
	    public float x
	    {
	        get
	        {
	            return m_XMin;
	        }
	        set
	        {
	            m_XMin = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The Y coordinate of the rectangle.</para>
	    /// </summary>
	    public float y
	    {
	        get
	        {
	            return m_YMin;
	        }
	        set
	        {
	            m_YMin = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The X and Y position of the rectangle.</para>
	    /// </summary>
	    public Vector2 position
	    {
	        get
	        {
	            return new Vector2(m_XMin, m_YMin);
	        }
	        set
	        {
	            m_XMin = value.x;
	            m_YMin = value.y;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The position of the center of the rectangle.</para>
	    /// </summary>
	    public Vector2 center
	    {
	        get
	        {
	            return new Vector2(x + m_Width / 2f, y + m_Height / 2f);
	        }
	        set
	        {
	            m_XMin = value.x - m_Width / 2f;
	            m_YMin = value.y - m_Height / 2f;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The position of the minimum corner of the rectangle.</para>
	    /// </summary>
	    public Vector2 min
	    {
	        get
	        {
	            return new Vector2(xMin, yMin);
	        }
	        set
	        {
	            xMin = value.x;
	            yMin = value.y;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The position of the maximum corner of the rectangle.</para>
	    /// </summary>
	    public Vector2 max
	    {
	        get
	        {
	            return new Vector2(xMax, yMax);
	        }
	        set
	        {
	            xMax = value.x;
	            yMax = value.y;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The width of the rectangle, measured from the X position.</para>
	    /// </summary>
	    public float width
	    {
	        get
	        {
	            return m_Width;
	        }
	        set
	        {
	            m_Width = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The height of the rectangle, measured from the Y position.</para>
	    /// </summary>
	    public float height
	    {
	        get
	        {
	            return m_Height;
	        }
	        set
	        {
	            m_Height = value;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The width and height of the rectangle.</para>
	    /// </summary>
	    public Vector2 size
	    {
	        get
	        {
	            return new Vector2(m_Width, m_Height);
	        }
	        set
	        {
	            m_Width = value.x;
	            m_Height = value.y;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The minimum X coordinate of the rectangle.</para>
	    /// </summary>
	    public float xMin
	    {
	        get
	        {
	            return m_XMin;
	        }
	        set
	        {
	            float xMax = this.xMax;
	            m_XMin = value;
	            m_Width = xMax - m_XMin;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The minimum Y coordinate of the rectangle.</para>
	    /// </summary>
	    public float yMin
	    {
	        get
	        {
	            return m_YMin;
	        }
	        set
	        {
	            float yMax = this.yMax;
	            m_YMin = value;
	            m_Height = yMax - m_YMin;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The maximum X coordinate of the rectangle.</para>
	    /// </summary>
	    public float xMax
	    {
	        get
	        {
	            return m_Width + m_XMin;
	        }
	        set
	        {
	            m_Width = value - m_XMin;
	        }
	    }
	
	    /// <summary>
	    ///   <para>The maximum Y coordinate of the rectangle.</para>
	    /// </summary>
	    public float yMax
	    {
	        get
	        {
	            return m_Height + m_YMin;
	        }
	        set
	        {
	            m_Height = value - m_YMin;
	        }
	    }
	
	    [Obsolete("use xMin")]
	    public float left
	    {
	        get
	        {
	            return m_XMin;
	        }
	    }
	
	    [Obsolete("use xMax")]
	    public float right
	    {
	        get
	        {
	            return m_XMin + m_Width;
	        }
	    }
	
	    [Obsolete("use yMin")]
	    public float top
	    {
	        get
	        {
	            return m_YMin;
	        }
	    }
	
	    [Obsolete("use yMax")]
	    public float bottom
	    {
	        get
	        {
	            return m_YMin + m_Height;
	        }
	    }
	
	    /// <summary>
	    ///   <para>Creates a new rectangle.</para>
	    /// </summary>
	    /// <param name="x">The X value the rect is measured from.</param>
	    /// <param name="y">The Y value the rect is measured from.</param>
	    /// <param name="width">The width of the rectangle.</param>
	    /// <param name="height">The height of the rectangle.</param>
	    public Rect(float x, float y, float width, float height)
	    {
	        m_XMin = x;
	        m_YMin = y;
	        m_Width = width;
	        m_Height = height;
	    }
	
	    /// <summary>
	    ///   <para>Creates a rectangle given a size and position.</para>
	    /// </summary>
	    /// <param name="position">The position of the minimum corner of the rect.</param>
	    /// <param name="size">The width and height of the rect.</param>
	    public Rect(Vector2 position, Vector2 size)
	    {
	        m_XMin = position.x;
	        m_YMin = position.y;
	        m_Width = size.x;
	        m_Height = size.y;
	    }
	
	    /// <summary>
	    ///   <para></para>
	    /// </summary>
	    /// <param name="source"></param>
	    public Rect(Rect source)
	    {
	        m_XMin = source.m_XMin;
	        m_YMin = source.m_YMin;
	        m_Width = source.m_Width;
	        m_Height = source.m_Height;
	    }
	
	    /// <summary>
	    ///   <para>Creates a rectangle from min/max coordinate values.</para>
	    /// </summary>
	    /// <param name="xmin">The minimum X coordinate.</param>
	    /// <param name="ymin">The minimum Y coordinate.</param>
	    /// <param name="xmax">The maximum X coordinate.</param>
	    /// <param name="ymax">The maximum Y coordinate.</param>
	    /// <returns>
	    ///   <para>A rectangle matching the specified coordinates.</para>
	    /// </returns>
	    public static Rect MinMaxRect(float xmin, float ymin, float xmax, float ymax)
	    {
	        return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
	    }
	
	    /// <summary>
	    ///   <para>Set components of an existing Rect.</para>
	    /// </summary>
	    /// <param name="x"></param>
	    /// <param name="y"></param>
	    /// <param name="width"></param>
	    /// <param name="height"></param>
	    public void Set(float x, float y, float width, float height)
	    {
	        m_XMin = x;
	        m_YMin = y;
	        m_Width = width;
	        m_Height = height;
	    }
	
	    /// <summary>
	    ///   <para>Returns true if the x and y components of point is a point inside this rectangle. If allowInverse is present and true, the width and height of the Rect are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
	    /// </summary>
	    /// <param name="point">Point to test.</param>
	    /// <param name="allowInverse">Does the test allow the Rect's width and height to be negative?</param>
	    /// <returns>
	    ///   <para>True if the point lies within the specified rectangle.</para>
	    /// </returns>
	    public bool Contains(Vector2 point)
	    {
	        return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
	    }
	
	    /// <summary>
	    ///   <para>Returns true if the x and y components of point is a point inside this rectangle. If allowInverse is present and true, the width and height of the Rect are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
	    /// </summary>
	    /// <param name="point">Point to test.</param>
	    /// <param name="allowInverse">Does the test allow the Rect's width and height to be negative?</param>
	    /// <returns>
	    ///   <para>True if the point lies within the specified rectangle.</para>
	    /// </returns>
	    public bool Contains(Vector3 point)
	    {
	        return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
	    }
	
	    /// <summary>
	    ///   <para>Returns true if the x and y components of point is a point inside this rectangle. If allowInverse is present and true, the width and height of the Rect are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
	    /// </summary>
	    /// <param name="point">Point to test.</param>
	    /// <param name="allowInverse">Does the test allow the Rect's width and height to be negative?</param>
	    /// <returns>
	    ///   <para>True if the point lies within the specified rectangle.</para>
	    /// </returns>
	    public bool Contains(Vector3 point, bool allowInverse)
	    {
	        if (!allowInverse)
	        {
	            return Contains(point);
	        }
	        bool flag = false;
	        if ((width < 0f && point.x <= xMin && point.x > xMax) || (width >= 0f && point.x >= xMin && point.x < xMax))
	        {
	            flag = true;
	        }
	        if (flag && ((height < 0f && point.y <= yMin && point.y > yMax) || (height >= 0f && point.y >= yMin && point.y < yMax)))
	        {
	            return true;
	        }
	        return false;
	    }
	
	    private static Rect OrderMinMax(Rect rect)
	    {
	        if (rect.xMin > rect.xMax)
	        {
	            float xMin = rect.xMin;
	            rect.xMin = rect.xMax;
	            rect.xMax = xMin;
	        }
	        if (rect.yMin > rect.yMax)
	        {
	            float yMin = rect.yMin;
	            rect.yMin = rect.yMax;
	            rect.yMax = yMin;
	        }
	        return rect;
	    }
	
	    /// <summary>
	    ///   <para>Returns true if the other rectangle overlaps this one. If allowInverse is present and true, the widths and heights of the Rects are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
	    /// </summary>
	    /// <param name="other">Other rectangle to test overlapping with.</param>
	    /// <param name="allowInverse">Does the test allow the widths and heights of the Rects to be negative?</param>
	    public bool Overlaps(Rect other)
	    {
	        return other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
	    }
	
	    /// <summary>
	    ///   <para>Returns true if the other rectangle overlaps this one. If allowInverse is present and true, the widths and heights of the Rects are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
	    /// </summary>
	    /// <param name="other">Other rectangle to test overlapping with.</param>
	    /// <param name="allowInverse">Does the test allow the widths and heights of the Rects to be negative?</param>
	    public bool Overlaps(Rect other, bool allowInverse)
	    {
	        Rect rect = this;
	        if (allowInverse)
	        {
	            rect = OrderMinMax(rect);
	            other = OrderMinMax(other);
	        }
	        return rect.Overlaps(other);
	    }
	
	    /// <summary>
	    ///   <para>Returns a point inside a rectangle, given normalized coordinates.</para>
	    /// </summary>
	    /// <param name="rectangle">Rectangle to get a point inside.</param>
	    /// <param name="normalizedRectCoordinates">Normalized coordinates to get a point for.</param>
	    public static Vector2 NormalizedToPoint(Rect rectangle, Vector2 normalizedRectCoordinates)
	    {
	        return new Vector2(Mathf.Lerp(rectangle.x, rectangle.xMax, normalizedRectCoordinates.x), Mathf.Lerp(rectangle.y, rectangle.yMax, normalizedRectCoordinates.y));
	    }
	
	    /// <summary>
	    ///   <para>Returns the normalized coordinates cooresponding the the point.</para>
	    /// </summary>
	    /// <param name="rectangle">Rectangle to get normalized coordinates inside.</param>
	    /// <param name="point">A point inside the rectangle to get normalized coordinates for.</param>
	    public static Vector2 PointToNormalized(Rect rectangle, Vector2 point)
	    {
	        return new Vector2(Mathf.InverseLerp(rectangle.x, rectangle.xMax, point.x), Mathf.InverseLerp(rectangle.y, rectangle.yMax, point.y));
	    }
	
	    public static bool operator !=(Rect lhs, Rect rhs)
	    {
	        return !(lhs == rhs);
	    }
	
	    public static bool operator ==(Rect lhs, Rect rhs)
	    {
	        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
	    }
	
	    public override int GetHashCode()
	    {
	        return x.GetHashCode() ^ (width.GetHashCode() << 2) ^ (y.GetHashCode() >> 2) ^ (height.GetHashCode() >> 1);
	    }
	
	    public override bool Equals(object other)
	    {
	        if (!(other is Rect))
	        {
	            return false;
	        }
	        Rect rect = (Rect)other;
	        return x.Equals(rect.x) && y.Equals(rect.y) && width.Equals(rect.width) && height.Equals(rect.height);
	    }
	
	    /// <summary>
	    ///   <para>Returns a nicely formatted string for this Rect.</para>
	    /// </summary>
	    /// <param name="format"></param>
	    public override string ToString()
	    {
	        return string.Format("(x:{0:F2}, y:{1:F2}, width:{2:F2}, height:{3:F2})", x, y, width, height);
	    }
	
	    /// <summary>
	    ///   <para>Returns a nicely formatted string for this Rect.</para>
	    /// </summary>
	    /// <param name="format"></param>
	    public string ToString(string format)
	    {
	        return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format), y.ToString(format), width.ToString(format), height.ToString(format));
	    }
	}
}
#endif
