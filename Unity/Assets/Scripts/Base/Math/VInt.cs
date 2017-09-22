using System;

[Serializable]
public struct VInt
{
	public int i;

	public float scalar
	{
		get
		{
			return (float)this.i * 0.001f;
		}
	}

	public VInt(int i)
	{
		this.i = i;
	}

	public VInt(float f)
	{
		this.i = (int)Math.Round((double)(f * 1000f));
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		VInt vInt = (VInt)o;
		return this.i == vInt.i;
	}

	public override int GetHashCode()
	{
		return this.i.GetHashCode();
	}

	public static VInt Min(VInt a, VInt b)
	{
		return new VInt(Math.Min(a.i, b.i));
	}

	public static VInt Max(VInt a, VInt b)
	{
		return new VInt(Math.Max(a.i, b.i));
	}

	public override string ToString()
	{
		return this.scalar.ToString();
	}

	public static explicit operator VInt(float f)
	{
		return new VInt((int)Math.Round((double)(f * 1000f)));
	}

	public static implicit operator VInt(int i)
	{
		return new VInt(i);
	}

	public static explicit operator float(VInt ob)
	{
		return (float)ob.i * 0.001f;
	}

	public static explicit operator long(VInt ob)
	{
		return (long)ob.i;
	}

	public static VInt operator +(VInt a, VInt b)
	{
		return new VInt(a.i + b.i);
	}

	public static VInt operator -(VInt a, VInt b)
	{
		return new VInt(a.i - b.i);
	}

	public static bool operator ==(VInt a, VInt b)
	{
		return a.i == b.i;
	}

	public static bool operator !=(VInt a, VInt b)
	{
		return a.i != b.i;
	}
}
