using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Stack;

public unsafe class QuaternionBinder : ValueTypeBinder<Quaternion>
{
    Vector3Binder vector3Binder;
    bool vector3BinderGot;

    Vector3Binder Vector3Binder
    {
        get
        {
            if (!vector3BinderGot)
            {
                vector3BinderGot = true;
                var vector3Type = CLRType.AppDomain.GetType(typeof(Vector3)) as CLRType;
                vector3Binder = vector3Type.ValueTypeBinder as Vector3Binder;
            }

            return vector3Binder;
        }
    }

    public override unsafe void AssignFromStack(ref Quaternion ins, StackObject* ptr, IList<object> mStack)
    {
        var v = ILIntepreter.Minus(ptr, 1);
        ins.x = *(float*)&v->Value;
        v = ILIntepreter.Minus(ptr, 2);
        ins.y = *(float*)&v->Value;
        v = ILIntepreter.Minus(ptr, 3);
        ins.z = *(float*)&v->Value;
        v = ILIntepreter.Minus(ptr, 4);
        ins.w = *(float*)&v->Value;
    }

    public override unsafe void CopyValueTypeToStack(ref Quaternion ins, StackObject* ptr, IList<object> mStack)
    {
        var v = ILIntepreter.Minus(ptr, 1);
        *(float*)&v->Value = ins.x;
        v = ILIntepreter.Minus(ptr, 2);
        *(float*)&v->Value = ins.y;
        v = ILIntepreter.Minus(ptr, 3);
        *(float*)&v->Value = ins.z;
        v = ILIntepreter.Minus(ptr, 4);
        *(float*)&v->Value = ins.w;
    }
    public override void RegisterCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
    {
        BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        MethodBase method;
        Type[] args;
        Type type = typeof(Quaternion);
        args = new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) };
        method = type.GetConstructor(flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, NewQuaternion);

        args = new Type[] { typeof(Quaternion), typeof(Quaternion) };
        method = type.GetMethod("op_Multiply", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Multiply);

        args = new Type[] { typeof(Quaternion), typeof(Vector3) };
        method = type.GetMethod("op_Multiply", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Multiply2);

        args = new Type[] { typeof(Quaternion), typeof(Quaternion) };
        method = type.GetMethod("op_Equality", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Equality);

        args = new Type[] { typeof(Quaternion), typeof(Quaternion) };
        method = type.GetMethod("op_Inequality", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Inequality);

        args = new Type[] { typeof(Quaternion), typeof(Quaternion) };
        method = type.GetMethod("Dot", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Dot);

        args = new Type[] { typeof(Quaternion), typeof(Quaternion) };
        method = type.GetMethod("Angle", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Angle);

        args = new Type[] { typeof(Vector3) };
        method = type.GetMethod("Euler", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Euler);

        args = new Type[] { typeof(float), typeof(float), typeof(float) };
        method = type.GetMethod("Euler", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Quaternion_Euler2);

        args = new Type[] { };
        method = type.GetMethod("get_eulerAngles", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Get_EulerAngle);

        args = new Type[] { };
        method = type.GetMethod("get_identity", flag, null, args, null);
        appdomain.RegisterCLRMethodRedirection(method, Get_Identity);
    }

    StackObject* Quaternion_Multiply(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        Quaternion left, right;

        var ptr = ILIntepreter.Minus(esp, 1);
        ParseQuaternion(out right, intp, ptr, mStack);
        
        ptr = ILIntepreter.Minus(esp, 2);
        ParseQuaternion(out left, intp, ptr, mStack);

        var res = left * right;
        PushQuaternion(ref res, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Quaternion_Multiply2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        Vector3 vec;
        Quaternion left;

        var ptr = ILIntepreter.Minus(esp, 1);
        Vector3Binder.ParseVector3(out vec, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseQuaternion(out left, intp, ptr, mStack);

        vec = left * vec;
        PushVector3(ref vec, intp, ret, mStack);

        return ret + 1;
    }

    StackObject* Quaternion_Equality(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Quaternion left, right;
        ParseQuaternion(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseQuaternion(out left, intp, ptr, mStack);

        var res = left == right;

        ret->ObjectType = ObjectTypes.Integer;
        ret->Value = res ? 1 : 0;
        return ret + 1;
    }

    StackObject* Quaternion_Inequality(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Quaternion left, right;
        ParseQuaternion(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseQuaternion(out left, intp, ptr, mStack);

        var res = left != right;

        ret->ObjectType = ObjectTypes.Integer;
        ret->Value = res ? 1 : 0;
        return ret + 1;
    }

    StackObject* Quaternion_Dot(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Quaternion left, right;
        ParseQuaternion(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseQuaternion(out left, intp, ptr, mStack);

        var res = Quaternion.Dot(left, right);

        ret->ObjectType = ObjectTypes.Float;
        *(float*)&ret->Value = res;
        return ret + 1;
    }

    StackObject* Quaternion_Angle(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 2);
        var ptr = ILIntepreter.Minus(esp, 1);

        Quaternion left, right;
        ParseQuaternion(out right, intp, ptr, mStack);

        ptr = ILIntepreter.Minus(esp, 2);
        ParseQuaternion(out left, intp, ptr, mStack);

        var res = Quaternion.Angle(left, right);

        ret->ObjectType = ObjectTypes.Float;
        *(float*)&ret->Value = res;
        return ret + 1;
    }

    StackObject* Quaternion_Euler(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 1);
        var ptr = ILIntepreter.Minus(esp, 1);

        Vector3 vec;
        Vector3Binder.ParseVector3(out vec, intp, ptr, mStack);

        var res = Quaternion.Euler(vec);

        PushQuaternion(ref res, intp, ptr, mStack);
        return ret + 1;
    }

    StackObject* Quaternion_Euler2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 3);
        float x, y, z;

        var ptr = ILIntepreter.GetObjectAndResolveReference(ILIntepreter.Minus(esp, 1));
        z = *(float*)&ptr->Value;

        ptr = ILIntepreter.GetObjectAndResolveReference(ILIntepreter.Minus(esp, 2));
        y = *(float*)&ptr->Value;

        ptr = ILIntepreter.GetObjectAndResolveReference(ILIntepreter.Minus(esp, 3));
        x = *(float*)&ptr->Value;

        var res = Quaternion.Euler(x, y, z);

        PushQuaternion(ref res, intp, ptr, mStack);
        return ret + 1;
    }

    StackObject* NewQuaternion(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        StackObject* ret;
        if (isNewObj)
        {
            ret = ILIntepreter.Minus(esp, 3);
            Quaternion vec;
            var ptr = ILIntepreter.Minus(esp, 1);
            vec.w = *(float*)&ptr->Value;
            ptr = ILIntepreter.Minus(esp, 2);
            vec.z = *(float*)&ptr->Value;
            ptr = ILIntepreter.Minus(esp, 3);
            vec.y = *(float*)&ptr->Value;
            ptr = ILIntepreter.Minus(esp, 4);
            vec.x = *(float*)&ptr->Value;

            PushQuaternion(ref vec, intp, ptr, mStack);
        }
        else
        {
            ret = ILIntepreter.Minus(esp, 5);
            var instance = ILIntepreter.GetObjectAndResolveReference(ret);
            var dst = *(StackObject**)&instance->Value;
            var f = ILIntepreter.Minus(dst, 1);
            var v = ILIntepreter.Minus(esp, 4);
            *f = *v;

            f = ILIntepreter.Minus(dst, 2);
            v = ILIntepreter.Minus(esp, 3);
            *f = *v;

            f = ILIntepreter.Minus(dst, 3);
            v = ILIntepreter.Minus(esp, 2);
            *f = *v;

            f = ILIntepreter.Minus(dst, 4);
            v = ILIntepreter.Minus(esp, 1);
            *f = *v;
        }
        return ret;
    }

    StackObject* Get_EulerAngle(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = ILIntepreter.Minus(esp, 1);

        var ptr = ILIntepreter.Minus(esp, 1);
        Quaternion vec;
        ParseQuaternion(out vec, intp, ptr, mStack);

        var res = vec.eulerAngles;

        PushVector3(ref res, intp, ret, mStack);
        return ret + 1;
    }

    StackObject* Get_Identity(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
    {
        var ret = esp;

        var res = Quaternion.identity;
        PushQuaternion(ref res, intp, ret, mStack);
        return ret + 1;
    }

    static void ParseQuaternion(out Quaternion vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
    {
        var a = ILIntepreter.GetObjectAndResolveReference(ptr);
        if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
        {
            var src = *(StackObject**)&a->Value;
            vec.x = *(float*)&ILIntepreter.Minus(src, 1)->Value;
            vec.y = *(float*)&ILIntepreter.Minus(src, 2)->Value;
            vec.z = *(float*)&ILIntepreter.Minus(src, 3)->Value;
            vec.w = *(float*)&ILIntepreter.Minus(src, 4)->Value;
            intp.FreeStackValueType(ptr);
        }
        else
        {
            vec = (Quaternion)StackObject.ToObject(a, intp.AppDomain, mStack);
            intp.Free(ptr);
        }
    }

    void PushQuaternion(ref Quaternion vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
    {
        intp.AllocValueType(ptr, CLRType);
        var dst = *((StackObject**)&ptr->Value);
        CopyValueTypeToStack(ref vec, dst, mStack);
    }

    void PushVector3(ref Vector3 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
    {
        var binder = Vector3Binder;
        if (binder != null)
            binder.PushVector3(ref vec, intp, ptr, mStack);
        else
            ILIntepreter.PushObject(ptr, mStack, vec, true);
    }
}
