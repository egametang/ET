using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HybridCLR.Editor.ABI
{
    public class MethodDesc : IEquatable<MethodDesc>
    {
        public string Sig { get; private set; }

        public MethodDef MethodDef { get; set; }

        public ReturnInfo ReturnInfo { get; set; }

        public List<ParamInfo> ParamInfos { get; set; }

        public void Init()
        {
            for(int i = 0; i < ParamInfos.Count; i++)
            {
                ParamInfos[i].Index = i;
            }
            Sig = CreateCallSigName();
        }

        public void TransfromSigTypes(Func<TypeInfo, bool, TypeInfo> transformer)
        {
            ReturnInfo.Type = transformer(ReturnInfo.Type, true);
            foreach(var paramType in ParamInfos)
            {
                paramType.Type = transformer(paramType.Type, false);
            }
        }

        public string CreateCallSigName()
        {
            var n = new StringBuilder();
            n.Append(ReturnInfo.Type.CreateSigName());
            foreach(var param in ParamInfos)
            {
                n.Append(param.Type.CreateSigName());
            }
            return n.ToString();
        }

        public string CreateInvokeSigName()
        {
            var n = new StringBuilder();
            n.Append(ReturnInfo.Type.CreateSigName());
            foreach (var param in ParamInfos)
            {
                n.Append(param.Type.CreateSigName());
            }
            return n.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals((MethodDesc)obj);
        }

        public bool Equals(MethodDesc other)
        {
            return Sig == other.Sig;
        }

        public override int GetHashCode()
        {
            return Sig.GetHashCode();
        }
    }
}
