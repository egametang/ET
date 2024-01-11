#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace YIUIFramework.Editor
{
    public class UII2LocalizationModule : BaseYIUIToolModule
    {
        private LanguageSourceData m_LanguageSourceData;

        [LabelText("全数据名称")]
        [ShowInInspector]
        [ReadOnly]
        public const string UII2SourceResName = "AllSource";

        [LabelText("全数据保存路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        public const string UII2SourceResPath = "Assets/Editor/I2Localization"; //这是编辑器下的数据 平台运行时 是不需要的

        [LabelText("指定数据保存路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        public const string UII2TargetLanguageResPath = "Assets/GameRes/I2Localization"; //运行时的资源是拆分的 根据需求加载

        [Button("打开多语言数据", 50)]
        [GUIColor(0.4f, 0.8f, 1)]
        private void OpenI2Languages()
        {
            EditorApplication.ExecuteMenuItem("Tools/I2 Localization/Open I2Languages.asset");
        }

        [Button("导入", 50)]
        [GUIColor(0f, 1f, 1f)]
        private void ImportAllCsvTips()
        {
            UnityTipsHelper.CallBack("确认导入当前所有多语言数据吗\n\n此操作将会覆盖现有数据\n\n请确认", ImportAllCsv);
        }

        [Button("导出", 50)]
        [GUIColor(0f, 1f, 1f)]
        private void ExportAllCsvTips()
        {
            UnityTipsHelper.CallBack("确认导出当前所有多语言数据吗\n\n此操作将会覆盖现有数据\n\n请确认", ExportAllCsv);
        }

        #region 导出

        private string GetSourceResPath()
        {
            var projPath = EditorHelper.GetProjPath(UII2SourceResPath);
            var path     = $"{projPath}/{I2LocalizeHelper.I2ResAssetNamePrefix}{UII2SourceResName}.csv";
            return path;
        }

        private void ExportAllCsv()
        {
            var editorAsset = LocalizationManager.GetEditorAsset(true);
            m_LanguageSourceData = editorAsset?.SourceData;

            if (m_LanguageSourceData == null)
            {
                UnityTipsHelper.ShowError($"没有找到多语言编辑器下的源数据 请检查 {I2LocalizeHelper.I2GlobalSourcesEditorPath}");
                return;
            }

            var path = GetSourceResPath();

            try
            {
                var content = Export_CSV(null);
                File.WriteAllText(path, content, Encoding.UTF8);
            }
            catch (Exception e)
            {
                UnityTipsHelper.ShowError($"导出全数据时发生错误 请检查");
                Debug.LogError(e);
                return;
            }

            Debug.Log($"多语言 全数据 {UII2SourceResName} 导出CSV成功 {path}");

            var projPath = EditorHelper.GetProjPath(UII2TargetLanguageResPath);
            if (!Directory.Exists(projPath))
            {
                Directory.CreateDirectory(projPath);
            }
            foreach (var languages in m_LanguageSourceData.mLanguages)
            {
                var targetPath = "";

                try
                {
                    var content = Export_CSV(languages.Name);
                    targetPath = $"{projPath}/{I2LocalizeHelper.I2ResAssetNamePrefix}{languages.Name}.csv";
                    File.WriteAllText(targetPath, content, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    UnityTipsHelper.ShowError($"导出指定数据时发生错误 {languages.Name} 请检查 ");
                    Debug.LogError(e);
                    return;
                }

                Debug.Log($"多语言 指定数据 {languages.Name} 导出CSV成功 {targetPath}");
            }

            UnityTipsHelper.Show($"导出全数据完成 {path}");
            YIUIAutoTool.CloseWindowRefresh();
        }

        #region 导出方法

        private string Export_CSV(string selectLanguage)
        {
            char Separator = ',';
            var  Builder   = new StringBuilder();

            var languages      = m_LanguageSourceData.mLanguages;
            var languagesCount = languages.Count;
            Builder.AppendFormat("Key{0}Type{0}Desc", Separator);
            var currentLanguageIndex = -1;

            for (int i = 0; i < languagesCount; i++)
            {
                var langData = languages[i];

                var currentLanguage = GoogleLanguages.GetCodedLanguage(langData.Name, langData.Code);

                if (!string.IsNullOrEmpty(selectLanguage) && currentLanguage != selectLanguage)
                {
                    continue;
                }

                Builder.Append(Separator);
                if (!langData.IsEnabled())
                    Builder.Append('$');
                AppendString(Builder, currentLanguage, Separator);
                currentLanguageIndex = i;
            }

            if (string.IsNullOrEmpty(selectLanguage))
            {
                currentLanguageIndex = -1;
            }

            Builder.Append("\n");

            var terms = m_LanguageSourceData.mTerms;

            if (string.IsNullOrEmpty(selectLanguage))
            {
                terms.Sort((a, b) => string.CompareOrdinal(a.Term, b.Term));
            }

            foreach (var termData in terms)
            {
                var term = termData.Term;

                foreach (var specialization in termData.GetAllSpecializations())
                    AppendTerm(Builder, currentLanguageIndex, term, termData, specialization, Separator);
            }

            return Builder.ToString();
        }

        private static void AppendTerm(StringBuilder Builder, int selectLanguageIndex, string Term, TermData termData,
                                       string        specialization, char Separator)
        {
            //--[ Key ] --------------				
            AppendString(Builder, Term, Separator);

            if (!string.IsNullOrEmpty(specialization) && specialization != "Any")
                Builder.AppendFormat("[{0}]", specialization);

            //--[ Type and Description ] --------------
            Builder.Append(Separator);
            Builder.Append(termData.TermType.ToString());
            Builder.Append(Separator);
            AppendString(Builder, selectLanguageIndex <= -1 ? termData.Description : "", Separator);

            var startIndex = selectLanguageIndex <= -1 ? 0 : selectLanguageIndex;
            var maxIndex   = selectLanguageIndex <= -1 ? termData.Languages.Length : selectLanguageIndex + 1;

            //--[ Languages ] --------------
            for (var i = startIndex; i < maxIndex; ++i)
            {
                Builder.Append(Separator);

                var translation = termData.Languages[i];
                if (!string.IsNullOrEmpty(specialization))
                    translation = termData.GetTranslation(i, specialization);

                AppendTranslation(Builder, translation, Separator, null);
            }

            Builder.Append("\n");
        }

        private static void AppendString(StringBuilder Builder, string Text, char Separator)
        {
            if (string.IsNullOrEmpty(Text))
                return;
            Text = Text.Replace("\\n", "\n");
            if (Text.IndexOfAny((Separator + "\n\"").ToCharArray()) >= 0)
            {
                Text = Text.Replace("\"", "\"\"");
                Builder.AppendFormat("\"{0}\"", Text);
            }
            else
            {
                Builder.Append(Text);
            }
        }

        private static void AppendTranslation(StringBuilder Builder, string Text, char Separator, string tags)
        {
            if (string.IsNullOrEmpty(Text))
                return;
            Text = Text.Replace("\\n", "\n");
            if (Text.IndexOfAny((Separator + "\n\"").ToCharArray()) >= 0)
            {
                Text = Text.Replace("\"", "\"\"");
                Builder.AppendFormat("\"{0}{1}\"", tags, Text);
            }
            else
            {
                Builder.Append(tags);
                Builder.Append(Text);
            }
        }

        #endregion

        #endregion

        #region 导入

        private void ImportAllCsv()
        {
            var editorAsset = LocalizationManager.GetEditorAsset(true);
            m_LanguageSourceData = editorAsset?.SourceData;

            if (m_LanguageSourceData == null)
            {
                UnityTipsHelper.ShowError($"没有找到多语言编辑器下的源数据 请检查 {I2LocalizeHelper.I2GlobalSourcesEditorPath}");
                return;
            }

            var path = GetSourceResPath();

            try
            {
                var content = LocalizationReader.ReadCSVfile(path, Encoding.UTF8);
                var sError =
                    m_LanguageSourceData.Import_CSV(string.Empty, content, eSpreadsheetUpdateMode.Replace, ',');
                if (!string.IsNullOrEmpty(sError))
                    UnityTipsHelper.ShowError($"导入全数据时发生错误 请检查 {sError} {path}");
            }
            catch (Exception e)
            {
                UnityTipsHelper.ShowError($"导入全数据时发生错误 请检查 {path}");
                Debug.LogError(e);
                return;
            }

            UnityTipsHelper.Show($"导入全数据完成 {path}");
            YIUIAutoTool.CloseWindowRefresh();
        }

        #endregion
    }
}
#endif