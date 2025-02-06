using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public enum DevelopMode
    {
        正式 = 0,
        开发 = 1,
        压测 = 2,
    }

    public class ServerCommandLineEditor: EditorWindow
    {
        [MenuItem("ET/Loader/ServerTools", false, ETMenuItemPriority.ServerTools)]
        public static void ShowWindow()
        {
            GetWindow<ServerCommandLineEditor>();
        }

        private int selectStartConfigIndex = 1;
        private string[] startConfigs;
        private string startConfig;
        private DevelopMode developMode;

        public void OnEnable()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("Packages/cn.etetet.excel/Config/Bytes/cs/StartConfig");
            this.startConfigs = directoryInfo.GetDirectories().Select(x => x.Name).ToArray();
        }

        public void OnGUI()
        {
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            selectStartConfigIndex = EditorGUILayout.Popup(selectStartConfigIndex, this.startConfigs);
            this.startConfig = this.startConfigs[this.selectStartConfigIndex];
            this.developMode = (DevelopMode)EditorGUILayout.EnumPopup("起服模式：", this.developMode);

            if (GUILayout.Button("Start Server(Single Process)"))
            {
                string arguments = $"Bin/ET.App.dll --SceneName={globalConfig.SceneName} --Process=1 --StartConfig=StartConfig/{this.startConfig} --Console=1";
                ProcessHelper.DotNet(arguments, "./");
            }

            if (GUILayout.Button("Start Watcher"))
            {
                string arguments = $"Bin/ET.App.dll --SceneName=Watcher --StartConfig=StartConfig/{this.startConfig} --Console=1";
                ProcessHelper.DotNet(arguments, "./");
            }

            if (GUILayout.Button("Start Mongo"))
            {
                ProcessHelper.Run("mongod", @"--dbpath=db", "../Database/bin/");
            }
        }
    }
}