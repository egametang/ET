using System;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    public class CodeBody
    {
        //以后准备用自定义Body采集一遍，可以先过滤处理掉Mono.Cecil的代码中的指向，执行会更快
        public CodeBody(CLRSharp.ICLRSharp_Environment env, Mono.Cecil.MethodDefinition _def)
        {
            this.method = _def;
            Init(env);
        }
        public MethodParamList typelistForLoc = null;
        Mono.Cecil.MethodDefinition method;
        public Mono.Cecil.Cil.MethodBody bodyNative
        {
            get
            {
                return method.Body;
            }
        }
        bool bInited = false;
        public void Init(CLRSharp.ICLRSharp_Environment env)
        {
            if (bInited) return;
            if(bodyNative.HasVariables)
            {
                typelistForLoc = new MethodParamList(env, bodyNative.Variables);
 
            }
        }
        /// <summary>
        /// 预约的优化项目，暂不进行
        /// </summary>
        public void cacheBody()
        {

        }
    }
}
