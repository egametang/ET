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

namespace ILRuntime.Mono.Cecil.Cil {

	public sealed class SequencePoint {

		internal InstructionOffset offset;
		Document document;

		int start_line;
		int start_column;
		int end_line;
		int end_column;

		public int Offset {
			get { return offset.Offset; }
		}

		public int StartLine {
			get { return start_line; }
			set { start_line = value; }
		}

		public int StartColumn {
			get { return start_column; }
			set { start_column = value; }
		}

		public int EndLine {
			get { return end_line; }
			set { end_line = value; }
		}

		public int EndColumn {
			get { return end_column; }
			set { end_column = value; }
		}

		public bool IsHidden {
			get { return start_line == 0xfeefee && start_line == end_line; }
		}

		public Document Document {
			get { return document; }
			set { document = value; }
		}

		internal SequencePoint (int offset, Document document)
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			this.offset = new InstructionOffset (offset);
			this.document = document;
		}

		public SequencePoint (Instruction instruction, Document document)
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			this.offset = new InstructionOffset (instruction);
			this.document = document;
		}
	}
}
