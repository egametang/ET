using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System.Reflection;
using System;
using System.Linq;

namespace UnityFS
{
    /// <summary>
    /// Unity 生成的二进制文件(本代码不支持5.x之前的版本)
    /// </summary>
    public unsafe class UnityBinFile
    {
        /*
         * MonoManager: idx: 6;
         * type: metaData.types[objects[6].typeID]
         */
        public const int kMonoManagerIdx = 6;

        public FileHeader header;
        public MetaData metaData;
        public ScriptsData scriptsData;

        private Stream _originStream;

        public void LoadFromStream(Stream source)
        {
            _originStream = source;
            using (var br = new BinaryReader(source, Encoding.UTF8, true))
            {
                header.LoadFromStream(br);
                // 按理说 metaData 应该新开一个buffer来避免加载时的对齐逻辑问题，但由于 sizeof(Header) = 20，已经对齐到4了，所以可以连续读
                metaData.LoadFromStream(br, header.dataOffset);
                scriptsData = metaData.GetScriptData(br);
            }
        }

        public void Load(string path)
        {
            LoadFromStream(new MemoryStream(File.ReadAllBytes(path)));
        }

        public void AddScriptingAssemblies(List<string> assemblies)
        {
            foreach (string name in assemblies)
            {
                if (!scriptsData.dllNames.Contains(name))
                {
                    scriptsData.dllNames.Add(name);
                    scriptsData.dllTypes.Add(16); // user dll type
                    Debug.Log($"[PatchScriptAssembliesJson] add dll:{name} to globalgamemanagers");
                }
            }
        }

        public byte[] CreatePatchedBytes()
        {
            var fsR = _originStream;
            fsR.Position = 0;
            var brR = new BinaryReader(fsR, Encoding.UTF8, true);

            var ms = new MemoryStream((int)(header.fileSize * 1.5f));
            var bw = new BinaryWriter(ms, Encoding.UTF8, true);

            /*
             * 开始写入data
             * dll名称列表存储于 data 区段，修改其数据并不会影响 MetaData 大小，因此 dataOffset 不会改变
             */
            ms.Position = header.dataOffset;

            Dictionary<long, ObjectInfo> newObjInfos = new Dictionary<long, ObjectInfo>();
            foreach (var kv in metaData.objects)
            {
                long objID = kv.Key;
                ObjectInfo objInfo = kv.Value;

                byte[] buff = new byte[objInfo.size];
                fsR.Position = objInfo.realPos;
                brR.Read(buff, 0, buff.Length);


                {// unity 的数据偏移貌似会对齐到 8
                    int newPos = (((int)ms.Position + 7) >> 3) << 3;
                    int gapSize = newPos - (int)ms.Position;

                    for (int i = 0; i < gapSize; i++)
                        bw.Write((byte)0);

                    objInfo.dataPos = (uint)ms.Position - header.dataOffset; // 重定位数据偏移
                }

                if (objID != kMonoManagerIdx)
                    bw.Write(buff, 0, buff.Length);
                else
                    objInfo.size = (uint)scriptsData.SaveToStream(bw);

                newObjInfos.Add(objID, objInfo);
            }

            metaData.objects = newObjInfos;
            header.fileSize = (uint)ms.Position;

            ms.Position = 0;
            header.SaveToStream(bw);
            metaData.SaveToStream(bw);

            brR.Close();

            // 写入新文件
            ms.Position = 0;
            return ms.ToArray();
        }

        public void Save(string newPath)
        {
            byte[] patchedBytes = CreatePatchedBytes();
            File.WriteAllBytes(newPath, patchedBytes);
        }
    }

}
