using System;
using System.Collections.Generic;
using ET;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [InfoBox("提示: 可用事件参数 <参数1:Unity.GameObj(被控制的对象),bool(激活状态)>")]
    [LabelText("显隐<Unity.GameObj,bool>")]
    [AddComponentMenu("YIUIBind/TaskEvent/显隐 【Active】 UITaskEventBindActive")]
    public class UITaskEventBindActive: UIEventBind
    {
        protected override bool IsTaskEvent => true;

        [NonSerialized]
        private readonly List<EUIEventParamType> m_FilterParamType = new List<EUIEventParamType>
        {
            EUIEventParamType.UnityGameObject, EUIEventParamType.Bool,
        };

        protected override List<EUIEventParamType> GetFilterParamType => m_FilterParamType;
        
        private void OnEnable()
        {
            try
            {
                OnTaskEvent(true).Coroutine();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        private void OnDisable()
        {
            try
            {
                OnTaskEvent(false).Coroutine();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        private async ETTask OnTaskEvent(bool value)
        {
            await m_UIEvent?.InvokeAsync(gameObject, value);
        }
    }
}