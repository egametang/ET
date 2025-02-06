using dnlib.DotNet;
using HybridCLR.Editor.ABI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Meta
{
    public class MethodReferenceAnalyzer
    {
        private readonly Action<MethodDef, List<TypeSig>, List<TypeSig>, GenericMethod> _onNewMethod;

        private readonly ConcurrentDictionary<MethodDef, List<IMethod>> _methodEffectInsts = new ConcurrentDictionary<MethodDef, List<IMethod>>();

        public MethodReferenceAnalyzer(Action<MethodDef, List<TypeSig>, List<TypeSig>, GenericMethod> onNewMethod)
        {
            _onNewMethod = onNewMethod;
        }

        public void WalkMethod(MethodDef method, List<TypeSig> klassGenericInst, List<TypeSig> methodGenericInst)
        {
            var ctx = new GenericArgumentContext(klassGenericInst, methodGenericInst);

            if (_methodEffectInsts.TryGetValue(method, out var effectInsts))
            {
                foreach (var met in effectInsts)
                {
                    var resolveMet = GenericMethod.ResolveMethod(met, ctx)?.ToGenericShare();
                    _onNewMethod(method, klassGenericInst, methodGenericInst, resolveMet);
                }
                return;
            }

            var body = method.Body;
            if (body == null || !body.HasInstructions)
            {
                return;
            }

            effectInsts = new List<IMethod>();
            foreach (var inst in body.Instructions)
            {
                if (inst.Operand == null)
                {
                    continue;
                }
                switch (inst.Operand)
                {
                    case IMethod met:
                    {
                        if (!met.IsMethod)
                        {
                            continue;
                        }
                        var resolveMet = GenericMethod.ResolveMethod(met, ctx)?.ToGenericShare();
                        if (resolveMet == null)
                        {
                            continue;
                        }
                        effectInsts.Add(met);
                        _onNewMethod(method, klassGenericInst, methodGenericInst, resolveMet);
                        break;
                    }
                    case ITokenOperand token:
                    {
                        //GenericParamContext paramContext = method.HasGenericParameters || method.DeclaringType.HasGenericParameters ?
                        //            new GenericParamContext(method.DeclaringType, method) : default;
                        //method.Module.ResolveToken(token.MDToken, paramContext);
                        break;
                    }
                }
            }
            _methodEffectInsts.TryAdd(method, effectInsts);
        }
    }
}
