using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BUI
{
    public class BUIScriptGenerator : EditorWindow
    {
        //TODO 生成代码后面用Scriban
        #region Editor Api
        private static void OpenWindow(BUIRoot root)
        {
            BUIScriptGenerator bUIScriptGenerator = GetWindow<BUIScriptGenerator>(true);
            bUIScriptGenerator.minSize = new Vector2(600, 100);
            bUIScriptGenerator.maxSize = new Vector2(600, 100);
            bUIScriptGenerator.root = root;
            bUIScriptGenerator.Show();
        }
        [MenuItem("GameObject/生成代码")]
        public static void GenerateScript()
        {
            GameObject gob = Selection.activeGameObject;
            if (gob != null)
            {
                BUIRoot root = gob.GetComponent<BUIRoot>();
                if (root != null)
                {
                    OpenWindow(root);
                }
                else
                {
                    EditorUtility.DisplayDialog("失败", "必须生成挂有BUIRoot脚本的物体", "好的");
                }
            }
        }
        #endregion

        #region GenerateScripte
        private BUISettingsObject _settingObject;
        private string _suffix = ".cs";
        private string _fieldTemplate = "\tpublic #TYPE# #FIELD_NAME#;";
        private void OnEnable()
        {
            _settingObject = AssetDatabase.LoadAssetAtPath<BUISettingsObject>(BUISettings.settingsObjectPath);
        }
        private void DoGenerateScript()
        {
            if (_settingObject == null)
            {
                EditorUtility.DisplayDialog("失败", "请先在\"BUI/BUI Settings\"里完成配置", "好的");
                return;
            }
            _members = root.SearchMembers();
            var nullMember = _members.FirstOrDefault(m => m.Type == null);
            if (nullMember != null)
            {
                EditorUtility.DisplayDialog("失败", $"{nullMember.name}的类型为空", "好的");
                return;
            }
            string outputpath = Path.Combine(_settingObject.outputFolder, _path + _suffix);
            FileInfo outputfile = new FileInfo(outputpath);
            string className = Path.GetFileNameWithoutExtension(outputfile.Name);
            if (!outputfile.Directory.Exists)
            {
                outputfile.Directory.Create();
            }
            string templatePath = _settingObject.templatePath;
            string content = File.ReadAllText(templatePath);
            content = content.Replace("#CLASS_NAME#", className);
            string fieldsStr = GetFieldsStr();
            content = content.Replace("#FIELDS#", fieldsStr);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
            FileStream fs = new FileStream(outputpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.SetLength(0);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            System.Diagnostics.Process.Start(outputfile.Directory.FullName);
        }
        private string GetFieldsStr()
        {
            StringBuilder sb = new StringBuilder();
            if (_members != null && _members.Count > 0)
            {
                foreach (var member in _members)
                {
                    sb.AppendLine(HandleField(member));
                }
            }
            return sb.ToString();
        }
        private string HandleField(BUIMember member)
        {
            if (member == null || string.IsNullOrEmpty(member.name))
            {
                return "";
            }
            member.SolveType();
            string typeName = BUITypeCatagory.Types[member.Type].FullName;
            string fieldName = member.name;
            return _fieldTemplate.Replace("#TYPE#", typeName).Replace("#FIELD_NAME#", fieldName);
        }
        #endregion
        #region Layout
        private string _path;
        private BUIRoot root;
        private List<BUIMember> _members;

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("UI名:", GUILayout.Width(40));
            if (string.IsNullOrEmpty(_path))
            {
                _path = root.name;
            }
            _path = GUILayout.TextField(_path);
            GUILayout.EndHorizontal();
            GUILayout.Space(60);
            if (GUILayout.Button("生成代码"))
            {
                DoGenerateScript();
                Close();
            }
        }
        #endregion
    }
}
