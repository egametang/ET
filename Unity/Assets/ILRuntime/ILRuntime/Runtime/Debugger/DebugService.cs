using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Debugger
{
    public class DebugService
    {
        BreakPointContext curBreakpoint;
        DebuggerServer server;
        Runtime.Enviorment.AppDomain domain;
        Dictionary<int, LinkedList<BreakpointInfo>> activeBreakpoints = new Dictionary<int, LinkedList<BreakpointInfo>>();
        Dictionary<int, BreakpointInfo> breakpointMapping = new Dictionary<int, BreakpointInfo>();
        AutoResetEvent evt = new AutoResetEvent(false);
        
        public Action<string> OnBreakPoint;

        public Enviorment.AppDomain AppDomain { get { return domain; } }

        public AutoResetEvent BlockEvent { get { return evt; } }

        public bool IsDebuggerAttached
        {
            get
            {
#if DEBUG
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
        /// <param name="port">Port to listen on</param>
        public void StartDebugService(int port)
        {
#if DEBUG
            server = new Debugger.DebuggerServer(this);
            server.Port = port;
            server.Start();
#endif
        }

        /// <summary>
        /// Stop Debugger Server
        /// </summary>
        public void StopDebugService()
        {
#if DEBUG
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

        public string GetStackTrance(ILIntepreter intepreper)
        {
            StringBuilder sb = new StringBuilder();
            ILRuntime.CLR.Method.ILMethod m;
            StackFrame[] frames = intepreper.Stack.Frames.ToArray();
            Mono.Cecil.Cil.Instruction ins = null;
            if (frames[0].Address != null)
            {
                ins = frames[0].Method.Definition.Body.Instructions[frames[0].Address.Value];
                sb.AppendLine(ins.ToString());
            }
            for (int i = 0; i < frames.Length; i++)
            {
                var f = frames[i];
                m = f.Method;
                string document = "";
                if (f.Address != null)
                {
                    ins = m.Definition.Body.Instructions[f.Address.Value];
                    var seq = FindSequencePoint(ins);
                    if (seq != null)
                    {
                        document = string.Format("{0}:Line {1}", seq.Document.Url, seq.StartLine);
                    }
                }
                sb.AppendFormat("at {0} {1}\r\n", m, document);
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
                arg = *(StackObject**)&arg->Value;
            }
            ILTypeInstance instance = arg->ObjectType != ObjectTypes.Null ? intepreter.Stack.ManagedStack[arg->Value] as ILTypeInstance : null;
            if (instance == null)
                return "null";
            var fields = instance.Type.TypeDefinition.Fields;
            int idx = 0;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fields.Count; i++)
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
            return sb.ToString();
        }

        public unsafe string GetLocalVariableInfo(ILIntepreter intepreter)
        {
            StackFrame topFrame = intepreter.Stack.Frames.Peek();
            var m = topFrame.Method;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m.LocalVariableCount; i++)
            {
                var lv = m.Definition.Body.Variables[i];
                var val = Add(topFrame.LocalVarPointer, i);
                var v = StackObject.ToObject(val, intepreter.AppDomain, intepreter.Stack.ManagedStack);
                if (v == null)
                    v = "null";
                string name = string.IsNullOrEmpty(lv.Name) ? "v" + lv.Index : lv.Name;
                sb.AppendFormat("{0} {1} = {2}", lv.VariableType.Name, name, v);
                if ((i % 3 == 0 && i != 0) || i == m.LocalVariableCount - 1)
                    sb.AppendLine();
                else
                    sb.Append(", ");
            }
            return sb.ToString();
        }

        internal static Mono.Cecil.Cil.SequencePoint FindSequencePoint(Mono.Cecil.Cil.Instruction ins)
        {
            Mono.Cecil.Cil.Instruction cur = ins;
            while (cur.SequencePoint == null && cur.Previous != null)
                cur = cur.Previous;

            return cur.SequencePoint;
        }

        unsafe StackObject* Add(StackObject* a, int b)
        {
            return (StackObject*)((long)a + sizeof(StackObject) * b);
        }

        unsafe StackObject* Minus(StackObject* a, int b)
        {
            return (StackObject*)((long)a - sizeof(StackObject) * b);
        }

        internal void NotifyModuleLoaded(string moduleName)
        {
            if (server != null && server.IsAttached)
                server.NotifyModuleLoaded(moduleName);
        }

        internal void SetBreakPoint(int methodHash, int bpHash, int startLine)
        {
            lock (activeBreakpoints)
            {
                LinkedList<BreakpointInfo> lst;
                if(!activeBreakpoints.TryGetValue(methodHash, out lst))
                {
                    lst = new LinkedList<Debugger.BreakpointInfo>();
                    activeBreakpoints[methodHash] = lst;
                }

                BreakpointInfo bpInfo = new BreakpointInfo();
                bpInfo.BreakpointHashCode = bpHash;
                bpInfo.MethodHashCode = methodHash;
                bpInfo.StartLine = startLine;

                lst.AddLast(bpInfo);
                breakpointMapping[bpHash] = bpInfo;
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
                    if(activeBreakpoints.TryGetValue(bpInfo.MethodHashCode, out lst))
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
                foreach(var i in AppDomain.Intepreters)
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
                if(AppDomain.Intepreters.TryGetValue(threadHash, out intp))
                {
                    intp.ClearDebugState();
                    intp.CurrentStepType = type;
                    intp.LastStepFrameBase = intp.Stack.Frames.Count > 0 ? intp.Stack.Frames.Peek().BasePointer : (StackObject*)0;
                    intp.LastStepInstructionIndex = intp.Stack.Frames.Count > 0 ? intp.Stack.Frames.Peek().Address.Value : 0;

                    intp.Resume();
                }
            }
        }

        unsafe internal void CheckShouldBreak(ILMethod method, ILIntepreter intp, int ip)
        {
            if (server != null && server.IsAttached)
            {
                int methodHash = method.GetHashCode();
                lock (activeBreakpoints)
                {
                    LinkedList<BreakpointInfo> lst;
                    bool bpHit = false;

                    if (activeBreakpoints.TryGetValue(methodHash, out lst))
                    {
                        var sp = method.Definition.Body.Instructions[ip].SequencePoint;
                        if (sp != null)
                        {
                            foreach (var i in lst)
                            {
                                if ((i.StartLine + 1) == sp.StartLine)
                                {
                                    DoBreak(intp, i.BreakpointHashCode, false);
                                    bpHit = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!bpHit)
                    {
                        var sp = method.Definition.Body.Instructions[ip].SequencePoint;
                        if (sp != null && IsSequenceValid(sp))
                        {                            
                            switch (intp.CurrentStepType)
                            {
                                case StepTypes.Into:
                                    DoBreak(intp, 0, true);
                                    break;
                                case StepTypes.Over:
                                    if (intp.Stack.Frames.Peek().BasePointer <= intp.LastStepFrameBase && ip != intp.LastStepInstructionIndex)
                                    {
                                        DoBreak(intp, 0, true);
                                    }
                                    break;
                                case StepTypes.Out:
                                    {
                                        if (intp.Stack.Frames.Count > 0 && intp.Stack.Frames.Peek().BasePointer < intp.LastStepFrameBase)
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
        }

        bool IsSequenceValid(Mono.Cecil.Cil.SequencePoint sp)
        {
            return sp.StartLine != sp.EndLine || sp.StartColumn != sp.EndColumn;
        }

        void DoBreak(ILIntepreter intp, int bpHash, bool isStep)
        {
            KeyValuePair<int, StackFrameInfo[]>[] frames = new KeyValuePair<int, StackFrameInfo[]>[AppDomain.Intepreters.Count];
            frames[0] = new KeyValuePair<int, StackFrameInfo[]>(intp.GetHashCode(), GetStackFrameInfo(intp));
            int idx = 1;
            foreach (var j in AppDomain.Intepreters)
            {
                if (j.Value != intp)
                {
                    j.Value.ShouldBreak = true;
                    frames[idx++] = new KeyValuePair<int, Debugger.StackFrameInfo[]>(j.Value.GetHashCode(), GetStackFrameInfo(j.Value));
                }
            }
            if (!isStep)
                server.SendSCBreakpointHit(intp.GetHashCode(), bpHash, frames);
            else
                server.SendSCStepComplete(intp.GetHashCode(), frames);
            //Breakpoint hit
            intp.Break();
        }

        unsafe StackFrameInfo[] GetStackFrameInfo(ILIntepreter intp)
        {
            StackFrame[] frames = intp.Stack.Frames.ToArray();
            Mono.Cecil.Cil.Instruction ins = null;
            ILMethod m;
            StackFrameInfo[] frameInfos = new StackFrameInfo[frames.Length];

            for (int j = 0; j < frames.Length; j++)
            {
                StackFrameInfo info = new Debugger.StackFrameInfo();
                var f = frames[j];
                m = f.Method;
                info.MethodName = m.ToString();

                if (f.Address != null)
                {
                    ins = m.Definition.Body.Instructions[f.Address.Value];
                    var seq = FindSequencePoint(ins);
                    if (seq != null)
                    {
                        info.DocumentName = seq.Document.Url;
                        info.StartLine = seq.StartLine - 1;
                        info.StartColumn = seq.StartColumn - 1;
                        info.EndLine = seq.EndLine - 1;
                        info.EndColumn = seq.EndColumn - 1;
                    }
                }
                StackFrame topFrame = f;
                m = topFrame.Method;
                int argumentCount = m.ParameterCount;
                if (m.HasThis)
                    argumentCount++;
                info.LocalVariables = new VariableInfo[argumentCount + m.LocalVariableCount];
                for(int i = 0; i < argumentCount; i++)
                {
                    int argIdx = m.HasThis ? i - 1 : i;
                    var arg = Minus(topFrame.LocalVarPointer, argumentCount);
                    string name = null;
                    object v = null;
                    string typeName = null;
                    var val = Add(arg, i);
                    v =  StackObject.ToObject(val, intp.AppDomain, intp.Stack.ManagedStack);
                    if (v == null)
                        v = "null";
                    if (argIdx >= 0)
                    {
                        var lv = m.Definition.Parameters[argIdx];
                        name = string.IsNullOrEmpty(lv.Name) ? "arg" + lv.Index : lv.Name;
                        typeName = lv.ParameterType.FullName;
                        if (v != null)
                            v = m.Parameters[argIdx].TypeForCLR.CheckCLRTypes(v);
                    }
                    else
                    {
                        name = "this";
                        typeName = m.DeclearingType.FullName;
                    }

                    VariableInfo vinfo = new Debugger.VariableInfo();
                    vinfo.Address = (long)val;
                    vinfo.Name = name;
                    vinfo.Value = v.ToString();
                    vinfo.TypeName = typeName;
                    vinfo.Expandable = GetValueExpandable(val, intp.Stack.ManagedStack);

                    info.LocalVariables[i] = vinfo;
                }
                for (int i = argumentCount; i < info.LocalVariables.Length; i++)
                {
                    var locIdx = i - argumentCount;
                    var lv = m.Definition.Body.Variables[locIdx];
                    var val = Add(topFrame.LocalVarPointer, locIdx);
                    var v = StackObject.ToObject(val, intp.AppDomain, intp.Stack.ManagedStack);
                    if (v == null)
                        v = "null";
                    else
                        v = intp.AppDomain.GetType(lv.VariableType, m.DeclearingType, m).TypeForCLR.CheckCLRTypes(v);
                    string name = string.IsNullOrEmpty(lv.Name) ? "v" + lv.Index : lv.Name;
                    VariableInfo vinfo = new Debugger.VariableInfo();
                    vinfo.Address = (long)val;
                    vinfo.Name = name;
                    vinfo.Value = v.ToString();
                    vinfo.TypeName = lv.VariableType.FullName;
                    vinfo.Expandable = GetValueExpandable(val, intp.Stack.ManagedStack);
                    info.LocalVariables[i] = vinfo;
                }
                frameInfos[j] = info;
            }
            return frameInfos;
        }

        internal VariableInfo ResolveVariable(VariableReference parent, string name)
        {
            return null;
        }

        unsafe bool GetValueExpandable(StackObject* esp, IList<object> mStack)
        {
            if (esp->ObjectType < ObjectTypes.Object)
                return false;
            else
            {
                var obj = mStack[esp->Value];
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
            foreach (var j in AppDomain.Intepreters)
            {
                j.Value.ClearDebugState();
                j.Value.Resume();
            }
        }
    }
}
