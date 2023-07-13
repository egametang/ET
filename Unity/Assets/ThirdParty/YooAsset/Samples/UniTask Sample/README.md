# UniTask 扩展

这里为了照顾新手使用，做了一些妥协，有定制需求的需要手动调整一下

## 代码示例

```csharp
public async UniTask Example(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
{
    var handle = YooAssets.LoadAssetAsync<GameObject>("Assets/Res/Prefabs/TestImg.prefab");

    await handle.ToUniTask(progress, timing);

    var obj = handle.AssetObject as GameObject;
    var go  = Instantiate(obj, transform);

    go.transform.localPosition = Vector3.zero;
    go.transform.localScale    = Vector3.one;
}
```

## 初学者教程

**如果你弄不明白 asmdef 文件到底是啥，就按照下发内容操作**

- 将 `Samples/UniTask Sample/UniTask` 文件夹拷入游戏中
- 如果项目有 `asmdef`，则引用 `UniTask` 和 `YooAsset`，如果没有，就不用关心这一步


## 项目定制教程

- 请去下载 [UniTask](https://github.com/Cysharp/UniTask) 源码
    - 注意不要用 `Sample` 里面的  `UniTask` 这个是专门给新手定制的
- 将 `Samples/UniTask Sample/UniTask/Runtime/External/YooAsset` 文件夹拷贝到 `UniTask/Runtime/External/YooAsset` 中
- 创建 `UniTask.YooAsset.asmdef` 文件
- 添加 `UniTask` 和 `YooAsset` 的引用 
- 在 UniTask `_InternalVisibleTo.cs` 文件中增加 `[assembly: InternalsVisibleTo("UniTask.YooAsset")]` 后即可使用

## 有效性检查

一般使用项目定制时, 会出现如下警告, 这说明项目没有配置正确, 建议使用 **初学者定制的** 版本

```
yield BundledSceneProvider is not supported on await IEnumerator or Enumerator. ToUniTaskO, please use ToUniTask MonoBehaviou
coroutine Runner) instead
```

- 在 IDE 中点击 ToUniTask 跳转代码, 看是否可以正确跳转到 `UniTask/Runtime/External/YooAsset` 文件夹中
- 增加 `handle.ToUniTask(progress, timing)` 参数, 看是否有编译错误

如果不正确, 需要检查业务逻辑的 `asmdef` 是否引用正确, 假设你项目业务逻辑的 `asmdef` 名为 `View.asmdef`, 那么在 `View` 中, 要包含如下引用

- YooAsset
- UniTask
- UniTask.YooAsset

如果引用正确, 依然还有报错, 说明定制流程有问题, 请检查定制内容是否正确, 或者使用 **初学者定制的** 版本