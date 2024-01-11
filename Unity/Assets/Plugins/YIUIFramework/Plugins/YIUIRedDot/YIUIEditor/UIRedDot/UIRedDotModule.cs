//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    internal class UIRedDotModule : BaseYIUIToolModule
    {
        private EnumPrefs<EUIRedDotViewType> m_EUIRedDotViewTypePrefs =
            new EnumPrefs<EUIRedDotViewType>("AutoUIRedDotModule_EUIRedDotViewTypePrefs", null, EUIRedDotViewType.Key);

        private const string UIRedDotAssetFolderPath = "Assets/GameRes/RedDot";

        [LabelText("红点枚举资源路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        public const string UIRedDotKeyAssetPath = "Assets/GameRes/RedDot/RedDotKeyAsset.asset";

        [ShowInInspector]
        [ReadOnly]
        internal RedDotKeyAsset m_RedDotKeyAsset;

        [LabelText("红点枚举类路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        public string UIRedDotKeyClassPath =
            $"{UIStaticHelper.UIFrameworkPath}/Plugins/YIUIRedDot/Runtime/Key/RedDotKeyEnum.cs";

        [LabelText("红点关系配置资源路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        public const string UIRedDotConfigAssetPath = "Assets/GameRes/RedDot/RedDotConfigAsset.asset";

        [ShowInInspector]
        [ReadOnly]
        internal RedDotConfigAsset m_RedDotConfigAsset;

        [EnumToggleButtons]
        [HideLabel]
        [ShowInInspector]
        [BoxGroup("红点设置", centerLabel: true)]
        [OnValueChanged("OnValueChangedViewSetIndex")]
        private EUIRedDotViewType m_EUIRedDotViewType = EUIRedDotViewType.Key;

        internal Action<EUIRedDotViewType> OnChangeViewSetIndex;

        private void OnValueChangedViewSetIndex()
        {
            m_EUIRedDotViewTypePrefs.Value = m_EUIRedDotViewType;
            OnChangeViewSetIndex?.Invoke(m_EUIRedDotViewType);
        }

        //key界面 (增删改查)
        [ShowInInspector]
        [ShowIf("m_EUIRedDotViewType", EUIRedDotViewType.Key)]
        private UIRedDotKeyView m_KeyView;

        //配置界面 所有key之间的关系配置
        [ShowInInspector]
        [ShowIf("m_EUIRedDotViewType", EUIRedDotViewType.Config)]
        private UIRedDotConfigView m_ConfigView;

        public override void Initialize()
        {
            m_EUIRedDotViewType = m_EUIRedDotViewTypePrefs.Value;
            LoadRedDotKeyAsset();
            LoadRedDotConfigAsset();
            m_KeyView = new UIRedDotKeyView(this);
            m_KeyView.Initialize();
            m_ConfigView = new UIRedDotConfigView(this);
            m_ConfigView.Initialize();
        }

        public override void OnDestroy()
        {
            m_KeyView.OnDestroy();
            m_ConfigView.OnDestroy();
            AssetDatabase.SaveAssets();
        }

        #region Key

        private void LoadRedDotKeyAsset()
        {
            m_RedDotKeyAsset = AssetDatabase.LoadAssetAtPath<RedDotKeyAsset>(UIRedDotKeyAssetPath);
            if (m_RedDotKeyAsset == null)
            {
                CreateRedDotKeyAsset();
            }

            if (m_RedDotKeyAsset == null)
            {
                Debug.LogError($"没有找到 Key 配置资源 且自动创建失败 请检查");
            }
        }

        private void CreateRedDotKeyAsset()
        {
            m_RedDotKeyAsset = ScriptableObject.CreateInstance<RedDotKeyAsset>();

            var assetFolder = Application.dataPath + UIRedDotAssetFolderPath.Replace("Assets", "");
            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);

            AssetDatabase.CreateAsset(m_RedDotKeyAsset, UIRedDotKeyAssetPath);
        }

        #endregion

        #region Config

        private void LoadRedDotConfigAsset()
        {
            m_RedDotConfigAsset = AssetDatabase.LoadAssetAtPath<RedDotConfigAsset>(UIRedDotConfigAssetPath);
            if (m_RedDotConfigAsset == null)
            {
                CreateRedDotConfigAsset();
            }

            if (m_RedDotConfigAsset == null)
            {
                Debug.LogError($"没有找到 Config 配置资源 且自动创建失败 请检查");
            }
        }

        private void CreateRedDotConfigAsset()
        {
            m_RedDotConfigAsset = ScriptableObject.CreateInstance<RedDotConfigAsset>();

            var assetFolder = Application.dataPath + UIRedDotAssetFolderPath.Replace("Assets", "");
            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);

            AssetDatabase.CreateAsset(m_RedDotConfigAsset, UIRedDotConfigAssetPath);
        }

        #endregion

        [HideLabel]
        internal enum EUIRedDotViewType
        {
            [LabelText("枚举 Key")]
            Key = 1,

            [LabelText("配置 Config")]
            Config = 2,
        }
    }
}
#endif