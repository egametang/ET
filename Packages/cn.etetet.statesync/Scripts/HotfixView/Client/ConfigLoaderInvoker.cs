using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET
{
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigLoader.GetAllConfigBytes, ETTask<Dictionary<Type, byte[]>>>
    {
        public override async ETTask<Dictionary<Type, byte[]>> Handle(ConfigLoader.GetAllConfigBytes args)
        {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            HashSet<Type> configTypes = CodeTypes.Instance.GetTypes(typeof (ConfigAttribute));
            
            if (Define.IsEditor)
            {
                string ct = "cs";
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                CodeMode codeMode = globalConfig.CodeMode;
                switch (codeMode)
                {
                    case CodeMode.Client:
                        ct = "c";
                        break;
                    case CodeMode.Server:
                        ct = "s";
                        break;
                    case CodeMode.ClientServer:
                        ct = "cs";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                List<string> startConfigs = new List<string>()
                {
                    "StartMachineConfigCategory", 
                    "StartProcessConfigCategory", 
                    "StartSceneConfigCategory", 
                    "StartZoneConfigCategory",
                };
                foreach (Type configType in configTypes)
                {
                    string configFilePath;
                    if (startConfigs.Contains(configType.Name))
                    {
                        configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{Options.Instance.StartConfig}/{configType.Name}.bytes";    
                    }
                    else
                    {
                        configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{configType.Name}.bytes";
                    }
                    output[configType] = File.ReadAllBytes(configFilePath);
                }
            }
            else
            {
                foreach (Type type in configTypes)
                {
                    TextAsset v = await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>($"{ConstValue.ExcelPackagePath}/Config/Bytes/c/{type.Name}.bytes");
                    output[type] = v.bytes;
                }
            }

            return output;
        }
    }
    
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigLoader.GetOneConfigBytes, ETTask<byte[]>>
    {
        public override async ETTask<byte[]> Handle(ConfigLoader.GetOneConfigBytes args)
        {
            string ct = "cs";
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            CodeMode codeMode = globalConfig.CodeMode;
            switch (codeMode)
            {
                case CodeMode.Client:
                    ct = "c";
                    break;
                case CodeMode.Server:
                    ct = "s";
                    break;
                case CodeMode.ClientServer:
                    ct = "cs";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };

            string configName = args.ConfigName;
                
            string configFilePath;
            if (startConfigs.Contains(configName))
            {
                configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{Options.Instance.StartConfig}/{configName}.bytes";    
            }
            else
            {
                configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{configName}.bytes";
            }

            await ETTask.CompletedTask;
            return File.ReadAllBytes(configFilePath);
        }
    }
}