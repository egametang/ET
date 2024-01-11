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
        public static T Instantiate<T>(Entity parentEntity, RectTransform parent = null) where T : Entity
        {
            var data = YIUIBindHelper.GetBindVoByType<T>();
            if (data == null) return null;
            var vo = data.Value;

            return Instantiate<T>(vo, parentEntity, parent);
        }

        public static T Instantiate<T>(YIUIBindVo vo, Entity parentEntity, RectTransform parent = null) where T : Entity
        {
            var instance = (T)Create(vo, parentEntity);
            if (instance == null) return null;

            SetParent(instance.GetParent<YIUIComponent>().OwnerRectTransform, parent? parent : YIUIMgrComponent.Inst.UICache);

            return instance;
        }

        public static Entity Instantiate(Type uiType, Entity parentEntity, RectTransform parent = null)
        {
            var data = YIUIBindHelper.GetBindVoByType(uiType);
            if (data == null) return null;
            var vo = data.Value;

            return Instantiate(vo, parentEntity, parent);
        }

        public static Entity Instantiate(YIUIBindVo vo, Entity parentEntity, RectTransform parent = null)
        {
            var instance = Create(vo, parentEntity);
            if (instance == null) return null;

            SetParent(instance.GetParent<YIUIComponent>().OwnerRectTransform, parent? parent : YIUIMgrComponent.Inst.UICache);

            return instance;
        }

        public static Entity Instantiate(string pkgName, string resName, Entity parentEntity, RectTransform parent = null)
        {
            var data = YIUIBindHelper.GetBindVoByPath(pkgName, resName);
            if (data == null) return null;
            var vo = data.Value;

            return Instantiate(vo, parentEntity, parent);
        }
    }
}