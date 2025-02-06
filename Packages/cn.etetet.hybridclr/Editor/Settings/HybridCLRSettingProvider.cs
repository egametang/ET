using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace HybridCLR.Editor.Settings
{
    public class HybridCLRSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedObject;
        private SerializedProperty _enable;
        private SerializedProperty _useGlobalIl2cpp;
        private SerializedProperty _hybridclrRepoURL;
        private SerializedProperty _il2cppPlusRepoURL;
        private SerializedProperty _hotUpdateAssemblyDefinitions;
        private SerializedProperty _hotUpdateAssemblies;
        private SerializedProperty _preserveHotUpdateAssemblies;
        private SerializedProperty _hotUpdateDllCompileOutputRootDir;
        private SerializedProperty _externalHotUpdateAssembliyDirs;
        private SerializedProperty _strippedAOTDllOutputRootDir;
        private SerializedProperty _patchAOTAssemblies;
        private SerializedProperty _outputLinkFile;
        private SerializedProperty _outputAOTGenericReferenceFile;
        private SerializedProperty _maxGenericReferenceIteration;
        private SerializedProperty _maxMethodBridgeGenericIteration;
        private SerializedProperty _enableProfilerInReleaseBuild;
        private SerializedProperty enableStraceTraceInWebGLReleaseBuild;

        private GUIStyle buttonStyle;
        public HybridCLRSettingsProvider() : base("Project/HybridCLR Settings", SettingsScope.Project) { }
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            EditorStatusWatcher.OnEditorFocused += OnEditorFocused;
            InitGUI();
        }
        private void InitGUI()
        {
            var setting = HybridCLRSettings.LoadOrCreate();
            _serializedObject?.Dispose();
            _serializedObject = new SerializedObject(setting);
            _enable = _serializedObject.FindProperty("enable");
            _useGlobalIl2cpp = _serializedObject.FindProperty("useGlobalIl2cpp");
            _hybridclrRepoURL = _serializedObject.FindProperty("hybridclrRepoURL");
            _il2cppPlusRepoURL = _serializedObject.FindProperty("il2cppPlusRepoURL");
            _hotUpdateAssemblyDefinitions = _serializedObject.FindProperty("hotUpdateAssemblyDefinitions");
            _hotUpdateAssemblies = _serializedObject.FindProperty("hotUpdateAssemblies");
            _preserveHotUpdateAssemblies = _serializedObject.FindProperty("preserveHotUpdateAssemblies");
            _hotUpdateDllCompileOutputRootDir = _serializedObject.FindProperty("hotUpdateDllCompileOutputRootDir");
            _externalHotUpdateAssembliyDirs = _serializedObject.FindProperty("externalHotUpdateAssembliyDirs");
            _strippedAOTDllOutputRootDir = _serializedObject.FindProperty("strippedAOTDllOutputRootDir");
            _patchAOTAssemblies = _serializedObject.FindProperty("patchAOTAssemblies");
            _outputLinkFile = _serializedObject.FindProperty("outputLinkFile");
            _outputAOTGenericReferenceFile = _serializedObject.FindProperty("outputAOTGenericReferenceFile");
            _maxGenericReferenceIteration = _serializedObject.FindProperty("maxGenericReferenceIteration");
            _maxMethodBridgeGenericIteration = _serializedObject.FindProperty("maxMethodBridgeGenericIteration");
            _enableProfilerInReleaseBuild = _serializedObject.FindProperty("enableProfilerInReleaseBuild");
            enableStraceTraceInWebGLReleaseBuild = _serializedObject.FindProperty("enableStraceTraceInWebGLReleaseBuild");
        }
        private void OnEditorFocused()
        {
            InitGUI();
            Repaint();
        }
        public override void OnTitleBarGUI()
        {
            base.OnTitleBarGUI();
            var rect = GUILayoutUtility.GetLastRect();
            buttonStyle = buttonStyle ?? GUI.skin.GetStyle("IconButton");

            #region  绘制官方网站跳转按钮
            var w = rect.x + rect.width;
            rect.x = w - 57;
            rect.y += 6;
            rect.width = rect.height = 18;
            var content = EditorGUIUtility.IconContent("_Help");
            content.tooltip = "点击访问 HybridCLR 官方文档";
            if (GUI.Button(rect, content, buttonStyle))
            {
                Application.OpenURL("https://hybridclr.doc.code-philosophy.com/");
            }
            #endregion
            #region 绘制 Preset
            rect.x += 19;
            content = EditorGUIUtility.IconContent("Preset.Context");
            content.tooltip = "点击存储或加载 Preset .";
            if (GUI.Button(rect, content, buttonStyle))
            {
                var target = HybridCLRSettings.Instance;
                var receiver = ScriptableObject.CreateInstance<SettingsPresetReceiver>();
                receiver.Init(target, this);
                PresetSelector.ShowSelector(target, null, true, receiver);
            }
            #endregion
            #region 绘制 Reset
            rect.x += 19;
            content = EditorGUIUtility.IconContent(
#if UNITY_2021_3_OR_NEWER
                "pane options"
#else
                "_Popup"
#endif
                );
            content.tooltip = "Reset";
            if (GUI.Button(rect, content, buttonStyle))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Reset"), false, () =>
                {
                    Undo.RecordObject(HybridCLRSettings.Instance, "Capture Value for Reset");
                    var dv = ScriptableObject.CreateInstance<HybridCLRSettings>();
                    var json = EditorJsonUtility.ToJson(dv);
                    UnityEngine.Object.DestroyImmediate(dv);
                    EditorJsonUtility.FromJsonOverwrite(json, HybridCLRSettings.Instance);
                    HybridCLRSettings.Save();
                });
                menu.ShowAsContext();
            }
            #endregion
        }
        public override void OnGUI(string searchContext)
        {
            using (CreateSettingsWindowGUIScope())
            {
                //解决编辑器打包时出现的 _serializedObject.targetObject 意外销毁的情况
                if (_serializedObject == null||!_serializedObject.targetObject)
                {
                    InitGUI();
                }
                _serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_enable);
                EditorGUILayout.PropertyField(_hybridclrRepoURL);
                EditorGUILayout.PropertyField(_il2cppPlusRepoURL);
                EditorGUILayout.PropertyField(_useGlobalIl2cpp);
                EditorGUILayout.PropertyField(_hotUpdateAssemblyDefinitions);
                EditorGUILayout.PropertyField(_hotUpdateAssemblies);
                EditorGUILayout.PropertyField(_preserveHotUpdateAssemblies);
                EditorGUILayout.PropertyField(_hotUpdateDllCompileOutputRootDir);
                EditorGUILayout.PropertyField(_externalHotUpdateAssembliyDirs);
                EditorGUILayout.PropertyField(_strippedAOTDllOutputRootDir);
                EditorGUILayout.PropertyField(_patchAOTAssemblies);
                EditorGUILayout.PropertyField(_outputLinkFile);
                EditorGUILayout.PropertyField(_outputAOTGenericReferenceFile);
                EditorGUILayout.PropertyField(_maxGenericReferenceIteration);
                EditorGUILayout.PropertyField(_maxMethodBridgeGenericIteration);
                EditorGUILayout.PropertyField(_enableProfilerInReleaseBuild);
                EditorGUILayout.PropertyField(enableStraceTraceInWebGLReleaseBuild);
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                    HybridCLRSettings.Save();
                }
            }
        }
        private IDisposable CreateSettingsWindowGUIScope()
        {
            var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
            return Activator.CreateInstance(type) as IDisposable;
        }
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EditorStatusWatcher.OnEditorFocused -= OnEditorFocused;
            HybridCLRSettings.Save();
        }

        static HybridCLRSettingsProvider provider;
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (HybridCLRSettings.Instance && provider == null)
            {
                provider = new HybridCLRSettingsProvider();
                using (var so = new SerializedObject(HybridCLRSettings.Instance))
                {
                    provider.keywords = GetSearchKeywordsFromSerializedObject(so);
                }
            }
            return provider;
        }
    }
}