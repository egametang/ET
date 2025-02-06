using LZ4;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityFS
{

    public class BundleFileReader
    {

        private Header m_Header;
        private StorageBlock[] m_BlocksInfo;
        private Node[] m_DirectoryInfo;

        private StreamFile[] fileList;

        public BundleFileReader()
        {

        }

        public void Load(EndianBinaryReader reader)
        {
            Debug.Log($"reader. pos:{reader.Position} length:{reader.BaseStream.Length}");
            m_Header = new Header();
            m_Header.signature = reader.ReadStringToNull();
            m_Header.version = reader.ReadUInt32();
            m_Header.unityVersion = reader.ReadStringToNull();
            m_Header.unityRevision = reader.ReadStringToNull();
            System.Diagnostics.Debug.Assert(m_Header.signature == "UnityFS");


            m_Header.size = reader.ReadInt64();
            Debug.Log($"header size:{m_Header.size}");
            m_Header.compressedBlocksInfoSize = reader.ReadUInt32();
            m_Header.uncompressedBlocksInfoSize = reader.ReadUInt32();
            m_Header.flags = (ArchiveFlags)reader.ReadUInt32();
            if (m_Header.signature != "UnityFS")
            {
                reader.ReadByte();
            }

            ReadMetadata(reader);
            using (var blocksStream = CreateBlocksStream())
            {
                ReadBlocks(reader, blocksStream);
                ReadFiles(blocksStream);
            }
        }

        public BundleFileInfo CreateBundleFileInfo()
        {
            return new BundleFileInfo
            {
                signature = m_Header.signature,
                version = m_Header.version,
                unityVersion = m_Header.unityVersion,
                unityRevision = m_Header.unityRevision,
                files = fileList.Select(f => new BundleSubFile { file = f.path, data = f.stream.ReadAllBytes() }).ToList(),
            };
        }

        private byte[] ReadBlocksInfoAndDirectoryMetadataUnCompressedBytes(EndianBinaryReader reader)
        {
            byte[] metadataUncompressBytes;
            if (m_Header.version >= 7)
            {
                reader.AlignStream(16);
            }
            if ((m_Header.flags & ArchiveFlags.BlocksInfoAtTheEnd) != 0)
            {
                var position = reader.Position;
                reader.Position = reader.BaseStream.Length - m_Header.compressedBlocksInfoSize;
                metadataUncompressBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
                reader.Position = position;
            }
            else //0x40 BlocksAndDirectoryInfoCombined
            {
                metadataUncompressBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
            }
            return metadataUncompressBytes;
        }

        private byte[] DecompressBytes(CompressionType compressionType, byte[] compressedBytes, uint uncompressedSize)
        {
            switch (compressionType)
            {
                case CompressionType.None:
                    {
                        return compressedBytes;
                    }
                case CompressionType.Lzma:
                    {
                        var uncompressedStream = new MemoryStream((int)(uncompressedSize));
                        using (var compressedStream = new MemoryStream(compressedBytes))
                        {
                            ComparessHelper.Decompress7Zip(compressedStream, uncompressedStream, m_Header.compressedBlocksInfoSize, m_Header.uncompressedBlocksInfoSize);
                        }
                        return uncompressedStream.ReadAllBytes();
                    }
                case CompressionType.Lz4:
                case CompressionType.Lz4HC:
                    {
                        var uncompressedBytes = new byte[uncompressedSize];
                        var numWrite = LZ4Codec.Decode(compressedBytes, 0, compressedBytes.Length, uncompressedBytes, 0, uncompressedBytes.Length, true);
                        if (numWrite != uncompressedSize)
                        {
                            throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                        }
                        return uncompressedBytes;
                    }
                default:
                    throw new IOException($"Unsupported compression type {compressionType}");
            }
        }

        private void ReadMetadata(EndianBinaryReader reader)
        {
            byte[] compressMetadataBytes = ReadBlocksInfoAndDirectoryMetadataUnCompressedBytes(reader);
            MemoryStream metadataStream = new MemoryStream(DecompressBytes((CompressionType)(m_Header.flags & ArchiveFlags.CompressionTypeMask), compressMetadataBytes, m_Header.uncompressedBlocksInfoSize));
            using (var blocksInfoReader = new EndianBinaryReader(metadataStream))
            {
                var uncompressedDataHash = blocksInfoReader.ReadBytes(16);
                var blocksInfoCount = blocksInfoReader.ReadInt32();
                m_BlocksInfo = new StorageBlock[blocksInfoCount];
                for (int i = 0; i < blocksInfoCount; i++)
                {
                    m_BlocksInfo[i] = new StorageBlock
                    {
                        uncompressedSize = blocksInfoReader.ReadUInt32(),
                        compressedSize = blocksInfoReader.ReadUInt32(),
                        flags = (StorageBlockFlags)blocksInfoReader.ReadUInt16()
                    };
                }

                var nodesCount = blocksInfoReader.ReadInt32();
                m_DirectoryInfo = new Node[nodesCount];
                for (int i = 0; i < nodesCount; i++)
                {
                    m_DirectoryInfo[i] = new Node
                    {
                        offset = blocksInfoReader.ReadInt64(),
                        size = blocksInfoReader.ReadInt64(),
                        flags = blocksInfoReader.ReadUInt32(),
                        path = blocksInfoReader.ReadStringToNull(),
                    };
                }
            }
            if (m_Header.flags.HasFlag(ArchiveFlags.BlockInfoNeedPaddingAtStart))
            {
                reader.AlignStream(16);
            }
        }


        private Stream CreateBlocksStream()
        {
            Stream blocksStream;
            var uncompressedSizeSum = m_BlocksInfo.Sum(x => x.uncompressedSize);
            if (uncompressedSizeSum >= int.MaxValue)
            {
                throw new Exception($"too fig file");
            }
            else
            {
                blocksStream = new MemoryStream((int)uncompressedSizeSum);
            }
            return blocksStream;
        }

        public void ReadFiles(Stream blocksStream)
        {
            fileList = new StreamFile[m_DirectoryInfo.Length];
            for (int i = 0; i < m_DirectoryInfo.Length; i++)
            {
                var node = m_DirectoryInfo[i];
                var file = new StreamFile();
                fileList[i] = file;
                file.path = node.path;
                file.fileName = Path.GetFileName(node.path);
                if (node.size >= int.MaxValue)
                {
                    throw new Exception($"exceed max file size");
                    /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, entryinfo_size);
                    file.stream = memoryMappedFile.CreateViewStream();*/
                    //var extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                    //Directory.CreateDirectory(extractPath);
                    //file.stream = new FileStream(extractPath + file.fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                file.stream = new MemoryStream((int)node.size);
                blocksStream.Position = node.offset;
                blocksStream.CopyTo(file.stream, node.size);
                file.stream.Position = 0;
            }
        }

        private void ReadBlocks(EndianBinaryReader reader, Stream blocksStream)
        {
            foreach (var blockInfo in m_BlocksInfo)
            {
                var compressedSize = (int)blockInfo.compressedSize;
                byte[] compressedBlockBytes = reader.ReadBytes(compressedSize);
                var compressionType = (CompressionType)(blockInfo.flags & StorageBlockFlags.CompressionTypeMask);
                byte[] uncompressedBlockBytes = DecompressBytes(compressionType, compressedBlockBytes, blockInfo.uncompressedSize);
                blocksStream.Write(uncompressedBlockBytes, 0, uncompressedBlockBytes.Length);
            }
            blocksStream.Position = 0;
        }
    }
}
