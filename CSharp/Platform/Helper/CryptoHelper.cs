using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Helper
{
	public class CryptoHelper
	{
		public static byte[] SRP6CalculateM1(
			BigInteger n, BigInteger g, BigInteger s, BigInteger b, string account, string password)
		{
			var random = new SecureRandom();

			var srp6Client = new Srp6Client();
			srp6Client.Init(n, g, new Sha1Digest(), random);
			BigInteger a = srp6Client.GenerateClientCredentials(
				s.ToByteArray(), account.ToByteArray(), password.ToByteArray());
			BigInteger clientS = srp6Client.CalculateSecret(b);

			// 计算M1
			var sha1Digest = new Sha1Digest();
			var kBytes = new byte[sha1Digest.GetDigestSize()];
			var clientSBytes = clientS.ToByteArray();
			sha1Digest.BlockUpdate(clientSBytes, 0, clientSBytes.Length);
			sha1Digest.DoFinal(kBytes, 0);

			sha1Digest.Reset();
			var m1 = new byte[sha1Digest.GetDigestSize()];
			var aBytes = a.ToByteArray();
			var bBytes = b.ToByteArray();
			sha1Digest.BlockUpdate(aBytes, 0, aBytes.Length);
			sha1Digest.BlockUpdate(bBytes, 0, bBytes.Length);
			sha1Digest.BlockUpdate(kBytes, 0, kBytes.Length);
			sha1Digest.DoFinal(m1, 0);
			return m1;
		}
	}
}
