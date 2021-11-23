//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

namespace ILRuntime.Mono.Cecil.Cil {

	public sealed class VariableDefinition : VariableReference {

		public bool IsPinned {
			get { return variable_type.IsPinned; }
		}

		public VariableDefinition (TypeReference variableType)
			: base (variableType)
		{
		}

		public override VariableDefinition Resolve ()
		{
			return this;
		}
	}
}
