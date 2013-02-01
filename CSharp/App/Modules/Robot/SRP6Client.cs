using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Helper;

namespace Robot
{
	public class SRP6Client
	{
		private readonly BigInteger n;    // N
		private readonly BigInteger g;    // g
		private readonly BigInteger b;    // B
		private readonly BigInteger a;    // A
		private readonly BigInteger x;    // X
		private readonly BigInteger u;    // U
		private readonly BigInteger s;    // S
		private readonly BigInteger k;    // K
		private readonly byte[] m;        // M
		private readonly byte[] p;
		private readonly string account;
		private readonly BigInteger salt; // s, 服务端发过来的salt
		private const int lowerK = 3;
		private readonly HashAlgorithm hashAlgorithm;

		public SRP6Client(
			HashAlgorithm hashAlgorithm, BigInteger n, BigInteger g, BigInteger b,
			BigInteger salt, string account, string password)
		{
			this.hashAlgorithm = hashAlgorithm;
			this.n = n;
			this.g = g;
			this.b = b;
			this.salt = salt;
			this.account = account;
			string identity = account + ":" + password;
			this.p = hashAlgorithm.ComputeHash(identity.ToByteArray());
			this.a = this.CalculateA();  // A = g ^ a % N
			this.x = this.CalculateX();  // X = H(s, P)
			this.u = this.CalculateU();  // U = H(A, B)
			this.s = this.CalculateS();  // S = (B - (k * g.ModExp(x, N))).ModExp(a + (u * x), N)
			this.k = this.CalculateK();
			this.m = this.CalculateM();  // 
		}

		public BigInteger N
		{
			get
			{
				return this.n;
			}
		}

		public BigInteger G
		{
			get
			{
				return this.g;
			}
		}

		public BigInteger B
		{
			get
			{
				return this.b;
			}
		}

		public BigInteger A
		{
			get
			{
				return this.a;
			}
		}

		public BigInteger X
		{
			get
			{
				return this.x;
			}
		}

		public BigInteger U
		{
			get
			{
				return this.u;
			}
		}

		public BigInteger S
		{
			get
			{
				return this.s;
			}
		}

		public BigInteger K
		{
			get
			{
				return this.k;
			}
		}

		public byte[] M
		{
			get
			{
				return this.m;
			}
		}

		public byte[] P
		{
			get
			{
				return this.p;
			}
		}

		/// <summary>
		/// 计算X: X = H(s, P)
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateX()
		{
			hashAlgorithm.Initialize();
			var joinBytes = new byte[0]
				.Concat(salt.ToTrimByteArray())
				.Concat(this.P)
				.ToArray();
			return hashAlgorithm.ComputeHash(joinBytes).ToUnsignedBigInteger();
		}

		/// <summary>
		/// 计算A: A = g ^ a % N
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateA()
		{
			BigInteger randomA = BigIntegerHelper.RandUnsignedBigInteger(19);
			BigInteger calculatA = BigInteger.ModPow(this.G, randomA, this.N);
			return calculatA;
		}

		/// <summary>
		/// 计算U: U = H(A, B)
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateU()
		{
			hashAlgorithm.Initialize();
			var joinBytes = new byte[0]
				.Concat(this.A.ToTrimByteArray())
				.Concat(this.B.ToTrimByteArray())
				.ToArray();
			return hashAlgorithm.ComputeHash(joinBytes).ToUnsignedBigInteger();
		}

		/// <summary>
		/// 计算S: S = (B - (k * g.ModExp(x, N))).ModExp(a + (u * x), N);
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateS()
		{
			BigInteger s1 = this.B - BigInteger.ModPow(this.G, this.X, this.N) * lowerK;
			BigInteger s2 = this.A + (this.U * this.X);
			return BigInteger.ModPow(s1, s2, this.N);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateK()
		{
			hashAlgorithm.Initialize();
			byte[] sBytes = this.S.ToTrimByteArray();
			int sLength = sBytes.Length;
			int halfLength = sLength / 2;
			var kBytes = new byte[40];
			var halfS = new byte[sLength];

			for (int i = 0; i < halfLength; ++i)
			{
				halfS[i] = sBytes[i * 2];
			}
			var p1 = hashAlgorithm.ComputeHash(halfS);
			for (int i = 0; i < 20; ++i)
			{
				kBytes[i * 2] = p1[i];
			}

			for (int i = 0; i < halfLength; ++i)
			{
				halfS[i] = sBytes[i * 2 + 1];
			}
			var p2 = hashAlgorithm.ComputeHash(halfS);
			for (int i = 0; i < 20; ++i)
			{
				kBytes[i * 2 + 1] = p2[i];
			}

			return kBytes.ToUnsignedBigInteger();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private Byte[] CalculateM()
		{
			hashAlgorithm.Initialize();
			var hashN = hashAlgorithm.ComputeHash(this.N.ToTrimByteArray());
			var hashG = hashAlgorithm.ComputeHash(this.G.ToTrimByteArray());

			// 这里与标准srp6不一样,只异或了20个byte,实际上有32个byte
			for (var i = 0; i < 20; ++i)
			{
				hashN[i] ^= hashG[i];
			}

			var hashGXorhashN = hashN; // H(N) ^ H(g)
			var hashedIdentity = hashAlgorithm.ComputeHash(this.account.ToByteArray()); // H(I)

			// H(H(N) ^ H(g), H(P), s, A, B, K_c)
			var calculateM = hashAlgorithm.ComputeHash(new byte[0]
				.Concat(hashGXorhashN)
				.Concat(hashedIdentity)
				.Concat(this.salt.ToTrimByteArray())
				.Concat(this.A.ToTrimByteArray())
				.Concat(this.B.ToTrimByteArray())
				.Concat(this.K.ToTrimByteArray())
				.ToArray());
			var copyM = new byte[20];
			Array.Copy(calculateM, copyM, copyM.Length);
			return calculateM;
		}
	}
}
