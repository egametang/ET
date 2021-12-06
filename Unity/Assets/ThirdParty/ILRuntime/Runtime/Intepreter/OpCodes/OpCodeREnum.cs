using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Intepreter.OpCodes
{
    enum OpCodeREnum
    {
        /// <summary>
        /// 如果修补操作码，则填充空间。尽管可能消耗处理周期，但未执行任何有意义的操作。
        /// </summary>
        Nop,
        /// <summary>
        /// 向公共语言结构 (CLI) 发出信号以通知调试器已撞上了一个断点。
        /// </summary>
        Break,
        /// <summary>
        /// 将索引为 0 的参数加载到计算堆栈上。
        /// </summary>
        Ldarg_0,
        /// <summary>
        /// 将索引为 1 的参数加载到计算堆栈上。
        /// </summary>
        Ldarg_1,
        /// <summary>
        /// 将索引为 2 的参数加载到计算堆栈上。
        /// </summary>
        Ldarg_2,
        /// <summary>
        /// 将索引为 3 的参数加载到计算堆栈上。
        /// </summary>
        Ldarg_3,
        /// <summary>
        /// 将索引 0 处的局部变量加载到计算堆栈上。
        /// </summary>
        Ldloc_0,
        /// <summary>
        /// 将索引 1 处的局部变量加载到计算堆栈上。
        /// </summary>
        Ldloc_1,
        /// <summary>
        /// 将索引 2 处的局部变量加载到计算堆栈上。
        /// </summary>
        Ldloc_2,
        /// <summary>
        /// 将索引 3 处的局部变量加载到计算堆栈上。
        /// </summary>
        Ldloc_3,
        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 0 处的局部变量列表中。
        /// </summary>
        Stloc_0,
        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 1 处的局部变量列表中。
        /// </summary>
        Stloc_1,
        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 2 处的局部变量列表中。
        /// </summary>
        Stloc_2,
        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到索引 3 处的局部变量列表中。
        /// </summary>
        Stloc_3,
        /// <summary>
        /// 将参数（由指定的短格式索引引用）加载到计算堆栈上。
        /// </summary>
        Ldarg_S,
        /// <summary>
        /// 以短格式将参数地址加载到计算堆栈上。
        /// </summary>
        Ldarga_S,
        /// <summary>
        /// 将位于计算堆栈顶部的值存储在参数槽中的指定索引处（短格式）。
        /// </summary>
        Starg_S,
        /// <summary>
        /// 将特定索引处的局部变量加载到计算堆栈上（短格式）。
        /// </summary>
        Ldloc_S,
        /// <summary>
        /// 将位于特定索引处的局部变量的地址加载到计算堆栈上（短格式）。
        /// </summary>
        Ldloca_S,
        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储在局部变量列表中的 index 处（短格式）。
        /// </summary>
        Stloc_S,
        /// <summary>
        /// 将空引用（O 类型）推送到计算堆栈上。
        /// </summary>
        Ldnull,
        /// <summary>
        /// 将整数值 -1 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_M1,
        /// <summary>
        /// 将整数值 0 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_0,
        /// <summary>
        /// 将整数值 1 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_1,
        /// <summary>
        /// 将整数值 2 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_2,
        /// <summary>
        /// 将整数值 3 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_3,
        /// <summary>
        /// 将整数值 4 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_4,
        /// <summary>
        /// 将整数值 5 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_5,
        /// <summary>
        /// 将整数值 6 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_6,
        /// <summary>
        /// 将整数值 7 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_7,
        /// <summary>
        /// 将整数值 8 作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4_8,
        /// <summary>
        /// 将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）。
        /// </summary>
        Ldc_I4_S,
        /// <summary>
        /// 将所提供的 int32 类型的值作为 int32 推送到计算堆栈上。
        /// </summary>
        Ldc_I4,
        /// <summary>
        /// 将所提供的 int64 类型的值作为 int64 推送到计算堆栈上。
        /// </summary>
        Ldc_I8,
        /// <summary>
        /// 将所提供的 float32 类型的值作为 F (float) 类型推送到计算堆栈上。
        /// </summary>
        Ldc_R4,
        /// <summary>
        /// 将所提供的 float64 类型的值作为 F (float) 类型推送到计算堆栈上。
        /// </summary>
        Ldc_R8,
        /// <summary>
        /// 复制计算堆栈上当前最顶端的值，然后将副本推送到计算堆栈上。
        /// </summary>
        Dup,
        /// <summary>
        /// 移除当前位于计算堆栈顶部的值。
        /// </summary>
        Pop,
        /// <summary>
        /// 退出当前方法并跳至指定方法。
        /// </summary>
        Jmp,
        /// <summary>
        /// 调用由传递的方法说明符指示的方法。
        /// </summary>
        Call,
        /// <summary>
        /// 通过调用约定描述的参数调用在计算堆栈上指示的方法（作为指向入口点的指针）。
        /// </summary>
        Calli,
        /// <summary>
        /// 从当前方法返回，并将返回值（如果存在）从调用方的计算堆栈推送到被调用方的计算堆栈上。
        /// </summary>
        Ret,
        /// <summary>
        /// 无条件地将控制转移到目标指令（短格式）。
        /// </summary>
        Br_S,
        /// <summary>
        /// 如果 value 为 false、空引用或零，则将控制转移到目标指令。
        /// </summary>
        Brfalse_S,
        /// <summary>
        /// 如果 value 为 true、非空或非零，则将控制转移到目标指令（短格式）。
        /// </summary>
        Brtrue_S,
        /// <summary>
        /// 如果两个值相等，则将控制转移到目标指令（短格式）。
        /// </summary>
        Beq_S,
        /// <summary>
        /// 如果第一个值大于或等于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        Bge_S,
        /// <summary>
        /// 如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        Bgt_S,
        /// <summary>
        /// 如果第一个值小于或等于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        Ble_S,
        /// <summary>
        /// 如果第一个值小于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        Blt_S,
        /// <summary>
        /// 当两个无符号整数值或不可排序的浮点型值不相等时，将控制转移到目标指令（短格式）。
        /// </summary>
        Bne_Un_S,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        Bge_Un_S,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        Bgt_Un_S,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点值时，如果第一个值小于或等于第二个值，则将控制权转移到目标指令（短格式）。
        /// </summary>
        Ble_Un_S,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于第二个值，则将控制转移到目标指令（短格式）。
        /// </summary>
        Blt_Un_S,
        /// <summary>
        /// 无条件地将控制转移到目标指令。
        /// </summary>
        Br,
        /// <summary>
        /// 如果 value 为 false、空引用（Visual Basic 中的 Nothing）或零，则将控制转移到目标指令。
        /// </summary>
        Brfalse,
        /// <summary>
        /// 如果 value 为 true、非空或非零，则将控制转移到目标指令。
        /// </summary>
        Brtrue,
        /// <summary>
        /// 如果两个值相等，则将控制转移到目标指令。
        /// </summary>
        Beq,
        /// <summary>
        /// 如果第一个值大于或等于第二个值，则将控制转移到目标指令。
        /// </summary>
        Bge,
        /// <summary>
        /// 如果第一个值大于第二个值，则将控制转移到目标指令。
        /// </summary>
        Bgt,
        /// <summary>
        /// 如果第一个值小于或等于第二个值，则将控制转移到目标指令。
        /// </summary>
        Ble,
        /// <summary>
        /// 如果第一个值小于第二个值，则将控制转移到目标指令。
        /// </summary>
        Blt,
        /// <summary>
        /// 当两个无符号整数值或不可排序的浮点型值不相等时，将控制转移到目标指令。
        /// </summary>
        Bne_Un,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令。
        /// </summary>
        Bge_Un,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令。
        /// </summary>
        Bgt_Un,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于或等于第二个值，则将控制转移到目标指令。
        /// </summary>
        Ble_Un,
        /// <summary>
        /// 当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于第二个值，则将控制转移到目标指令。
        /// </summary>
        Blt_Un,
        /// <summary>
        /// 实现跳转表。
        /// </summary>
        Switch,
        /// <summary>
        /// 将 int8 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        Ldind_I1,
        /// <summary>
        /// 将 unsigned int8 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        Ldind_U1,
        /// <summary>
        /// 将 int16 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        Ldind_I2,
        /// <summary>
        /// 将 unsigned int16 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        Ldind_U2,
        /// <summary>
        /// 将 int32 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        Ldind_I4,
        /// <summary>
        /// 将 unsigned int32 类型的值作为 int32 间接加载到计算堆栈上。
        /// </summary>
        Ldind_U4,
        /// <summary>
        /// 将 int64 类型的值作为 int64 间接加载到计算堆栈上。
        /// </summary>
        Ldind_I8,
        /// <summary>
        /// 将 native int 类型的值作为 native int 间接加载到计算堆栈上。
        /// </summary>
        Ldind_I,
        /// <summary>
        /// 将 float32 类型的值作为 F (float) 类型间接加载到计算堆栈上。
        /// </summary>
        Ldind_R4,
        /// <summary>
        /// 将 float64 类型的值作为 F (float) 类型间接加载到计算堆栈上。
        /// </summary>
        Ldind_R8,
        /// <summary>
        /// 将对象引用作为 O（对象引用）类型间接加载到计算堆栈上。
        /// </summary>
        Ldind_Ref,
        /// <summary>
        /// 存储所提供地址处的对象引用值。
        /// </summary>
        Stind_Ref,
        /// <summary>
        /// 在所提供的地址存储 int8 类型的值。
        /// </summary>
        Stind_I1,
        /// <summary>
        /// 在所提供的地址存储 int16 类型的值。
        /// </summary>
        Stind_I2,
        /// <summary>
        /// 在所提供的地址存储 int32 类型的值。
        /// </summary>
        Stind_I4,
        /// <summary>
        /// 在所提供的地址存储 int64 类型的值。
        /// </summary>
        Stind_I8,
        /// <summary>
        /// 在所提供的地址存储 float32 类型的值。
        /// </summary>
        Stind_R4,
        /// <summary>
        /// 在所提供的地址存储 float64 类型的值。
        /// </summary>
        Stind_R8,
        /// <summary>
        /// 将两个值相加并将结果推送到计算堆栈上。
        /// </summary>
        Add,
        /// <summary>
        /// 从其他值中减去一个值并将结果推送到计算堆栈上。
        /// </summary>
        Sub,
        /// <summary>
        /// 将两个值相乘并将结果推送到计算堆栈上。
        /// </summary>
        Mul,
        /// <summary>
        /// 将两个值相除并将结果作为浮点（F 类型）或商（int32 类型）推送到计算堆栈上。
        /// </summary>
        Div,
        /// <summary>
        /// 两个无符号整数值相除并将结果 ( int32 ) 推送到计算堆栈上。
        /// </summary>
        Div_Un,
        /// <summary>
        /// 将两个值相除并将余数推送到计算堆栈上。
        /// </summary>
        Rem,
        /// <summary>
        /// 将两个无符号值相除并将余数推送到计算堆栈上。
        /// </summary>
        Rem_Un,
        /// <summary>
        /// 计算两个值的按位“与”并将结果推送到计算堆栈上。
        /// </summary>
        And,
        /// <summary>
        /// 计算位于堆栈顶部的两个整数值的按位求补并将结果推送到计算堆栈上。
        /// </summary>
        Or,
        /// <summary>
        /// 计算位于计算堆栈顶部的两个值的按位异或，并且将结果推送到计算堆栈上。
        /// </summary>
        Xor,
        /// <summary>
        /// 将整数值左移（用零填充）指定的位数，并将结果推送到计算堆栈上。
        /// </summary>
        Shl,
        /// <summary>
        /// 将整数值右移（保留符号）指定的位数，并将结果推送到计算堆栈上。
        /// </summary>
        Shr,
        /// <summary>
        /// 将无符号整数值右移（用零填充）指定的位数，并将结果推送到计算堆栈上。
        /// </summary>
        Shr_Un,
        /// <summary>
        /// 对一个值执行求反并将结果推送到计算堆栈上。
        /// </summary>
        Neg,
        /// <summary>
        /// 计算堆栈顶部整数值的按位求补并将结果作为相同的类型推送到计算堆栈上。
        /// </summary>
        Not,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int8，然后将其扩展（填充）为 int32。
        /// </summary>
        Conv_I1,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int16，然后将其扩展（填充）为 int32。
        /// </summary>
        Conv_I2,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int32。
        /// </summary>
        Conv_I4,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 int64。
        /// </summary>
        Conv_I8,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 float32。
        /// </summary>
        Conv_R4,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 float64。
        /// </summary>
        Conv_R8,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int32，然后将其扩展为 int32。
        /// </summary>
        Conv_U4,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int64，然后将其扩展为 int64。
        /// </summary>
        Conv_U8,
        /// <summary>
        /// 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
        /// </summary>
        Callvirt,
        /// <summary>
        /// 将位于对象（&、* 或 native int 类型）地址的值类型复制到目标对象（&、* 或 native int 类型）的地址。
        /// </summary>
        Cpobj,
        /// <summary>
        /// 将地址指向的值类型对象复制到计算堆栈的顶部。
        /// </summary>
        Ldobj,
        /// <summary>
        /// 推送对元数据中存储的字符串的新对象引用。
        /// </summary>
        Ldstr,
        /// <summary>
        /// 创建一个值类型的新对象或新实例，并将对象引用（O 类型）推送到计算堆栈上。
        /// </summary>
        Newobj,
        /// <summary>
        /// 尝试将引用传递的对象转换为指定的类。
        /// </summary>
        Castclass,
        /// <summary>
        /// 测试对象引用（O 类型）是否为特定类的实例。
        /// </summary>
        Isinst,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号整数值转换为 float32。
        /// </summary>
        Conv_R_Un,
        /// <summary>
        /// 将值类型的已装箱的表示形式转换为其未装箱的形式。
        /// </summary>
        Unbox,
        /// <summary>
        /// 引发当前位于计算堆栈上的异常对象。
        /// </summary>
        Throw,
        /// <summary>
        /// 查找对象中其引用当前位于计算堆栈的字段的值。
        /// </summary>
        Ldfld,
        /// <summary>
        /// 查找对象中其引用当前位于计算堆栈的字段的地址。
        /// </summary>
        Ldflda,
        /// <summary>
        /// 用新值替换在对象引用或指针的字段中存储的值。
        /// </summary>
        Stfld,
        /// <summary>
        /// 将静态字段的值推送到计算堆栈上。
        /// </summary>
        Ldsfld,
        /// <summary>
        /// 将静态字段的地址推送到计算堆栈上。
        /// </summary>
        Ldsflda,
        /// <summary>
        /// 用来自计算堆栈的值替换静态字段的值。
        /// </summary>
        Stsfld,
        /// <summary>
        /// 将指定类型的值从计算堆栈复制到所提供的内存地址中。
        /// </summary>
        Stobj,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int8 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I1_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int16 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I2_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I4_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 int64，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I8_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int8 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U1_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int16 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U2_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U4_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned int64，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U8_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为有符号 native int，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I_Un,
        /// <summary>
        /// 将位于计算堆栈顶部的无符号值转换为 unsigned native int，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U_Un,
        /// <summary>
        /// 将值类转换为对象引用（O 类型）。
        /// </summary>
        Box,
        /// <summary>
        /// 将对新的从零开始的一维数组（其元素属于特定类型）的对象引用推送到计算堆栈上。
        /// </summary>
        Newarr,
        /// <summary>
        /// 将从零开始的、一维数组的元素的数目推送到计算堆栈上。
        /// </summary>
        Ldlen,
        /// <summary>
        /// 将位于指定数组索引的数组元素的地址作为 & 类型（托管指针）加载到计算堆栈的顶部。
        /// </summary>
        Ldelema,
        /// <summary>
        /// 将位于指定数组索引处的 int8 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_I1,
        /// <summary>
        /// 将位于指定数组索引处的 unsigned int8 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_U1,
        /// <summary>
        /// 将位于指定数组索引处的 int16 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_I2,
        /// <summary>
        /// 将位于指定数组索引处的 unsigned int16 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_U2,
        /// <summary>
        /// 将位于指定数组索引处的 int32 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_I4,
        /// <summary>
        /// 将位于指定数组索引处的 unsigned int32 类型的元素作为 int32 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_U4,
        /// <summary>
        /// 将位于指定数组索引处的 int64 类型的元素作为 int64 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_I8,
        /// <summary>
        /// 将位于指定数组索引处的 native int 类型的元素作为 native int 加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_I,
        /// <summary>
        /// 将位于指定数组索引处的 float32 类型的元素作为 F 类型（浮点型）加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_R4,
        /// <summary>
        /// 将位于指定数组索引处的 float64 类型的元素作为 F 类型（浮点型）加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_R8,
        /// <summary>
        /// 将位于指定数组索引处的包含对象引用的元素作为 O 类型（对象引用）加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_Ref,
        /// <summary>
        /// 用计算堆栈上的 native int 值替换给定索引处的数组元素。
        /// </summary>
        Stelem_I,
        /// <summary>
        /// 用计算堆栈上的 int8 值替换给定索引处的数组元素。
        /// </summary>
        Stelem_I1,
        /// <summary>
        /// 用计算堆栈上的 int16 值替换给定索引处的数组元素。
        /// </summary>
        Stelem_I2,
        /// <summary>
        /// 用计算堆栈上的 int32 值替换给定索引处的数组元素。
        /// </summary>
        Stelem_I4,
        /// <summary>
        /// 用计算堆栈上的 int64 值替换给定索引处的数组元素。
        /// </summary>
        Stelem_I8,
        /// <summary>
        /// 用计算堆栈上的 float32 值替换给定索引处的数组元素。
        /// </summary>
        Stelem_R4,
        /// <summary>
        /// 用计算堆栈上的 float64 值替换给定索引处的数组元素。
        /// </summary>
        Stelem_R8,
        /// <summary>
        /// 用计算堆栈上的对象 ref 值（O 类型）替换给定索引处的数组元素。
        /// </summary>
        Stelem_Ref,
        /// <summary>
        /// 按照指令中指定的类型，将指定数组索引中的元素加载到计算堆栈的顶部。
        /// </summary>
        Ldelem_Any,
        /// <summary>
        /// 用计算堆栈中的值替换给定索引处的数组元素，其类型在指令中指定。
        /// </summary>
        Stelem_Any,
        /// <summary>
        /// 将指令中指定类型的已装箱的表示形式转换成未装箱形式。
        /// </summary>
        Unbox_Any,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int8 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I1,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int8 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U1,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int16 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I2,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int16 并将其扩展为 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U2,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I4,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int32，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U4,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 int64，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I8,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned int64，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U8,
        /// <summary>
        /// 检索嵌入在类型化引用内的地址（& 类型）。
        /// </summary>
        Refanyval,
        /// <summary>
        /// 如果值不是有限数，则引发 ArithmeticException。
        /// </summary>
        Ckfinite,
        /// <summary>
        /// 将对特定类型实例的类型化引用推送到计算堆栈上。
        /// </summary>
        Mkrefany,
        /// <summary>
        /// 将元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上。
        /// </summary>
        Ldtoken,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int16，然后将其扩展为 int32。
        /// </summary>
        Conv_U2,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned int8，然后将其扩展为 int32。
        /// </summary>
        Conv_U1,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 native int。
        /// </summary>
        Conv_I,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为有符号 native int，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_I,
        /// <summary>
        /// 将位于计算堆栈顶部的有符号值转换为 unsigned native int，并在溢出时引发 OverflowException。
        /// </summary>
        Conv_Ovf_U,
        /// <summary>
        /// 将两个整数相加，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        Add_Ovf,
        /// <summary>
        /// 将两个无符号整数值相加，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        Add_Ovf_Un,
        /// <summary>
        /// 将两个整数值相乘，执行溢出检查，并将结果推送到计算堆栈上。
        /// </summary>
        Mul_Ovf,
        /// <summary>
        /// 将两个无符号整数值相乘，执行溢出检查，并将结果推送到计算堆栈上。
        /// </summary>
        Mul_Ovf_Un,
        /// <summary>
        /// 从另一值中减去一个整数值，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        Sub_Ovf,
        /// <summary>
        /// 从另一值中减去一个无符号整数值，执行溢出检查，并且将结果推送到计算堆栈上。
        /// </summary>
        Sub_Ovf_Un,
        /// <summary>
        /// 将控制从异常块的 fault 或 finally 子句转移回公共语言结构 (CLI) 异常处理程序。
        /// </summary>
        Endfinally,
        /// <summary>
        /// 退出受保护的代码区域，无条件将控制转移到特定目标指令。
        /// </summary>
        Leave,
        /// <summary>
        /// 退出受保护的代码区域，无条件将控制转移到目标指令（缩写形式）。
        /// </summary>
        Leave_S,
        /// <summary>
        /// 在所提供的地址存储 native int 类型的值。
        /// </summary>
        Stind_I,
        /// <summary>
        /// 将位于计算堆栈顶部的值转换为 unsigned native int，然后将其扩展为 native int。
        /// </summary>
        Conv_U,
        /// <summary>
        /// 返回指向当前方法的参数列表的非托管指针。
        /// </summary>
        Arglist,
        /// <summary>
        /// 比较两个值。如果这两个值相等，则将整数值 1 (int32) 推送到计算堆栈上；否则，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        Ceq,
        /// <summary>
        /// 比较两个值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        Cgt,
        /// <summary>
        /// 比较两个无符号的或不可排序的值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        Cgt_Un,
        /// <summary>
        /// 比较两个值。如果第一个值小于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        /// </summary>
        Clt,
        /// <summary>
        /// 比较无符号的或不可排序的值 value1 和 value2。如果 value1 小于 value2，则将整数值 1 (int32 ) 推送到计算堆栈上；反之，将 0 ( int32 ) 推送到计算堆栈上。
        /// </summary>
        Clt_Un,
        /// <summary>
        /// 将指向实现特定方法的本机代码的非托管指针（native int 类型）推送到计算堆栈上。
        /// </summary>
        Ldftn,
        /// <summary>
        /// 将指向实现与指定对象关联的特定虚方法的本机代码的非托管指针（native int 类型）推送到计算堆栈上。
        /// </summary>
        Ldvirtftn,
        /// <summary>
        /// 将参数（由指定索引值引用）加载到堆栈上。
        /// </summary>
        Ldarg,
        /// <summary>
        /// 将参数地址加载到计算堆栈上。
        /// </summary>
        Ldarga,
        /// <summary>
        /// 将位于计算堆栈顶部的值存储到位于指定索引的参数槽中。
        /// </summary>
        Starg,
        /// <summary>
        /// 将指定索引处的局部变量加载到计算堆栈上。
        /// </summary>
        Ldloc,
        /// <summary>
        /// 将位于特定索引处的局部变量的地址加载到计算堆栈上。
        /// </summary>
        Ldloca,
        /// <summary>
        /// 从计算堆栈的顶部弹出当前值并将其存储到指定索引处的局部变量列表中。
        /// </summary>
        Stloc,
        /// <summary>
        /// 从本地动态内存池分配特定数目的字节并将第一个分配的字节的地址（瞬态指针，* 类型）推送到计算堆栈上。
        /// </summary>
        Localloc,
        /// <summary>
        /// 将控制从异常的 filter 子句转移回公共语言结构 (CLI) 异常处理程序。
        /// </summary>
        Endfilter,
        /// <summary>
        /// 指示当前位于计算堆栈上的地址可能没有与紧接的 ldind、stind、ldfld、stfld、ldobj、stobj、initblk 或 cpblk 指令的自然大小对齐。
        /// </summary>
        Unaligned,
        /// <summary>
        /// 指定当前位于计算堆栈顶部的地址可以是易失的，并且读取该位置的结果不能被缓存，或者对该地址的多个存储区不能被取消。
        /// </summary>
        Volatile,
        /// <summary>
        /// 执行后缀的方法调用指令，以便在执行实际调用指令前移除当前方法的堆栈帧。
        /// </summary>
        Tail,
        /// <summary>
        /// 将位于指定地址的值类型的每个字段初始化为空引用或适当的基元类型的 0。
        /// </summary>
        Initobj,
        /// <summary>
        /// 约束要对其进行虚方法调用的类型。
        /// </summary>
        Constrained,
        /// <summary>
        /// 将指定数目的字节从源地址复制到目标地址。
        /// </summary>
        Cpblk,
        /// <summary>
        /// 将位于特定地址的内存的指定块初始化为给定大小和初始值。
        /// </summary>
        Initblk,
        No,
        /// <summary>
        /// 再次引发当前异常。
        /// </summary>
        Rethrow,
        /// <summary>
        /// 将提供的值类型的大小（以字节为单位）推送到计算堆栈上。
        /// </summary>
        Sizeof,
        /// <summary>
        /// 检索嵌入在类型化引用内的类型标记。
        /// </summary>
        Refanytype,
        /// <summary>
        /// 指定后面的数组地址操作在运行时不执行类型检查，并且返回可变性受限的托管指针。
        /// </summary>
        Readonly,
        /// <summary>
        /// 复制寄存器的值
        /// </summary>
        Move,
        /// <summary>
        /// 将指定寄存器的值压入栈
        /// </summary>
        Push,
        /// <summary>
        /// 内联开始
        /// </summary>
        InlineStart,
        /// <summary>
        /// 内联结束
        /// </summary>
        InlineEnd,
        Beqi,
        Bgei,
        Bgti,
        Blei,
        Blti,
        Bnei_Un,
        Bgei_Un,
        Bgti_Un,
        Blei_Un,
        Blti_Un,
        Ceqi,
        Cgti,
        Cgti_Un,
        Clti,
        Clti_Un,
        Addi,
        Subi,
        Muli,
        Divi,
        Divi_Un,
        Remi,
        Remi_Un,
        Andi,
        Ori,
        Xori,
        Shli,
        Shri,
        Shri_Un,
    }
}
