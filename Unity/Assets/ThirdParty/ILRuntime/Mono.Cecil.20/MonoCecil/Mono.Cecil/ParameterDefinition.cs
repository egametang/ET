//
// ParameterDefinition.cs
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

using Mono.Collections.Generic;

namespace Mono.Cecil
{

    public sealed class ParameterDefinition : ParameterReference, ICustomAttributeProvider, IConstantProvider, IMarshalInfoProvider
    {

        ushort attributes;

        internal IMethodSignature method;

        object constant = Mixin.NotResolved;
        Collection<CustomAttribute> custom_attributes;
        MarshalInfo marshal_info;

        public ParameterAttributes Attributes
        {
            get { return (ParameterAttributes)attributes; }
            set { attributes = (ushort)value; }
        }

        public IMethodSignature Method
        {
            get { return method; }
        }

        public int Sequence
        {
            get
            {
                if (method == null)
                    return -1;

                return Mixin.HasImplicitThis(method) ? index + 1 : index;
            }
        }

        public bool HasConstant
        {
            get
            {
                Mixin.ResolveConstant(this, ref constant, parameter_type.Module);

                return constant != Mixin.NoValue;
            }
            set { if (!value) constant = Mixin.NoValue; }
        }

        public object Constant
        {
            get { return HasConstant ? constant : null; }
            set { constant = value; }
        }

        public bool HasCustomAttributes
        {
            get
            {
                if (custom_attributes != null)
                    return custom_attributes.Count > 0;

                return Mixin.GetHasCustomAttributes(this, parameter_type.Module);
            }
        }

        public Collection<CustomAttribute> CustomAttributes
        {
            get { return custom_attributes ?? (Mixin.GetCustomAttributes(this, ref custom_attributes, parameter_type.Module)); }
        }

        public bool HasMarshalInfo
        {
            get
            {
                if (marshal_info != null)
                    return true;

                return Mixin.GetHasMarshalInfo(this, parameter_type.Module);
            }
        }

        public MarshalInfo MarshalInfo
        {
            get { return marshal_info ?? (Mixin.GetMarshalInfo(this, ref marshal_info, parameter_type.Module)); }
            set { marshal_info = value; }
        }

        #region ParameterAttributes

        public bool IsIn
        {
            get { return Mixin.GetAttributes(attributes, (ushort)ParameterAttributes.In); }
            set { attributes = Mixin.SetAttributes(attributes, (ushort)ParameterAttributes.In, value); }
        }

        public bool IsOut
        {
            get { return Mixin.GetAttributes(attributes, (ushort)ParameterAttributes.Out); }
            set { attributes = Mixin.SetAttributes(attributes, (ushort)ParameterAttributes.Out, value); }
        }

        public bool IsLcid
        {
            get { return Mixin.GetAttributes(attributes, (ushort)ParameterAttributes.Lcid); }
            set { attributes = Mixin.SetAttributes(attributes, (ushort)ParameterAttributes.Lcid, value); }
        }

        public bool IsReturnValue
        {
            get { return Mixin.GetAttributes(attributes, (ushort)ParameterAttributes.Retval); }
            set { attributes = Mixin.SetAttributes(attributes, (ushort)ParameterAttributes.Retval, value); }
        }

        public bool IsOptional
        {
            get { return Mixin.GetAttributes(attributes, (ushort)ParameterAttributes.Optional); }
            set { attributes = Mixin.SetAttributes(attributes, (ushort)ParameterAttributes.Optional, value); }
        }

        public bool HasDefault
        {
            get { return Mixin.GetAttributes(attributes, (ushort)ParameterAttributes.HasDefault); }
            set { attributes = Mixin.SetAttributes(attributes, (ushort)ParameterAttributes.HasDefault, value); }
        }

        public bool HasFieldMarshal
        {
            get { return Mixin.GetAttributes(attributes, (ushort)ParameterAttributes.HasFieldMarshal); }
            set { attributes = Mixin.SetAttributes(attributes, (ushort)ParameterAttributes.HasFieldMarshal, value); }
        }

        #endregion

        internal ParameterDefinition(TypeReference parameterType, IMethodSignature method)
            : this(string.Empty, ParameterAttributes.None, parameterType)
        {
            this.method = method;
        }

        public ParameterDefinition(TypeReference parameterType)
            : this(string.Empty, ParameterAttributes.None, parameterType)
        {
        }

        public ParameterDefinition(string name, ParameterAttributes attributes, TypeReference parameterType)
            : base(name, parameterType)
        {
            this.attributes = (ushort)attributes;
            this.token = new MetadataToken(TokenType.Param);
        }

        public override ParameterDefinition Resolve()
        {
            return this;
        }
    }
}
