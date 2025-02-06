#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    [ETPackageMenu("文档", -int.MaxValue)]
    public class ETPackageDocumentModule : BasePackageToolModule
    {
        [Button("ETPackageManager 包管理", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void ETPackageManager()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/DzqwwwBJvixRvtkCI4dcatGcnAd");
        }

        [Button("一键生成包", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void ETPackageCreate()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/O6Ukw7k3SiBHmek8MMwc5HdInev");
        }

        [Button("库", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void ETPackageHub()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/DABBw0ZrJiSYcLknKZBczGHAnqg");
        }

        [Button("版本管理", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void ETPackageVersion()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/SLlHwBMAMiAjVnkqiWAcKU5InIc");
        }

        [Button("更新", 50, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        public static void ETPackageUpdate()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/LsSNwacK5iqBvDkTFAgcEh15nfc");
        }

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }
    }
}
#endif
