using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.AOT
{
    public class GenericReferenceWriter
    {
        private static readonly Dictionary<Type, string> _typeNameMapping = new Dictionary<Type, string>
        {
            {typeof(bool), "bool" },
            {typeof(byte), "byte" },
            {typeof(sbyte), "sbyte" },
            {typeof(short), "short" },
            {typeof(ushort), "ushort" },
            {typeof(int), "int" },
            {typeof(uint), "uint" },
            {typeof(long), "long" },
            {typeof(ulong), "ulong" },
            {typeof(float), "float" },
            {typeof(double), "double" },
            {typeof(object), "object" },
            {typeof(string), "string" },
        };

        private readonly Dictionary<string, string> _typeSimpleNameMapping = new Dictionary<string, string>();
        private readonly Regex _systemTypePattern;
        private readonly Regex _genericPattern = new Regex(@"`\d+");

        public GenericReferenceWriter()
        {
            foreach (var e in _typeNameMapping)
            {
                _typeSimpleNameMapping.Add(e.Key.FullName, e.Value);
            }
            _systemTypePattern = new Regex(string.Join("|", _typeSimpleNameMapping.Keys.Select (k => $@"\b{k}\b")));
        }

        public string PrettifyTypeSig(string typeSig)
        {
            string s = _genericPattern.Replace(typeSig, "").Replace('/', '.');
            return _systemTypePattern.Replace(s, m => _typeSimpleNameMapping[m.Groups[0].Value]);
        }

        public string PrettifyMethodSig(string methodSig)
        {
            string s = PrettifyTypeSig(methodSig).Replace("::", ".");
            if (s.Contains(".ctor("))
            {
                s = "new " + s.Replace(".ctor(", "(");
            }
            return s;
        }

        public void Write(List<GenericClass> types, List<GenericMethod> methods, string outputFile)
        {
            string parentDir = Directory.GetParent(outputFile).FullName;
            Directory.CreateDirectory(parentDir);

            List<string> codes = new List<string>();
            codes.Add("using System.Collections.Generic;");
            codes.Add("public class AOTGenericReferences : UnityEngine.MonoBehaviour");
            codes.Add("{");

            codes.Add("");
            codes.Add("\t// {{ AOT assemblies");
            codes.Add("\tpublic static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>");
            codes.Add("\t{");
            List<dnlib.DotNet.ModuleDef> modules = new HashSet<dnlib.DotNet.ModuleDef>(
                types.Select(t => t.Type.Module).Concat(methods.Select(m => m.Method.Module))).ToList();
            modules.Sort((a, b) => a.Name.CompareTo(b.Name));
            foreach (dnlib.DotNet.ModuleDef module in modules)
            {
                codes.Add($"\t\t\"{module.Name}\",");
            }
            codes.Add("\t};");
            codes.Add("\t// }}");


            codes.Add("");
            codes.Add("\t// {{ constraint implement type");

            codes.Add("\t// }} ");

            codes.Add("");
            codes.Add("\t// {{ AOT generic types");
            
            List<string> typeNames = types.Select(t => PrettifyTypeSig(t.ToTypeSig().ToString())).ToList();
            typeNames.Sort(string.CompareOrdinal);
            foreach(var typeName in typeNames)
            {
                codes.Add($"\t// {typeName}");
            }

            codes.Add("\t// }}");

            codes.Add("");
            codes.Add("\tpublic void RefMethods()");
            codes.Add("\t{");
            
            List<(string, string, string)> methodTypeAndNames = methods.Select(m =>
                (PrettifyTypeSig(m.Method.DeclaringType.ToString()), PrettifyMethodSig(m.Method.Name), PrettifyMethodSig(m.ToMethodSpec().ToString())))
                .ToList();
            methodTypeAndNames.Sort((a, b) =>
            {
                int c = String.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
                if (c != 0)
                {
                    return c;
                }

                c = String.Compare(a.Item2, b.Item2, StringComparison.Ordinal);
                if (c != 0)
                {
                    return c;
                }
                return String.Compare(a.Item3, b.Item3, StringComparison.Ordinal);
            });
            foreach(var method in methodTypeAndNames)
            {
                codes.Add($"\t\t// {PrettifyMethodSig(method.Item3)}");
            }
            codes.Add("\t}");

            codes.Add("}");


            var utf8WithoutBom = new System.Text.UTF8Encoding(false);
            File.WriteAllText(outputFile, string.Join("\n", codes), utf8WithoutBom);
            Debug.Log($"[GenericReferenceWriter] write {outputFile}");
        }
    }
}
