using System;
using System.Collections.Generic;
namespace Mono.CompilerServices.SymbolWriter
{
	public class LineNumberTable
	{
		public const int Default_LineBase = -1;
		public const int Default_LineRange = 8;
		public const byte Default_OpcodeBase = 9;
		public const bool SuppressDuplicates = true;
		public const byte DW_LNS_copy = 1;
		public const byte DW_LNS_advance_pc = 2;
		public const byte DW_LNS_advance_line = 3;
		public const byte DW_LNS_set_file = 4;
		public const byte DW_LNS_const_add_pc = 8;
		public const byte DW_LNE_end_sequence = 1;
		public const byte DW_LNE_MONO_negate_is_hidden = 64;
		internal const byte DW_LNE_MONO__extensions_start = 64;
		internal const byte DW_LNE_MONO__extensions_end = 127;
		protected LineNumberEntry[] _line_numbers;
		public readonly int LineBase;
		public readonly int LineRange;
		public readonly byte OpcodeBase;
		public readonly int MaxAddressIncrement;
		public LineNumberEntry[] LineNumbers
		{
			get
			{
				return this._line_numbers;
			}
		}
		protected LineNumberTable(MonoSymbolFile file)
		{
			this.LineBase = file.OffsetTable.LineNumberTable_LineBase;
			this.LineRange = file.OffsetTable.LineNumberTable_LineRange;
			this.OpcodeBase = (byte)file.OffsetTable.LineNumberTable_OpcodeBase;
			this.MaxAddressIncrement = (int)(255 - this.OpcodeBase) / this.LineRange;
		}
		internal LineNumberTable(MonoSymbolFile file, LineNumberEntry[] lines) : this(file)
		{
			this._line_numbers = lines;
		}
        //internal void Write(MonoSymbolFile file, MyBinaryWriter bw)
        //{
        //    int start = (int)bw.BaseStream.Position;
        //    bool last_is_hidden = false;
        //    int last_line = 1;
        //    int last_offset = 0;
        //    int last_file = 1;
        //    int i = 0;
        //    while (i < this.LineNumbers.Length)
        //    {
        //        int line_inc = this.LineNumbers[i].Row - last_line;
        //        int offset_inc = this.LineNumbers[i].Offset - last_offset;
        //        if (i + 1 >= this.LineNumbers.Length)
        //        {
        //            goto IL_84;
        //        }
        //        if (!this.LineNumbers[i + 1].Equals(this.LineNumbers[i]))
        //        {
        //            goto IL_84;
        //        }
        //        IL_207:
        //        i++;
        //        continue;
        //        IL_84:
        //        if (this.LineNumbers[i].File != last_file)
        //        {
        //            bw.Write(4);
        //            bw.WriteLeb128(this.LineNumbers[i].File);
        //            last_file = this.LineNumbers[i].File;
        //        }
        //        if (this.LineNumbers[i].IsHidden != last_is_hidden)
        //        {
        //            bw.Write(0);
        //            bw.Write(1);
        //            bw.Write(64);
        //            last_is_hidden = this.LineNumbers[i].IsHidden;
        //        }
        //        if (offset_inc >= this.MaxAddressIncrement)
        //        {
        //            if (offset_inc < 2 * this.MaxAddressIncrement)
        //            {
        //                bw.Write(8);
        //                offset_inc -= this.MaxAddressIncrement;
        //            }
        //            else
        //            {
        //                bw.Write(2);
        //                bw.WriteLeb128(offset_inc);
        //                offset_inc = 0;
        //            }
        //        }
        //        if (line_inc < this.LineBase || line_inc >= this.LineBase + this.LineRange)
        //        {
        //            bw.Write(3);
        //            bw.WriteLeb128(line_inc);
        //            if (offset_inc != 0)
        //            {
        //                bw.Write(2);
        //                bw.WriteLeb128(offset_inc);
        //            }
        //            bw.Write(1);
        //        }
        //        else
        //        {
        //            byte opcode = (byte)(line_inc - this.LineBase + this.LineRange * offset_inc + (int)this.OpcodeBase);
        //            bw.Write(opcode);
        //        }
        //        last_line = this.LineNumbers[i].Row;
        //        last_offset = this.LineNumbers[i].Offset;
        //        goto IL_207;
        //    }
        //    bw.Write(0);
        //    bw.Write(1);
        //    bw.Write(1);
        //    file.ExtendedLineNumberSize += (int)bw.BaseStream.Position - start;
        //}
		internal static LineNumberTable Read(MonoSymbolFile file, MyBinaryReader br)
		{
			LineNumberTable lnt = new LineNumberTable(file);
			lnt.DoRead(file, br);
			return lnt;
		}
		private void DoRead(MonoSymbolFile file, MyBinaryReader br)
		{
			List<LineNumberEntry> lines = new List<LineNumberEntry>();
			bool is_hidden = false;
			bool modified = false;
			int stm_line = 1;
			int stm_offset = 0;
			int stm_file = 1;
			byte opcode;
			while (true)
			{
				opcode = br.ReadByte();
				if (opcode == 0)
				{
					byte size = br.ReadByte();
					long end_pos = br.BaseStream.Position + (long)((ulong)size);
					opcode = br.ReadByte();
					if (opcode == 1)
					{
						break;
					}
					if (opcode == 64)
					{
						is_hidden = !is_hidden;
						modified = true;
					}
					else
					{
						if (opcode < 64 || opcode > 127)
						{
							goto IL_B8;
						}
					}
					br.BaseStream.Position = end_pos;
				}
				else
				{
					if (opcode < this.OpcodeBase)
					{
						switch (opcode)
						{
						case 1:
							lines.Add(new LineNumberEntry(stm_file, stm_line, stm_offset, is_hidden));
							modified = false;
							continue;
						case 2:
							stm_offset += br.ReadLeb128();
							modified = true;
							continue;
						case 3:
							stm_line += br.ReadLeb128();
							modified = true;
							continue;
						case 4:
							stm_file = br.ReadLeb128();
							modified = true;
							continue;
						case 8:
							stm_offset += this.MaxAddressIncrement;
							modified = true;
							continue;
						}
						goto Block_8;
					}
					opcode -= this.OpcodeBase;
					stm_offset += (int)opcode / this.LineRange;
					stm_line += this.LineBase + (int)opcode % this.LineRange;
					lines.Add(new LineNumberEntry(stm_file, stm_line, stm_offset, is_hidden));
					modified = false;
				}
			}
			if (modified)
			{
				lines.Add(new LineNumberEntry(stm_file, stm_line, stm_offset, is_hidden));
			}
			this._line_numbers = new LineNumberEntry[lines.Count];
			lines.CopyTo(this._line_numbers, 0);
			return;
			IL_B8:
			throw new MonoSymbolFileException("Unknown extended opcode {0:x} in LNT ({1})", new object[]
			{
				opcode,
				file.FileName
			});
			Block_8:
			throw new MonoSymbolFileException("Unknown standard opcode {0:x} in LNT", new object[]
			{
				opcode
			});
		}
		public bool GetMethodBounds(out LineNumberEntry start, out LineNumberEntry end)
		{
			bool result;
			if (this._line_numbers.Length > 1)
			{
				start = this._line_numbers[0];
				end = this._line_numbers[this._line_numbers.Length - 1];
				result = true;
			}
			else
			{
				start = LineNumberEntry.Null;
				end = LineNumberEntry.Null;
				result = false;
			}
			return result;
		}
	}
}
