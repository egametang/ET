using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [Callback]
    public class GetAllConfigBytes: ACallbackHandler<ConfigComponent.GetAllConfigBytes, Dictionary<string, byte[]>>
    {
        public override Dictionary<string, byte[]> Handle(ConfigComponent.GetAllConfigBytes args)
        {
            Dictionary<string, byte[]> output = new Dictionary<string, byte[]>();
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
            HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute));
            foreach (Type configType in configTypes)
            {
                string configFilePath;
                if (startConfigs.Contains(configType.Name))
                {
                    configFilePath = $"../Config/{Options.Instance.StartConfig}/{configType.Name}.bytes";    
                }
                else
                {
                    configFilePath = $"../Config/{configType.Name}.bytes";
                }
                output[configType.Name] = File.ReadAllBytes(configFilePath);
            }

            return output;
        }
    }
    
    [Callback]
    public class GetOneConfigBytes: ACallbackHandler<ConfigComponent.GetOneConfigBytes, byte[]>
    {
        public override byte[] Handle(ConfigComponent.GetOneConfigBytes args)
        {
            byte[] configBytes = File.ReadAllBytes($"../Config/{args.ConfigName}.bytes");
            return configBytes;
        }
    }
}