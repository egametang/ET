#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    [Flags]
    public enum EETLifeTpye
    {
        [LabelText("所有")]
        All = -1,
        [LabelText("默认")]
        Def = IAwake | IDestroy,
        [LabelText("无")]
        None = 0,
        [LabelText("IAwake")]
        IAwake = 1 << 1,
        [LabelText("IUpdate")]
        IUpdate = 1 << 2,
        [LabelText("IDestroy")]
        IDestroy = 1 << 3,
    }
    
    /// <summary>
    ///  自动生成ET脚本
    /// </summary>
    public class UIETScriptModule: BaseYIUIToolModule
    {
        [HideLabel]
        [BoxGroup("名称")]
        public string ComponentName;
        [HideLabel]
        [BoxGroup("描述")]
        public string ComponentDesc;
        
        [EnumToggleButtons, HideLabel]
        [BoxGroup("生命周期")]
        public EETLifeTpye LifeTpye = EETLifeTpye.Def;
        
        private const string ParentFolderPath = "Assets/Scripts/";
        
        [BoxGroup("Component路径")]
        [HideLabel]
        [FolderPath(ParentFolder = ParentFolderPath)]
        public string ComponentPath;
        private StringPrefs ComponentPathPrefs = new StringPrefs("UIETScriptModule_ComponentPath", null, "Assets/Scripts/Codes/Model");
        
        
        [BoxGroup("System路径")]
        [HideLabel]
        [FolderPath(ParentFolder = ParentFolderPath)]
        public string SystemPath;
        private StringPrefs SystemPathPrefs = new StringPrefs("UIETScriptModule_SystemPath", null, "Assets/Scripts/Codes/Hotfix");
        

        [Button("生成",50),GUIColor(0.4f, 0.8f, 1)]
        private void CreateCode()
        {
            if (string.IsNullOrEmpty(ComponentName))
            {
                UnityTipsHelper.ShowError("必须输入名称");
                return;
            }
            
            ComponentName = NameUtility.ToFirstUpper(ComponentName);
            
            if (string.IsNullOrEmpty(ComponentPath) || 
                ComponentPath.IndexOf("../", StringComparison.Ordinal) >= 0)
            {
                UnityTipsHelper.ShowError($"路径无效 请重新选择 {ComponentPath}");
                return;
            }
            
            if (string.IsNullOrEmpty(SystemPath) || 
                SystemPath.IndexOf("../", StringComparison.Ordinal) >= 0)
            {
                UnityTipsHelper.ShowError($"路径无效 请重新选择 {SystemPath}");
                return;
            }
            
            var data = new UICreateETScriptData
            {
                Namespace     = GetNamespace(),
                Name          = ComponentName,
                Desc          = ComponentDesc,
                LifeTpye      = LifeTpye,
                ComponentPath = ParentFolderPath + ComponentPath,
                SystemPath    = ParentFolderPath + SystemPath,
            };
            
            new UICreateETScriptComponentCode(out var resultComponent, YIUIAutoTool.Author, data);
            if (resultComponent)
            {
                if ((int)LifeTpye != 0)
                {
                    new UICreateETScriptSystemCode(out var resultSystem, YIUIAutoTool.Author, data);
                }
            }
            
            YIUIAutoTool.CloseWindowRefresh();
        }

        private string GetNamespace()
        {
            string scriptNamespace = "ET";
            if (ComponentPath.Contains("Client"))
            {
                scriptNamespace += ".Client";
            }
            else if(ComponentPath.Contains("Server"))
            {
                scriptNamespace += ".Server";
            }
            return scriptNamespace;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            ComponentPath = ComponentPathPrefs.Value;
            SystemPath    = SystemPathPrefs.Value;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ComponentPathPrefs.Value = ComponentPath;
            SystemPathPrefs.Value    = SystemPath;
        }
    }
    
}
#endif