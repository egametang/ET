//using System;
//using System.Diagnostics.SymbolStore;
//namespace Mono.CompilerServices.SymbolWriter
//{
//    internal class SymbolDocumentWriterImpl : ISymbolDocumentWriter, ISourceFile, ICompileUnit
//    {
//        private CompileUnitEntry comp_unit;
//        SourceFileEntry ISourceFile.Entry
//        {
//            get
//            {
//                return this.comp_unit.SourceFile;
//            }
//        }
//        public CompileUnitEntry Entry
//        {
//            get
//            {
//                return this.comp_unit;
//            }
//        }
//        public SymbolDocumentWriterImpl(CompileUnitEntry comp_unit)
//        {
//            this.comp_unit = comp_unit;
//        }
//        public void SetCheckSum(Guid algorithmId, byte[] checkSum)
//        {
//        }
//        public void SetSource(byte[] source)
//        {
//        }
//    }
//}
