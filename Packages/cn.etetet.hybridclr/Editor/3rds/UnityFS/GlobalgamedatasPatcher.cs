using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityFS;

namespace HybridCLR.Editor.UnityBinFileReader
{
    public class Dataunity3dPatcher
    {

        public void ApplyPatch(string dataunity3dFile, List<string> hotUpdateAssemblies)
        {
            var reader = new BundleFileReader();
            using (var fs = new EndianBinaryReader(new MemoryStream(File.ReadAllBytes(dataunity3dFile))))
            {
                reader.Load(fs);
            }

            var info = reader.CreateBundleFileInfo();
            //Debug.Log($"name:{info.signature} version:{info.version} files:{info.files.Count}");
            //foreach (var file in info.files)
            //{
            //    Debug.Log($"file:{file.file} size:{file.data.Length}");
            //}

            var globalgamemanagersFile = info.files.Find(f => f.file == "globalgamemanagers");
            //Debug.LogFormat("gobalgamemanagers origin size:{0}", globalgamemanagersFile.data.Length);

            var ggdBinFile = new UnityBinFile();
            ggdBinFile.LoadFromStream(new MemoryStream(globalgamemanagersFile.data));
            ggdBinFile.AddScriptingAssemblies(hotUpdateAssemblies);
            byte[] patchedGlobalgamedatasBytes = ggdBinFile.CreatePatchedBytes();
            //Debug.LogFormat("gobalgamemanagers post patche size:{0}", patchedGlobalgamedatasBytes.Length);
            globalgamemanagersFile.data = patchedGlobalgamedatasBytes;

            var writer = new BundleFileWriter(info);
            var output = new MemoryStream();
            writer.Write(new EndianBinaryWriter(output));
            Debug.Log($"patch file:{dataunity3dFile} size:{output.Length}");

            //string bakFile = dataunity3dFile + ".bak";
            //if (!File.Exists(bakFile))
            //{
            //    File.Copy(dataunity3dFile, bakFile);
            //}
            File.WriteAllBytes(dataunity3dFile, output.ToArray());
        }
    }
}
