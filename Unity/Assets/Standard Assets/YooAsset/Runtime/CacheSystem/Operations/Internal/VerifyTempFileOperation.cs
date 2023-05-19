using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
	internal abstract class VerifyTempFileOperation : AsyncOperationBase
	{
		public EVerifyResult VerifyResult { protected set; get; }

		public static VerifyTempFileOperation CreateOperation(VerifyTempElement element)
		{
#if UNITY_WEBGL
			var operation = new VerifyTempFileWithoutThreadOperation(element);
#else
			var operation = new VerifyTempFileWithThreadOperation(element);
#endif
			return operation;
		}
	}

	/// <summary>
	/// 下载文件验证（线程版）
	/// </summary>
	internal class VerifyTempFileWithThreadOperation : VerifyTempFileOperation
	{
		private enum ESteps
		{
			None,
			VerifyFile,
			Waiting,
			Done,
		}

		private readonly VerifyTempElement _element;
		private ESteps _steps = ESteps.None;

		public VerifyTempFileWithThreadOperation(VerifyTempElement element)
		{
			_element = element;
		}
		internal override void Start()
		{
			_steps = ESteps.VerifyFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.VerifyFile)
			{
				if (BeginVerifyFileWithThread(_element))
				{
					_steps = ESteps.Waiting;
				}
			}

			if (_steps == ESteps.Waiting)
			{
				if (_element.IsDone == false)
					return;

				VerifyResult = _element.Result;
				if (_element.Result == EVerifyResult.Succeed)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Failed verify file : {_element.TempDataFilePath} ! ErrorCode : {_element.Result}";
				}
			}
		}

		private bool BeginVerifyFileWithThread(VerifyTempElement element)
		{
			return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), element);
		}
		private void VerifyInThread(object obj)
		{
			VerifyTempElement element = (VerifyTempElement)obj;
			element.Result = CacheSystem.VerifyingTempFile(element);
			element.IsDone = true;
		}
	}

	/// <summary>
	/// 下载文件验证（非线程版）
	/// </summary>
	internal class VerifyTempFileWithoutThreadOperation : VerifyTempFileOperation
	{
		private enum ESteps
		{
			None,
			VerifyFile,
			Done,
		}

		private readonly VerifyTempElement _element;
		private ESteps _steps = ESteps.None;

		public VerifyTempFileWithoutThreadOperation(VerifyTempElement element)
		{
			_element = element;
		}
		internal override void Start()
		{
			_steps = ESteps.VerifyFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.VerifyFile)
			{
				_element.Result = CacheSystem.VerifyingTempFile(_element);
				_element.IsDone = true;

				VerifyResult = _element.Result;
				if (_element.Result == EVerifyResult.Succeed)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Failed verify file : {_element.TempDataFilePath} ! ErrorCode : {_element.Result}";
				}
			}
		}
	}
}