using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using YIUIFramework;
using Object = UnityEngine.Object;

namespace I2.Loc
{
    [RequireComponent(typeof(LanguageSource))]
    public class I2LocalizeMgr : MonoSingleton<I2LocalizeMgr>, IResourceManager_Bundles
    {
        [SerializeField]
        [ReadOnly]
        private LanguageSource m_LanguageSource;

        private LanguageSourceData m_SourceData => m_LanguageSource.SourceData;

        private List<string> m_AllLanguage = new List<string>();

        [SerializeField]
        private bool m_UseRuntimeModule = false; //模拟平台运行时 编辑器资源不加载

        [SerializeField]
        #if UNITY_EDITOR
        [ValueDropdown("GetAllLanguageKeys")]
        [DisableIf("OnValueChangeIf")]
        #endif
        private string m_DefaultLanguage = "Chinese";

        [ShowInInspector]
        #if UNITY_EDITOR
        [EnableIf("OnValueChangeIf")]
        [ValueDropdown("GetAllLanguageKeys")]
        [OnValueChanged("OnValueChangedCurrentLanguage")]
        #endif
        private string m_CurrentLanguage;

        #region ResourceManager_Bundles

        public void OnEnable()
        {
            if (!ResourceManager.pInstance.mBundleManagers.Contains(this))
            {
                ResourceManager.pInstance.mBundleManagers.Add(this);
            }
        }

        public void OnDisable()
        {
            ResourceManager.pInstance.mBundleManagers.Remove(this);
        }

        public virtual Object LoadFromBundle(string path, Type assetType)
        {
            var assetObject = YIUILoadHelper.LoadAsset(path, assetType);
            if (assetObject != null) return assetObject;
            Debug.LogError($"没有加载到目标 {path}  类型 {assetType.Name}");
            return null;
        }

        #endregion

        #region Editor

        #if UNITY_EDITOR

        private bool OnValueChangeIf()
        {
            return Application.isPlaying;
        }

        private void OnValueChangedCurrentLanguage()
        {
            var tempLanguage = m_CurrentLanguage;
            m_CurrentLanguage = "";
            SetLanguage(tempLanguage);
        }

        private IEnumerable<string> GetAllLanguageKeys()
        {
            var allLanguage = new List<string>();

            foreach (var language in LocalizationManager.GetAllLanguages())
            {
                var newLanguage = Regex.Replace(language, @"[\r\n]", "");
                allLanguage.Add(newLanguage);
            }

            return allLanguage;
        }
        
        #endif

        #endregion

        protected override bool GetHideAndDontSave()
        {
            return false;
        }
        
        protected override async ETTask<bool> MgrAsyncInit()
        {
            if (string.IsNullOrEmpty(m_DefaultLanguage))
            {
                //TODO 这里也可以读取上一次选择的语言
                //TODO 初始化时还需要配合如果没有这个语言需要从服务器拉取的情况
                //TODO 也可以在语言设置界面 如果设置某个语言 发现没有这些数据 当时就加载 然后重启游戏
                Debug.LogError($"必须设置默认语言");
                return false;
            }

            m_LanguageSource = this.GetComponent<LanguageSource>();
            
            #if UNITY_EDITOR
            if (!m_UseRuntimeModule)
            {
                LocalizationManager.RegisterSourceInEditor();
                UpdateAllLanguages();
                SetLanguage(m_DefaultLanguage);
            }
            else
            {
                m_SourceData.Awake();
                await LoadLanguage(m_DefaultLanguage, true);
            }
            #else
                m_SourceData.Awake();
                await LoadLanguage(m_DefaultLanguage, true);
            #endif

            return true;
        }
        
        //根据需求可提前加载语言
        public async ETTask LoadLanguage(string language, bool setCurrent = false)
        {
            #if UNITY_EDITOR
            if (!m_UseRuntimeModule)
            {
                Debug.LogError($"禁止在此模式下 动态加载语言 {language}");
                return;
            }
            #endif

            if (CheckLanguage(language))
            {
                Debug.LogError($"当前语言已存在 请勿重复加载 {language}");
                return;
            }
            
            var assetName = GetLanguageAssetName(language);

            var assetTextAsset = await YIUILoadHelper.LoadAssetAsync<TextAsset>(assetName);
            if (assetTextAsset == null)
            {
                Debug.LogError($"没有加载到目标语言资源 {language}");
                return;
            }

            Debug.Log($"加载语言成功 {language}");
            
            UseLocalizationCSV(assetTextAsset.text, !setCurrent);
            if (setCurrent)
            {
                SetLanguage(language);
            }

            //语言加载完毕后就可以释放资源了
            YIUILoadHelper.Release(assetTextAsset);
        }

        private string GetLanguageAssetName(string language)
        {
            return $"{I2LocalizeHelper.I2ResAssetNamePrefix}{language}";
        }

        private void UseLocalizationCSV(string text, bool isLocalizeAll = false)
        {
            m_SourceData.Import_CSV(string.Empty, text, eSpreadsheetUpdateMode.Replace, ',');
            if (isLocalizeAll)
            {
                LocalizationManager.LocalizeAll(); // 强制使用新数据本地化所有启用的标签/精灵
            }

            UpdateAllLanguages();
        }

        private void UpdateAllLanguages()
        {
            m_AllLanguage.Clear();
            foreach (var language in LocalizationManager.GetAllLanguages())
            {
                var newLanguage = Regex.Replace(language, @"[\r\n]", "");
                m_AllLanguage.Add(newLanguage);
            }
        }

        public bool CheckLanguage(string language)
        {
            return m_AllLanguage.Contains(language);
        }

        //运行时注意 需要提前加载你需要的所有语言
        public bool SetLanguage(string language, bool load = false)
        {
            if (!CheckLanguage(language))
            {
                if (load)
                {
                    LoadLanguage(language, true).Coroutine();
                    return true;
                }

                Debug.LogError($"当前没有这个语言无法切换到此语言 {language}");
                return false;
            }

            if (m_CurrentLanguage == language)
            {
                return true;
            }

            Debug.Log($"设置当前语言 = {language}");
            LocalizationManager.CurrentLanguage = language;
            m_CurrentLanguage                   = language;
            return true;
        }

        public bool SetLanguage(int id)
        {
            if (id < 0 || id >= m_AllLanguage.Count)
            {
                Debug.LogError($"错误的语言ID 无法设定 请检查 {id}  Language.Count = {m_AllLanguage.Count}");
                return false;
            }

            var language = m_AllLanguage[id];
            return SetLanguage(language);
        }
    }
}