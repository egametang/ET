using System;

public struct VRect
{
	private int m_XMin;

	private int m_YMin;

	private int m_Width;

	private int m_Height;

	public int x
	{
		get
		{
			return this.m_XMin;
		}
		set
		{
			this.m_XMin = value;
		}
	}

	public int y
	{
		get
		{
			return this.m_YMin;
		}
		set
		{
			this.m_YMin = value;
		}
	}

	public VInt2 position
	{
		get
		{
			return new VInt2(this.m_XMin, this.m_YMin);
		}
		set
		{
			this.m_XMin = value.x;
			this.m_YMin = value.y;
		}
	}

	public VInt2 center
	{
		get
		{
			return new VInt2(this.x + (this.m_Width >> 1), this.y + (this.m_Height >> 1));
		}
		set
		{
			this.m_XMin = value.x - (this.m_Width >> 1);
			this.m_YMin = value.y - (this.m_Height >> 1);
		}
	}

	public VInt2 min
	{
		get
		{
			return new VInt2(this.xMin, this.yMin);
		}
		set
		{
			this.xMin = value.x;
			this.yMin = value.y;
		}
	}

	public VInt2 max
	{
		get
		{
			return new VInt2(this.xMax, this.yMax);
		}
		set
		{
			this.xMax = value.x;
			this.yMax = value.y;
		}
	}

	public int width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
		}
	}

	public int height
	{
		get
		{
			return this.m_Height;
		}
		set
		{
			this.m_Height = value;
		}
	}

	public VInt2 size
	{
		get
		{
			return new VInt2(this.m_Width, this.m_Height);
		}
		set
		{
			this.m_Width = value.x;
			this.m_Height = value.y;
		}
	}

	public int xMin
	{
		get
		{
			return this.m_XMin;
		}
		set
		{
			int xMax = this.xMax;
			this.m_XMin = value;
			this.m_Width = xMax - this.m_XMin;
		}
	}

	public int yMin
	{
		get
		{
			return this.m_YMin;
		}
		set
		{
			int yMax = this.yMax;
			this.m_YMin = value;
			this.m_Height = yMax - this.m_YMin;
		}
	}

	public int xMax
	{
		get
		{
			return this.m_Width + this.m_XMin;
		}
		set
		{
			this.m_Width = value - this.m_XMin;
		}
	}

	public int yMax
	{
		get
		{
			return this.m_Height + this.m_YMin;
		}
		set
		{
			this.m_Height = value - this.m_YMin;
		}
	}

	public VRect(int left, int top, int width, int height)
	{
		this.m_XMin = left;
		this.m_YMin = top;
		this.m_Width = width;
		this.m_Height = height;
	}

	public VRect(VRect source)
	{
		this.m_XMin = source.m_XMin;
		this.m_YMin = source.m_YMin;
		this.m_Width = source.m_Width;
		this.m_Height = source.m_Height;
	}

	public static VRect MinMaxRect(int left, int top, int right, int bottom)
	{
		return new VRect(left, top, right - left, bottom - top);
	}

	public void Set(int left, int top, int width, int height)
	{
		this.m_XMin = left;
		this.m_YMin = top;
		this.m_Width = width;
		this.m_Height = height;
	}

	public override string ToString()
	{
		object[] array = new object[]
		{
			this.x,
			this.y,
			this.width,
			this.height
		};
		return string.Format("(x:{0:F2}, y:{1:F2}, width:{2:F2}, height:{3:F2})", array);
	}

	public string ToString(string format)
	{
		object[] array = new object[]
		{
			this.x.ToString(format),
			this.y.ToString(format),
			this.width.ToString(format),
			this.height.ToString(format)
		};
		return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", array);
	}

	public bool Contains(VInt2 point)
	{
		return point.x >= this.xMin && point.x < this.xMax && point.y >= this.yMin && point.y < this.yMax;
	}

	public bool Contains(VInt3 point)
	{
		return point.x >= this.xMin && point.x < this.xMax && point.y >= this.yMin && point.y < this.yMax;
	}

	public bool Contains(VInt3 point, bool allowInverse)
	{
		if (!allowInverse)
		{
			return this.Contains(point);
		}
		bool flag = false;
		if (((float)this.width < 0f && point.x <= this.xMin && point.x > this.xMax) || ((float)this.width >= 0f && point.x >= this.xMin && point.x < this.xMax))
		{
			flag = true;
		}
		return flag && (((float)this.height < 0f && point.y <= this.yMin && point.y > this.yMax) || ((float)this.height >= 0f && point.y >= this.yMin && point.y < this.yMax));
	}

	private static VRect OrderMinMax(VRect rect)
	{
		if (rect.xMin > rect.xMax)
		{
			int xMin = rect.xMin;
			rect.xMin = rect.xMax;
			rect.xMax = xMin;
		}
		if (rect.yMin > rect.yMax)
		{
			int yMin = rect.yMin;
			rect.yMin = rect.yMax;
			rect.yMax = yMin;
		}
		return rect;
	}

	public bool Overlaps(VRect other)
	{
		return other.xMax > this.xMin && other.xMin < this.xMax && other.yMax > this.yMin && other.yMin < this.yMax;
	}

	public bool Overlaps(VRect other, bool allowInverse)
	{
		VRect rect = this;
		if (allowInverse)
		{
			rect = VRect.OrderMinMax(rect);
			other = VRect.OrderMinMax(other);
		}
		return rect.Overlaps(other);
	}

	public override int GetHashCode()
	{
		return this.x.GetHashCode() ^ this.width.GetHashCode() << 2 ^ this.y.GetHashCode() >> 2 ^ this.height.GetHashCode() >> 1;
	}

	public override bool Equals(object other)
	{
		if (!(other is VRect))
		{
			return false;
		}
		VRect vRect = (VRect)other;
		return this.x.Equals(vRect.x) && this.y.Equals(vRect.y) && this.width.Equals(vRect.width) && this.height.Equals(vRect.height);
	}

	public static bool operator !=(VRect lhs, VRect rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.width != rhs.width || lhs.height != rhs.height;
	}

	public static bool operator ==(VRect lhs, VRect rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
	}
}
