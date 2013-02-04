using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Helper;
using Org.BouncyCastle.Crypto.Digests;

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
		private readonly BigInteger m;        // M
		private readonly byte[] p;
		private readonly byte[] account;
		private readonly BigInteger salt; // s, 服务端发过来的salt
		private const int lowerK = 3;
		private readonly BigInteger smallA ;
		private readonly HashAlgorithm hashAlgorithm;

		public SRP6Client(
			HashAlgorithm hashAlgorithm, BigInteger n, BigInteger g, BigInteger b,
			BigInteger salt, byte[] account, byte[] passwordMd5Hex)
		{
			this.smallA = BigIntegerHelper.RandUnsignedBigInteger(19);

			this.hashAlgorithm = hashAlgorithm;
			this.n = n;
			this.g = g;
			this.b = b;
			this.salt = salt;
			this.account = account;
			this.p = hashAlgorithm.ComputeHash(new byte[0]
				.Concat(account)
				.Concat(new[] { (byte)':' })
				.Concat(passwordMd5Hex)
				.ToArray());

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

		public BigInteger M
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

		public BigInteger Salt
		{
			get
			{
				return this.salt;
			}
		}

		public BigInteger SmallA
		{
			get
			{
				return this.smallA;
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
				.Concat(this.Salt.ToUBigIntegerArray())
				.Concat(this.P)
				.ToArray();
			return hashAlgorithm.ComputeHash(joinBytes).ToUBigInteger();
		}

		/// <summary>
		/// 计算A: A = g ^ a % N
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateA()
		{
			return BigInteger.ModPow(this.G, this.SmallA, this.N);
		}

		/// <summary>
		/// 计算U: U = H(A, B)
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateU()
		{
			hashAlgorithm.Initialize();
			var joinBytes = new byte[0]
				.Concat(this.A.ToUBigIntegerArray())
				.Concat(this.B.ToUBigIntegerArray())
				.ToArray();
			return hashAlgorithm.ComputeHash(joinBytes).ToUBigInteger();
		}

		/// <summary>
		/// 计算S: S = (B - (k * g.ModExp(x, N))).ModExp(a + (u * x), N);
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateS()
		{
			BigInteger s1 = this.B - BigInteger.ModPow(this.G, this.X, this.N) * lowerK;
			BigInteger s2 = this.SmallA + (this.U * this.X);
			BigInteger s3 = BigInteger.ModPow(s1, s2, this.N);
			return s3;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateK()
		{
			hashAlgorithm.Initialize();
			byte[] sBytes = this.S.ToUBigIntegerArray();
			int halfLength = sBytes.Length / 2;
			var kBytes = new byte[40];
			var halfS = new byte[halfLength];

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

			return kBytes.ToUBigInteger();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private BigInteger CalculateM()
		{
			hashAlgorithm.Initialize();
			var hashN = hashAlgorithm.ComputeHash(this.N.ToUBigIntegerArray());
			var hashG = hashAlgorithm.ComputeHash(this.G.ToUBigIntegerArray());

			// 这里与标准srp6不一样,只异或了20个byte,实际上有32个byte
			for (var i = 0; i < 20; ++i)
			{
				hashN[i] ^= hashG[i];
			}

			var hashGXorhashN = hashN; // H(N) ^ H(g)
			var hashedIdentity = hashAlgorithm.ComputeHash(this.account); // H(I)

			// H(H(N) ^ H(g), H(P), s, A, B, K_c)
			var mBytes = hashAlgorithm.ComputeHash(new byte[0]
				.Concat(hashGXorhashN)
				.Concat(hashedIdentity)
				.Concat(this.Salt.ToUBigIntegerArray())
				.Concat(this.A.ToUBigIntegerArray())
				.Concat(this.B.ToUBigIntegerArray())
				.Concat(this.K.ToUBigIntegerArray())
				.ToArray());
			return mBytes.ToUBigInteger();
		}
	}
}
