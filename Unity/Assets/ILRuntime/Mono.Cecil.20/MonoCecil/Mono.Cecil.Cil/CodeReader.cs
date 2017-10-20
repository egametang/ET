//
// CodeReader.cs
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

using Mono.Cecil.PE;
using Mono.Collections.Generic;

using RVA = System.UInt32;

namespace Mono.Cecil.Cil
{

    sealed class CodeReader : ByteBuffer
    {

        readonly internal MetadataReader reader;

        int start;
        Section code_section;

        MethodDefinition method;
        MethodBody body;

        int Offset
        {
            get { return base.position - start; }
        }

        public CodeReader(Section section, MetadataReader reader)
            : base(section.Data)
        {
            this.code_section = section;
            this.reader = reader;
        }

        public MethodBody ReadMethodBody(MethodDefinition method)
        {
            this.method = method;
            this.body = new MethodBody(method);

            reader.context = method;

            ReadMethodBody();

            return this.body;
        }

        public void MoveTo(int rva)
        {
            if (!IsInSection(rva))
            {
                code_section = reader.image.GetSectionAtVirtualAddress((uint)rva);
                Reset(code_section.Data);
            }

            base.position = rva - (int)code_section.VirtualAddress;
        }

        bool IsInSection(int rva)
        {
            return code_section.VirtualAddress <= rva && rva < code_section.VirtualAddress + code_section.SizeOfRawData;
        }

        void ReadMethodBody()
        {
            MoveTo(method.RVA);

            var flags = ReadByte();
            switch (flags & 0x3)
            {
                case 0x2: // tiny
                    body.code_size = flags >> 2;
                    body.MaxStackSize = 8;
                    ReadCode();
                    break;
                case 0x3: // fat
                    base.position--;
                    ReadFatMethod();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var symbol_reader = reader.module.symbol_reader;

            if (symbol_reader != null)
            {
                var instructions = body.Instructions;
                symbol_reader.Read(body, offset => GetInstruction(instructions, offset));
            }
        }

        void ReadFatMethod()
        {
            var flags = ReadUInt16();
            body.max_stack_size = ReadUInt16();
            body.code_size = (int)ReadUInt32();
            body.local_var_token = new MetadataToken(ReadUInt32());
            body.init_locals = (flags & 0x10) != 0;

            if (body.local_var_token.RID != 0)
                body.variables = ReadVariables(body.local_var_token);

            ReadCode();

            if ((flags & 0x8) != 0)
                ReadSection();
        }

        public VariableDefinitionCollection ReadVariables(MetadataToken local_var_token)
        {
            var position = reader.position;
            var variables = reader.ReadVariables(local_var_token);
            reader.position = position;

            return variables;
        }

        void ReadCode()
        {
            start = position;
            var code_size = body.code_size;

            if (code_size < 0 || buffer.Length <= (uint)(code_size + position))
                code_size = 0;

            var end = start + code_size;
            var instructions = body.instructions = new InstructionCollection((code_size + 1) / 2);

            while (position < end)
            {
                var offset = base.position - start;
                var opcode = ReadOpCode();
                var current = new Instruction(offset, opcode);

                if (opcode.OperandType != OperandType.InlineNone)
                    current.operand = ReadOperand(current);

                instructions.Add(current);
            }

            ResolveBranches(instructions);
        }

        OpCode ReadOpCode()
        {
            var il_opcode = ReadByte();
            return il_opcode != 0xfe
                ? OpCodes.OneByteOpCode[il_opcode]
                : OpCodes.TwoBytesOpCode[ReadByte()];
        }

        object ReadOperand(Instruction instruction)
        {
            switch (instruction.opcode.OperandType)
            {
                case OperandType.InlineSwitch:
                    var length = ReadInt32();
                    var base_offset = Offset + (4 * length);
                    var branches = new int[length];
                    for (int i = 0; i < length; i++)
                        branches[i] = base_offset + ReadInt32();
                    return branches;
                case OperandType.ShortInlineBrTarget:
                    return ReadSByte() + Offset;
                case OperandType.InlineBrTarget:
                    return ReadInt32() + Offset;
                case OperandType.ShortInlineI:
                    if (instruction.opcode == OpCodes.Ldc_I4_S)
                        return ReadSByte();

                    return ReadByte();
                case OperandType.InlineI:
                    return ReadInt32();
                case OperandType.ShortInlineR:
                    return ReadSingle();
                case OperandType.InlineR:
                    return ReadDouble();
                case OperandType.InlineI8:
                    return ReadInt64();
                case OperandType.ShortInlineVar:
                    return GetVariable(ReadByte());
                case OperandType.InlineVar:
                    return GetVariable(ReadUInt16());
                case OperandType.ShortInlineArg:
                    return GetParameter(ReadByte());
                case OperandType.InlineArg:
                    return GetParameter(ReadUInt16());
                case OperandType.InlineSig:
                    return GetCallSite(ReadToken());
                case OperandType.InlineString:
                    return GetString(ReadToken());
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                    return reader.LookupToken(ReadToken());
                default:
                    throw new NotSupportedException();
            }
        }

        public string GetString(MetadataToken token)
        {
            return reader.image.UserStringHeap.Read(token.RID);
        }

        public ParameterDefinition GetParameter(int index)
        {
            return Mixin.GetParameter(body, index);
        }

        public VariableDefinition GetVariable(int index)
        {
            return Mixin.GetVariable(body, index);
        }

        public CallSite GetCallSite(MetadataToken token)
        {
            return reader.ReadCallSite(token);
        }

        void ResolveBranches(Collection<Instruction> instructions)
        {
            var items = instructions.items;
            var size = instructions.size;

            for (int i = 0; i < size; i++)
            {
                var instruction = items[i];
                switch (instruction.opcode.OperandType)
                {
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.InlineBrTarget:
                        instruction.operand = GetInstruction((int)instruction.operand);
                        break;
                    case OperandType.InlineSwitch:
                        var offsets = (int[])instruction.operand;
                        var branches = new Instruction[offsets.Length];
                        for (int j = 0; j < offsets.Length; j++)
                            branches[j] = GetInstruction(offsets[j]);

                        instruction.operand = branches;
                        break;
                }
            }
        }

        Instruction GetInstruction(int offset)
        {
            return GetInstruction(body.Instructions, offset);
        }

        static Instruction GetInstruction(Collection<Instruction> instructions, int offset)
        {
            var size = instructions.size;
            var items = instructions.items;
            if (offset < 0 || offset > items[size - 1].offset)
                return null;

            int min = 0;
            int max = size - 1;
            while (min <= max)
            {
                int mid = min + ((max - min) / 2);
                var instruction = items[mid];
                var instruction_offset = instruction.offset;

                if (offset == instruction_offset)
                    return instruction;

                if (offset < instruction_offset)
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            return null;
        }

        void ReadSection()
        {
            Align(4);

            const byte fat_format = 0x40;
            const byte more_sects = 0x80;

            var flags = ReadByte();
            if ((flags & fat_format) == 0)
                ReadSmallSection();
            else
                ReadFatSection();

            if ((flags & more_sects) != 0)
                ReadSection();
        }

        void ReadSmallSection()
        {
            var count = ReadByte() / 12;
            Advance(2);

            ReadExceptionHandlers(
                count,
                () => (int)ReadUInt16(),
                () => (int)ReadByte());
        }

        void ReadFatSection()
        {
            position--;
            var count = (ReadInt32() >> 8) / 24;

            ReadExceptionHandlers(
                count,
                ReadInt32,
                ReadInt32);
        }

        // inline ?
        void ReadExceptionHandlers(int count, Func<int> read_entry, Func<int> read_length)
        {
            for (int i = 0; i < count; i++)
            {
                var handler = new ExceptionHandler(
                    (ExceptionHandlerType)(read_entry() & 0x7));

                handler.TryStart = GetInstruction(read_entry());
                handler.TryEnd = GetInstruction(handler.TryStart.Offset + read_length());

                handler.HandlerStart = GetInstruction(read_entry());
                handler.HandlerEnd = GetInstruction(handler.HandlerStart.Offset + read_length());

                ReadExceptionHandlerSpecific(handler);

                this.body.ExceptionHandlers.Add(handler);
            }
        }

        void ReadExceptionHandlerSpecific(ExceptionHandler handler)
        {
            switch (handler.HandlerType)
            {
                case ExceptionHandlerType.Catch:
                    handler.CatchType = (TypeReference)reader.LookupToken(ReadToken());
                    break;
                case ExceptionHandlerType.Filter:
                    handler.FilterStart = GetInstruction(ReadInt32());
                    break;
                default:
                    Advance(4);
                    break;
            }
        }

        void Align(int align)
        {
            align--;
            Advance(((position + align) & ~align) - position);
        }

        public MetadataToken ReadToken()
        {
            return new MetadataToken(ReadUInt32());
        }

    }
}
