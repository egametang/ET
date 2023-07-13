using System;
using System.IO;
using System.Text;
using YooAsset;

public class FileOffsetEncryption : IEncryptionServices
{
	public EncryptResult Encrypt(EncryptFileInfo fileInfo)
	{
		if (fileInfo.BundleName.Contains("_gameres_audio"))
		{
			int offset = 32;
			byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
			var encryptedData = new byte[fileData.Length + offset];
			Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

			EncryptResult result = new EncryptResult();
			result.LoadMethod = EBundleLoadMethod.LoadFromFileOffset;
			result.EncryptedData = encryptedData;
			return result;
		}
		else
		{
			EncryptResult result = new EncryptResult();
			result.LoadMethod = EBundleLoadMethod.Normal;
			return result;
		}
	}
}

public class FileStreamEncryption : IEncryptionServices
{
	public EncryptResult Encrypt(EncryptFileInfo fileInfo)
	{
		// LoadFromStream
		if (fileInfo.BundleName.Contains("_gameres_audio"))
		{
			var fileData = File.ReadAllBytes(fileInfo.FilePath);
			for (int i = 0; i < fileData.Length; i++)
			{
				fileData[i] ^= BundleStream.KEY;
			}

			EncryptResult result = new EncryptResult();
			result.LoadMethod = EBundleLoadMethod.LoadFromStream;
			result.EncryptedData = fileData;
			return result;
		}

		// Normal
		{
			EncryptResult result = new EncryptResult();
			result.LoadMethod = EBundleLoadMethod.Normal;
			return result;
		}
	}
}