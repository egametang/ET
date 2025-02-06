#if ODIN_INSPECTOR
using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    [ETPackageMenu("更新")]
    public class ETPackageUpdateModule : BasePackageToolModule
    {
        [Button("文档", 30, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        [PropertyOrder(-9999)]
        public void OpenDocument()
        {
            ETPackageDocumentModule.ETPackageUpdate();
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