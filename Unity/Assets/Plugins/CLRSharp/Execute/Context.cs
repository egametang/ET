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
        }
        public ThreadContext(ICLRSharp_Environment env, int DebugLevel)
        {
            this.environment = env;
            this.DebugLevel = DebugLevel;
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
                var pos = s._pos;

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
                    str +="!no pdb info,no code filename(no line)!\n";
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
            _activeContext = this;
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
                stack._pos = method.body.bodyNative.Instructions[0];

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
        ICLRType GetType(string fullname)
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

        ICLRType GetType(object token)
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
        IMethod GetMethod(object token)
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
                throw new Exception("type can't find:" + typename);


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
        IMethod GetNewForArray(object token)
        {
            IMethod __method = null;
            if (methodCache.TryGetValue(token.GetHashCode(), out __method))
            {
                return __method;
            }
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

            ICLRType _Itype = GetType(typename);
            typename += "[]";
            //var _type = context.environment.GetType(typename, type.Module);
            var _type = GetType(typename);

            MethodParamList tlist = MethodParamList.const_OneParam_Int(environment);
            var m = _type.GetMethod(".ctor", tlist);
            methodCache[token.GetHashCode()] = m;
            return m;
        }
        IField GetField(object token)
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
                if (this.stacks.Peek().Name == ".ctor" || this.stacks.Peek().IsStatic == false)
                {
                    i++;
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
            var posnow = frame._pos;
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
                frame.Ldobj(this, err);
                frame._pos = ehNear.HandlerStart;
                RunCodeWithTry(body, frame);
                return true;
            }
            return false;
        }

        void RunCode(StackFrame stack, CodeBody body)
        {
            Mono.Collections.Generic.Collection<Mono.Cecil.Cil.Instruction> codes = body.bodyNative.Instructions;
            while (true)
            {
                var code = stack._pos;
                if (DebugLevel >= 9)
                {
                    environment.logger.Log(code.ToString());
                }
                switch (code.OpCode.Code)
                {

                    ///////////
                    //流程控制

                    case Code.Nop:
                        stack.Nop();
                        break;
                    case Code.Ret:
                        stack.Ret();
                        return;
                    case Code.Leave:
                        stack.Leave(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Leave_S:
                        //stack.Ret();
                        stack.Leave(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    //流程控制之goto
                    case Code.Br:
                        stack.Br(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Br_S:
                        stack.Br(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Brtrue:
                        stack.Brtrue(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Brtrue_S:
                        stack.Brtrue(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Brfalse:
                        stack.Brfalse(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Brfalse_S:
                        stack.Brfalse(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;

                    //比较流程控制
                    case Code.Beq:
                        stack.Beq(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Beq_S:
                        stack.Beq(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bne_Un:
                        stack.Bne_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bne_Un_S:
                        stack.Bne_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bge:
                        stack.Bge(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bge_S:
                        stack.Bge(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bge_Un:
                        stack.Bge_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bge_Un_S:
                        stack.Bge_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bgt:
                        stack.Bgt(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bgt_S:
                        stack.Bgt(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bgt_Un:
                        stack.Bgt_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Bgt_Un_S:
                        stack.Bge_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Ble:
                        stack.Ble(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Ble_S:
                        stack.Ble(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Ble_Un:
                        stack.Ble_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Ble_Un_S:
                        stack.Ble_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Blt:
                        stack.Blt(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Blt_S:
                        stack.Blt(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Blt_Un:
                        stack.Blt_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    case Code.Blt_Un_S:
                        stack.Ble_Un(code.Operand as Mono.Cecil.Cil.Instruction);
                        break;
                    //逻辑计算
                    case Code.Ceq:
                        stack.Ceq();
                        break;
                    case Code.Cgt:
                        stack.Cgt();
                        break;
                    case Code.Cgt_Un:
                        stack.Cgt_Un();
                        break;
                    case Code.Clt:
                        stack.Clt();
                        break;
                    case Code.Clt_Un:
                        stack.Clt_Un();
                        break;
                    case Code.Ckfinite:
                        stack.Ckfinite();
                        break;
                    //常量加载
                    case Code.Ldc_I4:
                        stack.Ldc_I4((int)Convert.ToDecimal(code.Operand));
                        break;
                    case Code.Ldc_I4_S:
                        stack.Ldc_I4((int)Convert.ToDecimal(code.Operand));
                        break;
                    case Code.Ldc_I4_M1:
                        stack.Ldc_I4(-1);
                        break;
                    case Code.Ldc_I4_0:
                        stack.Ldc_I4(0);
                        break;
                    case Code.Ldc_I4_1:
                        stack.Ldc_I4(1);
                        break;
                    case Code.Ldc_I4_2:
                        stack.Ldc_I4(2);
                        break;
                    case Code.Ldc_I4_3:
                        stack.Ldc_I4(3);
                        break;
                    case Code.Ldc_I4_4:
                        stack.Ldc_I4(4);
                        break;
                    case Code.Ldc_I4_5:
                        stack.Ldc_I4(5);
                        break;
                    case Code.Ldc_I4_6:
                        stack.Ldc_I4(6);
                        break;
                    case Code.Ldc_I4_7:
                        stack.Ldc_I4(7);
                        break;
                    case Code.Ldc_I4_8:
                        stack.Ldc_I4(8);
                        break;
                    case Code.Ldc_I8:
                        stack.Ldc_I8((Int64)(Convert.ToDecimal(code.Operand)));
                        break;
                    case Code.Ldc_R4:
                        stack.Ldc_R4((float)(Convert.ToDecimal(code.Operand)));
                        break;
                    case Code.Ldc_R8:
                        stack.Ldc_R8((double)(Convert.ToDecimal(code.Operand)));
                        break;

                    //定义为临时变量
                    case Code.Stloc:
                        stack.Stloc((int)code.Operand);
                        break;
                    case Code.Stloc_S:
                        stack.Stloc(((VariableDefinition)code.Operand).Index);
                        break;
                    case Code.Stloc_0:
                        stack.Stloc(0);
                        break;
                    case Code.Stloc_1:
                        stack.Stloc(1);
                        break;
                    case Code.Stloc_2:
                        stack.Stloc(2);
                        break;
                    case Code.Stloc_3:
                        stack.Stloc(3);
                        break;
                    //从临时变量加载
                    case Code.Ldloc:
                        stack.Ldloc((int)code.Operand);
                        break;
                    case Code.Ldloc_S:
                        stack.Ldloc(((VariableDefinition)code.Operand).Index);
                        break;
                    case Code.Ldloc_0:
                        stack.Ldloc(0);
                        break;
                    case Code.Ldloc_1:
                        stack.Ldloc(1);
                        break;
                    case Code.Ldloc_2:
                        stack.Ldloc(2);
                        break;
                    case Code.Ldloc_3:
                        stack.Ldloc(3);
                        break;
                    case Code.Ldloca:
                        stack.Ldloca(((VariableDefinition)code.Operand).Index);
                        break;
                    case Code.Ldloca_S:
                        stack.Ldloca(((VariableDefinition)code.Operand).Index);
                        break;
                    //加载字符串
                    case Code.Ldstr:
                        stack.Ldstr(code.Operand as string);
                        break;
                    //呼叫函数
                    case Code.Call:
                        stack.Call(this, GetMethod(code.Operand), false);
                        break;
                    case Code.Callvirt:
                        stack.Call(this, GetMethod(code.Operand), true);
                        break;
                    //算术指令
                    case Code.Add:
                        stack.Add();
                        break;
                    case Code.Sub:
                        stack.Sub();
                        break;
                    case Code.Mul:
                        stack.Mul();
                        break;
                    case Code.Div:
                        stack.Div();
                        break;
                    case Code.Div_Un:
                        stack.Div_Un();
                        break;
                    case Code.Rem:
                        stack.Rem();
                        break;
                    case Code.Rem_Un:
                        stack.Rem_Un();
                        break;
                    case Code.Neg:
                        stack.Neg();
                        break;

                    //装箱
                    case Code.Box:
                        stack.Box(GetType(code.Operand));
                        break;
                    case Code.Unbox:
                        stack.Unbox();
                        break;
                    case Code.Unbox_Any:
                        stack.Unbox_Any();
                        break;

                    //加载参数
                    case Code.Ldarg:
                        stack.Ldarg((int)code.Operand);
                        break;
                    case Code.Ldarg_S:
                        stack.Ldarg(GetParamPos(code.Operand));
                        break;
                    case Code.Ldarg_0:
                        stack.Ldarg(0);
                        break;
                    case Code.Ldarg_1:
                        stack.Ldarg(1);
                        break;
                    case Code.Ldarg_2:
                        stack.Ldarg(2);
                        break;
                    case Code.Ldarg_3:
                        stack.Ldarg(3);
                        break;
                    //转换
                    case Code.Conv_I1:
                        stack.Conv_I1();
                        break;
                    case Code.Conv_U1:
                        stack.Conv_U1();
                        break;
                    case Code.Conv_I2:
                        stack.Conv_I2();
                        break;
                    case Code.Conv_U2:
                        stack.Conv_U2();
                        break;
                    case Code.Conv_I4:
                        stack.Conv_I4();
                        break;
                    case Code.Conv_U4:
                        stack.Conv_U4();
                        break;
                    case Code.Conv_I8:
                        stack.Conv_I8();
                        break;
                    case Code.Conv_U8:
                        stack.Conv_U8();
                        break;
                    case Code.Conv_I:
                        stack.Conv_I();
                        break;
                    case Code.Conv_U:
                        stack.Conv_U();
                        break;
                    case Code.Conv_R4:
                        stack.Conv_R4();
                        break;
                    case Code.Conv_R8:
                        stack.Conv_R8();
                        break;
                    case Code.Conv_R_Un:
                        stack.Conv_R_Un();
                        break;
                    case Code.Conv_Ovf_I1:
                        stack.Conv_Ovf_I1();
                        break;
                    case Code.Conv_Ovf_U1:
                        stack.Conv_Ovf_U1();
                        break;
                    case Code.Conv_Ovf_I2:
                        stack.Conv_Ovf_I2();
                        break;
                    case Code.Conv_Ovf_U2:
                        stack.Conv_Ovf_U2();
                        break;
                    case Code.Conv_Ovf_I4:
                        stack.Conv_Ovf_I4();
                        break;
                    case Code.Conv_Ovf_U4:
                        stack.Conv_Ovf_U4();
                        break;

                    case Code.Conv_Ovf_I8:
                        stack.Conv_Ovf_I8();
                        break;
                    case Code.Conv_Ovf_U8:
                        stack.Conv_Ovf_U8();
                        break;
                    case Code.Conv_Ovf_I:
                        stack.Conv_Ovf_I();
                        break;
                    case Code.Conv_Ovf_U:
                        stack.Conv_Ovf_U();
                        break;
                    case Code.Conv_Ovf_I1_Un:
                        stack.Conv_Ovf_I1_Un();
                        break;

                    case Code.Conv_Ovf_U1_Un:
                        stack.Conv_Ovf_U1_Un();
                        break;
                    case Code.Conv_Ovf_I2_Un:
                        stack.Conv_Ovf_I2_Un();
                        break;
                    case Code.Conv_Ovf_U2_Un:
                        stack.Conv_Ovf_U2_Un();
                        break;
                    case Code.Conv_Ovf_I4_Un:
                        stack.Conv_Ovf_I4_Un();
                        break;
                    case Code.Conv_Ovf_U4_Un:
                        stack.Conv_Ovf_U4_Un();
                        break;

                    case Code.Conv_Ovf_I8_Un:
                        stack.Conv_Ovf_I8_Un();
                        break;
                    case Code.Conv_Ovf_U8_Un:
                        stack.Conv_Ovf_U8_Un();
                        break;
                    case Code.Conv_Ovf_I_Un:
                        stack.Conv_Ovf_I_Un();
                        break;
                    case Code.Conv_Ovf_U_Un:
                        stack.Conv_Ovf_U_Un();
                        break;
                    //数组
                    case Code.Newarr:
                        stack.NewArr(this, GetNewForArray(code.Operand));
                        break;
                    case Code.Ldlen:
                        stack.LdLen();
                        break;
                    case Code.Ldelema:
                        stack.Ldelema(code.Operand);
                        break;
                    case Code.Ldelem_I1:
                        stack.Ldelem_I1();
                        break;
                    case Code.Ldelem_U1:
                        stack.Ldelem_U1();
                        break;
                    case Code.Ldelem_I2:
                        stack.Ldelem_I2();
                        break;
                    case Code.Ldelem_U2:
                        stack.Ldelem_U2();
                        break;
                    case Code.Ldelem_I4:
                        stack.Ldelem_I4();
                        break;
                    case Code.Ldelem_U4:
                        stack.Ldelem_U4();
                        break;
                    case Code.Ldelem_I8:
                        stack.Ldelem_I8();
                        break;
                    case Code.Ldelem_I:
                        stack.Ldelem_I();
                        break;
                    case Code.Ldelem_R4:
                        stack.Ldelem_R4();
                        break;
                    case Code.Ldelem_R8:
                        stack.Ldelem_R8();
                        break;
                    case Code.Ldelem_Ref:
                        stack.Ldelem_Ref();
                        break;
                    case Code.Ldelem_Any:
                        stack.Ldelem_Any(code.Operand);
                        break;

                    case Code.Stelem_I:
                        stack.Stelem_I();
                        break;
                    case Code.Stelem_I1:
                        stack.Stelem_I1();
                        break;
                    case Code.Stelem_I2:
                        stack.Stelem_I2();
                        break;
                    case Code.Stelem_I4:
                        stack.Stelem_I4();
                        break;
                    case Code.Stelem_I8:
                        stack.Stelem_I8();
                        break;
                    case Code.Stelem_R4:
                        stack.Stelem_R4();
                        break;
                    case Code.Stelem_R8:
                        stack.Stelem_R8();
                        break;
                    case Code.Stelem_Ref:
                        stack.Stelem_Ref();
                        break;
                    case Code.Stelem_Any:
                        stack.Stelem_Any();
                        break;

                    case Code.Newobj:
                        stack.NewObj(this, GetMethod(code.Operand));
                        break;

                    case Code.Dup:
                        stack.Dup();
                        break;
                    case Code.Pop:
                        stack.Pop();
                        break;

                    case Code.Ldfld:
                        stack.Ldfld(this, GetField(code.Operand));
                        break;
                    case Code.Ldflda:
                        stack.Ldflda(this, GetField(code.Operand));
                        break;
                    case Code.Ldsfld:
                        stack.Ldsfld(this, GetField(code.Operand));
                        break;
                    case Code.Ldsflda:
                        stack.Ldsflda(this, GetField(code.Operand));
                        break;
                    case Code.Stfld:
                        stack.Stfld(this, GetField(code.Operand));
                        break;
                    case Code.Stsfld:
                        stack.Stsfld(this, GetField(code.Operand));
                        break;


                    case Code.Constrained:
                        stack.Constrained(this, GetType(code.Operand));
                        break;

                    case Code.Isinst:
                        stack.Isinst(this, GetType(code.Operand));
                        break;
                    case Code.Ldtoken:
                        stack.Ldtoken(this, GetToken(code.Operand));
                        break;

                    case Code.Ldftn:
                        stack.Ldftn(this, GetMethod(code.Operand));
                        break;
                    case Code.Ldvirtftn:
                        stack.Ldvirtftn(this, GetMethod(code.Operand));
                        break;
                    case Code.Ldarga:
                        stack.Ldarga(this, code.Operand);
                        break;
                    case Code.Ldarga_S:
                        stack.Ldarga(this, code.Operand);
                        break;
                    case Code.Calli:
                        stack.Calli(this, code.Operand);
                        break;
                    ///下面是还没有处理的指令
                    case Code.Break:
                        stack.Break(this, code.Operand);
                        break;
                    case Code.Starg_S:
                        stack.Starg_S(this, code.Operand);
                        break;
                    case Code.Ldnull:
                        stack.Ldnull();
                        break;
                    case Code.Jmp:
                        stack.Jmp(this, code.Operand);
                        break;
                    case Code.Switch:
                        stack.Switch(this, code.Operand as Mono.Cecil.Cil.Instruction[]);
                        break;
                    case Code.Ldind_I1:
                        stack.Ldind_I1(this, code.Operand);
                        break;
                    case Code.Ldind_U1:
                        stack.Ldind_U1(this, code.Operand);
                        break;
                    case Code.Ldind_I2:
                        stack.Ldind_I2(this, code.Operand);
                        break;
                    case Code.Ldind_U2:
                        stack.Ldind_U2(this, code.Operand);
                        break;
                    case Code.Ldind_I4:
                        stack.Ldind_I4(this, code.Operand);
                        break;
                    case Code.Ldind_U4:
                        stack.Ldind_U4(this, code.Operand);
                        break;
                    case Code.Ldind_I8:
                        stack.Ldind_I8(this, code.Operand);
                        break;
                    case Code.Ldind_I:
                        stack.Ldind_I(this, code.Operand);
                        break;
                    case Code.Ldind_R4:
                        stack.Ldind_R4(this, code.Operand);
                        break;
                    case Code.Ldind_R8:
                        stack.Ldind_R8(this, code.Operand);
                        break;
                    case Code.Ldind_Ref:
                        stack.Ldind_Ref(this, code.Operand);
                        break;
                    case Code.Stind_Ref:
                        stack.Stind_Ref(this, code.Operand);
                        break;
                    case Code.Stind_I1:
                        stack.Stind_I1(this, code.Operand);
                        break;
                    case Code.Stind_I2:
                        stack.Stind_I2(this, code.Operand);
                        break;
                    case Code.Stind_I4:
                        stack.Stind_I4(this, code.Operand);
                        break;
                    case Code.Stind_I8:
                        stack.Stind_I8(this, code.Operand);
                        break;
                    case Code.Stind_R4:
                        stack.Stind_R4(this, code.Operand);
                        break;
                    case Code.Stind_R8:
                        stack.Stind_R8(this, code.Operand);
                        break;
                    case Code.And:
                        stack.And(this, code.Operand);
                        break;
                    case Code.Or:
                        stack.Or(this, code.Operand);
                        break;
                    case Code.Xor:
                        stack.Xor(this, code.Operand);
                        break;
                    case Code.Shl:
                        stack.Shl(this, code.Operand);
                        break;
                    case Code.Shr:
                        stack.Shr(this, code.Operand);
                        break;
                    case Code.Shr_Un:
                        stack.Shr_Un(this, code.Operand);
                        break;
                    case Code.Not:
                        stack.Not(this, code.Operand);
                        break;
                    case Code.Cpobj:
                        stack.Cpobj(this, code.Operand);
                        break;
                    case Code.Ldobj:
                        stack.Ldobj(this, code.Operand);
                        break;
                    case Code.Castclass:
                        stack.Castclass(this, code.Operand);
                        break;
                    case Code.Throw:
                        stack.Throw(this, code.Operand);
                        break;
                    case Code.Stobj:
                        stack.Stobj(this, code.Operand);
                        break;
                    case Code.Refanyval:
                        stack.Refanyval(this, code.Operand);
                        break;
                    case Code.Mkrefany:
                        stack.Mkrefany(this, code.Operand);
                        break;

                    case Code.Add_Ovf:
                        stack.Add_Ovf(this, code.Operand);
                        break;
                    case Code.Add_Ovf_Un:
                        stack.Add_Ovf_Un(this, code.Operand);
                        break;
                    case Code.Mul_Ovf:
                        stack.Mul_Ovf(this, code.Operand);
                        break;
                    case Code.Mul_Ovf_Un:
                        stack.Mul_Ovf_Un(this, code.Operand);
                        break;
                    case Code.Sub_Ovf:
                        stack.Sub_Ovf(this, code.Operand);
                        break;
                    case Code.Sub_Ovf_Un:
                        stack.Sub_Ovf_Un(this, code.Operand);
                        break;
                    case Code.Endfinally:
                        stack.Endfinally(this, code.Operand);
                        break;
                    case Code.Stind_I:
                        stack.Stind_I(this, code.Operand);
                        break;
                    case Code.Arglist:
                        stack.Arglist(this, code.Operand);
                        break;

                    case Code.Starg:
                        stack.Starg(this, code.Operand);
                        break;
                    case Code.Localloc:
                        stack.Localloc(this, code.Operand);
                        break;
                    case Code.Endfilter:
                        stack.Endfilter(this, code.Operand);
                        break;
                    case Code.Unaligned:
                        stack.Unaligned(this, code.Operand);
                        break;
                    case Code.Volatile:
                        stack.Volatile(this, code.Operand);
                        break;
                    case Code.Tail:
                        stack.Tail(this, code.Operand);
                        break;
                    case Code.Initobj:
                        stack.Initobj(this, this.GetType(code.Operand));
                        break;
                    case Code.Cpblk:
                        stack.Cpblk(this, code.Operand);
                        break;
                    case Code.Initblk:
                        stack.Initblk(this, code.Operand);
                        break;
                    case Code.No:
                        stack.No(this, code.Operand);
                        break;
                    case Code.Rethrow:
                        stack.Rethrow(this, code.Operand);
                        break;
                    case Code.Sizeof:
                        stack.Sizeof(this, code.Operand);
                        break;
                    case Code.Refanytype:
                        stack.Refanytype(this, code.Operand);
                        break;
                    case Code.Readonly:
                        stack.Readonly(this, code.Operand);
                        break;
                    default:
                        throw new Exception("未实现的OpCode:" + code.OpCode.Code);
                }
            }

        }
    }
}
