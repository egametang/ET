#if DOTNET
using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigLoader.GetAllConfigBytes, ETTask<Dictionary<Type, byte[]>>>
    {
        public override async ETTask<Dictionary<Type, byte[]>> Handle(ConfigLoader.GetAllConfigBytes args)
        {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
            HashSet<Type> configTypes = CodeTypes.Instance.GetTypes(typeof (ConfigAttribute));
            foreach (Type configType in configTypes)
            {
                string configFilePath;
                if (startConfigs.Contains(configType.Name))
                {
                    configFilePath = Path.Combine($"{ConstValue.ExcelPackagePath}/Config/Bytes/s/{Options.Instance.StartConfig}/{configType.Name}.bytes");    
                }
                else
                {
                    configFilePath = Path.Combine($"{ConstValue.ExcelPackagePath}/Config/Bytes/s/{configType.Name}.bytes");
                }
                output[configType] = File.ReadAllBytes(configFilePath);
            }

            await ETTask.CompletedTask;
            return output;
        }
    }
    
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigLoader.GetOneConfigBytes, byte[]>
    {
        public override byte[] Handle(ConfigLoader.GetOneConfigBytes args)
        {
            byte[] configBytes = File.ReadAllBytes($"{ConstValue.ExcelPackagePath}/Config/Bytes/s/{args.ConfigName}.bytes");
            return configBytes;
        }
    }
}
#endif