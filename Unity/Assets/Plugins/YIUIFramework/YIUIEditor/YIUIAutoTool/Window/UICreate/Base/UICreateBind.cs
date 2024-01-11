#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YIUIFramework.Editor
{
    /// <summary>
    /// 绑定与解绑
    /// </summary>
    public static class UICreateBind
    {
        public static string GetBind(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GetComponentTable(sb);
            cdeTable.GetDataTable(sb);
            cdeTable.GetEventTable(sb);
            cdeTable.GetCDETable(sb);
            return SbPool.PutAndToStr(sb);
        }

        private static void GetComponentTable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.ComponentTable;
            if (tab == null) return;

            foreach (var value in tab.AllBindDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var bindCom = value.Value;
                if (bindCom == null) continue;
                sb.AppendFormat("            self.{1} = self.UIBase.ComponentTable.FindComponent<{0}>(\"{1}\");\r\n", bindCom.GetType(),
                    name);
            }
        }

        private static void GetDataTable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.DataTable;
            if (tab == null) return;

            foreach (var value in tab.DataDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var uiData    = value.Value;
                var dataValue = uiData?.DataValue;
                if (dataValue == null) continue;
                sb.AppendFormat("            self.{1} = self.UIBase.DataTable.FindDataValue<{0}>(\"{1}\");\r\n", dataValue.GetType(),
                    name);
            }
        }

        private static void GetEventTable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.EventTable;
            if (tab == null) return;

            foreach (var value in tab.EventDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var uiEventBase = value.Value;
                if (uiEventBase == null) continue;
                sb.AppendFormat("            self.{1} = self.UIBase.EventTable.FindEvent<{0}>(\"{1}\");\r\n", uiEventBase.GetEventType(),
                    name);
                sb.AppendFormat("            self.{0} = self.{1}.Add(self.{2});\r\n", $"{name}Handle", name,
                    $"OnEvent{name.Replace($"{NameUtility.FirstName}{NameUtility.EventName}", "")}Action");
            }
        }

        private static void GetCDETable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.AllChildCdeTable;
            if (tab == null) return;
            var existName = new HashSet<string>();
            foreach (var value in tab)
            {
                var name = value.name;
                if (string.IsNullOrEmpty(name)) continue;
                var pkgName = value.PkgName;
                var resName = value.ResName;
                if (string.IsNullOrEmpty(resName)) continue;
                var newName = UICreateVariables.GetCDEUIName(name);
                if (existName.Contains(newName))
                {
                    Debug.LogError($"{self.name} 内部公共组件存在同名 请修改 当前会被忽略");
                    continue;
                }

                existName.Add(newName);
                sb.AppendFormat("            self.{0} = self.UIBase.CDETable.FindUIOwner<{1}>(\"{2}\");\r\n",
                    newName,
                    $"{UIStaticHelper.UINamespace}.{resName}Component",
                    name);
            }
        }
        
        public static string GetFriend(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GeFriend(sb);
            return SbPool.PutAndToStr(sb);
        }

        private static void GeFriend(this UIBindCDETable self, StringBuilder sb)
        {
            switch (self.UICodeType)
            {
                case EUICodeType.Common:
                    sb.AppendFormat("    [FriendOf(typeof(YIUIComponent))]");
                    return;
                case EUICodeType.Panel:
                    sb.AppendFormat("    [FriendOf(typeof(YIUIComponent))]\r\n");
                    sb.AppendFormat("    [FriendOf(typeof(YIUIWindowComponent))]\r\n");
                    sb.AppendFormat("    [FriendOf(typeof(YIUIPanelComponent))]");
                    break;
                case EUICodeType.View:
                    sb.AppendFormat("    [FriendOf(typeof(YIUIComponent))]\r\n");
                    sb.AppendFormat("    [FriendOf(typeof(YIUIWindowComponent))]\r\n");
                    sb.AppendFormat("    [FriendOf(typeof(YIUIViewComponent))]");
                    break;
                default:
                    Debug.LogError($"新增类型未实现 {self.UICodeType}");
                    break;
            }
        }

        public static string GetBase(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GetBaseInit(sb);
            return SbPool.PutAndToStr(sb);
        }

        private static void GetBaseInit(this UIBindCDETable self, StringBuilder sb)
        {
            switch (self.UICodeType)
            {
                case EUICodeType.Common:
                    sb.AppendFormat("            self.u_UIBase = self.GetParent<YIUIComponent>();\r\n");
                    return;
                case EUICodeType.Panel:
                    sb.AppendFormat("            self.u_UIBase = self.GetParent<YIUIComponent>();\r\n");
                    sb.AppendFormat("            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();\r\n");
                    sb.AppendFormat("            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();\r\n");
                    sb.AppendFormat("            self.UIWindow.WindowOption = EWindowOption.{0};\r\n",
                        self.WindowOption.ToString().Replace(", ", "|EWindowOption."));
                    sb.AppendFormat("            self.UIPanel.Layer = EPanelLayer.{0};\r\n",
                        self.PanelLayer);
                    sb.AppendFormat("            self.UIPanel.PanelOption = EPanelOption.{0};\r\n",
                        self.PanelOption.ToString().Replace(", ", "|EPanelOption."));
                    sb.AppendFormat("            self.UIPanel.StackOption = EPanelStackOption.{0};\r\n",
                        self.PanelStackOption);
                    sb.AppendFormat("            self.UIPanel.Priority = {0};\r\n", self.Priority);
                    if (self.PanelOption.HasFlag(EPanelOption.TimeCache))
                        sb.AppendFormat("            self.UIPanel.CachePanelTime = {0};\r\n",
                            self.CachePanelTime);
                    break;
                case EUICodeType.View:
                    sb.AppendFormat("            self.u_UIBase = self.GetParent<YIUIComponent>();\r\n");
                    sb.AppendFormat("            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();\r\n");
                    sb.AppendFormat("            self.u_UIView = self.UIBase.GetComponent<YIUIViewComponent>();\r\n");
                    sb.AppendFormat("            self.UIWindow.WindowOption = EWindowOption.{0};\r\n",
                        self.WindowOption.ToString().Replace(", ", "|EWindowOption."));
                    sb.AppendFormat("            self.UIView.ViewWindowType = EViewWindowType.{0};\r\n",
                        self.ViewWindowType);
                    sb.AppendFormat("            self.UIView.StackOption = EViewStackOption.{0};\r\n",
                        self.ViewStackOption);
                    break;
                default:
                    Debug.LogError($"新增类型未实现 {self.UICodeType}");
                    break;
            }
        }

        public static string GetUnBind(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GetUnEventTable(sb);
            return SbPool.PutAndToStr(sb);
        }

        private static void GetUnEventTable(this UIBindCDETable self, StringBuilder sb)
        {
            var tab = self.EventTable;
            if (tab == null) return;

            foreach (var value in tab.EventDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var uiEventBase = value.Value;
                if (uiEventBase == null) continue;
                sb.AppendFormat("            {0}.Remove({1});\r\n", name, $"{name}Handle");
            }
        }
    }
}
#endif