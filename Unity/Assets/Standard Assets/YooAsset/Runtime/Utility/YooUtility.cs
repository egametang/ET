﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace YooAsset
{
	/// <summary>
	/// 字符串工具类
	/// </summary>
	internal static class StringUtility
	{
		[ThreadStatic]
		private static StringBuilder _cacheBuilder = new StringBuilder(1024);

		public static string Format(string format, object arg0)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, arg0);
			return _cacheBuilder.ToString();
		}
		public static string Format(string format, object arg0, object arg1)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, arg0, arg1);
			return _cacheBuilder.ToString();
		}
		public static string Format(string format, object arg0, object arg1, object arg2)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, arg0, arg1, arg2);
			return _cacheBuilder.ToString();
		}
		public static string Format(string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
				throw new ArgumentNullException();

			if (args == null)
				throw new ArgumentNullException();

			_cacheBuilder.Length = 0;
			_cacheBuilder.AppendFormat(format, args);
			return _cacheBuilder.ToString();
		}

		public static string RemoveFirstChar(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return str.Substring(1);
		}
		public static string RemoveLastChar(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return str.Substring(0, str.Length - 1);
		}
		public static string RemoveExtension(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;

			int index = str.LastIndexOf(".");
			if (index == -1)
				return str;
			else
				return str.Remove(index); //"assets/config/test.unity3d" --> "assets/config/test"
		}
	}

	/// <summary>
	/// 文件工具类
	/// </summary>
	internal static class FileUtility
	{
		/// <summary>
		/// 读取文件的文本数据
		/// </summary>
		public static string ReadAllText(string filePath)
		{
			if (File.Exists(filePath) == false)
				return string.Empty;
			return File.ReadAllText(filePath, Encoding.UTF8);
		}

		/// <summary>
		/// 读取文件的字节数据
		/// </summary>
		public static byte[] ReadAllBytes(string filePath)
		{
			if (File.Exists(filePath) == false)
				return null;
			return File.ReadAllBytes(filePath);
		}

		/// <summary>
		/// 创建文件（如果已经存在则删除旧文件）
		/// </summary>
		public static void CreateFile(string filePath, string content)
		{
			// 删除旧文件
			if (File.Exists(filePath))
				File.Delete(filePath);

			// 创建文件夹路径
			CreateFileDirectory(filePath);

			// 创建新文件
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			using (FileStream fs = File.Create(filePath))
			{
				fs.Write(bytes, 0, bytes.Length);
				fs.Flush();
				fs.Close();
			}
		}

		/// <summary>
		/// 创建文件（如果已经存在则删除旧文件）
		/// </summary>
		public static void CreateFile(string filePath, byte[] data)
		{
			// 删除旧文件
			if (File.Exists(filePath))
				File.Delete(filePath);

			// 创建文件夹路径
			CreateFileDirectory(filePath);

			// 创建新文件
			using (FileStream fs = File.Create(filePath))
			{
				fs.Write(data, 0, data.Length);
				fs.Flush();
				fs.Close();
			}
		}

		/// <summary>
		/// 创建文件的文件夹路径
		/// </summary>
		public static void CreateFileDirectory(string filePath)
		{
			// 获取文件的文件夹路径
			string directory = Path.GetDirectoryName(filePath);
			CreateDirectory(directory);
		}

		/// <summary>
		/// 创建文件夹路径
		/// </summary>
		public static void CreateDirectory(string directory)
		{
			// If the directory doesn't exist, create it.
			if (Directory.Exists(directory) == false)
				Directory.CreateDirectory(directory);
		}

		/// <summary>
		/// 获取文件大小（字节数）
		/// </summary>
		public static long GetFileSize(string filePath)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			return fileInfo.Length;
		}
	}

	/// <summary>
	/// 哈希工具类
	/// </summary>
	internal static class HashUtility
	{
		private static string ToString(byte[] hashBytes)
		{
			string result = BitConverter.ToString(hashBytes);
			result = result.Replace("-", "");
			return result.ToLower();
		}

		#region SHA1
		/// <summary>
		/// 获取字符串的Hash值
		/// </summary>
		public static string StringSHA1(string str)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(str);
			return BytesSHA1(buffer);
		}

		/// <summary>
		/// 获取文件的Hash值
		/// </summary>
		public static string FileSHA1(string filePath)
		{
			try
			{
				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return StreamSHA1(fs);
				}
			}
			catch (Exception e)
			{
				YooLogger.Exception(e);
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取数据流的Hash值
		/// </summary>
		public static string StreamSHA1(Stream stream)
		{
			// 说明：创建的是SHA1类的实例，生成的是160位的散列码
			HashAlgorithm hash = HashAlgorithm.Create();
			byte[] hashBytes = hash.ComputeHash(stream);
			return ToString(hashBytes);
		}

		/// <summary>
		/// 获取字节数组的Hash值
		/// </summary>
		public static string BytesSHA1(byte[] buffer)
		{
			// 说明：创建的是SHA1类的实例，生成的是160位的散列码
			HashAlgorithm hash = HashAlgorithm.Create();
			byte[] hashBytes = hash.ComputeHash(buffer);
			return ToString(hashBytes);
		}
		#endregion

		#region MD5
		/// <summary>
		/// 获取字符串的MD5
		/// </summary>
		public static string StringMD5(string str)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(str);
			return BytesMD5(buffer);
		}

		/// <summary>
		/// 获取文件的MD5
		/// </summary>
		public static string FileMD5(string filePath)
		{
			try
			{
				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return StreamMD5(fs);
				}
			}
			catch (Exception e)
			{
				YooLogger.Exception(e);
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取数据流的MD5
		/// </summary>
		public static string StreamMD5(Stream stream)
		{
			MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
			byte[] hashBytes = provider.ComputeHash(stream);
			return ToString(hashBytes);
		}

		/// <summary>
		/// 获取字节数组的MD5
		/// </summary>
		public static string BytesMD5(byte[] buffer)
		{
			MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
			byte[] hashBytes = provider.ComputeHash(buffer);
			return ToString(hashBytes);
		}
		#endregion

		#region CRC32
		/// <summary>
		/// 获取字符串的CRC32
		/// </summary>
		public static string StringCRC32(string str)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(str);
			return BytesCRC32(buffer);
		}

		/// <summary>
		/// 获取文件的CRC32
		/// </summary>
		public static string FileCRC32(string filePath)
		{
			try
			{
				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return StreamCRC32(fs);
				}
			}
			catch (Exception e)
			{
				YooLogger.Exception(e);
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取数据流的CRC32
		/// </summary>
		public static string StreamCRC32(Stream stream)
		{
			CRC32Algorithm hash = new CRC32Algorithm();
			byte[] hashBytes = hash.ComputeHash(stream);
			return ToString(hashBytes);
		}

		/// <summary>
		/// 获取字节数组的CRC32
		/// </summary>
		public static string BytesCRC32(byte[] buffer)
		{
			CRC32Algorithm hash = new CRC32Algorithm();
			byte[] hashBytes = hash.ComputeHash(buffer);
			return ToString(hashBytes);
		}
		#endregion
	}
}