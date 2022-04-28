using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter.RegisterVM;
using ILRuntime.Runtime.Debugger.Protocol;
#if ILRUNTIME_ENABLE_ROSYLN
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#endif
using System.Reflection;

namespace ILRuntime.Runtime.Debugger
{
    public class DebugService
    {
        BreakPointContext curBreakpoint;
        DebuggerServer server;
        Runtime.Enviorment.AppDomain domain;
        Dictionary<int, LinkedList<BreakpointInfo>> activeBreakpoints = new Dictionary<int, LinkedList<BreakpointInfo>>();
        Dictionary<int, BreakpointInfo> breakpointMapping = new Dictionary<int, BreakpointInfo>();
        Queue<Tuple<int, int, VariableReference>> pendingReferences = new Queue<Tuple<int, int, VariableReference>>();
        Queue<Tuple<int, int, VariableReference>> pendingEnuming = new Queue<Tuple<int, int, VariableReference>>();
        Queue<Tuple<int, int, VariableReference>> pendingIndexing = new Queue<Tuple<int, int, VariableReference>>();
        AutoResetEvent evt = new AutoResetEvent(false);
        string breakpointParseCode = "void Method() {{ ({0}) }}";  // ()是因为a?b:c会被解析为可空类型a的类型变量声明，加小括号才会解析为条件表达式
        public UsingInfo[] UsingInfosContext { get; set; }

        public Action<string> OnBreakPoint;
        public Action<string> OnILRuntimeException;

        public Enviorment.AppDomain AppDomain { get { return domain; } }

        public AutoResetEvent BlockEvent { get { return evt; } }

        public bool IsDebuggerAttached
        {
            get
            {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                return (server != null && server.IsAttached);
#else
                return false;
#endif
            }
        }

        public DebugService(Runtime.Enviorment.AppDomain domain)
        {
            this.domain = domain;
        }

        /// <summary>
        /// Start Debugger Server
        /// </summary>
        /// <param name="port">Port to listen on. default value is 56000</param>
        /// <param name="boardcastDebuggerInfo">true表示使用udp在参数port表示的端口上广播本地调试器信息，调试器使用动态端口。false表示本地调试器使用参数port表示的固定端口。默认值为true</param>
        /// <returns>错误描述，null表示没有错误</returns>
        public string StartDebugService(int port = 56000, bool boardcastDebuggerInfo = true)
        {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            server = new Debugger.DebuggerServer(this);
            server.Port = port;
            return server.Start(boardcastDebuggerInfo);
#else
            return null;
#endif
        }

        /// <summary>
        /// Stop Debugger Server
        /// </summary>
        public void StopDebugService()
        {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            if (server != null)
                server.Stop();
            server = null;
#endif
        }

        /// <summary>
        /// 中断运行
        /// </summary>
        /// <param name="intpreter"></param>
        /// <param name="ex"></param>
        /// <returns>如果挂的有调试器则返回true</returns>
        internal bool Break(ILIntepreter intpreter, Exception ex = null)
        {
            BreakPointContext ctx = new BreakPointContext();
            ctx.Interpreter = intpreter;
            ctx.Exception = ex;

            curBreakpoint = ctx;

            if (OnBreakPoint != null)
            {
                OnBreakPoint(ctx.DumpContext());
                return true;
            }
            return false;
        }

        string GetInstructionDocument(Mono.Cecil.Cil.Instruction ins, Mono.Cecil.MethodDefinition md)
        {
            if (ins != null)
            {
                var seq = FindSequencePoint(ins, md.DebugInformation.GetSequencePointMapping());
                if (seq != null)
                {
                    string path = seq.Document.Url.Replace("\\", "/");
                    return string.Format("(at {0}:{1})", path, seq.StartLine);
                }
            }
            return null;
        }

        public string GetStackTrace(ILIntepreter intepreper)
        {
            StringBuilder sb = new StringBuilder();
            ILRuntime.CLR.Method.ILMethod m;
            StackFrame[] frames = intepreper.Stack.Frames.ToArray();
            Mono.Cecil.Cil.Instruction ins = null;
            RegisterVMSymbol vmSymbol;
            if (frames[0].Address != null)
            {
                if (frames[0].IsRegister)
                {
                    frames[0].Method.RegisterVMSymbols.TryGetValue(frames[0].Address.Value, out vmSymbol);
                    ins = vmSymbol.Instruction;
                    sb.AppendLine(string.Format("{0}(JIT_{1:0000}:{2})", ins, frames[0].Address.Value, frames[0].Method.BodyRegister[frames[0].Address.Value]));
                }
                else
                {
                    ins = frames[0].Method.Definition.Body.Instructions[frames[0].Address.Value];
                    sb.AppendLine(ins.ToString());
                }
            }
            for (int i = 0; i < frames.Length; i++)
            {
                var f = frames[i];
                m = f.Method;
                string document = "";
                if (f.IsRegister)
                {
                    if (f.Address != null)
                    {
                        if (f.Method.RegisterVMSymbols.TryGetValue(f.Address.Value, out vmSymbol))
                        {
                            RegisterVMSymbolLink link = null;
                            do
                            {
                                if (link != null)
                                    vmSymbol = link.Value;
                                ins = vmSymbol.Instruction;
                                var md = vmSymbol.Method.Definition;
                                document = GetInstructionDocument(ins, md);
                                sb.AppendFormat("at {0} {1}\r\n", vmSymbol.Method, document);
                                link = vmSymbol.ParentSymbol;
                            }
                            while (link != null);
                        }
                        else
                            sb.AppendFormat("at {0} {1}\r\n", m, document);
                    }
                    else
                        sb.AppendFormat("at {0} {1}\r\n", m, document);
                }
                else
                {
                    if (f.Address != null)
                    {
                        ins = m.Definition.Body.Instructions[f.Address.Value];
                        var md = m.Definition;

                        document = GetInstructionDocument(ins, md);
                    }
                    sb.AppendFormat("at {0} {1}\r\n", m, document);
                }
            }
            return sb.ToString();
        }

        public unsafe string GetThisInfo(ILIntepreter intepreter)
        {
            var topFrame = intepreter.Stack.Frames.Peek();
            var arg = Minus(topFrame.LocalVarPointer, topFrame.Method.ParameterCount);
            if (topFrame.Method.HasThis)
                arg--;
            if (arg->ObjectType == ObjectTypes.StackObjectReference)
            {
                var addr = *(long*)&arg->Value;
                arg = (StackObject*)addr;
            }
            ILTypeInstance instance = arg->ObjectType != ObjectTypes.Null ? intepreter.Stack.ManagedStack[arg->Value] as ILTypeInstance : null;
            if (instance == null)
                return "null";
            var fields = instance.Type.TypeDefinition.Fields;
            int idx = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fields.Count; i++)
            {
                try
                {
                    var f = fields[i];
                    if (f.IsStatic)
                        continue;
                    var field = instance.Fields[idx];
                    var v = StackObject.ToObject(&field, intepreter.AppDomain, instance.ManagedObjects);
                    if (v == null)
                        v = "null";
                    string name = f.Name;
                    sb.AppendFormat("{0} {1} = {2}", f.FieldType.Name, name, v);
                    if ((idx % 3 == 0 && idx != 0) || idx == instance.Fields.Length - 1)
                        sb.AppendLine();
                    else
                        sb.Append(", ");
                    idx++;
                }
                catch
                {

                }
            }
            return sb.ToString();
        }

        public unsafe string GetLocalVariableInfo(ILIntepreter intepreter)
        {
            StackFrame topFrame = intepreter.Stack.Frames.Peek();
            var m = topFrame.Method;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m.LocalVariableCount; i++)
            {
                try
                {
                    var lv = m.Definition.Body.Variables[i];
                    var val = Add(topFrame.LocalVarPointer, i);
                    var v = StackObject.ToObject(val, intepreter.AppDomain, intepreter.Stack.ManagedStack);
                    if (v == null)
                        v = "null";
                    string vName = null;
                    m.Definition.DebugInformation.TryGetName(lv, out vName);
                    string name = string.IsNullOrEmpty(vName) ? "v" + lv.Index : vName;
                    sb.AppendFormat("{0} {1} = {2}", lv.VariableType.Name, name, v);
                    if ((i % 3 == 0 && i != 0) || i == m.LocalVariableCount - 1)
                        sb.AppendLine();
                    else
                        sb.Append(", ");
                }
                catch
                {

                }
            }
            return sb.ToString();
        }

        internal static Mono.Cecil.Cil.SequencePoint FindSequencePoint(Mono.Cecil.Cil.Instruction ins, IDictionary<Mono.Cecil.Cil.Instruction, Mono.Cecil.Cil.SequencePoint> seqMapping)
        {
            Mono.Cecil.Cil.Instruction cur = ins;
            Mono.Cecil.Cil.SequencePoint sp;
            while (!seqMapping.TryGetValue(cur, out sp) && cur.Previous != null)
                cur = cur.Previous;

            return sp;
        }

        unsafe static StackObject* Add(StackObject* a, int b)
        {
            return (StackObject*)((long)a + sizeof(StackObject) * b);
        }

        unsafe static StackObject* Minus(StackObject* a, int b)
        {
            return (StackObject*)((long)a - sizeof(StackObject) * b);
        }

        internal void NotifyModuleLoaded(string moduleName)
        {
            if (server != null && server.IsAttached)
                server.NotifyModuleLoaded(moduleName);
        }

        internal void SetBreakPoint(int methodHash, int bpHash, int startLine, bool enabled, BreakpointCondition breakpointCondition, UsingInfo[] usingInfos)
        {
            lock (activeBreakpoints)
            {
                LinkedList<BreakpointInfo> lst;
                if (!activeBreakpoints.TryGetValue(methodHash, out lst))
                {
                    lst = new LinkedList<Debugger.BreakpointInfo>();
                    activeBreakpoints[methodHash] = lst;
                }

                BreakpointInfo bpInfo = new BreakpointInfo();
                bpInfo.BreakpointHashCode = bpHash;
                bpInfo.MethodHashCode = methodHash;
                bpInfo.StartLine = startLine;
                bpInfo.Enabled = enabled;
                ParseBreakpointCondition(bpInfo, breakpointCondition);
                bpInfo.UsingInfos = usingInfos;

                lst.AddLast(bpInfo);
                breakpointMapping[bpHash] = bpInfo;
            }
        }

        private bool ParseBreakpointCondition(BreakpointInfo bpInfo, BreakpointCondition condition)
        {
#if ILRUNTIME_ENABLE_ROSYLN
            bpInfo.Condition = new BreakpointConditionDetails { Style = condition.Style, Expression = condition.Expression };

            if (condition.Style == BreakpointConditionStyle.None)
                return true;

            var codeWithExpression = string.Format(breakpointParseCode, condition.Expression);
            var syntaxTree = CSharpSyntaxTree.ParseText(codeWithExpression);
            syntaxTree.TryGetRoot(out var root);
            var methodDeclarationSyntax = root.ChildNodes().First() as MethodDeclarationSyntax;
            var expressionStatementSyntax = methodDeclarationSyntax.Body.Statements[0] as ExpressionStatementSyntax;
            if (expressionStatementSyntax == null)
            {
                bpInfo.Condition.ExpressionError = true;
                return false;
            }
            bpInfo.Condition.ExpressionSyntax = expressionStatementSyntax.Expression;
#endif
            return true;
        }

        internal void SetBreakpointEnabled(int bpHash, bool enabled)
        {
            lock (activeBreakpoints)
            {
                BreakpointInfo bpInfo;
                if (breakpointMapping.TryGetValue(bpHash, out bpInfo))
                    bpInfo.Enabled = enabled;
            }
        }

        internal void SetBreakpointCondition(int bpHash, BreakpointConditionStyle style, string expression)
        {
            lock (activeBreakpoints)
            {
                BreakpointInfo bpInfo;
                if (breakpointMapping.TryGetValue(bpHash, out bpInfo))
                    ParseBreakpointCondition(bpInfo, new BreakpointCondition { Style = style, Expression = expression });
            }
        }

        internal void DeleteBreakpoint(int bpHash)
        {
            lock (activeBreakpoints)
            {
                BreakpointInfo bpInfo;
                if (breakpointMapping.TryGetValue(bpHash, out bpInfo))
                {
                    LinkedList<BreakpointInfo> lst;
                    if (activeBreakpoints.TryGetValue(bpInfo.MethodHashCode, out lst))
                    {
                        lst.Remove(bpInfo);
                    }
                    breakpointMapping.Remove(bpHash);
                }
            }
        }

        internal void ExecuteThread(int threadHash)
        {
            lock (AppDomain.FreeIntepreters)
            {
                foreach (var i in AppDomain.Intepreters)
                {
                    //We should resume all threads on execute
                    i.Value.ClearDebugState();
                    i.Value.Resume();
                }
            }
        }

        internal unsafe void StepThread(int threadHash, StepTypes type)
        {
            lock (AppDomain.FreeIntepreters)
            {
                ILIntepreter intp;
                if (AppDomain.Intepreters.TryGetValue(threadHash, out intp))
                {
                    intp.ClearDebugState();
                    intp.CurrentStepType = type;
                    intp.LastStepInstructionIndex = intp.Stack.Frames.Count > 0 ? intp.Stack.Frames.Peek().Address.Value : 0;
                    intp.LastStepFrameBase = intp.Stack.Frames.Count > 0 ? ResolveCurrentFrameBasePointer(intp) : (StackObject*)0;

                    intp.Resume();
                }
            }
        }

        unsafe StackObject* ResolveCurrentFrameBasePointer(ILIntepreter intp, ILMethod method = null, int ip = -1)
        {
            StackObject* basePointer = intp.Stack.Frames.Peek().BasePointer;
            if (method == null)
                method = intp.Stack.Frames.Peek().Method;
            if (ip < 0)
                ip = intp.Stack.Frames.Peek().Address.Value;
            if (intp.Stack.Frames.Peek().IsRegister)
            {
                basePointer = intp.Stack.Frames.Peek().LocalVarPointer;
                RegisterVMSymbol vmSymbol;
                if (method.RegisterVMSymbols.TryGetValue(ip, out vmSymbol))
                {
                    var paramCnt = method.HasThis ? method.ParameterCount + 1 : method.ParameterCount;
                    var frameBase = basePointer - paramCnt;
                    int registerCnt = vmSymbol.Method.StackRegisterCount + vmSymbol.Method.LocalVariableCount;
                    if (method.HasThis)
                        frameBase--;
                    var curParamCnt = vmSymbol.Method.HasThis ? vmSymbol.Method.ParameterCount + 1 : vmSymbol.Method.ParameterCount;

                    if (vmSymbol.ParentSymbol != null)
                    {
                        basePointer = frameBase + vmSymbol.ParentSymbol.BaseRegisterIndex;
                    }
                    else
                    {
                        registerCnt -= vmSymbol.Method.StackRegisterCount;
                        basePointer = frameBase;
                    }
                    basePointer = basePointer + curParamCnt + registerCnt;
                }
            }
            return basePointer;
        }

        unsafe internal void CheckShouldBreak(ILMethod method, ILIntepreter intp, int ip)
        {
            if (server != null && server.IsAttached)
            {
                RegisterVMSymbol vmSymbol;
                Mono.Cecil.Cil.Instruction ins = null;
                Mono.Cecil.MethodDefinition md = null;
                ILMethod m = method;
                if (intp.Stack.Frames.Peek().IsRegister)
                {
                    if (!method.IsRegisterVMSymbolFixed)
                        method.FixRegisterVMSymbol();
                    if (method.RegisterVMSymbols.TryGetValue(ip, out vmSymbol))
                    {
                        ins = vmSymbol.Instruction;
                        m = vmSymbol.Method;
                        md = vmSymbol.Method.Definition;

                    }
                }
                else
                {
                    md = method.Definition;
                    ins = md.Body.Instructions[ip];
                }
                StackObject* basePointer = ResolveCurrentFrameBasePointer(intp, method, ip);

                int methodHash = m.GetHashCode();
                BreakpointInfo[] lst = null;

                lock (activeBreakpoints)
                {
                    LinkedList<BreakpointInfo> bps;
                    if (activeBreakpoints.TryGetValue(methodHash, out bps))
                        lst = bps.ToArray();
                }
                if (ins == null)
                    return;
                if (lst != null)
                {
                    var sp = md.DebugInformation.GetSequencePoint(ins);
                    if (sp != null)
                    {
                        foreach (var i in lst)
                        {
                            StackFrameInfo[] stackFrameInfos = null;
                            string error = "";
                            if ((i.StartLine + 1) == sp.StartLine && i.Enabled && i.CheckCondition(this, intp, ref stackFrameInfos, ref error))
                            {
                                DoBreak(intp, i.BreakpointHashCode, false, stackFrameInfos, error);
                                return;
                            }
                        }
                    }
                }

                if (intp.CurrentStepType != StepTypes.None)
                {
                    var sp = md.DebugInformation.GetSequencePoint(ins);
                    if (sp != null && IsSequenceValid(sp))
                    {
                        switch (intp.CurrentStepType)
                        {
                            case StepTypes.Into:
                                DoBreak(intp, 0, true);
                                break;
                            case StepTypes.Over:
                                if (basePointer <= intp.LastStepFrameBase && ip != intp.LastStepInstructionIndex)
                                {
                                    DoBreak(intp, 0, true);
                                }
                                break;
                            case StepTypes.Out:
                                {
                                    if (intp.Stack.Frames.Count > 0 && basePointer < intp.LastStepFrameBase)
                                    {
                                        DoBreak(intp, 0, true);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        bool IsSequenceValid(Mono.Cecil.Cil.SequencePoint sp)
        {
            return sp.StartLine != sp.EndLine || sp.StartColumn != sp.EndColumn;
        }

        void DoBreak(ILIntepreter intp, int bpHash, bool isStep, StackFrameInfo[] stackFrameInfos = null, string error = null)
        {
            var arr = AppDomain.Intepreters.ToArray();
            KeyValuePair<int, StackFrameInfo[]>[] frames = new KeyValuePair<int, StackFrameInfo[]>[arr.Length];
            frames[0] = new KeyValuePair<int, StackFrameInfo[]>(intp.GetHashCode(), stackFrameInfos == null ? GetStackFrameInfo(intp) : stackFrameInfos);
            int idx = 1;
            foreach (var j in arr)
            {
                if (j.Value != intp)
                {
                    j.Value.ShouldBreak = true;
                    try
                    {
                        frames[idx++] = new KeyValuePair<int, Debugger.StackFrameInfo[]>(j.Value.GetHashCode(), GetStackFrameInfo(j.Value));
                    }
                    catch
                    {
                        frames[idx++] = new KeyValuePair<int, Debugger.StackFrameInfo[]>(j.Value.GetHashCode(), new StackFrameInfo[0]);
                    }
                }
            }
            if (!isStep)
                server.SendSCBreakpointHit(intp.GetHashCode(), bpHash, frames, error);
            else
                server.SendSCStepComplete(intp.GetHashCode(), frames);
            //Breakpoint hit
            intp.Break();
        }

        unsafe void InitializeStackFrameInfo(ILIntepreter intp, StackFrame f, List<StackFrameInfo> frameInfos)
        {
            Mono.Cecil.Cil.Instruction ins = null;
            var m = f.Method;
            int argCnt = m.HasThis ? m.ParameterCount + 1 : m.ParameterCount;
            StackObject* frameBasePointer = Minus(f.LocalVarPointer, argCnt);
            if (f.Address != null)
            {
                if (f.IsRegister)
                {
                    RegisterVMSymbol vmSymbol;
                    if (m.RegisterVMSymbols.TryGetValue(f.Address.Value, out vmSymbol))
                    {
                        RegisterVMSymbolLink link = null;
                        StackObject* basePointer;
                        do
                        {
                            if (link != null)
                            {
                                vmSymbol = link.Value;
                            }
                            ins = vmSymbol.Instruction;
                            m = vmSymbol.Method;
                            if (vmSymbol.ParentSymbol != null)
                            {
                                basePointer = Add(frameBasePointer, vmSymbol.ParentSymbol.BaseRegisterIndex);
                            }
                            else
                            {
                                basePointer = frameBasePointer;
                            }
                            var info = CreateStackFrameInfo(m, ins);
                            AddStackFrameInfoVariables(intp, info, m, basePointer);
                            frameInfos.Add(info);
                            link = vmSymbol.ParentSymbol;
                        }
                        while (link != null);
                    }
                    else
                    {
                        var info = CreateStackFrameInfo(m, null);
                        AddStackFrameInfoVariables(intp, info, m, frameBasePointer);
                        frameInfos.Add(info);
                    }
                }
                else
                {
                    ins = m.Definition.Body.Instructions[f.Address.Value];
                    var info = CreateStackFrameInfo(m, ins);
                    AddStackFrameInfoVariables(intp, info, m, frameBasePointer);
                    frameInfos.Add(info);
                }
            }
            else
            {
                var info = CreateStackFrameInfo(m, null);
                AddStackFrameInfoVariables(intp, info, m, frameBasePointer);
                frameInfos.Add(info);
            }
        }

        StackFrameInfo CreateStackFrameInfo(ILMethod m, Mono.Cecil.Cil.Instruction ins)
        {
            Mono.Cecil.MethodDefinition md = m.Definition;
            StackFrameInfo info = new StackFrameInfo();
            info.MethodName = m.ToString();
            if (ins != null)
            {
                var seq = FindSequencePoint(ins, md.DebugInformation.GetSequencePointMapping());
                if (seq != null)
                {
                    info.DocumentName = seq.Document.Url;
                    info.StartLine = seq.StartLine - 1;
                    info.StartColumn = seq.StartColumn - 1;
                    info.EndLine = seq.EndLine - 1;
                    info.EndColumn = seq.EndColumn - 1;
                }
            }
            return info;
        }

        unsafe void AddStackFrameInfoVariables(ILIntepreter intp, StackFrameInfo info, ILMethod m, StackObject* basePointer)
        {
            int argumentCount = m.ParameterCount;
            if (m.HasThis)
                argumentCount++;
            info.ArgumentCount = argumentCount;
            info.LocalVariables = new VariableInfo[argumentCount + m.LocalVariableCount];
            for (int i = 0; i < argumentCount; i++)
            {
                int argIdx = m.HasThis ? i - 1 : i;
                var arg = basePointer;
                string name = null;
                object v = null;
                string typeName = null;
                var val = Add(arg, i);
                v = StackObject.ToObject(val, intp.AppDomain, intp.Stack.ManagedStack);
                CLR.TypeSystem.IType vType;
                if (argIdx >= 0)
                {
                    var lv = m.Definition.Parameters[argIdx];
                    name = string.IsNullOrEmpty(lv.Name) ? "arg" + lv.Index : lv.Name;
                    typeName = lv.ParameterType.FullName;
                    vType = m.Parameters[argIdx];
                }
                else
                {
                    name = "this";
                    typeName = m.DeclearingType.FullName;
                    vType = m.DeclearingType;
                }

                v = vType.TypeForCLR.CheckCLRTypes(v);
                VariableInfo vinfo = VariableInfo.FromObject(v);
                vinfo.Address = (long)val;
                vinfo.Name = name;
                vinfo.TypeName = typeName;
                vinfo.Expandable = GetValueExpandable(intp, val, intp.Stack.ManagedStack);
                vinfo.ValueObjType = vType.ReflectionType;

                info.LocalVariables[i] = vinfo;
            }
            for (int i = argumentCount; i < info.LocalVariables.Length; i++)
            {
                var locIdx = i - argumentCount;
                var lv = m.Definition.Body.Variables[locIdx];
                var val = Add(basePointer, argumentCount + locIdx);
                var v = StackObject.ToObject(val, intp.AppDomain, intp.Stack.ManagedStack);
                var type = intp.AppDomain.GetType(lv.VariableType, m.DeclearingType, m);
                string vName = null;
                m.Definition.DebugInformation.TryGetName(lv, out vName);
                string name = string.IsNullOrEmpty(vName) ? "v" + lv.Index : vName;
                v = type.TypeForCLR.CheckCLRTypes(v);
                VariableInfo vinfo = VariableInfo.FromObject(v);
                vinfo.Address = (long)val;
                vinfo.Name = name;
                vinfo.TypeName = lv.VariableType.FullName;
                vinfo.Expandable = GetValueExpandable(intp, val, intp.Stack.ManagedStack);
                vinfo.ValueObjType = type.ReflectionType;
                info.LocalVariables[i] = vinfo;
            }
        }

        internal unsafe StackFrameInfo[] GetStackFrameInfo(ILIntepreter intp)
        {
            StackFrame[] frames = intp.Stack.Frames.ToArray();
            List<StackFrameInfo> frameInfos = new List<StackFrameInfo>();

            for (int j = 0; j < frames.Length; j++)
            {
                InitializeStackFrameInfo(intp, frames[j], frameInfos);
            }
            return frameInfos.ToArray();
        }

        internal unsafe VariableInfo[] EnumChildren(int threadHashCode, int frameIndex, VariableReference parent)
        {
            ILIntepreter intepreter;
            if (AppDomain.Intepreters.TryGetValue(threadHashCode, out intepreter))
            {
#if DEBUG && !NO_PROFILER
                if (domain.IsNotUnityMainThread())
                {
                    lock (pendingEnuming)
                    {
                        pendingEnuming.Enqueue(new Tuple<int, int, VariableReference>(threadHashCode, frameIndex, parent));
                    }
                    return null;
                }
#endif
                object obj;
                var info = ResolveVariable(threadHashCode, frameIndex, parent, out obj);
                if (obj != null)
                {
                    if (obj is Array)
                    {
                        return EnumArray((Array)obj, intepreter);
                    }
                    else if (obj is IList)
                    {
                        return EnumList((IList)obj, intepreter);
                    }
                    else if (obj is IDictionary)
                    {
                        return EnumDictionary((IDictionary)obj, intepreter);
                    }
                    else if (obj is ILTypeInstance)
                    {
                        return EnumObject(((ILTypeInstance)obj).CLRInstance, ((ILTypeInstance)obj).Type.ReflectionType);
                        //return EnumILTypeInstance((ILTypeInstance)obj, intepreter);
                    }
                    else if (obj is ILRuntime.Runtime.Enviorment.CrossBindingAdaptorType)
                    {
                        return EnumObject(obj, ((Enviorment.CrossBindingAdaptorType)obj).ILInstance.Type.ReflectionType);
                        //return EnumILTypeInstance(((Enviorment.CrossBindingAdaptorType)obj).ILInstance, intepreter);
                    }
                    else
                    {
                        return EnumCLRObject(obj, intepreter);
                    }
                }
                else
                    return new VariableInfo[] { VariableInfo.NullReferenceExeption };
            }
            else
                return new VariableInfo[] { VariableInfo.NullReferenceExeption };
        }

        VariableInfo[] EnumArray(Array arr, ILIntepreter intepreter)
        {
            VariableInfo[] res = new VariableInfo[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                try
                {
                    var obj = arr.GetValue(i);

                    VariableInfo info = VariableInfo.FromObject(obj, true);
                    info.Name = string.Format("[{0}]", i);
                    info.Offset = i;
                    info.Type = VariableTypes.IndexAccess;
                    res[i] = info;
                }
                catch (Exception ex)
                {
                    var info = VariableInfo.GetException(ex);
                    info.Name = string.Format("[{0}]", i);
                    res[i] = info;
                }
            }

            return res;
        }

        VariableInfo[] EnumList(IList lst, ILIntepreter intepreter)
        {
            VariableInfo[] res = new VariableInfo[lst.Count];

            for (int i = 0; i < lst.Count; i++)
            {
                try
                {
                    var obj = lst[i];

                    VariableInfo info = VariableInfo.FromObject(obj, true);
                    info.Name = string.Format("[{0}]", i);
                    info.Offset = i;
                    info.Type = VariableTypes.IndexAccess;

                    res[i] = info;
                }
                catch (Exception ex)
                {
                    var info = VariableInfo.GetException(ex);
                    info.Name = string.Format("[{0}]", i);
                    res[i] = info;
                }
            }

            return res;
        }

        VariableInfo[] EnumDictionary(IDictionary lst, ILIntepreter intepreter)
        {
            VariableInfo[] res = new VariableInfo[lst.Count];

            var keys = GetArray(lst.Keys);
            var values = GetArray(lst.Values);
            for (int i = 0; i < lst.Count; i++)
            {
                try
                {
                    var obj = values[i];
                    VariableInfo info = VariableInfo.FromObject(obj, true);
                    info.Name = string.Format("[{0}]", i);
                    info.Type = VariableTypes.IndexAccess;
                    info.Offset = i;
                    info.Value = string.Format("{0},{1}", SafeToString(keys[i]), SafeToString(values[i]));
                    info.Expandable = true;
                    res[i] = info;
                }
                catch (Exception ex)
                {
                    var info = VariableInfo.GetException(ex);
                    info.Name = string.Format("[{0}]", i);
                    res[i] = info;
                }
            }
            return res;
        }

        string SafeToString(object obj)
        {
            if (obj != null)
                return obj.ToString();
            else
                return "null";
        }
        object[] GetArray(ICollection lst)
        {
            object[] res = new object[lst.Count];
            int idx = 0;
            foreach (var i in lst)
            {
                res[idx++] = i;
            }
            return res;
        }

        VariableInfo[] EnumILTypeInstance(ILTypeInstance obj, ILIntepreter intepreter)
        {
            return EnumObject(obj, obj.Type.ReflectionType);
        }

        VariableInfo[] EnumCLRObject(object obj, ILIntepreter intepreter)
        {
            return EnumObject(obj, obj.GetType());
        }

        VariableInfo[] EnumObject(object obj, Type t)
        {
            List<VariableInfo> lst = new List<VariableInfo>();
            foreach (var i in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            {
                try
                {
                    if (i.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Length > 0)
                        continue;
                    var val = i.GetValue(obj);
                    VariableInfo info = VariableInfo.FromObject(val);
                    info.Type = VariableTypes.FieldReference;
                    info.TypeName = i.FieldType.FullName;
                    info.Name = i.Name;
                    info.Expandable = !i.FieldType.IsPrimitive && val != null;
                    info.IsPrivate = i.IsPrivate;
                    info.IsProtected = i.IsFamily;

                    lst.Add(info);
                }
                catch (Exception ex)
                {
                    var info = VariableInfo.GetException(ex);
                    info.Name = i.Name;
                    lst.Add(info);
                }
            }

            foreach (var i in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            {
                try
                {
                    if (i.GetIndexParameters().Length > 0)
                        continue;
                    if (i.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
                        continue;
                    var val = i.GetValue(obj, null);
                    VariableInfo info = VariableInfo.FromObject(val);
                    info.Type = VariableTypes.PropertyReference;
                    info.TypeName = i.PropertyType.FullName;
                    info.Name = i.Name;
                    info.Expandable = !i.PropertyType.IsPrimitive && val != null;
                    info.IsPrivate = i.GetGetMethod(true).IsPrivate;
                    info.IsProtected = i.GetGetMethod(true).IsFamily;

                    lst.Add(info);
                }
                catch (Exception ex)
                {
                    var info = VariableInfo.GetException(ex);
                    info.Name = i.Name;
                    lst.Add(info);
                }
            }

            return lst.ToArray();
        }

        internal unsafe VariableInfo ResolveIndexAccess(int threadHashCode, int frameIndex, VariableReference indexAccess, out object res)
        {
            ILIntepreter intepreter;
            res = null;
            if (AppDomain.Intepreters.TryGetValue(threadHashCode, out intepreter))
            {
#if DEBUG && !NO_PROFILER
                if (domain.IsNotUnityMainThread())
                {
                    lock (pendingIndexing)
                    {
                        pendingIndexing.Enqueue(new Tuple<int, int, VariableReference>(threadHashCode, frameIndex, indexAccess));
                    }
                    res = null;
                    return new VariableInfo() { Type = VariableTypes.Pending };
                }
#endif
                var body = indexAccess.Parent;
                var idx = indexAccess.Parameters[0];
                object obj;
                var info = ResolveVariable(threadHashCode, frameIndex, body, out obj);
                var objType = info.ValueObjType;
                if (obj != null)
                {
                    object idxObj;
                    info = ResolveVariable(threadHashCode, frameIndex, idx, out idxObj);
                    if (obj is Array)
                    {
                        res = ((Array)obj).GetValue((int)idxObj);
                        info = VariableInfo.FromObject(res);
                        info.Type = VariableTypes.IndexAccess;
                        info.TypeName = obj.GetType().GetElementType().FullName;
                        info.Expandable = res != null && !obj.GetType().GetElementType().IsPrimitive;
                        info.ValueObjType = objType.GetElementType();

                        return info;
                    }
                    else
                    {
                        //if(obj is ILTypeInstance)
                        //{
                        //    var m = ((ILTypeInstance)obj).Type.GetMethod("get_Item");
                        //    if (m != null)
                        //    {
                        //        res = intepreter.AppDomain.Invoke(m, obj, idxObj);
                        //        info = VariableInfo.FromObject(res);
                        //        info.Type = VariableTypes.IndexAccess;
                        //        info.TypeName = m.ReturnType.FullName;
                        //        info.Expandable = res != null && !m.ReturnType.IsPrimitive;

                        //        return info;
                        //    }
                        //    else
                        //        return VariableInfo.NullReferenceExeption;
                        //}
                        //else
                        //{
                        //if(obj is ILRuntime.Runtime.Enviorment.CrossBindingAdaptorType)
                        //{
                        //    throw new NotImplementedException();
                        //}
                        //else
                        //{
                        //if (obj is IDictionary && idxObj is int)
                        //{
                        //    IDictionary dic = (IDictionary)obj;
                        //    var keys = GetArray(dic.Keys);
                        //    if (keys[0].GetType() != typeof(int))
                        //    {
                        //        int index = (int)idxObj;
                        //        var values = GetArray(dic.Values);
                        //        var t = typeof(KeyValuePair<,>).MakeGenericType(keys[index].GetType(), values[index].GetType());
                        //        var ctor = t.GetConstructor(new Type[] { keys[index].GetType(), values[index].GetType() });
                        //        res = ctor.Invoke(new object[] { keys[index], values[index] });
                        //        info = VariableInfo.FromObject(res);
                        //        info.Type = VariableTypes.IndexAccess;
                        //        info.Offset = index;
                        //        info.TypeName = t.FullName;
                        //        info.Expandable = true;
                        //        info.ValueObjType = t;

                        //        return info;
                        //    }
                        //}

                        var pi = GetOverrideIndexer(objType, info.ValueObjType);
                        //var pi = obj.GetType().GetProperty("Item");
                        if (pi != null)
                        {
                            res = pi.GetValue(obj, new object[] { idxObj });
                            info = VariableInfo.FromObject(res);
                            info.Type = VariableTypes.IndexAccess;
                            info.TypeName = pi.PropertyType.FullName;
                            info.Expandable = res != null && !pi.PropertyType.IsPrimitive;
                            info.ValueObjType = pi.PropertyType;
                            return info;
                        }
                        else
                            return VariableInfo.GetError(string.Format("无法将带[] 的索引应用于“{0}”类型的表达式", objType.FullName));
                        //}
                        //}
                    }
                }
                else
                {
                    if (indexAccess.Conditional)
                    {
                        if (objType.IsArray)
                        {
                            info.ValueObjType = objType.GetElementType();
                            info.TypeName = info.ValueObjType.FullName;
                            return info;
                        }
                        else
                        {
                            object idxObj;
                            info = ResolveVariable(threadHashCode, frameIndex, idx, out idxObj);
                            var pi = GetOverrideIndexer(objType, info.ValueObjType);
                            if (pi != null)
                            {
                                info.ValueObjType = pi.PropertyType;
                                info.TypeName = pi.PropertyType.FullName;
                                return info;
                            }
                            else
                                return VariableInfo.GetError(string.Format("无法将带[] 的索引应用于“{0}”类型的表达式", objType.FullName));
                        }
                    }
                    return VariableInfo.NullReferenceExeptionWithName(body.FullName);
                }
            }
            else
                return VariableInfo.NullReferenceExeption;
        }

        private PropertyInfo GetOverrideIndexer(Type bodyType, Type indexType)
        {
            var checkedTypes = new Type[1] { indexType };
            foreach (var property in bodyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var indexParameters = property.GetIndexParameters();
                if (CheckParameters(indexParameters, checkedTypes, true))
                    return property;
            }
            return null;
        }

        internal void ResolvePendingRequests()
        {
            lock (pendingReferences)
            {
                while (pendingReferences.Count > 0)
                {
                    VariableInfo info;
                    var r = pendingReferences.Dequeue();
                    try
                    {
                        object res;
                        info = ResolveVariable(r.Item1, r.Item2, r.Item3, out res);
                    }
                    catch (Exception ex)
                    {
                        info = VariableInfo.GetException(ex);
                    }
                    server.SendSCResolveVariableResult(info);
                }
            }
            lock (pendingEnuming)
            {
                while (pendingEnuming.Count > 0)
                {
                    VariableInfo[] info;
                    var r = pendingEnuming.Dequeue();
                    try
                    {
                        info = EnumChildren(r.Item1, r.Item2, r.Item3);
                    }
                    catch (Exception ex)
                    {
                        info = new VariableInfo[] { VariableInfo.GetException(ex) };
                    }
                    server.SendSCEnumChildrenResult(info);
                }
            }
            lock (pendingIndexing)
            {
                while (pendingIndexing.Count > 0)
                {
                    VariableInfo info;
                    var r = pendingIndexing.Dequeue();
                    try
                    {
                        object res;
                        info = ResolveIndexAccess(r.Item1, r.Item2, r.Item3, out res);
                    }
                    catch (Exception ex)
                    {
                        info = VariableInfo.GetException(ex);
                    }
                    server.SendSCResolveVariableResult(info);
                }
            }
        }

        public unsafe static object GetThis(ILIntepreter intepreter, int frameIndex)
        {
            ILMethod currentMethod;
            return GetThis(intepreter, frameIndex, out currentMethod);
        }

        public unsafe static object GetThis(ILIntepreter intepreter, int frameIndex, out ILMethod currentMethod)
        {
            var frame = intepreter.Stack.Frames.ToArray()[frameIndex];
            var m = frame.Method;
            currentMethod = m;
            if (m.HasThis)
            {
                var addr = Minus(frame.LocalVarPointer, m.ParameterCount + 1);
                return StackObject.ToObject(addr, intepreter.AppDomain, intepreter.Stack.ManagedStack);
            }
            return null;
        }

        private void GetVariableReferenceParameters(int threadHashCode, int frameIndex, VariableReference[] parameters, out Type[] paramterTypes, out object[] paramterObjs)
        {
            var paramterTypeList = new List<Type>();
            var paramterObjsList = new List<object>();
            foreach (var parameterReference in parameters)
            {
                object parameterValue;
                var info = ResolveVariable(threadHashCode, frameIndex, parameterReference, out parameterValue);
                paramterTypeList.Add(info == null ? null : info.ValueObjType);
                paramterObjsList.Add(parameterValue);
            }
            paramterTypes = paramterTypeList.ToArray();
            paramterObjs = paramterObjsList.ToArray();
        }

        internal unsafe VariableInfo ResolveVariable(int threadHashCode, int frameIndex, VariableReference variable, out object res)
        {
            ILIntepreter intepreter;
            res = null;
            if (AppDomain.Intepreters.TryGetValue(threadHashCode, out intepreter))
            {
                if (variable != null)
                {
#if DEBUG && !NO_PROFILER
                    if (domain.IsNotUnityMainThread())
                    {
                        lock (pendingReferences)
                        {
                            pendingReferences.Enqueue(new Tuple<int, int, VariableReference>(threadHashCode, frameIndex, variable));
                        }
                        res = null;
                        return new VariableInfo() { Type = VariableTypes.Pending };
                    }
#endif
                    switch (variable.Type)
                    {
                        case VariableTypes.Normal:
                            {
                                StackObject* ptr = (StackObject*)variable.Address;
                                object obj = StackObject.ToObject(ptr, AppDomain, intepreter.Stack.ManagedStack);
                                //if (obj != null)
                                //{
                                if (variable.ValueType != null && obj != null)
                                    obj = variable.ValueType.UnWrapper().CheckCLRTypes(obj);
                                res = obj;
                                VariableInfo info = VariableInfo.FromObject(res);
                                info.Address = variable.Address;
                                info.Name = variable.Name;
                                info.Type = VariableTypes.Normal;
                                info.TypeName = variable.ValueType == null ? "" : variable.ValueType.FullName;
                                info.ValueObjType = variable.ValueType;
                                return info;
                                //}
                                //else
                                //{
                                //    return VariableInfo.Null;
                                //}
                            }
                        case VariableTypes.FieldReference:
                        case VariableTypes.PropertyReference:
                        case VariableTypes.Invocation:
                            {
                                object obj;
                                Type[] paramterTypes = null;
                                object[] paramterObjs = null;
                                if (variable.Parent != null)
                                {
                                    var parentVariableInfo = ResolveVariable(threadHashCode, frameIndex, variable.Parent, out obj);
                                    if (parentVariableInfo.Type == VariableTypes.NotFound || parentVariableInfo.Type == VariableTypes.Error)
                                        return parentVariableInfo;
                                    if (variable.Type == VariableTypes.Invocation)
                                    {
                                        GetVariableReferenceParameters(threadHashCode, frameIndex, variable.Parameters, out paramterTypes, out paramterObjs);
                                    }
                                    if (obj != null)
                                    {
                                        return ResolveMember(obj, null, variable.Name, paramterTypes, paramterObjs, out res);
                                    }
                                    else
                                    {
                                        if (parentVariableInfo.Type == VariableTypes.Class)
                                        {
                                            return ResolveMember(null, parentVariableInfo.ValueObjType, variable.Name, paramterTypes, paramterObjs, out res);
                                        }
                                        if (variable.Conditional)
                                        {
                                            parentVariableInfo.ValueObjType = GetMemberType(parentVariableInfo.ValueObjType, variable.Name, paramterTypes); // 返回的应是成员类型。例如a为复杂类型，b为a的string类型的成员，那么a?.b返回string
                                            if (parentVariableInfo.ValueObjType == null)
                                                return VariableInfo.GetCannotFind(variable.Name);
                                            return parentVariableInfo;
                                        }
                                        return VariableInfo.NullReferenceExeptionWithName(variable.Parent.FullName);
                                    }
                                }
                                else
                                {
                                    VariableInfo result = null;
                                    var v = GetThis(intepreter, frameIndex);
                                    if (v != null)
                                    {
                                        if (variable.Type == VariableTypes.Invocation)
                                        {
                                            GetVariableReferenceParameters(threadHashCode, frameIndex, variable.Parameters, out paramterTypes, out paramterObjs);
                                        }
                                        result = ResolveMember(v, null, variable.Name, paramterTypes, paramterObjs, out res);
                                        if (result.Type == VariableTypes.NotFound)
                                        {
                                            ILTypeInstance ins = v as ILTypeInstance;
                                            if (ins != null)
                                            {
                                                var ilType = ins.Type.ReflectionType;
                                                var fields = ilType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                                foreach (var f in fields)
                                                {
                                                    if (f.Name.Contains("_this"))
                                                    {
                                                        result = ResolveMember(f.GetValue(v), null, variable.Name, null, null, out res);
                                                        if (result.Type != VariableTypes.NotFound)
                                                            return result;
                                                    }
                                                }
                                            }
                                        }
                                        //return result;
                                    }
                                    //else
                                    //{
                                    //    return VariableInfo.GetCannotFind(variable.Name);
                                    //}
                                    if (result != null && result.Type != VariableTypes.NotFound)
                                        return result;

                                    // variable.Name有可能表示一个类型
                                    CLR.TypeSystem.IType targetIType = null;
                                    string typeFullName = null;
                                    if (UsingInfosContext != null)
                                    {
                                        foreach (var usingInfo in UsingInfosContext)
                                        {
                                            typeFullName = null;
                                            if (string.IsNullOrWhiteSpace(usingInfo.Alias)) // 用每个using+类型名称 进行匹配
                                                typeFullName = usingInfo.Name + "." + variable.Name;
                                            else if (usingInfo.Alias == variable.Name) // 有别名的情况直接使用对应的名称
                                                typeFullName = usingInfo.Name;
                                            if (typeFullName != null)
                                            {
                                                var iType = AppDomain.GetType(typeFullName);
                                                if (iType != null)
                                                {
                                                    targetIType = iType;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (targetIType == null)
                                    {
                                        typeFullName = variable.Name;
                                        targetIType = AppDomain.GetType(typeFullName);
                                    }
                                    if (targetIType != null)
                                    {
                                        return new VariableInfo { Name = typeFullName, Type = VariableTypes.Class, ValueObjType = targetIType.ReflectionType };
                                    }

                                    if (result != null)
                                        return result;
                                    return VariableInfo.GetCannotFind(variable.Name);
                                }
                            }
                        case VariableTypes.IndexAccess:
                            {
                                return ResolveIndexAccess(threadHashCode, frameIndex, variable, out res);
                            }
                        case VariableTypes.Value:
                            {
                                res = variable.Value;
                                VariableInfo info = VariableInfo.FromObject(res);
                                info.ValueObjType = variable.ValueType;
                                return info;
                            }
                        case VariableTypes.Integer:
                            {
                                res = variable.Offset;
                                return VariableInfo.GetInteger(variable.Offset);
                            }
                        case VariableTypes.String:
                            {
                                res = variable.Name;
                                return VariableInfo.GetString(variable.Name);
                            }
                        case VariableTypes.Boolean:
                            {
                                if (variable.Offset == 1)
                                {
                                    res = true;
                                    return VariableInfo.True;
                                }
                                else
                                {
                                    res = false;
                                    return VariableInfo.False;
                                }
                            }
                        case VariableTypes.Null:
                            {
                                res = null;
                                return VariableInfo.Null;
                            }
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    return VariableInfo.NullReferenceExeption;
                }
            }
            else
                return VariableInfo.NullReferenceExeption;
        }

        Type GetMemberType(Type type, string name, Type[] parameterTypes)
        {
            var bindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            if (parameterTypes == null)
            {
                var fi = type.GetField(name, bindingFlag);
                if (fi != null)
                    return fi.FieldType;
                var pi = type.GetProperty(name, bindingFlag);
                if (pi != null)
                    return pi.PropertyType;
            }
            else
            {
                var m = GetMethod(type, name, bindingFlag, false, parameterTypes);
                if (m != null)
                    return m.ReturnType;
            }
            return null;
        }

        VariableInfo ResolveMember(object obj, Type objType, string name, Type[] parameterTypes, object[] parameters, out object res)
        {
            res = null;
            Type type;
            if (obj == null)
                type = objType;
            else
            {
                if (obj is ILTypeInstance)
                {
                    type = ((ILTypeInstance)obj).Type.ReflectionType;
                    obj = ((ILTypeInstance)obj).CLRInstance; // 如果CLRInstance表示一个适配器，那么一定要用适配器实例作反射，否则得不到CLR成员的值
                }
                else if (obj is Enviorment.CrossBindingAdaptorType)
                    type = ((Enviorment.CrossBindingAdaptorType)obj).ILInstance.Type.ReflectionType;
                else
                    type = obj.GetType();
            }

            var bindingFlag = BindingFlags.Public | BindingFlags.NonPublic | (obj == null ? BindingFlags.Static : BindingFlags.Instance);
            if (parameterTypes == null)
            {
                var fi = type.GetField(name, bindingFlag);
                if (fi != null)
                {
                    res = fi.GetValue(obj);
                    VariableInfo info = VariableInfo.FromObject(res);

                    info.Address = 0;
                    info.Name = name;
                    info.Type = VariableTypes.FieldReference;
                    info.TypeName = fi.FieldType.FullName;
                    info.IsPrivate = fi.IsPrivate;
                    info.IsProtected = fi.IsFamily;
                    info.Expandable = res != null && !fi.FieldType.IsPrimitive;
                    info.ValueObjType = fi.FieldType;

                    return info;
                }
                else
                {
                    var fields = type.GetFields(bindingFlag);
                    string match = string.Format("<{0}>", name);
                    foreach (var f in fields)
                    {
                        if (f.Name.Contains(match))
                        {
                            res = f.GetValue(obj);
                            VariableInfo info = VariableInfo.FromObject(res);

                            info.Address = 0;
                            info.Name = name;
                            info.Type = VariableTypes.FieldReference;
                            info.TypeName = f.FieldType.FullName;
                            info.IsPrivate = f.IsPrivate;
                            info.IsProtected = f.IsFamily;
                            info.Expandable = res != null && !f.FieldType.IsPrimitive;
                            info.ValueObjType = f.FieldType;

                            return info;
                        }
                    }
                }

                var pi = type.GetProperty(name, bindingFlag);
                if (pi != null)
                {
                    res = pi.GetValue(obj, null);
                    VariableInfo info = VariableInfo.FromObject(res);

                    info.Address = 0;
                    info.Name = name;
                    info.Type = VariableTypes.PropertyReference;
                    info.TypeName = pi.PropertyType.FullName;
                    info.IsPrivate = pi.GetGetMethod(true).IsPrivate;
                    info.IsProtected = pi.GetGetMethod(true).IsFamily;
                    info.Expandable = res != null && !pi.PropertyType.IsPrimitive;
                    info.ValueObjType = pi.PropertyType;
                    return info;
                }
            }
            else
            {
                var method = GetMethod(type, name, bindingFlag, false, parameterTypes);
                if (method != null)
                {
                    res = method.Invoke(obj, parameters);
                    VariableInfo info = VariableInfo.FromObject(res);

                    info.Address = 0;
                    info.Name = name;
                    info.Type = VariableTypes.Invocation;
                    info.TypeName = method.ReturnType.FullName;
                    info.IsPrivate = method.IsPrivate;
                    info.IsProtected = method.IsFamily;
                    info.Expandable = false;
                    info.ValueObjType = method.ReturnType;
                    return info;
                }
            }

            return VariableInfo.GetCannotFind(name);
        }

        public static MethodInfo GetMethod(Type searchType, string methodName, BindingFlags bindingFlags, bool exactlyParametersCount, params Type[] parameterTypes)
        {
            if (searchType == null)
                return null;
            return GetMethod(searchType, methodName, bindingFlags, (methodInfo) =>
            {
                var parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length < parameterTypes.Length)
                    return false;
                return CheckParameters(parameterInfos, parameterTypes, exactlyParametersCount);
            });
        }

        public static MethodInfo GetMethod(Type searchType, string methodName, BindingFlags bindingFlags, Func<MethodInfo, bool> parameterPredicate)
        {
            var methods = searchType.GetMethods(bindingFlags);
            foreach (var method in methods)
            {
                if (method.Name != methodName)
                    continue;
                if (parameterPredicate(method))
                    return method;
            }
            return null;
        }

        private static bool CheckParameters(ParameterInfo[] parameters, Type[] checkTypes, bool exactlyParametersCount)
        {
            if (exactlyParametersCount && parameters.Length != checkTypes.Length)
                return false;

            for (int i = 0; i < parameters.Length; ++i)
            {
                if (i < checkTypes.Length)
                {
                    if (checkTypes[i] == null)
                    {
                        if (parameters[i].ParameterType.IsValueType) // null只能匹配引用类型
                            return false;
                    }
                    else if (!parameters[i].ParameterType.IsAssignableFrom(checkTypes[i]))
                        return false;
                }
                else if (!parameters[i].HasDefaultValue)
                    return false;
            }
            return true;
        }

        unsafe bool GetValueExpandable(ILIntepreter intp, StackObject* esp, IList<object> mStack)
        {
            if (esp->ObjectType < ObjectTypes.ValueTypeObjectReference)
                return false;
            else
            {
                object obj;
                if(esp->ObjectType == ObjectTypes.ValueTypeObjectReference)
                {
                    obj = StackObject.ToObject(esp, intp.AppDomain, mStack);   
                }
                else
                    obj = mStack[esp->Value];
                if (obj == null)
                    return false;
                if (obj is ILTypeInstance)
                    return true;
                else if (obj.GetType().IsPrimitive)
                    return false;
                else
                    return true;

            }
        }

        internal void ThreadStarted(ILIntepreter intp)
        {
            if (server != null && server.IsAttached)
            {
                server.SendSCThreadStarted(intp.GetHashCode());
            }
        }

        internal void ThreadEnded(ILIntepreter intp)
        {
            if (server != null && server.IsAttached)
            {
                server.SendSCThreadEnded(intp.GetHashCode());
            }
        }

        internal void Detach()
        {
            activeBreakpoints.Clear();
            breakpointMapping.Clear();
            pendingEnuming.Clear();
            pendingReferences.Clear();
            pendingIndexing.Clear();
            foreach (var j in AppDomain.Intepreters)
            {
                j.Value.ClearDebugState();
                j.Value.Resume();
            }
        }

        internal unsafe void DumpStack(StackObject* esp, RuntimeStack stack)
        {
            var start = stack.StackBase;
            var end = esp + 10;
            var frames = stack.Frames;
            var mStack = stack.ManagedStack;
            var valuePointerEnd = stack.ValueTypeStackPointer;
            StringBuilder final = new StringBuilder();
            HashSet<long> leakVObj = new HashSet<long>();
            for (var i = stack.ValueTypeStackBase; i > stack.ValueTypeStackPointer;)
            {
                leakVObj.Add((long)i);
                i = Minus(i, i->ValueLow + 1);
            }
            for (var i = start; i <= end; i++)
            {
                StringBuilder sb = new StringBuilder();
                ILMethod localMethod = null, baseMethod = null;
                bool isLocal = false;
                bool isBase = false;
                int localIdx = 0;
                if (i == esp)
                    sb.Append("->");
                foreach (var j in frames)
                {
                    if (i >= j.LocalVarPointer && i < j.BasePointer)
                    {
                        isLocal = true;
                        localIdx = (int)(i - j.LocalVarPointer);
                        localMethod = j.Method;
                    }
                    else if (i == j.BasePointer)
                    {
                        isBase = true;
                        baseMethod = j.Method;
                    }
                }
                sb.Append(string.Format("(0x{0:X8}) Type:{1} ", (long)i, i->ObjectType));
                try
                {
                    GetStackObjectText(sb, i, mStack, valuePointerEnd);
                }
                catch
                {
                    sb.Append(" Cannot Fetch Object Info");
                }
                if (i < esp)
                {
                    if (i->ObjectType == ObjectTypes.ValueTypeObjectReference)
                        VisitValueTypeReference(ILIntepreter.ResolveReference(i), leakVObj);
                }
                if (isLocal)
                {
                    sb.Append(string.Format("|Loc:{0}", localIdx));
                    if (localIdx == 0)
                    {
                        sb.Append(" Method:");
                        sb.Append(localMethod.ToString());
                    }
                }
                if (isBase)
                {
                    sb.Append("|Base");
                    sb.Append(" Method:");
                    sb.Append(baseMethod.ToString());
                }

                final.AppendLine(sb.ToString());
            }

            for (var i = stack.ValueTypeStackBase; i > stack.ValueTypeStackPointer;)
            {
                var vt = domain.GetTypeByIndex(i->Value);
                var cnt = i->ValueLow;
                bool leak = leakVObj.Contains((long)i);
                final.AppendLine("----------------------------------------------");
                final.AppendLine(string.Format("{2}(0x{0:X8}){1}", (long)i, vt, leak ? "*" : ""));
                for (int j = 0; j < cnt; j++)
                {
                    StringBuilder sb = new StringBuilder();
                    var ptr = Minus(i, j + 1);
                    sb.Append(string.Format("(0x{0:X8}) Type:{1} ", (long)ptr, ptr->ObjectType));
                    GetStackObjectText(sb, ptr, mStack, valuePointerEnd);
                    final.AppendLine(sb.ToString());
                }
                i = Minus(i, i->ValueLow + 1);
            }
            final.AppendLine("Managed Objects:");
            for (int i = 0; i < mStack.Count; i++)
            {
                final.AppendLine(string.Format("({0}){1}", i, mStack[i]));
            }
#if !UNITY_5 && !UNITY_2017_1_OR_NEWER && !UNITY_4
            System.Diagnostics.Debug.Print(final.ToString());
#else
            UnityEngine.Debug.LogWarning(final.ToString());
#endif
        }

        unsafe void GetStackObjectText(StringBuilder sb, StackObject* esp, IList<object> mStack, StackObject* valueTypeEnd)
        {
            string text = "null";
            switch (esp->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        sb.Append(string.Format("Value:0x{0:X8}", (long)ILIntepreter.ResolveReference(esp)));
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        object obj = null;
                        var dst = ILIntepreter.ResolveReference(esp);
                        try
                        {
                            if (dst > valueTypeEnd)
                                obj = StackObject.ToObject(esp, domain, mStack);
                            if (obj != null)
                                text = obj.ToString();
                        }
                        catch
                        {
                            text = "Invalid Object";
                        }
                        text += string.Format("({0})", domain.GetTypeByIndex(dst->Value));
                    }
                    sb.Append(string.Format("Value:0x{0:X8} Text:{1} ", (long)ILIntepreter.ResolveReference(esp), text));
                    break;
                default:
                    {
                        if (esp->ObjectType >= ObjectTypes.Null && esp->ObjectType <= ObjectTypes.ArrayReference)
                        {
                            if (esp->ObjectType < ObjectTypes.Object || esp->Value < mStack.Count)
                            {
                                var obj = StackObject.ToObject(esp, domain, mStack);
                                if (obj != null)
                                    text = obj.ToString();
                            }
                        }

                        sb.Append(string.Format("Value:{0} ValueLow:{1} Text:{2} ", esp->Value, esp->ValueLow, text));
                    }
                    break;

            }
        }

        unsafe void VisitValueTypeReference(StackObject* esp, HashSet<long> leak)
        {
            leak.Remove((long)esp);
            for (int i = 0; i < esp->ValueLow; i++)
            {
                var ptr = Minus(esp, i + 1);
                if (ptr->ObjectType == ObjectTypes.ValueTypeObjectReference)
                {
                    VisitValueTypeReference(ILIntepreter.ResolveReference(ptr), leak);
                }
            }
        }
    }
}