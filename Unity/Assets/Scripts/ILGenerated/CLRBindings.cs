using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Int32_Binding.Register(app);
            System_Single_Binding.Register(app);
            System_Int64_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_String_Binding.Register(app);
            System_Console_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int32_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_ILTypeInstance_Int32_Binding.Register(app);
            Model_Component_Binding.Register(app);
            Model_Entity_Binding.Register(app);
        }
    }
}
