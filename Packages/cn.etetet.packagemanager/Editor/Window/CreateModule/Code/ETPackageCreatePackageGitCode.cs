using System.Collections.Generic;
using System;

namespace ET.PackageManager.Editor
{
    public class ETPackageCreatePackageGitCode : BaseTemplate
    {
        private readonly string m_EventName = "PackageGit";
        public override  string EventName => m_EventName;

        public override bool Cover => true;

        private readonly bool m_AutoRefresh = false;
        public override  bool AutoRefresh => m_AutoRefresh;

        private readonly bool m_ShowTips = false;
        public override  bool ShowTips => m_ShowTips;

        public ETPackageCreatePackageGitCode(ETPackageCreateData codeData) : base("")
        {
            Create(codeData);
        }

        public ETPackageCreatePackageGitCode(out bool result, ETPackageCreateData codeData) : base("")
        {
            result = Create(codeData);
        }

        private bool Create(ETPackageCreateData codeData)
        {
            var path     = $"{codeData.PackagePath}/packagegit.json";
            var template = $"{ETPackageCreateHelper.ETPackageCreateTemplatePath}/packagegit.txt";
            CreateVo = new CreateVo(template, path);

            ValueDic["PackageName"]       = codeData.PackageName;
            ValueDic["PackageId"]         = codeData.PackageId.ToString();
            ValueDic["ScriptsReferences"] = GetScriptsReferences(codeData);

            return CreateNewFile();
        }

        private string GetScriptsReferences(ETPackageCreateData codeData)
        {
            var refModule = new List<string>();
            var sb        = SbPool.Get();

            var runtimeRefType = codeData.RuntimeRefType;
            foreach (var value in Enum.GetValues(typeof(EPackageRuntimeRefType)))
            {
                var enumValue = (EPackageRuntimeRefType)value;
                if ((int)enumValue <= 0) continue;
                var hasflag = runtimeRefType.HasFlag(enumValue);
                if (hasflag)
                {
                    refModule.Add(enumValue.ToString());
                }
            }

            for (int i = 0; i < refModule.Count; i++)
            {
                var line = i < refModule.Count - 1 ? ",\n" : "";
                sb.AppendFormat("        \"{0}\": [\"{1}\"]{2}", refModule[i], codeData.AssemblyName, line);
            }

            return SbPool.PutAndToStr(sb);
        }
    }
}
