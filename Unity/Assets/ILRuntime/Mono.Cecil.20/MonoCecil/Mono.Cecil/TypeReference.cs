//
// TypeReference.cs
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

using Mono.Cecil.Metadata;
using Mono.Collections.Generic;

namespace Mono.Cecil
{

    public enum MetadataType : byte
    {
        Void = ElementType.Void,
        Boolean = ElementType.Boolean,
        Char = ElementType.Char,
        SByte = ElementType.I1,
        Byte = ElementType.U1,
        Int16 = ElementType.I2,
        UInt16 = ElementType.U2,
        Int32 = ElementType.I4,
        UInt32 = ElementType.U4,
        Int64 = ElementType.I8,
        UInt64 = ElementType.U8,
        Single = ElementType.R4,
        Double = ElementType.R8,
        String = ElementType.String,
        Pointer = ElementType.Ptr,
        ByReference = ElementType.ByRef,
        ValueType = ElementType.ValueType,
        Class = ElementType.Class,
        Var = ElementType.Var,
        Array = ElementType.Array,
        GenericInstance = ElementType.GenericInst,
        TypedByReference = ElementType.TypedByRef,
        IntPtr = ElementType.I,
        UIntPtr = ElementType.U,
        FunctionPointer = ElementType.FnPtr,
        Object = ElementType.Object,
        MVar = ElementType.MVar,
        RequiredModifier = ElementType.CModReqD,
        OptionalModifier = ElementType.CModOpt,
        Sentinel = ElementType.Sentinel,
        Pinned = ElementType.Pinned,
    }

    public class TypeReference : MemberReference, IGenericParameterProvider, IGenericContext
    {

        string @namespace;
        bool value_type;
        int hashCode = -1;
        static int instance_id;
        internal IMetadataScope scope;
        internal ModuleDefinition module;

        internal ElementType etype = ElementType.None;

        string fullname;

        protected Collection<GenericParameter> generic_parameters;

        public override string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                fullname = null;
            }
        }

        public virtual string Namespace
        {
            get { return @namespace; }
            set
            {
                @namespace = value;
                fullname = null;
            }
        }

        public virtual bool IsValueType
        {
            get { return value_type; }
            set { value_type = value; }
        }

        public override ModuleDefinition Module
        {
            get
            {
                if (module != null)
                    return module;

                var declaring_type = this.DeclaringType;
                if (declaring_type != null)
                    return declaring_type.Module;

                return null;
            }
        }

        IGenericParameterProvider IGenericContext.Type
        {
            get { return this; }
        }

        IGenericParameterProvider IGenericContext.Method
        {
            get { return null; }
        }

        GenericParameterType IGenericParameterProvider.GenericParameterType
        {
            get { return GenericParameterType.Type; }
        }

        public virtual bool HasGenericParameters
        {
            get { return !Mixin.IsNullOrEmpty(generic_parameters); }
        }

        public virtual Collection<GenericParameter> GenericParameters
        {
            get
            {
                if (generic_parameters != null)
                    return generic_parameters;

                return generic_parameters = new GenericParameterCollection(this);
            }
        }

        public virtual IMetadataScope Scope
        {
            get
            {
                var declaring_type = this.DeclaringType;
                if (declaring_type != null)
                    return declaring_type.Scope;

                return scope;
            }
        }

        public bool IsNested
        {
            get { return this.DeclaringType != null; }
        }

        public override TypeReference DeclaringType
        {
            get { return base.DeclaringType; }
            set
            {
                base.DeclaringType = value;
                fullname = null;
            }
        }

        public override string FullName
        {
            get
            {
                if (fullname != null)
                    return fullname;

                if (IsNested)
                    return fullname = DeclaringType.FullName + "/" + Name;

                if (string.IsNullOrEmpty(@namespace))
                    return fullname = Name;

                return fullname = @namespace + "." + Name;
            }
        }

        public virtual bool IsByReference
        {
            get { return false; }
        }

        public virtual bool IsPointer
        {
            get { return false; }
        }

        public virtual bool IsSentinel
        {
            get { return false; }
        }

        public virtual bool IsArray
        {
            get { return false; }
        }

        public virtual bool IsGenericParameter
        {
            get { return false; }
        }

        public virtual bool IsGenericInstance
        {
            get { return false; }
        }

        public virtual bool IsRequiredModifier
        {
            get { return false; }
        }

        public virtual bool IsOptionalModifier
        {
            get { return false; }
        }

        public virtual bool IsPinned
        {
            get { return false; }
        }

        public virtual bool IsFunctionPointer
        {
            get { return false; }
        }

        public override int GetHashCode()
        {
            if (hashCode == -1)
                hashCode = System.Threading.Interlocked.Add(ref instance_id, 1);
            return hashCode;
        }
        public virtual bool IsPrimitive
        {
            get { return Mixin.IsPrimitive(etype); }
        }

        public virtual MetadataType MetadataType
        {
            get
            {
                switch (etype)
                {
                    case ElementType.None:
                        return IsValueType ? MetadataType.ValueType : MetadataType.Class;
                    default:
                        return (MetadataType)etype;
                }
            }
        }

        protected TypeReference(string @namespace, string name)
            : base(name)
        {
            this.@namespace = @namespace ?? string.Empty;
            this.token = new MetadataToken(TokenType.TypeRef, 0);
        }

        public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope)
            : this(@namespace, name)
        {
            this.module = module;
            this.scope = scope;
        }

        public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope, bool valueType) :
            this(@namespace, name, module, scope)
        {
            value_type = valueType;
        }

        public virtual TypeReference GetElementType()
        {
            return this;
        }

        public virtual TypeDefinition Resolve()
        {
            var module = this.Module;
            if (module == null)
                throw new NotSupportedException();

            return module.Resolve(this);
        }
    }

    static partial class Mixin
    {

        public static bool IsPrimitive(ElementType self)
        {
            switch (self)
            {
                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I:
                case ElementType.U:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R4:
                case ElementType.R8:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsTypeOf(TypeReference self, string @namespace, string name)
        {
            return self.Name == name
                && self.Namespace == @namespace;
        }

        public static bool IsTypeSpecification(TypeReference type)
        {
            switch (type.etype)
            {
                case ElementType.Array:
                case ElementType.ByRef:
                case ElementType.CModOpt:
                case ElementType.CModReqD:
                case ElementType.FnPtr:
                case ElementType.GenericInst:
                case ElementType.MVar:
                case ElementType.Pinned:
                case ElementType.Ptr:
                case ElementType.SzArray:
                case ElementType.Sentinel:
                case ElementType.Var:
                    return true;
            }

            return false;
        }

        public static TypeDefinition CheckedResolve(TypeReference self)
        {
            var type = self.Resolve();
            if (type == null)
                throw new ResolutionException(self);

            return type;
        }
    }
}
