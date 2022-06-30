using System;

namespace Unity.IL2CPP.CompilerServices
{
    /// <summary>
    /// The code generation options available for IL to C++ conversion.
    /// Enable or disabled these with caution.
    /// </summary>
    public enum Option
    {
        /// <summary>
        /// Enable or disable code generation for null checks.
        ///
        /// Global null check support is enabled by default when il2cpp.exe
        /// is launched from the Unity editor.
        ///
        /// Disabling this will prevent NullReferenceException exceptions from
        /// being thrown in generated code. In *most* cases, code that dereferences
        /// a null pointer will crash then. Sometimes the point where the crash
        /// happens is later than the location where the null reference check would
        /// have been emitted though.
        /// </summary>
        NullChecks = 1,
        /// <summary>
        /// Enable or disable code generation for array bounds checks.
        ///
        /// Global array bounds check support is enabled by default when il2cpp.exe
        /// is launched from the Unity editor.
        ///
        /// Disabling this will prevent IndexOutOfRangeException exceptions from
        /// being thrown in generated code. This will allow reading and writing to
        /// memory outside of the bounds of an array without any runtime checks.
        /// Disable this check with extreme caution.
        /// </summary>
        ArrayBoundsChecks = 2,
        /// <summary>
        /// Enable or disable code generation for divide by zero checks.
        ///
        /// Global divide by zero check support is disabled by default when il2cpp.exe
        /// is launched from the Unity editor.
        ///
        /// Enabling this will cause DivideByZeroException exceptions to be
        /// thrown in generated code. Most code doesn't need to handle this
        /// exception, so it is probably safe to leave it disabled.
        /// </summary>
        DivideByZeroChecks = 3,
    }

    /// <summary>
    /// Use this attribute on an assembly, struct, class, method, or property to inform the IL2CPP code conversion utility to override the
    /// global setting for one of a few different runtime checks.
    ///
    /// Example:
    ///
    ///     [Il2CppSetOption(Option.NullChecks, false)]
    ///     public static string MethodWithNullChecksDisabled()
    ///     {
    ///         var tmp = new Object();
    ///         return tmp.ToString();
    ///     }
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate, Inherited = false, AllowMultiple = true)]
    public class Il2CppSetOptionAttribute : Attribute
    {
        public Option Option { get; private set; }
        public object Value { get; private set; }

        public Il2CppSetOptionAttribute(Option option, object value)
        {
            Option = option;
            Value = value;
        }
    }
}
