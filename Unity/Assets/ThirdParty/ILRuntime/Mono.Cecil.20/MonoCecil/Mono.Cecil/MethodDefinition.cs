//
// MethodDefinition.cs
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

using Mono.Cecil.Cil;
using Mono.Collections.Generic;

using RVA = System.UInt32;

namespace Mono.Cecil
{

    public sealed class MethodDefinition : MethodReference, IMemberDefinition, ISecurityDeclarationProvider
    {

        ushort attributes;
        ushort impl_attributes;
        internal volatile bool sem_attrs_ready;
        internal MethodSemanticsAttributes sem_attrs;
        Collection<CustomAttribute> custom_attributes;
        Collection<SecurityDeclaration> security_declarations;

        internal RVA rva;
        internal PInvokeInfo pinvoke;
        Collection<MethodReference> overrides;

        internal MethodBody body;

        public MethodAttributes Attributes
        {
            get { return (MethodAttributes)attributes; }
            set { attributes = (ushort)value; }
        }

        public MethodImplAttributes ImplAttributes
        {
            get { return (MethodImplAttributes)impl_attributes; }
            set { impl_attributes = (ushort)value; }
        }

        public MethodSemanticsAttributes SemanticsAttributes
        {
            get
            {
                if (sem_attrs_ready)
                    return sem_attrs;

                if (HasImage)
                {
                    ReadSemantics();
                    return sem_attrs;
                }

                sem_attrs = MethodSemanticsAttributes.None;
                sem_attrs_ready = true;
                return sem_attrs;
            }
            set { sem_attrs = value; }
        }

        internal void ReadSemantics()
        {
            if (sem_attrs_ready)
                return;

            var module = this.Module;
            if (module == null)
                return;

            if (!module.HasImage)
                return;

            module.Read(this, (method, reader) => reader.ReadAllSemantics(method));
        }

        public bool HasSecurityDeclarations
        {
            get
            {
                if (security_declarations != null)
                    return security_declarations.Count > 0;

                return Mixin.GetHasSecurityDeclarations(this, Module);
            }
        }

        public Collection<SecurityDeclaration> SecurityDeclarations
        {
            get { return security_declarations ?? (Mixin.GetSecurityDeclarations(this, ref security_declarations, Module)); }
        }

        public bool HasCustomAttributes
        {
            get
            {
                if (custom_attributes != null)
                    return custom_attributes.Count > 0;

                return Mixin.GetHasCustomAttributes(this, Module);
            }
        }

        public Collection<CustomAttribute> CustomAttributes
        {
            get { return custom_attributes ?? (Mixin.GetCustomAttributes(this, ref custom_attributes, Module)); }
        }

        public int RVA
        {
            get { return (int)rva; }
        }

        public bool HasBody
        {
            get
            {
                return (attributes & (ushort)MethodAttributes.Abstract) == 0 &&
                    (attributes & (ushort)MethodAttributes.PInvokeImpl) == 0 &&
                    (impl_attributes & (ushort)MethodImplAttributes.InternalCall) == 0 &&
                    (impl_attributes & (ushort)MethodImplAttributes.Native) == 0 &&
                    (impl_attributes & (ushort)MethodImplAttributes.Unmanaged) == 0 &&
                    (impl_attributes & (ushort)MethodImplAttributes.Runtime) == 0;
            }
        }

        public MethodBody Body
        {
            get
            {
                MethodBody localBody = this.body;
                if (localBody != null)
                    return localBody;

                if (!HasBody)
                    return null;

                if (HasImage && rva != 0)
                    return Module.Read(ref body, this, (method, reader) => reader.ReadMethodBody(method));

                return body = new MethodBody(this);
            }
            set
            {
                // we reset Body to null in ILSpy to save memory; so we need that operation to be thread-safe
                lock (Module.SyncRoot)
                {
                    body = value;
                }
            }
        }

        public bool HasPInvokeInfo
        {
            get
            {
                if (pinvoke != null)
                    return true;

                return IsPInvokeImpl;
            }
        }

        public PInvokeInfo PInvokeInfo
        {
            get
            {
                if (pinvoke != null)
                    return pinvoke;

                if (HasImage && IsPInvokeImpl)
                    return Module.Read(ref pinvoke, this, (method, reader) => reader.ReadPInvokeInfo(method));

                return null;
            }
            set
            {
                IsPInvokeImpl = true;
                pinvoke = value;
            }
        }

        public bool HasOverrides
        {
            get
            {
                if (overrides != null)
                    return overrides.Count > 0;

                if (HasImage)
                    return Module.Read(this, (method, reader) => reader.HasOverrides(method));

                return false;
            }
        }

        public Collection<MethodReference> Overrides
        {
            get
            {
                if (overrides != null)
                    return overrides;

                if (HasImage)
                    return Module.Read(ref overrides, this, (method, reader) => reader.ReadOverrides(method));

                return overrides = new Collection<MethodReference>();
            }
        }

        public override bool HasGenericParameters
        {
            get
            {
                if (generic_parameters != null)
                    return generic_parameters.Count > 0;

                return Mixin.GetHasGenericParameters(this, Module);
            }
        }

        public override Collection<GenericParameter> GenericParameters
        {
            get { return generic_parameters ?? (Mixin.GetGenericParameters(this, ref generic_parameters, Module)); }
        }

        #region MethodAttributes

        public bool IsCompilerControlled
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.CompilerControlled); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.CompilerControlled, value); }
        }

        public bool IsPrivate
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Private); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Private, value); }
        }

        public bool IsFamilyAndAssembly
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamANDAssem); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamANDAssem, value); }
        }

        public bool IsAssembly
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Assembly); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Assembly, value); }
        }

        public bool IsFamily
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Family); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Family, value); }
        }

        public bool IsFamilyOrAssembly
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamORAssem); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.FamORAssem, value); }
        }

        public bool IsPublic
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Public); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.MemberAccessMask, (ushort)MethodAttributes.Public, value); }
        }

        public bool IsStatic
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.Static); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.Static, value); }
        }

        public bool IsFinal
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.Final); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.Final, value); }
        }

        public bool IsVirtual
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.Virtual); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.Virtual, value); }
        }

        public bool IsHideBySig
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.HideBySig); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.HideBySig, value); }
        }

        public bool IsReuseSlot
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.ReuseSlot); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.ReuseSlot, value); }
        }

        public bool IsNewSlot
        {
            get { return Mixin.GetMaskedAttributes(attributes,(ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.NewSlot); }
            set { attributes = Mixin.SetMaskedAttributes(attributes,(ushort)MethodAttributes.VtableLayoutMask, (ushort)MethodAttributes.NewSlot, value); }
        }

        public bool IsCheckAccessOnOverride
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.CheckAccessOnOverride); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.CheckAccessOnOverride, value); }
        }

        public bool IsAbstract
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.Abstract); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.Abstract, value); }
        }

        public bool IsSpecialName
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.SpecialName); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.SpecialName, value); }
        }

        public bool IsPInvokeImpl
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.PInvokeImpl); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.PInvokeImpl, value); }
        }

        public bool IsUnmanagedExport
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.UnmanagedExport); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.UnmanagedExport, value); }
        }

        public bool IsRuntimeSpecialName
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.RTSpecialName); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.RTSpecialName, value); }
        }

        public bool HasSecurity
        {
            get { return Mixin.GetAttributes(attributes,(ushort)MethodAttributes.HasSecurity); }
            set { attributes =Mixin.SetAttributes(attributes,(ushort)MethodAttributes.HasSecurity, value); }
        }

        #endregion

        #region MethodImplAttributes

        public bool IsIL
        {
            get { return Mixin.GetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.IL); }
            set { impl_attributes = Mixin.SetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.IL, value); }
        }

        public bool IsNative
        {
            get { return Mixin.GetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Native); }
            set { impl_attributes = Mixin.SetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Native, value); }
        }

        public bool IsRuntime
        {
            get { return Mixin.GetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Runtime); }
            set { impl_attributes = Mixin.SetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.CodeTypeMask, (ushort)MethodImplAttributes.Runtime, value); }
        }

        public bool IsUnmanaged
        {
            get { return Mixin.GetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Unmanaged); }
            set { impl_attributes = Mixin.SetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Unmanaged, value); }
        }

        public bool IsManaged
        {
            get { return Mixin.GetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Managed); }
            set { impl_attributes = Mixin.SetMaskedAttributes(impl_attributes,(ushort)MethodImplAttributes.ManagedMask, (ushort)MethodImplAttributes.Managed, value); }
        }

        public bool IsForwardRef
        {
            get { return Mixin.GetAttributes(impl_attributes, (ushort)MethodImplAttributes.ForwardRef); }
            set { impl_attributes = Mixin.SetAttributes(impl_attributes,(ushort)MethodImplAttributes.ForwardRef, value); }
        }

        public bool IsPreserveSig
        {
            get { return Mixin.GetAttributes(impl_attributes, (ushort)MethodImplAttributes.PreserveSig); }
            set { impl_attributes = Mixin.SetAttributes(impl_attributes,(ushort)MethodImplAttributes.PreserveSig, value); }
        }

        public bool IsInternalCall
        {
            get { return Mixin.GetAttributes(impl_attributes, (ushort)MethodImplAttributes.InternalCall); }
            set { impl_attributes = Mixin.SetAttributes(impl_attributes,(ushort)MethodImplAttributes.InternalCall, value); }
        }

        public bool IsSynchronized
        {
            get { return Mixin.GetAttributes(impl_attributes, (ushort)MethodImplAttributes.Synchronized); }
            set { impl_attributes = Mixin.SetAttributes(impl_attributes,(ushort)MethodImplAttributes.Synchronized, value); }
        }

        public bool NoInlining
        {
            get { return Mixin.GetAttributes(impl_attributes, (ushort)MethodImplAttributes.NoInlining); }
            set { impl_attributes = Mixin.SetAttributes(impl_attributes,(ushort)MethodImplAttributes.NoInlining, value); }
        }

        public bool NoOptimization
        {
            get { return Mixin.GetAttributes(impl_attributes, (ushort)MethodImplAttributes.NoOptimization); }
            set { impl_attributes = Mixin.SetAttributes(impl_attributes,(ushort)MethodImplAttributes.NoOptimization, value); }
        }

        #endregion

        #region MethodSemanticsAttributes

        public bool IsSetter
        {
            get { return Mixin.GetSemantics(this, MethodSemanticsAttributes.Setter); }
            set { Mixin.SetSemantics(this, MethodSemanticsAttributes.Setter, value); }
        }

        public bool IsGetter
        {
            get { return Mixin.GetSemantics(this, MethodSemanticsAttributes.Getter); }
            set { Mixin.SetSemantics(this, MethodSemanticsAttributes.Getter, value); }
        }

        public bool IsOther
        {
            get { return Mixin.GetSemantics(this, MethodSemanticsAttributes.Other); }
            set { Mixin.SetSemantics(this, MethodSemanticsAttributes.Other, value); }
        }

        public bool IsAddOn
        {
            get { return Mixin.GetSemantics(this, MethodSemanticsAttributes.AddOn); }
            set { Mixin.SetSemantics(this, MethodSemanticsAttributes.AddOn, value); }
        }

        public bool IsRemoveOn
        {
            get { return Mixin.GetSemantics(this, MethodSemanticsAttributes.RemoveOn); }
            set { Mixin.SetSemantics(this, MethodSemanticsAttributes.RemoveOn, value); }
        }

        public bool IsFire
        {
            get { return Mixin.GetSemantics(this, MethodSemanticsAttributes.Fire); }
            set { Mixin.SetSemantics(this, MethodSemanticsAttributes.Fire, value); }
        }

        #endregion

        public new TypeDefinition DeclaringType
        {
            get { return (TypeDefinition)base.DeclaringType; }
            set { base.DeclaringType = value; }
        }

        public bool IsConstructor
        {
            get
            {
                return this.IsRuntimeSpecialName
                    && this.IsSpecialName
                    && (this.Name == ".cctor" || this.Name == ".ctor");
            }
        }

        public override bool IsDefinition
        {
            get { return true; }
        }

        internal MethodDefinition()
        {
            this.token = new MetadataToken(TokenType.Method);
        }

        public MethodDefinition(string name, MethodAttributes attributes, TypeReference returnType)
            : base(name, returnType)
        {
            this.attributes = (ushort)attributes;
            this.HasThis = !this.IsStatic;
            this.token = new MetadataToken(TokenType.Method);
        }

        public override MethodDefinition Resolve()
        {
            return this;
        }
    }

    static partial class Mixin
    {

        public static ParameterDefinition GetParameter(MethodBody self, int index)
        {
            var method = self.method;

            if (method.HasThis)
            {
                if (index == 0)
                    return self.ThisParameter;

                index--;
            }

            var parameters = method.Parameters;

            if (index < 0 || index >= parameters.size)
                return null;

            return parameters[index];
        }

        public static VariableDefinition GetVariable(MethodBody self, int index)
        {
            var variables = self.Variables;

            if (index < 0 || index >= variables.size)
                return null;

            return variables[index];
        }

        public static bool GetSemantics(MethodDefinition self, MethodSemanticsAttributes semantics)
        {
            return (self.SemanticsAttributes & semantics) != 0;
        }

        public static void SetSemantics(MethodDefinition self, MethodSemanticsAttributes semantics, bool value)
        {
            if (value)
                self.SemanticsAttributes |= semantics;
            else
                self.SemanticsAttributes &= ~semantics;
        }
    }
}
