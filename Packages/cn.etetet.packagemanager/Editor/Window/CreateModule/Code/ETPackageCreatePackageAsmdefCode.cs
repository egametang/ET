using System.Collections.Generic;
using System;

namespace ET.PackageManager.Editor
{
    public class ETPackageCreatePackageAsmdefCode : BaseTemplate
    {
        private readonly string m_EventName = "Asmdef";
        public override  string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override  bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override  bool ShowTips => m_ShowTips;

        public ETPackageCreatePackageAsmdefCode(ETPackageCreateData codeData) : base("")
        {
            Create(codeData);
        }

        public ETPackageCreatePackageAsmdefCode(out bool result, ETPackageCreateData codeData) : base("")
        {
            result = Create(codeData);
        }

        private bool Create(ETPackageCreateData codeData)
        {
            var path     = $"{codeData.PackagePath}/Runtime/{codeData.AssemblyName}.asmdef";
            var template = $"{ETPackageCreateHelper.ETPackageCreateTemplatePath}/asmdef.txt";
            CreateVo = new CreateVo(template, path);

            ValueDic["AssemblyName"] = codeData.AssemblyName;

            return CreateNewFile();
        }

    }
}
