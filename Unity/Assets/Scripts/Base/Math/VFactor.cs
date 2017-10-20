using System;

[Serializable]
public struct VFactor
{
	public long nom; //分子

	public long den; //分母

	[NonSerialized]
	public static VFactor zero = new VFactor(0L, 1L);

	[NonSerialized]
	public static VFactor one = new VFactor(1L, 1L);

	[NonSerialized]
	public static VFactor pi = new VFactor(31416L, 10000L);

	[NonSerialized]
	public static VFactor twoPi = new VFactor(62832L, 10000L);

	private static long mask_ = 9223372036854775807L;

	private static long upper_ = 16777215L;

	public int roundInt
	{
		get
		{
			return (int)IntMath.Divide(this.nom, this.den);
		}
	}

	public int integer
	{
		get
		{
			return (int)(this.nom / this.den);
		}
	}

	public float single
	{
		get
		{
			double num = (double)this.nom / (double)this.den;
			return (float)num;
		}
	}

	public bool IsPositive
	{
		get
		{
/*			DebugHelper.Assert(this.den != 0L, "VFactor: denominator is zero !");*/
			if (this.nom == 0L)
			{
				return false;
			}
			bool flag = this.nom > 0L;
			bool flag2 = this.den > 0L;
			return !(flag ^ flag2);
		}
	}

	public bool IsNegative
	{
		get
		{
/*			DebugHelper.Assert(this.den != 0L, "VFactor: denominator is zero !");*/
			if (this.nom == 0L)
			{
				return false;
			}
			bool flag = this.nom > 0L;
			bool flag2 = this.den > 0L;
			return flag ^ flag2;
		}
	}

	public bool IsZero
	{
		get
		{
			return this.nom == 0L;
		}
	}

	public VFactor Inverse
	{
		get
		{
			return new VFactor(this.den, this.nom);
		}
	}

	public VFactor(long n, long d)
	{
		this.nom = n;
		this.den = d;
	}

	public override bool Equals(object obj)
	{
		return obj != null && base.GetType() == obj.GetType() && this == (VFactor)obj;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return this.single.ToString();
	}

	public void strip()
	{
		while ((this.nom & VFactor.mask_) > VFactor.upper_ && (this.den & VFactor.mask_) > VFactor.upper_)
		{
			this.nom >>= 1;
			this.den >>= 1;
		}
	}

	public static bool operator <(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		bool flag = b.den > 0L ^ a.den > 0L;
		return (!flag) ? (num < num2) : (num > num2);
	}

	public static bool operator >(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		bool flag = b.den > 0L ^ a.den > 0L;
		return (!flag) ? (num > num2) : (num < num2);
	}

	public static bool operator <=(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		bool flag = b.den > 0L ^ a.den > 0L;
		return (!flag) ? (num <= num2) : (num >= num2);
	}

	public static bool operator >=(VFactor a, VFactor b)
	{
		long num = a.nom * b.den;
		long num2 = b.nom * a.den;
		bool flag = b.den > 0L ^ a.den > 0L;
		return (!flag) ? (num >= num2) : (num <= num2);
	}

	public static bool operator ==(VFactor a, VFactor b)
	{
		return a.nom * b.den == b.nom * a.den;
	}

	public static bool operator !=(VFactor a, VFactor b)
	{
		return a.nom * b.den != b.nom * a.den;
	}

	public static bool operator <(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		return (a.den <= 0L) ? (num > num2) : (num < num2);
	}

	public static bool operator >(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		return (a.den <= 0L) ? (num < num2) : (num > num2);
	}

	public static bool operator <=(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		return (a.den <= 0L) ? (num >= num2) : (num <= num2);
	}

	public static bool operator >=(VFactor a, long b)
	{
		long num = a.nom;
		long num2 = b * a.den;
		return (a.den <= 0L) ? (num <= num2) : (num >= num2);
	}

	public static bool operator ==(VFactor a, long b)
	{
		return a.nom == b * a.den;
	}

	public static bool operator !=(VFactor a, long b)
	{
		return a.nom != b * a.den;
	}

	public static VFactor operator +(VFactor a, VFactor b)
	{
		return new VFactor
		{
			nom = a.nom * b.den + b.nom * a.den,
			den = a.den * b.den
		};
	}

	public static VFactor operator +(VFactor a, long b)
	{
		a.nom += b * a.den;
		return a;
	}

	public static VFactor operator -(VFactor a, VFactor b)
	{
		return new VFactor
		{
			nom = a.nom * b.den - b.nom * a.den,
			den = a.den * b.den
		};
	}

	public static VFactor operator -(VFactor a, long b)
	{
		a.nom -= b * a.den;
		return a;
	}

	public static VFactor operator *(VFactor a, long b)
	{
		a.nom *= b;
		return a;
	}

	public static VFactor operator /(VFactor a, long b)
	{
		a.den *= b;
		return a;
	}

	public static VInt3 operator *(VInt3 v, VFactor f)
	{
		return IntMath.Divide(v, f.nom, f.den);
	}

	public static VInt2 operator *(VInt2 v, VFactor f)
	{
		return IntMath.Divide(v, f.nom, f.den);
	}

	public static VInt3 operator /(VInt3 v, VFactor f)
	{
		return IntMath.Divide(v, f.den, f.nom);
	}

	public static VInt2 operator /(VInt2 v, VFactor f)
	{
		return IntMath.Divide(v, f.den, f.nom);
	}

	public static int operator *(int i, VFactor f)
	{
		return (int)IntMath.Divide((long)i * f.nom, f.den);
	}

	public static VFactor operator -(VFactor a)
	{
		a.nom = -a.nom;
		return a;
	}
}

