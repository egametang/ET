using UnityEngine;
using System.Collections;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace cn.sharesdk.unity3d
{

	public class ZipHelper
	{
		public void UnzipWithPath(string path, string dirpath)
		{
			//这是根目录的路径  
			string dirPath = dirpath;
			//ZipEntry：文件条目 就是该目录下所有的文件列表(也就是所有文件的路径)  
			ZipEntry zip = null;
			//输入的所有的文件流都是存储在这里面的  
			ZipInputStream zipInStream = null;
			//读取文件流到zipInputStream  
			zipInStream = new ZipInputStream(File.OpenRead(path));
			//循环读取Zip目录下的所有文件  
			while ((zip = zipInStream.GetNextEntry()) != null)
			{
				UnzipFile(zip, zipInStream, dirPath);
			}
			try
			{
				zipInStream.Close();
			}
			catch (Exception ex)
			{
				Debug.Log("UnZip Error");
				throw ex;
			}
		}

		private void UnzipFile(ZipEntry zip, ZipInputStream zipInStream, string dirPath)
		{
			try
			{
				//文件名不为空  
				if (!string.IsNullOrEmpty(zip.Name))
				{
					string filePath = dirPath;
					filePath += ("/" + zip.Name);

					//如果是一个新的文件路径　这里需要创建这个文件路径  
					if (IsDirectory(filePath))
					{
						if (!Directory.Exists(filePath))
						{
							Directory.CreateDirectory(filePath);
						}
					}
					else
					{
						FileStream fs = null;
						//当前文件夹下有该文件  删掉  重新创建  
						if (File.Exists(filePath))
						{
							File.Delete(filePath);
						}
						fs = File.Create(filePath);
						int size = 2048;
						byte[] data = new byte[2048];
						//每次读取2MB  直到把这个内容读完  
						while (true)
						{
							size = zipInStream.Read(data, 0, data.Length);
							//小于0， 也就读完了当前的流  
							if (size > 0)
							{
								fs.Write(data, 0, size);
							}
							else
							{
								break;
							}
						}
						fs.Close();
					}
				}
			}
			catch (Exception e)
			{
				throw new Exception();
			}
		}
			
		private bool IsDirectory(string path)
		{

			if (path[path.Length - 1] == '/')
			{
				return true;
			}
			return false;
		}
	}

}