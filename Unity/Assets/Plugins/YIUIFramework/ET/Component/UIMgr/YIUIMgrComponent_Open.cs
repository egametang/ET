using System.Collections.Generic;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public partial class YIUIMgrComponent
    {
        /// <summary>
        /// 所有已经打开过的UI
        /// K = C#文件名
        /// 主要是作为缓存PanelInfo
        /// </summary>
        internal readonly Dictionary<string, PanelInfo> m_PanelCfgMap = new Dictionary<string, PanelInfo>();

        /// <summary>
        /// 获取PanelInfo
        /// 没有则创建  相当于一个打开过了 UI基础配置档
        /// 这个根据BindVo创建  为什么没有直接用VO  因为里面有Panel 实例对象
        /// 这个k 根据resName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal PanelInfo GetPanelInfo<T>() where T : Entity
        {
            var type = typeof (T);
            var name = type.Name;
            if (this.m_PanelCfgMap.TryGetValue(name, out var info))
            {
                return info;
            }

            var data = YIUIBindHelper.GetBindVoByType(type);
            if (data == null) return null;
            var vo = data.Value;

            if (vo.CodeType != EUICodeType.Panel)
            {
                Log.Error($"这个对象不是 Panel 无法打开 {typeof (T)}");
                return null;
            }

            m_PanelCfgMap.Add(name, new PanelInfo(vo));

            return m_PanelCfgMap[name];
        }

        /// <summary>
        /// 获取PanelInfo
        /// 没有则创建  相当于一个打开过了 UI基础配置档
        /// 这个根据BindVo创建  为什么没有直接用VO  因为里面有Panel 实例对象
        /// 这个k 根据resName
        /// </summary>
        internal PanelInfo GetPanelInfo(string name)
        {
            if (this.m_PanelCfgMap.TryGetValue(name, out var info))
            {
                return info;
            }

            var data = YIUIBindHelper.GetBindVoByResName(name);
            if (data == null) return null;
            var vo = data.Value;

            if (vo.CodeType != EUICodeType.Panel)
            {
                Log.Error($"这个对象不是 Panel 无法打开 {name}");
                return null;
            }

            m_PanelCfgMap.Add(name, new PanelInfo(vo));

            return m_PanelCfgMap[name];
        }

        /// <summary>
        /// 获取UI名称 用字符串开界面 不用类型 减少GC
        /// 另外也方便之后有可能需要的扩展 字符串会更好使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal string GetPanelName<T>() where T : Entity
        {
            var panelInfo = GetPanelInfo<T>();
            return panelInfo?.Name;
        }

        internal async ETTask<PanelInfo> OpenPanelStartAsync(string panelName, Entity parentEntity)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            if (!m_PanelCfgMap.TryGetValue(panelName, out var info))
            {
                Debug.LogError($"请检查 {panelName} 没有获取到PanelInfo  1. panel上使用特性 [YIUI(typeof(XXPanelComponent))]  2. 检查是否没有注册上");
                return null;
            }

            #if YIUIMACRO_PANEL_OPENCLOSE
            Debug.Log($"<color=yellow> 打开UI: {panelName} </color>");
            #endif

            EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelOpenBefore
            {
                UIPkgName = info.PkgName, UIResName = info.ResName, UIComponentName = info.Name, PanelLayer = info.PanelLayer,
            });

            if (info.UIBase == null)
            {
                if (PanelIsOpening(panelName))
                {
                    Debug.LogError($"请检查 {panelName} 正在异步打开中 请勿重复调用 请检查代码是否一瞬间频繁调用");
                    return null;
                }

                AddOpening(panelName);
                var uiCom = await YIUIFactory.CreatePanelAsync(info, parentEntity);
                RemovOpening(panelName);
                if (uiCom == null)
                {
                    Debug.LogError($"面板[{panelName}]没有创建成功，packName={info.PkgName}, resName={info.ResName}");
                    return null;
                }

                var uiBase = uiCom.GetParent<YIUIComponent>();
                uiBase.SetActive(false);
                info.ResetUI(uiBase);
                info.ResetEntity(uiCom);
            }

            AddUI(info);

            return info;
        }

        /// <summary>
        /// 打开之前
        /// </summary>
        internal async ETTask OpenPanelBefore(PanelInfo info)
        {
            if (!info.UIWindow.WindowFitstOpen)
            {
                await AddUICloseElse(info);
            }
        }

        /// <summary>
        /// 打开之后
        /// </summary>
        internal async ETTask OpenPanelAfter(PanelInfo info, bool success)
        {
            if (success)
            {
                if (info.UIWindow.WindowFitstOpen)
                {
                    await AddUICloseElse(info);
                }
            }
            else
            {
                #if YIUIMACRO_PANEL_OPENCLOSE
                Debug.Log($"<color=yellow> 打开UI失败: {info.ResName} </color>");
                #endif

                //如果打开失败直接屏蔽
                info?.UIBase?.SetActive(false);
                info?.UIPanel?.Close();
            }

            EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelOpenAfter
            {
                Success         = success,
                UIPkgName       = info.PkgName,
                UIResName       = info.ResName,
                UIComponentName = info.Name,
                PanelLayer      = info.PanelLayer,
            });
        }

        #region opening

        internal HashSet<string> m_PanelOpening = new HashSet<string>();

        internal void AddOpening(string name)
        {
            m_PanelOpening.Add(name);
        }

        internal void RemovOpening(string name)
        {
            m_PanelOpening.Remove(name);
        }

        internal bool PanelIsOpening(string name)
        {
            return m_PanelOpening.Contains(name);
        }

        #endregion
    }
}