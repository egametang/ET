//using System;
//using System.IO;
//namespace Mono.CompilerServices.SymbolWriter
//{
//    internal class MyBinaryWriter : BinaryWriter
//    {
//        public MyBinaryWriter(Stream stream) : base(stream)
//        {
//        }
//        public void WriteLeb128(int value)
//        {
//            base.Write7BitEncodedInt(value);
//        }
//    }
//}
