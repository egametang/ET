using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;

public unsafe class Vector3Binder : ValueTypeBinder<Vector3>
{
    public override unsafe void AssignFromStack(ref Vector3 ins, StackObject* ptr, IList<object> mStack)
    {
        var v = ILIntepreter.Minus(ptr, 1);
        ins.x = *(float*)&v->Value;
        v = ILIntepreter.Minus(ptr, 2);
        ins.y = *(float*)&v->Value;
        v = ILIntepreter.Minus(ptr, 3);
        ins.z = *(float*)&v->Value;
    }

    public override unsafe void CopyValueTypeToStack(ref Vector3 ins, StackObject* ptr, IList<object> mStack)
    {
        var v = ILIntepreter.Minus(ptr, 1);
        *(float*)&v->Value = ins.x;
        v = ILIntepreter.Minus(ptr, 2);
        *(float*)&v->Value = ins.y;
        v = ILIntepreter.Minus(ptr, 3);
        *(float*)&v->Value = ins.z;
    }
    public override void RegisterCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
    {
        BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        MethodBase method;
        Type[] args;
        Type type = typeof(Vector3);
        args = new Type[] { typeof(float), typeof(float), typeof(float) };
        method = type.GetConstructor(flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, NewVector3);

        args = new Type[] { typeof(float), typeof(float) };
        method = type.GetConstructor(flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, NewVector3_2);

        args = new Type[] { typeof(Vector3), typeof(Vector3) };
        method = type.GetMethod("op_Addition", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Add);

        args = new Type[] { typeof(Vector3), typeof(Vector3) };
        method = type.GetMethod("op_Subtraction", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Subtraction);

        args = new Type[] { typeof(Vector3), typeof(float) };
        method = type.GetMethod("op_Multiply", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Multiply);

        args = new Type[] { typeof(float), typeof(Vector3) };
        method = type.GetMethod("op_Multiply", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Multiply2);

        args = new Type[] { typeof(Vector3), typeof(float) };
        method = type.GetMethod("op_Division", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Division);

        args = new Type[] { typeof(Vector3) };
        method = type.GetMethod("op_UnaryNegation", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Negate);

        args = new Type[] { typeof(Vector3), typeof(Vector3) };
        method = type.GetMethod("op_Equality", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Equality);

        args = new Type[] { typeof(Vector3), typeof(Vector3) };
        method = type.GetMethod("op_Inequality", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Inequality);

        args = new Type[] { typeof(Vector3), typeof(Vector3) };
        method = type.GetMethod("Dot", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Dot);

        args = new Type[] { typeof(Vector3), typeof(Vector3) };
        method = type.GetMethod("Cross", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Cross);

        args = new Type[] { typeof(Vector3), typeof(Vector3) };
        method = type.GetMethod("Distance", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Vector3_Distance);

        args = new Type[] { };
        method = type.GetMethod("get_magnitude", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Get_Magnitude);

        args = new Type[] { };
        method = type.GetMethod("get_sqrMagnitude", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Get_SqrMagnitude);

        args = new Type[] { };
        method = type.GetMethod("get_normalized", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Get_Normalized);

        args = new Type[] { };
        method = type.GetMethod("get_one", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Get_One);

        args = new Type[] { };
        method = type.GetMethod("get_zero", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Get_Zero);
    }

    StackObject* Vector3_Add(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 left, right;
        ParseVector3(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out left, intp, ptr, mStack);

        var res = left + right;
        PushVector3(ref res, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Vector3_Subtraction(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 left, right;
        ParseVector3(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out left, intp, ptr, mStack);

        var res = left - right;
        PushVector3(ref res, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Vector3_Multiply(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);

        var ptr = ILIntepreter.Minus(esp, 1);
        var b = ILIntepreter.GetObjectAndResolveReference(ptr);

        float val = *(float*)&b->Value;

        Vector3 vec;

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out vec, intp, ptr, mStack);

        vec = vec * val;
        PushVector3(ref vec, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Vector3_Multiply2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        Vector3 vec;

        var ptr = ILIntepreter.Minus(esp, 1);
        ParseVector3(out vec, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        var b = ILIntepreter.GetObjectAndResolveReference(ptr);

        float val = *(float*)&b->Value;

        vec = val * vec;
        PushVector3(ref vec, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Vector3_Division(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);

        var ptr = ILIntepreter.Minus(esp, 1);
        var b = ILIntepreter.GetObjectAndResolveReference(ptr);

        float val = *(float*)&b->Value;

        Vector3 vec;

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out vec, intp, ptr, mStack);

        vec = vec / val;
        PushVector3(ref vec, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Vector3_Negate(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 1);

        var ptr = ILIntepreter.Minus(esp, 1);
        Vector3 vec;

        ptr = ILIntepreter.Minus(esp, 1);
        ParseVector3(out vec, intp, ptr, mStack);

        vec = -vec;
        PushVector3(ref vec, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Vector3_Equality(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 left, right;
        ParseVector3(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out left, intp, ptr, mStack);

        var res = left == right;

        ret->ObjectType = ObjectTypes.Integer;
        ret->Value = res ? 1 : 0;
        return ret + 1;
    }

    StackObject* Vector3_Inequality(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 left, right;
        ParseVector3(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out left, intp, ptr, mStack);

        var res = left != right;

        ret->ObjectType = ObjectTypes.Integer;
        ret->Value = res ? 1 : 0;
        return ret + 1;
    }

    StackObject* Vector3_Dot(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 left, right;
        ParseVector3(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out left, intp, ptr, mStack);

        var res = Vector3.Dot(left, right);

        ret->ObjectType = ObjectTypes.Float;
        *(float*)&ret->Value = res;
        return ret + 1;
    }

    StackObject* Vector3_Distance(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 left, right;
        ParseVector3(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out left, intp, ptr, mStack);

        var res = Vector3.Distance(left, right);

        ret->ObjectType = ObjectTypes.Float;
        *(float*)&ret->Value = res;
        return ret + 1;
    }

    StackObject* Vector3_Cross(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 left, right;
        ParseVector3(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseVector3(out left, intp, ptr, mStack);

        var res = Vector3.Cross(left, right);
        PushVector3(ref res, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* NewVector3(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        StackObject* ret;
        if (isNewObj)
        {
            ret = ILIntepreter.Minus(esp, 2);
            Vector3 vec;
            var ptr = ILIntepreter.Minus(esp, 1);
            vec.z = *(float*)&ptr->Value;
            ptr = ILIntepreter.Minus(esp, 2);
            vec.y = *(float*)&ptr->Value;
            ptr = ILIntepreter.Minus(esp, 3);
            vec.x = *(float*)&ptr->Value;

            PushVector3(ref vec, intp, ptr, mStack);
        }
        else
        {
            ret = ILIntepreter.Minus(esp, 4);
            var instance = ILIntepreter.GetObjectAndResolveReference(ret);
            var dst = *(StackObject**)&instance->Value;
            var f = ILIntepreter.Minus(dst, 1);
            var v = ILIntepreter.Minus(esp, 3);
            *f = *v;

            f = ILIntepreter.Minus(dst, 2);
            v = ILIntepreter.Minus(esp, 2);
            *f = *v;

            f = ILIntepreter.Minus(dst, 3);
            v = ILIntepreter.Minus(esp, 1);
            *f = *v;
        }
        return ret;
    }
    StackObject* NewVector3_2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        StackObject* ret;
        if (isNewObj)
        {
            ret = ILIntepreter.Minus(esp, 1);
            Vector3 vec;
            var ptr = ILIntepreter.Minus(esp, 1);
            vec.y = *(float*)&ptr->Value;
            ptr = ILIntepreter.Minus(esp, 2);
            vec.x = *(float*)&ptr->Value;
            vec.z = 0;

            PushVector3(ref vec, intp, ptr, mStack);
        }
        else
        {
            ret = ILIntepreter.Minus(esp, 3);
            var instance = ILIntepreter.GetObjectAndResolveReference(ret);
            var dst = *(StackObject**)&instance->Value;
            var f = ILIntepreter.Minus(dst, 1);
            var v = ILIntepreter.Minus(esp, 2);
            *f = *v;

            f = ILIntepreter.Minus(dst, 2);
            v = ILIntepreter.Minus(esp, 1);
            *f = *v;

            f = ILIntepreter.Minus(dst, 3);
            *(float*)&f->Value = 0f;
        }
        return ret;
    }

    StackObject* Get_Magnitude(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 1);

        var ptr = ILIntepreter.Minus(esp, 1);
        Vector3 vec;
        ParseVector3(out vec, intp, ptr, mStack);

        float res = vec.magnitude;

        ret->ObjectType = ObjectTypes.Float;
        *(float*)&ret->Value = res;
        return ret + 1;
    }

    StackObject* Get_SqrMagnitude(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 1);

        var ptr = ILIntepreter.Minus(esp, 1);
        Vector3 vec;
        ParseVector3(out vec, intp, ptr, mStack);

        float res = vec.sqrMagnitude;

        ret->ObjectType = ObjectTypes.Float;
        *(float*)&ret->Value = res;
        return ret + 1;
    }

    StackObject* Get_Normalized(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 1);
        var ptr = ILIntepreter.Minus(esp, 1);
        Vector3 vec;
        ParseVector3(out vec, intp, ptr, mStack);

        var res = vec.normalized;

        PushVector3(ref res, intp, ret, mStack);
        return ret + 1;
    }

    StackObject* Get_One(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = esp;
        var res = Vector3.one;
        PushVector3(ref res, intp, ret, mStack);
        return ret + 1;
    }

    StackObject* Get_Zero(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = esp;
        var res = Vector3.zero;
        PushVector3(ref res, intp, ret, mStack);
        return ret + 1;
    }

    public static void ParseVector3(out Vector3 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
    {
        var a = ILIntepreter.GetObjectAndResolveReference(ptr);
        if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
        {
            var src = *(StackObject**)&a->Value;
            vec.x = *(float*)&ILIntepreter.Minus(src, 1)->Value;
            vec.y = *(float*)&ILIntepreter.Minus(src, 2)->Value;
            vec.z = *(float*)&ILIntepreter.Minus(src, 3)->Value;
            intp.FreeStackValueType(ptr);
        }
        else
        {
            vec = (Vector3)StackObject.ToObject(a, intp.AppDomain, mStack);
            intp.Free(ptr);
        }
    }

    public void PushVector3(ref Vector3 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
    {
        intp.AllocValueType(ptr, CLRType);
        var dst = *((StackObject**)&ptr->Value);
        CopyValueTypeToStack(ref vec, dst, mStack);
    }
}
