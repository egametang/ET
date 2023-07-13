using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;

/// <summary>
/// 清理未使用的缓存文件
/// </summary>
internal class FsmClearCache : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("清理未使用的缓存文件！");
		var package = YooAsset.YooAssets.GetPackage("DefaultPackage");
		var operation = package.ClearUnusedCacheFilesAsync();
		operation.Completed += Operation_Completed;
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	private void Operation_Completed(YooAsset.AsyncOperationBase obj)
	{
		_machine.ChangeState<FsmPatchDone>();
	}
}