using System.Collections.Generic;
using System;

namespace ET.PackageManager.Editor
{
    public class ETPackageCreatePackageIgnoreAsmdefCode : BaseTemplate
    {
        private readonly string m_EventName = "IgnoreAsmdef";
        public override  string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override  bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override  bool ShowTips => m_ShowTips;

        public ETPackageCreatePackageIgnoreAsmdefCode(ETPackageCreateData codeData) : base("")
        {
            Create(codeData);
        }

        public ETPackageCreatePackageIgnoreAsmdefCode(out bool result, ETPackageCreateData codeData) : base("")
        {
            result = Create(codeData);
        }

        private bool Create(ETPackageCreateData codeData)
        {
            var path     = $"{codeData.PackagePath}/Ignore.{codeData.AssemblyName}.asmdef";
            var template = $"{ETPackageCreateHelper.ETPackageCreateTemplatePath}/ignoreasmdef.txt";
            CreateVo = new CreateVo(template, path);

            ValueDic["AssemblyName"] = codeData.AssemblyName;

            return CreateNewFile();
        }

    }
}
