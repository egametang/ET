using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;


namespace CLRSharp
{
    /// <summary>
    /// 线程上下文
    /// 一个线程上下文表示一次调用，直到结束
    /// </summary>
    public class ThreadContext
    {
        [ThreadStatic]
        static ThreadContext _activeContext = null;
        public static ThreadContext activeContext
        {
            get
            {
                return _activeContext;
            }
        }
        public ICLRSharp_Environment environment
        {
            get;
            private set;
        }
        public int DebugLevel
        {
            get;
            private set;
        }
        public ThreadContext(ICLRSharp_Environment env)
        {
            this.environment = env;
            DebugLevel = 0;
            if (_activeContext != null)
            {
                env.logger.Log_Error("在同一线程上多次创建ThreadContext");
            }
            _activeContext = this;

        }
        public ThreadContext(ICLRSharp_Environment env, int DebugLevel)
        {
            this.environment = env;
            this.DebugLevel = DebugLevel;
            if (_activeContext != null)
            {
                env.logger.Log_Error("在同一线程上多次创建ThreadContext");
            }
            _activeContext = this;

        }
        public Stack<StackFrame> GetStackFrames()
        {
            return stacks;
        }
        Stack<StackFrame> stacks = new Stack<StackFrame>();
        public bool SetNoTry = false;
        public string Dump()
        {
            string str = "";
            foreach (StackFrame s in GetStackFrames())
            {
                var pos = s.GetCode();
                Instruction sqIns = pos;
                while (sqIns != null && sqIns.SequencePoint == null)
                {
                    sqIns = sqIns.Previous;
                }
                if (sqIns != null && sqIns.SequencePoint != null)
                {
                    str += sqIns.SequencePoint.Document.Url + "(" + sqIns.SequencePoint.StartLine + ")\n";
                }
                else
                {
                    str += "!no pdb info,no code filename(no line)!\n";
                }
                if (pos == null)
                {
                    continue;
                }
                str += "    IL " + pos.ToString() + "\n";
                if (s._params != null)
                {
                    str += "    ===Params(" + s._params.Length + ")===\n";
                    for (int i = 0; i < s._params.Length; i++)
                    {
                        str += "        param" + i.ToString("D04") + s._params[i] + "\n";
                    }
                }
                str += "    ===VarSlots(" + s.slotVar.Count + ")===\n";
                for (int i = 0; i < s.slotVar.Count; i++)
                {
                    str += "        var" + i.ToString("D04") + s.slotVar[i] + "\n";
                }
            }
            return str;
        }
        public object ExecuteFunc(IMethod_Sharp method, object _this, object[] _params)
        {
            if (this.DebugLevel >= 9)
            {
                environment.logger.Log("<Call>::" + method.DeclaringType.FullName + "::" + method.Name.ToString());

            }
            StackFrame stack = new StackFrame(method.Name, method.isStatic);
            stacks.Push(stack);

            object[] _withp = null;
            bool isctor = method.Name == ".ctor";
            if (isctor)
            {
                //CLRSharp_Instance pthis = new CLRSharp_Instance(GetType(func.ReturnType) as Type_Common_CLRSharp);
                //StackFrame.RefObj pthis = new StackFrame.RefObj(stack, 0, StackFrame.RefType.arg);
                _withp = new object[_params == null ? 1 : (_params.Length + 1)];
                if (_params != null)
                    _params.CopyTo(_withp, 1);
                _withp[0] = _this;
            }
            else
            {
                if (!method.isStatic)
                {
                    _withp = new object[(_params == null) ? 1 : (_params.Length + 1)];
                    _withp[0] = _this;
                    if (_params != null)
                        _params.CopyTo(_withp, 1);
                }
                else
                {
                    _withp = _params;
                }
            }
            stack.SetParams(_withp);

            if (method.body != null)
            {
                stack.Init(method.body);
                stack.SetCodePos(0);
                //._pos = method.body.bodyNative.Instructions[0];
                stack._codepos = 0;
                if (method.body.bodyNative.HasExceptionHandlers && !SetNoTry)
                {
                    RunCodeWithTry(method.body, stack);
                }
                else
                {
                    RunCode(stack, method.body);
                }
            }

            if (this.DebugLevel >= 9)
            {
                environment.logger.Log("<CallEnd>");

            }
            var ret = stacks.Pop().Return();

            return isctor ? _this : ret;

            //if (func.HasBody)
            //{
            //    RunCode(stack, func.Body.Instructions);
            //}
            //var ret = stacks.Pop().Return();
            //if (this.DebugLevel >= 9)
            //{
            //    environment.logger.Log("<CallEnd>");

            //}
            //return ret;



        }

        private void RunCodeWithTry(CodeBody body, StackFrame stack)
        {
            try
            {

                RunCode(stack, body);

            }
            catch (Exception err)
            {
                bool bEH = false;
                if (body.bodyNative.HasExceptionHandlers)
                {
                    bEH = JumpToErr(body, stack, err);
                }
                if (!bEH)
                {
                    throw err;
                }
            }
        }
        public ICLRType GetType(string fullname)
        {
            var type = environment.GetType(fullname);
            ICLRType_Sharp stype = type as ICLRType_Sharp;
            if (stype != null && stype.NeedCCtor)
            {
                //执行.cctor
                stype.InvokeCCtor(this);
            }
            return type;
        }

        public ICLRType GetType(object token)
        {
            token.GetHashCode();
            Mono.Cecil.ModuleDefinition module = null;
            string typename = null;
            if (token is Mono.Cecil.TypeDefinition)
            {
                Mono.Cecil.TypeDefinition _def = (token as Mono.Cecil.TypeDefinition);
                module = _def.Module;
                typename = _def.FullName;
            }
            else if (token is Mono.Cecil.TypeReference)
            {
                Mono.Cecil.TypeReference _ref = (token as Mono.Cecil.TypeReference);
                module = _ref.Module;
                typename = _ref.FullName;
            }
            else
            {
                throw new NotImplementedException();
            }
            return GetType(typename);
        }
        Dictionary<int, IMethod> methodCache = new Dictionary<int, IMethod>();
        Dictionary<int, IField> fieldCache = new Dictionary<int, IField>();
        public IMethod GetMethod(object token)
        {
            try
            {
                IMethod __method = null;
                if (methodCache.TryGetValue(token.GetHashCode(), out __method))
                {
                    return __method;
                }
                Mono.Cecil.ModuleDefinition module = null;
                string methodname = null;
                string typename = null;
                MethodParamList genlist = null;
                MethodParamList list = null;
                if (token is Mono.Cecil.MethodReference)
                {
                    Mono.Cecil.MethodReference _ref = (token as Mono.Cecil.MethodReference);
                    module = _ref.Module;
                    methodname = _ref.Name;
                    typename = _ref.DeclaringType.FullName;
                    list = new MethodParamList(environment, _ref);
                    if (_ref.IsGenericInstance)
                    {
                        Mono.Cecil.GenericInstanceMethod gmethod = _ref as Mono.Cecil.GenericInstanceMethod;
                        genlist = new MethodParamList(environment, gmethod);

                    }
                }
                else if (token is Mono.Cecil.MethodDefinition)
                {
                    Mono.Cecil.MethodDefinition _def = token as Mono.Cecil.MethodDefinition;
                    module = _def.Module;
                    methodname = _def.Name;
                    typename = _def.DeclaringType.FullName;
                    list = new MethodParamList(environment, _def);
                    if (_def.IsGenericInstance)
                    {
                        throw new NotImplementedException();
                        //Mono.Cecil.GenericInstanceMethod gmethod = _def as Mono.Cecil.GenericInstanceMethod;
                        //genlist = new MethodParamList(environment, gmethod);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                var typesys = GetType(typename);
                if (typesys == null)
                {
                    typename = typename.Replace("0...", "");
                    typesys = GetType(typename);

                }
                if (typesys == null)
                {
                    throw new Exception("type can't find:" + typename);
                }

                IMethod _method = null;
                if (genlist != null)
                {
                    _method = typesys.GetMethodT(methodname, genlist, list);
                }
                else
                {
                    _method = typesys.GetMethod(methodname, list);
                }
                methodCache[token.GetHashCode()] = _method;
                return _method;
            }
            catch (Exception err)
            {

                throw new Exception("Error GetMethod==<这意味着这个函数无法被L#找到>" + token, err);
            }
        }
        //IMethod GetNewForArray(object token)
        //{
        //    IMethod __method = null;
        //    if (methodCache.TryGetValue(token.GetHashCode(), out __method))
        //    {
        //        return __method;
        //    }
        //    Mono.Cecil.ModuleDefinition module = null;
        //    string typename = null;
        //    if (token is Mono.Cecil.TypeDefinition)
        //    {
        //        Mono.Cecil.TypeDefinition _def = (token as Mono.Cecil.TypeDefinition);
        //        module = _def.Module;
        //        typename = _def.FullName;
        //    }
        //    else if (token is Mono.Cecil.TypeReference)
        //    {
        //        Mono.Cecil.TypeReference _ref = (token as Mono.Cecil.TypeReference);
        //        module = _ref.Module;
        //        typename = _ref.FullName;
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //    }

        //    ICLRType _type = null;
        //    ICLRType _Itype = GetType(typename);
        //    if (_Itype is ICLRType_Sharp)
        //    {
        //        _type = environment.GetType(typeof(CLRSharp.CLRSharp_Instance[]));
        //    }
        //    else
        //    {
        //        typename += "[]";
        //        //var _type = context.environment.GetType(typename, type.Module);
        //        _type = GetType(typename);

        //    }
        //    MethodParamList tlist = MethodParamList.const_OneParam_Int(environment);
        //    var m = _type.GetMethod(".ctor", tlist);
        //    methodCache[token.GetHashCode()] = m;
        //    return m;
        //}
        public IField GetField(object token)
        {
            IField __field = null;
            if (fieldCache.TryGetValue(token.GetHashCode(), out __field))
            {
                return __field;
            }
            if (token is Mono.Cecil.FieldDefinition)
            {
                Mono.Cecil.FieldDefinition field = token as Mono.Cecil.FieldDefinition;
                var type = GetType(field.DeclaringType.FullName);
                __field = type.GetField(field.Name);



            }
            else if (token is Mono.Cecil.FieldReference)
            {
                Mono.Cecil.FieldReference field = token as Mono.Cecil.FieldReference;
                var type = GetType(field.DeclaringType.FullName);
                __field = type.GetField(field.Name);


            }
            //else if(token is CLRSharp_Instance)
            // {
            //CLRSharp_Instance inst = token as CLRSharp_Instance;
            //return inst.Fields[field.Name];
            // }

            else
            {
                throw new NotImplementedException("不可处理的token" + token.GetType().ToString());
            }
            fieldCache[token.GetHashCode()] = __field;
            return __field;
        }
        object GetToken(object token)
        {
            if (token is Mono.Cecil.FieldDefinition || token is Mono.Cecil.FieldReference)
            {
                var def = token as Mono.Cecil.FieldDefinition;
                if (def != null && def.Name[0] == '$') return def.InitialValue;
                //都是用来初始化数组的方法，忽略

                return GetField(token);

            }

            else if (token is Mono.Cecil.TypeDefinition || token is Mono.Cecil.TypeReference)
            {
                return GetType(token);
            }
            else
            {
                throw new NotImplementedException("不可处理的token" + token.GetType().ToString());
            }
        }
        int GetParamPos(object token)
        {
            if (token is byte)
            {
                return (byte)token;
            }
            else if (token is sbyte)
            {
                return (sbyte)token;
            }
            else if (token is int)
            {
                return (int)token;
            }
            else if (token is Mono.Cecil.ParameterReference)
            {
                int i = (token as Mono.Cecil.ParameterReference).Index;
                //if (this.stacks.Peek().Name == ".ctor" || this.stacks.Peek().IsStatic == false)
                {
                    //i++;
                }
                return i;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        int GetBaseCount(Type _now, Type _base)
        {
            if (_now == _base)
                return 0;
            if (_now.IsSubclassOf(_base) == false)
            {
                return -1;
            }
            return GetBaseCount(_now.BaseType, _base) + 1;
        }
        bool JumpToErr(CodeBody body, StackFrame frame, Exception err)
        {
            var posnow = frame.GetCode();
            List<Mono.Cecil.Cil.ExceptionHandler> ehs = new List<ExceptionHandler>();
            Mono.Cecil.Cil.ExceptionHandler ehNear = null;
            int ehNearB = -1;
            foreach (var eh in body.bodyNative.ExceptionHandlers)
            {
                if (eh.HandlerType == ExceptionHandlerType.Catch)
                {
                    Type ehtype = GetType(eh.CatchType).TypeForSystem;
                    if (ehtype == err.GetType() || err.GetType().IsSubclassOf(ehtype))
                    //if(GetType(eh.CatchType)== environment.GetType(err.GetType()))
                    {
                        if (eh.TryStart.Offset <= posnow.Offset && eh.TryEnd.Offset >= posnow.Offset)
                        {
                            if (ehNear == null)
                            {
                                ehNear = eh;//第一个
                                ehNearB = GetBaseCount(ehtype, err.GetType());
                            }
                            else
                            {
                                if (eh.TryStart.Offset > ehNear.TryStart.Offset || eh.TryEnd.Offset < ehNear.TryEnd.Offset)//范围更小
                                {
                                    ehNear = eh;
                                    ehNearB = GetBaseCount(ehtype, err.GetType());
                                }
                                else if (eh.TryStart.Offset == ehNear.TryStart.Offset || eh.TryEnd.Offset == ehNear.TryEnd.Offset)//范围相等
                                {
                                    if (ehtype == err.GetType())//类型一致，没有比这个更牛的了
                                    {
                                        ehNear = eh;
                                        ehNearB = GetBaseCount(ehtype, err.GetType());
                                    }
                                    else if (GetType(ehNear.CatchType).TypeForSystem == err.GetType())//上次找到的就是第一，不用比了
                                    {
                                        continue;
                                    }
                                    else //比较上次找到的类型，和这次找到的类型的亲缘性；
                                    {
                                        int newehNearB = GetBaseCount(ehtype, err.GetType());
                                        if (newehNearB == -1) continue;
                                        if (newehNearB < ehNearB)
                                        {
                                            ehNear = eh;
                                            ehNearB = newehNearB;
                                        }
                                    }
                                }
                            }
                            ehs.Add(eh);
                        }
                    }

                }
            }
            if (ehNear != null)
            {
                frame.stackCalc.Push(err);
                frame.SetCodePos(ehNear.HandlerStart.Offset);// ._pos = ehNear.HandlerStart;
                RunCodeWithTry(body, frame);
                return true;
            }
            return false;
        }

        void RunCode(StackFrame stack, CodeBody body)
        {
            while (true)
            {
                //var code = stack._pos;
                int _pos = stack._codepos;
                var _code = body.opCodes[_pos];
                if (DebugLevel >= 9)
                {
                    environment.logger.Log(_code.ToString());
                }
                switch (_code.code)
                {
                    ///////////
                    //流程控制
                    case CodeEx.Nop:
                        stack.Nop();
                        break;
                    case CodeEx.Ret:
                        stack.Ret();
                        return;
                    case CodeEx.Leave:
                        stack.Leave(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Leave_S:
                        stack.Leave(_code.tokenAddr_Index);
                        break;
                    //流程控制之goto
                    case CodeEx.Br:
                        stack.Br(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Br_S:
                        stack.Br(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Brtrue:
                        stack.Brtrue(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Brtrue_S:
                        stack.Brtrue(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Brfalse:
                        stack.Brfalse(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Brfalse_S:
                        stack.Brfalse(_code.tokenAddr_Index);
                        break;

                    //比较流程控制
                    case CodeEx.Beq:
                        stack.Beq(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Beq_S:
                        stack.Beq(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bne_Un:
                        stack.Bne_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bne_Un_S:
                        stack.Bne_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bge:
                        stack.Bge(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bge_S:
                        stack.Bge(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bge_Un:
                        stack.Bge_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bge_Un_S:
                        stack.Bge_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bgt:
                        stack.Bgt(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bgt_S:
                        stack.Bgt(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bgt_Un:
                        stack.Bgt_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Bgt_Un_S:
                        stack.Bgt_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Ble:
                        stack.Ble(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Ble_S:
                        stack.Ble(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Ble_Un:
                        stack.Ble_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Ble_Un_S:
                        stack.Ble_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Blt:
                        stack.Blt(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Blt_S:
                        stack.Blt(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Blt_Un:
                        stack.Blt_Un(_code.tokenAddr_Index);
                        break;
                    case CodeEx.Blt_Un_S:
                        stack.Blt_Un(_code.tokenAddr_Index);
                        break;
                    //逻辑计算
                    case CodeEx.Ceq:
                        stack.Ceq();
                        break;
                    case CodeEx.Cgt:
                        stack.Cgt();
                        break;
                    case CodeEx.Cgt_Un:
                        stack.Cgt_Un();
                        break;
                    case CodeEx.Clt:
                        stack.Clt();
                        break;
                    case CodeEx.Clt_Un:
                        stack.Clt_Un();
                        break;
                    case CodeEx.Ckfinite:
                        stack.Ckfinite();
                        break;
                    //常量加载
                    case CodeEx.Ldc_I4:
                        stack.Ldc_I4(_code.tokenI32);
                        break;
                    case CodeEx.Ldc_I4_S:
                        stack.Ldc_I4(_code.tokenI32);
                        break;
                    case CodeEx.Ldc_I4_M1:
                        stack.Ldc_I4(-1);
                        break;
                    case CodeEx.Ldc_I4_0:
                        stack.Ldc_I4(0);
                        break;
                    case CodeEx.Ldc_I4_1:
                        stack.Ldc_I4(1);
                        break;
                    case CodeEx.Ldc_I4_2:
                        stack.Ldc_I4(2);
                        break;
                    case CodeEx.Ldc_I4_3:
                        stack.Ldc_I4(3);
                        break;
                    case CodeEx.Ldc_I4_4:
                        stack.Ldc_I4(4);
                        break;
                    case CodeEx.Ldc_I4_5:
                        stack.Ldc_I4(5);
                        break;
                    case CodeEx.Ldc_I4_6:
                        stack.Ldc_I4(6);
                        break;
                    case CodeEx.Ldc_I4_7:
                        stack.Ldc_I4(7);
                        break;
                    case CodeEx.Ldc_I4_8:
                        stack.Ldc_I4(8);
                        break;
                    case CodeEx.Ldc_I8:
                        stack.Ldc_I8(_code.tokenI64);
                        break;
                    case CodeEx.Ldc_R4:
                        stack.Ldc_R4(_code.tokenR32);
                        break;
                    case CodeEx.Ldc_R8:
                        stack.Ldc_R8(_code.tokenR64);
                        break;

                    //定义为临时变量
                    case CodeEx.Stloc:
                        stack.Stloc(_code.tokenI32);
                        break;
                    case CodeEx.Stloc_S:
                        stack.Stloc(_code.tokenI32);
                        break;
                    case CodeEx.Stloc_0:
                        stack.Stloc(0);
                        break;
                    case CodeEx.Stloc_1:
                        stack.Stloc(1);
                        break;
                    case CodeEx.Stloc_2:
                        stack.Stloc(2);
                        break;
                    case CodeEx.Stloc_3:
                        stack.Stloc(3);
                        break;
                    //从临时变量加载
                    case CodeEx.Ldloc:
                        stack.Ldloc(_code.tokenI32);
                        break;
                    case CodeEx.Ldloc_S:
                        stack.Ldloc(_code.tokenI32);
                        break;
                    case CodeEx.Ldloc_0:
                        stack.Ldloc(0);
                        break;
                    case CodeEx.Ldloc_1:
                        stack.Ldloc(1);
                        break;
                    case CodeEx.Ldloc_2:
                        stack.Ldloc(2);
                        break;
                    case CodeEx.Ldloc_3:
                        stack.Ldloc(3);
                        break;
                    case CodeEx.Ldloca:
                        stack.Ldloca(_code.tokenI32);
                        break;
                    case CodeEx.Ldloca_S:
                        stack.Ldloca(_code.tokenI32);
                        break;
                    //加载字符串
                    case CodeEx.Ldstr:
                        stack.Ldstr(_code.tokenStr);
                        break;
                    //呼叫函数
                    case CodeEx.Call:
                        stack.Call(this, _code.tokenMethod, false);
                        break;
                    case CodeEx.Callvirt:
                        stack.Call(this, _code.tokenMethod, true);
                        break;
                    //算术指令
                    case CodeEx.Add:
                        stack.Add();
                        break;
                    case CodeEx.Sub:
                        stack.Sub();
                        break;
                    case CodeEx.Mul:
                        stack.Mul();
                        break;
                    case CodeEx.Div:
                        stack.Div();
                        break;
                    case CodeEx.Div_Un:
                        stack.Div_Un();
                        break;
                    case CodeEx.Rem:
                        stack.Rem();
                        break;
                    case CodeEx.Rem_Un:
                        stack.Rem_Un();
                        break;
                    case CodeEx.Neg:
                        stack.Neg();
                        break;

                    //装箱
                    case CodeEx.Box:
                        stack.Box(_code.tokenType);
                        break;
                    case CodeEx.Unbox:
                        stack.Unbox();
                        break;
                    case CodeEx.Unbox_Any:
                        stack.Unbox_Any();
                        break;

                    //加载参数
                    case CodeEx.Ldarg:
                        if (body.bodyNative.Method.IsStatic)
                            stack.Ldarg(_code.tokenI32);
                        else
                            stack.Ldarg(_code.tokenI32 + 1);

                        break;
                    case CodeEx.Ldarg_S:
                        if (body.bodyNative.Method.IsStatic)
                            stack.Ldarg(_code.tokenI32);
                        else
                            stack.Ldarg(_code.tokenI32 + 1);
                        break;
                    case CodeEx.Ldarg_0:
                        stack.Ldarg(0);
                        break;
                    case CodeEx.Ldarg_1:
                        stack.Ldarg(1);
                        break;
                    case CodeEx.Ldarg_2:
                        stack.Ldarg(2);
                        break;
                    case CodeEx.Ldarg_3:
                        stack.Ldarg(3);
                        break;
                    case CodeEx.Ldarga:
                        if (body.bodyNative.Method.IsStatic)
                            stack.Ldarga(_code.tokenI32);
                        else
                            stack.Ldarga(_code.tokenI32 + 1);

                        break;
                    case CodeEx.Ldarga_S:
                        if (body.bodyNative.Method.IsStatic)
                            stack.Ldarga(_code.tokenI32);
                        else
                            stack.Ldarga(_code.tokenI32 + 1);

                        break;
                    //转换
                    case CodeEx.Conv_I1:
                        stack.Conv_I1();
                        break;
                    case CodeEx.Conv_U1:
                        stack.Conv_U1();
                        break;
                    case CodeEx.Conv_I2:
                        stack.Conv_I2();
                        break;
                    case CodeEx.Conv_U2:
                        stack.Conv_U2();
                        break;
                    case CodeEx.Conv_I4:
                        stack.Conv_I4();
                        break;
                    case CodeEx.Conv_U4:
                        stack.Conv_U4();
                        break;
                    case CodeEx.Conv_I8:
                        stack.Conv_I8();
                        break;
                    case CodeEx.Conv_U8:
                        stack.Conv_U8();
                        break;
                    case CodeEx.Conv_I:
                        stack.Conv_I();
                        break;
                    case CodeEx.Conv_U:
                        stack.Conv_U();
                        break;
                    case CodeEx.Conv_R4:
                        stack.Conv_R4();
                        break;
                    case CodeEx.Conv_R8:
                        stack.Conv_R8();
                        break;
                    case CodeEx.Conv_R_Un:
                        stack.Conv_R_Un();
                        break;
                    case CodeEx.Conv_Ovf_I1:
                        stack.Conv_Ovf_I1();
                        break;
                    case CodeEx.Conv_Ovf_U1:
                        stack.Conv_Ovf_U1();
                        break;
                    case CodeEx.Conv_Ovf_I2:
                        stack.Conv_Ovf_I2();
                        break;
                    case CodeEx.Conv_Ovf_U2:
                        stack.Conv_Ovf_U2();
                        break;
                    case CodeEx.Conv_Ovf_I4:
                        stack.Conv_Ovf_I4();
                        break;
                    case CodeEx.Conv_Ovf_U4:
                        stack.Conv_Ovf_U4();
                        break;

                    case CodeEx.Conv_Ovf_I8:
                        stack.Conv_Ovf_I8();
                        break;
                    case CodeEx.Conv_Ovf_U8:
                        stack.Conv_Ovf_U8();
                        break;
                    case CodeEx.Conv_Ovf_I:
                        stack.Conv_Ovf_I();
                        break;
                    case CodeEx.Conv_Ovf_U:
                        stack.Conv_Ovf_U();
                        break;
                    case CodeEx.Conv_Ovf_I1_Un:
                        stack.Conv_Ovf_I1_Un();
                        break;

                    case CodeEx.Conv_Ovf_U1_Un:
                        stack.Conv_Ovf_U1_Un();
                        break;
                    case CodeEx.Conv_Ovf_I2_Un:
                        stack.Conv_Ovf_I2_Un();
                        break;
                    case CodeEx.Conv_Ovf_U2_Un:
                        stack.Conv_Ovf_U2_Un();
                        break;
                    case CodeEx.Conv_Ovf_I4_Un:
                        stack.Conv_Ovf_I4_Un();
                        break;
                    case CodeEx.Conv_Ovf_U4_Un:
                        stack.Conv_Ovf_U4_Un();
                        break;

                    case CodeEx.Conv_Ovf_I8_Un:
                        stack.Conv_Ovf_I8_Un();
                        break;
                    case CodeEx.Conv_Ovf_U8_Un:
                        stack.Conv_Ovf_U8_Un();
                        break;
                    case CodeEx.Conv_Ovf_I_Un:
                        stack.Conv_Ovf_I_Un();
                        break;
                    case CodeEx.Conv_Ovf_U_Un:
                        stack.Conv_Ovf_U_Un();
                        break;
                    //数组
                    case CodeEx.Newarr:
                        stack.NewArr(this, _code.tokenType.TypeForSystem);
                        break;
                    case CodeEx.Ldlen:
                        stack.LdLen();
                        break;
                    case CodeEx.Ldelema:
                        stack.Ldelema(_code.tokenUnknown);
                        break;
                    case CodeEx.Ldelem_I1:
                        stack.Ldelem_I1();
                        break;
                    case CodeEx.Ldelem_U1:
                        stack.Ldelem_U1();
                        break;
                    case CodeEx.Ldelem_I2:
                        stack.Ldelem_I2();
                        break;
                    case CodeEx.Ldelem_U2:
                        stack.Ldelem_U2();
                        break;
                    case CodeEx.Ldelem_I4:
                        stack.Ldelem_I4();
                        break;
                    case CodeEx.Ldelem_U4:
                        stack.Ldelem_U4();
                        break;
                    case CodeEx.Ldelem_I8:
                        stack.Ldelem_I8();
                        break;
                    case CodeEx.Ldelem_I:
                        stack.Ldelem_I();
                        break;
                    case CodeEx.Ldelem_R4:
                        stack.Ldelem_R4();
                        break;
                    case CodeEx.Ldelem_R8:
                        stack.Ldelem_R8();
                        break;
                    case CodeEx.Ldelem_Ref:
                        stack.Ldelem_Ref();
                        break;
                    case CodeEx.Ldelem_Any:
                        stack.Ldelem_Any(_code.tokenUnknown);
                        break;

                    case CodeEx.Stelem_I:
                        stack.Stelem_I();
                        break;
                    case CodeEx.Stelem_I1:
                        stack.Stelem_I1();
                        break;
                    case CodeEx.Stelem_I2:
                        stack.Stelem_I2();
                        break;
                    case CodeEx.Stelem_I4:
                        stack.Stelem_I4();
                        break;
                    case CodeEx.Stelem_I8:
                        stack.Stelem_I8();
                        break;
                    case CodeEx.Stelem_R4:
                        stack.Stelem_R4();
                        break;
                    case CodeEx.Stelem_R8:
                        stack.Stelem_R8();
                        break;
                    case CodeEx.Stelem_Ref:
                        stack.Stelem_Ref();
                        break;
                    case CodeEx.Stelem_Any:
                        stack.Stelem_Any();
                        break;

                    case CodeEx.Newobj:
                        stack.NewObj(this, _code.tokenMethod);
                        break;

                    case CodeEx.Dup:
                        stack.Dup();
                        break;
                    case CodeEx.Pop:
                        stack.Pop();
                        break;

                    case CodeEx.Ldfld:
                        stack.Ldfld(this, _code.tokenField);
                        break;
                    case CodeEx.Ldflda:
                        stack.Ldflda(this, _code.tokenField);
                        break;
                    case CodeEx.Ldsfld:
                        stack.Ldsfld(this, _code.tokenField);
                        break;
                    case CodeEx.Ldsflda:
                        stack.Ldsflda(this, _code.tokenField);
                        break;
                    case CodeEx.Stfld:
                        stack.Stfld(this, _code.tokenField);
                        break;
                    case CodeEx.Stsfld:
                        stack.Stsfld(this, _code.tokenField);
                        break;


                    case CodeEx.Constrained:
                        stack.Constrained(this, _code.tokenType);
                        break;

                    case CodeEx.Isinst:
                        stack.Isinst(this, _code.tokenType);
                        break;
                    case CodeEx.Ldtoken:
                        stack.Ldtoken(this, GetToken(_code.tokenUnknown));
                        break;

                    case CodeEx.Ldftn:
                        stack.Ldftn(this, _code.tokenMethod);
                        break;
                    case CodeEx.Ldvirtftn:
                        stack.Ldvirtftn(this, _code.tokenMethod);
                        break;

                    case CodeEx.Calli:
                        stack.Calli(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Starg_S:
                        if (body.bodyNative.Method.IsStatic)
                            stack.Starg(this, _code.tokenI32);
                        else
                            stack.Starg(this, _code.tokenI32 + 1);
                        break;
                    case CodeEx.Starg:
                        if (body.bodyNative.Method.IsStatic)
                            stack.Starg(this, _code.tokenI32);
                        else
                            stack.Starg(this, _code.tokenI32 + 1);
                        break;
                    case CodeEx.Volatile:
                        stack.Volatile();
                        break;
                    ///下面是还没有处理的指令
                    case CodeEx.Break:
                        stack.Break(this, _code.tokenUnknown);
                        break;

                    case CodeEx.Ldnull:
                        stack.Ldnull();
                        break;
                    case CodeEx.Jmp:
                        stack.Jmp(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Switch:
                        stack.Switch(this, _code.tokenAddr_Switch);
                        break;
                    case CodeEx.Ldind_I1:
                        stack.Ldind_I1();
                        break;
                    case CodeEx.Ldind_U1:
                        stack.Ldind_U1();
                        break;
                    case CodeEx.Ldind_I2:
                        stack.Ldind_I2();
                        break;
                    case CodeEx.Ldind_U2:
                        stack.Ldind_U2();
                        break;
                    case CodeEx.Ldind_I4:
                        stack.Ldind_I4();
                        break;
                    case CodeEx.Ldind_U4:
                        stack.Ldind_U4();
                        break;
                    case CodeEx.Ldind_I8:
                        stack.Ldind_I8();
                        break;
                    case CodeEx.Ldind_I:
                        stack.Ldind_I();
                        break;
                    case CodeEx.Ldind_R4:
                        stack.Ldind_R4();
                        break;
                    case CodeEx.Ldind_R8:
                        stack.Ldind_R8();
                        break;
                    case CodeEx.Ldind_Ref:
                        stack.Ldind_Ref();
                        break;
                    case CodeEx.Stind_Ref:
                        stack.Stind_Ref(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stind_I1:
                        stack.Stind_I1(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stind_I2:
                        stack.Stind_I2(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stind_I4:
                        stack.Stind_I4(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stind_I8:
                        stack.Stind_I8(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stind_R4:
                        stack.Stind_R4(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stind_R8:
                        stack.Stind_R8(this, _code.tokenUnknown);
                        break;
                    case CodeEx.And:
                        stack.And();
                        break;
                    case CodeEx.Or:
                        stack.Or();
                        break;
                    case CodeEx.Xor:
                        stack.Xor();
                        break;
                    case CodeEx.Shl:
                        stack.Shl(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Shr:
                        stack.Shr(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Shr_Un:
                        stack.Shr_Un(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Not:
                        stack.Not();
                        break;
                    case CodeEx.Cpobj:
                        stack.Cpobj(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Ldobj:
                        stack.Ldobj(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Castclass:
                        stack.Castclass(this, _code.tokenType);
                        break;
                    case CodeEx.Throw:
                        stack.Throw(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stobj:
                        stack.Stobj(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Refanyval:
                        stack.Refanyval(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Mkrefany:
                        stack.Mkrefany(this, _code.tokenUnknown);
                        break;

                    case CodeEx.Add_Ovf:
                        stack.Add_Ovf(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Add_Ovf_Un:
                        stack.Add_Ovf_Un(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Mul_Ovf:
                        stack.Mul_Ovf(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Mul_Ovf_Un:
                        stack.Mul_Ovf_Un(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Sub_Ovf:
                        stack.Sub_Ovf(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Sub_Ovf_Un:
                        stack.Sub_Ovf_Un(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Endfinally:
                        stack.Endfinally(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Stind_I:
                        stack.Stind_I(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Arglist:
                        stack.Arglist(this, _code.tokenUnknown);
                        break;


                    case CodeEx.Localloc:
                        stack.Localloc(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Endfilter:
                        stack.Endfilter(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Unaligned:
                        stack.Unaligned(this, _code.tokenUnknown);
                        break;

                    case CodeEx.Tail:
                        stack.Tail(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Initobj:
                        stack.Initobj(this, _code.tokenType);
                        break;
                    case CodeEx.Cpblk:
                        stack.Cpblk(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Initblk:
                        stack.Initblk(this, _code.tokenUnknown);
                        break;
                    case CodeEx.No:
                        stack.No(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Rethrow:
                        stack.Rethrow(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Sizeof:
                        stack.Sizeof(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Refanytype:
                        stack.Refanytype(this, _code.tokenUnknown);
                        break;
                    case CodeEx.Readonly:
                        stack.Readonly(this, _code.tokenUnknown);
                        break;
                    default:
                        throw new Exception("未实现的OpCode:" + _code.code);
                }
            }

        }
    }
}
