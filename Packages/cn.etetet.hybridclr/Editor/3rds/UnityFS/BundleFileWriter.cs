using LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityFS
{
    public class BundleFileWriter
    {
        private readonly BundleFileInfo _bundle;

        private readonly List<Node> _files = new List<Node>();
        private readonly List<StorageBlock> _blocks = new List<StorageBlock>();

        private readonly EndianBinaryWriter _blockDirectoryMetadataStream = new EndianBinaryWriter(new MemoryStream());
        private byte[] _blockBytes;

        public BundleFileWriter(BundleFileInfo bundle)
        {
            _bundle = bundle;
        }

        public void Write(EndianBinaryWriter output)
        {
            InitBlockAndDirectories();

            output.WriteNullEndString(_bundle.signature);
            output.Write(_bundle.version);
            output.WriteNullEndString(_bundle.unityVersion);
            output.WriteNullEndString(_bundle.unityRevision);

            BuildBlockDirectoryMetadata();


            long sizePos = output.Position;
            output.Write(0L);
            output.Write((uint)_blockDirectoryMetadataStream.Length);
            output.Write((uint)_blockDirectoryMetadataStream.Length);
            ArchiveFlags flags = ArchiveFlags.BlocksAndDirectoryInfoCombined | (uint)CompressionType.None;
            output.Write((uint)flags);

            if (_bundle.version >= 7)
            {
                output.AlignStream(16);
            }
            byte[] metadataBytes = _blockDirectoryMetadataStream.BaseStream.ReadAllBytes();
            output.Write(metadataBytes, 0, metadataBytes.Length);

            byte[] dataBytes = _blockBytes;
            output.Write(dataBytes, 0, dataBytes.Length);

            output.Position = sizePos;
            output.Write(output.Length);
        }

        private void InitBlockAndDirectories()
        {
            var dataStream = new MemoryStream();
            foreach(var file in _bundle.files)
            {
                byte[] data = file.data;
                _files.Add(new Node { path = file.file, flags = 0, offset = dataStream.Length, size = data.LongLength });
                dataStream.Write(data, 0, data.Length);
            }
            byte[] dataBytes = dataStream.ToArray();

            var compressedBlockStream = new MemoryStream(dataBytes.Length / 2);
            int blockByteSize = 128 * 1024;
            long dataSize = dataBytes.Length;
            byte[] tempCompressBlock = new byte[blockByteSize * 2];
            for(long i = 0, blockNum = (dataSize + blockByteSize - 1) /  blockByteSize; i < blockNum; i++)
            {
                long curBlockSize = Math.Min(dataSize, blockByteSize);
                dataSize -= curBlockSize;

                int compressedSize = LZ4Codec.Encode(dataBytes, (int)(i * blockByteSize), (int)curBlockSize, tempCompressBlock, 0, tempCompressBlock.Length);
                compressedBlockStream.Write(tempCompressBlock, 0, compressedSize);
                _blocks.Add(new StorageBlock { flags = (StorageBlockFlags)(int)CompressionType.Lz4, compressedSize = (uint)compressedSize, uncompressedSize = (uint)curBlockSize });
                //Debug.Log($"== block[{i}] uncompressedSize:{curBlockSize} compressedSize:{compressedSize}  totalblocksize:{compressedBlockStream.Length}");
            }
            _blockBytes = compressedBlockStream.ToArray();
        }

        private void BuildBlockDirectoryMetadata()
        {
            var hash = new byte[16];
            _blockDirectoryMetadataStream.Write(hash, 0, 16);

            _blockDirectoryMetadataStream.Write((uint)_blocks.Count);
            foreach(var b in _blocks)
            {
                _blockDirectoryMetadataStream.Write(b.uncompressedSize);
                _blockDirectoryMetadataStream.Write(b.compressedSize);
                _blockDirectoryMetadataStream.Write((ushort)b.flags);
            }

            _blockDirectoryMetadataStream.Write((uint)_files.Count);
            foreach(var f in _files)
            {
                _blockDirectoryMetadataStream.Write(f.offset);
                _blockDirectoryMetadataStream.Write(f.size);
                _blockDirectoryMetadataStream.Write(f.flags);
                _blockDirectoryMetadataStream.WriteNullEndString(f.path);
            }
            //Debug.Log($"block and directory metadata size:{_blockDirectoryMetadataStream.Length}");
        }
    }
}
