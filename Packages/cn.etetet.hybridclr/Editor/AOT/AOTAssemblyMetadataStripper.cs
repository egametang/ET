using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.AOT
{
    public class AOTAssemblyMetadataStripper
    {
        public static byte[] Strip(byte[] assemblyBytes)
        {
            var mod = ModuleDefMD.Load(assemblyBytes);
            foreach (var type in mod.GetTypes())
            {
                if (type.HasGenericParameters)
                {
                    continue;
                }
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody || method.HasGenericParameters)
                    {
                        continue;
                    }
                    method.Body = null;
                }
            }
            var writer = new System.IO.MemoryStream();
            var options = new ModuleWriterOptions(mod);
            options.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            mod.Write(writer);
            writer.Flush();
            return writer.ToArray();
        }

        public static void Strip(string originalAssemblyPath, string strippedAssemblyPath)
        {
            byte[] originDllBytes = System.IO.File.ReadAllBytes(originalAssemblyPath);
            byte[] strippedDllBytes = Strip(originDllBytes);
            UnityEngine.Debug.Log($"aot dll:{originalAssemblyPath}, length: {originDllBytes.Length} -> {strippedDllBytes.Length}, stripping rate:{(originDllBytes.Length - strippedDllBytes.Length)/(double)originDllBytes.Length} ");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(strippedAssemblyPath));
            System.IO.File.WriteAllBytes(strippedAssemblyPath, strippedDllBytes);
        }
    }
}
