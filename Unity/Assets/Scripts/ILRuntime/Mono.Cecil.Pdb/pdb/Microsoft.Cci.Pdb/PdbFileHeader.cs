//-----------------------------------------------------------------------------
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

namespace Microsoft.Cci.Pdb {
  internal class PdbFileHeader {
    //internal PdbFileHeader(int pageSize) {
    //  this.magic = new byte[32] {
    //            0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, // "Microsof"
    //            0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20, // "t C/C++ "
    //            0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30, // "MSF 7.00"
    //            0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00  // "^^^DS^^^"
    //        };
    //  this.pageSize = pageSize;
    //}

    internal PdbFileHeader(Stream reader, BitAccess bits) {
      bits.MinCapacity(56);
      reader.Seek(0, SeekOrigin.Begin);
      bits.FillBuffer(reader, 52);

      this.magic = new byte[32];
      bits.ReadBytes(this.magic);                 //   0..31
      bits.ReadInt32(out this.pageSize);          //  32..35
      bits.ReadInt32(out this.freePageMap);       //  36..39
      bits.ReadInt32(out this.pagesUsed);         //  40..43
      bits.ReadInt32(out this.directorySize);     //  44..47
      bits.ReadInt32(out this.zero);              //  48..51

      int directoryPages = ((((directorySize + pageSize - 1) / pageSize) * 4) + pageSize - 1) / pageSize;
      this.directoryRoot = new int[directoryPages];
      bits.FillBuffer(reader, directoryPages * 4);
      bits.ReadInt32(this.directoryRoot);
    }

    //internal string Magic {
    //  get { return StringFromBytesUTF8(magic); }
    //}

    //internal void Write(Stream writer, BitAccess bits) {
    //  bits.MinCapacity(pageSize);
    //  bits.WriteBytes(magic);                     //   0..31
    //  bits.WriteInt32(pageSize);                  //  32..35
    //  bits.WriteInt32(freePageMap);               //  36..39
    //  bits.WriteInt32(pagesUsed);                 //  40..43
    //  bits.WriteInt32(directorySize);             //  44..47
    //  bits.WriteInt32(zero);                      //  48..51
    //  bits.WriteInt32(directoryRoot);             //  52..55

    //  writer.Seek(0, SeekOrigin.Begin);
    //  bits.WriteBuffer(writer, pageSize);
    //}

    //////////////////////////////////////////////////// Helper Functions.
    //
    //internal static string StringFromBytesUTF8(byte[] bytes) {
    //  return StringFromBytesUTF8(bytes, 0, bytes.Length);
    //}

    //internal static string StringFromBytesUTF8(byte[] bytes, int offset, int length) {
    //  for (int i = 0; i < length; i++) {
    //    if (bytes[offset + i] < ' ') {
    //      length = i;
    //    }
    //  }
    //  return Encoding.UTF8.GetString(bytes, offset, length);
    //}

    ////////////////////////////////////////////////////////////// Fields.
    //
    internal readonly byte[] magic;
    internal readonly int pageSize;
    internal int freePageMap;
    internal int pagesUsed;
    internal int directorySize;
    internal readonly int zero;
    internal int[] directoryRoot;
  }

}
