using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace YooAsset
{
	/// <summary>
	/// 下载器
	/// 说明：UnityWebRequest(UWR) supports reading streaming assets since 2017.1
	/// </summary>
	internal class UnityWebFileRequester
	{
		protected UnityWebRequest _webRequest;
		protected UnityWebRequestAsyncOperation _operationHandle;

		/// <summary>
		/// 请求URL地址
		/// </summary>
		public string URL { private set; get; }


		/// <summary>
		/// 发送GET请求
		/// </summary>
		public void SendRequest(string url, string savePath)
		{
			if (_webRequest == null)
			{
				URL = url;
				_webRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
				DownloadHandlerFile handler = new DownloadHandlerFile(savePath);
				handler.removeFileOnAbort = true;
				_webRequest.downloadHandler = handler;
				_webRequest.disposeDownloadHandlerOnDispose = true;
				_operationHandle = _webRequest.SendWebRequest();
			}
		}

		/// <summary>
		/// 释放下载器
		/// </summary>
		public void Dispose()
		{
			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
				_operationHandle = null;
			}
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		public bool IsDone()
		{
			if (_operationHandle == null)
				return false;
			return _operationHandle.isDone;
		}

		/// <summary>
		/// 下载进度
		/// </summary>
		public float Progress()
		{
			if (_operationHandle == null)
				return 0;
			return _operationHandle.progress;
		}

		/// <summary>
		/// 下载是否发生错误
		/// </summary>
		public bool HasError()
		{
#if UNITY_2020_3_OR_NEWER
			return _webRequest.result != UnityWebRequest.Result.Success;
#else
			if (_webRequest.isNetworkError || _webRequest.isHttpError)
				return true;
			else
				return false;
#endif
		}

		/// <summary>
		/// 获取错误信息
		/// </summary>
		public string GetError()
		{
			if (_webRequest != null)
			{
				return $"URL : {URL} Error : {_webRequest.error}";
			}
			return string.Empty;
		}
	}
}