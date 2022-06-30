using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Huatuo.Generators
{
    public class MethodBridgeSig : IEquatable<MethodBridgeSig>
    {

        private readonly static Regex s_sigPattern = new Regex(@"^(v|i1|i2|i4|i8|r4|r8|sr|vf2|vf3|vf4|vd2|vd3|vd4|s2|S\d+)+$");

        public static MethodBridgeSig CreateBySignatuer(string sigName)
        {
            var re = s_sigPattern.Match(sigName);
            if (!re.Success)
            {
                throw new ArgumentException($"{sigName} is not valid signature");
            }
            
            var mbs = new MethodBridgeSig() { ParamInfos = new List<ParamInfo>()};
            var sigs = re.Groups[1].Captures;
            mbs.ReturnInfo = new ReturnInfo() { Type = CreateTypeInfoBySignature(sigs[0].Value)};
            for(int i = 1; i < sigs.Count; i++)
            {
                mbs.ParamInfos.Add(new ParamInfo() { Type = CreateTypeInfoBySignature(sigs[i].Value)});
            }
            return mbs;
        }


        private static TypeInfo CreateTypeInfoBySignature(string sigName)
        {
            switch(sigName)
            {
                case "v": return new TypeInfo(typeof(void), ParamOrReturnType.VOID);
                case "i1": return new TypeInfo(typeof(sbyte), ParamOrReturnType.I1_U1);
                case "i2": return new TypeInfo(typeof(short), ParamOrReturnType.I2_U2);
                case "i4": return new TypeInfo(typeof(int), ParamOrReturnType.I4_U4);
                case "i8": return new TypeInfo(typeof(long), ParamOrReturnType.I8_U8);
                case "r4": return new TypeInfo(typeof(float), ParamOrReturnType.R4);
                case "r8": return new TypeInfo(typeof(double), ParamOrReturnType.R8);
                case "sr": return new TypeInfo(null, ParamOrReturnType.STRUCTURE_AS_REF_PARAM);
                case "vf2": return new TypeInfo(null, ParamOrReturnType.ARM64_HFA_FLOAT_2);
                case "vf3": return new TypeInfo(null, ParamOrReturnType.ARM64_HFA_FLOAT_3);
                case "vf4": return new TypeInfo(null, ParamOrReturnType.ARM64_HFA_FLOAT_4);
                case "vd2": return new TypeInfo(null, ParamOrReturnType.ARM64_HFA_DOUBLE_2);
                case "vd3": return new TypeInfo(null, ParamOrReturnType.ARM64_HFA_DOUBLE_3);
                case "vd4": return new TypeInfo(null, ParamOrReturnType.ARM64_HFA_DOUBLE_4);
                case "s2": return new TypeInfo(null, ParamOrReturnType.STRUCTURE_SIZE_LE_16);
                default:
                    {
                        if (sigName.StartsWith("S"))
                        {
                            return new TypeInfo(null, ParamOrReturnType.STRUCTURE_SIZE_GT_16, int.Parse(sigName.Substring(1)));
                        }
                        else
                        {
                            throw new ArgumentException($"invalid signature:{sigName}");
                        }
                    }
            }
        }


        public ReturnInfo ReturnInfo { get; set; }

        public List<ParamInfo> ParamInfos { get; set; }

        public void Init()
        {
            for(int i = 0; i < ParamInfos.Count; i++)
            {
                ParamInfos[i].Index = i;
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
            return Equals((MethodBridgeSig)obj);
        }

        public bool Equals(MethodBridgeSig other)
        {
            if (other == null)
            {
                return false;
            }

            if (!ReturnInfo.Type.Equals(other.ReturnInfo.Type))
            {
                return false;
            }
            if (ParamInfos.Count != other.ParamInfos.Count)
            {
                return false;
            }
            for(int i = 0; i < ParamInfos.Count; i++)
            {
                if (!ParamInfos[i].Type.Equals(other.ParamInfos[i].Type))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23  + ReturnInfo.Type.GetHashCode();

            foreach(var p in ParamInfos)
            {
                hash = hash * 23 + p.Type.GetHashCode();
            }

            return hash;
        }
    }
}
