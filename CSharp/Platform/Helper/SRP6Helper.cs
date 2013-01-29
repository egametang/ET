using System.Linq;
using System.Security.Cryptography;

namespace Helper
{
	public static class SRP6Helper
	{
		public static byte[] SRP6ClientCalcK(HashAlgorithm hashAlgorithm, byte[] clientS)
		{
			int clientSLength = clientS.Length;
			int halfLength = clientSLength / 2;
			var kBytes = new byte[40];
			var halfS = new byte[clientSLength];

			for (int i = 0; i < halfLength; ++i)
			{
				halfS[i] = clientS[i * 2];
			}

			var p1 = hashAlgorithm.ComputeHash(halfS);
			for (int i = 0; i < 20; ++i)
			{
				kBytes[i * 2] = p1[i];
			}

			for (int i = 0; i < halfLength; ++i)
			{
				halfS[i] = clientS[i * 2 + 1];
			}

			var p2 = hashAlgorithm.ComputeHash(halfS);
			for (int i = 0; i < 20; ++i)
			{
				kBytes[i * 2 + 1] = p2[i];
			}

			return kBytes;
		}

		public static byte[] SRP6ClientM1(
			HashAlgorithm hashAlgorithm, byte[] identitySalt,
			byte[] n, byte[] g, byte[] s, byte[] a,
			byte[] b, byte[] k)
		{
			var hashN = hashAlgorithm.ComputeHash(n);
			var hashG = hashAlgorithm.ComputeHash(g);
			for (var i = 0; i < hashN.Length; ++i)
			{
				hashN[i] ^= hashG[i];
			}

			var hashGXorhashN = hashN; // H(N) ^ H(g)
			var hashedIdentitySalt = hashAlgorithm.ComputeHash(identitySalt); // H(I)

			// H(H(N) ^ H(g), H(I), s, A, B, K_c)
			var m = hashAlgorithm.ComputeHash(new byte[0] 
				.Concat(hashGXorhashN)
				.Concat(hashedIdentitySalt)
				.Concat(s)
				.Concat(a)
				.Concat(b)
				.Concat(k)
				.ToArray());

			return m;
		}
	}
}
