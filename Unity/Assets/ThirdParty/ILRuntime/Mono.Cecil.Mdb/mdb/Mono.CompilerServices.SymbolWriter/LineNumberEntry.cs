using System;
using System.Collections.Generic;
namespace Mono.CompilerServices.SymbolWriter
{
	public class LineNumberEntry
	{
		private class OffsetComparerClass : IComparer<LineNumberEntry>
		{
			public int Compare(LineNumberEntry l1, LineNumberEntry l2)
			{
				int result;
				if (l1.Offset < l2.Offset)
				{
					result = -1;
				}
				else
				{
					if (l1.Offset > l2.Offset)
					{
						result = 1;
					}
					else
					{
						result = 0;
					}
				}
				return result;
			}
		}
		private class RowComparerClass : IComparer<LineNumberEntry>
		{
			public int Compare(LineNumberEntry l1, LineNumberEntry l2)
			{
				int result;
				if (l1.Row < l2.Row)
				{
					result = -1;
				}
				else
				{
					if (l1.Row > l2.Row)
					{
						result = 1;
					}
					else
					{
						result = 0;
					}
				}
				return result;
			}
		}
		public readonly int Row;
		public readonly int File;
		public readonly int Offset;
		public readonly bool IsHidden;
		public static LineNumberEntry Null = new LineNumberEntry(0, 0, 0);
		public static readonly IComparer<LineNumberEntry> OffsetComparer = new LineNumberEntry.OffsetComparerClass();
		public static readonly IComparer<LineNumberEntry> RowComparer = new LineNumberEntry.RowComparerClass();
		public LineNumberEntry(int file, int row, int offset) : this(file, row, offset, false)
		{
		}
		public LineNumberEntry(int file, int row, int offset, bool is_hidden)
		{
			this.File = file;
			this.Row = row;
			this.Offset = offset;
			this.IsHidden = is_hidden;
		}
		public override string ToString()
		{
			return string.Format("[Line {0}:{1}:{2}]", this.File, this.Row, this.Offset);
		}
	}
}
