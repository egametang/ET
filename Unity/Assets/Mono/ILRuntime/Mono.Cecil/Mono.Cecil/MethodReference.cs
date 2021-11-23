//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;
using System.Text;
using System.Threading;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	public class MethodReference : MemberReference, IMethodSignature, IGenericParameterProvider, IGenericContext {
        int hashCode = -1;
        static int instance_id;
        internal ParameterDefinitionCollection parameters;
		MethodReturnType return_type;

		bool has_this;
		bool explicit_this;
		MethodCallingConvention calling_convention;
		internal Collection<GenericParameter> generic_parameters;

		public virtual bool HasThis {
			get { return has_this; }
			set { has_this = value; }
		}

		public virtual bool ExplicitThis {
			get { return explicit_this; }
			set { explicit_this = value; }
		}

		public virtual MethodCallingConvention CallingConvention {
			get { return calling_convention; }
			set { calling_convention = value; }
		}

		public virtual bool HasParameters {
			get { return !parameters.IsNullOrEmpty (); }
		}

		public virtual Collection<ParameterDefinition> Parameters {
			get {
				if (parameters == null)
					Interlocked.CompareExchange (ref parameters, new ParameterDefinitionCollection (this), null);

				return parameters;
			}
		}

		IGenericParameterProvider IGenericContext.Type {
			get {
				var declaring_type = this.DeclaringType;
				var instance = declaring_type as GenericInstanceType;
				if (instance != null)
					return instance.ElementType;

				return declaring_type;
			}
		}

		IGenericParameterProvider IGenericContext.Method {
			get { return this; }
		}

		GenericParameterType IGenericParameterProvider.GenericParameterType {
			get { return GenericParameterType.Method; }
		}

		public virtual bool HasGenericParameters {
			get { return !generic_parameters.IsNullOrEmpty (); }
		}

		public virtual Collection<GenericParameter> GenericParameters {
			get {
				if (generic_parameters == null)
					Interlocked.CompareExchange (ref generic_parameters, new GenericParameterCollection (this), null);

				return generic_parameters;
			}
		}

		public TypeReference ReturnType {
			get {
				var return_type = MethodReturnType;
				return return_type != null ? return_type.ReturnType : null;
			}
			set {
				var return_type = MethodReturnType;
				if (return_type != null)
					return_type.ReturnType = value;
			}
		}

		public virtual MethodReturnType MethodReturnType {
			get { return return_type; }
			set { return_type = value; }
		}

		public override string FullName {
			get {
				var builder = new StringBuilder ();
				builder.Append (ReturnType.FullName)
					.Append (" ")
					.Append (MemberFullName ());
				this.MethodSignatureFullName (builder);
				return builder.ToString ();
			}
		}

		public virtual bool IsGenericInstance {
			get { return false; }
		}

		public override bool ContainsGenericParameter {
			get {
				if (this.ReturnType.ContainsGenericParameter || base.ContainsGenericParameter)
					return true;

				if (!HasParameters)
					return false;

				var parameters = this.Parameters;

				for (int i = 0; i < parameters.Count; i++)
					if (parameters [i].ParameterType.ContainsGenericParameter)
						return true;

				return false;
			}
		}

        public override int GetHashCode()
        {
            if (hashCode == -1)
                hashCode = System.Threading.Interlocked.Add(ref instance_id, 1);
            return hashCode;
        }

        internal MethodReference ()
		{
			this.return_type = new MethodReturnType (this);
			this.token = new MetadataToken (TokenType.MemberRef);
		}

		public MethodReference (string name, TypeReference returnType)
			: base (name)
		{
			Mixin.CheckType (returnType, Mixin.Argument.returnType);

			this.return_type = new MethodReturnType (this);
			this.return_type.ReturnType = returnType;
			this.token = new MetadataToken (TokenType.MemberRef);
		}

		public MethodReference (string name, TypeReference returnType, TypeReference declaringType)
			: this (name, returnType)
		{
			Mixin.CheckType (declaringType, Mixin.Argument.declaringType);

			this.DeclaringType = declaringType;
		}

		public virtual MethodReference GetElementMethod ()
		{
			return this;
		}

		protected override IMemberDefinition ResolveDefinition ()
		{
			return this.Resolve ();
		}

		public new virtual MethodDefinition Resolve ()
		{
			var module = this.Module;
			if (module == null)
				throw new NotSupportedException ();

			return module.Resolve (this);
		}
	}

	static partial class Mixin {

		public static bool IsVarArg (this IMethodSignature self)
		{
			return self.CallingConvention == MethodCallingConvention.VarArg;
		}

		public static int GetSentinelPosition (this IMethodSignature self)
		{
			if (!self.HasParameters)
				return -1;

			var parameters = self.Parameters;
			for (int i = 0; i < parameters.Count; i++)
				if (parameters [i].ParameterType.IsSentinel)
					return i;

			return -1;
		}
	}
}
