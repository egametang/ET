using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Intepreter.OpCodes
{
  
    /// <summary>
    /// IL指令
    /// </summary>
    struct OpCode
    {
        /// <summary>
        /// 当前指令
        /// </summary>
        public OpCodeEnum Code;

        /// <summary>
        ///  Int32 操作数
        /// </summary>
        public int TokenInteger;

        /// <summary>
        /// Int64 操作数
        /// </summary>
        public long TokenLong;
    }
}
