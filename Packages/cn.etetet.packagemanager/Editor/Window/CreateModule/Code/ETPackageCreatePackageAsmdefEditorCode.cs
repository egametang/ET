using System.Collections.Generic;
using System;

namespace ET.PackageManager.Editor
{
    public class ETPackageCreatePackageAsmdefEditorCode : BaseTemplate
    {
        private readonly string m_EventName = "AsmdefEditor";
        public override  string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override  bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override  bool ShowTips => m_ShowTips;

        public ETPackageCreatePackageAsmdefEditorCode(ETPackageCreateData codeData) : base("")
        {
            Create(codeData);
        }

        public ETPackageCreatePackageAsmdefEditorCode(out bool result, ETPackageCreateData codeData) : base("")
        {
            result = Create(codeData);
        }

        private bool Create(ETPackageCreateData codeData)
        {
            var path     = $"{codeData.PackagePath}/Editor/{codeData.AssemblyName}.Editor.asmdef";
            var template = $"{ETPackageCreateHelper.ETPackageCreateTemplatePath}/asmdefeditor.txt";
            CreateVo = new CreateVo(template, path);

            ValueDic["AssemblyName"] = codeData.AssemblyName;

            return CreateNewFile();
        }

    }
}
