using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_Console_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Console);
            args = new Type[]{};
            method = type.GetMethod("get_Error", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Error_0);
            args = new Type[]{};
            method = type.GetMethod("get_Out", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Out_1);
            args = new Type[]{};
            method = type.GetMethod("get_In", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_In_2);
            args = new Type[]{};
            method = type.GetMethod("OpenStandardError", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardError_3);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("OpenStandardError", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardError_4);
            args = new Type[]{};
            method = type.GetMethod("OpenStandardInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardInput_5);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("OpenStandardInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardInput_6);
            args = new Type[]{};
            method = type.GetMethod("OpenStandardOutput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardOutput_7);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("OpenStandardOutput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OpenStandardOutput_8);
            args = new Type[]{typeof(System.IO.TextWriter)};
            method = type.GetMethod("SetError", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetError_9);
            args = new Type[]{typeof(System.IO.TextReader)};
            method = type.GetMethod("SetIn", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetIn_10);
            args = new Type[]{typeof(System.IO.TextWriter)};
            method = type.GetMethod("SetOut", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetOut_11);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_12);
            args = new Type[]{typeof(System.Char)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_13);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_14);
            args = new Type[]{typeof(System.Decimal)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_15);
            args = new Type[]{typeof(System.Double)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_16);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_17);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_18);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_19);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_20);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_21);
            args = new Type[]{typeof(System.UInt32)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_22);
            args = new Type[]{typeof(System.UInt64)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_23);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_24);
            args = new Type[]{typeof(System.String), typeof(System.Object[])};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_25);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_26);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_27);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_28);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Write", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Write_29);
            args = new Type[]{};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_30);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_31);
            args = new Type[]{typeof(System.Char)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_32);
            args = new Type[]{typeof(System.Char[])};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_33);
            args = new Type[]{typeof(System.Decimal)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_34);
            args = new Type[]{typeof(System.Double)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_35);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_36);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_37);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_38);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_39);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_40);
            args = new Type[]{typeof(System.UInt32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_41);
            args = new Type[]{typeof(System.UInt64)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_42);
            args = new Type[]{typeof(System.String), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_43);
            args = new Type[]{typeof(System.String), typeof(System.Object[])};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_44);
            args = new Type[]{typeof(System.Char[]), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_45);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_46);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_47);
            args = new Type[]{typeof(System.String), typeof(System.Object), typeof(System.Object), typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("WriteLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, WriteLine_48);
            args = new Type[]{};
            method = type.GetMethod("Read", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Read_49);
            args = new Type[]{};
            method = type.GetMethod("ReadLine", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReadLine_50);
            args = new Type[]{};
            method = type.GetMethod("get_InputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_InputEncoding_51);
            args = new Type[]{typeof(System.Text.Encoding)};
            method = type.GetMethod("set_InputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_InputEncoding_52);
            args = new Type[]{};
            method = type.GetMethod("get_OutputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_OutputEncoding_53);
            args = new Type[]{typeof(System.Text.Encoding)};
            method = type.GetMethod("set_OutputEncoding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_OutputEncoding_54);
            args = new Type[]{};
            method = type.GetMethod("get_BackgroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_BackgroundColor_55);
            args = new Type[]{typeof(System.ConsoleColor)};
            method = type.GetMethod("set_BackgroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_BackgroundColor_56);
            args = new Type[]{};
            method = type.GetMethod("get_BufferHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_BufferHeight_57);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_BufferHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_BufferHeight_58);
            args = new Type[]{};
            method = type.GetMethod("get_BufferWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_BufferWidth_59);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_BufferWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_BufferWidth_60);
            args = new Type[]{};
            method = type.GetMethod("get_CapsLock", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CapsLock_61);
            args = new Type[]{};
            method = type.GetMethod("get_CursorLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorLeft_62);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_CursorLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorLeft_63);
            args = new Type[]{};
            method = type.GetMethod("get_CursorTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorTop_64);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_CursorTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorTop_65);
            args = new Type[]{};
            method = type.GetMethod("get_CursorSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorSize_66);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_CursorSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorSize_67);
            args = new Type[]{};
            method = type.GetMethod("get_CursorVisible", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CursorVisible_68);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("set_CursorVisible", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_CursorVisible_69);
            args = new Type[]{};
            method = type.GetMethod("get_ForegroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_ForegroundColor_70);
            args = new Type[]{typeof(System.ConsoleColor)};
            method = type.GetMethod("set_ForegroundColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_ForegroundColor_71);
            args = new Type[]{};
            method = type.GetMethod("get_KeyAvailable", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_KeyAvailable_72);
            args = new Type[]{};
            method = type.GetMethod("get_LargestWindowHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_LargestWindowHeight_73);
            args = new Type[]{};
            method = type.GetMethod("get_LargestWindowWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_LargestWindowWidth_74);
            args = new Type[]{};
            method = type.GetMethod("get_NumberLock", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_NumberLock_75);
            args = new Type[]{};
            method = type.GetMethod("get_Title", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Title_76);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("set_Title", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Title_77);
            args = new Type[]{};
            method = type.GetMethod("get_TreatControlCAsInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_TreatControlCAsInput_78);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("set_TreatControlCAsInput", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_TreatControlCAsInput_79);
            args = new Type[]{};
            method = type.GetMethod("get_WindowHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowHeight_80);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowHeight", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowHeight_81);
            args = new Type[]{};
            method = type.GetMethod("get_WindowLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowLeft_82);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowLeft", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowLeft_83);
            args = new Type[]{};
            method = type.GetMethod("get_WindowTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowTop_84);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowTop", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowTop_85);
            args = new Type[]{};
            method = type.GetMethod("get_WindowWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_WindowWidth_86);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_WindowWidth", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_WindowWidth_87);
            args = new Type[]{};
            method = type.GetMethod("Beep", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Beep_88);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Beep", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Beep_89);
            args = new Type[]{};
            method = type.GetMethod("Clear", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clear_90);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("MoveBufferArea", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, MoveBufferArea_91);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32), typeof(System.Char), typeof(System.ConsoleColor), typeof(System.ConsoleColor)};
            method = type.GetMethod("MoveBufferArea", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, MoveBufferArea_92);
            args = new Type[]{};
            method = type.GetMethod("ReadKey", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReadKey_93);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("ReadKey", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReadKey_94);
            args = new Type[]{};
            method = type.GetMethod("ResetColor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ResetColor_95);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetBufferSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetBufferSize_96);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetCursorPosition", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetCursorPosition_97);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetWindowPosition", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetWindowPosition_98);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetWindowSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetWindowSize_99);


        }


        static StackObject* get_Error_0(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.Error;

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_Out_1(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.Out;

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_In_2(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.In;

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* OpenStandardError_3(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.OpenStandardError();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* OpenStandardError_4(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 bufferSize = ptr_of_this_method->Value;

            var result_of_this_method = System.Console.OpenStandardError(bufferSize);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* OpenStandardInput_5(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.OpenStandardInput();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* OpenStandardInput_6(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 bufferSize = ptr_of_this_method->Value;

            var result_of_this_method = System.Console.OpenStandardInput(bufferSize);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* OpenStandardOutput_7(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.OpenStandardOutput();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* OpenStandardOutput_8(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 bufferSize = ptr_of_this_method->Value;

            var result_of_this_method = System.Console.OpenStandardOutput(bufferSize);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* SetError_9(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.IO.TextWriter newError = (System.IO.TextWriter)typeof(System.IO.TextWriter).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.SetError(newError);

            return __ret;
        }

        static StackObject* SetIn_10(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.IO.TextReader newIn = (System.IO.TextReader)typeof(System.IO.TextReader).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.SetIn(newIn);

            return __ret;
        }

        static StackObject* SetOut_11(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.IO.TextWriter newOut = (System.IO.TextWriter)typeof(System.IO.TextWriter).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.SetOut(newOut);

            return __ret;
        }

        static StackObject* Write_12(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean value = ptr_of_this_method->Value == 1;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_13(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Char value = (char)ptr_of_this_method->Value;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_14(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Char[] buffer = (System.Char[])typeof(System.Char[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(buffer);

            return __ret;
        }

        static StackObject* Write_15(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Decimal value = (System.Decimal)typeof(System.Decimal).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_16(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Double value = *(double*)&ptr_of_this_method->Value;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_17(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_18(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 value = *(long*)&ptr_of_this_method->Value;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_19(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_20(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single value = *(float*)&ptr_of_this_method->Value;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_21(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_22(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.UInt32 value = (uint)ptr_of_this_method->Value;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_23(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.UInt64 value = *(ulong*)&ptr_of_this_method->Value;

            System.Console.Write(value);

            return __ret;
        }

        static StackObject* Write_24(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(format, arg0);

            return __ret;
        }

        static StackObject* Write_25(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object[] arg = (System.Object[])typeof(System.Object[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(format, arg);

            return __ret;
        }

        static StackObject* Write_26(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 count = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Char[] buffer = (System.Char[])typeof(System.Char[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(buffer, index, count);

            return __ret;
        }

        static StackObject* Write_27(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(format, arg0, arg1);

            return __ret;
        }

        static StackObject* Write_28(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg2 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(format, arg0, arg1, arg2);

            return __ret;
        }

        static StackObject* Write_29(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg3 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object arg2 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Write(format, arg0, arg1, arg2, arg3);

            return __ret;
        }

        static StackObject* WriteLine_30(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            System.Console.WriteLine();

            return __ret;
        }

        static StackObject* WriteLine_31(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean value = ptr_of_this_method->Value == 1;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_32(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Char value = (char)ptr_of_this_method->Value;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_33(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Char[] buffer = (System.Char[])typeof(System.Char[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(buffer);

            return __ret;
        }

        static StackObject* WriteLine_34(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Decimal value = (System.Decimal)typeof(System.Decimal).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_35(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Double value = *(double*)&ptr_of_this_method->Value;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_36(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_37(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 value = *(long*)&ptr_of_this_method->Value;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_38(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_39(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single value = *(float*)&ptr_of_this_method->Value;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_40(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_41(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.UInt32 value = (uint)ptr_of_this_method->Value;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_42(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.UInt64 value = *(ulong*)&ptr_of_this_method->Value;

            System.Console.WriteLine(value);

            return __ret;
        }

        static StackObject* WriteLine_43(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(format, arg0);

            return __ret;
        }

        static StackObject* WriteLine_44(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object[] arg = (System.Object[])typeof(System.Object[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(format, arg);

            return __ret;
        }

        static StackObject* WriteLine_45(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 count = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Char[] buffer = (System.Char[])typeof(System.Char[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(buffer, index, count);

            return __ret;
        }

        static StackObject* WriteLine_46(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(format, arg0, arg1);

            return __ret;
        }

        static StackObject* WriteLine_47(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg2 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(format, arg0, arg1, arg2);

            return __ret;
        }

        static StackObject* WriteLine_48(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object arg3 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object arg2 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object arg1 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Object arg0 = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.String format = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.WriteLine(format, arg0, arg1, arg2, arg3);

            return __ret;
        }

        static StackObject* Read_49(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.Read();

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* ReadLine_50(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.ReadLine();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_InputEncoding_51(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.InputEncoding;

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_InputEncoding_52(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Text.Encoding value = (System.Text.Encoding)typeof(System.Text.Encoding).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.InputEncoding = value;

            return __ret;
        }

        static StackObject* get_OutputEncoding_53(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.OutputEncoding;

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_OutputEncoding_54(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Text.Encoding value = (System.Text.Encoding)typeof(System.Text.Encoding).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.OutputEncoding = value;

            return __ret;
        }

        static StackObject* get_BackgroundColor_55(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.BackgroundColor;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_BackgroundColor_56(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.ConsoleColor value = (System.ConsoleColor)typeof(System.ConsoleColor).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.BackgroundColor = value;

            return __ret;
        }

        static StackObject* get_BufferHeight_57(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.BufferHeight;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_BufferHeight_58(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.BufferHeight = value;

            return __ret;
        }

        static StackObject* get_BufferWidth_59(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.BufferWidth;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_BufferWidth_60(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.BufferWidth = value;

            return __ret;
        }

        static StackObject* get_CapsLock_61(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.CapsLock;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* get_CursorLeft_62(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.CursorLeft;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_CursorLeft_63(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.CursorLeft = value;

            return __ret;
        }

        static StackObject* get_CursorTop_64(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.CursorTop;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_CursorTop_65(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.CursorTop = value;

            return __ret;
        }

        static StackObject* get_CursorSize_66(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.CursorSize;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_CursorSize_67(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.CursorSize = value;

            return __ret;
        }

        static StackObject* get_CursorVisible_68(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.CursorVisible;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* set_CursorVisible_69(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean value = ptr_of_this_method->Value == 1;

            System.Console.CursorVisible = value;

            return __ret;
        }

        static StackObject* get_ForegroundColor_70(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.ForegroundColor;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_ForegroundColor_71(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.ConsoleColor value = (System.ConsoleColor)typeof(System.ConsoleColor).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.ForegroundColor = value;

            return __ret;
        }

        static StackObject* get_KeyAvailable_72(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.KeyAvailable;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* get_LargestWindowHeight_73(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.LargestWindowHeight;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_LargestWindowWidth_74(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.LargestWindowWidth;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_NumberLock_75(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.NumberLock;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* get_Title_76(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.Title;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_Title_77(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String value = (System.String)typeof(System.String).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Console.Title = value;

            return __ret;
        }

        static StackObject* get_TreatControlCAsInput_78(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.TreatControlCAsInput;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* set_TreatControlCAsInput_79(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean value = ptr_of_this_method->Value == 1;

            System.Console.TreatControlCAsInput = value;

            return __ret;
        }

        static StackObject* get_WindowHeight_80(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.WindowHeight;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_WindowHeight_81(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.WindowHeight = value;

            return __ret;
        }

        static StackObject* get_WindowLeft_82(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.WindowLeft;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_WindowLeft_83(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.WindowLeft = value;

            return __ret;
        }

        static StackObject* get_WindowTop_84(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.WindowTop;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_WindowTop_85(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.WindowTop = value;

            return __ret;
        }

        static StackObject* get_WindowWidth_86(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.WindowWidth;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_WindowWidth_87(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 value = ptr_of_this_method->Value;

            System.Console.WindowWidth = value;

            return __ret;
        }

        static StackObject* Beep_88(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            System.Console.Beep();

            return __ret;
        }

        static StackObject* Beep_89(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 duration = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 frequency = ptr_of_this_method->Value;

            System.Console.Beep(frequency, duration);

            return __ret;
        }

        static StackObject* Clear_90(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            System.Console.Clear();

            return __ret;
        }

        static StackObject* MoveBufferArea_91(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 6);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 targetTop = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 targetLeft = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 sourceHeight = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Int32 sourceWidth = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Int32 sourceTop = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 6);
            System.Int32 sourceLeft = ptr_of_this_method->Value;

            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);

            return __ret;
        }

        static StackObject* MoveBufferArea_92(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 9);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.ConsoleColor sourceBackColor = (System.ConsoleColor)typeof(System.ConsoleColor).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.ConsoleColor sourceForeColor = (System.ConsoleColor)typeof(System.ConsoleColor).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Char sourceChar = (char)ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Int32 targetTop = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Int32 targetLeft = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 6);
            System.Int32 sourceHeight = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 7);
            System.Int32 sourceWidth = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 8);
            System.Int32 sourceTop = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 9);
            System.Int32 sourceLeft = ptr_of_this_method->Value;

            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor);

            return __ret;
        }

        static StackObject* ReadKey_93(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = System.Console.ReadKey();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ReadKey_94(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean intercept = ptr_of_this_method->Value == 1;

            var result_of_this_method = System.Console.ReadKey(intercept);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ResetColor_95(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            System.Console.ResetColor();

            return __ret;
        }

        static StackObject* SetBufferSize_96(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 height = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 width = ptr_of_this_method->Value;

            System.Console.SetBufferSize(width, height);

            return __ret;
        }

        static StackObject* SetCursorPosition_97(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 top = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 left = ptr_of_this_method->Value;

            System.Console.SetCursorPosition(left, top);

            return __ret;
        }

        static StackObject* SetWindowPosition_98(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 top = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 left = ptr_of_this_method->Value;

            System.Console.SetWindowPosition(left, top);

            return __ret;
        }

        static StackObject* SetWindowSize_99(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 height = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 width = ptr_of_this_method->Value;

            System.Console.SetWindowSize(width, height);

            return __ret;
        }



    }
}
