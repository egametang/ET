# UniFramework.Pooling

一个功能强大的游戏对象池系统。

该系统依赖于YooAsset资源系统，支持各类异步编程，支持同步接口和异步接口。

```c#
using UnityEngine;
using YooAsset;
using UniFramework.Pooling;

IEnumerator Start()
{
    // 初始化游戏对象池系统
    UniPooling.Initalize();
    
    // 创建孵化器
    var spawner = UniPooling.CreateSpawner("DefaultPackage");
    
    // 创建Cube预制体的对象池
    var operation = spawner.CreateGameObjectPoolAsync("Cube.prefab");
    yield return operation;
    
    // 孵化Cube游戏对象
    SpawnHandle handle = spawner.SpawnAsync("Cube.prefab");
    yield return handle;  
    Debug.Log(handle.GameObj.name);
    
    // 回收游戏对象
    handle.Restore();
    
    // 丢弃游戏对象
    handle.Discard();
}
```

