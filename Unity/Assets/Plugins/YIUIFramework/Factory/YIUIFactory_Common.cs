//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using UnityEngine;
using ET;

namespace YIUIFramework
{
    public static partial class YIUIFactory
    {
        //普通的UI预制体 创建与摧毁 一定要成对
        //为了防止忘记 所以默认自动回收
        public static GameObject InstantiateGameObject(string pkgName, string resName)
        {
            var obj = YIUILoadHelper.LoadAssetInstantiate(pkgName, resName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {pkgName}/{resName}");
                return null;
            }

            //强制添加 既然你要使用这个方法那就必须接受 否则请使用其他方式
            //被摧毁时 自动回收 无需调用 UIFactory.Destroy
            obj.AddComponent<YIUIReleaseInstantiate>();

            return obj;
        }

        public static async ETTask<GameObject> InstantiateGameObjectAsync(string pkgName, string resName)
        {
            var obj = await YIUILoadHelper.LoadAssetAsyncInstantiate(pkgName, resName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {pkgName}/{resName}");
                return null;
            }

            obj.AddComponent<YIUIReleaseInstantiate>();

            return obj;
        }
    }
}