using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniFramework.Event;
using UniFramework.Singleton;
using YooAsset;

public class Boot : MonoBehaviour
{
	/// <summary>
	/// 资源系统运行模式
	/// </summary>
	public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

	void Awake()
	{
		Debug.Log($"资源系统运行模式：{PlayMode}");
		Application.targetFrameRate = 60;
		Application.runInBackground = true;
	}
	void Start()
	{
		// 初始化事件系统
		UniEvent.Initalize();

		// 初始化单例系统
		UniSingleton.Initialize();

		// 初始化资源系统
		YooAssets.Initialize();
		YooAssets.SetOperationSystemMaxTimeSlice(30);

		// 创建补丁管理器
		UniSingleton.CreateSingleton<PatchManager>();

		// 开始补丁更新流程
		PatchManager.Instance.Run(PlayMode);
	}
	
	public async UniTask Example(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
	{
		var handle = YooAssets.LoadAssetAsync<GameObject>("Assets/Res/Prefabs/TestImg.prefab");

		await handle.ToUniTask(progress, timing);

		var obj = handle.AssetObject as GameObject;
		var go  = Instantiate(obj, transform);

		go.transform.localPosition = Vector3.zero;
		go.transform.localScale    = Vector3.one;
	}
}