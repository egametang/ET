//using System;
//using System.Collections;
//using System.Diagnostics.SymbolStore;
//using System.Reflection;
//using System.Text;
//namespace Mono.CompilerServices.SymbolWriter
//{
//    public class SymbolWriterImpl : ISymbolWriter
//    {
//        private MonoSymbolWriter msw;
//        private int nextLocalIndex;
//        private int currentToken;
//        private string methodName;
//        private Stack namespaceStack = new Stack();
//        private bool methodOpened;
//        private Hashtable documents = new Hashtable();
//        private Guid guid;
//        public SymbolWriterImpl(Guid guid)
//        {
//            this.guid = guid;
//        }
//        public void Close()
//        {
//            this.msw.WriteSymbolFile(this.guid);
//        }
//        public void CloseMethod()
//        {
//            if (this.methodOpened)
//            {
//                this.methodOpened = false;
//                this.nextLocalIndex = 0;
//                this.msw.CloseMethod();
//            }
//        }
//        public void CloseNamespace()
//        {
//            this.namespaceStack.Pop();
//            this.msw.CloseNamespace();
//        }
//        public void CloseScope(int endOffset)
//        {
//            this.msw.CloseScope(endOffset);
//        }
//        public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
//        {
//            SymbolDocumentWriterImpl doc = (SymbolDocumentWriterImpl)this.documents[url];
//            if (doc == null)
//            {
//                SourceFileEntry entry = this.msw.DefineDocument(url);
//                CompileUnitEntry comp_unit = this.msw.DefineCompilationUnit(entry);
//                doc = new SymbolDocumentWriterImpl(comp_unit);
//                this.documents[url] = doc;
//            }
//            return doc;
//        }
//        public void DefineField(SymbolToken parent, string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
//        {
//        }
//        public void DefineGlobalVariable(string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
//        {
//        }
//        public void DefineLocalVariable(string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset)
//        {
//            this.msw.DefineLocalVariable(this.nextLocalIndex++, name);
//        }
//        public void DefineParameter(string name, ParameterAttributes attributes, int sequence, SymAddressKind addrKind, int addr1, int addr2, int addr3)
//        {
//        }
//        public void DefineSequencePoints(ISymbolDocumentWriter document, int[] offsets, int[] lines, int[] columns, int[] endLines, int[] endColumns)
//        {
//            SymbolDocumentWriterImpl doc = (SymbolDocumentWriterImpl)document;
//            SourceFileEntry file = (doc != null) ? doc.Entry.SourceFile : null;
//            for (int i = 0; i < offsets.Length; i++)
//            {
//                if (i <= 0 || offsets[i] != offsets[i - 1] || lines[i] != lines[i - 1] || columns[i] != columns[i - 1])
//                {
//                    this.msw.MarkSequencePoint(offsets[i], file, lines[i], columns[i], false);
//                }
//            }
//        }
//        public void Initialize(IntPtr emitter, string filename, bool fFullBuild)
//        {
//            this.msw = new MonoSymbolWriter(filename);
//        }
//        public void OpenMethod(SymbolToken method)
//        {
//            this.currentToken = method.GetToken();
//        }
//        public void OpenNamespace(string name)
//        {
//            NamespaceInfo i = new NamespaceInfo();
//            i.NamespaceID = -1;
//            i.Name = name;
//            this.namespaceStack.Push(i);
//        }
//        public int OpenScope(int startOffset)
//        {
//            return this.msw.OpenScope(startOffset);
//        }
//        public void SetMethodSourceRange(ISymbolDocumentWriter startDoc, int startLine, int startColumn, ISymbolDocumentWriter endDoc, int endLine, int endColumn)
//        {
//            int nsId = this.GetCurrentNamespace(startDoc);
//            SourceMethodImpl sm = new SourceMethodImpl(this.methodName, this.currentToken, nsId);
//            this.msw.OpenMethod(((ICompileUnit)startDoc).Entry, nsId, sm);
//            this.methodOpened = true;
//        }
//        public void SetScopeRange(int scopeID, int startOffset, int endOffset)
//        {
//        }
//        public void SetSymAttribute(SymbolToken parent, string name, byte[] data)
//        {
//            if (name == "__name")
//            {
//                this.methodName = Encoding.UTF8.GetString(data);
//            }
//        }
//        public void SetUnderlyingWriter(IntPtr underlyingWriter)
//        {
//        }
//        public void SetUserEntryPoint(SymbolToken entryMethod)
//        {
//        }
//        public void UsingNamespace(string fullName)
//        {
//            if (this.namespaceStack.Count == 0)
//            {
//                this.OpenNamespace("");
//            }
//            NamespaceInfo ni = (NamespaceInfo)this.namespaceStack.Peek();
//            if (ni.NamespaceID != -1)
//            {
//                NamespaceInfo old = ni;
//                this.CloseNamespace();
//                this.OpenNamespace(old.Name);
//                ni = (NamespaceInfo)this.namespaceStack.Peek();
//                ni.UsingClauses = old.UsingClauses;
//            }
//            ni.UsingClauses.Add(fullName);
//        }
//        private int GetCurrentNamespace(ISymbolDocumentWriter doc)
//        {
//            if (this.namespaceStack.Count == 0)
//            {
//                this.OpenNamespace("");
//            }
//            NamespaceInfo ni = (NamespaceInfo)this.namespaceStack.Peek();
//            if (ni.NamespaceID == -1)
//            {
//                string[] usings = (string[])ni.UsingClauses.ToArray(typeof(string));
//                int parentId = 0;
//                if (this.namespaceStack.Count > 1)
//                {
//                    this.namespaceStack.Pop();
//                    parentId = ((NamespaceInfo)this.namespaceStack.Peek()).NamespaceID;
//                    this.namespaceStack.Push(ni);
//                }
//                ni.NamespaceID = this.msw.DefineNamespace(ni.Name, ((ICompileUnit)doc).Entry, usings, parentId);
//            }
//            return ni.NamespaceID;
//        }
//    }
//}
