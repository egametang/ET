//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using UnityEngine;

namespace YIUIFramework
{
    public static partial class YIUIFactory
    {
        internal static void Destroy(GameObject obj)
        {
            YIUILoadHelper.ReleaseInstantiate(obj);
        }

        //内部会自动调用
        //一定要使用本类中的创建 否则会有报错提示
        internal static void Destroy(ET.Client.YIUIComponent self)
        {
            if (self.OwnerGameObject == null)
            {
                //Debug.LogError($"此UI 是空对象 请检查{self.UIBindVo.PkgName} {self.UIBindVo.ResName}");
                return;
            }

            Destroy(self.OwnerGameObject);
        }
    }
}