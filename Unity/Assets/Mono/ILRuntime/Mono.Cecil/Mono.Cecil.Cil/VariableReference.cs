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

	public abstract class VariableReference {

		internal int index = -1;
		protected TypeReference variable_type;

		public TypeReference VariableType {
			get { return variable_type; }
			set { variable_type = value; }
		}

		public int Index {
			get { return index; }
		}

		internal VariableReference (TypeReference variable_type)
		{
			this.variable_type = variable_type;
		}

		public abstract VariableDefinition Resolve ();

		public override string ToString ()
		{
			if (index >= 0)
				return "V_" + index;

			return string.Empty;
		}
	}
}
