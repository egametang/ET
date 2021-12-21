using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.CLRBinding
{
    public class CLRBindingUtils
    {

        static private Action<ILRuntime.Runtime.Enviorment.AppDomain> initializeAction;
        static public void RegisterBindingAction(Action<ILRuntime.Runtime.Enviorment.AppDomain> action)
        {
            initializeAction = action;
        }

        /// <summary>
        /// This method can instead of CLRBindings.Initialize for avoid compile error when hasn't generator bindingCode.
        /// </summary>
        /// <param name="appDomain"></param>
        static public void Initialize(ILRuntime.Runtime.Enviorment.AppDomain appDomain)
        {
            if (initializeAction != null)
            {
                initializeAction(appDomain);
            }
        }

    }
}
