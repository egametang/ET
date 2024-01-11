using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 界面回退功能 关闭 / 恢复 / Home
    /// </summary>
    public partial class YIUIMgrComponent
    {
        /// <summary>
        /// 打开一个同级UI时关闭其他UI 
        /// 只有Panel层才有这个逻辑
        /// </summary>
        internal async ETTask AddUICloseElse(PanelInfo info)
        {
            if (!(info.UIPanel is { Layer: EPanelLayer.Panel }))
            {
                return;
            }

            if (info.UIPanel.PanelIgnoreBack)
            {
                return;
            }

            var layerList = this.GetLayerPanelInfoList(EPanelLayer.Panel);
            var skipTween = info.UIWindow.WindowSkipOtherCloseTween;

            for (var i = layerList.Count - 1; i >= 0; i--)
            {
                var child = layerList[i];

                if (child == info)
                {
                    continue;
                }

                if (child.OwnerUIEntity is IYIUIBack back)
                {
                    back.DoBackClose(info);
                }

                EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelCloseBefore
                {
                    UIPkgName       = child.PkgName,
                    UIResName       = child.ResName,
                    UIComponentName = child.Name,
                    StackOption     = true,
                    PanelLayer      = child.PanelLayer,
                });

                switch (child.UIPanel.StackOption)
                {
                    case EPanelStackOption.Omit:
                        if (skipTween)
                            child.UIPanel.Close(true, true);
                        else
                            await child.UIPanel.CloseAsync(true, true);
                        break;
                    case EPanelStackOption.None:
                        break;
                    case EPanelStackOption.Visible:
                        child.UIBase.SetActive(false);
                        break;
                    case EPanelStackOption.VisibleTween:
                        if (!skipTween)
                            await child.UIWindow.InternalOnWindowCloseTween();
                        child.UIBase.SetActive(false);
                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {child.UIPanel.StackOption}");
                        child.UIBase.SetActive(false);
                        break;
                }

                EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelCloseAfter
                {
                    UIPkgName       = child.PkgName,
                    UIResName       = child.ResName,
                    UIComponentName = child.Name,
                    StackOption     = true,
                    PanelLayer      = child.PanelLayer,
                });
            }
        }

        internal async ETTask RemoveUIAddElse(PanelInfo info)
        {
            if (!(info.UIPanel is { Layer: EPanelLayer.Panel }))
            {
                return;
            }

            if (info.UIPanel.PanelIgnoreBack)
            {
                return;
            }

            var layerList = this.GetLayerPanelInfoList(EPanelLayer.Panel);
            var skipTween = info.UIWindow.WindowSkipOtherOpenTween;

            for (var i = layerList.Count - 1; i >= 0; i--)
            {
                var child = layerList[i];

                if (child == info)
                {
                    continue;
                }

                if (child.OwnerUIEntity is IYIUIBack back)
                {
                    back.DoBackAdd(info);
                }

                EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelOpenBefore
                {
                    UIPkgName       = child.PkgName,
                    UIResName       = child.ResName,
                    UIComponentName = child.Name,
                    StackOption     = true,
                    PanelLayer      = child.PanelLayer,
                });

                var isBreak = true;
                switch (child.UIPanel.StackOption)
                {
                    case EPanelStackOption.Omit: //不可能进入这里因为他已经被关闭了 如果进入则跳过这个界面
                        isBreak = false;
                        break;
                    case EPanelStackOption.None:
                        break;
                    case EPanelStackOption.Visible:
                        child.UIBase.SetActive(true);
                        break;
                    case EPanelStackOption.VisibleTween:
                        child.UIBase.SetActive(true);
                        if (!skipTween)
                            await child.UIWindow.InternalOnWindowOpenTween();
                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {child.UIPanel.StackOption}");
                        child.UIBase.SetActive(true);
                        break;
                }

                EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelOpenAfter
                {
                    UIPkgName       = child.PkgName,
                    UIResName       = child.ResName,
                    UIComponentName = child.Name,
                    StackOption     = true,
                    PanelLayer      = child.PanelLayer,
                });

                if (isBreak)
                    break;
            }
        }

        internal async ETTask<bool> RemoveUIToHome(PanelInfo home, bool tween = true)
        {
            if (!(home.UIPanel is { Layer: EPanelLayer.Panel }))
            {
                return false; //home的UI必须在panel层
            }

            var layerList           = this.GetLayerPanelInfoList(EPanelLayer.Panel);
            var skipOtherCloseTween = home.UIWindow.WindowSkipOtherCloseTween;
            var skipHomeOpenTween   = home.UIWindow.WindowSkipHomeOpenTween;

            for (var i = layerList.Count - 1; i >= 0; i--)
            {
                var child = layerList[i];

                if (child != home)
                {
                    if (child.OwnerUIEntity is IYIUIBack back)
                    {
                        back.DoBackHome(home);
                    }

                    if (skipOtherCloseTween)
                    {
                        this.ClosePanel(child.Name, false, true);
                    }
                    else
                    {
                        var success = await this.ClosePanelAsync(child.Name, tween, true);
                        if (!success)
                        {
                            return false;
                        }
                    }

                    continue;
                }

                EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelOpenBefore
                {
                    UIPkgName       = child.PkgName,
                    UIResName       = child.ResName,
                    UIComponentName = child.Name,
                    StackOption     = true,
                    PanelLayer      = child.PanelLayer,
                });

                switch (child.UIPanel.StackOption)
                {
                    case EPanelStackOption.Omit:
                    case EPanelStackOption.None:
                    case EPanelStackOption.Visible:
                        child.UIBase.SetActive(true);
                        break;
                    case EPanelStackOption.VisibleTween:
                        child.UIBase.SetActive(true);
                        if (tween && !skipHomeOpenTween)
                            await child.UIWindow.InternalOnWindowOpenTween();
                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {child.UIPanel.StackOption}");
                        child.UIBase.SetActive(true);
                        break;
                }

                EventSystem.Instance.Publish(this.Root(), new YIUIEventPanelOpenAfter
                {
                    UIPkgName       = child.PkgName,
                    UIResName       = child.ResName,
                    UIComponentName = child.Name,
                    StackOption     = true,
                    PanelLayer      = child.PanelLayer,
                });
            }

            return true;
        }
    }
}