//using System;
//using System.Collections.Generic;
//using System.IO;
//namespace Mono.CompilerServices.SymbolWriter
//{
//    public class MonoSymbolWriter
//    {
//        private List<SourceMethodBuilder> methods;
//        private List<SourceFileEntry> sources;
//        private List<CompileUnitEntry> comp_units;
//        protected readonly MonoSymbolFile file;
//        private string filename;
//        private SourceMethodBuilder current_method;
//        private Stack<SourceMethodBuilder> current_method_stack = new Stack<SourceMethodBuilder>();
//        public MonoSymbolFile SymbolFile
//        {
//            get
//            {
//                return this.file;
//            }
//        }
//        public MonoSymbolWriter(string filename)
//        {
//            this.methods = new List<SourceMethodBuilder>();
//            this.sources = new List<SourceFileEntry>();
//            this.comp_units = new List<CompileUnitEntry>();
//            this.file = new MonoSymbolFile();
//            this.filename = filename + ".mdb";
//        }
//        public void CloseNamespace()
//        {
//        }
//        public void DefineLocalVariable(int index, string name)
//        {
//            if (this.current_method != null)
//            {
//                this.current_method.AddLocal(index, name);
//            }
//        }
//        public void DefineCapturedLocal(int scope_id, string name, string captured_name)
//        {
//            this.file.DefineCapturedVariable(scope_id, name, captured_name, CapturedVariable.CapturedKind.Local);
//        }
//        public void DefineCapturedParameter(int scope_id, string name, string captured_name)
//        {
//            this.file.DefineCapturedVariable(scope_id, name, captured_name, CapturedVariable.CapturedKind.Parameter);
//        }
//        public void DefineCapturedThis(int scope_id, string captured_name)
//        {
//            this.file.DefineCapturedVariable(scope_id, "this", captured_name, CapturedVariable.CapturedKind.This);
//        }
//        public void DefineCapturedScope(int scope_id, int id, string captured_name)
//        {
//            this.file.DefineCapturedScope(scope_id, id, captured_name);
//        }
//        public void DefineScopeVariable(int scope, int index)
//        {
//            if (this.current_method != null)
//            {
//                this.current_method.AddScopeVariable(scope, index);
//            }
//        }
//        public void MarkSequencePoint(int offset, SourceFileEntry file, int line, int column, bool is_hidden)
//        {
//            if (this.current_method != null)
//            {
//                this.current_method.MarkSequencePoint(offset, file, line, column, is_hidden);
//            }
//        }
//        public SourceMethodBuilder OpenMethod(ICompileUnit file, int ns_id, IMethodDef method)
//        {
//            SourceMethodBuilder builder = new SourceMethodBuilder(file, ns_id, method);
//            this.current_method_stack.Push(this.current_method);
//            this.current_method = builder;
//            this.methods.Add(this.current_method);
//            return builder;
//        }
//        public void CloseMethod()
//        {
//            this.current_method = this.current_method_stack.Pop();
//        }
//        public SourceFileEntry DefineDocument(string url)
//        {
//            SourceFileEntry entry = new SourceFileEntry(this.file, url);
//            this.sources.Add(entry);
//            return entry;
//        }
//        public SourceFileEntry DefineDocument(string url, byte[] guid, byte[] checksum)
//        {
//            SourceFileEntry entry = new SourceFileEntry(this.file, url, guid, checksum);
//            this.sources.Add(entry);
//            return entry;
//        }
//        public CompileUnitEntry DefineCompilationUnit(SourceFileEntry source)
//        {
//            CompileUnitEntry entry = new CompileUnitEntry(this.file, source);
//            this.comp_units.Add(entry);
//            return entry;
//        }
//        public int DefineNamespace(string name, CompileUnitEntry unit, string[] using_clauses, int parent)
//        {
//            if (unit == null || using_clauses == null)
//            {
//                throw new NullReferenceException();
//            }
//            return unit.DefineNamespace(name, using_clauses, parent);
//        }
//        public int OpenScope(int start_offset)
//        {
//            int result;
//            if (this.current_method == null)
//            {
//                result = 0;
//            }
//            else
//            {
//                this.current_method.StartBlock(CodeBlockEntry.Type.Lexical, start_offset);
//                result = 0;
//            }
//            return result;
//        }
//        public void CloseScope(int end_offset)
//        {
//            if (this.current_method != null)
//            {
//                this.current_method.EndBlock(end_offset);
//            }
//        }
//        public void OpenCompilerGeneratedBlock(int start_offset)
//        {
//            if (this.current_method != null)
//            {
//                this.current_method.StartBlock(CodeBlockEntry.Type.CompilerGenerated, start_offset);
//            }
//        }
//        public void CloseCompilerGeneratedBlock(int end_offset)
//        {
//            if (this.current_method != null)
//            {
//                this.current_method.EndBlock(end_offset);
//            }
//        }
//        public void StartIteratorBody(int start_offset)
//        {
//            this.current_method.StartBlock(CodeBlockEntry.Type.IteratorBody, start_offset);
//        }
//        public void EndIteratorBody(int end_offset)
//        {
//            this.current_method.EndBlock(end_offset);
//        }
//        public void StartIteratorDispatcher(int start_offset)
//        {
//            this.current_method.StartBlock(CodeBlockEntry.Type.IteratorDispatcher, start_offset);
//        }
//        public void EndIteratorDispatcher(int end_offset)
//        {
//            this.current_method.EndBlock(end_offset);
//        }
//        public void DefineAnonymousScope(int id)
//        {
//            this.file.DefineAnonymousScope(id);
//        }
//        public void WriteSymbolFile(Guid guid)
//        {
//            foreach (SourceMethodBuilder method in this.methods)
//            {
//                method.DefineMethod(this.file);
//            }
//            try
//            {
//                File.Delete(this.filename);
//            }
//            catch
//            {
//            }
//            using (FileStream fs = new FileStream(this.filename, FileMode.Create, FileAccess.Write))
//            {
//                this.file.CreateSymbolFile(guid, fs);
//            }
//        }
//    }
//}
