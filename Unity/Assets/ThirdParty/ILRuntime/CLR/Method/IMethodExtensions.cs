namespace ILRuntime.CLR.Method
{
    public static class IMethodExtensions
    {
        public static bool IsExtendMethod(this ILMethod im)
        {
            if (!im.IsStatic || im.ParameterCount == 0)
            {
                return false;
            }

            return im.ReflectionMethodInfo.GetCustomAttributes(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false).Length > 0;

        }
        public static bool IsExtendMethod(this CLRMethod cm)
        {
            if (!cm.IsStatic || cm.ParameterCount == 0)
            {
                return false;
            }

            return cm.MethodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);

        }
    }
}
