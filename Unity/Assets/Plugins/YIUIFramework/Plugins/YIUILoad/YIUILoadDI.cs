//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using Object = UnityEngine.Object;
using ET;

namespace YIUIFramework
{
    /// <summary>
    /// 注入加载方法
    /// </summary>
    public static partial class YIUILoadDI
    {
        //同步加载方法
        //参数1: pkgName 包名
        //参数2: resName 资源名
        //参数3: Type 需要加载的资源类型
        //返回值: obj对象
        public static Func<string, string, Type, (Object, int)> LoadAssetFunc { internal get; set; }

        //异步加载方法
        public static Func<string, string, Type, ETTask<(Object, int)>> LoadAssetAsyncFunc { internal get; set; }

        //验证是否有效
        public static Func<string, string, bool> VerifyAssetValidityFunc { internal get; set; }

        //释放方法
        public static Action<int> ReleaseAction { internal get; set; }

        //释放所有方法
        public static Action ReleaseAllAction { internal get; set; }
    }
}