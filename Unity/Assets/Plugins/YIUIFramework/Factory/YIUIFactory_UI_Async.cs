//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using ET;
using ET.Client;
using UnityEngine;

namespace YIUIFramework
{
    public static partial class YIUIFactory
    {
        public static async ETTask<T> InstantiateAsync<T>(Entity parentEntity, RectTransform parent = null) where T : Entity
        {
            var data = YIUIBindHelper.GetBindVoByType<T>();
            if (data == null) return null;
            var vo = data.Value;

            return await InstantiateAsync<T>(vo, parentEntity, parent);
        }

        public static async ETTask<T> InstantiateAsync<T>(YIUIBindVo vo, Entity parentEntity, RectTransform parent = null) where T : Entity
        {
            var uiCom = await CreateAsync(vo, parentEntity);
            SetParent(uiCom.GetParent<YIUIComponent>().OwnerRectTransform, parent? parent : YIUIMgrComponent.Inst.UICache);
            return (T)uiCom;
        }

        public static async ETTask<Entity> InstantiateAsync(YIUIBindVo vo, Entity parentEntity, RectTransform parent = null)
        {
            var uiCom = await CreateAsync(vo, parentEntity);
            SetParent(uiCom.GetParent<YIUIComponent>().OwnerRectTransform, parent? parent : YIUIMgrComponent.Inst.UICache);
            return uiCom;
        }

        public static async ETTask<Entity> InstantiateAsync(Type uiType, Entity parentEntity, RectTransform parent = null)
        {
            var data = YIUIBindHelper.GetBindVoByType(uiType);
            if (data == null) return null;
            var vo = data.Value;

            return await InstantiateAsync(vo, parentEntity, parent);
        }

        public static async ETTask<Entity> InstantiateAsync(string        pkgName, string resName, Entity parentEntity,
                                                             RectTransform parent = null)
        {
            var data = YIUIBindHelper.GetBindVoByPath(pkgName, resName);
            if (data == null) return null;
            var vo = data.Value;

            return await InstantiateAsync(vo, parentEntity, parent);
        }

        public static async ETTask<Entity> CreatePanelAsync(PanelInfo panelInfo, Entity parentEntity)
        {
            var bingVo = YIUIBindHelper.GetBindVoByPath(panelInfo.PkgName, panelInfo.ResName);
            if (bingVo == null) return null;
            var uiCom = await CreateAsync(bingVo.Value, parentEntity);
            return uiCom;
        }

        public static async ETTask<Entity> CreateAsync(YIUIBindVo vo, Entity parentEntity)
        {
            var obj = await YIUILoadHelper.LoadAssetAsyncInstantiate(vo.PkgName, vo.ResName);
            if (obj == null)
            {
                Debug.LogError($"没有加载到这个资源 {vo.PkgName}/{vo.ResName}");
                return null;
            }

            return CreateByObjVo(vo, obj, parentEntity);
        }
    }
}