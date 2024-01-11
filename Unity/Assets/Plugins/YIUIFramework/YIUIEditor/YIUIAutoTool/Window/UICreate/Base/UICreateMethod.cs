#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace YIUIFramework.Editor
{
    /// <summary>
    /// 事件方法
    /// </summary>
    public static class UICreateMethod
    {
        public static string Get(UIBindCDETable cdeTable)
        {
            var sb = SbPool.Get();
            cdeTable.GetEventTable(sb);
            return SbPool.PutAndToStr(sb);
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
                sb.AppendFormat("        protected virtual void {0}({1}){{}}\r\n",
                    $"OnEvent{name.Replace($"{NameUtility.FirstName}{NameUtility.EventName}", "")}Action",
                    GetEventMethodParam(uiEventBase));
            }
        }

        private static string GetEventMethodParam(UIEventBase uiEventBase)
        {
            var paramCount = uiEventBase.AllEventParamType.Count;
            if (paramCount <= 0)
            {
                return "";
            }

            var sb = SbPool.Get();

            for (int i = 0; i < paramCount; i++)
            {
                var paramType = uiEventBase.AllEventParamType[i];
                sb.AppendFormat("{0} p{1}", paramType.GetParamTypeString(), i + 1);
                if (i < paramCount - 1)
                {
                    sb.Append(",");
                }
            }

            return SbPool.PutAndToStr(sb);
        }

        private static string GetEventSystemMethodParam(UIEventBase uiEventBase)
        {
            var paramCount = uiEventBase.AllEventParamType.Count;
            if (paramCount <= 0)
            {
                return "";
            }

            var sb = SbPool.Get();

            for (int i = 0; i < paramCount; i++)
            {
                var paramType = uiEventBase.AllEventParamType[i];
                sb.AppendFormat("p{0}", i + 1);
                if (i < paramCount - 1)
                {
                    sb.Append(",");
                }
            }

            return SbPool.PutAndToStr(sb);
        }

        //都会有的默认事件 这个不允许修改 强制全部重写
        public static Dictionary<string, List<Dictionary<string, string>>> CoverSystemDefaultEventDic(UIBindCDETable cdeTable)
        {
            if (cdeTable == null) return null;

            var overrideDic = new Dictionary<string, List<Dictionary<string, string>>>();

            #region DefaultComponentSystem开始

            var newList = new List<Dictionary<string, string>>();
            overrideDic.Add("DefaultComponentSystem", newList);

            var template    = $"{UIStaticHelper.UITemplatePath}/UICreateDefaultComponentSystemTemplate.txt";
            var path        = EditorHelper.GetProjPath(template);
            var templateStr = "";
            try
            {
                templateStr = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError("读取文件失败: " + e);
                return overrideDic;
            }

            var content = templateStr.Replace("${ResName}", cdeTable.ResName);

            newList.Add(new Dictionary<string, string> { { "不可能通过检查的判断强制刷新", content } });

            #endregion DefaultComponentSystem结束

            return overrideDic;
        }

        /// <summary>
        /// 子类 帮助直接写上重写事件
        /// </summary>
        public static Dictionary<string, List<Dictionary<string, string>>> GetEventOverrideDic(UIBindCDETable cdeTable)
        {
            var tab = cdeTable.EventTable;
            if (tab == null) return null;

            var overrideDic = new Dictionary<string, List<Dictionary<string, string>>>();

            #region YIUIEvent开始

            var newList = new List<Dictionary<string, string>>();
            overrideDic.Add("YIUIEvent", newList);
            
            foreach (var value in tab.EventDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var uiEventBase = value.Value;
                if (uiEventBase == null) continue;
                var onEvent           = $"OnEvent{name.Replace($"{NameUtility.FirstName}{NameUtility.EventName}", "")}";
                var methodParam       = $"Action({GetEventMethodParam(uiEventBase)})";
                var check             = $"{onEvent}{methodParam}";
                var firstContent      = $"\r\n        protected override void {onEvent}{methodParam}";
                var systemMethodParam = $"Action({GetEventSystemMethodParam(uiEventBase)})";
                var systemMethod      = $"this.UIComponent.{onEvent}{systemMethodParam};";
                var content           = $"{firstContent}\r\n        {{\r\n            {systemMethod}\r\n        }}\r\n        ";
                newList.Add(new Dictionary<string, string> { { check, content } });
            }

            #endregion YIUIEvent结束

            return overrideDic;
        }

        /// <summary>
        /// system类 帮助直接写上事件
        /// </summary>
        public static Dictionary<string, List<Dictionary<string, string>>> GetSystemEventOverrideDic(UIBindCDETable cdeTable)
        {
            var tab = cdeTable.EventTable;
            if (tab == null) return null;

            var overrideDic = new Dictionary<string, List<Dictionary<string, string>>>();

            #region YIUIEvent开始

            var newList = new List<Dictionary<string, string>>();
            overrideDic.Add("YIUIEvent", newList);

            foreach (var value in tab.EventDic)
            {
                var name = value.Key;
                if (string.IsNullOrEmpty(name)) continue;
                var uiEventBase = value.Value;
                if (uiEventBase == null) continue;
                var    onEvent          = $"OnEvent{name.Replace($"{NameUtility.FirstName}{NameUtility.EventName}", "")}";
                var    eventParam       = GetEventMethodParam(uiEventBase);
                var    systemEventParam = string.IsNullOrEmpty(eventParam)? "" : $", {eventParam}";
                var    methodParam      = $"Action(this {cdeTable.ResName}Component self{systemEventParam})";
                string check;
                if (uiEventBase.IsTaskEvent)
                    check = $"private static async ETTask {onEvent}{methodParam}";
                else
                    check = $"private static void {onEvent}{methodParam}";
                var firstContent = $"\r\n        {check}";
                string content;
                if (uiEventBase.IsTaskEvent)
                    content = firstContent + "\r\n        {\r\n            await ETTask.CompletedTask;\r\n        }\r\n        ";
                else
                    content = firstContent + "\r\n        {\r\n            \r\n        }\r\n        ";
                newList.Add(new Dictionary<string, string> { { check, content } });
            }

            #endregion YIUIEvent结束

            return overrideDic;
        }
    }
}
#endif