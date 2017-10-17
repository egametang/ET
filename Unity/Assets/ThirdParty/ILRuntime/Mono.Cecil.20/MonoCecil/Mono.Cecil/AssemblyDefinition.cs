//
// AssemblyDefinition.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2011 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

using Mono.Collections.Generic;

namespace Mono.Cecil
{

    public sealed class AssemblyDefinition : ICustomAttributeProvider, ISecurityDeclarationProvider
    {

        AssemblyNameDefinition name;

        internal ModuleDefinition main_module;
        Collection<ModuleDefinition> modules;
        Collection<CustomAttribute> custom_attributes;
        Collection<SecurityDeclaration> security_declarations;

        public AssemblyNameDefinition Name
        {
            get { return name; }
            set { name = value; }
        }

        public string FullName
        {
            get { return name != null ? name.FullName : string.Empty; }
        }

        public MetadataToken MetadataToken
        {
            get { return new MetadataToken(TokenType.Assembly, 1); }
            set { }
        }

        public Collection<ModuleDefinition> Modules
        {
            get
            {
                if (modules != null)
                    return modules;

                if (main_module.HasImage)
                    return main_module.Read(ref modules, this, (_, reader) => reader.ReadModules());

                return modules = new Collection<ModuleDefinition>(1) { main_module };
            }
        }

        public ModuleDefinition MainModule
        {
            get { return main_module; }
        }

        public MethodDefinition EntryPoint
        {
            get { return main_module.EntryPoint; }
            set { main_module.EntryPoint = value; }
        }

        public bool HasCustomAttributes
        {
            get
            {
                if (custom_attributes != null)
                    return custom_attributes.Count > 0;

                return Mixin.GetHasCustomAttributes(this, main_module);
            }
        }

        public Collection<CustomAttribute> CustomAttributes
        {
            get { return custom_attributes ?? (Mixin.GetCustomAttributes(this, ref custom_attributes, main_module)); }
        }

        public bool HasSecurityDeclarations
        {
            get
            {
                if (security_declarations != null)
                    return security_declarations.Count > 0;

                return Mixin.GetHasSecurityDeclarations(this, main_module);
            }
        }

        public Collection<SecurityDeclaration> SecurityDeclarations
        {
            get { return security_declarations ?? (Mixin.GetSecurityDeclarations(this, ref security_declarations, main_module)); }
        }

        internal AssemblyDefinition()
        {
        }



        public static AssemblyDefinition ReadAssembly(string fileName)
        {
            return ReadAssembly(ModuleDefinition.ReadModule(fileName));
        }

        public static AssemblyDefinition ReadAssembly(string fileName, ReaderParameters parameters)
        {
            return ReadAssembly(ModuleDefinition.ReadModule(fileName, parameters));
        }

        public static AssemblyDefinition ReadAssembly(Stream stream)
        {
            return ReadAssembly(ModuleDefinition.ReadModule(stream));
        }

        public static AssemblyDefinition ReadAssembly(Stream stream, ReaderParameters parameters)
        {
            return ReadAssembly(ModuleDefinition.ReadModule(stream, parameters));
        }

        static AssemblyDefinition ReadAssembly(ModuleDefinition module)
        {
            var assembly = module.Assembly;
            if (assembly == null)
                throw new ArgumentException();

            return assembly;
        }



        public override string ToString()
        {
            return this.FullName;
        }
    }
}
