#if UNITY_EDITOR

//#define YIUI_GETALL_ASSEMBLY
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace YIUIFramework
{
    internal class YIUIBindProvider: ICodeGenerator<YIUIBindVo>
    {
        #region 扫描指定程序集

        #if !YIUI_GETALL_ASSEMBLY

        //业务代码相关程序集的名字
        //默认有Unity默认程序集 可以根据需求修改
        private static readonly string[] LogicAssemblyNames = { "Unity.ModelView" };

        private static Type[] GetLogicTypes()
        {
            return AppDomain.CurrentDomain.GetTypesByAssemblyName(LogicAssemblyNames);
        }
        #else
        //找所有程序集 全部遍历一次 消耗比指定程序集大 但是简单 如果你有需求扫描指定的
        //使用上面的给的方法 写入你要的程序集
        //这个也仅限于编辑器模式下 正常打包出去后会使用生成的就没有消耗了 根据需求自行选择
        private static Type[] GetLogicTypes()
        {
            System.Reflection.Assembly[]               assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
            return types.Values.ToArray();
        }
        #endif

        #endregion
        
        public Type[] GetLogicTypesByDll()
        {
            var buildOutputDir = "./Temp/Bin/Debug";
            string[] logicFiles     = Directory.GetFiles(buildOutputDir, "Model.dll");
            if (logicFiles.Length != 1)
            {
                throw new Exception("Logic dll count != 1");
            }
            string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
            var assBytes = File.ReadAllBytes(Path.Combine(buildOutputDir, $"{logicName}.dll"));
            var pdbBytes = File.ReadAllBytes(Path.Combine(buildOutputDir, $"{logicName}.pdb"));
            var model = Assembly.Load(assBytes, pdbBytes);
            var typesDic = AssemblyHelper.GetAssemblyTypes(model);
            return typesDic.Values.ToArray();
        }
        
        public YIUIBindVo[] Get()
        {
            #if !ENABLE_DLL
            var types = GetLogicTypes();
            #else
            var types = GetLogicTypesByDll();
            #endif
            
            var binds = new List<YIUIBindVo>();

            foreach (var type in types)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                var uiAttributes = (YIUIAttribute[])type.GetCustomAttributes(typeof (YIUIAttribute), true);

                foreach (var attribute in uiAttributes)
                {
                    if (GetBindVo(out var bindVo, attribute, type))
                    {
                        binds.Add(bindVo);
                    }
                }
            }

            return binds.ToArray();
        }

        private static bool GetBindVo(out YIUIBindVo bindVo,
                                      YIUIAttribute  attribute,
                                      Type           componentType)
        {
            bindVo = new YIUIBindVo();
            if (componentType == null ||
                !componentType.GetFieldValue("PkgName", out bindVo.PkgName) ||
                !componentType.GetFieldValue("ResName", out bindVo.ResName))
            {
                return false;
            }

            bindVo.ComponentType = componentType;
            bindVo.CodeType      = attribute.YIUICodeType;
            bindVo.PanelLayer    = attribute.YIUIPanelLayer;
            if (bindVo is { CodeType: EUICodeType.Panel, PanelLayer: EPanelLayer.Any })
            {
                Debug.LogError($"{componentType.Name} 错误的设定 既然是Panel 那必须设定所在层级 不能是Any 请检查重新导出");
            }
            return true;
        }

        public void WriteCode(YIUIBindVo info, StringBuilder sb)
        {
            sb.Append("            {\r\n");
            sb.AppendFormat("                PkgName       = {0}.PkgName,\r\n", info.ComponentType.FullName);
            sb.AppendFormat("                ResName       = {0}.ResName,\r\n", info.ComponentType.FullName);
            sb.AppendFormat("                CodeType      = EUICodeType.{0},\r\n", info.CodeType.ToString());
            sb.AppendFormat("                PanelLayer    = EPanelLayer.{0},\r\n", info.PanelLayer.ToString());
            sb.AppendFormat("                ComponentType = typeof({0}),\r\n", info.ComponentType.FullName);
            sb.Append("            };\r\n");
        }

        public void NewCode(YIUIBindVo info, StringBuilder sb)
        {
        }
    }
}
#endif