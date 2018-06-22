using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class UnityEngine_Time_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEngine.Time);
            args = new Type[]{};
            method = type.GetMethod("get_time", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_time_0);
            args = new Type[]{};
            method = type.GetMethod("get_timeSinceLevelLoad", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_timeSinceLevelLoad_1);
            args = new Type[]{};
            method = type.GetMethod("get_deltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_deltaTime_2);
            args = new Type[]{};
            method = type.GetMethod("get_fixedTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_fixedTime_3);
            args = new Type[]{};
            method = type.GetMethod("get_unscaledTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_unscaledTime_4);
            args = new Type[]{};
            method = type.GetMethod("get_fixedUnscaledTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_fixedUnscaledTime_5);
            args = new Type[]{};
            method = type.GetMethod("get_unscaledDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_unscaledDeltaTime_6);
            args = new Type[]{};
            method = type.GetMethod("get_fixedUnscaledDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_fixedUnscaledDeltaTime_7);
            args = new Type[]{};
            method = type.GetMethod("get_fixedDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_fixedDeltaTime_8);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("set_fixedDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_fixedDeltaTime_9);
            args = new Type[]{};
            method = type.GetMethod("get_maximumDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_maximumDeltaTime_10);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("set_maximumDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_maximumDeltaTime_11);
            args = new Type[]{};
            method = type.GetMethod("get_smoothDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_smoothDeltaTime_12);
            args = new Type[]{};
            method = type.GetMethod("get_maximumParticleDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_maximumParticleDeltaTime_13);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("set_maximumParticleDeltaTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_maximumParticleDeltaTime_14);
            args = new Type[]{};
            method = type.GetMethod("get_timeScale", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_timeScale_15);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("set_timeScale", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_timeScale_16);
            args = new Type[]{};
            method = type.GetMethod("get_frameCount", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_frameCount_17);
            args = new Type[]{};
            method = type.GetMethod("get_renderedFrameCount", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_renderedFrameCount_18);
            args = new Type[]{};
            method = type.GetMethod("get_realtimeSinceStartup", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_realtimeSinceStartup_19);
            args = new Type[]{};
            method = type.GetMethod("get_captureFramerate", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_captureFramerate_20);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_captureFramerate", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_captureFramerate_21);
            args = new Type[]{};
            method = type.GetMethod("get_inFixedTimeStep", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_inFixedTimeStep_22);



            app.RegisterCLRCreateDefaultInstance(type, () => new UnityEngine.Time());
            app.RegisterCLRCreateArrayInstance(type, s => new UnityEngine.Time[s]);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* get_time_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.time;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_timeSinceLevelLoad_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.timeSinceLevelLoad;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_deltaTime_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.deltaTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_fixedTime_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.fixedTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_unscaledTime_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.unscaledTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_fixedUnscaledTime_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.fixedUnscaledTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_unscaledDeltaTime_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.unscaledDeltaTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_fixedUnscaledDeltaTime_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.fixedUnscaledDeltaTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_fixedDeltaTime_8(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.fixedDeltaTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_fixedDeltaTime_9(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @value = *(float*)&ptr_of_this_method->Value;


            UnityEngine.Time.fixedDeltaTime = value;

            return __ret;
        }

        static StackObject* get_maximumDeltaTime_10(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.maximumDeltaTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_maximumDeltaTime_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @value = *(float*)&ptr_of_this_method->Value;


            UnityEngine.Time.maximumDeltaTime = value;

            return __ret;
        }

        static StackObject* get_smoothDeltaTime_12(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.smoothDeltaTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_maximumParticleDeltaTime_13(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.maximumParticleDeltaTime;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_maximumParticleDeltaTime_14(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @value = *(float*)&ptr_of_this_method->Value;


            UnityEngine.Time.maximumParticleDeltaTime = value;

            return __ret;
        }

        static StackObject* get_timeScale_15(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.timeScale;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_timeScale_16(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @value = *(float*)&ptr_of_this_method->Value;


            UnityEngine.Time.timeScale = value;

            return __ret;
        }

        static StackObject* get_frameCount_17(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.frameCount;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_renderedFrameCount_18(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.renderedFrameCount;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_realtimeSinceStartup_19(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.realtimeSinceStartup;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_captureFramerate_20(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.captureFramerate;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* set_captureFramerate_21(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @value = ptr_of_this_method->Value;


            UnityEngine.Time.captureFramerate = value;

            return __ret;
        }

        static StackObject* get_inFixedTimeStep_22(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.Time.inFixedTimeStep;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }




        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new UnityEngine.Time();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
