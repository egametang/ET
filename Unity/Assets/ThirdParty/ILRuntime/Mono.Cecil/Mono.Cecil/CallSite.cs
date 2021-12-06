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

using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil {

	public sealed class CallSite : IMethodSignature {

		readonly MethodReference signature;

		public bool HasThis {
			get { return signature.HasThis; }
			set { signature.HasThis = value; }
		}

		public bool ExplicitThis {
			get { return signature.ExplicitThis; }
			set { signature.ExplicitThis = value; }
		}

		public MethodCallingConvention CallingConvention {
			get { return signature.CallingConvention; }
			set { signature.CallingConvention = value; }
		}

		public bool HasParameters {
			get { return signature.HasParameters; }
		}

		public Collection<ParameterDefinition> Parameters {
			get { return signature.Parameters; }
		}

		public TypeReference ReturnType {
			get { return signature.MethodReturnType.ReturnType; }
			set { signature.MethodReturnType.ReturnType = value; }
		}

		public MethodReturnType MethodReturnType {
			get { return signature.MethodReturnType; }
		}

		public string Name {
			get { return string.Empty; }
			set { throw new InvalidOperationException (); }
		}

		public string Namespace {
			get { return string.Empty; }
			set { throw new InvalidOperationException (); }
		}

		public ModuleDefinition Module {
			get { return ReturnType.Module; }
		}

		public IMetadataScope Scope {
			get { return signature.ReturnType.Scope; }
		}

		public MetadataToken MetadataToken {
			get { return signature.token; }
			set { signature.token = value; }
		}

		public string FullName {
			get {
				var signature = new StringBuilder ();
				signature.Append (ReturnType.FullName);
				this.MethodSignatureFullName (signature);
				return signature.ToString ();
			}
		}

		internal CallSite ()
		{
			this.signature = new MethodReference ();
			this.signature.token = new MetadataToken (TokenType.Signature, 0);
		}

		public CallSite (TypeReference returnType)
			: this ()
		{
			if (returnType == null)
				throw new ArgumentNullException ("returnType");

			this.signature.ReturnType = returnType;
		}

		public override string ToString ()
		{
			return FullName;
		}
	}
}
