using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class FsmPatchInit : IFsmNode
{
	public string Name { private set; get; } = nameof(FsmPatchInit);

	void IFsmNode.OnEnter()
	{
		Debug.Log("开始补丁更新...");

		FsmManager.Transition(nameof(FsmUpdateStaticVersion));
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}
}