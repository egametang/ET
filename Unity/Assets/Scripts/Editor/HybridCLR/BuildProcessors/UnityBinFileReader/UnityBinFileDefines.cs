using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static HybridCLR.Editor.GlobalManagers.UnityBinUtils;

namespace HybridCLR.Editor.GlobalManagers
{
    public struct FileHeader
    {
        public const int kSize = 20;

        public uint dataSize => fileSize - metadataSize;

        public uint metadataSize;
        public uint fileSize;
        public uint version;
        public uint dataOffset;
        public byte endianess;

        public void LoadFromStream(BinaryReader br)
        {
            long startPos = br.BaseStream.Position;
            metadataSize = br.ReadUInt32();
            fileSize = br.ReadUInt32();
            version = br.ReadUInt32();
            dataOffset = br.ReadUInt32();
            endianess = br.ReadByte();
            br.BaseStream.Position = startPos + kSize;

            SwapEndianess();
        }

        public long SaveToStream(BinaryWriter bw)
        {
            SwapEndianess();

            long startPos = bw.BaseStream.Position;
            bw.Write(metadataSize);
            bw.Write(fileSize);
            bw.Write(version);
            bw.Write(dataOffset);
            bw.Write(endianess);
            bw.BaseStream.Position = startPos + kSize;
            return kSize;
        }

        void SwapEndianess()
        {
            SwapUInt(ref metadataSize);
            SwapUInt(ref fileSize);
            SwapUInt(ref version);
            SwapUInt(ref dataOffset);
        }
    }

    public struct MetaData
    {
        public long dataStartPos;

        public string version;
        public uint platform;
        public bool enableTypeTree;
        public int typeCount;
        public ObjectType[] types;
        public int objectCount;
        public Dictionary<long, ObjectInfo> objects;
        public int scriptTypeCount;
        public ScriptType[] scriptTypes;
        public int externalsCount;
        public ExternalInfo[] externals;

#if UNITY_2019_2_OR_NEWER
        public int refTypeCount;
        public ObjectType[] refTypes;
#endif
        public string dummyStr;

        public void LoadFromStream(BinaryReader br, uint dataOffset)
        {
            long startPos = br.BaseStream.Position;
            dataStartPos = startPos;

            version = br.ReadRawString();
            platform = br.ReadUInt32();
            enableTypeTree = br.ReadBoolean();
            typeCount = br.ReadInt32();
            types = new ObjectType[typeCount];

            for (int i = 0; i < typeCount; i++)
            {
                types[i].LoadFromStream(br);
            }

            objectCount = br.ReadInt32();
            objects = new Dictionary<long, ObjectInfo>();
            for(int i = 0; i < objectCount; i++)
            {
                long id = br.AlignedReadInt64();
                ObjectInfo objInfo = new ObjectInfo();
                objInfo.LoadFromStream(br);
                objInfo.realPos = objInfo.dataPos + dataOffset;

                objects.Add(id, objInfo);
            }

            scriptTypeCount = br.ReadInt32();
            scriptTypes = new ScriptType[scriptTypeCount];
            for(int i = 0; i < scriptTypeCount; i++)
            {
                scriptTypes[i].LoadFromStream(br);
            }

            externalsCount = br.ReadInt32();
            externals = new ExternalInfo[externalsCount];
            for(int i = 0; i < externalsCount; i++)
            {
                externals[i].LoadFromStream(br);
            }

#if UNITY_2019_2_OR_NEWER
            refTypeCount = br.ReadInt32();
            refTypes = new ObjectType[refTypeCount];
            for(int i = 0; i < refTypeCount; i++)
            {
                refTypes[i].LoadFromStream(br);
            }
#endif
            dummyStr = br.ReadRawString();
        }

        public long SaveToStream(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.WriteRawString(version);
            bw.Write(platform);
            bw.Write(enableTypeTree);
            
            bw.Write(typeCount);
            foreach(var type in types)
                type.SaveToStream(bw);

            bw.Write(objectCount);
            foreach (var kv in objects)
            {
                bw.AlignedWriteInt64(kv.Key);
                kv.Value.SaveToStream(bw);
            }
                
            bw.Write(scriptTypeCount);
            foreach(var st in scriptTypes)
                st.SaveToStream(bw);

            bw.Write(externalsCount);
            foreach(var external in externals)
                external.SaveToStream(bw);

#if UNITY_2019_2_OR_NEWER
            bw.Write(refTypeCount);
            foreach(var refT in refTypes)
                refT.SaveToStream(bw);
#endif

            bw.WriteRawString(dummyStr);

            return bw.BaseStream.Position - startPos;
        }

        public ScriptsData GetScriptData(BinaryReader br)
        {
            ObjectInfo objInfo = objects[UnityBinFile.kMonoManagerIdx];
            br.BaseStream.Seek(objInfo.realPos, SeekOrigin.Begin);

            ScriptsData data = new ScriptsData();
            data.LoadFromStream(br);
            return data;
        }
    }

    public struct ObjectType
    {
        public int typeID;
        public bool isStriped;
        public short scriptTypeIndex;

        public bool needReadScriptHash; // dont save

        public Hash scriptSigHash;
        public Hash typeHash;

        public void LoadFromStream(BinaryReader br)
        {
            typeID = br.ReadInt32();
            isStriped = br.ReadBoolean();
            scriptTypeIndex = br.ReadInt16();

            needReadScriptHash = typeID == -1 || typeID == 0x72;
            if(needReadScriptHash)
                scriptSigHash.LoadFromStream(br);

            typeHash.LoadFromStream(br);

            // GlobalManagers does not has TypeTrees
        }

        public long SaveToStream(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.Write(typeID);
            bw.Write(isStriped);
            bw.Write(scriptTypeIndex);
            
            if(needReadScriptHash)
                scriptSigHash.SaveToStream(bw);

            typeHash.SaveToStream(bw);
            return bw.BaseStream.Position - startPos;
        }

        public int Size()
        {
            int ret = 0;
            ret += sizeof(int);
            ret += sizeof(bool);
            ret += sizeof(short);

            if (needReadScriptHash)
                ret += Hash.kSize;

            ret += Hash.kSize;
            return ret;
        }
    }

    public struct ObjectInfo
    {
        public const int kSize = 12;

        public uint dataPos;
        public uint size;
        public uint typeID;

        public uint realPos; // dataPos + Header.dataOffset; // dont save

        public void LoadFromStream(BinaryReader br)
        {
            dataPos = br.ReadUInt32();
            size = br.ReadUInt32();
            typeID = br.ReadUInt32();
        }

        public long SaveToStream(BinaryWriter bw)
        {
            bw.Write(dataPos);
            bw.Write(size);
            bw.Write(typeID);
            return kSize;
        }
    }

    public struct ScriptType
    {
        public int localFileIndex;
        public long localIdentifierOfBin;

        public void LoadFromStream(BinaryReader br)
        {
            localFileIndex = br.ReadInt32();
            localIdentifierOfBin = br.AlignedReadInt64();
        }

        public long SaveToStream(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.Write(localFileIndex);
            bw.AlignedWriteInt64(localIdentifierOfBin);
            return bw.BaseStream.Position - startPos;
        }
    }

    public struct ExternalInfo
    {
        public string dummy;
        public Hash guid;
        public int type;
        public string name;
        
        public void LoadFromStream(BinaryReader br)
        {
            dummy = br.ReadRawString();
            guid.LoadFromStream(br);
            type = br.ReadInt32();
            name = br.ReadRawString();
        }

        public long SaveToStream(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.WriteRawString(dummy);
            guid.SaveToStream(bw);
            bw.Write(type);
            bw.WriteRawString(name);
            return bw.BaseStream.Position - startPos;
        }
    }

    public struct ScriptsData
    {
        public ScriptID[] scriptIDs;
        public List<string> dllNames;
        public List<int> dllTypes; // 16 is user type

        public void LoadFromStream(BinaryReader br)
        {
            {
                int count = br.ReadInt32();
                scriptIDs = new ScriptID[count];
                for(int i = 0; i < count; i++)
                    scriptIDs[i].LoadFromStream(br);
            }
            {
                int count = br.ReadInt32();
                dllNames = new List<string>(count);
                for (var i = 0; i < count; i++)
                    dllNames.Add(br.ReadSizeString());
            }
            {
                int count = br.ReadInt32();
                dllTypes = new List<int>(count);
                for(var i = 0; i < count; i++)
                    dllTypes.Add(br.ReadInt32());
            }
        }

        public long SaveToStream(BinaryWriter bw)
        {
            long startPos = bw.BaseStream.Position;
            bw.Write(scriptIDs.Length);
            for(int i = 0; i < scriptIDs.Length; i++)
                scriptIDs[i].SaveToStream(bw);

            bw.Write(dllNames.Count);
            for(int i = 0, imax = dllNames.Count; i < imax; i++)
                bw.WriteSizeString(dllNames[i]);

            bw.Write(dllTypes.Count);
            for(int i = 0, imax = dllTypes.Count; i < imax; i++)
                bw.Write(dllTypes[i]);

            return bw.BaseStream.Position - startPos;
        }
    }

    public struct ScriptID
    {
        public int fileID;
        public long pathID; // localIdentifier

        public void LoadFromStream(BinaryReader br)
        {
            fileID = br.ReadInt32();
            pathID = br.ReadInt64();
        }

        public long SaveToStream(BinaryWriter bw)
        {
            bw.Write(fileID);
            bw.Write(pathID);
            return 4 + 8;
        }
    }

    public struct Hash
    {
        public const int kSize = 16;

        public int[] data;

        public void LoadFromStream(BinaryReader br)
        {
            data = new int[4];
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = br.ReadInt32();
            }
        }

        public long SaveToStream(BinaryWriter bw)
        {
            for(int i = 0; i < data.Length; i++)
            {
                bw.Write(data[i]);
            }
            return kSize;
        }
    }
}
